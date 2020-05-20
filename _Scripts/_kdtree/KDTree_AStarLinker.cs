using Godot;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// KDTree AStarLinker dll class representing an 
/// library for AStar pathfinding functionality.
/// </summary>
public class KDTree_AStarLinker : Dll, IAStarLinker {
	
	#region Delegates

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void setupGrid(float nodeRadius, int _minPenalty, int _maxPenalty);
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void addGridPoint(float pointX, float pointY, float pointZ, int gridX, int gridY, bool walkable, int movePenalty);
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void addGridPoints([In][MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.R4)] float[,] points, int d1, int d2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr getGridPoint(float pointX, float pointY, float pointZ);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr getNeighbors(float pointX, float pointY, float pointZ);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr getPathList(float startX, float startY, float startZ, float endX, float endY, float endZ, bool smooth, float turnDist, float stopDist);

	//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	//private delegate IntPtr blurWeights(int blurSize);

	#endregion Delegates

	#region  Private Parameters

	private setupGrid setup;
	private addGridPoint add;
	private addGridPoints addMany;
	private getGridPoint get;
	private getNeighbors getNN;
	private getPathList path;
	//private blurWeights blur;

	#endregion Private Parameters

	/// <summary>
	/// Constructor initializing delegate methods of the library
	/// </summary>
	/// <param name="filePath">The file path to the dll</param>
	/// <returns></returns>
	public KDTree_AStarLinker(string filePath) : base(filePath)
	{
		setup = (setupGrid)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "setup"), typeof(setupGrid));
		add = (addGridPoint)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "addPoint"), typeof(addGridPoint));
		addMany = (addGridPoints)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "addPoints"), typeof(addGridPoints));
		get = (getGridPoint)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "getPoint"), typeof(getGridPoint));
		getNN = (getNeighbors)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "getNearestNeighbors"), typeof(getNeighbors));
		path = (getPathList)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "path"), typeof(getPathList));
		//blur = (blurWeights)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "blur"), typeof(blurWeights));
	}

	/// <summary>
	/// Set up grid method
	/// </summary>
	/// <param name="sizeX">The width of the grid space</param>
	/// <param name="sizeY">The depth of the grid space</param>
	/// <param name="_minPenalty">The minimum movement penalty</param>
	/// <param name="_maxPenalty">The maximum movement penalty</param>
	public void setUpGrid(float _nodeRadius, int _minPenalty, int _maxPenalty)
	{
		setup(_nodeRadius, _minPenalty, _maxPenalty);
	}

	/// <summary>
	/// Adds a point to the grid at a specific position
	/// </summary>
	/// <param name="point">The point to add</param>
	/// <param name="grid">The position in the grid to add the point</param>
	/// <param name="walkable">Whether the point is walkable</param>
	/// <param name="movePenalty">The movement penalty of the point</param>
	public void addPoint(Vector3 point, Vector2 grid, bool walkable, int movePenalty)
	{
		add(point.x, point.y, point.z, (int)grid.x, (int)grid.y, walkable, movePenalty);
	}

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

	public Vector3[] getNearNeighbors(Vector3 position) {
		IntPtr neighborPtr = getNN(position.x, position.y, position.z);
		float[] sizeArray = new float[1];
		Marshal.Copy(neighborPtr, sizeArray, 0, 1);
		int size = (int)sizeArray[0];

		if (size == 1) {
			return new Vector3[0];
		}

		float[] points = new float[size];
		Marshal.Copy(neighborPtr, points, 0, size);

		Vector3[] waypoints = new Vector3[(size-1) / 3];
		for (int i = 1; i < size; i += 3) {
			waypoints[i/3] = new Vector3(points[i], points[i + 1], points[i+2]);
		}
		return waypoints;
	}

	/// <summary>
	/// Gets a path based on passed path request and returns a path
	/// result to the passed callback.
	/// </summary>
	/// <param name="request">The path request repsenting the desired path</param>
	/// <param name="callback">The callback to return the result to</param>
	public void getPath(PathRequest request, Action<PathResult> callback)
	{
		lock(path) {
			(var start, var end) = (request.pathStart, request.pathEnd);
			IntPtr pathPtr = path(start.x, start.y, start.z, end.x, end.y, end.z, request.smooth, request.turnDist, request.stopDist);

			float[] sizeArray = new float[1];
			Marshal.Copy(pathPtr, sizeArray, 0, 1);
			int size = (int)sizeArray[0];
			if (size == 1)
			{
				// TODO: Fix Empty Path Bug -- requester becomes stuck on an unwalkable node
				callback(new PathResult(null, false, request.hash, request.callback));
			}

			
			float[] points = new float[size];
			Marshal.Copy(pathPtr, points, 0, size);
			
			if (request.smooth) {
				var finishIndex = (int)points[1];
				var slowIndex = (int)points[2];

				Vector3[] waypoints = new Vector3[(size-3) / 10];
				Line[] lines = new Line[(size-3) / 10];
				for (int i = 3; i < size; i += 10) {
					waypoints[i/10] = new Vector3(points[i], points[i + 1], points[i+2]);
					lines[i/10] = new Line(points[i+3], points[i+4], new Vector2(points[i+5], points[i+6]), new Vector2(points[i+7], points[i+8]), (int)points[i+9]);
				}
				callback(new PathResult(new SmoothPath(waypoints, lines, finishIndex, slowIndex), true, request.hash, request.callback));
			} 
			else {
				Vector3[] waypoints = new Vector3[(size-1) / 3];
				for (int i = 1; i < size; i += 3) {
					waypoints[i/3] = new Vector3(points[i], points[i + 1], points[i+2]);
				}
				callback(new PathResult(new Path(waypoints), true, request.hash, request.callback));
			}
				
		}
	}

	/// <summary>
	/// Blurs the weights of the grid map to represent smoother
	/// transition based on the passed blur size.
	/// </summary>
	/// <param name="blurSize">The window size used to blur the weights</param>
	/// <returns>A tuple containing the new minimum and maximum movement penalties</returns>
	// public (int, int) blurMap(int blurSize)
	// {
	// 	var blurPtr = blur(blurSize);
	// 	int[] returnArray = new int[2];
	// 	Marshal.Copy(blurPtr, returnArray, 0, 2);
	// 	return (returnArray[0], returnArray[1]);
	// }
}