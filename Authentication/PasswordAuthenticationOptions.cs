using Microsoft.AspNetCore.Authentication;

namespace QuestionApi.Authentication;

public class PasswordAuthenticationOptions : AuthenticationSchemeOptions {
    public string Username = "Username";
    public string Password = "Password";
}