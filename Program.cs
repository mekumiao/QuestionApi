using Mapster;

using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using QuestionApi;
using QuestionApi.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<QuestionDbContext>(options =>
    options.UseNpgsql("Host=mini.dev;Username=postgres;Database=questiondb")
);
builder.Services.AddMapster();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>(options => {
    options.ClaimsIdentity.RoleClaimType = "role";
    options.ClaimsIdentity.UserNameClaimType = "name";
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<QuestionDbContext>();

builder.Services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, (options) => {
    options.BearerTokenExpiration = TimeSpan.FromHours(1);
    options.RefreshTokenExpiration = TimeSpan.FromDays(14);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition(IdentityConstants.BearerScheme, new OpenApiSecurityScheme() {
        In = ParameterLocation.Header,
        Description = "Please enter Token with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,

    });
    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

var app = builder.Build();

// using var scope = app.Services.CreateScope();
// var manager = scope.ServiceProvider.GetRequiredService<SignInManager<IdentityUser>>();
// var ss = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
// var schemes = scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
// var scheme = await schemes.GetSchemeAsync(manager.AuthenticationScheme);
// var scheme = await schemes.GetSchemeAsync(IdentityConstants.ApplicationScheme);
// var scheme = await schemes.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme);
// var handler = scope.ServiceProvider.GetRequiredService(scheme!.HandlerType);
// var signInHandler = handler as IAuthenticationSignInHandler;
// await signInHandler!.SignInAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapIdentityApi<IdentityUser>();
app.MapControllers();

app.Run();