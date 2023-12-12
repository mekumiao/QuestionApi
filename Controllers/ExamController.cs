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
/// 试卷
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class ExamController(ILogger<ExamController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<ExamController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync([FromQuery, Range(minimum: 0, maximum: int.MaxValue)] int offset = 0, [FromQuery, Range(minimum: 10, maximum: 100)] int limit = 10) {
        var result = await _dbContext.Exams.AsNoTracking()
                                           .Skip(offset)
                                           .Take(limit)
                                           .ProjectToType<ExamDto>()
                                           .ToListAsync();
        return Ok(result);
    }

    [HttpGet("{examId:int}", Name = "GetExamById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExamById([FromRoute] int examId) {
        var item = await _dbContext.Exams.FindAsync(examId);
        return item is null
            ? NotFound()
            : Ok(_mapper.Map<ExamDto>(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody, FromForm] ExamCreateDto dto) {
        var item = _mapper.Map<Exam>(dto);
        _dbContext.Exams.Add(item);
        await _dbContext.SaveChangesAsync();
        var result = _mapper.Map<ExamDto>(item);
        return CreatedAtRoute("GetExamById", new { examId = item.ExamId }, result);
    }

    [HttpPut("{examId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int examId, [FromBody, FromForm] QuestionUpdateDto dto) {
        var item = await _dbContext.Exams.FindAsync(examId);
        if (item is null) {
            return NotFound();
        }
        _mapper.From(dto).AdaptTo(item);
        await _dbContext.SaveChangesAsync();
        item = await _dbContext.Exams.FindAsync(examId);
        if (item is null) {
            return NotFound();
        }
        var result = _mapper.Map<ExamDto>(item);
        return Ok(result);
    }
}