namespace InternetCompany.Domain.Entities;

public class UserStatus
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;

    public ICollection<User> Users { get; set; } = new List<User>();
}
