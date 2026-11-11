namespace Atc.Network.Test.TestHelpers;

/// <summary>
/// ONVIF answers captured from real devices (TP-Link Tapo C210 and a Synology NAS). Device ids are replaced,
/// and unused namespace declarations removed. <c>{RelatesTo}</c> is replaced with the probe's message id.
/// </summary>
internal static class OnvifFixtures
{
    public const string TapoC210ProbeMatch = """
        <?xml version="1.0" encoding="UTF-8"?>
        <SOAP-ENV:Envelope xmlns:SOAP-ENV="http://www.w3.org/2003/05/soap-envelope" xmlns:wsa="http://schemas.xmlsoap.org/ws/2004/08/addressing" xmlns:wsdd="http://schemas.xmlsoap.org/ws/2005/04/discovery" xmlns:tdn="http://www.onvif.org/ver10/network/wsdl"><SOAP-ENV:Header><wsa:MessageID>{RelatesTo}</wsa:MessageID><wsa:RelatesTo>{RelatesTo}</wsa:RelatesTo><wsa:To SOAP-ENV:mustUnderstand="true">urn:schemas-xmlsoap-org:ws:2005:04:discovery</wsa:To><wsa:Action SOAP-ENV:mustUnderstand="true">http://schemas.xmlsoap.org/ws/2005/04/discovery/ProbeMatches</wsa:Action></SOAP-ENV:Header><SOAP-ENV:Body><wsdd:ProbeMatches><wsdd:ProbeMatch><wsa:EndpointReference><wsa:Address>uuid:00000000-0000-4000-8000-000000000132</wsa:Address><wsa:ReferenceProperties></wsa:ReferenceProperties><wsa:PortType>ttl</wsa:PortType></wsa:EndpointReference><wsdd:Types>tdn:NetworkVideoTransmitter</wsdd:Types><wsdd:Scopes>onvif://www.onvif.org/name/C210 onvif://www.onvif.org/hardware/C210 onvif://www.onvif.org/Profile/Streaming onvif://www.onvif.org/location/Hong%20Kong onvif://www.onvif.org/type/NetworkVideoTransmitter</wsdd:Scopes><wsdd:XAddrs>http://192.168.1.132:2020/onvif/device_service</wsdd:XAddrs><wsdd:MetadataVersion>1</wsdd:MetadataVersion></wsdd:ProbeMatch></wsdd:ProbeMatches></SOAP-ENV:Body></SOAP-ENV:Envelope>
        """;

    public const string NasEmptyProbeMatches = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope" xmlns:wsa="http://schemas.xmlsoap.org/ws/2004/08/addressing" xmlns:wsd="http://schemas.xmlsoap.org/ws/2005/04/discovery"><soap:Header><wsa:Action>http://schemas.xmlsoap.org/ws/2005/04/discovery/ProbeMatches</wsa:Action><wsa:MessageID>urn:uuid:00000000-0000-4000-8000-000000000212</wsa:MessageID><wsa:RelatesTo>{RelatesTo}</wsa:RelatesTo><wsa:To>http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</wsa:To><wsd:AppSequence InstanceId="95138242" MessageNumber="1"/></soap:Header><soap:Body><wsd:ProbeMatches/></soap:Body></soap:Envelope>
        """;

    public const string TapoGetSystemDateAndTimeResponse = """
        <?xml version="1.0" encoding="UTF-8"?>
        <SOAP-ENV:Envelope xmlns:SOAP-ENV="http://www.w3.org/2003/05/soap-envelope" xmlns:tt="http://www.onvif.org/ver10/schema" xmlns:tds="http://www.onvif.org/ver10/device/wsdl"><SOAP-ENV:Body><tds:GetSystemDateAndTimeResponse><tds:SystemDateAndTime><tt:DateTimeType>NTP</tt:DateTimeType><tt:DaylightSavings>true</tt:DaylightSavings><tt:TimeZone><tt:TZ>GMT-01:00</tt:TZ></tt:TimeZone><tt:UTCDateTime><tt:Time><tt:Hour>23</tt:Hour><tt:Minute>4</tt:Minute><tt:Second>46</tt:Second></tt:Time><tt:Date><tt:Year>2026</tt:Year><tt:Month>11</tt:Month><tt:Day>11</tt:Day></tt:Date></tt:UTCDateTime><tt:LocalDateTime><tt:Time><tt:Hour>0</tt:Hour><tt:Minute>4</tt:Minute><tt:Second>46</tt:Second></tt:Time><tt:Date><tt:Year>2026</tt:Year><tt:Month>11</tt:Month><tt:Day>12</tt:Day></tt:Date></tt:LocalDateTime></tds:SystemDateAndTime></tds:GetSystemDateAndTimeResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>
        """;

    public const string TapoNotAuthorizedFault = """
        <?xml version="1.0" encoding="UTF-8"?>
        <SOAP-ENV:Envelope xmlns:SOAP-ENV="http://www.w3.org/2003/05/soap-envelope" xmlns:ter="http://www.onvif.org/ver10/error"><SOAP-ENV:Body><SOAP-ENV:Fault><SOAP-ENV:Code><SOAP-ENV:Value>SOAP-ENV:Sender</SOAP-ENV:Value><SOAP-ENV:Subcode><SOAP-ENV:Value>ter:NotAuthorized</SOAP-ENV:Value></SOAP-ENV:Subcode></SOAP-ENV:Code><SOAP-ENV:Reason><SOAP-ENV:Text xml:lang="en">Authority failure</SOAP-ENV:Text></SOAP-ENV:Reason></SOAP-ENV:Fault></SOAP-ENV:Body></SOAP-ENV:Envelope>
        """;

    /// <summary>
    /// Returns the answer with the probe's message id as RelatesTo.
    /// </summary>
    /// <param name="answer">The answer, with <c>{RelatesTo}</c> placeholders.</param>
    /// <param name="messageId">The probe's message id.</param>
    /// <returns>The answer as UTF-8 bytes.</returns>
    public static byte[] RelatedTo(
        string answer,
        string messageId)
        => Encoding.UTF8.GetBytes(answer.Replace("{RelatesTo}", messageId, StringComparison.Ordinal));

    /// <summary>
    /// Reads the message id and the probed types of a WS-Discovery probe.
    /// </summary>
    /// <param name="probe">The probe.</param>
    /// <returns>The message id and the Types text.</returns>
    public static (string MessageId, string Types) ReadProbe(byte[] probe)
    {
        var document = System.Xml.Linq.XDocument.Parse(Encoding.UTF8.GetString(probe));
        var messageId = document.Descendants().First(x => x.Name.LocalName == "MessageID").Value;
        var types = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "Types")?.Value ?? string.Empty;
        return (messageId, types);
    }
}