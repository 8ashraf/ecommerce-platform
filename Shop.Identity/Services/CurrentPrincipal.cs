using System.Security.Claims;

namespace Shop.Identity.Services;

public class CurrentPrincipal(IHttpContextAccessor accessor) : ICurrentPrincipal
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid UserId => Guid.Parse(Require("sub"));
    public Guid AccountId => Guid.Parse(Require("account_id"));
    public string AccountType => Require("account_type");

    private string Require(string claim) =>User?.FindFirstValue(claim)?? throw new InvalidOperationException($"Claim '{claim}' is missing.");
}