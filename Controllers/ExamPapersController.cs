using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Security.Claims;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;
using QuestionApi.Services;

namespace QuestionApi.Controllers;

/// <summary>
/// 试卷
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[ApiController]
[Authorize(Roles = "admin")]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class ExamPapersController(ILogger<ExamPapersController> logger,
                              QuestionDbContext dbContext,
                              ExamPaperService examPaperService,
                              IMapper mapper) : ControllerBase {
    private readonly ILogger<ExamPapersController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly ExamPaperService _examPaperService = examPaperService;
    private readonly IMapper _mapper = mapper;
    private readonly static FileExtensionContentTypeProvider TypeProvider = new();

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount([FromQuery] ExamPaperFilter filter) {
        var queryable = _dbContext.ExamPapers
            .AsNoTracking()
            .Where(v => v.ExamPaperType > ExamPaperType.None && v.ExamPaperType < ExamPaperType.RedoIncorrect);
        queryable = filter.Build(queryable);
        var result = await queryable.CountAsync();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ExamPaperDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] ExamPaperFilter filter, [FromQuery] Paging paging) {
        var queryable = _dbContext.ExamPapers
            .AsNoTracking()
            .Include(v => v.ExamPaperQuestions.OrderByDescending(t => t.Order))
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options.OrderBy(t => t.OptionCode))
            .Where(v => v.ExamPaperType > ExamPaperType.None && v.ExamPaperType < ExamPaperType.RedoIncorrect)
            .OrderByDescending(v => v.ExamPaperId)
            .AsQueryable();

        queryable = paging.Build(queryable);
        queryable = filter.Build(queryable);

        var result = await queryable.ToListAsync();
        return Ok(_mapper.Map<ExamPaperDto[]>(result));
    }

    [HttpGet("{paperId:int}", Name = "GetExamPaperById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExamPaperDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExamPaperById([FromRoute] int paperId) {
        var queryable = _dbContext.ExamPapers
                    .AsNoTracking()
                    .Include(v => v.ExamPaperQuestions.OrderByDescending(t => t.Order).ThenBy(v => v.QuestionId))
                    .ThenInclude(v => v.Question)
                    .ThenInclude(v => v.Options.OrderBy(t => t.OptionCode));
        var result = await queryable.SingleOrDefaultAsync(v => v.ExamPaperId == paperId);
        return result is null ? NotFound() : Ok(_mapper.Map<ExamPaperDto>(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExamPaperDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody, FromForm] ExamPaperInput dto) {
        var item = _mapper.Map<ExamPaper>(dto);
        if (dto.Questions.Count != 0) {
            item.ExamPaperQuestions.AddRange(_mapper.Map<ExamPaperQuestion[]>(dto.Questions));
            item.TotalQuestions = item.ExamPaperQuestions.Count;
        }

        var questionIds = dto.Questions.Select(v => v.QuestionId).ToArray();
        await _dbContext.Questions
            .Include(v => v.Options.OrderBy(t => t.OptionCode))
            .Where(v => questionIds.Contains(v.QuestionId))
            .LoadAsync();

        item.ExamPaperType = ExamPaperType.Create;

        _dbContext.ExamPapers.Add(item);
        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<ExamPaperDto>(item);
        return CreatedAtRoute("GetExamPaperById", new { paperId = item.ExamPaperId }, result);
    }

    [HttpPut("{paperId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExamPaperDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] int paperId, [FromBody, FromForm] ExamPaperUpdate dto) {
        var item = await _dbContext.ExamPapers
            .Include(v => v.ExamPaperQuestions)
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options.OrderBy(t => t.OptionCode))
            .SingleOrDefaultAsync(v => v.ExamPaperId == paperId);
        if (item is null) {
            return NotFound();
        }

        _mapper.Map(dto, item);

        if (dto.Questions is not null) {
            item.ExamPaperQuestions.Clear();
            item.ExamPaperQuestions.AddRange(_mapper.Map<List<ExamPaperQuestion>>(dto.Questions));
            item.TotalQuestions = item.ExamPaperQuestions.Count;
        }

        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<ExamPaperDto>(item);
        return Ok(result);
    }

    [HttpDelete("{paperId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] int paperId) {
        try {
            var item = new ExamPaper { ExamPaperId = paperId };
            _dbContext.ExamPapers.Attach(item);
            _dbContext.ExamPapers.Remove(item);
            var rows = await _dbContext.SaveChangesAsync();
            return rows > 0 ? NoContent() : NotFound();
        }
        catch (DbUpdateConcurrencyException) {
            return NotFound();
        }
    }

    /// <summary>
    /// 随机生成试卷
    /// </summary>
    /// <returns></returns>
    [HttpPost("random")]
    [ProducesResponseType(typeof(ExamPaperDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> RandomGeneration([FromBody, FromForm] RandomGenerationInput input) {
        var userName = User.FindFirstValue("name") ?? string.Empty;

        var examPaper = new ExamPaper {
            ExamPaperType = ExamPaperType.Random,
            ExamPaperName = input.ExamPaperName ?? $"随机生成-{userName}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            DifficultyLevel = input.DifficultyLevel ?? DifficultyLevel.None,
        };

        var (_, message) = await _examPaperService.RandomGenerationAsync(examPaper);
        if (message is not null) {
            return ValidationProblem(message);
        }
        var result = _mapper.Map<ExamPaperDto>(examPaper);
        return CreatedAtRoute("GetExamPaperById", new { paperId = examPaper.ExamPaperId }, result);
    }

    /// <summary>
    /// 导入试卷
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ExamPaperDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportFromExcel([FromForm] ImportExamPaperFromExcelInput input) {
        if (input.File == null || input.File.Length == 0) {
            return ValidationProblem("未选择文件或文件为空");
        }
        using var memoryStream = new MemoryStream();
        await input.File.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        var (examPapers, errors) = await _examPaperService.ImportFromExcelAsync(input.ExamPaperName ?? string.Empty, memoryStream);
        if (errors.Count > 0) {
            return ValidationProblem(new ValidationProblemDetails(errors));
        }
        var result = _mapper.Map<ExamPaperDto[]>(examPapers);
        return Ok(result);
    }

    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToExcel([FromBody, MaxLength(10), MinLength(1)] int[] input) {
        var memoryStream = new MemoryStream();
        var (firstName, error) = await _examPaperService.ExportToExcelAsync(memoryStream, input);
        if (error is not null) {
            return ValidationProblem(error);
        }
        var contentType = GetContentType("file.xlsx");
        var fileName = $"{firstName}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.xlsx";
        return File(memoryStream, contentType, fileName);
    }

    [HttpPost("export/template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ExportToExcelTemplate() {
        var memoryStream = new MemoryStream();
        var (firstName, _) = _examPaperService.ExportToExcelTemplate(memoryStream);
        var contentType = GetContentType("file.xlsx");
        var fileName = $"{firstName}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.xlsx";
        return File(memoryStream, contentType, fileName);
    }

    private static string GetContentType(string filePath) {
        string contentType = TypeProvider.TryGetContentType(filePath, out string? foundContentType)
                ? foundContentType
                : "application/octet-stream"; // 如果无法确定文件类型，默认使用二进制流
        return contentType;
    }
}