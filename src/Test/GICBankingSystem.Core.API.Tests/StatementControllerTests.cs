using GICBankingSystem.Core.API.Controllers;
using GICBankingSystem.Core.Application.DTOs;
using GICBankingSystem.Core.Application.Queries.GetStatementQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GICBankingSystem.Core.API.Tests;

[TestFixture]
public class StatementControllerTests : ControllerTestBase<StatementController>
{
    protected override StatementController CreateController(IMediator mediator)
    {
        return new StatementController(mediator);
    }


    [Test]
    public async Task GetStatement_Success_ReturnsOkResult()
    {
        // Arrange
        var accountNo = "ACC001";
        var period = new DateTime(2024, 1, 1);
        var query = new GetStatementQuery(accountNo, period);

        var statement = new StatementDto
        {
            AccountNo = accountNo,
            StartingBalance = 1000,
            FinalBalance = 1500,
            Transactions = new List<StatementLineDto>
            {
                new()
                {
                    TransactionId = "TR001",
                    Type = "D",
                    Amount = 500,
                    Balance = 1500,
                    CreatedDate = period
                }
            }
        };

        _mediatorMock.Setup(m => m.Send(query, default))
            .ReturnsAsync(new GetStatementResult(statement));

        // Act
        var result = await _controller.GetStatement(accountNo, period);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        var response = okResult.Value as StatementDto;

        Assert.Multiple(() =>
        {
            Assert.That(response.AccountNo, Is.EqualTo(accountNo));
            Assert.That(response.StartingBalance, Is.EqualTo(1000));
            Assert.That(response.FinalBalance, Is.EqualTo(1500));
            Assert.That(response.Transactions.Count, Is.EqualTo(1));
            Assert.That(response.Transactions[0].Amount, Is.EqualTo(500));
        });
    }

    [Test]
    public async Task GetStatement_WhenExceptionOccurs_ReturnsBadRequest()
    {
        // Arrange
        var accountNo = "ACC001";
        var period = new DateTime(2024, 1, 1);
        var expectedError = "Account not found";

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetStatementQuery>(), default))
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var result = await _controller.GetStatement(accountNo, period);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo(expectedError));
    }

}