using InternetCompany.Application.DTOs.Auth;
using InternetCompany.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InternetCompany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
}
