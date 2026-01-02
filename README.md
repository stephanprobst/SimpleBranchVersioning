# SimpleBranchVersioning

[![Build](https://github.com/stephanprobst/SimpleBranchVersioning/actions/workflows/build.yml/badge.svg)](https://github.com/stephanprobst/SimpleBranchVersioning/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/SimpleBranchVersioning.svg)](https://www.nuget.org/packages/SimpleBranchVersioning)

A .NET source generator that derives version information from Git branch names.

**Why use SimpleBranchVersioning?**

- **Zero configuration** - Install the package and it just works
- **Convention-based** - Version format follows your branch naming (e.g., `release/v1.2.3` becomes version `1.2.3`)
- **Automatic NuGet versioning** - Package versions are set automatically based on the branch
- **Compile-time generation** - Version info is embedded as constants, no runtime Git dependencies

## Installation

```shell
dotnet add package SimpleBranchVersioning
```

## Usage

After installing the package, a static `AppVersion` class is automatically generated:

```csharp
Console.WriteLine($"Version: {AppVersion.Version}");
Console.WriteLine($"Branch: {AppVersion.Branch}");
Console.WriteLine($"Commit: {AppVersion.CommitId}");
```

## Version Format

| Branch Pattern | Version | PackageVersion |
|----------------|---------|----------------|
| `release/v1.2.3` | `1.2.3` | `1.2.3+abc1234` |
| `release/1.2.3` | `1.2.3` | `1.2.3+abc1234` |
| `feature/login` | `feature.login.abc1234` | `0.0.0-feature.login+abc1234` |
| `main` | `main.abc1234` | `0.0.0-main+abc1234` |

## Generated Class

```csharp
public static class AppVersion
{
    public const string Version = "...";              // Display version
    public const string Branch = "...";               // Git branch name
    public const string CommitId = "...";             // Short commit ID
    public const string PackageVersion = "...";       // NuGet-compatible version
    public const string AssemblyVersion = "...";      // Assembly version (X.Y.Z.0)
    public const string FileVersion = "...";          // File version
    public const string InformationalVersion = "..."; // Full version with metadata
}
```

## Configuration

### MSBuild Properties

| Property | Default | Description |
|----------|---------|-------------|
| `SetPackageVersionFromBranch` | `true` | Automatically set NuGet package version from branch |
| `IncludeCommitIdMetadata` | `true` | Include commit ID as build metadata in versions |
| `GenerateVersionFile` | `false` | Generate `version.json` file during build |

### Custom Class Name and Namespace

```csharp
[assembly: SimpleBranchVersioning.AppVersionConfig(
    Namespace = "MyApp.Versioning",
    ClassName = "BuildInfo")]
```

## CI/CD Integration

Enable `GenerateVersionFile` to output version information during build:

```xml
<PropertyGroup>
    <GenerateVersionFile>true</GenerateVersionFile>
</PropertyGroup>
```

The file is written to `$(OutputPath)/version.json`.

## Requirements

- .NET SDK 8.0 or later
- Git repository

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
