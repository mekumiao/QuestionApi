using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;

using MapsterMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Controllers;

/// <summary>
/// 用户
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[Authorize(Roles = "admin")]
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public partial class UsersController(ILogger<UsersController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<UsersController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    [GeneratedRegex("^https?://")]
    private static partial Regex HttpSchemeRegex();

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount([FromQuery] UserFilter filter) {
        var queryable = _dbContext.Set<AppUser>().AsNoTracking();
        queryable = filter.Build(queryable);
        var result = await queryable.CountAsync();
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(UserDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] UserFilter filter, [FromQuery] Paging paging) {
        var users = paging.Build(_dbContext.Set<AppUser>());
        users = filter.Build(users);

        var queryable = from u in users
                        join urr in from ur in _dbContext.UserRoles
                                    join r in _dbContext.Roles on ur.RoleId equals r.Id
                                    select new { ur.UserId, r } on u.Id equals urr.UserId into grouping
                        from urr in grouping.DefaultIfEmpty()
                        group new { u, urr } by new {
                            UserId = u.Id,
                            u.UserName,
                            u.Email,
                            u.NickName,
                            u.Avatar,
                            u.CreateTime,
                            u.LockoutEnabled,
                        } into g
                        select new UserDto {
                            UserId = g.Key.UserId,
                            UserName = g.Key.UserName,
                            NickName = g.Key.NickName,
                            Avatar = g.Key.Avatar,
                            CreateTime = g.Key.CreateTime,
                            Email = g.Key.Email,
                            Roles = g.OrderBy(v => v.urr.r.Id).Select(v => v.urr.r.Name)!,
                            LockoutEnabled = g.Key.LockoutEnabled,
                        };

        var result = await queryable.AsNoTracking().OrderByDescending(v => v.CreateTime).ToArrayAsync();
        var avatarBuilder = new UriBuilder(HttpContext.Request.Host.Value);
        var pathBase = HttpContext.Request.PathBase.Value?.TrimEnd('/');
        foreach (var item in result) {
            if (!string.IsNullOrWhiteSpace(item.Avatar)) {
                if (HttpSchemeRegex().IsMatch(item.Avatar)) {
                    continue;
                }
                avatarBuilder.Path = $"{pathBase}/{item.Avatar.TrimStart('/')}";
                item.Avatar = avatarBuilder.Uri.AbsoluteUri;
            }
        }
        return Ok(result);
    }

    [HttpGet("{userId:int}", Name = "GetUserById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserById([FromRoute] int userId,
                                                 [FromServices] UserManager<AppUser> userManager) {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) {
            return NotFound();
        }
        var roles = await userManager.GetRolesAsync(user);
        var result = _mapper.Map<UserDto>(user);
        result.Roles = roles;
        return Ok(result);
    }

    [HttpPut("{userId:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] int userId,
                                            [FromBody, FromForm] UserUpdate dto,
                                            [FromServices] UserManager<AppUser> userManager) {
        var item = await userManager.FindByIdAsync(userId.ToString());
        if (item is null) {
            return NotFound();
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        if (dto.Roles is not null) {
            await _dbContext.UserRoles.Where(v => v.UserId == userId).ExecuteDeleteAsync();
            await userManager.AddToRolesAsync(item, dto.Roles.Distinct());
        }
        if (dto.LockoutEnabled.HasValue) {
            await userManager.SetLockoutEnabledAsync(item, dto.LockoutEnabled.Value);
            if (dto.LockoutEnabled.Value) {
                await userManager.SetLockoutEndDateAsync(item, DateTimeOffset.UtcNow.AddYears(10));
            }
        }
        if (string.IsNullOrWhiteSpace(dto.NickName) is false) {
            await _dbContext.Students.Where(v => v.UserId == userId)
                .ExecuteUpdateAsync(v => v.SetProperty(b => b.StudentName, dto.NickName));
        }
        _mapper.Map(dto, item);
        await userManager.UpdateAsync(item);
        await transaction.CommitAsync();

        var result = _mapper.Map<UserDto>(item);
        var roles = await userManager.GetRolesAsync(item);
        result.Roles = roles;

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody, FromForm] UserInput dto,
                                            [FromServices] UserManager<AppUser> userManager,
                                            [FromServices] IUserStore<AppUser> userStore) {
        var emailStore = (IUserEmailStore<AppUser>)userStore;
        var user = _mapper.Map<AppUser>(dto);
        user.CreateTime = DateTime.UtcNow;
        await userStore.SetUserNameAsync(user, dto.Email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, dto.Email, CancellationToken.None);
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) {
            return ValidationProblem(result.ToString());
        }
        if (dto.Roles.Count != 0) {
            await userManager.AddToRolesAsync(user, dto.Roles.Distinct());
        }
        var roles = await userManager.GetRolesAsync(user);
        var poco = _mapper.Map<UserDto>(user);
        poco.Roles = roles;
        return CreatedAtRoute("GetUserById", new { userId = user.Id }, poco);
    }
}