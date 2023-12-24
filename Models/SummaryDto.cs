namespace QuestionApi.Models;

public class SummaryDto {
    /// <summary>
    /// 错题率
    /// </summary>
    public double MistakeRate { get; set; }
    /// <summary>
    /// 答题率
    /// </summary>
    public double AnswerRate { get; set; }
    /// <summary>
    /// 用户总数
    /// </summary>
    public int UserCount { get; set; }
    /// <summary>
    /// 题目总数
    /// </summary>
    public int QuestionCount { get; set; }
    /// <summary>
    /// 考试总数
    /// </summary>
    public int ExaminationCount { get; set; }
    /// <summary>
    /// 考试次数
    /// </summary>
    public int ExaminationCountNumber { get; set; }
    /// <summary>
    /// 试卷总数
    /// </summary>
    public int ExamPaperCount { get; set; }
}