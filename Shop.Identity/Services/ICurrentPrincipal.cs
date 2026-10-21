namespace Shop.Identity.Services;

public interface ICurrentPrincipal
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    Guid AccountId { get; }
    string AccountType { get; }
}