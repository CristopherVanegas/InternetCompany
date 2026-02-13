using InternetCompany.Application.DTOs.Users;
using InternetCompany.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Admin,Gestor")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        int currentUserId = int.Parse(User.FindFirst("UserId")!.Value);
        var result = await _service.CreateAsync(dto, currentUserId);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        int adminId = int.Parse(User.FindFirst("UserId")!.Value);
        await _service.ApproveAsync(id, adminId);
        return Ok("Usuario aprobado");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }
}
