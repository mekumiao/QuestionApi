using System.Net.Mime;

using EntityFramework.Exceptions.Common;

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
[Authorize(Roles = "admin")]
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
    /// <param name="examinationId"></param>
    /// <returns></returns>
    [HttpGet("{examinationId:int}", Name = "GetExaminationById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExaminationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExaminationById([FromRoute] int examinationId) {
        var queryable = _dbContext.Examinations.AsNoTracking().OrderBy(v => v.Order);
        var result = await queryable.SingleOrDefaultAsync(v => v.ExaminationId == examinationId);
        return result is null ? NotFound() : Ok(_mapper.Map<ExaminationDto>(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ExaminationDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] ExaminationFilter filter, [FromQuery] Paging paging) {
        var queryable = _dbContext.Examinations.AsNoTracking().OrderBy(v => v.Order).AsQueryable();
        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);
        var result = await queryable.ToArrayAsync();
        return Ok(_mapper.Map<ExaminationDto[]>(result));
    }

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount([FromQuery] ExaminationFilter filter) {
        var queryable = _dbContext.Examinations.AsNoTracking();
        queryable = filter.Build(queryable);
        var result = await queryable.CountAsync();
        return Ok(result);
    }

    /// <summary>
    /// 创建考试
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(ExaminationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody, FromForm] ExaminationInput dto) {
        var item = _mapper.Map<Examination>(dto);
        try {
            _dbContext.ExamPapers.Attach(new ExamPaper { ExamPaperId = item.ExamPaperId });
            _dbContext.Examinations.Add(item);
            await _dbContext.SaveChangesAsync();
        }
        catch (ReferenceConstraintException) {
            return BadRequest(new BadDetail {
                Message = $"试卷ID{dto.ExamPaperId}不存在或已被删除"
            });
        }
        var result = _mapper.Map<ExaminationDto>(item);
        return CreatedAtRoute("GetExaminationById", new { examinationId = item.ExaminationId }, result);
    }

    [HttpPut("{examinationId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExaminationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] int examinationId, [FromBody, FromForm] ExaminationUpdate dto) {
        var item = await _dbContext.Examinations.FindAsync(examinationId);
        if (item is null) {
            return NotFound();
        }
        _mapper.Map(dto, item);
        await _dbContext.SaveChangesAsync();
        var result = _mapper.Map<ExaminationDto>(item);
        return Ok(result);
    }

    [HttpDelete("{examinationId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] int examinationId) {
        try {
            var rows = await _dbContext.Examinations
                .Where(v => v.ExaminationId == examinationId)
                .ExecuteDeleteAsync();
            return rows > 0 ? NoContent() : NotFound();
        }
        catch (DbUpdateConcurrencyException) {
            return NotFound();
        }
    }

    // /// <summary>
    // /// 交卷
    // /// </summary>
    // /// <param name="dto"></param>
    // /// <returns></returns>
    // [HttpPut("{examinationId:int}/submit")]
    // [ProducesResponseType(typeof(ExaminationDto), StatusCodes.Status201Created)]
    // public async Task<IActionResult> SubmitExam([FromRoute] int examinationId,
    //                                             [FromBody, FromForm] List<AnswerInput> inputs) {
    //     var userClaim = User.FindFirst(v => v.Type == "sub")!;
    //     var student = await _dbContext.Students.SingleOrDefaultAsync(v => v.UserId == userClaim.Value);

    //     if (student is null) {
    //         _dbContext.Students.Add(new Student {
    //             UserId = userClaim.Value,
    //             Name = User.Identity?.Name ?? string.Empty,
    //         });
    //     }

    //     var item = _mapper.Map<StudentAnswer>(inputs);

    //     await _dbContext.SaveChangesAsync();

    //     var result = _mapper.Map<ExaminationDto>(item);
    //     return Ok();
    // }
}