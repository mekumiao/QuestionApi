using Mapster;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi;

public class MappingRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase)
                      .IgnoreNullValues(true);

        config.NewConfig<Question, QuestionDto>();
        config.NewConfig<QuestionInput, Question>().Map(dest => dest.Options, src => src.Options);

        config.NewConfig<Option, OptionDto>();
        config.NewConfig<OptionInput, Option>();

        config.NewConfig<ExamPaper, ExamPaperDto>();
        config.NewConfig<ExamPaperInput, ExamPaper>();

        config.NewConfig<ExamPaperQuestion, ExamPaperQuestionDto>().Map(dest => dest, src => src.Question);

        config.NewConfig<Student, StudentDto>()
            .Map(dest => dest, src => src.User)
            .Map(dest => dest.AnswerRate, src => (double)src.TotalNumberAnswers / src.TotalQuestions, should => should.TotalQuestions > 0)
            .Map(dest => dest.IncorrectRate, src => (double)src.TotalIncorrectAnswers / src.TotalNumberAnswers, should => should.TotalNumberAnswers > 0);
        config.NewConfig<StudentUpdate, Student>();

        config.NewConfig<AppUser, UserDto>()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.NickName, src => src.NickName ?? src.UserName);
        config.NewConfig<AnswerHistory, AnswerHistoryDto>()
            .Map(dest => dest, src => src.Examination, should => should.Examination != null)
            .Map(dest => dest.ExamPaperName, src => src.ExamPaper.ExamPaperName)
            .Map(dest => dest.StudentName, src => src.Student.StudentName);
        // .Map(dest => dest.DifficultyLevel, src => src.ExamPaper.DifficultyLevel);

        config.NewConfig<AnswerHistory, AnswerBoard>()
            .Fork(f => f.ForType<StudentAnswer, AnswerBoardQuestion>()
            .Map(dest => dest.DifficultyLevel, src => src.Question.DifficultyLevel)
            .Map(dest => dest.QuestionText, src => src.Question.QuestionText)
            .Map(dest => dest.Options, src => src.Question.Options.OrderBy(v => v.OptionCode))
            .Map(dest => dest.CorrectAnswer, src => src.Question.CorrectAnswer, should => should.AnswerHistory.IsSubmission))
            .Map(dest => dest.ExamPaperName, src => src.ExamPaper.ExamPaperName)
            .Map(dest => dest.Questions, src => src.StudentAnswers)
            .Map(dest => dest.AnswerBoardId, src => src.AnswerHistoryId);

        config.NewConfig<ExamPaperQuestion, StudentAnswer>()
            .Map(dest => dest, src => src.Question);

        config.NewConfig<ExamPaper, ExamPaperDto>()
            .Map(dest => dest.Questions, src => src.ExamPaperQuestions.OrderBy(v => v.Order), should => should.ExamPaperQuestions.Count > 0);

        config.NewConfig<Examination, ExaminationDto>()
            .Map(dest => dest.ExamPaperName, src => src.ExamPaper.ExamPaperName);

        config.NewConfig<AppUser, CertificateDto>()
            .Map(dest => dest.UserId, src => src.Id);

        config.NewConfig<AnswerHistory, CertificateDto>()
            .Map(dest => dest.ExaminationName, src => src.Examination!.ExaminationName, should => should.Examination != null)
            .Map(dest => dest.Score, src => (src.TotalNumberAnswers - src.TotalIncorrectAnswers) / (double)src.TotalQuestions * 5, should => should.TotalQuestions > 0 && should.IsSubmission)
            .AfterMapping((src, dest) => dest.IsSuccess = dest.Score >= 3 && src.IsSubmission && src.IsTimeout == false);

        config.NewConfig<ExamPaperUpdate, ExamPaperQuestion>()
            .Map(dest => dest.ExamPaperId, src => MapContext.Current!.Parameters[nameof(ExamPaperQuestion.ExamPaperId)]);
    }
}