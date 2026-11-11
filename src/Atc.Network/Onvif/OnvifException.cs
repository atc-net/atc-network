namespace Atc.Network.Onvif;

/// <summary>
/// An ONVIF request failed: the device answered with a SOAP fault, an HTTP error, or a response that could not be read.
/// </summary>
public sealed class OnvifException : Exception
{
    public OnvifException()
    {
    }

    public OnvifException(string message)
        : base(message)
    {
    }

    public OnvifException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }

    public OnvifException(
        string message,
        HttpStatusCode? statusCode,
        string? code,
        string? subcode,
        string? reason)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        Subcode = subcode;
        Reason = reason;
    }

    /// <summary>
    /// Gets the HTTP status code, when the device answered.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the SOAP fault code, such as "SOAP-ENV:Sender".
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// Gets the SOAP fault subcode, such as "ter:NotAuthorized".
    /// </summary>
    public string? Subcode { get; }

    /// <summary>
    /// Gets the fault reason, such as "Authority failure".
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// Gets a value indicating whether the device rejected the credentials.
    /// </summary>
    public bool IsNotAuthorized
        => StatusCode == HttpStatusCode.Unauthorized ||
           (Subcode?.EndsWith("NotAuthorized", StringComparison.OrdinalIgnoreCase) ?? false);
}