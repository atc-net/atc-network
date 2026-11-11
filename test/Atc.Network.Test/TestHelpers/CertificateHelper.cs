namespace Atc.Network.Test.TestHelpers;

public static class CertificateHelper
{
    /// <summary>
    /// Creates a self-signed certificate for localhost and 127.0.0.1, valid for one hour.
    /// </summary>
    public static X509Certificate2 CreateSelfSignedCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddDnsName("localhost");
        sanBuilder.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(sanBuilder.Build());

        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddHours(1));

        // Round-trip through PFX so the private key is usable by the platform TLS stack.
        return X509CertificateLoader.LoadPkcs12(certificate.Export(X509ContentType.Pfx), password: null);
    }

    /// <summary>
    /// Returns a validation callback that trusts only the given certificate.
    /// </summary>
    /// <param name="trusted">The only certificate to accept.</param>
    public static RemoteCertificateValidationCallback TrustOnly(
        X509Certificate2 trusted)
    {
        ArgumentNullException.ThrowIfNull(trusted);

        var thumbprint = trusted.Thumbprint;
        return (_, certificate, _, _) => certificate is not null &&
                                         string.Equals(new X509Certificate2(certificate).Thumbprint, thumbprint, StringComparison.Ordinal);
    }
}