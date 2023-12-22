using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using QuestionApi.Database;

namespace QuestionApi.Authentication;

public class PasswordAuthenticationHandler(
    IOptionsMonitor<PasswordAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<PasswordAuthenticationOptions>(options, logger, encoder) {
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
        await Task.CompletedTask;
        if (!Context.Request.Headers.TryGetValue(Options.Username, out var username)) {
            if (!Context.Request.Query.TryGetValue(Options.Username, out username)) {
                return AuthenticateResult.NoResult();
            }
        }

        if (string.IsNullOrWhiteSpace(username)) {
            return AuthenticateResult.NoResult();
        }

        if (!Context.Request.Headers.TryGetValue(Options.Password, out var password)) {
            if (!Context.Request.Query.TryGetValue(Options.Password, out password)) {
                return AuthenticateResult.NoResult();
            }
        }

        if (string.IsNullOrWhiteSpace(password)) {
            return AuthenticateResult.NoResult();
        }

        using var scope = Context.RequestServices.CreateScope();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<AppUser>>();

        var result = await signInManager.PasswordSignInAsync(username.ToString(), password.ToString(), false, true);
        if (!result.Succeeded) {
            return AuthenticateResult.NoResult();
        }

        var identity = new ClaimsIdentity(Scheme.Name, "name", "role");

        identity.AddClaim(new Claim("role", "admin"));
        identity.AddClaim(new Claim("sub", "1"));
        identity.AddClaim(new Claim("name", "admin"));
        identity.AddClaim(new Claim("nickname", "admin"));

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(
            principal,
            Scheme.Name
        );

        return AuthenticateResult.Success(ticket);
    }
}