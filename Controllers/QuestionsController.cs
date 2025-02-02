using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;
using QuestionApi.Services;

namespace QuestionApi.Controllers;

/// <summary>
/// 题目
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize(Roles = "admin")]
public class QuestionsController(ILogger<QuestionsController> logger,
                                 QuestionDbContext dbContext,
                                 IMapper mapper,
                                 ExamPaperService examPaperService,
                                 QuestionService questionService) : BaseController {
    private readonly ILogger<QuestionsController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly ExamPaperService _examPaperService = examPaperService;
    private readonly QuestionService _questionService = questionService;

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount([FromQuery] QuestionFilter filter) {
        var queryable = _dbContext.Questions.AsNoTracking();
        queryable = filter.Build(queryable);
        var result = await queryable.CountAsync();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagingResult<QuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] QuestionFilter filter, [FromQuery] Paging paging) {
        var queryable = _dbContext.Questions
            .AsNoTracking()
            .Include(v => v.Options.OrderBy(t => t.OptionCode))
            .OrderByDescending(v => v.QuestionId)
            .AsQueryable();
        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var totalQueryable = _dbContext.Questions.AsNoTracking();
        totalQueryable = filter.Build(totalQueryable);
        var total = await totalQueryable.CountAsync();

        var result = await queryable.ToListAsync();
        var resultItems = _mapper.Map<QuestionDto[]>(result);
        return Ok(new PagingResult<QuestionDto>(paging, total, resultItems));
    }

    [HttpGet("{questionId:int}", Name = "GetQuestionById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestionById([FromRoute] int questionId) {
        var result = await _dbContext.Questions
            .AsNoTracking()
            .Include(v => v.Options.OrderBy(t => t.OptionCode))
            .SingleOrDefaultAsync(v => v.QuestionId == questionId);
        return result is null ? NotFound() : Ok(_mapper.Map<QuestionDto>(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody, FromForm] QuestionInput dto) {
        var item = _mapper.Map<Question>(dto);
        if (dto.Options is not null && dto.Options.Count != 0) {
            item.Options.AddRange(_mapper.Map<List<Option>>(dto.Options));
        }
        _dbContext.Questions.Add(item);
        await _dbContext.SaveChangesAsync();
        var result = _mapper.Map<QuestionDto>(item);
        return CreatedAtRoute("GetQuestionById", new { questionId = item.QuestionId }, result);
    }

    [HttpPut("{questionId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] int questionId, [FromBody, FromForm] QuestionUpdate dto) {
        var item = await _dbContext.Questions
            .Include(v => v.Options.OrderBy(t => t.OptionCode))
            .SingleOrDefaultAsync(v => v.QuestionId == questionId);
        if (item is null) {
            return NotFound();
        }
        _mapper.Map(dto, item);
        if (dto.Options is not null) {
            item.Options.Clear();
            item.Options.AddRange(_mapper.Map<List<Option>>(dto.Options));
        }
        await _dbContext.SaveChangesAsync();
        var result = _mapper.Map<QuestionDto>(item);
        return Ok(result);
    }

    [HttpDelete("{questionId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] int questionId) {
        try {
            var item = new Question { QuestionId = questionId };
            _dbContext.Questions.Attach(item);
            _dbContext.Questions.Remove(item);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0 ? NoContent() : NotFound();
        }
        catch (DbUpdateConcurrencyException) {
            return NotFound();
        }
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteItems([FromBody, FromForm, MaxLength(20), MinLength(1)] int[] questionIds) {
        await _dbContext.Questions.Where(v => questionIds.Contains(v.QuestionId)).ExecuteDeleteAsync();
        return NoContent();
    }

    /// <summary>
    /// 导入题目
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(QuestionDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportFromExcel([FromForm, Required] ImportQuestionFromExcelInput input) {
        if (input.File == null || input.File.Length == 0) {
            return ValidationProblem("未选择文件或文件为空");
        }
        using var memoryStream = new MemoryStream();
        await input.File.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        var (questions, errors) = await _questionService.ImportFromExcelAsync(memoryStream);
        if (errors.Count > 0) {
            return ValidationProblem(new ValidationProblemDetails(errors));
        }
        var result = _mapper.Map<QuestionDto[]>(questions);
        return Ok(result);
    }

    /// <summary>
    /// 导出题目
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToExcel([FromBody, MaxLength(10), MinLength(1)] int[] input) {
        var memoryStream = new MemoryStream();
        var error = await _questionService.ExportToExcelAsync(memoryStream, input);
        if (error is not null) {
            return ValidationProblem(error);
        }
        var contentType = ExamPapersController.GetContentType("file.xlsx");
        var fileName = $"导出题目-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.xlsx";
        return File(memoryStream, contentType, fileName);
    }

    /// <summary>
    /// 下载导入模板
    /// </summary>
    /// <returns></returns>
    [HttpPost("export/template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ExportToExcelTemplate() {
        var memoryStream = new MemoryStream();
        _examPaperService.ExportToExcelTemplate(memoryStream);
        var contentType = ExamPapersController.GetContentType("file.xlsx");
        var fileName = $"题目导入模板-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.xlsx";
        return File(memoryStream, contentType, fileName);
    }
}