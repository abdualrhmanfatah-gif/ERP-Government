using ERP_Government.Application.Accounting.Commands.TemplateLines.CreateTemplateLine;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting.TemplateLines;

public class TemplateLineTests
{
    [Test]
    public void CreateValidator_ExchangeRateZero_ShouldFail()
    {
        var cmd = new CreateTemplateLineCommand { TemplateId = 1, AccountId = 1, CurrencyId = 1, ExchangeRate = 0, Debit = 100, Credit = 0 };
        var validator = new CreateTemplateLineCommandValidator();
        var result = validator.Validate(cmd);
        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void CreateValidator_XorViolation_PassesValidatorFailsHandler()
    {
        var cmd = new CreateTemplateLineCommand { TemplateId = 1, AccountId = 1, CurrencyId = 1, ExchangeRate = 1, Debit = 100, Credit = 100 };
        var validator = new CreateTemplateLineCommandValidator();
        var result = validator.Validate(cmd);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void CreateValidator_MissingTemplate_ShouldFail()
    {
        var cmd = new CreateTemplateLineCommand { TemplateId = 0, AccountId = 1, CurrencyId = 1, ExchangeRate = 1, Debit = 100, Credit = 0 };
        var validator = new CreateTemplateLineCommandValidator();
        var result = validator.Validate(cmd);
        result.IsValid.ShouldBeFalse();
    }
}
