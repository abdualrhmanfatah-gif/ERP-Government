using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// AssetAttributeDefinition seed data — 32 definitions.
/// </summary>
public static class AssetAttributeDefinitionSeedData
{
    public static List<AssetAttributeDefinition> GetDefinitions() =>
    [
        new() { Code="PLATE_NO", Name="رقم اللوحة", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="SERIAL_NO", Name="الرقم التسلسلي للمصنّع", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="MANUFACTURER", Name="الشركة المصنّعة", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="MODEL_YEAR", Name="سنة الصنع", AttributeDataType=AssetAttributeDataType.Integer, IsActive=true },
        new() { Code="WARRANTY_END", Name="نهاية الضمان", AttributeDataType=AssetAttributeDataType.Date, IsActive=true },
        new() { Code="OPERATIONAL", Name="جاهز للتشغيل", AttributeDataType=AssetAttributeDataType.Boolean, IsActive=true },
        new() { Code="CONDITION", Name="الحالة الفنية (جيد/متوسط/تالف)", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },

        new() { Code="DEED_NO", Name="رقم الصك", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="AREA_SQM", Name="المساحة بالمتر المربع", AttributeDataType=AssetAttributeDataType.Integer, IsActive=true },
        new() { Code="FLOORS_COUNT", Name="عدد الطوابق", AttributeDataType=AssetAttributeDataType.Integer, IsActive=true },
        new() { Code="CONSTRUCTION_DATE", Name="تاريخ الإنشاء", AttributeDataType=AssetAttributeDataType.Date, IsActive=true },
        new() { Code="HAS_ELEVATOR", Name="وجود مصعد", AttributeDataType=AssetAttributeDataType.Boolean, IsActive=true },

        new() { Code="MATERIAL", Name="المادة", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="COLOR", Name="اللون", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="STYLE_MODEL", Name="الطراز/الموديل", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="LENGTH_CM", Name="الطول بالسنتيمتر", AttributeDataType=AssetAttributeDataType.Decimal, IsActive=true },
        new() { Code="WIDTH_CM", Name="العرض بالسنتيمتر", AttributeDataType=AssetAttributeDataType.Decimal, IsActive=true },
        new() { Code="HEIGHT_CM", Name="الارتفاع بالسنتيمتر", AttributeDataType=AssetAttributeDataType.Decimal, IsActive=true },
        new() { Code="WEIGHT_KG", Name="الوزن بالكيلوغرام", AttributeDataType=AssetAttributeDataType.Decimal, IsActive=true },
        new() { Code="POWER_KW", Name="القدرة بالكيلوواط", AttributeDataType=AssetAttributeDataType.Decimal, IsActive=true },

        new() { Code="CHASSIS_NO", Name="رقم الهيكل", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="FUEL_TYPE", Name="نوع الوقود", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="ENGINE_CAPACITY_L", Name="سعة المحرك باللتر", AttributeDataType=AssetAttributeDataType.Decimal, IsActive=true },
        new() { Code="PAYLOAD_KG", Name="الحمولة بالكيلوغرام", AttributeDataType=AssetAttributeDataType.Decimal, IsActive=true },
        new() { Code="SEATS_COUNT", Name="عدد المقاعد", AttributeDataType=AssetAttributeDataType.Integer, IsActive=true },
        new() { Code="INSPECTION_END", Name="انتهاء الفحص الفني", AttributeDataType=AssetAttributeDataType.Date, IsActive=true },
        new() { Code="INSURANCE_END", Name="انتهاء التأمين", AttributeDataType=AssetAttributeDataType.Date, IsActive=true },

        new() { Code="PROCESSOR", Name="المعالج", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="DEVICE_TYPE", Name="نوع الجهاز", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="MODEL", Name="الموديل", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="RAM_TYPE", Name="نوع الذاكرة", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="STORAGE_TYPE", Name="نوع التخزين", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="GRAPHICS_CARD", Name="كرت الشاشة", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="OPERATING_SYSTEM", Name="نظام التشغيل", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="SCREEN_SIZE_INCH", Name="حجم الشاشة بالبوصة", AttributeDataType=AssetAttributeDataType.Decimal, IsActive=true },
        new() { Code="SCREEN_RESOLUTION", Name="دقة الشاشة", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="ARABIC_KEYBOARD", Name="لوحة مفاتيح عربية", AttributeDataType=AssetAttributeDataType.Boolean, IsActive=true },
        new() { Code="RAM_GB", Name="الذاكرة العشوائية بالجيجابايت", AttributeDataType=AssetAttributeDataType.Integer, IsActive=true },
        new() { Code="STORAGE_GB", Name="مساحة التخزين بالجيجابايت", AttributeDataType=AssetAttributeDataType.Integer, IsActive=true },
        new() { Code="NETWORK_PORTS", Name="عدد منافذ الشبكة", AttributeDataType=AssetAttributeDataType.Integer, IsActive=true },
        new() { Code="PRINT_SPEED_PPM", Name="سرعة الطباعة صفحة/دقيقة", AttributeDataType=AssetAttributeDataType.Integer, IsActive=true },
        new() { Code="COLOR_SUPPORT", Name="دعم الألوان", AttributeDataType=AssetAttributeDataType.Boolean, IsActive=true },
        new() { Code="CONNECTIVITY", Name="نوع الاتصال", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },

        new() { Code="VERSION", Name="الإصدار", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="LICENSE_TYPE", Name="نوع الترخيص", AttributeDataType=AssetAttributeDataType.Text, IsActive=true },
        new() { Code="LICENSE_USERS", Name="عدد المستخدمين المرخّصين", AttributeDataType=AssetAttributeDataType.Integer, IsActive=true },
        new() { Code="LICENSE_START", Name="بداية الترخيص", AttributeDataType=AssetAttributeDataType.Date, IsActive=true },
        new() { Code="LICENSE_END", Name="انتهاء الترخيص", AttributeDataType=AssetAttributeDataType.Date, IsActive=true },
        new() { Code="AUTO_RENEW", Name="التجديد التلقائي", AttributeDataType=AssetAttributeDataType.Boolean, IsActive=true },
    ];
}
