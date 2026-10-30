using MoreHotbarSlots;
using Xunit;

public class SlotLogicTests
{
    [Fact]
    public void CapIsTen()
    {
        Assert.Equal(10, SlotLogic.Cap);
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(1, 1)]
    [InlineData(9, 9)]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(12, 9)]
    public void ClampBaseKeepsOneToNine(int value, int expected)
    {
        Assert.Equal(expected, SlotLogic.ClampBase(value));
    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(2, 8)]
    public void BagSlotsMapsTheTwoBackpacksWithTheDefaults(int vanilla, int expected)
    {
        Assert.Equal(expected, SlotLogic.BagSlots(vanilla, 2, 4, 8));
    }

    [Fact]
    public void DefaultTotalsAreTwoSixTen()
    {
        Assert.Equal(6, 2 + SlotLogic.BagSlots(1, 2, 4, 8));
        Assert.Equal(10, 2 + SlotLogic.BagSlots(2, 2, 4, 8));
    }

    [Fact]
    public void BagSlotsFitTheCap()
    {
        Assert.Equal(6, SlotLogic.BagSlots(2, 4, 4, 9));
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(6, 6)]
    [InlineData(9, 8)]
    public void UnknownBackpackKeepsItsValueWithinTheCap(int vanilla, int expected)
    {
        Assert.Equal(expected, SlotLogic.BagSlots(vanilla, 2, 4, 8));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void BaseNineLeavesOneBagSlot(int vanilla)
    {
        Assert.Equal(1, SlotLogic.BagSlots(vanilla, 9, 4, 8));
    }

    [Theory]
    [InlineData(8, -1)]
    [InlineData(9, 0)]
    [InlineData(10, 1)]
    [InlineData(11, -1)]
    [InlineData(0, -1)]
    public void KeySlotNamesTheKeysOfSlotsNineAndTen(int slotNumber, int expected)
    {
        Assert.Equal(expected, SlotLogic.KeySlot(slotNumber));
    }

    [Fact]
    public void BagSlotsIsAtLeastOne()
    {
        Assert.Equal(1, SlotLogic.BagSlots(0, 2, 4, 8));
    }
}
