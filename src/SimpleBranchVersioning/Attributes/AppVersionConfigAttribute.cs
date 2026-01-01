namespace SimpleBranchVersioning;

/// <summary>
/// Configures the generated AppVersion class.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class AppVersionConfigAttribute : Attribute
{
    /// <summary>
    /// The namespace for the generated AppVersion class.
    /// If not specified, the root namespace of the consuming project is used.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// The name of the generated class. Defaults to "AppVersion".
    /// </summary>
    public string? ClassName { get; set; }
}
