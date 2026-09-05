using ERP_Government.Application.Banking.Commands.BankStatements;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Banking.Entities;
using ERP_Government.Domain.Banking.Enums;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Banking;

[TestFixture]
public class ReconcileBankStatementCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private ReconcileBankStatementCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new ReconcileBankStatementCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_StatementNotFound_ReturnsFailure()
    {
        var command = new ReconcileBankStatementCommand { Id = 999 };

        _contextMock.Setup(x => x.BankStatements.FindAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BankStatement?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Bank statement not found.");
    }

    [Test]
    public async Task Handle_StatementIsDraft_ReturnsFailure()
    {
        var entity = new BankStatement { Id = 1, Status = BankStatementStatus.Draft };
        var command = new ReconcileBankStatementCommand { Id = 1 };

        _contextMock.Setup(x => x.BankStatements.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Only imported statements can be reconciled.");
    }

    [Test]
    public async Task Handle_StatementIsImported_TransitionsToReconciled()
    {
        var entity = new BankStatement { Id = 1, Status = BankStatementStatus.Imported };
        var command = new ReconcileBankStatementCommand { Id = 1 };

        _contextMock.Setup(x => x.BankStatements.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        entity.Status.ShouldBe(BankStatementStatus.Reconciled);
    }

    [Test]
    public async Task Handle_StatementAlreadyReconciled_ReturnsSuccessIdempotent()
    {
        var entity = new BankStatement { Id = 1, Status = BankStatementStatus.Reconciled };
        var command = new ReconcileBankStatementCommand { Id = 1 };

        _contextMock.Setup(x => x.BankStatements.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }
}

[TestFixture]
public class CancelBankStatementCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private CancelBankStatementCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CancelBankStatementCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_StatementIsDraft_TransitionsToCancelled()
    {
        var entity = new BankStatement { Id = 1, Status = BankStatementStatus.Draft };
        var command = new CancelBankStatementCommand { Id = 1 };

        _contextMock.Setup(x => x.BankStatements.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        entity.Status.ShouldBe(BankStatementStatus.Cancelled);
    }

    [Test]
    public async Task Handle_StatementIsImported_ReturnsFailure()
    {
        var entity = new BankStatement { Id = 1, Status = BankStatementStatus.Imported };
        var command = new CancelBankStatementCommand { Id = 1 };

        _contextMock.Setup(x => x.BankStatements.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Only draft statements can be cancelled.");
    }

    [Test]
    public async Task Handle_StatementAlreadyCancelled_ReturnsSuccessIdempotent()
    {
        var entity = new BankStatement { Id = 1, Status = BankStatementStatus.Cancelled };
        var command = new CancelBankStatementCommand { Id = 1 };

        _contextMock.Setup(x => x.BankStatements.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }
}
