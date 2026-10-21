using Microsoft.EntityFrameworkCore;
using Shop.Identity.Entities;

namespace Shop.Identity;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<OtpChallenge> OtpChallenges => Set<OtpChallenge>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.HasDefaultSchema("identity");

        b.Entity<User>().HasIndex(u => u.Phone).IsUnique();
        b.Entity<OrganizationMember>().HasKey(m => new { m.OrgId, m.UserId });
        b.Entity<Account>().HasIndex(a => new { a.UserId, a.Type }).IsUnique();

        b.Entity<Account>()
        .HasOne(a => a.User)
        .WithMany(u => u.Accounts)
        .HasForeignKey(a => a.UserId)
        .OnDelete(DeleteBehavior.Restrict);

        b.Entity<OrganizationMember>()
        .HasOne(m => m.Org)
        .WithMany(o => o.Members)
        .HasForeignKey(m => m.OrgId)
        .OnDelete(DeleteBehavior.Restrict);

        b.Entity<OrganizationMember>()
        .HasOne(m => m.User)
        .WithMany(u => u.Memberships)
        .HasForeignKey(m => m.UserId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}