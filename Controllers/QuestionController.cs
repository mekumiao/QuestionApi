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
    public async Task<IActionResult> GetListAsync() {
        var result = await _dbContext.Questions.AsNoTracking().Select(v => new QuestionDto {
            QuestionId = v.QuestionId,
            Remark = v.Remark,
            Type = v.Type,
        }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("{questionId:int}", Name = "GetQuestionById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestionById([FromRoute] int questionId) {
        var item = await _dbContext.Questions.FindAsync(questionId);
        if (item is null) {
            return NotFound();
        }
        return Ok(new QuestionDto {
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