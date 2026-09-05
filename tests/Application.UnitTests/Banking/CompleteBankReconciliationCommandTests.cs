using ERP_Government.Application.Banking.Commands.BankReconciliations;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Banking.Entities;
using ERP_Government.Domain.Banking.Enums;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Banking;

[TestFixture]
public class CompleteBankReconciliationCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private CompleteBankReconciliationCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CompleteBankReconciliationCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ReconciliationNotFound_ReturnsFailure()
    {
        var command = new CompleteBankReconciliationCommand { Id = 999 };

        _contextMock.Setup(x => x.BankReconciliations.FindAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BankReconciliation?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Bank reconciliation not found.");
    }

    [Test]
    public async Task Handle_StatusIsCompleted_ReturnsSuccessIdempotent()
    {
        var entity = new BankReconciliation { Id = 1, Status = ReconciliationStatus.Completed };
        var command = new CompleteBankReconciliationCommand { Id = 1 };

        _contextMock.Setup(x => x.BankReconciliations.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_StatusIsApproved_ReturnsFailure()
    {
        var entity = new BankReconciliation { Id = 1, Status = ReconciliationStatus.Approved };
        var command = new CompleteBankReconciliationCommand { Id = 1 };

        _contextMock.Setup(x => x.BankReconciliations.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Only draft reconciliations can be completed.");
    }
}

[TestFixture]
public class RejectBankReconciliationCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private RejectBankReconciliationCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new RejectBankReconciliationCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_StatusIsCompleted_TransitionsToRejected()
    {
        var entity = new BankReconciliation { Id = 1, Status = ReconciliationStatus.Completed };
        var command = new RejectBankReconciliationCommand { Id = 1, Reason = "Test rejection" };

        _contextMock.Setup(x => x.BankReconciliations.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        entity.Status.ShouldBe(ReconciliationStatus.Rejected);
        entity.RejectionReason.ShouldBe("Test rejection");
    }

    [Test]
    public async Task Handle_StatusIsDraft_ReturnsFailure()
    {
        var entity = new BankReconciliation { Id = 1, Status = ReconciliationStatus.Draft };
        var command = new RejectBankReconciliationCommand { Id = 1, Reason = "Test" };

        _contextMock.Setup(x => x.BankReconciliations.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Only completed reconciliations can be rejected.");
    }

    [Test]
    public async Task Handle_AlreadyRejected_ReturnsSuccessIdempotent()
    {
        var entity = new BankReconciliation { Id = 1, Status = ReconciliationStatus.Rejected };
        var command = new RejectBankReconciliationCommand { Id = 1, Reason = "Test" };

        _contextMock.Setup(x => x.BankReconciliations.FindAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }
}
