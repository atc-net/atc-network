namespace Atc.Network.Console.Scanner;

/// <summary>
/// Chooses the range to scan: from the command line, from a menu of this machine's networks, or the default.
/// </summary>
public static class RangeSelector
{
    public const string DefaultStart = "192.168.1.1";
    public const string DefaultEnd = "192.168.1.255";

    private const string DefaultChoice = "192.168.1.1 - 192.168.1.255  (default)";
    private const string CustomChoice = "Another range ...";

    /// <summary>
    /// Returns the range to scan.
    /// </summary>
    /// <param name="args">The command-line arguments: [startIp] [endIp].</param>
    /// <returns>The first and last address.</returns>
    public static (IPAddress StartIpAddress, IPAddress EndIpAddress) Select(
        string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length > 0)
        {
            return (IPAddress.Parse(args[0]), IPAddress.Parse(args.Length > 1 ? args[1] : args[0]));
        }

        var defaultRange = (IPAddress.Parse(DefaultStart), IPAddress.Parse(DefaultEnd));
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            return defaultRange;
        }

        var networks = LocalNetwork.GetAll();
        var choices = new List<string> { DefaultChoice };
        choices.AddRange(networks.Select(x => x.ToString()));
        choices.Add(CustomChoice);

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which [green]network[/] should be scanned?")
                .AddChoices(choices.Select(Markup.Escape)));

        if (choice == Markup.Escape(CustomChoice))
        {
            var start = AskAddress("First address", DefaultStart);
            var end = AskAddress("Last address", DefaultEnd);
            return (start, end);
        }

        var network = networks.FirstOrDefault(x => Markup.Escape(x.ToString()) == choice);
        return network?.Range ?? defaultRange;
    }

    private static IPAddress AskAddress(
        string title,
        string defaultValue)
        => IPAddress.Parse(AnsiConsole.Prompt(
            new TextPrompt<string>($"{title}:")
                .DefaultValue(defaultValue)
                .Validate(x => IPAddress.TryParse(x, out var address) && address.AddressFamily == AddressFamily.InterNetwork
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Enter an IPv4 address, such as 192.168.1.1[/]"))));
}