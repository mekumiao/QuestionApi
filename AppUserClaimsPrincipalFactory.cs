using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using QuestionApi.Database;

namespace QuestionApi;

public class AppUserClaimsPrincipalFactory(UserManager<AppUser> userManager,
                                           RoleManager<AppRole> roleManager,
                                           IOptions<IdentityOptions> optionsAccessor,
                                           IServiceScopeFactory scopeFactory) : UserClaimsPrincipalFactory<AppUser, AppRole>(userManager, roleManager, optionsAccessor) {
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public async override Task<ClaimsPrincipal> CreateAsync(AppUser user) {
        var principal = await base.CreateAsync(user);
        if (principal.Identity is not null) {
            using var scope = _scopeFactory.CreateAsyncScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<QuestionDbContext>();
            var student = await dbContext.Students.AsNoTracking().SingleOrDefaultAsync(v => v.UserId == user.Id);
            if (student is not null) {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("studentId", student.StudentId.ToString()));
            }
        }
        return principal;
    }
}
