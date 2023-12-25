namespace QuestionApi.Models;

public class SummaryDto {
    /// <summary>
    /// 错题率
    /// </summary>
    public double IncorrectRate { get; set; }
    /// <summary>
    /// 答题率
    /// </summary>
    public double AnswerRate { get; set; }
    /// <summary>
    /// 用户总数
    /// </summary>
    public int TotalUsers { get; set; }
    /// <summary>
    /// 题目总数
    /// </summary>
    public int TotalQuestions { get; set; }
    /// <summary>
    /// 发布的考试总场数
    /// </summary>
    public int TotalExamSessions { get; set; }
    /// <summary>
    /// 所有考试的总参与人数
    /// </summary>
    public int TotalExamParticipations { get; set; }
    /// <summary>
    /// 试卷总数
    /// </summary>
    public int ExamPaperCount { get; set; }
}