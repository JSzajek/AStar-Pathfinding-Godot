using Godot;

/// <summary>
/// Static class representing functionality to bridge file manipulations
/// between Godot and System as well as during Debug and Release modes.
/// </summary>
public static class FileSystem
{
    /// <summary>
    /// Ensures the validity of the passed file path. If found to be within
    /// release mode with incorrect filepath then unpacking of the file 
    /// will be attempted.
    /// </summary>
    /// <param name="filepath">The file path to check</param>
    /// <returns>The filepath converted when applicable</returns>
    public static string EnsureFilePath(string filepath) {
        if (BuildEnvironment.IsDebugBuild) {
            // Within the editor
            using (var file = new Godot.File()) {
                if (file.Open(filepath, File.ModeFlags.Read) == Error.Ok) {
                    return file.GetPathAbsolute();
                }
            }
            return filepath.Replace("res:/", System.IO.Directory.GetCurrentDirectory().Replace("\\", "/"));
        }
        else {
            // In release mode
            var convertedpath = filepath.Replace("res:/", System.IO.Directory.GetCurrentDirectory().Replace("\\", "/"));
            using (var dir = new Directory())
            using (var file = new Godot.File()) {
                var dirpath = System.IO.Path.GetDirectoryName(convertedpath).Replace("\\", "/");
                
                // Make sure the directory exists
                if (!dir.DirExists(dirpath)) {
                    dir.MakeDirRecursive(dirpath);
                }

                // Already unpacked?
                if (System.IO.File.Exists(convertedpath)) {
                    return convertedpath;
                }

                // Else check if it is packed and needs to be unpacked
                if (file.Open(filepath, File.ModeFlags.Read) == Error.Ok) {
                    dir.Copy(filepath, convertedpath);
                    file.Close();
                }
                return convertedpath;
            }
        }
    }

    /// <summary>
    /// Checks if the file exists at the passed filepath
    /// </summary>
    /// <param name="filepath">The file path to check</param>
    /// <returns>Whether the file exists</returns>
    public static bool FileExists(string filepath) {
        using (var file = new Godot.File()) 
        {
            return file.FileExists(filepath);
        }
    }
}