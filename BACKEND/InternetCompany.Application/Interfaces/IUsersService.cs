using InternetCompany.Application.DTOs.Users;

namespace InternetCompany.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateAsync(CreateUserDto dto, int currentUserId);
        Task ApproveAsync(int userId, int adminId);
        Task<IEnumerable<UserResponseDto>> GetAllAsync();
    }
}
