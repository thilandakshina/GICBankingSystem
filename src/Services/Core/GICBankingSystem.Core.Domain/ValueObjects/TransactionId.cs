namespace GICBankingSystem.Core.Domain.ValueObjects;

public class TransactionId : ValueObject
{
    public string Value { get; }

    private TransactionId(string value)
    {
        Value = value;
    }

    public static Result<TransactionId> Create(DateTime date, int sequence)
    {
        //date = date.Replace("-", "");
        if (sequence <= 0)
            return Result<TransactionId>.Failure("Sequence must be greater than zero");
        var id = $"{date:yyyyMMdd}-{sequence}";  // Format date as yyyy-MM-dd

        //var id = $"{date}-{sequence}";
        return Result<TransactionId>.Success(new TransactionId(id));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(TransactionId transactionId) => transactionId.Value;
}
