using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace QuestionApi;

public class CacheControlAttribute : ActionFilterAttribute {
    public int MaxAge { get; set; }

    public CacheControlAttribute() {
        MaxAge = 3600;
    }

    public override void OnActionExecuted(ActionExecutedContext context) {
        context.HttpContext.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue() {
            Public = true,
            MaxAge = TimeSpan.FromSeconds(MaxAge)
        };
        base.OnActionExecuted(context);
    }
}