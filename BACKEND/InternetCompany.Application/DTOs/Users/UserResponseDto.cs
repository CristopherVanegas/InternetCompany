namespace InternetCompany.Application.DTOs.Users
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsApproved { get; set; }
    }
}
