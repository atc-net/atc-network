namespace Atc.Network.Test;

public class CodeDocumentationTests
{
    // ReSharper disable once NotAccessedField.Local
    [SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "OK.")]
    private readonly ITestOutputHelper testOutputHelper;
    private readonly Assembly sourceAssembly = typeof(AtcNetworkAssemblyTypeInitializer).Assembly;

    public CodeDocumentationTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "OK. This \"Test\" generates our Markdown files.")]
    public void RunMarkdownCodeDocGenerator()
    {
        // Arrange
        var assemblyName = typeof(CodeDocumentationTests).Assembly.GetName().Name;
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var index = baseDirectory!.IndexOf(assemblyName!, StringComparison.Ordinal);
        var testProjectDirectory = new DirectoryInfo(baseDirectory.Substring(0, index + assemblyName!.Length));
        var rootDirectory = testProjectDirectory!.Parent!.Parent;
        var codeDocPath =
            Path.Combine(
                Path.Combine(
                    Path.Combine(rootDirectory!.FullName, "docs"), "CodeDoc"),
                sourceAssembly.GetName().Name!);
        var codeDocDirectory = new DirectoryInfo(codeDocPath);
        if (!codeDocDirectory.Exists)
        {
            codeDocDirectory.Create();
        }

        // Act & Assert
        MarkdownCodeDocGenerator.Run(sourceAssembly, codeDocDirectory);
    }
}