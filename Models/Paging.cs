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

public class PagingResult<T> where T : class {
    public int Offset { get; }
    public int Limit { get; }
    public int Total { get; set; }
    public T[] Items { get; set; } = [];

    public PagingResult(Paging paging) {
        Offset = paging.Offset;
        Limit = paging.Limit;
    }

    public PagingResult(Paging paging, int total, T[] items) {
        Offset = paging.Offset;
        Limit = paging.Limit;
        Total = total;
        Items = items;
    }
}