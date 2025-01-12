using GICBankingSystem.Core.Application.Interfaces.Repository;
using GICBankingSystem.Core.Domain.Models;
using GICBankingSystem.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GICBankingSystem.Core.Infrastructure.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccountEntity> GetByAccountNoAsync(string accountNo)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNo == accountNo);
    }

    public async Task<string> GenerateTransactionIdAsync(DateTime date , string accountNo)
    {
        var transactionCount = await _context.Transactions
            .Where(t => t.CreatedDate == date && t.AccountNo == accountNo)
            .CountAsync();

        int sequence = 1;
        if (transactionCount != 0)
        {
            sequence = transactionCount + 1;
        }

        var transactionId = TransactionId.Create(date, sequence);
        return transactionId.IsSuccess ? transactionId.Value.Value : throw new Exception("Failed to generate transaction ID");
    }

    public async Task AddAccountAsync(AccountEntity account)
    {
        await _context.Accounts.AddAsync(account);
    }

    public async Task AddTransactionAsync(TransactionEntity transaction)
    {
        await _context.Transactions.AddAsync(transaction);
    }

    public async Task<IEnumerable<TransactionEntity>> GetStatementAsync(string accountNo, DateTime startDate, DateTime endDate)
    {
        var transactions = await _context.Transactions
            .Where(t => t.AccountNo == accountNo &&
                t.CreatedDate >= startDate && t.CreatedDate <= endDate
                )
            .ToListAsync();

        return transactions;

    }

}
