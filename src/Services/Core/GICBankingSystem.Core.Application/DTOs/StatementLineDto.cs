namespace GICBankingSystem.Core.Application.DTOs;

[ExcludeFromCodeCoverage]
public record StatementLineDto
{
    public DateTime CreatedDate { get; init; }
    public string TransactionId { get; init; }
    public string Type { get; init; }
    public decimal Amount { get; init; }
    public decimal Balance { get; set; }
}
