using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class StudentFilter {
    [MaxLength(50)]
    public string? Name { get; set; }

    public IQueryable<Student> Build(IQueryable<Student> queryable) {
        if (!string.IsNullOrWhiteSpace(Name)) {
            queryable = queryable.Where(v => v.Name.Contains(Name));
        }
        return queryable;
    }
}

public class StudentDto {
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UserName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public int? UserId { get; set; }
}

public class StudentUpdate {
    [MaxLength(256)]
    public string? Name { get; set; }
}