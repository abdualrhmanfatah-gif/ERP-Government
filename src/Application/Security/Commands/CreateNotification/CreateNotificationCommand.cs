using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;
using FluentValidation;

namespace ERP_Government.Application.Security.Commands.CreateNotification;

[Authorize(Policy = PermissionCodes.NotificationsCreate)]
public class CreateNotificationCommand : IRequest<Result<long>>
{
    public int UserId { get; init; }
    public string NotificationType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Priority { get; init; } = "Normal";
    public string? DocumentType { get; init; }
    public int? DocumentId { get; init; }
}

public class CreateNotificationCommandHandler(
    INotificationService notificationService) : IRequestHandler<CreateNotificationCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var id = await notificationService.CreateAsync(new CreateNotificationRequest
        {
            UserId = request.UserId,
            NotificationType = request.NotificationType,
            Title = request.Title,
            Message = request.Message,
            Priority = request.Priority,
            DocumentType = request.DocumentType,
            DocumentId = request.DocumentId
        }, cancellationToken);

        return Result<long>.Success(id);
    }
}

public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters.");

        RuleFor(x => x.NotificationType)
            .NotEmpty().WithMessage("Notification type is required.")
            .Must(v => Enum.TryParse<Domain.Security.Enums.NotificationType>(v, out _))
            .WithMessage("Invalid notification type.");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(v => Enum.TryParse<Domain.Security.Enums.NotificationPriority>(v, out _))
            .WithMessage("Invalid priority.");

        RuleFor(x => x.DocumentType)
            .MaximumLength(50).WithMessage("Document type must not exceed 50 characters.");
    }
}
