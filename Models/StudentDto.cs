using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class StudentFilter {
    [MaxLength(50)]
    public string? StudentName { get; set; }

    public IQueryable<Student> Build(IQueryable<Student> queryable) {
        if (!string.IsNullOrWhiteSpace(StudentName)) {
            queryable = queryable.Where(v => v.StudentName.Contains(StudentName));
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