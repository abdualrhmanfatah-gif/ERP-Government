namespace ERP_Government.Domain.Assets.Constants;

public abstract class AssetAcquisitionTypes
{
    public const string Purchase = nameof(Purchase);
    public const string Grant = nameof(Grant);
    public const string Transfer = nameof(Transfer);
    public const string Donation = nameof(Donation);
    public const string Inherited = nameof(Inherited);

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.Ordinal)
        {
            Purchase, Grant, Transfer, Donation, Inherited
        };

    public static bool IsValid(string? value) =>
        value is not null && All.Contains(value);
}
