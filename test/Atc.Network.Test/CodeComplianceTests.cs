// ReSharper disable once NotAccessedField.Local
namespace Atc.Network.Test;

public class CodeComplianceTests
{
    [SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "OK.")]
    private readonly ITestOutputHelper testOutputHelper;
    private readonly Assembly sourceAssembly = typeof(AtcNetworkAssemblyTypeInitializer).Assembly;
    private readonly Assembly testAssembly = typeof(CodeComplianceTests).Assembly;

    private readonly List<Type> excludeTypes = new()
    {
        // TODO: Imp. missing test for:
        typeof(TcpClient),
        typeof(TcpServer),
        typeof(IPv4AddressHelper),
        typeof(MacAddressVendorLookupHelper),
        typeof(PingHelper),
        typeof(IPPortScan),
        typeof(IPScanner),
        typeof(DnsLookupHelper),
        typeof(InternetWorldTimeHelper),
        typeof(IPAddressExtensions),
        typeof(UshortExtensions),
        typeof(UdpClient),
        typeof(UdpServer),
    };

    public CodeComplianceTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void AssertExportedMethodsWithMissingTests_AbstractSyntaxTree()
    {
        // Act & Assert
        CodeComplianceTestHelper.AssertExportedMethodsWithMissingTests(
            DecompilerType.AbstractSyntaxTree,
            sourceAssembly,
            testAssembly,
            excludeTypes);
    }

    [Fact]
    public void AssertExportedMethodsWithMissingTests_MonoReflection()
    {
        // Act & Assert
        CodeComplianceTestHelper.AssertExportedMethodsWithMissingTests(
            DecompilerType.MonoReflection,
            sourceAssembly,
            testAssembly,
            excludeTypes);
    }

    [Fact]
    public void AssertExportedTypesWithMissingComments()
    {
        // Act & Assert
        CodeComplianceDocumentationHelper.AssertExportedTypesWithMissingComments(
            sourceAssembly);
    }

    [Fact]
    public void AssertExportedTypesWithWrongNaming()
    {
        // Act & Assert
        CodeComplianceHelper.AssertExportedTypesWithWrongDefinitions(
            sourceAssembly,
            excludeTypes);
    }
}