using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Models;

public class Paging {
    [Range(minimum: 0, maximum: int.MaxValue)]
    public int Offset { get; set; }
    [Range(minimum: 0, maximum: 100)]
    public int Limit { get; set; } = 10;

    public IQueryable<T> Build<T>(IQueryable<T> queryable) where T : class {
        return queryable.Skip(Offset).Take(Limit);
    }
}