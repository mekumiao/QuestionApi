using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace QuestionApi.Database;

public class AppUser : IdentityUser {
    [MaxLength(256)]
    public string? NickName { get; set; }
    public Student? Student { get; set; }
    public DateTime? CreateTime { get; set; }
    [MaxLength(256)]
    public string? Avatar { get; set; }
}