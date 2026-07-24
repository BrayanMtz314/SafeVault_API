using System.ComponentModel.DataAnnotations;

namespace SafeVault.Api.DTOs;

public class CreateFinancialRecordDto
{
    [Required(ErrorMessage = "Account Name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Account Name must be between 3 and 100 characters.")]
    // Regex prevents basic XSS by restricting input to alphanumeric characters, spaces, and basic punctuation
    [RegularExpression(@"^[a-zA-Z0-9\s\-_,\.]+$", ErrorMessage = "Account Name contains invalid characters.")]
    public required string AccountName { get; set; }

    [Required]
    [Range(-1000000000, 1000000000, ErrorMessage = "Balance is out of safe transaction limits.")]
    public decimal Balance { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_,\.\$]*$", ErrorMessage = "Description contains invalid characters or potential script tags.")]
    public string? Description { get; set; }
}

public class UpdateFinancialRecordDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_,\.]+$", ErrorMessage = "Account Name contains invalid characters.")]
    public required string AccountName { get; set; }

    [Required]
    [Range(-1000000000, 1000000000)]
    public decimal Balance { get; set; }

    [StringLength(500)]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_,\.\$]*$", ErrorMessage = "Description contains invalid characters.")]
    public string? Description { get; set; }
}

// Used to return data securely without exposing internal metadata if we don't want to
public class FinancialRecordResponseDto
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string AccountName { get; set; }
    public decimal Balance { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}