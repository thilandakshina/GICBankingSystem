namespace GICBankingSystem.Core.Application.DTOs;

[ExcludeFromCodeCoverage]
public record StatementDto
{
    public string AccountNo { get; init; }
    public List<StatementLineDto> Transactions { get; init; } = new();
    public decimal StartingBalance { get; init; }
    public decimal FinalBalance { get; init; }
}
