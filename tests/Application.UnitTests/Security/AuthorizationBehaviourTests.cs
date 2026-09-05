using ERP_Government.Application.Common.Behaviours;
using ERP_Government.Application.Common.Exceptions;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Security;

[TestFixture]
public class AuthorizationBehaviourTests
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

        _requestContextMock.Setup(r => r.Endpoint).Returns("/api/test");
        _requestContextMock.Setup(r => r.Method).Returns("GET");
        _requestContextMock.Setup(r => r.IpAddress).Returns("127.0.0.1");

        _dbContextMock.Setup(db => db.SecurityAuditLogs).Returns(new Mock<Microsoft.EntityFrameworkCore.DbSet<ERP_Government.Domain.Security.Entities.SecurityAuditLog>>().Object);
        _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _nextMock = ct => Task.FromResult("OK");
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
    public async Task Handle_NoAuthorizeAttribute_ShouldProceedToNext()
    {
        // Arrange
        var behaviour = CreateBehaviour<PlainRequest>();

        // Act
        var result = await behaviour.Handle(new PlainRequest(), _nextMock, CancellationToken.None);

        // Assert
        result.ShouldBe("OK");
    }

    [Test]
    public async Task Handle_AuthorizeAttribute_NoUserId_ShouldThrowUnauthorized()
    {
        // Arrange
        var behaviour = CreateBehaviour<AuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns((int?)null);

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(
            () => behaviour.Handle(new AuthorizedRequest(), _nextMock, CancellationToken.None));
    }

    [Test]
    public async Task Handle_AuthorizeAttribute_WithUserId_ShouldProceedToNext()
    {
        // Arrange
        var behaviour = CreateBehaviour<AuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns(123);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "User" });
        _identityServiceMock.Setup(i => i.AuthorizeAsync(123, It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await behaviour.Handle(new AuthorizedRequest(), _nextMock, CancellationToken.None);

        // Assert
        result.ShouldBe("OK");
    }

    [Test]
    public async Task Handle_AuthorizeAttribute_SystemAdmin_ShouldBypassAllChecks()
    {
        // Arrange
        var behaviour = CreateBehaviour<AuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns(1);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "SystemAdmin" });

        // Act
        var result = await behaviour.Handle(new AuthorizedRequest(), _nextMock, CancellationToken.None);

        // Assert
        result.ShouldBe("OK");
        _identityServiceMock.Verify(i => i.AuthorizeAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Handle_AuthorizeAttribute_MissingRole_ShouldThrowForbidden()
    {
        // Arrange
        var behaviour = CreateBehaviour<RoleAuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns(123);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "User" });

        // Act & Assert
        await Should.ThrowAsync<ForbiddenAccessException>(
            () => behaviour.Handle(new RoleAuthorizedRequest(), _nextMock, CancellationToken.None));
    }

    [Test]
    public async Task Handle_AuthorizeAttribute_HasRequiredRole_ShouldProceedToNext()
    {
        // Arrange
        var behaviour = CreateBehaviour<RoleAuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns(123);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "Admin" });

        // Act
        var result = await behaviour.Handle(new RoleAuthorizedRequest(), _nextMock, CancellationToken.None);

        // Assert
        result.ShouldBe("OK");
    }

    [Test]
    public async Task Handle_AuthorizeAttribute_PolicyDenied_ShouldThrowForbidden()
    {
        // Arrange
        var behaviour = CreateBehaviour<PolicyAuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns(123);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "User" });
        _identityServiceMock.Setup(i => i.AuthorizeAsync(123, "TestPolicy"))
            .ReturnsAsync(false);

        // Act & Assert
        await Should.ThrowAsync<ForbiddenAccessException>(
            () => behaviour.Handle(new PolicyAuthorizedRequest(), _nextMock, CancellationToken.None));
    }

    [Test]
    public async Task Handle_AuthorizeAttribute_PolicyAuthorized_ShouldProceedToNext()
    {
        // Arrange
        var behaviour = CreateBehaviour<PolicyAuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns(123);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "User" });
        _identityServiceMock.Setup(i => i.AuthorizeAsync(123, "TestPolicy"))
            .ReturnsAsync(true);

        // Act
        var result = await behaviour.Handle(new PolicyAuthorizedRequest(), _nextMock, CancellationToken.None);

        // Assert
        result.ShouldBe("OK");
    }

    [Test]
    public async Task Handle_AuthorizeAttribute_ServiceError_ShouldThrowForbidden()
    {
        // Arrange - fail closed behavior
        var behaviour = CreateBehaviour<PolicyAuthorizedRequest>();
        _userMock.Setup(u => u.Id).Returns(123);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "User" });
        _identityServiceMock.Setup(i => i.AuthorizeAsync(123, "TestPolicy"))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert - should throw ForbiddenAccessException (fail closed)
        await Should.ThrowAsync<ForbiddenAccessException>(
            () => behaviour.Handle(new PolicyAuthorizedRequest(), _nextMock, CancellationToken.None));
    }

    #region Test Helpers

    public class PlainRequest : IRequest<string> { }

    [Authorize]
    public class AuthorizedRequest : IRequest<string> { }

    [Authorize(Roles = "Admin")]
    public class RoleAuthorizedRequest : IRequest<string> { }

    [Authorize(Policy = "TestPolicy")]
    public class PolicyAuthorizedRequest : IRequest<string> { }

    #endregion
}
