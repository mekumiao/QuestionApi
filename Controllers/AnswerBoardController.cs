using System.Diagnostics;
using System.Net.Mime;
using System.Security.Claims;

using EntityFramework.Exceptions.Common;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;
using QuestionApi.Services;

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
public class AnswerBoardController(ILogger<AnswerBoardController> logger,
                                   QuestionDbContext dbContext,
                                   IMapper mapper,
                                   AnswerBoardService answerBoardService) : ControllerBase {
    private readonly ILogger<AnswerBoardController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly AnswerBoardService _answerBoardService = answerBoardService;

    [HttpGet("{answerBoardId:int}", Name = "GetAnswerBoardById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerBoard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswerBoardById([FromRoute] int answerBoardId) {
        var userId = Convert.ToInt32(User.FindFirstValue("sub"));
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
        // 不是管理员的情况下才做限制
        if (User.IsInRole("admin") is false && history.Student.UserId != userId) {
            return ValidationProblem($"答题板ID:{answerBoardId}不属于当前用户");
        }
        if (history.IsSubmission is false && history.StartTime.HasValue && history.DurationSeconds > 0) {
            history.DurationSeconds -= (int)(DateTime.UtcNow - history.StartTime.Value).TotalSeconds;
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
    [ProducesResponseType(typeof(AnswerBoard), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAnswerBoard([FromBody, FromForm] AnswerBoardInput dto) {
        var userId = Convert.ToInt32(User.FindFirstValue("sub"));
        if (dto.ExaminationId is not null and > 0) {
            var (result, message) = await _answerBoardService.CreateAnswerBoardByExaminationId(userId, dto.ExaminationId.Value);
            return message is not null
                ? ValidationProblem(message)
                : CreatedAtRoute("GetAnswerBoardById", new { answerBoardId = result!.AnswerBoardId }, result);
        }
        else if (dto.ExamPaperId is not null and > 0) {
            var (result, message) = await _answerBoardService.CreateAnswerBoardByExamPaperId(userId, dto.ExamPaperId.Value);
            return message is not null
                ? ValidationProblem(message)
                : CreatedAtRoute("GetAnswerBoardById", new { answerBoardId = result!.AnswerBoardId }, result);
        }
        else {
            return NotFound();
        }
        // var user = await _dbContext.Set<AppUser>()
        //     .Include(v => v.Student)
        //     .SingleOrDefaultAsync(v => v.Id == userId);
        // if (user is null) {
        //     return ValidationProblem("当前登录用户信息不存在或已被删除");
        // }

        // var examPaper = await _dbContext.ExamPapers
        //     .Include(v => v.ExamPaperQuestions)
        //     .ThenInclude(v => v.Question)
        //     .ThenInclude(v => v.Options)
        //     .SingleOrDefaultAsync(v => v.ExamPaperId == dto.ExamPaperId);
        // if (examPaper is null) {
        //     return ValidationProblem($"试卷ID:{dto.ExamPaperId}不存在或已经被删除");
        // }
        // if (examPaper.Questions.Count == default) {
        //     return ValidationProblem($"试卷ID:{dto.ExamPaperId}没有设置题目");
        // }

        // if (dto.ExaminationId.HasValue && user.Student is not null) {
        //     // 仅能创建一条考试记录
        //     var existsHistory = await _dbContext.AnswerHistories
        //         .AsNoTracking()
        //         .Where(v => v.StudentId == user.Student.StudentId && v.ExaminationId == dto.ExaminationId)
        //         .FirstOrDefaultAsync();
        //     if (existsHistory is not null) {
        //         var resultDto = _mapper.Map<AnswerBoard>(existsHistory);
        //         return CreatedAtRoute("GetAnswerBoardById", new { answerBoardId = existsHistory.AnswerHistoryId }, resultDto);
        //     }
        // }

        // user.Student ??= new Student {
        //     UserId = userId,
        //     StudentName = user.NickName ?? user.UserName ?? string.Empty,
        // };

        // var history = new AnswerHistory {
        //     Student = user.Student,
        //     ExamPaper = examPaper,
        //     StartTime = DateTime.UtcNow,
        //     DifficultyLevel = examPaper.DifficultyLevel,
        // };
        // var answers = _mapper.Map<StudentAnswer[]>(examPaper.ExamPaperQuestions);
        // foreach (var item in answers) {
        //     item.Student = user.Student;
        //     // ExamPaperQuestion到StudentAnswer会将Question赋值过来，所以需要置空，让EF不创建Question
        //     item.Question = null!;
        // }
        // history.StudentAnswers.AddRange(answers);
        // history.TotalQuestions = history.StudentAnswers.Count;

        // if (dto.ExaminationId.HasValue) {
        //     var examination = await _dbContext.Examinations.FindAsync(dto.ExaminationId.Value);
        //     if (examination is null) {
        //         return NotFound($"考试ID:{dto.ExaminationId}不存在或已被删除");
        //     }
        //     history.Examination = examination;
        //     history.DurationSeconds = examination.DurationSeconds;
        //     history.DifficultyLevel = examination.DifficultyLevel;
        // }

        // _dbContext.AnswerHistories.Add(history);

        // // 学生重复参加考试仅计数一次（没有找到学生的任何考试记录时才将考试次数累加1）
        // async Task<bool> IsExistsExamination() {
        //     return dto.ExaminationId.HasValue && await _dbContext.AnswerHistories
        //            .Where(v => v.ExaminationId == dto.ExaminationId.Value)
        //            .Where(v => v.StudentId == user.Student.StudentId)
        //            .CountAsync() == 1; //仅有当前添加的一条
        // }

        // var studentQueryable = _dbContext.Students.Where(v => v.StudentId == user.Student.StudentId);
        // using var transaction = await _dbContext.Database.BeginTransactionAsync();
        // try {
        //     await _dbContext.SaveChangesAsync();
        //     // 判断是否是考试，增加学生考试或者练习的次数
        //     _ = history.ExaminationId == null ?
        //     await studentQueryable.ExecuteUpdateAsync(v => v.SetProperty(b => b.TotalPracticeSessions, b => b.TotalPracticeSessions + 1)) :
        //     await studentQueryable.ExecuteUpdateAsync(v => v.SetProperty(b => b.TotalExamParticipations, b => b.TotalExamParticipations + 1));

        //     // 增加参加考试人数
        //     if (await IsExistsExamination()) {
        //         await _dbContext.Examinations
        //             .Where(v => v.ExaminationId == dto.ExaminationId!.Value)
        //             .ExecuteUpdateAsync(v => v.SetProperty(b => b.ExamParticipantCount, b => b.ExamParticipantCount + 1));
        //     }
        // }
        // catch (ReferenceConstraintException) {
        //     Debug.Assert(false);
        //     return ValidationProblem($"考试ID:{dto.ExamPaperId}不存在");
        // }

        // await transaction.CommitAsync();

        // var result = _mapper.Map<AnswerBoard>(history);
        // return CreatedAtRoute("GetAnswerBoardById", new { answerBoardId = history.AnswerHistoryId }, result);
    }

    /// <summary>
    /// 随机创建答题板(随机练习)
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("random")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AnswerBoard), StatusCodes.Status201Created)]
    public async Task<IActionResult> RandomlyCreateAnswerBoard([FromBody, FromForm] RandomGenerationInput input,
                                                               [FromServices] ExamPaperService examPaperService) {
        if (string.IsNullOrWhiteSpace(input.ExamPaperName)) {
            input.ExamPaperName = $"随机练习-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        }

        var examPaper = new ExamPaper {
            ExamPaperType = ExamPaperType.RandomPractice,
            ExamPaperName = input.ExamPaperName,
            DifficultyLevel = input.DifficultyLevel ?? DifficultyLevel.None,
        };

        var (_, message) = await examPaperService.RandomGenerationAsync(examPaper);
        if (message is not null) {
            return ValidationProblem(message);
        }

        var boardInput = new AnswerBoardInput { ExamPaperId = examPaper.ExamPaperId };
        return await CreateAnswerBoard(boardInput);
    }

    /// <summary>
    /// 错题重做
    /// </summary>
    /// <param name="answerBoardId"></param>
    /// <returns></returns>
    [HttpPost("{answerBoardId:int}/redo-incorrect")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerBoard), StatusCodes.Status201Created)]
    public async Task<IActionResult> RedoIncorrectQuestions([FromRoute] int answerBoardId) {
        var userName = User.FindFirstValue("name")!;

        var examPaper = new ExamPaper {
            ExamPaperType = ExamPaperType.RedoIncorrect,
        };
        var history = await _dbContext.AnswerHistories
            .AsNoTracking()
            .Include(v => v.ExamPaper)
            .SingleOrDefaultAsync(v => v.AnswerHistoryId == answerBoardId);
        if (history is null) {
            return NotFound();
        }

        using (var scope = HttpContext.RequestServices.CreateAsyncScope()) {
            using var dbContext = scope.ServiceProvider.GetRequiredService<QuestionDbContext>();

            var ids = await dbContext.AnswerHistories
                .Include(v => v.StudentAnswers)
                .ThenInclude(v => v.Question)
                .Where(v => v.AnswerHistoryId == answerBoardId)
                .Where(v => v.IsSubmission == true)
                .SelectMany(v => v.StudentAnswers.Select(n => new {
                    n.QuestionId,
                    n.Question.DifficultyLevel,
                    n.Order,
                    n.IsCorrect,
                }))
                .Where(v => v.IsCorrect != true)
                .ToArrayAsync();

            if (ids.Length == 0) {
                return ValidationProblem($"答题历史:{answerBoardId}没有错题");
            }

            ids = ids.DistinctBy(v => v.QuestionId).ToArray();
            examPaper.DifficultyLevel = (DifficultyLevel)ids.Average(v => (int)v.DifficultyLevel);
            examPaper.ExamPaperName = history.ExamPaper.ExamPaperName;

            var questions = ids.Select(v => new ExamPaperQuestion {
                QuestionId = v.QuestionId,
                Order = v.Order
            });
            examPaper.ExamPaperQuestions.AddRange(questions);
            examPaper.TotalQuestions = examPaper.ExamPaperQuestions.Count;
            try {
                await dbContext.ExamPapers.AddAsync(examPaper);
                await dbContext.SaveChangesAsync();
            }
            catch (ReferenceConstraintException ex) {
                Debug.Assert(false);
                _logger.LogError(ex, "保存错题重做生成的试卷时失败");
                return ValidationProblem("错题重做生成失败，请尝试重新生成");
            }
        }

        var boardInput = new AnswerBoardInput { ExamPaperId = examPaper.ExamPaperId };
        return await CreateAnswerBoard(boardInput);
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
        var userId = Convert.ToInt32(User.FindFirstValue("sub"));

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

        // 赋值学生提交的答案
        foreach (var input in inputs) {
            var record = history.StudentAnswers.Find(v => v.QuestionId == input.QuestionId);
            var answer = input.AnswerText?.Trim();
            if (record is not null && !string.IsNullOrWhiteSpace(answer)) {
                record.AnswerText = answer;
                history.TotalNumberAnswers++;
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

        var studentQueryable = _dbContext.Students.Where(v => v.StudentId == history.StudentId);
        try {
            await _dbContext.SaveChangesAsync();
            // 修改学生的：总题目数、作答数、错题数
            await studentQueryable.ExecuteUpdateAsync(v => v.SetProperty(b => b.TotalQuestions, b => b.TotalQuestions + history.TotalQuestions));
            await studentQueryable.ExecuteUpdateAsync(v => v.SetProperty(b => b.TotalNumberAnswers, b => b.TotalNumberAnswers + history.TotalNumberAnswers));
            await studentQueryable.ExecuteUpdateAsync(v => v.SetProperty(b => b.TotalIncorrectAnswers, b => b.TotalIncorrectAnswers + history.TotalIncorrectAnswers));
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
    /// 批改学生答案，并返回错题总数(不包含未作答的)
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
            if (string.IsNullOrWhiteSpace(right)) {
                // 标准答案为空时，作答判为对
                item.IsCorrect = true;
                continue;
            }
            item.IsCorrect = item.Question.QuestionType == QuestionType.MultipleChoice
                ? left.Length == right.Length && right.Join(left, v => v, n => n, (v, n) => v).Count() == right.Length
                : left == right;
        }
        return studentAnswers.Count(v => v.IsCorrect is false);
    }

    private static void SetTimeTakenSeconds(AnswerHistory history) {
        history.TimeTakenSeconds = !history.StartTime.HasValue || !history.SubmissionTime.HasValue
            ? -1
            : (int)(history.SubmissionTime.Value - history.StartTime.Value).TotalSeconds;
    }

    private static void SetTimeout(AnswerHistory history) {
        if (history.TimeTakenSeconds < 0) {
            // 异常的答题时间
            history.IsTimeout = true;
        }
        else if (history.DurationSeconds <= 0) {
            // 未限制答题时间
            history.IsTimeout = false;
        }
        else {
            // 加10秒的网络延迟补偿
            history.IsTimeout = history.TimeTakenSeconds > history.DurationSeconds + 10;
        }
    }
}