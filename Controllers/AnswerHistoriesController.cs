using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
        var queryable = _dbContext.AnswerHistories
            .AsNoTracking()
            .Include(v => v.Examination)
            .Include(v => v.ExamPaper)
            .AsQueryable();
        queryable = filter.Build(queryable);
        var result = await queryable.CountAsync();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagingResult<AnswerHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] AnswerHistoryFilter filter, [FromQuery] Paging paging) {
        var queryable = _dbContext.AnswerHistories
            .AsNoTracking()
            .Include(v => v.Student)
            .Include(v => v.Examination)
            .Include(v => v.ExamPaper)
            .OrderByDescending(v => v.AnswerHistoryId)
            .AsQueryable();
        var totalQueryable = queryable;
        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var result = await queryable.ToArrayAsync();
        var total = await totalQueryable.CountAsync();
        var resultItems = _mapper.Map<AnswerHistoryDto[]>(result);
        return Ok(new PagingResult<AnswerHistoryDto>(paging, total, resultItems));
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

    [HttpDelete("{answerHistoryId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAnswerHistoryItem([FromRoute] int answerHistoryId) {
        var history = await _dbContext.AnswerHistories.Include(v => v.Examination).SingleOrDefaultAsync(v => v.AnswerHistoryId == answerHistoryId);
        if (history is null) {
            return NotFound();
        }
        _dbContext.AnswerHistories.Remove(history);

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try {
            await _dbContext.SaveChangesAsync();
            if (history.Examination is not null) {
                await _dbContext.Examinations
                    .Where(v => v.ExaminationId == history.ExaminationId)
                    .ExecuteUpdateAsync(v => v.SetProperty(b => b.ExamParticipantCount, b => b.ExamParticipantCount - 1));
            }
        }
        catch (DbUpdateException ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "方法{name}: 删除历史记录{id}时移除", nameof(DeleteAnswerHistoryItem), answerHistoryId);
            throw;
        }
        await transaction.CommitAsync();

        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAnswerHistoryItems([FromBody, FromForm, MaxLength(20), MinLength(1)] int[] answerHistoryIds) {
        var histories = await _dbContext.AnswerHistories
            .Where(v => answerHistoryIds.Contains(v.AnswerHistoryId))
            .ToArrayAsync();

        var examinations = histories
            .Where(v => v.ExaminationId != null)
            .GroupBy(v => v.ExaminationId)
            .Select(v => new { ExaminationId = v.Key!.Value, Count = v.Count() })
            .ToArray();

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        foreach (var item in examinations) {
            await _dbContext.Examinations
                .Where(v => v.ExaminationId == item.ExaminationId)
                .ExecuteUpdateAsync(v => v.SetProperty(b => b.ExamParticipantCount, b => b.ExamParticipantCount - item.Count));
        }
        var rows = await _dbContext.AnswerHistories
            .Where(v => answerHistoryIds.Contains(v.AnswerHistoryId))
            .ExecuteDeleteAsync();
        await transaction.CommitAsync();

        return NoContent();
    }
}