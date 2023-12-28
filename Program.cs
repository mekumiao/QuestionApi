using System.Text.Json;
using System.Text.Json.Serialization;

using Mapster;

using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using OfficeOpenXml;

using QuestionApi;
using QuestionApi.Database;
using QuestionApi.Services;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddLogging(builder => builder.AddConsole());

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});
builder.Services.AddDbContext<QuestionDbContext>(options =>
    options.UseNpgsql("Host=mini.dev;Username=postgres;Database=questiondb",
        v => v.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
);
builder.Services.AddMapster();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// builder.Services.AddIdentity<AppUser, AppRole>(options => {
//     options.ClaimsIdentity.RoleClaimType = "role";
//     options.ClaimsIdentity.UserNameClaimType = "name";
//     options.ClaimsIdentity.UserIdClaimType = "sub";

//     options.Password.RequireUppercase = false;
//     options.Password.RequireNonAlphanumeric = false;
//     options.Password.RequireDigit = false;
//     options.Password.RequireLowercase = false;

//     // 其他配置...
//     options.Lockout.AllowedForNewUsers = true; // 允许新用户锁定
//     options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 锁定时间
//     options.Lockout.MaxFailedAccessAttempts = 3; // 允许的最大登录失败尝试次数
// })
//     .AddEntityFrameworkStores<QuestionDbContext>()
//     .AddDefaultTokenProviders();

builder.Services.AddIdentityApiEndpoints<AppUser>(options => {
    options.ClaimsIdentity.RoleClaimType = "role";
    options.ClaimsIdentity.UserNameClaimType = "name";
    options.ClaimsIdentity.UserIdClaimType = "sub";

    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;

    options.Lockout.AllowedForNewUsers = false; // 新建用户默认不锁定
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 锁定时间
    options.Lockout.MaxFailedAccessAttempts = 5; // 登录失败5次后锁定
})
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<QuestionDbContext>();

builder.Services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, (options) => {
    if (builder.Environment.IsDevelopment()) {
        options.BearerTokenExpiration = TimeSpan.FromDays(356);
    }
});

builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

builder.Services.AddScoped<ExamPaperService>();
builder.Services.AddScoped<AnswerBoardService>();
builder.Services.AddScoped<QuestionService>();

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

builder.Services.AddCors(options => options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);
TypeAdapterConfig.GlobalSettings.Apply(new MappingRegister());
TypeAdapterConfig.GlobalSettings.Compile();

builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);

var app = builder.Build();

// using var scope = app.Services.CreateScope();
// var factory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<AppUser>>();

// Configure the HTTP request pipeline.

app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.All // 可转发前缀
});

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapIdentityApi<AppUser>();
app.MapControllers();

app.Run();