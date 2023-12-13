using Microsoft.AspNetCore.Identity;

namespace QuestionApi.Models;

public class StudentDto {
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public IdentityUser? User { get; set; }
    public List<StudentAnswerDto> StudentAnswers { get; } = [];
    public List<AnswerHistoryDto> AnswerHistories { get; } = [];
}

public class StudentInput {
    public string Name { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

public class StudentAnswerDto {
    public int AnswerId { get; set; }
    public int StudentId { get; set; }
    public StudentDto Student { get; set; } = null!;
    public int QuestionId { get; set; }
    public QuestionDto Question { get; set; } = null!;
    public int AnswerHistoryId { get; set; }
    public AnswerHistoryDto AnswerHistory { get; set; } = null!;
    /// <summary>
    /// 答案选项（单选题和多选题。用英文逗号","隔开）
    /// </summary>
    public string ChosenOptions { get; set; } = string.Empty;
    /// <summary>
    /// 答案文本（填空题）
    /// </summary>
    public string AnswerText { get; set; } = string.Empty;
}

public class StudentAnswerInput {
    public int StudentId { get; set; }
    public int QuestionId { get; set; }
    public int AnswerHistoryId { get; set; }
    /// <summary>
    /// 答案选项（单选题和多选题。用英文逗号","隔开）
    /// </summary>
    public string ChosenOptions { get; set; } = string.Empty;
    /// <summary>
    /// 答案文本（填空题）
    /// </summary>
    public string AnswerText { get; set; } = string.Empty;
}

public class AnswerHistoryDto {
    public int AnswerHistoryId { get; set; }
    public int StudentId { get; set; }
    public StudentDto Student { get; set; } = null!;
    public int ExamId { get; set; }
    public ExamDto Exam { get; set; } = null!;
    /// <summary>
    /// 交卷时间
    /// </summary>
    public DateTime SubmissionTime { get; set; }
    public List<StudentAnswerDto> StudentAnswers { get; } = [];
}
