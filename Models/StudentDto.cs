using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class StudentFilter {
    /// <summary>
    /// 学生名称或者用户ID
    /// </summary>
    [MaxLength(50)]
    public string? StudentNameOrUserId { get; set; }

    public IQueryable<Student> Build(IQueryable<Student> queryable) {
        if (!string.IsNullOrWhiteSpace(StudentNameOrUserId)) {
            queryable = int.TryParse(StudentNameOrUserId, out var userId) && userId > 0
                ? queryable.Where(v => v.StudentName.Contains(StudentNameOrUserId) || v.UserId == userId)
                : queryable.Where(v => v.StudentName.Contains(StudentNameOrUserId));
        }
        return queryable;
    }
}

public class StudentDto {
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? UserName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public int? UserId { get; set; }
}

public class StudentUpdate {
    [MaxLength(256), Required]
    public string? StudentName { get; set; }
}