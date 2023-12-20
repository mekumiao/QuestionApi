using System.Net.Mime;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Controllers;

/// <summary>
/// 答题历史
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize(Roles = "admin")]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class AnswerHistoriesController(ILogger<AnswerHistoriesController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<AnswerHistoriesController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount([FromQuery] AnswerHistoryFilter filter) {
        var queryable = _dbContext.AnswerHistories.AsNoTracking();
        queryable = filter.Build(queryable);
        var result = await queryable.CountAsync();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(AnswerHistoryDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] AnswerHistoryFilter filter, [FromQuery] Paging paging) {
        var queryable = _dbContext.AnswerHistories
            .AsNoTracking()
            .Include(v => v.Student)
            .AsQueryable();

        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var result = await queryable.ToArrayAsync();
        return Ok(_mapper.Map<AnswerHistoryDto[]>(result));
    }

    [HttpGet("{answerHistoryId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AnswerHistoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswerHistoryById([FromRoute] int answerHistoryId) {
        var history = await _dbContext.AnswerHistories
            .Include(v => v.Student)
            .Include(v => v.ExamPaper)
            .ThenInclude(v => v.Questions)
            .SingleOrDefaultAsync(v => v.AnswerHistoryId == answerHistoryId);
        if (history is null) {
            return NotFound();
        }
        var result = _mapper.Map<AnswerHistoryDto>(history);
        return Ok(result);
    }
}