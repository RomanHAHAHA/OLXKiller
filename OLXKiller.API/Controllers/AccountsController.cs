using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OLXKiller.API.Extensions;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.User;
using OLXKiller.Application.Options;
using System.Net;

namespace OLXKiller.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly IAccountsService _accountsService;
    private readonly CustomCookieOptions _options;

    public AccountsController(
        IAccountsService accountsService,
        IOptions<CustomCookieOptions> options)
    {
        _accountsService = accountsService;
        _options = options.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
    {
        var response = await _accountsService.Login(loginUserDto);

        switch (response.Status)
        {
            case HttpStatusCode.OK:
                var token = response.Data;

                HttpContext.Response.Cookies.Append(_options.Name, token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                });

                return Ok(new { token });

            default:
                return this.CreateResponse(response.Status, response.Description);
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
    {
        var response = await _accountsService.Register(registerUserDto);

        if (response.IsSuccess)
        {
            return Ok();
        }

        return this.CreateResponse(response.Status, response.Description);
    }

    [HttpDelete("logout")]
    public IActionResult LogOut()
    {
        HttpContext.Response.Cookies.Delete(_options.Name);

        return Ok();
    }

    [HttpGet("is-authorized")]
    [Authorize]
    public IActionResult IsAuthorized()
    {
        return Ok();
    }
}
