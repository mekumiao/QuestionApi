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
    public async Task<IActionResult> GetListAsync() {
        var result = await _dbContext.Questions.AsNoTracking().Select(v => new QuestionDto {
            QuestionId = v.QuestionId,
            Remark = v.Remark,
            Type = v.Type,
        }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("{questionId:int}", Name = "GetQuestionById")]
    public async Task<IActionResult> GetQuestionById([FromRoute] int questionId) {
        var item = await _dbContext.Questions.FindAsync(questionId);
        if (item is null) {
            var errorResponse = new {
                Message = "Resource not found",
                ErrorDetail = "The requested resource could not be found."
            };
            return NotFound(errorResponse);
        }
        return Ok(new QuestionDto {
            QuestionId = item.QuestionId,
            Remark = item.Remark,
            Type = item.Type,
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody, FromForm] QuestionCreateDto dto) {
        var item = new Question {
            Remark = dto.Remark,
            Type = dto.Type,
        };
        _dbContext.Questions.Add(item);
        await _dbContext.SaveChangesAsync();
        return CreatedAtRoute("GetQuestionById", new { questionId = item.QuestionId }, item);
    }

    [HttpPut("{questionId:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int questionId, [FromBody, FromForm] QuestionUpdateDto dto) {
        var item = await _dbContext.Questions.FindAsync(questionId);
        if (item is null) {
            var errorResponse = new {
                Message = "Resource not found",
                ErrorDetail = "The requested resource could not be found."
            };
            return NotFound(errorResponse);
        }
        item.Remark = dto.Remark;
        item.Type = dto.Type;
        await _dbContext.SaveChangesAsync();
        var result = await _dbContext.Questions.FindAsync(questionId);
        return Ok(result);
    }
}