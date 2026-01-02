using MyApp.Versioning;

[assembly: SimpleBranchVersioning.AppVersionConfig(Namespace = "MyApp.Versioning", ClassName = "BuildInfo")]

Console.WriteLine("=== SimpleBranchVersioning ConfiguredApp Demo ===");
Console.WriteLine();
Console.WriteLine("Basic Info:");
Console.WriteLine($"  Version:  {BuildInfo.Version}");
Console.WriteLine($"  Branch:   {BuildInfo.Branch}");
Console.WriteLine($"  CommitId: {BuildInfo.CommitId}");
Console.WriteLine();
Console.WriteLine("NuGet/Assembly Versions:");
Console.WriteLine($"  PackageVersion:       {BuildInfo.PackageVersion}");
Console.WriteLine($"  AssemblyVersion:      {BuildInfo.AssemblyVersion}");
Console.WriteLine($"  FileVersion:          {BuildInfo.FileVersion}");
Console.WriteLine($"  InformationalVersion: {BuildInfo.InformationalVersion}");
