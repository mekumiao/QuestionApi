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

        config.NewConfig<Exam, ExamDto>();
        config.NewConfig<ExamInput, Exam>();

        config.NewConfig<ExamQuestion, ExamQuestionDto>();

        config.NewConfig<Student, StudentDto>();
        config.NewConfig<StudentInput, Student>();
    }
}