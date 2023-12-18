using System.Net.Mime;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Controllers;

/// <summary>
/// 试卷
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class ExamPapersController(ILogger<ExamPapersController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<ExamPapersController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount([FromQuery] ExamPaperFilter filter) {
        var queryable = _dbContext.ExamPapers.AsNoTracking();
        queryable = filter.Build(queryable);
        var result = await queryable.CountAsync();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ExamPaperDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] ExamPaperFilter filter, [FromQuery] Paging paging) {
        var queryable = _dbContext.ExamPapers
            .AsNoTracking()
            .Include(v => v.ExamPaperQuestions.OrderBy(t => t.Order))
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options.OrderBy(t => t.OptionCode))
            .AsQueryable();

        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var result = await queryable.ToListAsync();
        return Ok(_mapper.Map<ExamPaperDto[]>(result));
    }

    [HttpGet("{paperId:int}", Name = "GetExamPaperById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExamPaperDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExamPaperById([FromRoute] int paperId) {
        var queryable = _dbContext.ExamPapers
                    .AsNoTracking()
                    .Include(v => v.ExamPaperQuestions.OrderBy(t => t.Order))
                    .ThenInclude(v => v.Question)
                    .ThenInclude(v => v.Options.OrderBy(t => t.OptionCode));
        var result = await queryable.SingleOrDefaultAsync(v => v.ExamPaperId == paperId);
        return result is null ? NotFound() : Ok(_mapper.Map<ExamPaperDto>(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExamPaperDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody, FromForm] ExamPaperInput dto) {
        var item = _mapper.Map<ExamPaper>(dto);
        if (dto.ExamPaperQuestions.Count != 0) {
            item.ExamPaperQuestions.AddRange(_mapper.Map<List<ExamPaperQuestion>>(dto.ExamPaperQuestions).OrderBy(v => v.Order));
        }

        var questions = dto.ExamPaperQuestions.Select(v => v.QuestionId).ToArray();
        await _dbContext.Questions
            .Include(v => v.Options.OrderBy(t => t.OptionCode))
            .Where(v => questions.Contains(v.QuestionId))
            .LoadAsync();

        _dbContext.ExamPapers.Add(item);
        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<ExamPaperDto>(item);
        return CreatedAtRoute("GetExamPaperById", new { paperId = item.ExamPaperId }, result);
    }

    [HttpPut("{paperId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExamPaperDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] int paperId, [FromBody, FromForm] ExamPaperUpdate dto) {
        var item = await _dbContext.ExamPapers
            .Include(v => v.ExamPaperQuestions.OrderBy(t => t.Order))
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options.OrderBy(t => t.OptionCode))
            .SingleOrDefaultAsync(v => v.ExamPaperId == paperId);
        if (item is null) {
            return NotFound();
        }

        _mapper.Map(dto, item);

        if (dto.ExamPaperQuestions is not null) {
            item.ExamPaperQuestions.Clear();
            item.ExamPaperQuestions.AddRange(_mapper.Map<List<ExamPaperQuestion>>(dto.ExamPaperQuestions));
        }

        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<ExamPaperDto>(item);
        return Ok(result);
    }

    [HttpDelete("{paperId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] int paperId) {
        try {
            var item = new ExamPaper { ExamPaperId = paperId };
            _dbContext.ExamPapers.Attach(item);
            _dbContext.ExamPapers.Remove(item);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0 ? NoContent() : NotFound();
        }
        catch (DbUpdateConcurrencyException) {
            return NotFound();
        }
    }
}