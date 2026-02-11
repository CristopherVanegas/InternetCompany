using InternetCompany.Application.DTOs.Auth;

namespace InternetCompany.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}
