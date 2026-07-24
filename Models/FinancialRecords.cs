namespace SafeVault.Api.Models;

public class FinancialRecord
{
    public int Id { get; set; }
    
    // Links this sensitive record to a specific IdentityUser
    public required string UserId { get; set; }
    
    public required string AccountName { get; set; }
    public decimal Balance { get; set; }
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}