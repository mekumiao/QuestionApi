using Mapster;

using Microsoft.AspNetCore.Identity;

using QuestionApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<QuestionDbContext>();
builder.Services.AddMapster();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>(options => {
    options.ClaimsIdentity.RoleClaimType = "role";
    options.ClaimsIdentity.UserNameClaimType = "name";
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<QuestionDbContext>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapIdentityApi<IdentityUser>();
app.MapControllers();

app.Run();