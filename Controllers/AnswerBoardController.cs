using System.Net.Mime;

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
[Authorize(Roles = "admin")]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class AnswerBoardController(ILogger<AnswerBoardController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<AnswerBoardController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// 创建答题记录，并返回答题板
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<IActionResult> CreateAnswerBoard(int examPaperId, Examination? examination) {
        var userClaim = User.FindFirst(v => v.Type == "sub")!;
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

        var examPaper = new ExamPaper { ExamPaperId = examPaperId };
        _dbContext.ExamPapers.Attach(examPaper);

        var histry = new AnswerHistory {
            Student = user.Student,
            ExamPaper = examPaper,
            Examination = examination,
            StartTime = DateTime.UtcNow,
        };
        _dbContext.AnswerHistories.Add(histry);
        await _dbContext.SaveChangesAsync();
        var result = _mapper.Map<AnswerBoard>(histry);
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
    public async Task<IActionResult> CreateAnswerBoard([FromBody, FromForm] int examPaperId) {
        return await CreateAnswerBoard(examPaperId, null);
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
        var userId = User.FindFirst(v => v.Type == "sub")!.Value;
        var user = await _dbContext.Set<AppUser>()
            .Include(v => v.Student)
            .SingleOrDefaultAsync(v => v.Id == userId);
        if (user is null) {
            return ValidationProblem("当前登录用户信息不存在或已被删除");
        }

        var histry = await _dbContext.AnswerHistories
            .Include(v => v.StudentAnswers)
            .SingleOrDefaultAsync(v => v.AnswerHistoryId == answerBoardId);
        if (histry is null) {
            return NotFound();
        }
        if (histry.IsSubmission) {
            return ValidationProblem("您已经交卷");
        }
        _dbContext.AnswerHistories.Add(histry);

        user.Student ??= new Student {
            UserId = userId,
            Name = user.NickName ?? user.UserName ?? string.Empty,
        };

        var items = _mapper.Map<StudentAnswer[]>(inputs);
        histry.StudentAnswers.AddRange(items);
        foreach (var item in items) {
            item.Student = user.Student;
        }

        var total_incorrect_answers = Correction(items);
        histry.IsSubmission = true;//设置为已交卷
        histry.TotalIncorrectAnswers = total_incorrect_answers;
        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<AnswerBoard>(histry);
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
}