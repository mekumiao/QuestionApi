using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace QuestionApi.Database;

public class AppUser : IdentityUser<int> {
    [MaxLength(256)]
    public string? NickName { get; set; }
    public Student? Student { get; set; }
    public DateTime? CreateTime { get; set; }
    [MaxLength(256)]
    public string? Avatar { get; set; }
    public int? AvatarFileId { get; set; }
    public List<AppFile> Files { get; } = [];
}

public class AppRole : IdentityRole<int> {
}