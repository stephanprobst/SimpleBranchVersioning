using MyApp.Versioning;

[assembly: SimpleBranchVersioning.AppVersionConfig(Namespace = "MyApp.Versioning", ClassName = "BuildInfo")]

Console.WriteLine($"Version:  {BuildInfo.Version}");
Console.WriteLine($"Branch:   {BuildInfo.Branch}");
Console.WriteLine($"CommitId: {BuildInfo.CommitId}");
