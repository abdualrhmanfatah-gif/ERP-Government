using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Egyptian Government Pension Fund — Chart of Accounts (دليل محاسبي)
/// Account seed data — Levels 3-7 based on official chart.
/// AccountGroupId references AccountGroupSeedData IDs.
/// </summary>
public static class AccountSeedData
{
    public static List<Account> GetAccounts()
    {
        var accounts = new List<Account>();

        // ═══ Asset Section (AccountGroupId=1: الموجودات) ═══
        accounts.AddRange(GetAssetAccounts());

        // ═══ Equity Section (AccountGroupId=2: الموارد الرأسمالية) ═══
        accounts.AddRange(GetEquityAccounts());

        // ═══ Expense Section (AccountGroupId=3: الاستخدامات الجارية) ═══
        accounts.AddRange(GetExpenseAccounts());

        // ═══ Revenue Section (AccountGroupId=4: الإيرادات) ═══
        accounts.AddRange(GetRevenueAccounts());

        return accounts;
    }

    // ─── ASSET (الموجودات — AccountGroupId=1) ───
    private static List<Account> GetAssetAccounts() =>
    [
        // ═══ 12: مشاريع قيد التنفيذ (AccountGroupId=18: المشاريع الاستثمارية) ═══
        new() { Code="122",  Name="المشاريع الاستثمارية",                        AccountGroupId=18, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },

        // 1222: مشاريع المباني والإنشاءات
        new() { Code="1222", Name="مشاريع المباني والإنشاءات",                   AccountGroupId=18, ParentId=1,    Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="12221",Name="مباني الإدارة",                               AccountGroupId=18, ParentId=2,    Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="12222",Name="مباني استثمارية",                             AccountGroupId=18, ParentId=2,    Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 1223: مشاريع الآلات والتجهيزات والمعدات
        new() { Code="1223", Name="مشاريع الآلات والتجهيزات والمعدات",          AccountGroupId=18, ParentId=1,    Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="12233",Name="آلات وتجهيزات أخرى ومختلفة",                 AccountGroupId=18, ParentId=5,    Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 1225: مشاريع الأثاث ومعدات المكاتب
        new() { Code="1225", Name="مشاريع الأثاث ومعدات المكاتب",               AccountGroupId=18, ParentId=1,    Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="12258",Name="أجهزة كمبيوتر",                              AccountGroupId=18, ParentId=7,    Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="12259",Name="أثاث ومعدات أخرى ومختلفة",                   AccountGroupId=18, ParentId=7,    Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // ═══ 13: التوظيفات والاستثمارات ═══
        // 133: حصص المشاركة (AccountGroupId=19)
        new() { Code="133",  Name="حصص المشاركة",                               AccountGroupId=19, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="1331", Name="مؤسسات وشركات محلية",                        AccountGroupId=19, ParentId=10,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 135: استثمارات مالية بسندات (AccountGroupId=20)
        new() { Code="135",  Name="استثمارات مالية بسندات",                      AccountGroupId=20, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="1351", Name="استثمارات مالية بسندات حكومية",               AccountGroupId=20, ParentId=12,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // ═══ 18: الزيادة في الأموال الجاهزة ═══
        // 182: نقد لدى البنوك (AccountGroupId=21)
        new() { Code="182",  Name="نقد لدى البنوك",                            AccountGroupId=21, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="1821", Name="حسابات جارية محلية",                         AccountGroupId=21, ParentId=14,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
    ];

    // ─── EQUITY (الموارد الرأسمالية — AccountGroupId=2) ───
    private static List<Account> GetEquityAccounts() =>
    [
        // ═══ 22: الاحتياطيات والفائض ═══
        // 222: الفائض المرحل (AccountGroupId=22)
        new() { Code="222",  Name="الفائض المرحل",                            AccountGroupId=22, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="2221", Name="فائض الدورة الحالية",                       AccountGroupId=22, ParentId=16,   Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

        // ═══ 23: المخصصات ═══
        // 231: مخصص الاهتلاكات (AccountGroupId=23)
        new() { Code="231",  Name="مخصص الاهتلاكات",                           AccountGroupId=23, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="2311", Name="مخصص اهتلاك المباني والإنشاءات",            AccountGroupId=23, ParentId=18,   Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
        new() { Code="2312", Name="مخصص اهتلاك الآلات والتجهيزات والمعدات",    AccountGroupId=23, ParentId=18,   Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
        new() { Code="2313", Name="مخصص اهتلاك السيارات ووسائل النقل",         AccountGroupId=23, ParentId=18,   Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
        new() { Code="2314", Name="مخصص اهتلاك الأثاث والمفروشات",            AccountGroupId=23, ParentId=18,   Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
        new() { Code="2315", Name="مخصص حقوق العاملين أخرى ومختلفة",           AccountGroupId=23, ParentId=18,   Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

        // ═══ 28: حسابات النتائج ═══
        // 281: حساب توزيع (عجز النشاط الجاري) (AccountGroupId=24)
        new() { Code="281",  Name="حساب توزيع (عجز النشاط الجاري)",           AccountGroupId=24, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="2812", Name="حساب التوزيع (عجز النشاط الجاري)",         AccountGroupId=24, ParentId=25,   Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

        // 282: حساب توزيع (فائض النشاط الجاري) (AccountGroupId=25)
        new() { Code="282",  Name="حساب توزيع (فائض النشاط الجاري)",          AccountGroupId=25, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="2822", Name="حساب التوزيع (فائض النشاط الجاري)",        AccountGroupId=25, ParentId=27,   Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
    ];

    // ─── EXPENSE (الاستخدامات الجارية — AccountGroupId=3) ───
    private static List<Account> GetExpenseAccounts() =>
    [
        // ═══ 31: المرتبات والأجور وما في حكمها ═══
        // 311: المرتبات والأجور النقدية (AccountGroupId=26)
        new() { Code="311",  Name="المرتبات والأجور النقدية",                  AccountGroupId=26, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="3111", Name="مرتبات الموظفين الدائمين",                   AccountGroupId=26, ParentId=29,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="3113", Name="مرتبات وأجور موسمية",                        AccountGroupId=26, ParentId=29,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 312: البدلات والتعويضات (AccountGroupId=27)
        new() { Code="312",  Name="البدلات والتعويضات",                        AccountGroupId=27, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="3124", Name="بدل حضور جلسات",                            AccountGroupId=27, ParentId=32,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="3125", Name="تعويض المسؤولية",                           AccountGroupId=27, ParentId=32,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="3126", Name="تعويض العمل الإضافي",                       AccountGroupId=27, ParentId=32,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="3129", Name="بدلات وتعويضات أخرى",                      AccountGroupId=27, ParentId=32,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 313: المزايا العينية (AccountGroupId=28)
        new() { Code="313",  Name="المزايا العينية",                           AccountGroupId=28, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="3139", Name="مزايا عينية مختلفة أخرى",                   AccountGroupId=28, ParentId=37,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 314: المكافآت (AccountGroupId=29)
        new() { Code="314",  Name="المكافآت",                                  AccountGroupId=29, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="3141", Name="المكافآت التشجيعية",                        AccountGroupId=29, ParentId=39,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 316: تأمينات وتقاعد العاملين (AccountGroupId=30)
        new() { Code="316",  Name="تأمينات وتقاعد العاملين",                   AccountGroupId=30, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="3161", Name="تأمين الشيخوخة أو التقاعد",                AccountGroupId=30, ParentId=41,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // ═══ 32: المستلزمات السلعية ومشتريات بغرض البيع ═══
        // 321: المستلزمات السلعية (AccountGroupId=31)
        new() { Code="321",  Name="المستلزمات السلعية",                        AccountGroupId=31, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },

        // 3212: الوقود والزيوت والقوى المحركة والمياه
        new() { Code="3212", Name="الوقود والزيوت والقوى المحركة والمياه",     AccountGroupId=31, ParentId=43,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32122",Name="المواد البترولية",                           AccountGroupId=31, ParentId=44,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32123",Name="الكهرباء",                                  AccountGroupId=31, ParentId=44,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32128",Name="الزيوت والشحوم",                            AccountGroupId=31, ParentId=44,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32129",Name="المياه",                                    AccountGroupId=31, ParentId=44,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3213: قطع التبديل واللوازم
        new() { Code="3213", Name="قطع التبديل واللوازم",                      AccountGroupId=31, ParentId=43,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32131",Name="قطع التبديل للصيانة",                        AccountGroupId=31, ParentId=49,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3215: القرطاسية والمطبوعات
        new() { Code="3215", Name="القرطاسية والمطبوعات",                      AccountGroupId=31, ParentId=43,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32151",Name="المنشورات والكتب الدورية",                  AccountGroupId=31, ParentId=51,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32152",Name="القرطاسية - لوازم الكتابة",                 AccountGroupId=31, ParentId=51,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32154",Name="قرطاسية - لوازم التصوير",                   AccountGroupId=31, ParentId=51,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32156",Name="السجلات والدفاتر",                          AccountGroupId=31, ParentId=51,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 322: المستلزمات الخدمية (AccountGroupId=32)
        new() { Code="322",  Name="المستلزمات الخدمية",                        AccountGroupId=32, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },

        // 3221: الصيانة والتصليحات
        new() { Code="3221", Name="الصيانة والتصليحات",                        AccountGroupId=32, ParentId=56,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32211",Name="صيانة المباني والطرق",                      AccountGroupId=32, ParentId=57,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32212",Name="صيانة الآلات والتجهيزات",                   AccountGroupId=32, ParentId=57,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32213",Name="صيانة السيارات ووسائل النقل",               AccountGroupId=32, ParentId=57,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32215",Name="صيانة الأثاث والمفروشات",                   AccountGroupId=32, ParentId=57,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3223: الأبحاث والتجارب
        new() { Code="3223", Name="الأبحاث والتجارب",                          AccountGroupId=32, ParentId=56,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32235",Name="الخدمات الاستشارية الفنية",                 AccountGroupId=32, ParentId=62,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3224: نشر وإعلان ومصروفات ضيافة واستقبال
        new() { Code="3224", Name="نشر وإعلان ومصروفات ضيافة واستقبال",       AccountGroupId=32, ParentId=56,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32241",Name="إعلانات منشورة",                            AccountGroupId=32, ParentId=64,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32242",Name="الدعاية الدورية أو الموسمية",               AccountGroupId=32, ParentId=64,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32246",Name="خدمات الاستقبال والضيافة",                  AccountGroupId=32, ParentId=64,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3225: التنقلات وبدلات السفر والمواصلات
        new() { Code="3225", Name="التنقلات وبدلات السفر والمواصلات",         AccountGroupId=32, ParentId=56,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32251",Name="نقل مهمات",                                AccountGroupId=32, ParentId=68,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32252",Name="نقل وانتقالات عامة",                        AccountGroupId=32, ParentId=68,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32253",Name="بدلات السفر الداخلية",                      AccountGroupId=32, ParentId=68,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32255",Name="الاتصالات الهاتفية",                        AccountGroupId=32, ParentId=68,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3227: خدمات الإدارات الحكومية والمؤسسات
        new() { Code="3227", Name="خدمات الإدارات الحكومية والمؤسسات",        AccountGroupId=32, ParentId=56,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32271",Name="خدمات الحراسة والأمن",                      AccountGroupId=32, ParentId=73,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="32277",Name="خدمات التفتيش ومراجعة الحسابات",            AccountGroupId=32, ParentId=73,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3228: الخدمات المتممة
        new() { Code="3228", Name="الخدمات المتممة",                           AccountGroupId=32, ParentId=56,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32283",Name="خدمات المكاتب الاستشارية",                  AccountGroupId=32, ParentId=75,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3229: مستلزمات خدمية أخرى
        new() { Code="3229", Name="مستلزمات خدمية أخرى",                      AccountGroupId=32, ParentId=56,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32299",Name="مستلزمات خدمية أخرى ومختلفة",              AccountGroupId=32, ParentId=77,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 323: مشتريات بغرض البيع (AccountGroupId=33)
        new() { Code="323",  Name="مشتريات بغرض البيع",                        AccountGroupId=33, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="3231", Name="مشتريات محلية",                             AccountGroupId=33, ParentId=79,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="32311",Name="مشتريات بثمن التكلفة",                      AccountGroupId=33, ParentId=80,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // ═══ 35: المصروفات الجارية التحويلية والمخصصة ═══
        // 351: المصروفات الجارية التحويلية (AccountGroupId=34)
        new() { Code="351",  Name="المصروفات الجارية التحويلية",               AccountGroupId=34, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },

        // 3511: الاهتلاك
        new() { Code="3511", Name="الاهتلاك",                                  AccountGroupId=34, ParentId=82,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="35111",Name="اهتلاك المباني والإنشاءات",                 AccountGroupId=34, ParentId=83,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="35112",Name="اهتلاك الآلات والتجهيزات والمعدات",        AccountGroupId=34, ParentId=83,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="35113",Name="اهتلاك السيارات ووسائل النقل",             AccountGroupId=34, ParentId=83,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="35114",Name="اهتلاك الأثاث والمفروشات",                AccountGroupId=34, ParentId=83,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3513: الإيجارات
        new() { Code="3513", Name="الإيجارات",                                 AccountGroupId=34, ParentId=82,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="35136",Name="إيجارات مباني في الداخل",                  AccountGroupId=34, ParentId=88,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3514: الفوائد والعمولات
        new() { Code="3514", Name="الفوائد والعمولات",                         AccountGroupId=34, ParentId=82,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="35141",Name="الفوائد والعمولات المحلية",                AccountGroupId=34, ParentId=90,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 352: المصروفات المخصصة (AccountGroupId=35)
        new() { Code="352",  Name="المصروفات المخصصة",                         AccountGroupId=35, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Debit, IsPostable=false },

        // 3521: التبرعات
        new() { Code="3521", Name="التبرعات",                                  AccountGroupId=35, ParentId=92,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="35211",Name="التبرعات النقدية",                          AccountGroupId=35, ParentId=93,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3522: الإعانات والمساعدات والزكاة
        new() { Code="3522", Name="الإعانات والمساعدات والزكاة",              AccountGroupId=35, ParentId=92,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="35221",Name="الإعانات النقدية",                          AccountGroupId=35, ParentId=95,   Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="352211",Name="معاشات التقاعد (أساسي + بدلات)",          AccountGroupId=35, ParentId=96,   Level=6, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="352212",Name="غلاء المعيشة للمتقاعدين",                 AccountGroupId=35, ParentId=96,   Level=6, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="352213",Name="فروقات تسويات المعاشات للمتقاعدين",       AccountGroupId=35, ParentId=96,   Level=6, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="352214",Name="مكافأة التقاعد",                           AccountGroupId=35, ParentId=96,   Level=6, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="352215",Name="تجهيز وتكفين",                            AccountGroupId=35, ParentId=96,   Level=6, NormalBalance=NormalBalanceType.Debit, IsPostable=true },

        // 3526: أعباء المخصصات
        new() { Code="3526", Name="أعباء المخصصات",                            AccountGroupId=35, ParentId=92,   Level=4, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="35263",Name="الديون المشكوك في تحصيلها",                AccountGroupId=35, ParentId=102,  Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="35269",Name="التعويضات والغرامات المختلفة",             AccountGroupId=35, ParentId=102,  Level=5, NormalBalance=NormalBalanceType.Debit, IsPostable=false },
        new() { Code="352691",Name="تبادل الاحتياطيات",                       AccountGroupId=35, ParentId=104,  Level=6, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
        new() { Code="352692",Name="نفقات تأمينية",                           AccountGroupId=35, ParentId=104,  Level=6, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
    ];

    // ─── REVENUE (الإيرادات — AccountGroupId=4) ───
    private static List<Account> GetRevenueAccounts() =>
    [
        // ═══ 41: إيرادات النشاط الجاري ═══
        // 412: إيرادات العمليات التجارية (AccountGroupId=36)
        new() { Code="412",  Name="إيرادات العمليات التجارية",                 AccountGroupId=36, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="4121", Name="مبيعات البضائع الجاهزة",                   AccountGroupId=36, ParentId=106,  Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

        // 414: إيرادات قطاع الخدمات (AccountGroupId=37)
        new() { Code="414",  Name="إيرادات قطاع الخدمات",                     AccountGroupId=37, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="4142", Name="إيرادات الاشتراكات",                        AccountGroupId=37, ParentId=108,  Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="41421",Name="إيرادات حصة الحكومة في التقاعد",            AccountGroupId=37, ParentId=109,  Level=5, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
        new() { Code="41422",Name="إيرادات حصة الموظفين في التقاعد",           AccountGroupId=37, ParentId=109,  Level=5, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
        new() { Code="41423",Name="إيرادات دعم الحكومة لغلاء المعيشة",        AccountGroupId=37, ParentId=109,  Level=5, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
        new() { Code="41424",Name="إيرادات دعم الحكومة للمعاشات الاستثنائية", AccountGroupId=37, ParentId=109,  Level=5, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
        new() { Code="41425",Name="إيرادات دعم الحكومة لتسويات معاشات المحالين", AccountGroupId=37, ParentId=109, Level=5, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
        new() { Code="41426",Name="إيرادات ضم الخدمات",                        AccountGroupId=37, ParentId=109,  Level=5, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

        // ═══ 42: الإيرادات المتنوعة ═══
        // 429: إيرادات أخرى ومختلفة (AccountGroupId=15)
        new() { Code="429",  Name="إيرادات أخرى ومختلفة",                     AccountGroupId=15, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="4291", Name="أخرى ومختلفة",                             AccountGroupId=15, ParentId=117,  Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

        // ═══ 43: إيرادات الأوراق المالية والعوائد ═══
        // 431: إيرادات أوراق مالية محلية (AccountGroupId=38)
        new() { Code="431",  Name="إيرادات أوراق مالية محلية",                 AccountGroupId=38, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="4311", Name="إيرادات أوراق مالية محلية",                 AccountGroupId=38, ParentId=119,  Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

        // 433: إيرادات عوائد محلية (AccountGroupId=39)
        new() { Code="433",  Name="إيرادات عوائد محلية",                       AccountGroupId=39, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="4331", Name="إيرادات عوائد محلية",                       AccountGroupId=39, ParentId=121,  Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

        // ═══ 45: الإيرادات الجارية التحويلية ═══
        // 452: الإيجارات الدائنة (AccountGroupId=40)
        new() { Code="452",  Name="الإيجارات الدائنة",                        AccountGroupId=40, ParentId=null, Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
        new() { Code="4521", Name="إيجار مباني في الداخل",                    AccountGroupId=40, ParentId=123,  Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
    ];
}
