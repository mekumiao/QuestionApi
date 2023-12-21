using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class UserFilter {
    [MaxLength(256)]
    public string? UserName { get; set; }
    [MaxLength(256)]
    public string? NickName { get; set; }
    public string? Email { get; set; }

    public IQueryable<AppUser> Build(IQueryable<AppUser> queryable) {
        if (!string.IsNullOrWhiteSpace(UserName)) {
            queryable = queryable.Where(v => v.UserName!.Contains(UserName));
        }
        if (!string.IsNullOrWhiteSpace(NickName)) {
            queryable = queryable.Where(v => v.NickName!.Contains(NickName));
        }
        if (!string.IsNullOrWhiteSpace(Email)) {
            queryable = queryable.Where(v => v.Email!.Contains(Email));
        }
        return queryable;
    }
}


public class UserDto {
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? NickName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public DateTime? CreateTime { get; set; }
    public IEnumerable<string> Roles { get; set; } = [];
}

public class UserUpdate {
    [MaxLength(256)]
    public string? NickName { get; set; }
    public List<string>? Roles { get; set; }
}

public class UserInput {
    [MaxLength(256)]
    public string? NickName { get; set; }
    [EmailAddress, MaxLength(256)]
    public required string Email { get; set; } = string.Empty;
    [StringLength(256, MinimumLength = 6)]
    public required string Password { get; set; } = string.Empty;
    [MaxLength(256)]
    public string? Avatar { get; set; }
    public List<string> Roles { get; set; } = [];
}