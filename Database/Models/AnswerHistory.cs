using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Database;

/// <summary>
/// 答题历史表
/// </summary>
public class AnswerHistory {
    public int AnswerHistoryId { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int ExamPaperId { get; set; }
    public ExamPaper ExamPaper { get; set; } = null!;
    public int? ExaminationId { get; set; }
    public Examination? Examination { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }
    /// <summary>
    /// 交卷时间
    /// </summary>
    public DateTime? SubmissionTime { get; set; }
    /// <summary>
    /// 限制的时长秒
    /// </summary>
    public int DurationSeconds { get; set; }
    /// <summary>
    /// 答题所用的时间秒
    /// </summary>
    public int TimeTakenSeconds { get; set; }
    public bool IsSubmission { get; set; }
    public bool IsTimeout { get; set; }
    /// <summary>
    /// 错题总数
    /// </summary>
    public int TotalIncorrectAnswers { get; set; }
    /// <summary>
    /// 题目总数
    /// </summary>
    public int TotalQuestions { get; set; }
    /// <summary>
    /// 作答总数
    /// </summary>
    public int TotalNumberAnswers { get; set; }
    public List<StudentAnswer> StudentAnswers { get; } = [];
}

/// <summary>
/// 学生答题表
/// </summary>
public class StudentAnswer {
    [Key]
    public int StudentAnswerId { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public QuestionType QuestionType { get; set; }
    public int AnswerHistoryId { get; set; }
    public AnswerHistory AnswerHistory { get; set; } = null!;
    /// <summary>
    /// 答案:
    /// 1.单选题和多选题，多选题直接将答案拼接即可。如：ABC
    /// 2.判断题，0表示错，1表示对
    /// 3.填空题，直接填入文本
    /// 4.null表示未作答
    /// </summary>
    public string? AnswerText { get; set; }
    public bool? IsCorrect { get; set; }
    /// <summary>
    /// 题目排序
    /// </summary>
    public int Order { get; set; }
}