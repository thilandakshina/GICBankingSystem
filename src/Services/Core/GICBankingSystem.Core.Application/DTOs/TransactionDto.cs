namespace GICBankingSystem.Core.Application.DTOs;

[ExcludeFromCodeCoverage]
public record TransactionDto
{
    public DateTime CreatedDate { get; init; }
    public string AccountNo { get; init; }
    public string Type { get; init; }
    public decimal Amount { get; init; }
}