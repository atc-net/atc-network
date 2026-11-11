# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Restore packages
dotnet restore

# Build in Release mode
dotnet build -c Release --no-restore

# Run unit tests (excludes integration tests)
dotnet test -c Release --no-build --filter-query "/[category!=integration]"

# Run a single test class
dotnet test -c Release --filter-class "Atc.Network.Test.Helpers.PingHelperTests"

# Run a single test method
dotnet test -c Release --filter-method "Atc.Network.Test.Helpers.PingHelperTests.GetStatus_TimeoutInMs"

# Create NuGet package
dotnet pack -c Release --no-restore -o ./packages
```

## Architecture Overview

Atc.Network is a .NET 10 library for network communication and scanning. Core modules:

**TCP/UDP Clients & Servers** (`Tcp/`, `Udp/`):
- Interface-based design: `ITcpClient`/`TcpClient`, `ITcpServer`/`TcpServer`, etc.
- Event-driven: Connected, Disconnected, DataReceived, ConnectionStateChanged
- Configuration via `*Config` classes (e.g., `TcpClientConfig`, `TcpClientKeepAliveConfig`)

**IP Scanning** (`Internet/`):
- `IPScanner` - Scans IP ranges with ICMP ping, hostname/MAC resolution, vendor lookup, port scanning
- `IPPortScan` - Tests TCP/HTTP connectivity on specific ports
- Progress reporting via events

**Helpers** (`Helpers/`):
- `PingHelper` - ICMP operations
- `DnsLookupHelper` - DNS resolution
- `ArpHelper` - ARP table operations
- `MacAddressVendorLookupHelper` - MAC vendor database lookups

## Code Style

- Uses ATC coding rules (.editorconfig) - StyleCop, Roslyn analyzers
- File-scoped namespaces (`csharp_style_namespace_declarations = file_scoped`)
- `var` keyword preferred for all types
- Private fields: camelCase (no underscore prefix)
- Interfaces prefixed with `I`, generic type parameters with `T`
- Release builds treat warnings as errors

## Dependencies

- Atc base library for common utilities
- Microsoft.Extensions.Hosting/Logging.Abstractions for DI/logging patterns
- Test framework: xUnit v3 on Microsoft.Testing.Platform (see `global.json`), with Atc.Test and Atc.XUnit; shared test packages live in `test/Directory.Build.props`

## CI/CD

- `build.yml` (pushes and PRs to main): builds and runs unit tests with coverage on ubuntu, macos and windows
- `release-please.yml` (pushes to main): maintains the release PR; merging it tags a release and publishes to nuget.org via NuGet Trusted Publishing (OIDC)
- The version lives in `Directory.Build.props` between the `x-release-please` markers; release-please bumps it from Conventional Commits (`feat:` → minor, `fix:` → patch, `!`/`BREAKING CHANGE:` → major)
