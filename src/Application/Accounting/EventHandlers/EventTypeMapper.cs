using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.EventHandlers;

/// <summary>
/// Maps domain event class names to the structured EventType enum.
/// Falls back to Other for events without a dedicated enum value.
/// </summary>
public static class EventTypeMapper
{
    public static EventType MapFrom(string eventName) =>
        Enum.TryParse<EventType>(eventName, ignoreCase: false, out var eventType)
            ? eventType
            : EventType.Other;
}
