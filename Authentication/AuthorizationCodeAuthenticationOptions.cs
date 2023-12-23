using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

namespace QuestionApi.Authentication;

public class AuthorizationCodeAuthenticationOptions : AuthenticationSchemeOptions {
    public string AuthorizationCode = "AuthorizationCode";

    public ClaimsPrincipal CreateClaimsPrincipal(string scheme, string authorizationCode) {
        var identity = new ClaimsIdentity(scheme, "name", "role");

        identity.AddClaim(new Claim("role", "admin"));
        identity.AddClaim(new Claim("sub", "1", ClaimValueTypes.Integer32));
        identity.AddClaim(new Claim("name", "admin"));
        identity.AddClaim(new Claim("nickname", "admin"));

        return new ClaimsPrincipal(identity);
    }

    public bool ValidateAuthorizationCode(string authorizationCode) {
        return authorizationCode == "123123";
    }
}