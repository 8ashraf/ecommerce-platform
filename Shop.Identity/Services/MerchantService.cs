using Microsoft.EntityFrameworkCore;
using Shop.Identity.Entities;

namespace Shop.Identity.Services;

public record MerchantRegistrationResult(Guid OrganizationId, Guid AccountId);

public class MerchantService(AppDbContext db)
{
    public async Task<MerchantRegistrationResult?> RegisterAsync(Guid userId, string storeName)
    {
        // المستخدم عنده حساب تاجر بالفعل؟
        var exists = await db.Accounts
            .AnyAsync(a => a.UserId == userId && a.Type == AccountType.Merchant);
        if (exists) return null;

        var org = new Organization { Name = storeName.Trim() };
        var account = new Account
        {
            UserId = userId,
            Type = AccountType.Merchant,
            Status = AccountStatus.Pending
        };

        db.Organizations.Add(org);
        db.Accounts.Add(account);
        db.OrganizationMembers.Add(new OrganizationMember
        {
            OrgId = org.Id,
            UserId = userId,
            Role = OrgRole.Owner
        });

        // حفظ واحد = transaction واحدة
        await db.SaveChangesAsync();
        return new MerchantRegistrationResult(org.Id, account.Id);
    }


    public async Task<bool> ApproveAsync(Guid organizationId)
    {
        var org = await db.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId);
        if (org is null || org.Status != OrgStatus.Pending) return false;

        var ownerId = await db.OrganizationMembers
            .Where(m => m.OrgId == organizationId && m.Role == OrgRole.Owner)
            .Select(m => m.UserId)
           .FirstOrDefaultAsync();
        if (ownerId == Guid.Empty) return false;

        var merchantAccount = await db.Accounts.FirstOrDefaultAsync(a =>
            a.UserId == ownerId && a.Type == AccountType.Merchant);
        if (merchantAccount is null) return false;

        org.Status = OrgStatus.Active;
        merchantAccount.Status = AccountStatus.Active;

        await db.SaveChangesAsync();
        return true;
    }
}