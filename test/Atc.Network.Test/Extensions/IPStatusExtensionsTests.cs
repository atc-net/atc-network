// ReSharper disable StringLiteralTypo
namespace Atc.Network.Test.Extensions;

public class IPStatusExtensionsTests
{
    [Theory]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Success", IPStatus.Success)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Destination network unreachable", IPStatus.DestinationNetworkUnreachable)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Destination host unreachable", IPStatus.DestinationHostUnreachable)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Destination port unreachable", IPStatus.DestinationPortUnreachable)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Destination prohibited", IPStatus.DestinationProhibited)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "No resources", IPStatus.NoResources)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Bad option", IPStatus.BadOption)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Hardware error", IPStatus.HardwareError)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Packet is too big", IPStatus.PacketTooBig)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Timed out", IPStatus.TimedOut)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Bad route", IPStatus.BadRoute)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "TTL expired", IPStatus.TtlExpired)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "TTL reassembly time exceeded", IPStatus.TtlReassemblyTimeExceeded)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Parameter problem", IPStatus.ParameterProblem)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Source quench", IPStatus.SourceQuench)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Bad destination", IPStatus.BadDestination)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Destination unreachable", IPStatus.DestinationUnreachable)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Time exceeded", IPStatus.TimeExceeded)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Bad header", IPStatus.BadHeader)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Unrecognized next header", IPStatus.UnrecognizedNextHeader)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "ICMP error", IPStatus.IcmpError)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Destination scope mismatch", IPStatus.DestinationScopeMismatch)]
    [InlineData(GlobalizationLcidConstants.UnitedStates, "Unknown", IPStatus.Unknown)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Succes", IPStatus.Success)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Destinations netværket er ikke tilgængeligt", IPStatus.DestinationNetworkUnreachable)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Destination er ikke til at få fat på", IPStatus.DestinationHostUnreachable)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Destination kan ikke nås på port", IPStatus.DestinationPortUnreachable)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Destination forbudt", IPStatus.DestinationProhibited)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Ingen ressourcer", IPStatus.NoResources)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Dårlig indstilling", IPStatus.BadOption)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Hardware fejl", IPStatus.HardwareError)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Pakken er for stor", IPStatus.PacketTooBig)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Timeout", IPStatus.TimedOut)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Dårlig rute", IPStatus.BadRoute)]
    [InlineData(GlobalizationLcidConstants.Denmark, "TTL udløb", IPStatus.TtlExpired)]
    [InlineData(GlobalizationLcidConstants.Denmark, "TTL gensamlingstiden er overskredet", IPStatus.TtlReassemblyTimeExceeded)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Parameter problem", IPStatus.ParameterProblem)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Kildeslukning", IPStatus.SourceQuench)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Dårlig destination", IPStatus.BadDestination)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Destination uopnåelig", IPStatus.DestinationUnreachable)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Tiden er overskredet", IPStatus.TimeExceeded)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Dårligt header", IPStatus.BadHeader)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Næste header er ikke genkendt", IPStatus.UnrecognizedNextHeader)]
    [InlineData(GlobalizationLcidConstants.Denmark, "ICMP fejl", IPStatus.IcmpError)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Destinations scope stemmer ikke overens", IPStatus.DestinationScopeMismatch)]
    [InlineData(GlobalizationLcidConstants.Denmark, "Ukendt", IPStatus.Unknown)]
    [InlineData(GlobalizationLcidConstants.Germany, "Erfolg", IPStatus.Success)]
    [InlineData(GlobalizationLcidConstants.Germany, "Zielnetzwerk nicht erreichbar", IPStatus.DestinationNetworkUnreachable)]
    [InlineData(GlobalizationLcidConstants.Germany, "Ziel-Host nicht erreichbar", IPStatus.DestinationHostUnreachable)]
    [InlineData(GlobalizationLcidConstants.Germany, "Zielport nicht erreichbar", IPStatus.DestinationPortUnreachable)]
    [InlineData(GlobalizationLcidConstants.Germany, "Ziel verboten", IPStatus.DestinationProhibited)]
    [InlineData(GlobalizationLcidConstants.Germany, "Keine Ressourcen", IPStatus.NoResources)]
    [InlineData(GlobalizationLcidConstants.Germany, "Schlechte Option", IPStatus.BadOption)]
    [InlineData(GlobalizationLcidConstants.Germany, "Hardwarefehler", IPStatus.HardwareError)]
    [InlineData(GlobalizationLcidConstants.Germany, "Paket ist zu groß", IPStatus.PacketTooBig)]
    [InlineData(GlobalizationLcidConstants.Germany, "Zeitüberschreitung", IPStatus.TimedOut)]
    [InlineData(GlobalizationLcidConstants.Germany, "Schlechte Route", IPStatus.BadRoute)]
    [InlineData(GlobalizationLcidConstants.Germany, "TTL abgelaufen", IPStatus.TtlExpired)]
    [InlineData(GlobalizationLcidConstants.Germany, "Die Zeit für den Zusammenbau des TTL wurde überschritten", IPStatus.TtlReassemblyTimeExceeded)]
    [InlineData(GlobalizationLcidConstants.Germany, "Parameterproblem", IPStatus.ParameterProblem)]
    [InlineData(GlobalizationLcidConstants.Germany, "Quellenlöschung", IPStatus.SourceQuench)]
    [InlineData(GlobalizationLcidConstants.Germany, "Schlechtes Ziel", IPStatus.BadDestination)]
    [InlineData(GlobalizationLcidConstants.Germany, "Ziel unerreichbar", IPStatus.DestinationUnreachable)]
    [InlineData(GlobalizationLcidConstants.Germany, "Zeit überschritten", IPStatus.TimeExceeded)]
    [InlineData(GlobalizationLcidConstants.Germany, "Schlechter Header", IPStatus.BadHeader)]
    [InlineData(GlobalizationLcidConstants.Germany, "Nächster Header nicht erkannt", IPStatus.UnrecognizedNextHeader)]
    [InlineData(GlobalizationLcidConstants.Germany, "ICMP-Fehler", IPStatus.IcmpError)]
    [InlineData(GlobalizationLcidConstants.Germany, "Nichtübereinstimmung des Zielbereichs", IPStatus.DestinationScopeMismatch)]
    [InlineData(GlobalizationLcidConstants.Germany, "Unbekannt", IPStatus.Unknown)]
    public void GetDescription(int arrangeUiLcid, string expected, IPStatus input)
    {
        // Arrange
        if (arrangeUiLcid > 0)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(arrangeUiLcid);
        }

        // Act
        var actual = input.GetLocalizedDescription();

        // Assert
        Assert.Equal(expected, actual);
    }
}