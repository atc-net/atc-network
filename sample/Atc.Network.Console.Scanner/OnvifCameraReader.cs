namespace Atc.Network.Console.Scanner;

/// <summary>
/// Reads the clock, model and stream URLs of an ONVIF camera.
/// </summary>
public static class OnvifCameraReader
{
    public const string UserNameVariable = "ATC_ONVIF_USERNAME";
    public const string PasswordVariable = "ATC_ONVIF_PASSWORD";

    /// <summary>
    /// Returns the ONVIF account: from <c>ATC_ONVIF_USERNAME</c> and <c>ATC_ONVIF_PASSWORD</c>, or asked for
    /// (the password hidden) when the console is interactive and the user wants it.
    /// </summary>
    /// <param name="cameraCount">The number of ONVIF cameras found.</param>
    /// <returns>The credential, or <see langword="null"/> to read only what needs none.</returns>
    public static NetworkCredential? GetCredential(int cameraCount)
    {
        var userName = Environment.GetEnvironmentVariable(UserNameVariable);
        if (!string.IsNullOrEmpty(userName))
        {
            return new NetworkCredential(userName, Environment.GetEnvironmentVariable(PasswordVariable));
        }

        if (!AnsiConsole.Profile.Capabilities.Interactive ||
            !AnsiConsole.Confirm($"Read the model and stream URLs of the {cameraCount} ONVIF camera(s)? This needs the cameras' ONVIF account.", defaultValue: false))
        {
            return null;
        }

        var promptUserName = AnsiConsole.Prompt(new TextPrompt<string>("ONVIF user name:"));
        var password = AnsiConsole.Prompt(new TextPrompt<string>("ONVIF password:").Secret().AllowEmpty());
        return new NetworkCredential(promptUserName, password);
    }

    /// <summary>
    /// Reads a camera. Never throws for a camera that fails; the error is in the result.
    /// </summary>
    /// <param name="camera">The camera, with an ONVIF device service URL.</param>
    /// <param name="credential">The ONVIF account, or <see langword="null"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What could be read.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK - a sample that reports every camera, even when one fails.")]
    public static async Task<OnvifCameraDetails> Read(
        CameraCandidate camera,
        NetworkCredential? credential,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(camera);

        var client = new OnvifClient(camera.OnvifDeviceServiceUrl!, credential);
        var details = new OnvifCameraDetails(camera, client.DeviceServiceUrl);

        try
        {
            details.Clock = await client.GetSystemDateAndTime(cancellationToken);
            details.ClockOffset = client.ClockOffset;

            if (credential is not null)
            {
                details.Information = await client.GetDeviceInformation(cancellationToken);
                details.Streams = await client.GetStreams(cancellationToken);
            }
        }
        catch (OnvifException ex) when (ex.IsNotAuthorized)
        {
            details.Error = "Not authorized: wrong ONVIF user name or password.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            details.Error = $"{ex.GetType().Name}: {ex.Message}";
        }

        return details;
    }
}