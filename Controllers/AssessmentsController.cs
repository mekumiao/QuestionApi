using System.Net.Mime;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Controllers;

/// <summary>
/// 考试
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class ExaminationController(ILogger<ExaminationController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<ExaminationController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// 获取考试（时间、试卷）
    /// </summary>
    /// <param name="assessmentId"></param>
    /// <returns></returns>
    [HttpGet("{assessmentId:int}", Name = "GetAssessmentById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssessmentById([FromRoute] int assessmentId) {
        var queryable = _dbContext.ExamPapers
                    .AsNoTracking()
                    .Include(v => v.ExamPaperQuestions.OrderBy(t => t.Order))
                    .ThenInclude(v => v.Question)
                    .ThenInclude(v => v.Options.OrderBy(t => t.OptionCode));
        var result = await queryable.SingleOrDefaultAsync(v => v.ExamPaperId == assessmentId);
        return result is null ? NotFound() : Ok(_mapper.Map<ExamDto>(result));
    }

    /// <summary>
    /// 创建考试
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody, FromForm] ExamInput dto) {
        var item = _mapper.Map<ExamPaper>(dto);
        if (dto.ExamQuestions.Count != 0) {
            item.ExamPaperQuestions.AddRange(_mapper.Map<List<ExamPaperQuestion>>(dto.ExamQuestions).OrderBy(v => v.Order));
        }

        var questions = dto.ExamQuestions.Select(v => v.QuestionId).ToArray();
        await _dbContext.Questions
            .Include(v => v.Options.OrderBy(t => t.OptionCode))
            .Where(v => questions.Contains(v.QuestionId))
            .LoadAsync();

        _dbContext.ExamPapers.Add(item);
        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<ExamDto>(item);
        return CreatedAtRoute("GetExamById", new { examId = item.ExamPaperId }, result);
    }

    /// <summary>
    /// 交卷
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{assessmentId:int}/submit")]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> SubmitExam([FromRoute] int assessmentId,
                                                [FromBody, FromForm] List<AnswerInput> inputs) {
        var userClaim = User.FindFirst(v => v.Type == "sub")!;
        var student = await _dbContext.Students.SingleOrDefaultAsync(v => v.UserId == userClaim.Value);

        if (student is null) {
            _dbContext.Students.Add(new Student {
                UserId = userClaim.Value,
                Name = User.Identity?.Name ?? string.Empty,
            });
        }

        var item = _mapper.Map<StudentAnswer>(inputs);

        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<ExamDto>(item);
        return Ok();
    }
}