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
    public string? UserId { get; set; }
}

public class StudentUpdate {
    [MaxLength(256)]
    public string? Name { get; set; }
}

public class AnswerInput {
    public int QuestionId { get; set; }
    [MaxLength(500)]
    public string AnswerText { get; set; } = string.Empty;
}

public class AnswerHistoryDto {
    public int AnswerHistoryId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int ExamPaperId { get; set; }
    public string ExamPaperName { get; set; } = string.Empty;
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }
    /// <summary>
    /// 交卷时间
    /// </summary>
    public DateTime SubmissionTime { get; set; }
}

public class StudentAnswerDto {
    public int StudentAnswerId { get; set; }
    public int StudentId { get; set; }
    public int QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }
    public int AnswerHistoryId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
}
