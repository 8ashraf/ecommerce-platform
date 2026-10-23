namespace Shop.Identity.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public string TokenHash { get; set; } = "";// hashed value of the token
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }// if the token was revoked, store the date here
    public string? ReplacedByHash { get; set; }// if this token was replaced by another token, store the hash of the new token here
}