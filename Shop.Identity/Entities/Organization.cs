namespace Shop.Identity.Entities;

public class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public OrgStatus Status { get; set; } = OrgStatus.Pending;
}

public class OrganizationMember
{
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public OrgRole Role { get; set; }
}