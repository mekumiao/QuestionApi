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
public class ExamsController(ILogger<ExamsController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<ExamsController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount([FromQuery] ExamFilter filter) {
        var queryable = _dbContext.Exams.AsNoTracking();
        queryable = filter.Build(queryable);
        var result = await queryable.CountAsync();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ExamDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] ExamFilter filter, [FromQuery] Paging paging) {
        var queryable = _dbContext.Exams
            .AsNoTracking()
            .Include(v => v.ExamQuestions.OrderBy(t => t.Order))
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options.OrderBy(t => t.OptionCode))
            .AsQueryable();

        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var result = await queryable.ToListAsync();
        return Ok(_mapper.Map<ExamDto[]>(result));
    }

    [HttpGet("{examId:int}", Name = "GetExamById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExamById([FromRoute] int examId) {
        var queryable = _dbContext.Exams
                    .AsNoTracking()
                    .Include(v => v.ExamQuestions.OrderBy(t => t.Order))
                    .ThenInclude(v => v.Question)
                    .ThenInclude(v => v.Options.OrderBy(t => t.OptionCode));
        var result = await queryable.SingleOrDefaultAsync(v => v.ExamId == examId);
        return result is null ? NotFound() : Ok(_mapper.Map<ExamDto>(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody, FromForm] ExamInput dto) {
        var item = _mapper.Map<Exam>(dto);
        if (dto.ExamQuestions.Count != 0) {
            item.ExamQuestions.AddRange(_mapper.Map<List<ExamQuestion>>(dto.ExamQuestions).OrderBy(v => v.Order));
        }

        var questions = dto.ExamQuestions.Select(v => v.QuestionId).ToArray();
        await _dbContext.Questions
            .Include(v => v.Options.OrderBy(t => t.OptionCode))
            .Where(v => questions.Contains(v.QuestionId))
            .LoadAsync();

        _dbContext.Exams.Add(item);
        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<ExamDto>(item);
        return CreatedAtRoute("GetExamById", new { examId = item.ExamId }, result);
    }

    [HttpPut("{examId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] int examId, [FromBody, FromForm] ExamUpdate dto) {
        var item = await _dbContext.Exams
            .Include(v => v.ExamQuestions.OrderBy(t => t.Order))
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options.OrderBy(t => t.OptionCode))
            .SingleOrDefaultAsync(v => v.ExamId == examId);
        if (item is null) {
            return NotFound();
        }

        _mapper.Map(dto, item);

        if (dto.ExamQuestions is not null) {
            item.ExamQuestions.Clear();
            item.ExamQuestions.AddRange(_mapper.Map<List<ExamQuestion>>(dto.ExamQuestions));
        }

        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<ExamDto>(item);
        return Ok(result);
    }

    [HttpDelete("{examId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] int examId) {
        try {
            var item = new Exam { ExamId = examId };
            _dbContext.Exams.Attach(item);
            _dbContext.Exams.Remove(item);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0 ? NoContent() : NotFound();
        }
        catch (DbUpdateConcurrencyException) {
            return NotFound();
        }
    }
}