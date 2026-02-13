namespace InternetCompany.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public int StatusId { get; set; }
    public UserStatus Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsApproved { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

}
