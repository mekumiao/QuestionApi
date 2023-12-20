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
            .ThenInclude(v => v.Questions)
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
            .SingleOrDefaultAsync(v => v.ExamPaperId == dto.ExamPaperId);
        if (examPaper is null) {
            return ValidationProblem($"试卷ID:{dto.ExamPaperId}不存在或已经被删除");
        }

        var history = new AnswerHistory {
            Student = user.Student,
            ExamPaper = examPaper,
            StartTime = DateTime.UtcNow,
        };
        if (dto.ExaminationId.HasValue) {
            var examination = await _dbContext.Examinations.FindAsync(dto.ExaminationId.Value);
            if (examination is null) {
                return NotFound($"考试ID:{dto.ExaminationId}不存在或已被删除");
            }
            history.Examination = examination;
            history.DurationSeconds = examination.DurationSeconds;
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
        var user = await _dbContext.Set<AppUser>()
            .Include(v => v.Student)
            .SingleOrDefaultAsync(v => v.Id == userId);
        if (user is null) {
            return ValidationProblem("当前登录用户信息不存在或已被删除");
        }

        var history = await _dbContext.AnswerHistories
            .Include(v => v.StudentAnswers)
            .SingleOrDefaultAsync(v => v.AnswerHistoryId == answerBoardId);
        if (history is null) {
            return NotFound();
        }
        if (history.IsSubmission) {
            return ValidationProblem("您已经交卷");
        }
        _dbContext.AnswerHistories.Add(history);

        user.Student ??= new Student {
            UserId = userId,
            Name = user.NickName ?? user.UserName ?? string.Empty,
        };

        var items = _mapper.Map<StudentAnswer[]>(inputs);
        history.StudentAnswers.AddRange(items);
        foreach (var item in items) {
            item.Student = user.Student;
        }

        var total_incorrect_answers = Correction(items);
        history.IsSubmission = true;//设置为已交卷
        history.TotalIncorrectAnswers = total_incorrect_answers;
        history.SubmissionTime = submissionTime;
        SetTimeTakenSeconds(history);
        SetTimeout(history);
        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<AnswerBoard>(history);
        return Ok(result);
    }

    /// <summary>
    /// 批改学生答案，并返回错题总数
    /// </summary>
    /// <param name="studentAnswers"></param>
    /// <returns></returns>
    private static int Correction(StudentAnswer[] studentAnswers) {
        var total_incorrect_answers = 0;
        foreach (var item in studentAnswers) {
            var left = item.AnswerText.Trim();
            var right = item.Question.CorrectAnswer.Trim();
            if (item.QuestionType == QuestionType.MultipleChoice) {
                item.IsCorrect = right.All(v => left.Contains(v));
                continue;
            }
            item.IsCorrect = left == right;
            if (item.IsCorrect is false) {
                total_incorrect_answers++;
            }
        }
        return total_incorrect_answers;
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