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
    [ProducesResponseType(typeof(List<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] StudentFilter filter, [FromQuery] Paging paging) {
        var queryable = _dbContext.Students
            .AsNoTracking()
            .Include(v => v.User)
            .AsQueryable();

        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var result = await queryable.ToArrayAsync();
        return Ok(_mapper.Map<List<StudentDto>>(result));
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
    [ProducesResponseType(typeof(List<AnswerHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswerHistoryList([FromRoute] int studentId) {
        var result = await _dbContext.AnswerHistories
            .AsNoTracking()
            .Include(v => v.Student)
            .Where(v => v.StudentId == studentId)
            .ToArrayAsync();
        return Ok(_mapper.Map<List<AnswerHistoryDto>>(result));
    }

    /// <summary>
    /// 获取当前登录用户的答题记录
    /// </summary>
    /// <returns></returns>
    [HttpGet("me/answer-history")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerHistoryDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswerHistoryListByCurrentUserId() {
        var userId = User.FindFirst(v => v.Type == ClaimTypes.NameIdentifier)?.Value;
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
}