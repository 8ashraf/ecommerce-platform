using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shop.Identity;
using Shop.Identity.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration["constr"]));
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IdentityService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false; // نحافظ على أسماء الـ claims زي ما هي (sub...)
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/identity/register", async (RegisterRequest req, IdentityService svc) =>
{
    if (string.IsNullOrWhiteSpace(req.Phone))
        return Results.BadRequest(new { error = "Phone is required" });

    await svc.StartRegistrationAsync(req.Phone);
    return Results.Ok(new { message = "OTP sent" });
});

app.MapPost("/identity/verify", async (VerifyRequest req, IdentityService svc) =>
{
    var result = await svc.VerifyAsync(req.Phone, req.Code);
    return result is null
        ? Results.Unauthorized()
        : Results.Ok(new
        {
            accountId = result.Account.Id,
            type = result.Account.Type.ToString(),
            accessToken = result.AccessToken
        });
});

// endpoint محمي: بيرجّع بيانات المستخدم الحالي من الـ token
app.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
{
    userId = user.FindFirstValue("sub"),
    accountId = user.FindFirstValue("account_id"),
    accountType = user.FindFirstValue("account_type")
})).RequireAuthorization();

app.Run();

record RegisterRequest(string Phone);
record VerifyRequest(string Phone, string Code);