using GICBankingSystem.Core.Domain.Enums;
using GICBankingSystem.Core.Domain.Exceptions;

namespace GICBankingSystem.Core.Domain.Models;

public class AccountEntity
{
    public string AccountNo { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedDate { get; set; }

    public void Add(string accountNo, DateTime date)
    {
        AccountNo = accountNo;
        CreatedDate = date;
        Balance = 0;
    }
    public void ProcessTransaction(TransactionEntity transaction)
    {
        if (transaction.Type == TransactionType.Withdrawal && Balance < transaction.Amount)
            throw new DomainException("Insufficient balance");

        Balance = transaction.Type == TransactionType.Deposit
            ? Balance + transaction.Amount
            : Balance - transaction.Amount;
    }
}