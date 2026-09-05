using ERP_Government.Application.Accounting.Commands.AccountGroups.UpdateAccountGroup;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting.AccountGroups;

public class UpdateAccountGroupCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UpdateAccountGroupCommandHandler _handler;

    public UpdateAccountGroupCommandTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateAccountGroupCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_EntityExists_ShouldReturnSuccess()
    {
        var entity = new AccountGroup
        {
            Id = 1,
            Code = "AST",
            Name = "Assets",
            Type = AccountGroupType.Asset,
            NormalBalance = NormalBalanceType.Debit
        };

        var mockSet = new Mock<DbSet<AccountGroup>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(entity);

        _contextMock.Setup(x => x.AccountGroups).Returns(mockSet.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateAccountGroupCommand
        {
            Id = 1,
            Name = "Assets Updated",
            Type = AccountGroupType.Asset,
            NormalBalance = NormalBalanceType.Debit,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        entity.Name.ShouldBe("Assets Updated");
    }

    [Test]
    public async Task Handle_EntityNotFound_ShouldReturnFailure()
    {
        var mockSet = new Mock<DbSet<AccountGroup>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((AccountGroup?)null);

        _contextMock.Setup(x => x.AccountGroups).Returns(mockSet.Object);

        var command = new UpdateAccountGroupCommand
        {
            Id = 999,
            Name = "Assets",
            Type = AccountGroupType.Asset,
            NormalBalance = NormalBalanceType.Debit,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }
}
