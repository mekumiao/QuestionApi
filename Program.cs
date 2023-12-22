using System.Text.Json;
using System.Text.Json.Serialization;

using Mapster;

using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using QuestionApi;
using QuestionApi.Database;
using QuestionApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

if (builder.Environment.IsDevelopment()) {
    builder.Services.AddAuthentication().AddAuthorizationCode();
}
else {
    builder.Services.AddIdentityApiEndpoints<AppUser>(options => {
        options.ClaimsIdentity.RoleClaimType = "role";
        options.ClaimsIdentity.UserNameClaimType = "name";
        options.ClaimsIdentity.UserIdClaimType = "sub";
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;

    })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<QuestionDbContext>();
}

builder.Services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, (options) => {
    if (builder.Environment.IsDevelopment()) {
        options.BearerTokenExpiration = TimeSpan.FromDays(356);
    }
});

builder.Services.AddScoped<ExamPaperService>();

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
app.UseAuthorization();
// app.MapIdentityApi<AppUser>();
app.MapControllers();

app.Run();