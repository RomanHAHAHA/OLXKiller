using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OLXKiller.API.Extensions;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.User;
using OLXKiller.Application.Options;

namespace OLXKiller.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController(
    IAccountsService _accountsService,
    IOptions<CustomCookieOptions> _options,
    IValidator<UserLoginDto> _userLoginValidator,
    IValidator<UserRegistrationDto> _userRegistrationValidator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginUserDto)
    {
        var validationResult = _userLoginValidator.Validate(loginUserDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.ToDictionary() });
        }

        var response = await _accountsService.Login(loginUserDto);

        if (response.IsFailure)
        {
            return this.HandleErrorResponse(response);
        }

        var token = response.Data;

        HttpContext.Response.Cookies.Append(_options.Value.Name, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
        });

        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto registerUserDto)
    {
        var validationResult = _userRegistrationValidator.Validate(registerUserDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.ToDictionary() });
        }

        var response = await _accountsService.Register(registerUserDto);

        if (response.IsFailure)
        {
            return this.HandleErrorResponse(response);
        }

        return Ok();
    }

    [HttpDelete("logout")]
    public IActionResult LogOut()
    {
        HttpContext.Response.Cookies.Delete(_options.Value.Name);

        return Ok();
    }

    [HttpGet("is-authorized")]
    [Authorize]
    public IActionResult IsAuthorized()
    {
        return Ok();
    }
}
