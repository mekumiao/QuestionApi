using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace QuestionApi;

public class AuthorizeCheckOperationFilter : IOperationFilter {
    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
        if (context.MethodInfo.DeclaringType is null) {
            return;
        }

        // 排除特点的接口
        if (IsExclude(context.MethodInfo, "ProductImageController.GetProductImage")) {
            return;
        }

        var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
          || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (hasAuthorize) {
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            operation.Security = new List<OpenApiSecurityRequirement> {
                new() {
                    [
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = IdentityConstants.BearerScheme
                            }
                        }
                    ] = Array.Empty<string>()
                }
            };
        }
    }

    private static bool IsExclude(System.Reflection.MethodInfo methodInfo, string exclude) {
        return $"{methodInfo.DeclaringType?.Name}.{methodInfo.Name}" == exclude;
    }
}