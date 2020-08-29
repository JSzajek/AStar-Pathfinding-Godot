using System;
using System.Runtime.InteropServices;

/// <summary>
/// Class representing the basic functionality of all
/// dlls including loading and unloading.
/// </summary>
public class Dll
{
    #region Delegates

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    protected delegate void releaseMemory(IntPtr arrayPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void nativeLibDestroy();

    #endregion Delegates

    #region Fields

    protected IntPtr pDll;
    protected releaseMemory _releaseMemory;
	private nativeLibDestroy _nativeLibDestroy;

    #endregion Fields

    #region Constructors

    /// <summary>
	/// Initializes a new instance of the <see cref="Dll"/> class.
	/// </summary>
    /// <param name="path">The path to the dll</param>
    public Dll(string path)
    {
        Load(FileSystem.EnsureFilePath(path));
		_nativeLibDestroy = (nativeLibDestroy)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "native_lib_destroy"), typeof(nativeLibDestroy));
    }

    #endregion Constructors
    
    #region Public Methods

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
        _nativeLibDestroy();
        
        if (!NativeMethods.FreeLibrary(pDll)) {
            throw (new Exception("AStar Binding dll not unloaded."));
        }
    }

    #endregion Public Methods
}