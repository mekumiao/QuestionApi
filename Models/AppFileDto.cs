using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Models;

public class AppFileDto {
    public int FileId { get; set; }
    public int? UploadUserId { get; set; }
    public string? UploadUserName { get; set; }
    public long Size { get; set; }
    public DateTime? UploadTime { get; set; }
    public string? UploadFileName { get; set; }
    /// <summary>
    /// 文件扩展名。例如：.png、.txt
    /// </summary>
    public string? ExtensionName { get; set; }
}

public class AppFileInput {
    [MaxLength(256)]
    public string? FileName { get; set; }
    public required IFormFile? File { get; set; }
}