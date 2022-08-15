namespace Atc.Network.Test.Helpers;

public class ArpHelperTests
{
    [Fact]
    public void GetArpResult()
        => Assert.NotNull(ArpHelper.GetArpResult());
}