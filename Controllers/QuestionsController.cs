using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

using Mapster;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Controllers;

/// <summary>
/// 题目
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize(Roles = "admin")]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class QuestionsController(ILogger<QuestionsController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<QuestionsController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount() {
        var result = await _dbContext.Questions.CountAsync();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<QuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery, Range(minimum: 0, maximum: int.MaxValue)] int offset = 0, [FromQuery, Range(minimum: 10, maximum: 100)] int limit = 10) {
        var result = await _dbContext.Questions.AsNoTracking()
            .Include(v => v.Options.OrderBy(t => t.OptionCode))
            .OrderBy(v => v.QuestionId)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
        return Ok(_mapper.Map<List<QuestionDto>>(result));
    }

    [HttpGet("{questionId:int}", Name = "GetQuestionById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestionById([FromRoute] int questionId) {
        var result = await _dbContext.Questions.AsNoTracking()
            .Include(v => v.Options.OrderBy(t => t.OptionCode))
            .FirstOrDefaultAsync(v => v.QuestionId == questionId);
        return result is null ? NotFound() : Ok(_mapper.Map<QuestionDto>(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody, FromForm] QuestionInput dto) {
        var item = _mapper.Map<Question>(dto);
        if (dto.Options is not null) {
            item.Options.AddRange(_mapper.Map<List<Option>>(dto.Options));
        }
        _dbContext.Questions.Add(item);
        await _dbContext.SaveChangesAsync();
        var result = _mapper.Map<QuestionDto>(item);
        return CreatedAtRoute("GetQuestionById", new { questionId = item.QuestionId }, result);
    }

    [HttpPut("{questionId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] int questionId, [FromBody, FromForm] QuestionUpdate dto) {
        var item = await _dbContext.Questions.Include(v => v.Options).FirstOrDefaultAsync(v => v.QuestionId == questionId);
        if (item is null) {
            return NotFound();
        }
        _mapper.Map(dto, item);
        if (dto.Options is not null) {
            item.Options.Clear();
            item.Options.AddRange(_mapper.Map<List<Option>>(dto.Options));
        }
        await _dbContext.SaveChangesAsync();
        var result = _mapper.Map<QuestionDto>(item);
        return Ok(result);
    }
}