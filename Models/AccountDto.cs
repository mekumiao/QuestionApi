using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Models;

public class PasswordLogin {
    [MaxLength(256)]
    public required string Username { get; set; }
    [MaxLength(256)]
    public required string Password { get; set; }
}