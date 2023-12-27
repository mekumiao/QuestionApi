using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Database;

public class AppFile {
    [Key]
    public int FileId { get; set; }
    public int? UploadUserId { get; set; }
    [MaxLength(256)]
    public string? UploadUserName { get; set; }
    public AppUser? UploadUser { get; set; }
    public long Size { get; set; }
    public DateTime? UploadTime { get; set; }
    [MaxLength(256)]
    public string? UploadFileName { get; set; }
    /// <summary>
    /// 文件扩展名。例如：.png、.txt
    /// </summary>
    [MaxLength(256)]
    public string? ExtensionName { get; set; }
    public byte[] Content { get; set; } = [];
}
