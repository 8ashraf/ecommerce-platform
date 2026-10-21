namespace Shop.Identity.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public bool PhoneVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Account> Accounts { get; set; } = new();//عشان اعمل علاقة بين اليوزر والحسابات بتاعته
    public List<OrganizationMember> Memberships { get; set; } = new();//عشان اعمل علاقة بين اليوزر والمنظمات اللي هو عضو فيها    
}