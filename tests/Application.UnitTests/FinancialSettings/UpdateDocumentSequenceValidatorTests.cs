using ERP_Government.Application.FinancialSettings.Commands.DocumentSequences.UpdateDocumentSequence;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings;

[TestFixture]
public class UpdateDocumentSequenceValidatorTests
{
    private UpdateDocumentSequenceValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateDocumentSequenceValidator();
    }

    [Test]
    public async Task Validate_IdIsZero_ShouldHaveError()
    {
        var command = new UpdateDocumentSequenceCommand { Id = 0, Name = "Test" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Id");
    }

    [Test]
    public async Task Validate_NameExceeds100Chars_ShouldHaveError()
    {
        var command = new UpdateDocumentSequenceCommand
        {
            Id = 1,
            Name = new string('A', 101)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task Validate_NameIsEmpty_ShouldHaveError()
    {
        var command = new UpdateDocumentSequenceCommand
        {
            Id = 1,
            Name = ""
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task Validate_NullName_ShouldPass()
    {
        var command = new UpdateDocumentSequenceCommand
        {
            Id = 1,
            Name = null
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task Validate_ValidIdWithValidName_ShouldPass()
    {
        var command = new UpdateDocumentSequenceCommand
        {
            Id = 1,
            Name = "Valid Name"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }
}
