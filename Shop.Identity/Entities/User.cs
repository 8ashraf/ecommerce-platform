namespace Shop.Identity.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public bool PhoneVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}