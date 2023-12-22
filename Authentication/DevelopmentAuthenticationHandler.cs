using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace QuestionApi.Authentication;

public class DevelopmentAuthenticationHandler(
    IOptionsMonitor<DevelopmentAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<DevelopmentAuthenticationOptions>(options, logger, encoder) {
    private ClaimsPrincipal CreateClaimsPrincipal() {
        var identity = new ClaimsIdentity(Scheme.Name, "name", "role");

        identity.AddClaim(new Claim("sub", "1"));
        identity.AddClaim(new Claim("name", "admin@qq.com"));
        identity.AddClaim(new Claim("role", "admin"));

        return new ClaimsPrincipal(identity);
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
        var principal = CreateClaimsPrincipal();
        var ticket = new AuthenticationTicket(
            principal,
            Scheme.Name
        );
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}