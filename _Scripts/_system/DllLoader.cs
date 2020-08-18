using Godot;
using System.Collections.Generic;
using AStar;

/// <summary>
/// DllLoader class representing loader for loading
/// in external dll libraries
/// </summary>
public class DllLoader : Node
{
    #region Fields

    private const string dllDirPath = "res://_Libs/dlls/";

    // Predefined of library types
    public enum LibType 
    {
        GRID_ASTAR, KDTREE_ASTAR, NAVMESH_ASTAR
    }

    // Loaded Libraries dictionary
    private Dictionary<LibType, Dll> dllLib;

    // Libraries file paths
    private Dictionary<LibType, string> dllPaths = new Dictionary<LibType, string>()
    {
        {LibType.GRID_ASTAR, dllDirPath + "Grid_AstarBinding.dll"},
        {LibType.KDTREE_ASTAR, dllDirPath + "KDTree_AstarBinding.dll"},
        {LibType.NAVMESH_ASTAR, dllDirPath + "NavmeshBinding.dll"}
    };

    #endregion Fields

    #region Contructors

    /// <summary>
	/// Initializes a new instance of the <see cref="DllLoader"/> class.
	/// </summary>
    public DllLoader()
    {
        dllLib = new Dictionary<LibType, Dll>();
    }

    #endregion Contructors

    #region Public Methods

    /// <summary>
    /// Performs memory managment freeing the dlls upon exiting the application.
    /// </summary>
    public override void _ExitTree()
    {
        Destroy();
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
                output = new GridAStarLinker(dllPaths[LibType.GRID_ASTAR]); 
                dllLib.Add(LibType.GRID_ASTAR, output);
                return output;
            case LibType.KDTREE_ASTAR:
                output = new KDTreeAStarLinker(dllPaths[LibType.KDTREE_ASTAR]);
                dllLib.Add(LibType.KDTREE_ASTAR, output);
                return output;
            case LibType.NAVMESH_ASTAR:
                output = new NavmeshAStarLinker(dllPaths[LibType.NAVMESH_ASTAR]);
                dllLib.Add(LibType.NAVMESH_ASTAR, output);
                return output;
        }
        return null; // Should never happen
    }

    /// <summary>
    /// Getter method to get an Grid AStar Linker library
    /// </summary>
    /// <returns>A Grid AStarLinker</returns>
    public GridAStarLinker GetGridAStarLinker()
    {
        return Load(LibType.GRID_ASTAR) as GridAStarLinker;
    }

    /// <summary>
    /// Getter method to get an KDTree Astar Linker library
    /// </summary>
    /// <returns>A KDTree AStarLinker</returns>
    public KDTreeAStarLinker GetKDTreeAStarLinker() {
        return Load(LibType.KDTREE_ASTAR) as KDTreeAStarLinker;
    }

    /// <summary>
    /// Getter method to get an Navmesh Astar Linker library
    /// </summary>
    /// <returns>A Navmesh AStarLinker</returns>
    public NavmeshAStarLinker GetNavmeshAStarLinker() {
        return Load(LibType.NAVMESH_ASTAR) as NavmeshAStarLinker;
    }

    #endregion Public Methods
}