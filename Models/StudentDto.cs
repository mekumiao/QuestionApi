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
    /// <summary>
    /// 总题目数
    /// </summary>
    public int TotalQuestions { get; set; }
    /// <summary>
    /// 作答总数
    /// </summary>
    public int TotalNumberAnswers { get; set; }
    /// <summary>
    /// 答题率
    /// </summary>
    public double AnswerRate { get; set; }
    /// <summary>
    /// 错题总数
    /// </summary>
    public int TotalIncorrectAnswers { get; set; }
    /// <summary>
    /// 错题率
    /// </summary>
    public double IncorrectRate { get; set; }
    /// <summary>
    /// 参加考试次数
    /// </summary>
    public int TotalExamParticipations { get; set; }
    /// <summary>
    /// 练习次数
    /// </summary>
    public int TotalPracticeSessions { get; set; }
}

public class StudentUpdate {
    [MaxLength(256), Required]
    public string? StudentName { get; set; }
}