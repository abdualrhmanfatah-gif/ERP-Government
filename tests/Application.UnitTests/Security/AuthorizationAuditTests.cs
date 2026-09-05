using ERP_Government.Application.Common.Behaviours;
using ERP_Government.Application.Common.Exceptions;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Security;

[TestFixture]
public class AuthorizationAuditTests
{
    private Mock<IUser> _userMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;
    private Mock<IApplicationDbContext> _dbContextMock = null!;
    private Mock<IRequestContext> _requestContextMock = null!;
    private RequestHandlerDelegate<string> _nextMock = null!;

    [SetUp]
    public void SetUp()
    {
        _userMock = new Mock<IUser>();
        _identityServiceMock = new Mock<IIdentityService>();
        _dbContextMock = new Mock<IApplicationDbContext>();
        _requestContextMock = new Mock<IRequestContext>();
        _nextMock = ct => Task.FromResult("OK");

        _requestContextMock.Setup(r => r.Endpoint).Returns("/api/test");
        _requestContextMock.Setup(r => r.Method).Returns("GET");
        _requestContextMock.Setup(r => r.IpAddress).Returns("127.0.0.1");

        _dbContextMock.Setup(db => db.SecurityAuditLogs).Returns(new Mock<Microsoft.EntityFrameworkCore.DbSet<SecurityAuditLog>>().Object);
        _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private AuthorizationBehaviour<TRequest, string> CreateBehaviour<TRequest>()
        where TRequest : notnull, IRequest<string>
    {
        return new AuthorizationBehaviour<TRequest, string>(
            _userMock.Object,
            _identityServiceMock.Object,
            _dbContextMock.Object,
            _requestContextMock.Object,
            new Mock<ILogger<AuthorizationBehaviour<TRequest, string>>>().Object);
    }

    [Test]
    public async Task Handle_Unauthenticated_ShouldLogAuditEvent()
    {
        // Arrange
        var behaviour = CreateBehaviour<AuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns((int?)null);

        // Act
        await Should.ThrowAsync<UnauthorizedAccessException>(
            () => behaviour.Handle(new AuthorizedRequest(), _nextMock, CancellationToken.None));

        // Assert - audit log should be created
        _dbContextMock.Verify(db => db.SecurityAuditLogs.Add(It.Is<SecurityAuditLog>(
            log => log.EventCategory == "Authorization"
                && log.Action == "Unauthorized"
                && log.Success == false
                && log.FailureReason == "Unauthenticated"
                && log.EntityName == "GET /api/test"
        )), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_Forbidden_ShouldLogAuditEvent()
    {
        // Arrange
        var behaviour = CreateBehaviour<PolicyAuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns(123);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "User" });
        _identityServiceMock.Setup(i => i.AuthorizeAsync(123, "TestPolicy"))
            .ReturnsAsync(false);

        // Act
        await Should.ThrowAsync<ForbiddenAccessException>(
            () => behaviour.Handle(new PolicyAuthorizedRequest(), _nextMock, CancellationToken.None));

        // Assert - audit log should be created
        _dbContextMock.Verify(db => db.SecurityAuditLogs.Add(It.Is<SecurityAuditLog>(
            log => log.EventCategory == "Authorization"
                && log.Action == "Forbidden"
                && log.Success == false
                && log.FailureReason!.Contains("TestPolicy")
        )), Times.Once);
    }

    [Test]
    public async Task Handle_ServiceError_ShouldLogAuditEventWithServiceError()
    {
        // Arrange - fail closed
        var behaviour = CreateBehaviour<PolicyAuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns(123);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "User" });
        _identityServiceMock.Setup(i => i.AuthorizeAsync(123, "TestPolicy"))
            .ThrowsAsync(new Exception("Database down"));

        // Act
        await Should.ThrowAsync<ForbiddenAccessException>(
            () => behaviour.Handle(new PolicyAuthorizedRequest(), _nextMock, CancellationToken.None));

        // Assert - audit log should show service error
        _dbContextMock.Verify(db => db.SecurityAuditLogs.Add(It.Is<SecurityAuditLog>(
            log => log.EventCategory == "Authorization"
                && log.Action == "Forbidden"
                && log.Success == false
                && log.FailureReason!.Contains("Service error")
        )), Times.Once);
    }

    #region Test Helpers

    [Authorize]
    public class AuthorizedRequest : IRequest<string> { }

    [Authorize(Policy = "TestPolicy")]
    public class PolicyAuthorizedRequest : IRequest<string> { }

    #endregion
}
