using ERP_Government.Application.FinancialSettings.Commands.DocumentSequences.CreateDocumentSequence;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings;

[TestFixture]
public class CreateDocumentSequenceValidatorTests
{
    private CreateDocumentSequenceValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateDocumentSequenceValidator();
    }

    [Test]
    public async Task Validate_NameIsEmpty_ShouldHaveError()
    {
        var command = new CreateDocumentSequenceCommand { Name = "", DocumentType = "Test" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task Validate_DocumentTypeIsEmpty_ShouldHaveError()
    {
        var command = new CreateDocumentSequenceCommand { Name = "Test", DocumentType = "" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "DocumentType");
    }

    [Test]
    public async Task Validate_NameExceeds100Chars_ShouldHaveError()
    {
        var command = new CreateDocumentSequenceCommand
        {
            Name = new string('A', 101),
            DocumentType = "Test"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateDocumentSequenceCommand
        {
            Name = "Valid Name",
            DocumentType = "PaymentOrder"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }
}
