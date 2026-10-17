using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shop.Identity.Entities;

namespace Shop.Identity;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<OtpChallenge> OtpChallenges => Set<OtpChallenge>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        optionsBuilder.UseNpgsql(configuration["constr"]);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.HasDefaultSchema("identity");

        b.Entity<User>().HasIndex(u => u.Phone).IsUnique();
        b.Entity<OrganizationMember>().HasKey(m => new { m.OrgId, m.UserId });
        b.Entity<Account>().HasIndex(a => new { a.UserId, a.Type }).IsUnique();
    }
}