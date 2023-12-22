using Microsoft.AspNetCore.Authentication;

using QuestionApi.Authentication;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuthenticationBuilderExtension {
    public static AuthenticationBuilder AddDevelopment(this AuthenticationBuilder builder) {
        return builder.AddDevelopment(DevelopmentAuthenticationDefaults.AuthenticationScheme);
    }

    public static AuthenticationBuilder AddDevelopment(this AuthenticationBuilder builder, string schema) {
        return builder.AddDevelopment(schema, _ => { });
    }

    public static AuthenticationBuilder AddDevelopment(this AuthenticationBuilder builder, Action<DevelopmentAuthenticationOptions> configureOptions) {
        return builder.AddDevelopment(DevelopmentAuthenticationDefaults.AuthenticationScheme, configureOptions);
    }

    public static AuthenticationBuilder AddDevelopment(this AuthenticationBuilder builder, string schema, Action<DevelopmentAuthenticationOptions> configureOptions) {
        return builder.AddScheme<DevelopmentAuthenticationOptions, DevelopmentAuthenticationHandler>(schema, configureOptions);
    }

    public static AuthenticationBuilder AddPassword(this AuthenticationBuilder builder) {
        return builder.AddPassword(PasswordAuthenticationDefaults.AuthenticationScheme);
    }

    public static AuthenticationBuilder AddPassword(this AuthenticationBuilder builder, string schema) {
        return builder.AddPassword(schema, _ => { });
    }

    public static AuthenticationBuilder AddPassword(this AuthenticationBuilder builder, Action<PasswordAuthenticationOptions> configureOptions) {
        return builder.AddPassword(PasswordAuthenticationDefaults.AuthenticationScheme, configureOptions);
    }

    public static AuthenticationBuilder AddPassword(this AuthenticationBuilder builder, string schema, Action<PasswordAuthenticationOptions> configureOptions) {
        return builder.AddScheme<PasswordAuthenticationOptions, PasswordAuthenticationHandler>(schema, configureOptions);
    }

    public static AuthenticationBuilder AddAuthorizationCode(this AuthenticationBuilder builder) {
        return builder.AddAuthorizationCode(AuthorizationCodeAuthenticationDefaults.AuthenticationScheme);
    }

    public static AuthenticationBuilder AddAuthorizationCode(this AuthenticationBuilder builder, string schema) {
        return builder.AddAuthorizationCode(schema, _ => { });
    }

    public static AuthenticationBuilder AddAuthorizationCode(this AuthenticationBuilder builder, Action<AuthorizationCodeAuthenticationOptions> configureOptions) {
        return builder.AddAuthorizationCode(AuthorizationCodeAuthenticationDefaults.AuthenticationScheme, configureOptions);
    }

    public static AuthenticationBuilder AddAuthorizationCode(this AuthenticationBuilder builder, string schema, Action<AuthorizationCodeAuthenticationOptions> configureOptions) {
        return builder.AddScheme<AuthorizationCodeAuthenticationOptions, AuthorizationCodeAuthenticationHandler>(schema, configureOptions);
    }
}