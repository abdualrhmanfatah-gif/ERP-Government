namespace ERP_Government.Infrastructure.Services;

public static class ArabicNumberToWords
{
    private static readonly string[] Units =
        ["صفر", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة"];

    private static readonly string[] Teens =
        ["عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر",
         "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر"];

    private static readonly string[] Tens =
        ["", "", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون"];

    private static readonly string[] Hundreds =
        ["", "مئة", "مئتان", "ثلاثمئة", "أربعمئة", "خمسمئة", "ستمئة", "سبعمئة", "ثمانمئة", "تسعمئة"];

    public static string Convert(decimal amount)
    {
        if (amount == 0) return "صفر";

        var integerPart = (long)Math.Abs(amount);
        var fractionPart = (int)(Math.Abs(amount) * 100) % 100;

        var result = ConvertInteger(integerPart);

        if (fractionPart > 0)
        {
            result += $" و{fractionPart}/100";
        }

        return result;
    }

    private static string ConvertInteger(long number)
    {
        if (number == 0) return "صفر";

        var parts = new List<string>();

        if (number >= 1_000_000_000)
        {
            var billions = number / 1_000_000_000;
            number %= 1_000_000_000;
            parts.Add(billions switch
            {
                1 => "مليار",
                2 => "ملياران",
                <= 10 => $"{ConvertBelow1000(billions)} ملايين",
                _ => $"{ConvertBelow1000(billions)} مليار"
            });
        }

        if (number >= 1_000_000)
        {
            var millions = number / 1_000_000;
            number %= 1_000_000;
            parts.Add(millions switch
            {
                1 => "مليون",
                2 => "مليونان",
                <= 10 => $"{ConvertBelow1000(millions)} ملايين",
                _ => $"{ConvertBelow1000(millions)} مليون"
            });
        }

        if (number >= 1000)
        {
            var thousands = number / 1000;
            number %= 1000;
            parts.Add(thousands switch
            {
                1 => "ألف",
                2 => "ألفان",
                <= 10 => $"{ConvertBelow1000(thousands)} آلاف",
                _ => $"{ConvertBelow1000(thousands)} ألف"
            });
        }

        if (number > 0)
        {
            parts.Add(ConvertBelow1000(number));
        }

        return string.Join(" و", parts);
    }

    private static string ConvertBelow1000(long number)
    {
        var parts = new List<string>();

        if (number >= 100)
        {
            var hundreds = number / 100;
            number %= 100;
            parts.Add(Hundreds[hundreds]);
        }

        if (number >= 20)
        {
            var tens = number / 10;
            var units = number % 10;
            parts.Add(units > 0 ? $"{Units[units]} و{Tens[tens]}" : Tens[tens]);
        }
        else if (number >= 10)
        {
            parts.Add(Teens[number - 10]);
        }
        else if (number > 0)
        {
            parts.Add(Units[number]);
        }

        return string.Join(" و", parts);
    }
}
