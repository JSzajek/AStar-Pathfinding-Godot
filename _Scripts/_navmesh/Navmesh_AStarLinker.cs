using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// KDTree AStarLinker dll class representing an 
/// library for AStar pathfinding functionality.
/// </summary>
public class Navmesh_AStarLinker : Dll, IAStarLinker {
	
	#region Delegates

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void setupGrid();
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void addGridPoint(float pointX, float pointY, float pointZ);
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void addGridPoints([In][MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.R4)] float[,] points, int d1, int d2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr getGridPoint(float pointX, float pointY, float pointZ);

	#endregion Delegates

	#region  Private Parameters

	private setupGrid setup;
	private addGridPoint add;
	private addGridPoints addMany;
	private getGridPoint get;
    private BiDirectionalSearch<Vector3> graph;

	#endregion Private Parameters

	/// <summary>
	/// Constructor initializing delegate methods of the library
	/// </summary>
	/// <param name="filePath">The file path to the dll</param>
	/// <returns></returns>
	public Navmesh_AStarLinker(string filePath) : base(filePath)
	{
		setup = (setupGrid)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "setup"), typeof(setupGrid));
		add = (addGridPoint)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "addPoint"), typeof(addGridPoint));
		addMany = (addGridPoints)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "addPoints"), typeof(addGridPoints));
		get = (getGridPoint)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "getPoint"), typeof(getGridPoint));
	}

	/// <summary>
	/// Set up grid method
	/// </summary>
	public void setUpGrid(IEnumerable<Vector3> points, IEnumerable<Tuple<Vector3, Vector3>> edges)
	{
        graph = new BiDirectionalSearch<Vector3>(points, edges);
        var data = new List<float[]>();
        foreach(var vec in points) {
            data.Add(new float[5] { vec.x, vec.y, vec.z, 1, 0});
        }
		setup();
        addPoints(data.ToArray().To2D());
	}

	/// <summary>
	/// Adds a point to the grid at a specific position
	/// </summary>
	/// <param name="point">The point to add</param>
	/// <param name="grid">The position in the grid to add the point</param>
	/// <param name="walkable">Whether the point is walkable</param>
	/// <param name="movePenalty">The movement penalty of the point</param>
	public void addPoint(Vector3 point)
	{
		add(point.x, point.y, point.z);
	}

	/// <summary>
	/// Adds a list of points to the grid.
	/// </summary>
	/// <param name="points">The points to be added</param>
	public void addPoints(float[,] points) 
	{
		addMany(points, points.GetLength(0), points.GetLength(1));
	}

	/// <summary>
	/// Gets a point from the grid
	/// </summary>
	/// <param name="gridX">The x coordinate of the grid</param>
	/// <param name="gridY">The y coordinate of the grid</param>
	/// <returns>A Tuple representing the grid point of world position movement pentalty and walkablity</returns>
	public (Vector3, int, bool) getPoint(Vector3  position)
	{
		IntPtr  pointPtr = get(position.x, position.y, position.z);
		float[] arr = new float[5];
		Marshal.Copy(pointPtr, arr, 0, 5);
		return (new Vector3(arr[0], arr[1], arr[2]), (int)arr[3], (int)arr[4] == 1);
	}

	/// <summary>
	/// Gets a path based on passed path request and returns a path
	/// result to the passed callback.
	/// </summary>
	/// <param name="request">The path request repsenting the desired path</param>
	/// <param name="callback">The callback to return the result to</param>
	public void getPath(PathRequest request, Action<PathResult> callback)
	{
		lock(graph) {
            var startPoint = getPoint(request.pathStart).Item1;
            var endPoint = getPoint(request.pathEnd).Item1;
           
            List<Vector3> waypoints = new List<Vector3>(graph.ShortestPath(startPoint, endPoint));
            waypoints[0] = request.pathStart;
		    waypoints[waypoints.Count - 1] = request.pathEnd;


			callback(new PathResult(new Path(waypoints.ToArray()), true, request.hash, request.callback));
		}
	}
}