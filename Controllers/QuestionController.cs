using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

using Mapster;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Modules;

namespace QuestionApi.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class QuestionController(ILogger<QuestionController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<QuestionController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync([FromQuery, Range(minimum: 0, maximum: int.MaxValue)] int offset = 0, [FromQuery, Range(minimum: 10, maximum: 100)] int limit = 10) {
        var result = await _dbContext.Questions.AsNoTracking().ProjectToType<QuestionDto>().Skip(offset).Take(limit).ToListAsync();
        return Ok(result);
    }

    [HttpGet("{questionId:int}", Name = "GetQuestionById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestionById([FromRoute] int questionId) {
        var item = await _dbContext.Questions.FindAsync(questionId);
        return item is null
            ? NotFound()
            : Ok(_mapper.Map<QuestionDto>(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody, FromForm] QuestionCreateDto dto) {
        var item = _mapper.Map<Question>(dto);
        _dbContext.Questions.Add(item);
        await _dbContext.SaveChangesAsync();
        var result = _mapper.Map<QuestionDto>(item);
        return CreatedAtRoute("GetQuestionById", new { questionId = item.QuestionId }, result);
    }

    [HttpPut("{questionId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int questionId, [FromBody, FromForm] QuestionUpdateDto dto) {
        var item = await _dbContext.Questions.FindAsync(questionId);
        if (item is null) {
            return NotFound();
        }
        _mapper.From(dto).AdaptTo(item);
        await _dbContext.SaveChangesAsync();
        item = await _dbContext.Questions.FindAsync(questionId);
        if (item is null) {
            return NotFound();
        }
        var result = _mapper.Map<QuestionDto>(item);
        return Ok(result);
    }
}