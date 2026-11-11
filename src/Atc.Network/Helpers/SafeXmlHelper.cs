namespace Atc.Network.Helpers;

/// <summary>
/// Loads XML that comes from the network (WS-Discovery answers, ONVIF responses) without trusting it:
/// no DTDs (so no entity expansion), no external resolving, and a size limit.
/// </summary>
internal static class SafeXmlHelper
{
    private const long MaxCharactersInDocument = 2_000_000;

    /// <summary>
    /// Loads the XML, or returns <see langword="false"/> when it is malformed, too large or has a DTD.
    /// </summary>
    /// <param name="xml">The XML text.</param>
    /// <param name="document">The document, when it could be loaded.</param>
    /// <returns><see langword="true"/> when the XML could be loaded.</returns>
    public static bool TryLoad(
        string xml,
        [NotNullWhen(true)] out XDocument? document)
    {
        document = null;
        if (string.IsNullOrWhiteSpace(xml))
        {
            return false;
        }

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = MaxCharactersInDocument,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
        };

        try
        {
            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            document = XDocument.Load(xmlReader);
            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the first descendant with the local name, whatever its namespace prefix.
    /// </summary>
    /// <param name="element">The element to search under.</param>
    /// <param name="localName">The local name.</param>
    /// <returns>The element, or <see langword="null"/>.</returns>
    public static XElement? Descendant(
        this XContainer element,
        string localName)
        => element.Descendants().FirstOrDefault(x => x.Name.LocalName == localName);

    /// <summary>
    /// Returns the trimmed value of the first descendant with the local name, or <see langword="null"/>.
    /// </summary>
    /// <param name="element">The element to search under.</param>
    /// <param name="localName">The local name.</param>
    /// <returns>The value, or <see langword="null"/>.</returns>
    public static string? DescendantValue(
        this XContainer element,
        string localName)
        => element.Descendant(localName)?.Value.Trim();
}