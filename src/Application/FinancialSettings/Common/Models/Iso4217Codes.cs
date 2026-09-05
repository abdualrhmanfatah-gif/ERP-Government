namespace ERP_Government.Application.FinancialSettings.Common.Models;

/// <summary>
/// Built-in reference list of active ISO 4217 currency codes.
/// Used for validation when creating new currencies.
/// Source: ISO 4217 Maintenance Agency — active codes only.
/// </summary>
public static class Iso4217Codes
{
    private static readonly Dictionary<string, Iso4217Code> Codes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AED"] = new("AED", "UAE Dirham", 2),
        ["AFN"] = new("AFN", "Afghani", 2),
        ["ALL"] = new("ALL", "Lek", 2),
        ["AMD"] = new("AMD", "Armenian Dram", 2),
        ["ANG"] = new("ANG", "Netherlands Antillean Guilder", 2),
        ["AOA"] = new("AOA", "Kwanza", 2),
        ["ARS"] = new("ARS", "Argentine Peso", 2),
        ["AUD"] = new("AUD", "Australian Dollar", 2),
        ["AWG"] = new("AWG", "Aruban Florin", 2),
        ["AZN"] = new("AZN", "Azerbaijanian Manat", 2),
        ["BAM"] = new("BAM", "Convertible Mark", 2),
        ["BBD"] = new("BBD", "Barbados Dollar", 2),
        ["BDT"] = new("BDT", "Taka", 2),
        ["BGN"] = new("BGN", "Bulgarian Lev", 2),
        ["BHD"] = new("BHD", "Bahraini Dinar", 3),
        ["BIF"] = new("BIF", "Burundi Franc", 0),
        ["BMD"] = new("BMD", "Bermudian Dollar", 2),
        ["BND"] = new("BND", "Brunei Dollar", 2),
        ["BOB"] = new("BOB", "Boliviano", 2),
        ["BRL"] = new("BRL", "Brazilian Real", 2),
        ["BSD"] = new("BSD", "Bahamian Dollar", 2),
        ["BTN"] = new("BTN", "Ngultrum", 2),
        ["BWP"] = new("BWP", "Pula", 2),
        ["BYN"] = new("BYN", "Belarusian Ruble", 2),
        ["BZD"] = new("BZD", "Belize Dollar", 2),
        ["CAD"] = new("CAD", "Canadian Dollar", 2),
        ["CDF"] = new("CDF", "Congolese Franc", 2),
        ["CHF"] = new("CHF", "Swiss Franc", 2),
        ["CLP"] = new("CLP", "Chilean Peso", 0),
        ["CNY"] = new("CNY", "Yuan Renminbi", 2),
        ["COP"] = new("COP", "Colombian Peso", 2),
        ["CRC"] = new("CRC", "Costa Rican Colon", 2),
        ["CUP"] = new("CUP", "Cuban Peso", 2),
        ["CVE"] = new("CVE", "Cape Verde Escudo", 2),
        ["CZK"] = new("CZK", "Czech Koruna", 2),
        ["DJF"] = new("DJF", "Djibouti Franc", 0),
        ["DKK"] = new("DKK", "Danish Krone", 2),
        ["DOP"] = new("DOP", "Dominican Peso", 2),
        ["DZD"] = new("DZD", "Algerian Dinar", 2),
        ["EGP"] = new("EGP", "Egyptian Pound", 2),
        ["ERN"] = new("ERN", "Nakfa", 2),
        ["ETB"] = new("ETB", "Ethiopian Birr", 2),
        ["EUR"] = new("EUR", "Euro", 2),
        ["FJD"] = new("FJD", "Fiji Dollar", 2),
        ["FKP"] = new("FKP", "Falkland Islands Pound", 2),
        ["GBP"] = new("GBP", "Pound Sterling", 2),
        ["GEL"] = new("GEL", "Lari", 2),
        ["GHS"] = new("GHS", "Ghana Cedi", 2),
        ["GIP"] = new("GIP", "Gibraltar Pound", 2),
        ["GMD"] = new("GMD", "Dalasi", 2),
        ["GNF"] = new("GNF", "Guinea Franc", 0),
        ["GTQ"] = new("GTQ", "Quetzal", 2),
        ["GYD"] = new("GYD", "Guyana Dollar", 2),
        ["HKD"] = new("HKD", "Hong Kong Dollar", 2),
        ["HNL"] = new("HNL", "Lempira", 2),
        ["HRK"] = new("HRK", "Croatian Kuna", 2),
        ["HTG"] = new("HTG", "Haiti Gourde", 2),
        ["HUF"] = new("HUF", "Forint", 2),
        ["IDR"] = new("IDR", "Rupiah", 2),
        ["ILS"] = new("ILS", "New Israeli Sheqel", 2),
        ["INR"] = new("INR", "Indian Rupee", 2),
        ["IQD"] = new("IQD", "Iraqi Dinar", 3),
        ["IRR"] = new("IRR", "Iranian Rial", 2),
        ["ISK"] = new("ISK", "Iceland Krona", 0),
        ["JMD"] = new("JMD", "Jamaican Dollar", 2),
        ["JOD"] = new("JOD", "Jordanian Dinar", 3),
        ["JPY"] = new("JPY", "Yen", 0),
        ["KES"] = new("KES", "Kenyan Shilling", 2),
        ["KGS"] = new("KGS", "Som", 2),
        ["KHR"] = new("KHR", "Riel", 2),
        ["KMF"] = new("KMF", "Comoro Franc", 0),
        ["KPW"] = new("KPW", "North Korean Won", 2),
        ["KRW"] = new("KRW", "Won", 0),
        ["KWD"] = new("KWD", "Kuwaiti Dinar", 3),
        ["KYD"] = new("KYD", "Cayman Islands Dollar", 2),
        ["KZT"] = new("KZT", "Tenge", 2),
        ["LAK"] = new("LAK", "Kip", 2),
        ["LBP"] = new("LBP", "Lebanese Pound", 2),
        ["LKR"] = new("LKR", "Sri Lanka Rupee", 2),
        ["LRD"] = new("LRD", "Liberian Dollar", 2),
        ["LSL"] = new("LSL", "Loti", 2),
        ["LYD"] = new("LYD", "Libyan Dinar", 3),
        ["MAD"] = new("MAD", "Moroccan Dirham", 2),
        ["MDL"] = new("MDL", "Moldovan Leu", 2),
        ["MGA"] = new("MGA", "Malagasy Ariary", 2),
        ["MKD"] = new("MKD", "Denar", 2),
        ["MMK"] = new("MMK", "Kyat", 2),
        ["MNT"] = new("MNT", "Tugrik", 2),
        ["MOP"] = new("MOP", "Pataca", 2),
        ["MRU"] = new("MRU", "Ouguiya", 2),
        ["MUR"] = new("MUR", "Mauritius Rupee", 2),
        ["MVR"] = new("MVR", "Rufiyaa", 2),
        ["MWK"] = new("MWK", "Kwacha", 2),
        ["MXN"] = new("MXN", "Mexican Peso", 2),
        ["MYR"] = new("MYR", "Malaysian Ringgit", 2),
        ["MZN"] = new("MZN", "Mozambique Metical", 2),
        ["NAD"] = new("NAD", "Namibia Dollar", 2),
        ["NGN"] = new("NGN", "Naira", 2),
        ["NIO"] = new("NIO", "Cordoba Oro", 2),
        ["NOK"] = new("NOK", "Norwegian Krone", 2),
        ["NPR"] = new("NPR", "Nepalese Rupee", 2),
        ["NZD"] = new("NZD", "New Zealand Dollar", 2),
        ["OMR"] = new("OMR", "Rial Omani", 3),
        ["PAB"] = new("PAB", "Balboa", 2),
        ["PEN"] = new("PEN", "Sol", 2),
        ["PGK"] = new("PGK", "Kina", 2),
        ["PHP"] = new("PHP", "Philippine Peso", 2),
        ["PKR"] = new("PKR", "Pakistan Rupee", 2),
        ["PLN"] = new("PLN", "Zloty", 2),
        ["PYG"] = new("PYG", "Guarani", 0),
        ["QAR"] = new("QAR", "Qatari Rial", 2),
        ["RON"] = new("RON", "Romanian Leu", 2),
        ["RSD"] = new("RSD", "Serbian Dinar", 2),
        ["RUB"] = new("RUB", "Russian Ruble", 2),
        ["RWF"] = new("RWF", "Rwanda Franc", 0),
        ["SAR"] = new("SAR", "Saudi Riyal", 2),
        ["SBD"] = new("SBD", "Solomon Islands Dollar", 2),
        ["SCR"] = new("SCR", "Seychelles Rupee", 2),
        ["SDG"] = new("SDG", "Sudanese Pound", 2),
        ["SEK"] = new("SEK", "Swedish Krona", 2),
        ["SGD"] = new("SGD", "Singapore Dollar", 2),
        ["SHP"] = new("SHP", "Saint Helena Pound", 2),
        ["SLE"] = new("SLE", "Sierra Leonean Leone", 2),
        ["SOS"] = new("SOS", "Somali Shilling", 2),
        ["SRD"] = new("SRD", "Surinam Dollar", 2),
        ["SSP"] = new("SSP", "South Sudanese Pound", 2),
        ["STN"] = new("STN", "Dobra", 2),
        ["SYP"] = new("SYP", "Syrian Pound", 2),
        ["SZL"] = new("SZL", "Lilangeni", 2),
        ["THB"] = new("THB", "Baht", 2),
        ["TJS"] = new("TJS", "Somoni", 2),
        ["TMT"] = new("TMT", "Turkmenistan New Manat", 2),
        ["TND"] = new("TND", "Tunisian Dinar", 3),
        ["TOP"] = new("TOP", "Pa'anga", 2),
        ["TRY"] = new("TRY", "Turkish Lira", 2),
        ["TTD"] = new("TTD", "Trinidad and Tobago Dollar", 2),
        ["TWD"] = new("TWD", "New Taiwan Dollar", 2),
        ["TZS"] = new("TZS", "Tanzanian Shilling", 2),
        ["UAH"] = new("UAH", "Hryvnia", 2),
        ["UGX"] = new("UGX", "Uganda Shilling", 0),
        ["USD"] = new("USD", "US Dollar", 2),
        ["UYU"] = new("UYU", "Peso Uruguayo", 2),
        ["UZS"] = new("UZS", "Uzbekistan Sum", 2),
        ["VED"] = new("VED", "Digital Bolivar", 2),
        ["VES"] = new("VES", "Bolivar Soberano", 2),
        ["VND"] = new("VND", "Dong", 0),
        ["VUV"] = new("VUV", "Vatu", 0),
        ["WST"] = new("WST", "Tala", 2),
        ["XAF"] = new("XAF", "CFA Franc BEAC", 0),
        ["XCD"] = new("XCD", "East Caribbean Dollar", 2),
        ["XOF"] = new("XOF", "CFA Franc BCEAO", 0),
        ["XPF"] = new("XPF", "CFP Franc", 0),
        ["YER"] = new("YER", "Yemeni Rial", 2),
        ["ZAR"] = new("ZAR", "Rand", 2),
        ["ZMW"] = new("ZMW", "Zambian Kwacha", 2),
        ["ZWL"] = new("ZWL", "Zimbabwe Dollar", 2),
    };

    /// <summary>
    /// Check if a currency code is valid ISO 4217.
    /// </summary>
    public static bool IsValid(string code) => Codes.ContainsKey(code);

    /// <summary>
    /// Get the ISO 4217 entry for a code, or null if invalid.
    /// </summary>
    public static Iso4217Code? GetByCode(string code) =>
        Codes.TryGetValue(code, out var entry) ? entry : null;

    /// <summary>
    /// Get all active ISO 4217 codes, optionally filtered by name or code.
    /// </summary>
    public static IReadOnlyList<Iso4217Code> GetAll(string? filter = null)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return Codes.Values.ToList();

        var lower = filter.ToLowerInvariant();
        return Codes.Values
            .Where(c => c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
