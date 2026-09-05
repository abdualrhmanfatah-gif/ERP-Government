namespace ERP_Government.Application.Payments.Common.DTOs;

public record AvailabilityBreakdownDto(
    decimal NetAppropriated,
    decimal Encumbered,
    decimal Available,
    decimal Requested,
    decimal Shortfall);
