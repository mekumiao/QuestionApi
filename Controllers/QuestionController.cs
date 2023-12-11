using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Modules;

namespace QuestionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class QuestionController(ILogger<QuestionController> logger, QuestionDbContext dbContext) : ControllerBase {
    private readonly ILogger<QuestionController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;

    [HttpGet]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync([FromQuery, Range(minimum: 0, maximum: int.MaxValue)] int offset = 0, [FromQuery, Range(minimum: 10, maximum: 100)] int limit = 10) {
        var result = await _dbContext.Questions.AsNoTracking().Select(v => new QuestionDto {
            QuestionId = v.QuestionId,
            Remark = v.Remark,
            Type = v.Type,
        }).Skip(offset).Take(limit).ToListAsync();
        return Ok(result);
    }

    [HttpGet("{questionId:int}", Name = "GetQuestionById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestionById([FromRoute] int questionId) {
        var item = await _dbContext.Questions.FindAsync(questionId);
        return item is null
            ? NotFound()
            : Ok(new QuestionDto {
                QuestionId = item.QuestionId,
                Remark = item.Remark,
                Type = item.Type,
            });
    }

    [HttpPost]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody, FromForm] QuestionCreateDto dto) {
        var item = new Question {
            Remark = dto.Remark,
            Type = dto.Type,
        };
        _dbContext.Questions.Add(item);
        await _dbContext.SaveChangesAsync();
        var result = new QuestionDto {
            QuestionId = item.QuestionId,
            Remark = item.Remark,
            Type = item.Type,
        };
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
        item.Remark = dto.Remark;
        item.Type = dto.Type;
        await _dbContext.SaveChangesAsync();
        item = await _dbContext.Questions.FindAsync(questionId);
        if (item is null) {
            return NotFound();
        }
        var result = new QuestionDto {
            QuestionId = questionId,
            Remark = item.Remark,
            Type = item.Type,
        };
        return Ok(result);
    }
}