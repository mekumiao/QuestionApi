using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Mime;
using System.Security.Claims;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Controllers;

/// <summary>
/// 统计
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize(Roles = "admin")]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class StatisticsController(ILogger<StatisticsController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<StatisticsController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpGet("summary")]
    [ProducesResponseType(typeof(SummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Summary() {
        var summary = new SummaryDto {
            UserCount = await _dbContext.Users.CountAsync(),
            QuestionCount = await _dbContext.Questions.CountAsync(),
            ExaminationCount = await _dbContext.Examinations.CountAsync(),
            ExaminationCountNumber = await _dbContext.AnswerHistories.Where(v => v.ExaminationId != null).CountAsync(),
            ExamPaperCount = await _dbContext.ExamPapers.Where(v => v.ExamPaperType >= ExamPaperType.Random && v.ExamPaperType < ExamPaperType.Create).CountAsync()
        };

        #region 计算答题率
        var totalQuestions = await _dbContext.AnswerHistories.Where(v => v.IsSubmission == true).SumAsync(v => v.TotalQuestions);
        summary.AnswerRate = await _dbContext.AnswerHistories.Where(v => v.IsSubmission == true).SumAsync(v => v.TotalNumberAnswers);
        summary.AnswerRate /= totalQuestions;
        #endregion

        #region 计算错题率
        var answerCount = await _dbContext.StudentAnswers.Where(v => v.IsCorrect != null).CountAsync();
        summary.MistakeRate = await _dbContext.StudentAnswers.Where(v => v.IsCorrect == false).CountAsync();
        summary.MistakeRate /= answerCount;
        #endregion

        return Ok(summary);
    }
}