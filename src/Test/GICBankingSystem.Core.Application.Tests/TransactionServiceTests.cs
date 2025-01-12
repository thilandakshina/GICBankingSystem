using GICBankingSystem.Core.Application.DTOs;
using GICBankingSystem.Core.Application.Interfaces.Repository;
using GICBankingSystem.Core.Application.Services;
using GICBankingSystem.Core.Application.Tests.Base;
using GICBankingSystem.Core.Domain.Enums;
using GICBankingSystem.Core.Domain.Models;
using Moq;

namespace GICBankingSystem.Core.Application.Tests;
[TestFixture]
public class TransactionServiceTests : ServiceTestBase<TransactionService>
{
    private Mock<IAccountRepository> _accountRepositoryMock;
    private Mock<IInterestRepository> _interestRepositoryMock;

    public override void Setup()
    {
        base.Setup();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _interestRepositoryMock = new Mock<IInterestRepository>();

        UnitOfWorkMock.Setup(u => u.AccountRepository).Returns(_accountRepositoryMock.Object);
        UnitOfWorkMock.Setup(u => u.InterestRepository).Returns(_interestRepositoryMock.Object);

        SetupMapperMocks();
    }

    protected override TransactionService CreateService()
    {
        return new TransactionService(UnitOfWorkMock.Object, MapperMock.Object);
    }

    private void SetupMapperMocks()
    {
        MapperMock.Setup(m => m.Map<TransactionEntity>(It.IsAny<TransactionDto>()))
            .Returns((TransactionDto dto) => new TransactionEntity());

        MapperMock.Setup(m => m.Map<StatementLineDto>(It.IsAny<TransactionEntity>()))
            .Returns((TransactionEntity entity) => new StatementLineDto
            {
                CreatedDate = entity.CreatedDate,
                TransactionId = entity.TransactionId,
                Type = entity.Type == TransactionType.Deposit ? "D" : "W",
                Amount = entity.Amount
            });
    }

    [Test]
    public async Task ProcessTransaction_ValidDeposit_ShouldCreateNewAccount()
    {
        // Arrange
        var transactionDto = new TransactionDto
        {
            AccountNo = "ACC001",
            Amount = 1000,
            Type = "D",
            CreatedDate = DateTime.Now
        };

        _accountRepositoryMock.Setup(r => r.GetByAccountNoAsync(It.IsAny<string>()))
            .ReturnsAsync((AccountEntity)null);

        _accountRepositoryMock.Setup(r => r.GenerateTransactionIdAsync(
            It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync("20240112-001");

        // Act
        var result = await Service.ProcessTransaction(transactionDto, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        _accountRepositoryMock.Verify(r => r.AddAccountAsync(It.IsAny<AccountEntity>()), Times.Once);
        _accountRepositoryMock.Verify(r => r.AddTransactionAsync(It.IsAny<TransactionEntity>()), Times.Once);
    }

    [Test]
    public async Task GetMonthlyStatement_ValidAccount_ShouldReturnCorrectStatement()
    {
        // Arrange
        var accountNo = "ACC001";
        var startDate = new DateTime(2024, 1, 1);
        var account = new AccountEntity();
        account.Add(accountNo, startDate);

        var transactions = new List<TransactionEntity>
        {
            CreateTestTransaction(startDate.AddDays(-5), "D", 1000),
            CreateTestTransaction(startDate.AddDays(2), "D", 500),
            CreateTestTransaction(startDate.AddDays(5), "W", 200)
        };

        _accountRepositoryMock.Setup(r => r.GetByAccountNoAsync(accountNo))
            .ReturnsAsync(account);

        _accountRepositoryMock.Setup(r => r.GetStatementAsync(
            accountNo, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await Service.GetMonthlyStatement(accountNo, startDate);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AccountNo, Is.EqualTo(accountNo));
        Assert.That(result.Transactions.Count, Is.EqualTo(2));
    }

    private TransactionEntity CreateTestTransaction(DateTime date, string type, decimal amount)
    {
        var transaction = new TransactionEntity();
        transaction.Add(
            date,
            type == "D" ? TransactionType.Deposit : TransactionType.Withdrawal,
            amount,
            "ACC001",
            $"{date:yyyyMMdd}-001"
        );
        return transaction;
    }

    [Test]
    public async Task GetStatement_ValidDateRange_ShouldReturnCorrectStatement()
    {
        // Arrange
        var accountNo = "ACC001";
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 1, 31);

        var transactions = new List<TransactionEntity>
        {
            CreateTestTransaction(fromDate.AddDays(1), "D", 1000),
            CreateTestTransaction(fromDate.AddDays(2), "W", 300),
            CreateTestTransaction(fromDate.AddDays(3), "D", 500)
        };

        _accountRepositoryMock.Setup(r => r.GetStatementAsync(
            accountNo, fromDate, toDate))
            .ReturnsAsync(transactions);

        // Act
        var result = await Service.GetStatement(accountNo, fromDate, toDate);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AccountNo, Is.EqualTo(accountNo));
        Assert.That(result.Transactions.Count, Is.EqualTo(3));
        Assert.That(result.FinalBalance, Is.EqualTo(1200)); // 1000 - 300 + 500
        Assert.That(result.StartingBalance, Is.EqualTo(0));
    }

    [Test]
    public void CalculateStartingBalance_WithMixedTransactions_ShouldReturnCorrectBalance()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 15);
        var transactions = new List<TransactionEntity>
        {
            CreateTestTransaction(startDate.AddDays(-10), "D", 1000),
            CreateTestTransaction(startDate.AddDays(-5), "W", 300),
            CreateTestTransaction(startDate.AddDays(1), "D", 500), // After start date, should not be included
            CreateTestTransaction(startDate.AddDays(2), "W", 200)  // After start date, should not be included
        };

        // Act
        var result = PrivateHelper.InvokePrivateMethod<decimal>(
            Service,
            "CalculateStartingBalance",
            new object[] { transactions, startDate });

        // Assert
        Assert.That(result, Is.EqualTo(700)); // 1000 - 300
    }

    [Test]
    public void GetMonthlyTransactions_WithValidData_ShouldReturnCorrectTransactions()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var startingBalance = 1000m;
        var transactions = new List<TransactionEntity>
        {
            CreateTestTransaction(startDate.AddDays(-1), "D", 500),  // Previous month
            CreateTestTransaction(startDate.AddDays(1), "D", 300),
            CreateTestTransaction(startDate.AddDays(15), "W", 200),
            CreateTestTransaction(startDate.AddMonths(1).AddDays(1), "D", 400) // Next month
        };

        // Act
        var result = PrivateHelper.InvokePrivateMethod<List<StatementLineDto>>(
            Service,
            "GetMonthlyTransactions",
            new object[] { transactions, startDate, startingBalance });

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2)); // Only transactions within the month
        Assert.That(result[0].Amount, Is.EqualTo(300));
        Assert.That(result[1].Amount, Is.EqualTo(200));
    }

    private static class PrivateHelper
    {
        public static T InvokePrivateMethod<T>(object instance, string methodName, object[] parameters)
        {
            var type = instance.GetType();
            var method = type.GetMethod(methodName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            return (T)method.Invoke(instance, parameters);
        }
    }
}