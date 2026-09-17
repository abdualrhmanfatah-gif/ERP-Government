using ERP_Government.Domain.Assets.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Domain.UnitTests.Assets;

[TestFixture]
public class AssetAttributeDataTypeTests
{
    [Test]
    public void AssetAttributeDataType_HasExactlyFiveMembers()
    {
        var values = Enum.GetValues<AssetAttributeDataType>();
        values.Length.ShouldBe(5);
    }

    [Test]
    public void AssetAttributeDataType_MembersAreSequential()
    {
        ((int)AssetAttributeDataType.Text).ShouldBe(1);
        ((int)AssetAttributeDataType.Integer).ShouldBe(2);
        ((int)AssetAttributeDataType.Decimal).ShouldBe(3);
        ((int)AssetAttributeDataType.Date).ShouldBe(4);
        ((int)AssetAttributeDataType.Boolean).ShouldBe(5);
    }

    [Test]
    public void AssetAttributeDataType_NumericIsRemoved()
    {
        Enum.IsDefined(typeof(AssetAttributeDataType), 1).ShouldBeTrue();
        typeof(AssetAttributeDataType).GetField("Numeric").ShouldBeNull();
    }
}
