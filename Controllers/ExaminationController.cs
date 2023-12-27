using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Mime;
using System.Security.Claims;

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
    /// <param name="examinationId"></param>
    /// <returns></returns>
    [HttpGet("{examinationId:int}", Name = "GetExaminationById")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExaminationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExaminationById([FromRoute] int examinationId) {
        var queryable = _dbContext.Examinations.AsNoTracking();
        var result = await queryable.SingleOrDefaultAsync(v => v.ExaminationId == examinationId);
        return result is null ? NotFound() : Ok(_mapper.Map<ExaminationDto>(result));
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(PagingResult<ExaminationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] ExaminationFilter filter, [FromQuery] Paging paging) {
        var queryable = _dbContext.Examinations
            .AsNoTracking()
            .Include(v => v.ExamPaper)
            .OrderByDescending(v => v.Order)
            .ThenByDescending(v => v.ExaminationId)
            .AsQueryable();
        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var totalQueryable = _dbContext.Examinations.AsNoTracking();
        totalQueryable = filter.Build(queryable);
        var total = await totalQueryable.CountAsync();

        var result = await queryable.ToArrayAsync();
        var resultItems = _mapper.Map<ExaminationDto[]>(result);
        return Ok(new PagingResult<ExaminationDto>(paging, total, resultItems));
    }

    [HttpGet("count")]
    [Authorize(Roles = "admin")]
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
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ExaminationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody, FromForm] ExaminationInput dto) {
        var item = _mapper.Map<Examination>(dto);
        try {
            _dbContext.ExamPapers.Attach(new ExamPaper { ExamPaperId = item.ExamPaperId });
            _dbContext.Examinations.Add(item);
            await _dbContext.SaveChangesAsync();
        }
        catch (ReferenceConstraintException) {
            return ValidationProblem($"{nameof(dto.ExamPaperId)}: {dto.ExamPaperId} 不存在或已被删除");
        }
        var result = _mapper.Map<ExaminationDto>(item);
        return CreatedAtRoute("GetExaminationById", new { examinationId = item.ExaminationId }, result);
    }

    [HttpPut("{examinationId:int}")]
    [Authorize(Roles = "admin")]
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
    [Authorize(Roles = "admin")]
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
        catch (DbUpdateException ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "删除考试:{examinationId}时失败", examinationId);
            throw;
        }
    }

    /// <summary>
    /// 获取已发布的考试列表
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="paging"></param>
    /// <returns></returns>
    [HttpGet("publish")]
    [ProducesResponseType(typeof(PagingResult<ExaminationPublishDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublishList([FromQuery] ExaminationFilter filter, [FromQuery] Paging paging) {
        var userId = Convert.ToInt32(User.FindFirstValue("sub"));
        var student = await _dbContext.Students.AsNoTracking().SingleOrDefaultAsync(v => v.UserId == userId);

        var queryable = _dbContext.Examinations
            .AsNoTracking()
            .Include(v => v.ExamPaper)
            .Where(v => v.IsPublish)
            .OrderByDescending(v => v.Order)
            .ThenByDescending(v => v.ExaminationId)
            .AsQueryable();
        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var totalQueryable = _dbContext.Examinations.AsNoTracking();
        totalQueryable = filter.Build(queryable);
        var total = await totalQueryable.CountAsync();

        var result = await queryable.ToArrayAsync();
        var resultItems = _mapper.Map<ExaminationPublishDto[]>(result);

        if (student is not null) {
            // 查询当前用户参加过的考试记录
            var histories = await _dbContext.AnswerHistories
                .AsNoTracking()
                .Where(v => v.StudentId == student.StudentId && v.ExaminationId != null)
                .ToArrayAsync();

            foreach (var exam in resultItems) {
                exam.AnswerState = AnswerState.Unanswered;
                foreach (var history in histories) {
                    if (exam.ExaminationId == history.ExaminationId) {
                        _mapper.Map(history, exam);
                        if (history.IsSubmission) {
                            exam.AnswerState = AnswerState.Finished;
                        }
                        else if (history.StartTime.HasValue && history.DurationSeconds > 0) {
                            // 设置剩余的考试时间
                            exam.RemainingSeconds = history.DurationSeconds - (int)(DateTime.UtcNow - history.StartTime.Value).TotalSeconds;
                            if (exam.RemainingSeconds < 0) {
                                exam.RemainingSeconds = 0;
                                exam.AnswerState = AnswerState.Timeout;
                            }
                            else {
                                exam.AnswerState = AnswerState.Answering;
                            }
                        }
                        else {
                            exam.AnswerState = AnswerState.Answering;
                        }
                    }
                }
            }
        }

        return Ok(new PagingResult<ExaminationPublishDto>(paging, total, resultItems));
    }

    [HttpGet("{examinationId:int}/{userId:int}/certificate")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CertificateDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCertificate([FromRoute, Range(1, int.MaxValue)] int examinationId, [FromRoute, Range(1, int.MaxValue)] int userId) {
        var user = await _dbContext.Set<AppUser>()
            .Include(v => v.Student)
            .SingleOrDefaultAsync(v => v.Id == userId);
        if (user is null or { Student: null }) {
            return NotFound();
        }
        var history = await _dbContext.AnswerHistories
            .AsNoTracking()
            .Include(v => v.Examination)
            .Where(v => v.ExaminationId == examinationId && v.StudentId == user.Student.StudentId)
            .FirstOrDefaultAsync();
        if (history is null) {
            return NotFound();
        }

        var result = _mapper.Map<CertificateDto>(user);
        _mapper.Map(history, result);
        return Ok(result);
    }
}