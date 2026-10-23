using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shop.Identity;
using Shop.Identity.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- الخدمات ----------
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration["constr"]));
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IdentityService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentPrincipal, CurrentPrincipal>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)//بنستخدم الـ JWT Bearer Authentication
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

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "الصق الـ accessToken هنا (من غير كلمة Bearer ومن غير تنصيص)"
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------- الـ Middleware (الترتيب مهم) ----------
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// ---------- الـ Endpoints ----------
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
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken
        });
});

app.MapPost("/identity/refresh", async (RefreshRequest req, IdentityService svc) =>
{
    var result = await svc.RefreshAsync(req.RefreshToken);
    return result is null
        ? Results.Unauthorized()
        : Results.Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken
        });
});

app.MapPost("/identity/logout", async (RefreshRequest req, IdentityService svc) =>
{
    await svc.LogoutAsync(req.RefreshToken);
    return Results.NoContent();
});

// endpoint محمي: بيرجّع بيانات المستخدم الحالي من الـ token
app.MapGet("/me", (ICurrentPrincipal me) => Results.Ok(new
{
    userId = me.UserId,
    accountId = me.AccountId,
    accountType = me.AccountType
})).RequireAuthorization();

app.Run();

record RegisterRequest(string Phone);
record VerifyRequest(string Phone, string Code);
record RefreshRequest(string RefreshToken);