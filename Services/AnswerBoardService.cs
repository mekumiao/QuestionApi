using System.Diagnostics;

using EntityFramework.Exceptions.Common;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Services;

public class AnswerBoardService(ILogger<ExamPaperService> logger, QuestionDbContext dbContext, IMapper mapper) {
    private readonly ILogger<ExamPaperService> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// 根据试卷创建答题板
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="examPaperId"></param>
    /// <returns></returns>
    public async Task<(AnswerBoard?, string?)> CreateAnswerBoardByExamPaperId(int userId, int examPaperId) {
        var user = await _dbContext.Set<AppUser>()
            .Include(v => v.Student)
            .SingleOrDefaultAsync(v => v.Id == userId);
        if (user is null) {
            return (null, "当前登录用户信息不存在或已被删除");
        }

        var examPaper = await _dbContext.ExamPapers
            .Include(v => v.ExamPaperQuestions)
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options)
            .SingleOrDefaultAsync(v => v.ExamPaperId == examPaperId);
        if (examPaper is null) {
            return (null, $"试卷ID:{examPaperId}不存在或已经被删除");
        }
        if (examPaper.Questions.Count == default) {
            return (null, $"试卷ID:{examPaperId}没有设置题目");
        }

        user.Student ??= new Student {
            UserId = userId,
            StudentName = user.NickName ?? user.UserName ?? string.Empty,
        };

        var history = new AnswerHistory {
            Student = user.Student,
            ExamPaper = examPaper,
            StartTime = DateTime.UtcNow,
            DifficultyLevel = examPaper.DifficultyLevel,
        };
        var answers = _mapper.Map<StudentAnswer[]>(examPaper.ExamPaperQuestions);
        foreach (var item in answers) {
            item.Student = user.Student;
            // ExamPaperQuestion到StudentAnswer会将Question赋值过来，所以需要置空，让EF不创建Question
            item.Question = null!;
        }
        history.StudentAnswers.AddRange(answers);
        history.TotalQuestions = history.StudentAnswers.Count;

        _dbContext.AnswerHistories.Add(history);

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try {
            await _dbContext.SaveChangesAsync();
            // 添加练习次数
            await _dbContext.Students
                .Where(v => v.StudentId == user.Student.StudentId)
                .ExecuteUpdateAsync(v => v.SetProperty(b => b.TotalPracticeSessions, b => b.TotalPracticeSessions + 1));
        }
        catch (ReferenceConstraintException) {
            Debug.Assert(false);
            throw;
        }

        await transaction.CommitAsync();

        var result = _mapper.Map<AnswerBoard>(history);
        return (result, null);
    }

    /// <summary>
    /// 根据考试ID创建答题板
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="examinationId"></param>
    /// <returns></returns>
    public async Task<(AnswerBoard?, string?)> CreateAnswerBoardByExaminationId(int userId, int examinationId) {
        var user = await _dbContext.Set<AppUser>()
            .Include(v => v.Student)
            .SingleOrDefaultAsync(v => v.Id == userId);
        if (user is null) {
            return (null, "当前登录用户信息不存在或已被删除");
        }

        if (user.Student is not null) {
            // 检查已存在的考试记录
            var existsHistory = await _dbContext.AnswerHistories
                .AsNoTracking()
                .Include(v => v.Examination)
                .FirstOrDefaultAsync(v => v.StudentId == user.Student.StudentId && v.ExaminationId == examinationId);
            if (existsHistory is not null) {
                if (existsHistory.Examination!.IsPublish is false) {
                    return (null, $"考试{examinationId}还未发布，不能答题");
                }
                var resultDto = _mapper.Map<AnswerBoard>(existsHistory);
                return (resultDto, null);
            }
        }
        else {
            user.Student = new Student {
                UserId = userId,
                StudentName = user.NickName ?? user.UserName ?? string.Empty,
            };
        }

        var examination = await _dbContext.Examinations
            .Include(v => v.ExamPaper)
            .ThenInclude(v => v.ExamPaperQuestions)
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options)
            .SingleOrDefaultAsync(v => v.ExaminationId == examinationId);
        if (examination is null) {
            return (null, $"考试ID:{examinationId}不存在或已经被删除");
        }
        if (examination.ExamPaper.ExamPaperQuestions.Count == default) {
            return (null, $"考试ID:{examinationId}关联的试卷ID:{examination.ExamPaperId}没有设置题目");
        }
        if (examination.IsPublish is false) {
            return (null, $"考试{examinationId}还未发布，不能答题");
        }

        user.Student ??= new Student {
            UserId = userId,
            StudentName = user.NickName ?? user.UserName ?? string.Empty,
        };

        var history = new AnswerHistory {
            Student = user.Student,
            ExamPaper = examination.ExamPaper,
            StartTime = DateTime.UtcNow,
            DifficultyLevel = examination.DifficultyLevel,
            Examination = examination,
            DurationSeconds = examination.DurationSeconds,
        };

        var answers = _mapper.Map<StudentAnswer[]>(examination.ExamPaper.ExamPaperQuestions);
        foreach (var item in answers) {
            item.Student = user.Student;
            // ExamPaperQuestion到StudentAnswer会将Question赋值过来，所以需要置空，让EF不创建Question
            item.Question = null!;
        }
        history.StudentAnswers.AddRange(answers);
        history.TotalQuestions = history.StudentAnswers.Count;

        _dbContext.AnswerHistories.Add(history);

        // 学生重复参加考试仅计数一次（没有找到学生的任何考试记录时才将考试次数累加1）
        async Task<bool> IsExistsExamination() {
            return await _dbContext.AnswerHistories
                   .Where(v => v.ExaminationId == examinationId)
                   .Where(v => v.StudentId == user.Student.StudentId)
                   .CountAsync() == 1; //仅有当前添加的一条
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try {
            await _dbContext.SaveChangesAsync();
            await _dbContext.Students
                .Where(v => v.StudentId == user.Student.StudentId)
                .ExecuteUpdateAsync(v => v.SetProperty(b => b.TotalExamParticipations, b => b.TotalExamParticipations + 1));

            // 增加参加考试人数
            if (await IsExistsExamination()) {
                await _dbContext.Examinations
                    .Where(v => v.ExaminationId == examinationId)
                    .ExecuteUpdateAsync(v => v.SetProperty(b => b.ExamParticipantCount, b => b.ExamParticipantCount + 1));
            }
        }
        catch (ReferenceConstraintException) {
            Debug.Assert(false);
            throw;
        }

        await transaction.CommitAsync();

        var result = _mapper.Map<AnswerBoard>(history);
        return (result, null);
    }
}