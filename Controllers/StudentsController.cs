using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Mime;
using System.Security.Claims;

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
[Authorize]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class StudentsController(ILogger<StudentsController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<StudentsController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpGet("count")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount([FromQuery] StudentFilter filter) {
        var queryable = _dbContext.Students.AsNoTracking();
        queryable = filter.Build(queryable);
        var result = await queryable.CountAsync();
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
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
    [Authorize(Roles = "admin")]
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
    [Authorize(Roles = "admin")]
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

    [HttpGet("me/answer-history/count")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswerHistoryCountByCurrentUserId([FromQuery] AnswerHistoryFilter filter) {
        var userId = Convert.ToInt32(User.FindFirstValue("sub"));

        var queryable = _dbContext.AnswerHistories
            .AsNoTracking()
            .Include(v => v.Student)
            .Where(v => v.Student.UserId == userId);

        queryable = filter.Build(queryable);

        var count = await queryable.CountAsync();
        return Ok(count);
    }

    /// <summary>
    /// 获取当前登录用户的答题记录
    /// </summary>
    /// <returns></returns>
    [HttpGet("me/answer-history")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerHistoryDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswerHistoryListByCurrentUserId([FromQuery] Paging paging, [FromQuery] AnswerHistoryFilter filter) {
        var userId = Convert.ToInt32(User.FindFirstValue("sub"));

        var queryable = _dbContext.AnswerHistories
            .AsNoTracking()
            .Include(v => v.Student)
            .Include(v => v.ExamPaper)
            .Include(v => v.Examination)
            .OrderByDescending(v => v.AnswerHistoryId)
            .Where(v => v.Student.UserId == userId);

        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var items = await queryable.ToArrayAsync();
        var result = _mapper.Map<AnswerHistoryDto[]>(items);
        return Ok(result);
    }

    /// <summary>
    /// 获取当前学生的答题历史明细
    /// </summary>
    /// <param name="answerHistoryId"></param>
    /// <returns></returns>
    [HttpGet("me/answer-history/{answerHistoryId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerHistoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswerHistoryById([FromRoute] int answerHistoryId) {
        var userId = Convert.ToInt32(User.FindFirstValue("sub"));

        var history = await _dbContext.AnswerHistories
            .Include(v => v.Student)
            .Include(v => v.StudentAnswers)
            .Include(v => v.ExamPaper)
            .ThenInclude(v => v.Questions)
            .SingleOrDefaultAsync(v => v.AnswerHistoryId == answerHistoryId);
        if (history is null) {
            return NotFound();
        }
        var student = await _dbContext.Students
                .AsNoTracking()
                .SingleOrDefaultAsync(v => v.UserId == userId);
        if (student is null) {
            return Ok(Array.Empty<AnswerHistoryDto>());
        }
        else if (student.StudentId != history.StudentId) {
            return NotFound($"答题历史ID:{answerHistoryId}不属于当前用户");
        }
        var result = _mapper.Map<AnswerHistoryDto>(history);
        return Ok(result);
    }

    [HttpDelete("me/answer-history/{answerHistoryId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAnswerHistoryItem([FromRoute] int answerHistoryId) {
        var userId = Convert.ToInt32(User.FindFirstValue("sub"));
        var user = await _dbContext.Set<AppUser>().Include(v => v.Student).SingleOrDefaultAsync(v => v.Id == userId);
        if (user is null or { Student: null }) {
            return NotFound("未找到当前用户信息");
        }
        var studentId = user.Student.StudentId;
        var history = await _dbContext.AnswerHistories.Include(v => v.Examination).SingleOrDefaultAsync(v => v.AnswerHistoryId == answerHistoryId);
        if (history is null) {
            return NotFound();
        }
        if (history.StudentId != studentId) {
            return ValidationProblem($"历史:{answerHistoryId}不属于当前用户");
        }
        if (history.Examination != null && history.Examination.ExaminationType == ExaminationType.Exam) {
            return ValidationProblem($"历史:{answerHistoryId}与考试:{history.ExaminationId}关联，不能删除");
        }
        _dbContext.AnswerHistories.Remove(history);
        try {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "方法{name}: 删除历史记录{id}时移除", nameof(DeleteAnswerHistoryItem), answerHistoryId);
            throw;
        }
        return NoContent();
    }

    [HttpDelete("me/answer-history")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAnswerHistoryItems([FromBody, FromForm, MaxLength(20), MinLength(1)] int[] answerHistoryIds) {
        var userId = Convert.ToInt32(User.FindFirstValue("sub"));
        var user = await _dbContext.Set<AppUser>().Include(v => v.Student).SingleOrDefaultAsync(v => v.Id == userId);
        if (user is null or { Student: null }) {
            return NotFound();
        }
        var studentId = user.Student.StudentId;
        var rows = await _dbContext.AnswerHistories
            .Where(v => v.StudentId == studentId)
            .Where(v => v.Examination == null || v.Examination.ExaminationType != ExaminationType.Exam)
            .Where(v => answerHistoryIds.Contains(v.AnswerHistoryId))
            .ExecuteDeleteAsync();
        return NoContent();
    }
}