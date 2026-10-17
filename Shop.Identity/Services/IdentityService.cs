using System.Security.Cryptography;//RandomNumberGeneratorداله ال
using System.Text;
using Microsoft.EntityFrameworkCore;
using Shop.Identity.Entities;

namespace Shop.Identity.Services;

public class IdentityService(AppDbContext db)
{
    public async Task StartRegistrationAsync(string phone)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();//بدأ من 100000 لحد 999999 ارقام من 6

        db.OtpChallenges.Add(new OtpChallenge
        {
            Phone = phone,
            CodeHash = Hash(code),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)// بنتهي بعد 5 دقايق
        });
        await db.SaveChangesAsync();

        Console.WriteLine($"[DEV OTP] {phone} => {code}");
    }

    public async Task<Account?> VerifyAsync(string phone, string code)// بتأكد من صحة الكود واذا صح بيرجع الحساب واذا غلط بيرجع null
    {
        var challenge = await db.OtpChallenges
            .Where(c => c.Phone == phone && !c.Consumed && c.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(c => c.ExpiresAt)
            .FirstOrDefaultAsync();

        if (challenge is null || challenge.Attempts >= 5) return null;

        challenge.Attempts++;
        if (challenge.CodeHash != Hash(code))
        {
            await db.SaveChangesAsync();
            return null;
        }
        challenge.Consumed = true;

        var user = await db.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        if (user is null)
        {
            user = new User { Phone = phone };
            db.Users.Add(user);
        }
        user.PhoneVerified = true;

        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Type == AccountType.Customer);
        if (account is null)
        {
            account = new Account
            {
                UserId = user.Id,
                Type = AccountType.Customer,
                Status = AccountStatus.Active
            };
            db.Accounts.Add(account);
        }

        await db.SaveChangesAsync();
        return account;
    }

    private static string Hash(string s) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
}