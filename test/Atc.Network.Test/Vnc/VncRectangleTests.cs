namespace Atc.Network.Test.Vnc;

public class VncRectangleTests
{
    [Fact]
    public void Properties_Are_Set_Correctly()
    {
        // Arrange & Act
        var rect = new VncRectangle(10, 20, 100, 200);

        // Assert
        Assert.Equal(10, rect.X);
        Assert.Equal(20, rect.Y);
        Assert.Equal(100, rect.Width);
        Assert.Equal(200, rect.Height);
    }

    [Fact]
    public void Equality_Works_For_Same_Values()
    {
        // Arrange
        var rect1 = new VncRectangle(1, 2, 3, 4);
        var rect2 = new VncRectangle(1, 2, 3, 4);

        // Act & Assert
        Assert.Equal(rect1, rect2);
    }

    [Fact]
    public void Inequality_Works_For_Different_Values()
    {
        // Arrange
        var rect1 = new VncRectangle(1, 2, 3, 4);
        var rect2 = new VncRectangle(5, 6, 7, 8);

        // Act & Assert
        Assert.NotEqual(rect1, rect2);
    }
}