using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

using MapsterMapper;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using QuestionApi.Database;
using QuestionApi.Models;

namespace QuestionApi.Controllers;

/// <summary>
/// 账户
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
[ApiController]
[Authorize]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class AccountController(ILogger<AccountController> logger,
                               QuestionDbContext dbContext,
                               IMapper mapper,
                               IOptions<JwtBearerOptions> jwtOptions,
                               RoleManager<AppRole> roleManager,
                               SignInManager<AppUser> signInManager,
                               UserManager<AppUser> userManager) : ControllerBase {
    private readonly ILogger<AccountController> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly JwtBearerOptions _jwtOptions = jwtOptions.Value;
    private readonly RoleManager<AppRole> _roleManager = roleManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly UserManager<AppUser> _userManager = userManager;

    // [HttpPost("login")]
    // [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    // public async Task<IActionResult> Login([FromBody, FromForm] PasswordLogin passwordLogin) {
    //     var result = await _signInManager.PasswordSignInAsync(passwordLogin.Username, passwordLogin.Password, true, false);
    //     if (result.Succeeded) {
    //         var user = await _userManager.FindByNameAsync(passwordLogin.Username);

    //         // Get valid claims and pass them into JWT
    //         var claims = await GetValidClaims(passwordLogin);

    //         // Create the JWT security token and encode it.
    //         var jwt = new JwtSecurityToken(
    //             issuer: _jwtOptions.Issuer,
    //             audience: _jwtOptions.Audience,
    //             claims: claims,
    //             notBefore: _jwtOptions.NotBefore,
    //             expires: _jwtOptions.Expiration,
    //             signingCredentials: _jwtOptions.SigningCredentials);
    //         //...
    //     }
    //     else {
    //         return Unauthorized(result);
    //     }
    // }

    // private async Task<List<Claim>> GetValidClaims(AppUser user, string[] roles) {
    //     var issuer = _jwtOptions.TokenValidationParameters.ValidIssuer;
    //     var audience = _jwtOptions.TokenValidationParameters.ValidAudience;
    //     var secretKey = _jwtOptions.TokenValidationParameters.IssuerSigningKey;
    //     var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

    //     var claims = new List<Claim> {
    //         new(ClaimTypes.Name, user.UserName ?? string.Empty),
    //     };
    //     foreach (var role in roles) {
    //         claims.Add(new(ClaimTypes.Role, role));
    //     }

    //     var tokeOptions = new JwtSecurityToken(
    //         issuer: issuer,
    //         audience: audience,
    //         claims: claims,
    //         expires: DateTime.Now.AddMinutes(30),
    //         signingCredentials: signinCredentials
    //     );

    //     var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
    //     return claims;
    // }

    // [HttpPost("register")]
    // [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    // public async Task<IActionResult> Register([FromQuery] UserFilter filter) {
    //     var queryable = _dbContext.Set<AppUser>().AsNoTracking();
    //     queryable = filter.Build(queryable);
    //     var result = await queryable.CountAsync();
    //     return Ok(result);
    // }

    // [HttpPost("refresh")]
    // [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    // public async Task<IActionResult> Refresh([FromQuery] UserFilter filter) {
    //     var queryable = _dbContext.Set<AppUser>().AsNoTracking();
    //     queryable = filter.Build(queryable);
    //     var result = await queryable.CountAsync();
    //     return Ok(result);
    // }

    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInfo() {
        var userId = User.FindFirstValue("sub")!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) {
            return NotFound();
        }
        var roles = await _userManager.GetRolesAsync(user);
        var result = _mapper.Map<UserDto>(user);
        result.Roles = roles;
        return Ok(result);
    }

    [HttpPut("info")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInfo([FromBody, FromForm] InfoUpdate input) {
        var userId = User.FindFirstValue("sub")!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) {
            return NotFound();
        }
        _mapper.Map(input, user);
        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var result = _mapper.Map<UserDto>(user);
        result.Roles = roles;
        return Ok(result);
    }

    [HttpPut("change-password")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword([FromBody, FromForm] ChangePasswordInput input) {
        var userId = User.FindFirstValue("sub")!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) {
            return NotFound();
        }
        var result = await _userManager.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);
        return result.Succeeded ? NoContent() : ValidationProblem(result.ToString());
    }
}