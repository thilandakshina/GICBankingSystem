using System.Transactions;
using GICBankingSystem.Core.Domain.Enums;
using GICBankingSystem.Core.Domain.Exceptions;
using GICBankingSystem.Core.Domain.ValueObjects;

namespace GICBankingSystem.Core.Domain.Models;

public class TransactionEntity
{
    public Guid Id { get; set; }
    public string TransactionId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string AccountNo { get; set; }
    public DateTime CreatedDate { get; set; }

    public void Add(DateTime createdDate, TransactionType type, decimal amount, string accountNo, string transactionId)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero");

        CreatedDate = createdDate;
        Type = type;
        Amount = amount;
        AccountNo = accountNo;
        TransactionId = transactionId;
        Id = Guid.NewGuid();
    }
}
