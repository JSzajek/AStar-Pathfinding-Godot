using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// DllLoader class representing loader for loading
/// in external dll libraries
/// </summary>
public class DllLoader : Node
{
    private const string dllDirPath = "res://_Libs/dlls/";

    // Predefined of library types
    public enum LibType 
    {
        GRID_ASTAR, KDTREE_ASTAR, NAVMESH_ASTAR
    }
    
    // Loaded Libraries dictionary
    private Dictionary<LibType, string> dllPaths {get;} = new Dictionary<LibType, string>()
    {
        {LibType.GRID_ASTAR, dllDirPath + "Grid_AstarBinding.dll"},
        {LibType.KDTREE_ASTAR, dllDirPath + "KDTree_AstarBinding.dll"}
    };

    private Dictionary<LibType, Dll> dllLib;

    /// <summary>
    /// Constructor for dllLoader initializing the libraries and file
    /// pathes for loading
    /// </summary>
    public DllLoader()
    {
        // Check to unpack the dll files 
        if (ProjectSettings.GlobalizePath("res://").Empty()) {
            UnPackDlls();
        }
        dllLib = new Dictionary<LibType, Dll>();
    }

    /// <summary>
    /// Unpacking method to do one time unpacking of dlls when
    /// outside editor.
    /// </summary>
    private void UnPackDlls() {
        // Only unpack if project has been exported and dll file has changed.
        using (var dir = new Directory()) {
            if (dir.Open(dllDirPath) == Error.Ok) {
                foreach(var key in dllPaths.Keys.ToList()) {
                    var dllPath = dllPaths[key];
                    using (var file = new Godot.File())
                    {
                        if (file.Open(dllPath, File.ModeFlags.Read) == Error.Ok) {
                            var newDirPath = OS.GetExecutablePath().GetBaseDir().Replace("\\", "/") + "/libs/";
                            var newPath = newDirPath + dllPath.Substr(dllPath.LastIndexOf("/") + 1, dllPath.Length - 1);
                            
                            dllPaths[key] = newPath; // Reset path to new unpacked path

                            // Only unpack if packed dll is newer                            
                            if (!dir.DirExists(newDirPath)) {
                                dir.MakeDir(newDirPath);
                            }
                            else {
                                if (file.Open(newPath, File.ModeFlags.Read) == Error.Ok) {
                                    if (file.GetModifiedTime(dllPath) < file.GetModifiedTime(newPath)) {
                                        continue; // No need to update the dll
                                    }
                                }
                            }
                            dir.Copy(dllPath, newPath);
                            file.Close();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Destructor method to unload libraries
    /// </summary>
    public void Destroy()
    {
        foreach(var lib in dllLib)
        {
            lib.Value.Unload();
        }
    }

    /// <summary>
    /// Loading method to load the passed library type
    /// </summary>
    /// <param name="type">The type of library to load</param>
    /// <returns>The loaded library as Dll</returns>
    public Dll Load(LibType type)
    {
        Dll output;
        if (dllLib.TryGetValue(type, out output))
        {   
            return output; 
        }
        switch(type)
        {
            case LibType.GRID_ASTAR:
                output = new Grid_AStarLinker(dllPaths[LibType.GRID_ASTAR]); 
                dllLib.Add(LibType.GRID_ASTAR, output);
                return output;
            case LibType.KDTREE_ASTAR:
                output = new KDTree_AStarLinker(dllPaths[LibType.KDTREE_ASTAR]);
                dllLib.Add(LibType.KDTREE_ASTAR, output);
                return output;
        }
        return null; // Should never happen
    }

    /// <summary>
    /// Getter method to get an Grid AStar Linker library
    /// </summary>
    /// <returns>A Grid AStarLinker</returns>
    public Grid_AStarLinker GetGridAStarLinker()
    {
        return Load(LibType.GRID_ASTAR) as Grid_AStarLinker;
    }

    /// <summary>
    /// Getter method to get an KDTree Astar Linker library
    /// </summary>
    /// <returns>A KDTree AStarLinker</returns>
    public KDTree_AStarLinker GetKDTreeAStarLinker() {
        return Load(LibType.KDTREE_ASTAR) as KDTree_AStarLinker;
    }
}