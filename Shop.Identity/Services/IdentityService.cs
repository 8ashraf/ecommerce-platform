using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Shop.Identity.Entities;

namespace Shop.Identity.Services;

public record VerifyResult(Account Account, string AccessToken, string RefreshToken);

public class IdentityService(AppDbContext db, TokenService tokens)
{
    public async Task StartRegistrationAsync(string phone)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        db.OtpChallenges.Add(new OtpChallenge
        {
            Phone = phone,
            CodeHash = Hash(code),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5) // بينتهي بعد 5 دقايق
        });
        await db.SaveChangesAsync();

        Console.WriteLine($"[DEV OTP] {phone} => {code}");
    }

    public async Task<VerifyResult?> VerifyAsync(string phone, string code)
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

            // لو المستخدم موظف منصة فعّال، نصدر الـ token بحساب الموظف
        var staff = await db.Accounts.FirstOrDefaultAsync(a =>
            a.UserId == user.Id &&
            a.Type == AccountType.PlatformStaff &&
            a.Status == AccountStatus.Active);
            var active = staff ?? account;

var refresh = await IssueRefreshTokenAsync(user.Id, active.Id);
return new VerifyResult(active, tokens.CreateAccessToken(user, active), refresh);
    }

    // تجديد الجلسة: بياخد refresh token قديم ويدّي access + refresh جداد
    public async Task<VerifyResult?> RefreshAsync(string refreshToken)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
        if (stored is null) return null;

        // token اتلغى قبل كده = استخدام مشبوه، نلغي كل جلسات الحساب
        if (stored.RevokedAt is not null)
        {
            var sessions = await db.RefreshTokens
                .Where(r => r.AccountId == stored.AccountId && r.RevokedAt == null)
                .ToListAsync();
            foreach (var s in sessions) s.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return null;
        }

        if (stored.ExpiresAt <= DateTime.UtcNow) return null;

        var user = await db.Users.FindAsync(stored.UserId);
        var account = await db.Accounts.FindAsync(stored.AccountId);
        if (user is null || account is null || account.Status != AccountStatus.Active)
            return null;

        var newRaw = await IssueRefreshTokenAsync(user.Id, account.Id);
        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByHash = Hash(newRaw);
        await db.SaveChangesAsync();

        return new VerifyResult(account, tokens.CreateAccessToken(user, account), newRaw);
    }

    // تسجيل الخروج: بنلغي الـ refresh token
    public async Task LogoutAsync(string refreshToken)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
        if (stored is not null && stored.RevokedAt is null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    // بيولّد قيمة عشوائية، بيخزن hash بتاعها بس، وبيرجّع القيمة الأصلية للمستخدم
    private async Task<string> IssueRefreshTokenAsync(Guid userId, Guid accountId)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));// 64 bytes = 512 bits of randomness
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            AccountId = accountId,
            TokenHash = Hash(raw),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });
        await db.SaveChangesAsync();
        return raw;
    }

    private static string Hash(string s) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
}