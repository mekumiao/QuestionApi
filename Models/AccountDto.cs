using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Models;

public class PasswordLogin {
    [MaxLength(256)]
    public required string Username { get; set; }
    [MaxLength(256)]
    public required string Password { get; set; }
}

public class InfoUpdate {
    [MaxLength(256)]
    public required string NickName { get; set; }
    [MaxLength(256)]
    public string? Avatar { get; set; }
    public int? AvatarFileId { get; set; }
}

public class ChangePasswordInput {
    [MaxLength(256)]
    public required string OldPassword { get; set; }
    [MaxLength(256)]
    public required string NewPassword { get; set; }
}