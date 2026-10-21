namespace Shop.Identity.Entities;

public class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public OrgStatus Status { get; set; } = OrgStatus.Pending;
    public List<OrganizationMember> Members { get; set; } = new();
}

public class OrganizationMember
{
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public OrgRole Role { get; set; }
    public Organization Org { get; set; } = null!;
    public User User { get; set; } = null!;
}