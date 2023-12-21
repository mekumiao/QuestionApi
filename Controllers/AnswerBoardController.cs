using System.Diagnostics;
using System.Net.Mime;

using EntityFramework.Exceptions.Common;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Controllers;

/// <summary>
/// 答题板
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class AnswerBoardController(ILogger<AnswerBoardController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<AnswerBoardController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpGet("{answerBoardId:int}", Name = "GetAnswerBoardById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerBoard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswerBoardById([FromRoute] int answerBoardId) {
        var userId = User.FindFirst(v => v.Type == "sub")!.Value;
        var history = await _dbContext.AnswerHistories
            .Include(v => v.Student)
            .Include(v => v.ExamPaper)
            .Include(v => v.StudentAnswers)
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options.OrderBy(n => n.OptionCode))
            .SingleOrDefaultAsync(v => v.AnswerHistoryId == answerBoardId);
        if (history is null) {
            return NotFound();
        }
        if (history.Student.UserId != userId) {
            return ValidationProblem($"答题板ID:{answerBoardId}不属于当前用户");
        }
        if (history.IsSubmission is false && history.StartTime != default && history.DurationSeconds > 0) {
            history.DurationSeconds -= (int)(DateTime.UtcNow - history.StartTime).TotalSeconds;
            if (history.DurationSeconds < 0) {
                history.DurationSeconds = 0;
            }
        }
        var result = _mapper.Map<AnswerBoard>(history);
        return Ok(result);
    }

    /// <summary>
    /// 创建答题板
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerBoard), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAnswerBoard([FromBody, FromForm] AnswerBoardInput dto) {
        var userId = User.FindFirst(v => v.Type == "sub")!.Value;
        var user = await _dbContext.Set<AppUser>()
            .Include(v => v.Student)
            .SingleOrDefaultAsync(v => v.Id == userId);
        if (user is null) {
            return ValidationProblem("当前登录用户信息不存在或已被删除");
        }

        user.Student ??= new Student {
            UserId = userId,
            Name = user.NickName ?? user.UserName ?? string.Empty,
        };

        var examPaper = await _dbContext.ExamPapers
            .Include(v => v.Questions)
            .ThenInclude(v => v.Options.OrderBy(n => n.OptionCode))
            .SingleOrDefaultAsync(v => v.ExamPaperId == dto.ExamPaperId);
        if (examPaper is null) {
            return ValidationProblem($"试卷ID:{dto.ExamPaperId}不存在或已经被删除");
        }
        if (examPaper.Questions.Count == default) {
            return ValidationProblem($"试卷ID:{dto.ExamPaperId}没有设置题目");
        }

        var history = new AnswerHistory {
            Student = user.Student,
            ExamPaper = examPaper,
            StartTime = DateTime.UtcNow,
            DifficultyLevel = examPaper.DifficultyLevel,
        };
        var answers = _mapper.Map<StudentAnswer[]>(examPaper.Questions);
        foreach (var item in answers) {
            item.Student = user.Student;
        }
        history.StudentAnswers.AddRange(answers);

        if (dto.ExaminationId.HasValue) {
            var examination = await _dbContext.Examinations.FindAsync(dto.ExaminationId.Value);
            if (examination is null) {
                return NotFound($"考试ID:{dto.ExaminationId}不存在或已被删除");
            }
            history.Examination = examination;
            history.DurationSeconds = examination.DurationSeconds;
            history.DifficultyLevel = examination.DifficultyLevel;
        }

        _dbContext.AnswerHistories.Add(history);

        try {
            await _dbContext.SaveChangesAsync();
        }
        catch (ReferenceConstraintException) {
            Debug.Assert(false);
            return ValidationProblem($"考试ID:{dto.ExamPaperId}不存在");
        }
        var result = _mapper.Map<AnswerBoard>(history);
        return CreatedAtRoute("GetAnswerBoardById", new { answerBoardId = history.AnswerHistoryId }, result);
    }

    /// <summary>
    /// 交卷
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{answerBoardId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerBoard), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAnswerBoard([FromRoute] int answerBoardId,
                                                       [FromBody, FromForm] AnswerInput[] inputs) {
        var submissionTime = DateTime.UtcNow;
        var userId = User.FindFirst(v => v.Type == "sub")!.Value;

        var history = await _dbContext.AnswerHistories
            .Include(v => v.Student)
            .Include(v => v.ExamPaper)
            .Include(v => v.StudentAnswers)
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options)
            .SingleOrDefaultAsync(v => v.AnswerHistoryId == answerBoardId);
        if (history is null) {
            return NotFound();
        }
        if (history.Student.UserId != userId) {
            return ValidationProblem($"答题板ID:{answerBoardId}不属于当前用户");
        }
        if (history.IsSubmission) {
            return ValidationProblem("您已经交卷");
        }

        foreach (var input in inputs) {
            var record = history.StudentAnswers.Find(v => v.QuestionId == input.QuestionId);
            var answer = input.AnswerText?.Trim();
            if (record is not null && !string.IsNullOrWhiteSpace(answer)) {
                record.AnswerText = answer;
            }
        }

        history.IsSubmission = true;//设置为已交卷
        history.SubmissionTime = submissionTime;
        SetTimeTakenSeconds(history);
        SetTimeout(history);

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try {
            await _dbContext.SaveChangesAsync();
        }
        catch (ReferenceConstraintException ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "交卷保存信息到数据库时失败");
            return ValidationProblem("请检查数据");
        }

        var total_incorrect_answers = Correction(history.StudentAnswers);
        history.TotalIncorrectAnswers = total_incorrect_answers;

        try {
            await _dbContext.SaveChangesAsync();
        }
        catch (ReferenceConstraintException ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "交卷保存信息到数据库时失败");
            return ValidationProblem("请检查数据");
        }
        await transaction.CommitAsync();

        var result = _mapper.Map<AnswerBoard>(history);
        return Ok(result);
    }

    /// <summary>
    /// 批改学生答案，并返回错题总数
    /// </summary>
    /// <param name="studentAnswers"></param>
    /// <returns></returns>
    private static int Correction(ICollection<StudentAnswer> studentAnswers) {
        foreach (var item in studentAnswers) {
            var left = item.AnswerText;
            var right = item.Question.CorrectAnswer;
            if (string.IsNullOrWhiteSpace(left)) {
                // 跳过未作答的题目
                continue;
            }
            item.IsCorrect = item.Question.QuestionType == QuestionType.MultipleChoice
                ? left.Length == right.Length && right.Join(left, v => v, n => n, (v, n) => v).Count() == right.Length
                : left == right;
        }
        return studentAnswers.Count(v => v.IsCorrect is false or null);
    }

    private static void SetTimeTakenSeconds(AnswerHistory history) {
        history.TimeTakenSeconds = history.StartTime == default || history.SubmissionTime == default
            ? -1
            : (int)(history.SubmissionTime - history.StartTime).TotalSeconds;
    }

    private static void SetTimeout(AnswerHistory history) {
        // 异常的答题时间
        if (history.TimeTakenSeconds < 0) {
            history.IsTimeout = true;
        }
        // 未限制答题时间
        else if (history.DurationSeconds <= 0) {
            history.IsTimeout = false;
        }
        // 加10秒的网络延迟补偿
        else if (history.TimeTakenSeconds > history.DurationSeconds + 10) {
            history.IsTimeout = false;
        }
    }
}