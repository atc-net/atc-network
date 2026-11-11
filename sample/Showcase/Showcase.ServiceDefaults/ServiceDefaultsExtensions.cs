namespace Showcase.ServiceDefaults;

/// <summary>
/// Shared setup for the showcase apps.
/// </summary>
public static class ServiceDefaultsExtensions
{
    /// <summary>
    /// The meter and activity source names of Atc.Network. The wildcard also matches names
    /// such as "Atc.Network.Tcp", so the library's metrics and traces show up as soon as it emits them.
    /// </summary>
    public const string AtcNetworkInstrumentationName = "Atc.Network*";

    /// <summary>
    /// Adds OpenTelemetry logs, metrics and traces. They are exported over OTLP when
    /// OTEL_EXPORTER_OTLP_ENDPOINT is set, which Aspire does for the apps it runs.
    /// </summary>
    /// <typeparam name="TBuilder">The host builder type.</typeparam>
    /// <param name="builder">The host builder.</param>
    /// <returns>The same builder.</returns>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });

        var openTelemetry = builder.Services
            .AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddRuntimeInstrumentation()
                .AddMeter(AtcNetworkInstrumentationName))
            .WithTracing(tracing => tracing
                .AddSource(AtcNetworkInstrumentationName));

        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            openTelemetry.UseOtlpExporter();
        }

        return builder;
    }

    /// <summary>
    /// Returns the endpoint of another showcase app, as passed by Aspire with WithReference
    /// (configuration key services:{serviceName}:{endpointName}:0, for example "tcp://localhost:9100").
    /// "localhost" is resolved to 127.0.0.1, because the servers listen on IPv4.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="serviceName">The Aspire resource name of the server.</param>
    /// <param name="endpointName">The endpoint name.</param>
    /// <param name="fallback">The endpoint to use when the app runs without Aspire.</param>
    /// <returns>The server's endpoint.</returns>
    public static IPEndPoint GetServiceEndpoint(
        this IConfiguration configuration,
        string serviceName,
        string endpointName,
        IPEndPoint fallback)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var value = configuration[$"services:{serviceName}:{endpointName}:0"];
        if (string.IsNullOrEmpty(value) ||
            !Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return fallback;
        }

        var address = string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            ? IPAddress.Loopback
            : IPAddress.Parse(uri.Host);

        return new IPEndPoint(address, uri.Port);
    }
}