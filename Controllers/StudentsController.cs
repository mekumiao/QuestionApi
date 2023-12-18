using System.Net.Mime;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Controllers;

/// <summary>
/// 学生
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize(Roles = "admin")]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class StudentsController(ILogger<StudentsController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<StudentsController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount([FromQuery] StudentFilter filter) {
        var queryable = _dbContext.Students.AsNoTracking();
        queryable = filter.Build(queryable);
        var result = await queryable.CountAsync();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(StudentDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] StudentFilter filter, [FromQuery] Paging paging) {
        var queryable = _dbContext.Students
            .AsNoTracking()
            .Include(v => v.User)
            .AsQueryable();

        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var result = await queryable.ToArrayAsync();
        return Ok(_mapper.Map<StudentDto[]>(result));
    }

    [HttpGet("{studentId:int}", Name = "GetStudentById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentById([FromRoute] int studentId) {
        var result = await _dbContext.Students
            .AsNoTracking()
            .Include(v => v.User)
            .SingleOrDefaultAsync(v => v.StudentId == studentId);
        return result is null ? NotFound() : Ok(_mapper.Map<StudentDto>(result));
    }

    [HttpPut("{studentId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] int studentId, [FromBody, FromForm] StudentUpdate dto) {
        var item = await _dbContext.Students
            .Include(v => v.User)
            .SingleOrDefaultAsync(v => v.StudentId == studentId);
        if (item is null) {
            return NotFound();
        }
        _mapper.Map(dto, item);
        await _dbContext.SaveChangesAsync();
        var result = _mapper.Map<StudentDto>(item);
        return Ok(result);
    }

    [HttpGet("{studentId:int}/answer-history")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerHistoryDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswerHistoryList([FromRoute] int studentId) {
        var result = await _dbContext.AnswerHistories
            .AsNoTracking()
            .Include(v => v.Student)
            .Where(v => v.StudentId == studentId)
            .ToArrayAsync();
        return Ok(_mapper.Map<AnswerHistoryDto[]>(result));
    }

    /// <summary>
    /// 获取当前登录用户的答题记录
    /// </summary>
    /// <returns></returns>
    [HttpGet("me/answer-history", Name = "GetAnswerHistoryListByCurrentUserId")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerHistoryDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswerHistoryListByCurrentUserId() {
        var userId = User.FindFirst(v => v.Type == "sub")?.Value;
        if (string.IsNullOrWhiteSpace(userId)) {
            return NotFound();
        }
        var student = await _dbContext.Students.AsNoTracking().SingleOrDefaultAsync(v => v.UserId == userId);
        if (student is null) {
            return Ok(Array.Empty<AnswerHistoryDto>());
        }
        var result = await _dbContext.AnswerHistories
            .AsNoTracking()
            .Include(v => v.Student)
            .Where(v => v.StudentId == student.StudentId)
            .ToArrayAsync();
        return Ok(_mapper.Map<AnswerHistoryDto[]>(result));
    }

    /// <summary>
    /// 根据考试ID获取当前学生的答题记录
    /// </summary>
    /// <returns></returns>
    [HttpGet("me/submit-examination/{examinationId:int}", Name = "GetSubmitExam")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(StudentAnswerDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubmitExam([FromRoute] int examinationId) {
        var userClaim = User.FindFirst(v => v.Type == "sub")!;
        var student = await _dbContext.Students
            .AsNoTracking()
            .SingleOrDefaultAsync(v => v.UserId == userClaim.Value);
        if (student is null) {
            return NotFound();
        }
        var items = await _dbContext.StudentAnswers
            .AsNoTracking()
            .Where(v => v.StudentId == student.StudentId)
            .Where(v => v.AnswerHistory.ExaminationId == examinationId)
            .ToArrayAsync();
        var result = _mapper.Map<StudentAnswerDto[]>(items);
        return items.Length == 0 ? NotFound() : Ok(result);
    }

    /// <summary>
    /// 交卷
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("me/submit-examination/{examinationId:int}")]
    [ProducesResponseType(typeof(StudentAnswerDto[]), StatusCodes.Status201Created)]
    public async Task<IActionResult> SubmitExam([FromRoute] int examinationId,
                                                [FromBody, FromForm] AnswerInput[] inputs) {
        var userClaim = User.FindFirst(v => v.Type == "sub")!;
        var user = await _dbContext.Set<AppUser>()
            .Include(v => v.Student)
            .SingleOrDefaultAsync(v => v.Id == userClaim.Value);
        if (user is null) {
            return BadRequest(new BadDetail { Message = "当前登录用户信息不存在或已被删除" });
        }
        var examination = await _dbContext.Examinations
            .Include(v => v.ExamPaper)
            .ThenInclude(v => v.Questions)
            .SingleOrDefaultAsync(v => v.ExaminationId == examinationId);
        if (examination is null) {
            return BadRequest(new BadDetail { Message = $"考试ID{examinationId}不存在或已被删除" });
        }

        user.Student ??= new Student {
            UserId = userClaim.Value,
            Name = user.NickName ?? user.UserName ?? string.Empty,
        };

        var histry = new AnswerHistory {
            Student = user.Student,
            ExamPaper = examination.ExamPaper,
            Examination = examination,
            SubmissionTime = DateTime.UtcNow,
        };

        var items = _mapper.Map<StudentAnswer[]>(inputs);
        foreach (var item in items) {
            item.Student = user.Student;
            item.StudentId = user.Student.StudentId;
        }

        histry.StudentAnswers.AddRange(items);
        _dbContext.AnswerHistories.Add(histry);
        Correction(items);

        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<StudentAnswerDto[]>(items);
        return CreatedAtRoute("GetSubmitExam", new { examinationId }, result);
    }

    private static void Correction(StudentAnswer[] studentAnswers) {
        foreach (var item in studentAnswers) {
            var left = item.AnswerText.Trim();
            var right = item.Question.CorrectAnswer.Trim();
            if (item.QuestionType == QuestionType.MultipleChoice) {
                item.IsCorrect = right.All(v => left.Contains(v));
                continue;
            }
            item.IsCorrect = left == right;
        }
    }
}