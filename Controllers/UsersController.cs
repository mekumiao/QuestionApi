using System.Net.Mime;

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
public class UsersController(ILogger<UsersController> logger, QuestionDbContext dbContext, IMapper mapper) : ControllerBase {
    private readonly ILogger<UsersController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

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
                        join us in _dbContext.UserRoles on u.Id equals us.UserId
                        join r in _dbContext.Roles on us.RoleId equals r.Id
                        group r by new { UserId = u.Id, u.UserName, u.Email, u.NickName } into g
                        select new UserDto {
                            UserId = g.Key.UserId,
                            UserName = g.Key.UserName,
                            NickName = g.Key.NickName,
                            Email = g.Key.Email,
                            Roles = g.OrderBy(v => v.Id).Select(v => v.Name)!
                        };

        var result = await queryable.AsNoTracking().ToArrayAsync();
        return Ok(_mapper.Map<UserDto[]>(result));
    }

    [HttpGet("{userId}", Name = "GetUserById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserById([FromRoute] string userId,
                                                 [FromServices] UserManager<AppUser> userManager) {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) {
            return NotFound();
        }
        var roles = await userManager.GetRolesAsync(user);
        var result = _mapper.Map<UserDto>(user);
        result.Roles = roles;
        return Ok(result);
    }

    [HttpPut("{userId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] string userId,
                                            [FromBody, FromForm] UserUpdate dto,
                                            [FromServices] UserManager<AppUser> userManager) {
        var item = await userManager.FindByIdAsync(userId);
        if (item is null) {
            return NotFound();
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        if (dto.Roles is not null) {
            await _dbContext.UserRoles.Where(v => v.UserId == userId).ExecuteDeleteAsync();
            await userManager.AddToRolesAsync(item, dto.Roles.Distinct());

            if (dto.Roles.Contains("student")) {
                if (!await _dbContext.Students.AnyAsync(v => v.UserId == userId)) {
                    _dbContext.Students.Add(new Student {
                        UserId = userId,
                        Name = item.UserName ?? string.Empty,
                    });
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
        _mapper.Map(dto, item);
        await userManager.UpdateAsync(item);
        await transaction.CommitAsync();

        var result = _mapper.Map<UserDto>(item);
        var roles = await userManager.GetRolesAsync(item);
        result.Roles = roles;

        return Ok(result);
    }
}