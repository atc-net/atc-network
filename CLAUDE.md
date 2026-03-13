# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Restore packages
dotnet restore

# Build in Release mode
dotnet build -c Release --no-restore

# Run unit tests (excludes integration tests)
dotnet test -c Release --no-build --filter "Category!=Integration"

# Run a single test class
dotnet test -c Release --filter "FullyQualifiedName~Atc.Network.Test.PingHelperTests"

# Run a single test method
dotnet test -c Release --filter "FullyQualifiedName~Atc.Network.Test.PingHelperTests.Run_ShouldReturn_Succeed"

# Create NuGet package
dotnet pack -c Release --no-restore -o ./packages
```

## Architecture Overview

Atc.Network is a .NET 8 library for network communication and scanning. Core modules:

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
- Test framework: xUnit with Atc.XUnit

## CI/CD

- Pre-integration (PRs): Builds on ubuntu, macos, windows
- Post-integration (main): Tests, SonarCloud analysis, NuGet publish
- Uses Nerdbank.GitVersioning for versioning
