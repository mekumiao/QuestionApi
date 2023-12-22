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

        config.NewConfig<Student, StudentDto>().Map(dest => dest, src => src.User);
        config.NewConfig<StudentUpdate, Student>();

        config.NewConfig<AppUser, UserDto>().Map(dest => dest.UserId, src => src.Id);
        config.NewConfig<AnswerHistory, AnswerHistoryDto>()
            .Map(dest => dest, src => src.Examination, should => should.Examination != null)
            .Map(dest => dest.ExamPaperName, src => src.ExamPaper.ExamPaperName);
        // .Map(dest => dest.DifficultyLevel, src => src.ExamPaper.DifficultyLevel);

        config.NewConfig<AnswerHistory, AnswerBoard>()
            .Fork(f => f.ForType<StudentAnswer, AnswerBoardQuestion>()
            .Map(dest => dest.QuestionText, src => src.Question.QuestionText)
            .Map(dest => dest.Options, src => src.Question.Options.OrderBy(v => v.OptionCode))
            .Map(dest => dest.CorrectAnswer, src => src.Question.CorrectAnswer, should => should.AnswerHistory.IsSubmission))
            .Map(dest => dest.ExamPaperName, src => src.ExamPaper.ExamPaperName)
            .Map(dest => dest.Questions, src => src.StudentAnswers)
            .Map(dest => dest.AnswerBoardId, src => src.AnswerHistoryId);

        config.NewConfig<ExamPaperQuestion, StudentAnswer>()
            .Map(dest => dest, src => src.Question);

        config.NewConfig<ExamPaper, ExamPaperDto>()
            .Map(dest => dest.Questions, src => src.ExamPaperQuestions, should => should.ExamPaperQuestions.Count > 0);
    }
}