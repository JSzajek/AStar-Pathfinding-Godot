using Godot;

/// <summary>
/// Static class containing functions and constants relevant across
/// the application.
/// </summary>
public static class BuildEnvironment
{
    // The current build state.
    public static bool IsDebugBuild => !Engine.EditorHint;

    // The file save path of the application.
    public static readonly string SavePath = System.IO.Path.Combine(System.IO.Path.Combine(System.Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents"), "Architect Game/Saves/").Replace("\\", "/");

}