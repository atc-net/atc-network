namespace Atc.Network.Test;

/// <summary>
/// Pins the packages that consumers of Atc.Network get. They are read from the test's .deps.json,
/// which lists the package references of Atc.Network.
/// </summary>
public sealed class PackageDependencyTests
{
    [Fact]
    public void AtcNetwork_DependsOnHostingAbstractions_NotTheFullHostingPackage()
    {
        // Act
        var dependencies = GetDependencies("Atc.Network/");

        // Assert - the library only needs IHostedService, which is in Hosting.Abstractions.
        Assert.Contains("Microsoft.Extensions.Hosting.Abstractions", dependencies.Keys, StringComparer.Ordinal);
        Assert.DoesNotContain("Microsoft.Extensions.Hosting", dependencies.Keys, StringComparer.Ordinal);
    }

    [Fact]
    public void AtcNetwork_MicrosoftExtensions_AreOnTheTargetFrameworksMajorVersion()
    {
        // Arrange - net10.0 => 10. An app on net10.0 with Microsoft.Extensions 10.0.x must be able to restore
        // Atc.Network; a dependency on 11.0.0 makes its restore fail with a downgrade error (NU1605).
        var targetFramework = typeof(TcpClient).Assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();
        Assert.NotNull(targetFramework);
        var targetMajor = new Version(targetFramework.FrameworkName.Split("Version=v")[1]).Major;

        // Act
        var microsoftExtensions = GetDependencies("Atc.Network/")
            .Where(x => x.Key.StartsWith("Microsoft.Extensions.", StringComparison.Ordinal))
            .ToList();

        // Assert
        Assert.NotEmpty(microsoftExtensions);
        Assert.All(microsoftExtensions, x => Assert.Equal(targetMajor, new Version(x.Value).Major));
    }

    /// <summary>
    /// Returns the package dependencies (name and version) of the first library in the .deps.json
    /// whose name starts with <paramref name="libraryPrefix"/>, such as "Atc.Network/".
    /// </summary>
    private static Dictionary<string, string> GetDependencies(
        string libraryPrefix)
    {
        var depsFile = Path.Combine(AppContext.BaseDirectory, "Atc.Network.Test.deps.json");
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(depsFile));

        foreach (var target in document.RootElement.GetProperty("targets").EnumerateObject())
        {
            foreach (var library in target.Value.EnumerateObject())
            {
                if (library.Name.StartsWith(libraryPrefix, StringComparison.Ordinal))
                {
                    return library.Value
                        .GetProperty("dependencies")
                        .EnumerateObject()
                        .ToDictionary(x => x.Name, x => x.Value.GetString() ?? string.Empty, StringComparer.Ordinal);
                }
            }
        }

        throw new InvalidOperationException($"{libraryPrefix} is not listed in {depsFile}.");
    }
}