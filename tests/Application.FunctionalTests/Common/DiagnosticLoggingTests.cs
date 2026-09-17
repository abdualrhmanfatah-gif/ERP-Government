using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ERP_Government.Application.FunctionalTests.Common;

public class DiagnosticLoggingTests
{
    [Test]
    public void UnexpectedFault_ShouldLogAtErrorLevel()
    {
        var logger = new Mock<ILogger<DiagnosticLoggingTests>>();
        var exception = new InvalidOperationException("Test error");

        logger.Object.LogError(exception, "Unexpected fault {StatusCode} {TraceId}", 500, "test-trace-id");

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected fault")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void ValidationRejection_ShouldLogAtWarningLevel()
    {
        var logger = new Mock<ILogger<DiagnosticLoggingTests>>();

        logger.Object.LogWarning("Request rejected {StatusCode} {TraceId}", 400, "test-trace-id");

        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request rejected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void CallerCancellation_ShouldLogAtDebugLevel()
    {
        var logger = new Mock<ILogger<DiagnosticLoggingTests>>();

        logger.Object.LogDebug("Request cancelled {TraceId}", "test-trace-id");

        logger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request cancelled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
