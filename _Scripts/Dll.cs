using Godot;
using System;

/// <summary>
/// Class representing the basic functionality of all
/// dlls including loading and unloading.
/// </summary>
public class Dll
{
    protected IntPtr pDll;

    /// <summary>
    /// Constructor Initializing the dll by loading
    /// </summary>
    /// <param name="path">The path to the dll</param>
    public Dll(string path)
    {
        if (GetAbsPath(path) is String absPath)
        {
            Load(absPath);
        }
    }
    
    /// <summary>
    /// Loads the dll library from the passed absolute file path
    /// </summary>
    /// <param name="absPath">The file path of the dll</param>
    public void Load(string absPath)
    {
        pDll = NativeMethods.LoadLibrary(absPath);
        if (pDll == IntPtr.Zero) {
            throw (new Exception("AStar Binding dll not found."));
        }
    }

    /// <summary>
    /// Unloads the dll.
    /// </summary>
    public void Unload()
    {
        if (!NativeMethods.FreeLibrary(pDll)) {
            throw (new Exception("AStar Binding dll not unloaded."));
        }
    }

    /// <summary>
    /// Helper method to find the absolute system-based file path 
    /// </summary>
    /// <param name="localPath"></param>
    /// <returns></returns>
    private string GetAbsPath(string localPath)
    {
        using (var file = new File())
        {
            if (file.Open(localPath, File.ModeFlags.Read) == Error.Ok)
            {
                var filePath = file.GetPathAbsolute();
                file.Close();
                return filePath;
            }
        }
        return null;
    }
}