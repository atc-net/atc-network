namespace Atc.Network.Test.TestHelpers;

/// <summary>
/// ONVIF responses for <c>OnvifClient</c> tests. These need credentials on a real camera, so they follow the
/// ONVIF schema and the gSOAP style of the captured Tapo answers; replace them with real captures when a camera
/// account is available. Unauthenticated captures are in <see cref="OnvifFixtures"/>.
/// </summary>
internal static class OnvifClientFixtures
{
    private const string EnvelopeStart = """<?xml version="1.0" encoding="UTF-8"?><SOAP-ENV:Envelope xmlns:SOAP-ENV="http://www.w3.org/2003/05/soap-envelope" xmlns:tt="http://www.onvif.org/ver10/schema" xmlns:tds="http://www.onvif.org/ver10/device/wsdl" xmlns:trt="http://www.onvif.org/ver10/media/wsdl"><SOAP-ENV:Body>""";
    private const string EnvelopeEnd = "</SOAP-ENV:Body></SOAP-ENV:Envelope>";

    public const string GetDeviceInformationResponse = EnvelopeStart + """<tds:GetDeviceInformationResponse><tds:Manufacturer>TP-Link</tds:Manufacturer><tds:Model>C210</tds:Model><tds:FirmwareVersion>1.3.11 Build 231115</tds:FirmwareVersion><tds:SerialNumber>00000000</tds:SerialNumber><tds:HardwareId>2.0</tds:HardwareId></tds:GetDeviceInformationResponse>""" + EnvelopeEnd;

    public const string GetCapabilitiesResponse = EnvelopeStart + """<tds:GetCapabilitiesResponse><tds:Capabilities><tt:Device><tt:XAddr>http://192.168.1.132:2020/onvif/device_service</tt:XAddr></tt:Device><tt:Events><tt:XAddr>http://192.168.1.132:2020/onvif/service</tt:XAddr></tt:Events><tt:Imaging><tt:XAddr>http://192.168.1.132:2020/onvif/service</tt:XAddr></tt:Imaging><tt:Media><tt:XAddr>http://192.168.1.132:2020/onvif/media_service</tt:XAddr></tt:Media><tt:PTZ><tt:XAddr>http://192.168.1.132:2020/onvif/ptz_service</tt:XAddr></tt:PTZ></tds:Capabilities></tds:GetCapabilitiesResponse>""" + EnvelopeEnd;

    public const string GetProfilesResponse = EnvelopeStart + """<trt:GetProfilesResponse><trt:Profiles token="profile_1" fixed="true"><tt:Name>mainStream</tt:Name><tt:VideoEncoderConfiguration token="main"><tt:Name>VideoEncoder_1</tt:Name><tt:Encoding>H264</tt:Encoding><tt:Resolution><tt:Width>2304</tt:Width><tt:Height>1296</tt:Height></tt:Resolution><tt:RateControl><tt:FrameRateLimit>15</tt:FrameRateLimit><tt:BitrateLimit>2048</tt:BitrateLimit></tt:RateControl></tt:VideoEncoderConfiguration></trt:Profiles><trt:Profiles token="profile_2" fixed="true"><tt:Name>minorStream</tt:Name><tt:VideoEncoderConfiguration token="minor"><tt:Name>VideoEncoder_2</tt:Name><tt:Encoding>H264</tt:Encoding><tt:Resolution><tt:Width>640</tt:Width><tt:Height>360</tt:Height></tt:Resolution><tt:RateControl><tt:FrameRateLimit>15</tt:FrameRateLimit><tt:BitrateLimit>256</tt:BitrateLimit></tt:RateControl></tt:VideoEncoderConfiguration></trt:Profiles></trt:GetProfilesResponse>""" + EnvelopeEnd;

    public const string GetNetworkInterfacesResponse = EnvelopeStart + """<tds:GetNetworkInterfacesResponse><tds:NetworkInterfaces token="eth0"><tt:Enabled>true</tt:Enabled><tt:Info><tt:Name>eth0</tt:Name><tt:HwAddress>00:11:22:33:44:55</tt:HwAddress><tt:MTU>1500</tt:MTU></tt:Info><tt:IPv4><tt:Enabled>true</tt:Enabled><tt:Config><tt:FromDHCP><tt:Address>192.168.1.140</tt:Address><tt:PrefixLength>24</tt:PrefixLength></tt:FromDHCP><tt:DHCP>true</tt:DHCP></tt:Config></tt:IPv4><tt:Extension><tt:InterfaceType>6</tt:InterfaceType></tt:Extension></tds:NetworkInterfaces><tds:NetworkInterfaces token="wlan0"><tt:Enabled>true</tt:Enabled><tt:Info><tt:Name>wlan0</tt:Name><tt:HwAddress>00:11:22:33:44:66</tt:HwAddress></tt:Info><tt:IPv4><tt:Enabled>true</tt:Enabled><tt:Config><tt:Manual><tt:Address>192.168.1.132</tt:Address><tt:PrefixLength>24</tt:PrefixLength></tt:Manual><tt:DHCP>false</tt:DHCP></tt:Config></tt:IPv4><tt:Extension><tt:InterfaceType>71</tt:InterfaceType><tt:Dot11><tt:SSID>SG9tZQ==</tt:SSID></tt:Dot11></tt:Extension></tds:NetworkInterfaces></tds:GetNetworkInterfacesResponse>""" + EnvelopeEnd;

    public static string GetStreamUriResponse(string uri)
        => EnvelopeStart + $"<trt:GetStreamUriResponse><trt:MediaUri><tt:Uri>{uri}</tt:Uri><tt:InvalidAfterConnect>false</tt:InvalidAfterConnect><tt:InvalidAfterReboot>false</tt:InvalidAfterReboot><tt:Timeout>PT60S</tt:Timeout></trt:MediaUri></trt:GetStreamUriResponse>" + EnvelopeEnd;

    public static string GetSystemDateAndTimeResponse(DateTime utc)
        => EnvelopeStart + $"<tds:GetSystemDateAndTimeResponse><tds:SystemDateAndTime><tt:DateTimeType>NTP</tt:DateTimeType><tt:UTCDateTime><tt:Time><tt:Hour>{utc.Hour}</tt:Hour><tt:Minute>{utc.Minute}</tt:Minute><tt:Second>{utc.Second}</tt:Second></tt:Time><tt:Date><tt:Year>{utc.Year}</tt:Year><tt:Month>{utc.Month}</tt:Month><tt:Day>{utc.Day}</tt:Day></tt:Date></tt:UTCDateTime></tds:SystemDateAndTime></tds:GetSystemDateAndTimeResponse>" + EnvelopeEnd;
}