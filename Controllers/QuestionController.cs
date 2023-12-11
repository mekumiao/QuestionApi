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
    public async Task<IActionResult> GetAsync() {
        var result = await _dbContext.Questions.AsNoTracking().Select(v => new QuestionDto {
            QuestionId = v.QuestionId,
            Remark = v.Remark,
            Type = v.Type,
        }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("{questionId:int}")]
    public async Task<IActionResult> GetAsync([FromRoute] int questionId) {
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

    // public async Task<IActionResult> 
}