using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace QuestionApi.Authentication;

public class AuthorizationCodeAuthenticationHandler(
    IOptionsMonitor<AuthorizationCodeAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthorizationCodeAuthenticationOptions>(options, logger, encoder) {
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
        await Task.CompletedTask;
        if (!Context.Request.Headers.TryGetValue(Options.AuthorizationCode, out var authorizationCode)) {
            if (!Context.Request.Query.TryGetValue(Options.AuthorizationCode, out authorizationCode)) {
                return AuthenticateResult.NoResult();
            }
        }

        if (!Options.ValidateAuthorizationCode(authorizationCode.ToString())) {
            return AuthenticateResult.NoResult();
        }

        var principal = Options.CreateClaimsPrincipal(Scheme.Name, authorizationCode.ToString());
        var ticket = new AuthenticationTicket(
            principal,
            Scheme.Name
        );
        return AuthenticateResult.Success(ticket);
    }
}