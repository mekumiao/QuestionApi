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
    }
}