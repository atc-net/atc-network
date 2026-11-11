namespace Atc.Network.Test;

/// <summary>
/// Every config class lists all its public properties in ToString, so a logged config is complete.
/// </summary>
public sealed class ConfigToStringTests
{
    public static TheoryData<Type> ConfigTypes()
    {
        var data = new TheoryData<Type>();
        foreach (var type in typeof(TcpServerConfig).Assembly
                     .GetExportedTypes()
                     .Where(x => x is { IsClass: true, IsAbstract: false } &&
                                 x.Name.EndsWith("Config", StringComparison.Ordinal))
                     .OrderBy(x => x.FullName, StringComparer.Ordinal))
        {
            data.Add(type);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ConfigTypes))]
    public void ToString_ListsEveryPublicProperty(Type configType)
    {
        // Arrange
        ArgumentNullException.ThrowIfNull(configType);
        var config = Activator.CreateInstance(configType);

        // Act
        var actual = config?.ToString() ?? string.Empty;

        // Assert
        var missing = configType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => x.Name)
            .Where(x => !actual.Contains($"{x}: ", StringComparison.Ordinal))
            .ToList();
        Assert.True(missing.Count == 0, $"{configType.Name}.ToString() misses: {string.Join(", ", missing)}");
    }

    [Fact]
    public void IPScannerConfig_ToString_ShowsTheNumberOfPorts()
    {
        // Arrange
        var config = new IPScannerConfig { PortNumbers = [80, 443] };

        // Act
        var actual = config.ToString();

        // Assert - not the collection's type name.
        Assert.Contains("PortNumbers: 2 ports", actual, StringComparison.Ordinal);
    }

    [Fact]
    public void TcpServerConfig_ToString_ShowsWhetherTlsIsSet()
    {
        // Arrange
        var config = new TcpServerConfig { SslOptions = new SslServerAuthenticationOptions(), DualMode = true };

        // Act
        var actual = config.ToString();

        // Assert - not the options' type name.
        Assert.Contains("SslOptions: Set", actual, StringComparison.Ordinal);
        Assert.Contains("DualMode: True", actual, StringComparison.Ordinal);
    }
}