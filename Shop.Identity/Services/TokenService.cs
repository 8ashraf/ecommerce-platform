using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shop.Identity.Entities;

namespace Shop.Identity.Services;

public class TokenService(IConfiguration config)//بتتقرأ من ملف الإعدادات appsettings.json
{
    public string CreateAccessToken(User user, Account account)
    {
        var key = new SymmetricSecurityKey(//     قراءة المفتاح من الإعدادات     تحويله إلى Bytes     إنشاء كائن يمثل مفتاح التوقيع
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

    var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),//sub: Subject claim, يمثل معرف المستخدم
            new("principal_type", "Human"),//principal_type: نوع الكيان الممثل للمستخدم (هنا "Human" يعني إنسان)
            new("account_id", account.Id.ToString()),  //account_id: معرف الحساب المرتبط بالمستخدم
            new("account_type", account.Type.ToString()) //account_type: نوع الحساب (Customer, Merchant, PlatformStaff)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"], //issuer: الجهة التي أصدرت التوكن (هنا "shop-identity")
            audience: config["Jwt:Audience"], //audience: الجهة المستهدفة بالتوكن (هنا "shop-api")
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));//signingCredentials: بيانات التوقيع الرقمي للتوكن، باستخدام المفتاح الذي تم إنشاؤه وخوارزمية HMAC-SHA256

        return new JwtSecurityTokenHandler().WriteToken(token);//كتابة التوكن في شكل نصي (String) ليتم إرساله للمستخدم
    }
}