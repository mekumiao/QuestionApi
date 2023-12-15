using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace QuestionApi.Models;

public class UserFilter {
    [MaxLength(50)]
    public string? UserName { get; set; }
    public string? Email { get; set; }

    public IQueryable<IdentityUser> Build(IQueryable<IdentityUser> queryable) {
        if (!string.IsNullOrWhiteSpace(UserName)) {
            queryable = queryable.Where(v => v.UserName!.Contains(UserName));
        }
        if (!string.IsNullOrWhiteSpace(Email)) {
            queryable = queryable.Where(v => v.Email!.Contains(Email));
        }
        return queryable;
    }
}


public class UserDto {
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = [];
}

public class UserUpdate {
    [MaxLength(50)]
    public string? UserName { get; set; } = string.Empty;
    public List<string>? Roles { get; set; }
}