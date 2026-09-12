using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;


namespace ERP_Government.Infrastructure.Data.Seeds;


// طريقة الدمج:
// 1) احذف/استبدل AccountGroupSeedData وAccountSeedData القديمين بهذا الملف.
// 2) في مهيئ قاعدة البيانات استدعِ:
//    await AccountingChartSeeder.SeedAsync(dbContext, cancellationToken);


/// <summary>
/// الدليل المحاسبي التشغيلي لصندوق التقاعد الأمني في الجمهورية اليمنية.
///
/// ملاحظات مهمة:
/// - ParentId وAccountGroupId يحلهما AccountingChartSeeder من الأكواد الفعلية في قاعدة البيانات.
/// - الحسابات غير الملائمة أو غير المطبقة حاليًا لا تُزرع في الدليل التشغيلي.
/// - حسابات 122 تخص المشاريع قيد التنفيذ فقط، وليست أصولًا مكتملة جاهزة للاستخدام.
/// </summary>
public static class AccountSeedData
{
    internal static List<Account> GetBlueprints()
    {
        ValidateDefinitions(Definitions);


        return Definitions
            .Select(definition => new Account
            {
                Code = definition.Code,
                Name = definition.Name,
                AccountGroupId = definition.AccountGroupId,
                ParentId = null,
                Level = (byte)definition.Code.Length,
                NormalBalance = definition.NormalBalance,
                IsPostable = definition.IsPostable
            })
            .ToList();
    }


    private static readonly AccountDefinition[] Definitions =
    [
        // ═══════════════════════════════════════════════════════════════
        // الموجودات
        // ═══════════════════════════════════════════════════════════════


        // 11: أصول ثابتة مكتملة وجاهزة للاستخدام
        // الأرقام التحليلية أدناه مقترحة للصندوق ويجب مطابقتها مع النسخة
        // المعتمدة لدى وزارة المالية قبل الانتقال إلى الإنتاج.
        D("111", "الأراضي", 111, null, NormalBalanceType.Debit, false),
        D("1111", "أراضي الإدارة", 111, "111", NormalBalanceType.Debit),
        D("1112", "أراضٍ استثمارية", 111, "111", NormalBalanceType.Debit),
        D("112", "المباني والإنشاءات", 112, null, NormalBalanceType.Debit, false),
        D("1121", "مباني الإدارة", 112, "112", NormalBalanceType.Debit),
        D("1122", "مبانٍ استثمارية", 112, "112", NormalBalanceType.Debit),
        D("113", "الآلات والتجهيزات والمعدات", 113, null, NormalBalanceType.Debit, false),
        D("1131", "آلات وتجهيزات ومعدات", 113, "113", NormalBalanceType.Debit),
        D("114", "السيارات ووسائل النقل", 114, null, NormalBalanceType.Debit, false),
        D("1141", "سيارات ووسائل نقل", 114, "114", NormalBalanceType.Debit),
        D("115", "الأثاث ومعدات المكاتب", 115, null, NormalBalanceType.Debit, false),
        D("1151", "الأثاث والمفروشات", 115, "115", NormalBalanceType.Debit),
        D("1158", "أجهزة الكمبيوتر وملحقاتها", 115, "115", NormalBalanceType.Debit),
        D("1159", "معدات مكاتب أخرى", 115, "115", NormalBalanceType.Debit),


        // 122: مشاريع قيد التنفيذ فقط
        D("122", "المشاريع الاستثمارية قيد التنفيذ", 18, null, NormalBalanceType.Debit, false),
        D("1222", "مشاريع المباني والإنشاءات قيد التنفيذ", 18, "122", NormalBalanceType.Debit, false),
        D("12221", "مشروع مبنى الإدارة قيد التنفيذ", 18, "1222", NormalBalanceType.Debit),
        D("12222", "مشروع مبنى استثماري قيد التنفيذ", 18, "1222", NormalBalanceType.Debit),
        D("1223", "مشاريع الآلات والتجهيزات والمعدات قيد التنفيذ", 18, "122", NormalBalanceType.Debit, false),
        D("12233", "مشاريع آلات وتجهيزات أخرى قيد التنفيذ", 18, "1223", NormalBalanceType.Debit),
        D("1225", "مشاريع الأثاث ومعدات المكاتب قيد التنفيذ", 18, "122", NormalBalanceType.Debit, false),
        D("12258", "مشروع تجهيز أجهزة كمبيوتر قيد التنفيذ", 18, "1225", NormalBalanceType.Debit),
        D("12259", "مشروع أثاث ومعدات مكاتب أخرى قيد التنفيذ", 18, "1225", NormalBalanceType.Debit),


        // 13: التوظيفات والاستثمارات
        D("133", "حصص المشاركة", 19, null, NormalBalanceType.Debit, false),
        D("1331", "مؤسسات وشركات محلية", 19, "133", NormalBalanceType.Debit),
        D("135", "استثمارات مالية بسندات", 20, null, NormalBalanceType.Debit, false),
        D("1351", "استثمارات مالية بسندات حكومية", 20, "135", NormalBalanceType.Debit),


        // 18: الأموال الجاهزة
        D("182", "نقد لدى البنوك", 21, null, NormalBalanceType.Debit, false),
        D("1821", "حسابات جارية محلية", 21, "182", NormalBalanceType.Debit),


        // ═══════════════════════════════════════════════════════════════
        // الموارد الرأسمالية والالتزامات
        // ═══════════════════════════════════════════════════════════════


        D("222", "الفائض المرحل", 22, null, NormalBalanceType.Credit, false),
        D("2221", "فائض الدورة الحالية", 22, "222", NormalBalanceType.Credit),


        D("231", "مخصص الاهتلاكات", 23, null, NormalBalanceType.Credit, false),
        D("2311", "مخصص اهتلاك المباني والإنشاءات", 23, "231", NormalBalanceType.Credit),
        D("2312", "مخصص اهتلاك الآلات والتجهيزات والمعدات", 23, "231", NormalBalanceType.Credit),
        D("2313", "مخصص اهتلاك السيارات ووسائل النقل", 23, "231", NormalBalanceType.Credit),
        D("2314", "مخصص اهتلاك الأثاث والمفروشات", 23, "231", NormalBalanceType.Credit),
        D("2315", "مخصص حقوق العاملين الأخرى", 23, "231", NormalBalanceType.Credit),


        // العجز مدين، والفائض دائن
        D("281", "حساب توزيع عجز النشاط الجاري", 24, null, NormalBalanceType.Debit, false),
        D("2812", "عجز النشاط الجاري", 24, "281", NormalBalanceType.Debit),
        D("282", "حساب توزيع فائض النشاط الجاري", 25, null, NormalBalanceType.Credit, false),
        D("2822", "فائض النشاط الجاري", 25, "282", NormalBalanceType.Credit),


        // 25: الدائنون
        D("251", "الموردون", 44, null, NormalBalanceType.Credit, false),
        D("2511", "موردون محليون - قطاع عام", 44, "251", NormalBalanceType.Credit),
        D("2512", "موردون محليون - قطاع خاص", 44, "251", NormalBalanceType.Credit),
        D("2513", "موردون خارجيون", 44, "251", NormalBalanceType.Credit),


        D("253", "دائنون متنوعون", 46, null, NormalBalanceType.Credit, false),
        D("2531", "دائنون متنوعون محليون", 46, "253", NormalBalanceType.Credit),
        D("2533", "دائنون - ذمم موقوفة", 46, "253", NormalBalanceType.Credit),


        D("254", "ذمم دائنة مختلفة", 47, null, NormalBalanceType.Credit, false),
        D("2541", "مصلحة الضرائب - ضريبة غير مباشرة", 47, "254", NormalBalanceType.Credit),
        D("2542", "مصلحة الضرائب - ضريبة كسب العمل", 47, "254", NormalBalanceType.Credit),
        D("2543", "مصلحة الضرائب - ضريبة الدمغة", 47, "254", NormalBalanceType.Credit),
        D("2544", "صندوق الغرامات والجزاءات", 47, "254", NormalBalanceType.Credit),
        D("2545", "صندوق الضمان الاجتماعي", 47, "254", NormalBalanceType.Credit),
        D("2547", "مبالغ محجوزة من المعاشات لنفقة شرعية", 47, "254", NormalBalanceType.Credit),
        D("2548", "حجوزات رواتب موظفي الصندوق", 47, "254", NormalBalanceType.Credit),
        D("2549", "ذمم دائنة أخرى", 47, "254", NormalBalanceType.Credit),


        // التأمينات والتوقيفات اللازمة للعقود والمناقصات
        D("262", "التأمينات الدائنة", 53, null, NormalBalanceType.Credit, false),
        D("2621", "تأمينات للغير", 53, "262", NormalBalanceType.Credit),
        D("2622", "تأمينات المقاولين", 53, "262", NormalBalanceType.Credit),
        D("2623", "تأمينات المناقصات", 53, "262", NormalBalanceType.Credit),


        D("263", "التوقيفات", 54, null, NormalBalanceType.Credit, false),
        D("2631", "توقيفات للغير", 54, "263", NormalBalanceType.Credit),
        D("2632", "توقيفات المقاولين", 54, "263", NormalBalanceType.Credit),


        // 27: الحسابات الانتقالية الدائنة
        D("271", "إيرادات محصلة مقدمًا", 56, null, NormalBalanceType.Credit, false),
        D("2711", "إيرادات فوائد محصلة مقدمًا", 56, "271", NormalBalanceType.Credit),
        D("2712", "إيرادات إيجارات محصلة مقدمًا", 56, "271", NormalBalanceType.Credit),
        D("2713", "إيرادات أوراق مالية محصلة مقدمًا", 56, "271", NormalBalanceType.Credit),
        D("2715", "إيرادات إعانات محصلة مقدمًا", 56, "271", NormalBalanceType.Credit),


        D("272", "مصاريف جارية وتخصيصية مستحقة", 57, null, NormalBalanceType.Credit, false),
        D("2721", "رواتب وأجور محلية مستحقة", 57, "272", NormalBalanceType.Credit),
        D("2724", "صيانة وتصليحات مستحقة", 57, "272", NormalBalanceType.Credit),
        D("2725", "دعاية وإعلان مستحق", 57, "272", NormalBalanceType.Credit),
        D("2727", "إيجارات مستحقة", 57, "272", NormalBalanceType.Credit),
        D("2728", "إعانات ومساعدات مستحقة", 57, "272", NormalBalanceType.Credit),
        D("2729", "مصاريف أخرى مستحقة", 57, "272", NormalBalanceType.Credit),


        // ═══════════════════════════════════════════════════════════════
        // الاستخدامات الجارية
        // ═══════════════════════════════════════════════════════════════


        D("311", "المرتبات والأجور النقدية", 26, null, NormalBalanceType.Debit, false),
        D("3111", "مرتبات الموظفين الدائمين", 26, "311", NormalBalanceType.Debit),


        D("312", "البدلات والتعويضات", 27, null, NormalBalanceType.Debit, false),
        D("3124", "بدل حضور جلسات", 27, "312", NormalBalanceType.Debit),
        D("3125", "تعويض المسؤولية", 27, "312", NormalBalanceType.Debit),
        D("3126", "تعويض العمل الإضافي", 27, "312", NormalBalanceType.Debit),
        D("3129", "بدلات وتعويضات أخرى", 27, "312", NormalBalanceType.Debit),


        D("313", "المزايا العينية", 28, null, NormalBalanceType.Debit, false),
        D("3139", "مزايا عينية أخرى", 28, "313", NormalBalanceType.Debit),


        D("314", "المكافآت", 29, null, NormalBalanceType.Debit, false),
        D("3141", "المكافآت التشجيعية", 29, "314", NormalBalanceType.Debit),


        D("316", "تأمينات وتقاعد العاملين", 30, null, NormalBalanceType.Debit, false),
        D("3161", "تأمين الشيخوخة أو التقاعد", 30, "316", NormalBalanceType.Debit),


        D("321", "المستلزمات السلعية", 31, null, NormalBalanceType.Debit, false),
        D("3212", "الوقود والزيوت والقوى المحركة والمياه", 31, "321", NormalBalanceType.Debit, false),
        D("32122", "المواد البترولية", 31, "3212", NormalBalanceType.Debit),
        D("32123", "الكهرباء", 31, "3212", NormalBalanceType.Debit),
        D("32128", "الزيوت والشحوم", 31, "3212", NormalBalanceType.Debit),
        D("32129", "المياه", 31, "3212", NormalBalanceType.Debit),
        D("3213", "قطع التبديل واللوازم", 31, "321", NormalBalanceType.Debit, false),
        D("32131", "قطع التبديل للصيانة", 31, "3213", NormalBalanceType.Debit),
        D("3215", "القرطاسية والمطبوعات", 31, "321", NormalBalanceType.Debit, false),
        D("32151", "المنشورات والكتب الدورية", 31, "3215", NormalBalanceType.Debit),
        D("32152", "لوازم الكتابة والقرطاسية", 31, "3215", NormalBalanceType.Debit),
        D("32154", "لوازم التصوير", 31, "3215", NormalBalanceType.Debit),
        D("32156", "السجلات والدفاتر", 31, "3215", NormalBalanceType.Debit),


        D("322", "المستلزمات الخدمية", 32, null, NormalBalanceType.Debit, false),
        D("3221", "الصيانة والتصليحات", 32, "322", NormalBalanceType.Debit, false),
        D("32211", "صيانة المباني والطرق", 32, "3221", NormalBalanceType.Debit),
        D("32212", "صيانة الآلات والتجهيزات", 32, "3221", NormalBalanceType.Debit),
        D("32213", "صيانة السيارات ووسائل النقل", 32, "3221", NormalBalanceType.Debit),
        D("32215", "صيانة الأثاث والمفروشات", 32, "3221", NormalBalanceType.Debit),
        D("3223", "الأبحاث والتجارب", 32, "322", NormalBalanceType.Debit, false),
        D("32235", "الخدمات الاستشارية الفنية", 32, "3223", NormalBalanceType.Debit),
        D("3224", "النشر والإعلان والضيافة والاستقبال", 32, "322", NormalBalanceType.Debit, false),
        D("32241", "إعلانات منشورة", 32, "3224", NormalBalanceType.Debit),
        D("32242", "الدعاية الدورية أو الموسمية", 32, "3224", NormalBalanceType.Debit),
        D("32246", "خدمات الاستقبال والضيافة", 32, "3224", NormalBalanceType.Debit),
        D("3225", "التنقلات وبدلات السفر والمواصلات", 32, "322", NormalBalanceType.Debit, false),
        D("32251", "نقل مهمات", 32, "3225", NormalBalanceType.Debit),
        D("32252", "نقل وانتقالات عامة", 32, "3225", NormalBalanceType.Debit),
        D("32253", "بدلات السفر الداخلية", 32, "3225", NormalBalanceType.Debit),
        D("32255", "الاتصالات الهاتفية", 32, "3225", NormalBalanceType.Debit),
        D("3227", "خدمات الإدارات الحكومية والمؤسسات", 32, "322", NormalBalanceType.Debit, false),
        D("32271", "خدمات الحراسة والأمن", 32, "3227", NormalBalanceType.Debit),
        D("32277", "خدمات التفتيش ومراجعة الحسابات", 32, "3227", NormalBalanceType.Debit),
        D("3228", "الخدمات المتممة", 32, "322", NormalBalanceType.Debit, false),
        D("32283", "خدمات المكاتب الاستشارية", 32, "3228", NormalBalanceType.Debit),
        D("3229", "مستلزمات خدمية أخرى", 32, "322", NormalBalanceType.Debit, false),
        D("32299", "مستلزمات خدمية أخرى ومختلفة", 32, "3229", NormalBalanceType.Debit),


        D("351", "المصروفات الجارية التحويلية", 34, null, NormalBalanceType.Debit, false),
        D("3511", "الاهتلاك", 34, "351", NormalBalanceType.Debit, false),
        D("35111", "اهتلاك المباني والإنشاءات", 34, "3511", NormalBalanceType.Debit),
        D("35112", "اهتلاك الآلات والتجهيزات والمعدات", 34, "3511", NormalBalanceType.Debit),
        D("35113", "اهتلاك السيارات ووسائل النقل", 34, "3511", NormalBalanceType.Debit),
        D("35114", "اهتلاك الأثاث والمفروشات", 34, "3511", NormalBalanceType.Debit),
        D("3513", "الإيجارات", 34, "351", NormalBalanceType.Debit, false),
        D("35136", "إيجارات مبانٍ في الداخل", 34, "3513", NormalBalanceType.Debit),
        D("3514", "الفوائد والعمولات", 34, "351", NormalBalanceType.Debit, false),
        D("35141", "الفوائد والعمولات المحلية", 34, "3514", NormalBalanceType.Debit),


        D("352", "المصروفات المخصصة", 35, null, NormalBalanceType.Debit, false),
        D("3522", "الإعانات والمساعدات والزكاة", 35, "352", NormalBalanceType.Debit, false),
        D("35221", "الإعانات النقدية", 35, "3522", NormalBalanceType.Debit, false),
        D("352211", "معاشات التقاعد - أساسي وبدلات", 35, "35221", NormalBalanceType.Debit),
        D("352212", "غلاء المعيشة للمتقاعدين", 35, "35221", NormalBalanceType.Debit),
        D("352213", "فروقات تسويات معاشات المتقاعدين", 35, "35221", NormalBalanceType.Debit),
        D("352214", "مكافأة التقاعد", 35, "35221", NormalBalanceType.Debit),
        D("352215", "تجهيز وتكفين", 35, "35221", NormalBalanceType.Debit),
        D("352216", "مساعدات مرضية", 35, "35221", NormalBalanceType.Debit),
        D("3526", "أعباء المخصصات", 35, "352", NormalBalanceType.Debit, false),
        D("35263", "الديون المشكوك في تحصيلها", 35, "3526", NormalBalanceType.Debit),
        D("35269", "التعويضات والغرامات المختلفة", 35, "3526", NormalBalanceType.Debit, false),
        D("352692", "نفقات تأمينية", 35, "35269", NormalBalanceType.Debit),


        // ═══════════════════════════════════════════════════════════════
        // الإيرادات
        // ═══════════════════════════════════════════════════════════════


        D("414", "إيرادات قطاع الخدمات", 37, null, NormalBalanceType.Credit, false),
        D("4142", "إيرادات الاشتراكات", 37, "414", NormalBalanceType.Credit, false),
        D("41421", "إيرادات حصة الحكومة في التقاعد", 37, "4142", NormalBalanceType.Credit),
        D("41422", "إيرادات حصة المنتفعين في التقاعد", 37, "4142", NormalBalanceType.Credit),
        D("41423", "إيرادات دعم الحكومة لغلاء المعيشة", 37, "4142", NormalBalanceType.Credit),
        D("41424", "إيرادات دعم الحكومة للمعاشات الاستثنائية", 37, "4142", NormalBalanceType.Credit),
        D("41425", "إيرادات دعم الحكومة لتسويات معاشات المحالين", 37, "4142", NormalBalanceType.Credit),
        D("41426", "إيرادات ضم الخدمات", 37, "4142", NormalBalanceType.Credit),


        D("429", "إيرادات أخرى ومختلفة", 15, null, NormalBalanceType.Credit, false),
        D("4291", "إيرادات أخرى ومختلفة", 15, "429", NormalBalanceType.Credit),


        D("431", "إيرادات أوراق مالية محلية", 38, null, NormalBalanceType.Credit, false),
        D("4311", "إيرادات أوراق مالية محلية", 38, "431", NormalBalanceType.Credit),
        D("433", "إيرادات عوائد محلية", 39, null, NormalBalanceType.Credit, false),
        D("4331", "إيرادات عوائد محلية", 39, "433", NormalBalanceType.Credit),
        D("452", "الإيجارات الدائنة", 40, null, NormalBalanceType.Credit, false),
        D("4521", "إيراد إيجار مبانٍ في الداخل", 40, "452", NormalBalanceType.Credit)
    ];


    private static AccountDefinition D(
        string code,
        string name,
        int accountGroupId,
        string? parentCode,
        NormalBalanceType normalBalance,
        bool isPostable = true) =>
        new(code, name, accountGroupId, parentCode, normalBalance, isPostable);


    private static void ValidateDefinitions(IReadOnlyCollection<AccountDefinition> definitions)
    {
        var duplicateCodes = definitions
            .GroupBy(definition => definition.Code, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();


        if (duplicateCodes.Length > 0)
        {
            throw new InvalidOperationException(
                $"Duplicate account codes: {string.Join(", ", duplicateCodes)}");
        }


        var definitionsByCode = definitions.ToDictionary(
            definition => definition.Code,
            StringComparer.Ordinal);


        foreach (var definition in definitions)
        {
            if (definition.Code.Length is < 3 or > 7 ||
                !definition.Code.All(char.IsAsciiDigit))
            {
                throw new InvalidOperationException(
                    $"Invalid numeric account code: {definition.Code}");
            }


            if (definition.ParentCode is null)
            {
                if (definition.Code.Length != 3)
                {
                    throw new InvalidOperationException(
                        $"Account {definition.Code} must have a parent account.");
                }


                continue;
            }


            if (!definitionsByCode.TryGetValue(definition.ParentCode, out var parent))
            {
                throw new InvalidOperationException(
                    $"Parent account {definition.ParentCode} was not found for {definition.Code}.");
            }


            if (!definition.Code.StartsWith(parent.Code, StringComparison.Ordinal) ||
                definition.Code.Length != parent.Code.Length + 1)
            {
                throw new InvalidOperationException(
                    $"Invalid hierarchy: {definition.Code} cannot be a direct child of {parent.Code}.");
            }


            if (definition.AccountGroupId != parent.AccountGroupId)
            {
                throw new InvalidOperationException(
                    $"Account {definition.Code} and parent {parent.Code} must use the same AccountGroupId.");
            }


            if (parent.IsPostable)
            {
                throw new InvalidOperationException(
                    $"Parent account {parent.Code} cannot be postable.");
            }
        }
    }


    private sealed record AccountDefinition(
        string Code,
        string Name,
        int AccountGroupId,
        string? ParentCode,
        NormalBalanceType NormalBalance,
        bool IsPostable);
}


/// <summary>
/// يزرع مجموعات الحسابات والحسابات بالاعتماد على Code بدل الاعتماد على IDs ثابتة.
/// استخدم هذه الدالة بدل منطق الإدخال القديم ذي المرحلتين والأرقام المتسلسلة المفترضة.
/// </summary>
public static class AccountingChartSeeder
{
    public static async Task SeedAsync(
        DbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);


        var accountBlueprints = AccountSeedData.GetBlueprints();
        var groupDefinitions = CreateGroupDefinitions(accountBlueprints);


        ValidateGroupDefinitions(groupDefinitions);


        var groupsByCode = await UpsertAccountGroupsAsync(
            dbContext,
            groupDefinitions,
            cancellationToken);


        await UpsertAccountsAsync(
            dbContext,
            accountBlueprints,
            groupsByCode,
            cancellationToken);
    }


    private static async Task<Dictionary<string, AccountGroup>> UpsertAccountGroupsAsync(
        DbContext dbContext,
        IReadOnlyCollection<AccountGroupDefinition> definitions,
        CancellationToken cancellationToken)
    {
        var codes = definitions.Select(definition => definition.Code).ToArray();
        var groupsByCode = await dbContext.Set<AccountGroup>()
            .Where(group => codes.Contains(group.Code))
            .ToDictionaryAsync(group => group.Code, StringComparer.Ordinal, cancellationToken);


        foreach (var level in definitions.Select(definition => definition.Level).Distinct().Order())
        {
            foreach (var definition in definitions.Where(definition => definition.Level == level))
            {
                var parentId = definition.ParentCode is null
                    ? (int?)null
                    : groupsByCode[definition.ParentCode].Id;


                if (!groupsByCode.TryGetValue(definition.Code, out var group))
                {
                    group = new AccountGroup
                    {
                        Code = definition.Code,
                        Name = definition.Name,
                        Type = definition.Type,
                        NormalBalance = definition.NormalBalance,
                        Level = (byte)definition.Level,
                        ParentCode = definition.ParentCode,
                        ParentId = parentId,
                        Description = definition.Description
                    };


                    dbContext.Set<AccountGroup>().Add(group);
                    groupsByCode.Add(group.Code, group);
                }
                else
                {
                    group.Name = definition.Name;
                    group.Type = definition.Type;
                    group.NormalBalance = definition.NormalBalance;
                    group.Level = (byte)definition.Level;
                    group.ParentCode = definition.ParentCode;
                    group.ParentId = parentId;
                    group.Description = definition.Description;
                }
            }


            // الحفظ بعد كل مستوى يضمن توليد IDs قبل استخدامها في المستوى التالي.
            await dbContext.SaveChangesAsync(cancellationToken);
        }


        return groupsByCode;
    }


    private static async Task UpsertAccountsAsync(
        DbContext dbContext,
        IReadOnlyCollection<Account> blueprints,
        IReadOnlyDictionary<string, AccountGroup> groupsByCode,
        CancellationToken cancellationToken)
    {
        var codes = blueprints.Select(account => account.Code).ToArray();
        var accountsByCode = await dbContext.Set<Account>()
            .Where(account => codes.Contains(account.Code))
            .ToDictionaryAsync(account => account.Code, StringComparer.Ordinal, cancellationToken);


        foreach (var level in blueprints.Select(account => account.Level).Distinct().Order())
        {
            foreach (var blueprint in blueprints.Where(account => account.Level == level))
            {
                var groupCode = blueprint.Code[..3];
                if (!groupsByCode.TryGetValue(groupCode, out var group))
                {
                    throw new InvalidOperationException(
                        $"Account group {groupCode} was not found for account {blueprint.Code}.");
                }


                var parentCode = blueprint.Level == 3
                    ? null
                    : blueprint.Code[..^1];


                var parentId = parentCode is null
                    ? (int?)null
                    : accountsByCode[parentCode].Id;


                if (!accountsByCode.TryGetValue(blueprint.Code, out var account))
                {
                    account = new Account
                    {
                        Code = blueprint.Code,
                        Name = blueprint.Name,
                        AccountGroupId = group.Id,
                        ParentId = parentId,
                        Level = blueprint.Level,
                        NormalBalance = blueprint.NormalBalance,
                        IsPostable = blueprint.IsPostable
                    };


                    dbContext.Set<Account>().Add(account);
                    accountsByCode.Add(account.Code, account);
                }
                else
                {
                    account.Name = blueprint.Name;
                    account.AccountGroupId = group.Id;
                    account.ParentId = parentId;
                    account.Level = blueprint.Level;
                    account.NormalBalance = blueprint.NormalBalance;
                    account.IsPostable = blueprint.IsPostable;
                }
            }


            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }


    private static AccountGroupDefinition[] CreateGroupDefinitions(
        IReadOnlyCollection<Account> accountBlueprints)
    {
        AccountGroupDefinition[] baseDefinitions =
        [
            G("1", "الموجودات", AccountGroupType.Asset, NormalBalanceType.Debit, null,
                "إجمالي الموجودات الثابتة والاستثمارات والمشاريع والنقدية"),
            G("2", "الموارد الرأسمالية والالتزامات", AccountGroupType.Equity, NormalBalanceType.Credit, null,
                "الفائض والمخصصات والدائنون والحسابات الانتقالية"),
            G("3", "الاستخدامات الجارية", AccountGroupType.Expense, NormalBalanceType.Debit, null,
                "المصروفات التشغيلية والتقاعدية"),
            G("4", "الإيرادات", AccountGroupType.Revenue, NormalBalanceType.Credit, null,
                "الاشتراكات والدعم وعوائد الاستثمار والإيجارات"),


            G("11", "الموجودات الثابتة", AccountGroupType.Asset, NormalBalanceType.Debit, "1",
                "الأراضي والمباني والآلات والسيارات والأثاث ومعدات المكاتب"),
            G("12", "مشاريع قيد التنفيذ", AccountGroupType.Asset, NormalBalanceType.Debit, "1",
                "تكلفة المشاريع إلى حين اكتمالها ورسملتها"),
            G("13", "التوظيفات والاستثمارات", AccountGroupType.Asset, NormalBalanceType.Debit, "1",
                "حصص المشاركة والاستثمارات المالية"),
            G("18", "الأموال الجاهزة", AccountGroupType.Asset, NormalBalanceType.Debit, "1",
                "النقد لدى البنوك"),


            G("22", "الاحتياطيات والفائض", AccountGroupType.Equity, NormalBalanceType.Credit, "2",
                "الفائض المرحل وفائض الدورة"),
            G("23", "المخصصات", AccountGroupType.Equity, NormalBalanceType.Credit, "2",
                "مخصصات الاهتلاك والحقوق"),
            G("25", "الدائنون", AccountGroupType.Equity, NormalBalanceType.Credit, "2",
                "الموردون والذمم الدائنة"),
            G("26", "التأمينات والتوقيفات الدائنة", AccountGroupType.Equity, NormalBalanceType.Credit, "2",
                "تأمينات العقود والمناقصات والتوقيفات"),
            G("27", "الحسابات الانتقالية الدائنة", AccountGroupType.Equity, NormalBalanceType.Credit, "2",
                "إيرادات محصلة مقدمًا ومصاريف مستحقة"),
            G("28", "حسابات النتائج", AccountGroupType.Equity, NormalBalanceType.Credit, "2",
                "عجز أو فائض النشاط الجاري"),


            G("31", "المرتبات والأجور وما في حكمها", AccountGroupType.Expense, NormalBalanceType.Debit, "3",
                "مرتبات موظفي الصندوق وبدلاتهم ومكافآتهم"),
            G("32", "المستلزمات السلعية والخدمية", AccountGroupType.Expense, NormalBalanceType.Debit, "3",
                "المستلزمات التشغيلية والصيانة والخدمات"),
            G("35", "المصروفات التحويلية والمخصصة", AccountGroupType.Expense, NormalBalanceType.Debit, "3",
                "المعاشات والمساعدات والاهتلاك والإيجارات والعمولات"),


            G("41", "إيرادات النشاط الجاري", AccountGroupType.Revenue, NormalBalanceType.Credit, "4",
                "اشتراكات التقاعد والدعم الحكومي"),
            G("42", "الإيرادات المتنوعة", AccountGroupType.Revenue, NormalBalanceType.Credit, "4",
                "إيرادات أخرى ومختلفة"),
            G("43", "إيرادات الأوراق المالية والعوائد", AccountGroupType.Revenue, NormalBalanceType.Credit, "4",
                "عوائد الاستثمارات والأوراق المالية"),
            G("45", "الإيرادات الجارية التحويلية", AccountGroupType.Revenue, NormalBalanceType.Credit, "4",
                "إيرادات الإيجارات")
        ];


        var typeBySection = new Dictionary<char, AccountGroupType>
        {
            ['1'] = AccountGroupType.Asset,
            ['2'] = AccountGroupType.Equity,
            ['3'] = AccountGroupType.Expense,
            ['4'] = AccountGroupType.Revenue
        };


        var levelThreeDefinitions = accountBlueprints
            .Where(account => account.Level == 3)
            .Select(account => G(
                account.Code,
                account.Name,
                typeBySection[account.Code[0]],
                account.NormalBalance,
                account.Code[..2],
                account.Name));


        return baseDefinitions.Concat(levelThreeDefinitions).ToArray();
    }


    private static AccountGroupDefinition G(
        string code,
        string name,
        AccountGroupType type,
        NormalBalanceType normalBalance,
        string? parentCode,
        string description) =>
        new(code, name, type, normalBalance, code.Length, parentCode, description);


    private static void ValidateGroupDefinitions(
        IReadOnlyCollection<AccountGroupDefinition> definitions)
    {
        var definitionsByCode = definitions.ToDictionary(
            definition => definition.Code,
            StringComparer.Ordinal);


        if (definitionsByCode.Count != definitions.Count)
        {
            throw new InvalidOperationException("Duplicate account-group codes were found.");
        }


        foreach (var definition in definitions)
        {
            if (definition.ParentCode is null)
            {
                if (definition.Level != 1)
                {
                    throw new InvalidOperationException(
                        $"Account group {definition.Code} must have a parent.");
                }


                continue;
            }


            if (!definitionsByCode.TryGetValue(definition.ParentCode, out var parent) ||
                definition.Level != parent.Level + 1 ||
                !definition.Code.StartsWith(parent.Code, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Invalid account-group hierarchy: {definition.Code} -> {definition.ParentCode}.");
            }
        }
    }


    private sealed record AccountGroupDefinition(
        string Code,
        string Name,
        AccountGroupType Type,
        NormalBalanceType NormalBalance,
        int Level,
        string? ParentCode,
        string Description);
}
