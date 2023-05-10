using CurrencyExchange.Application.Entities;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Application.Requests;
using CurrencyExchange.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.Controllers;

[ApiController]
[AllowAnonymous]
[Route("Users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly JwtOptions _jwtOptions;

    public UserController(IConfiguration configuration, IUserService userService)
    {
        _userService = userService;
        _jwtOptions = new JwtOptions(configuration);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginRequest request)
    {
        string token = await _userService.LoginAsync(request, _jwtOptions.Secret);
        
        return Ok(token);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(
        [FromBody]UserRequest request, 
        CancellationToken cancellationToken)
    {
        User user = await _userService.RegisterAsync(request, cancellationToken);
        
        return Ok(user);
    }
}