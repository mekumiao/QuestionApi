using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace QuestionApi.Database;

public class AppUser : IdentityUser {
    [MaxLength(256)]
    public string? NickName { get; set; }
}