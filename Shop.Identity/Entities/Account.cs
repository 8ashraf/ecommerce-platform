namespace Shop.Identity.Entities;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }//FK لسه ما اتعملت
    public AccountType Type { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Pending;
}