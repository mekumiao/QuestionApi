using System.Net.Mime;
using System.Security.Claims;

using Mapster;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Controllers;

/// <summary>
/// 账户
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize]
public class FilesController(ILogger<FilesController> logger, QuestionDbContext dbContext, IMapper mapper) : BaseController {
    private readonly ILogger<FilesController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [HttpPost]
    [ProducesResponseType(typeof(AppFileDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Upload([FromForm] AppFileInput fileInput) {
        if (fileInput.File is null or { Length: 0 }) {
            return ValidationProblem("没有上传任何文件，或文件大小为0");
        }
        else if (fileInput.File.Length > 2097152) {
            ModelState.AddModelError(nameof(AppFileInput.File), "文件太大，最大支持2M的文件");
            return ValidationProblem(ModelState);
        }
        var userId = Convert.ToInt32(User.FindFirstValue("sub"));
        var userName = User.FindFirstValue("name");
        using var memoryStream = new MemoryStream();
        await fileInput.File.CopyToAsync(memoryStream);

        var appFile = new AppFile() {
            Size = memoryStream.Length,
            UploadFileName = fileInput.FileName,
            UploadTime = DateTime.UtcNow,
            UploadUserId = userId,
            UploadUserName = userName,
            ExtensionName = Path.GetExtension(fileInput.FileName),
            Content = memoryStream.ToArray(),
        };

        _dbContext.AppFiles.Add(appFile);

        await _dbContext.SaveChangesAsync();

        var result = _mapper.Map<AppFileDto>(appFile);
        return CreatedAtRoute("GetFile", new { fileId = appFile.FileId }, result);
    }

    [HttpGet("{fileId:int}", Name = "GetFile")]
    [ProducesResponseType(typeof(AppFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppFileDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile([FromRoute] int fileId) {
        var file = await _dbContext.AppFiles
            .AsNoTracking()
            .ProjectToType<AppFileDto>()
            .SingleOrDefaultAsync(v => v.FileId == fileId);
        return file is null ? NotFound() : Ok(file);
    }

    [HttpGet("{fileId:int}/content")]
    [CacheControl]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFileContent([FromRoute] int fileId) {
        var file = await _dbContext.AppFiles.FindAsync(fileId);
        if (file is null) {
            return NotFound();
        }
        var contentType = ExamPapersController.GetContentType(file.UploadFileName);
        return File(file.Content, contentType);
    }
}