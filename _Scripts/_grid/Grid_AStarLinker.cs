using Godot;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Grid AStarLinker dll class representing an 
/// library for AStar pathfinding functionality.
/// </summary>
public class Grid_AStarLinker : Dll, IAStarLinker {
	
	#region Delegates

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void setupGrid(int sizeX, int sizeY, int _minPenalty, int _maxPenalty);
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void addGridPoint(float pointX, float pointY, float pointZ, int gridX, int gridY, bool walkable, int movePenalty);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr getGridPoint(int gridX, int gridY);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr getPathList(float startX, float startY, float startZ, float endX, float endY, float endZ, bool smooth, float turnDist, float stopDist);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr blurWeights(int blurSize);

	#endregion Delegates

	#region  Private Parameters

	private setupGrid setup;
	private addGridPoint add;
	private getGridPoint get;
	private getPathList path;
	private blurWeights blur;

	#endregion Private Parameters

	/// <summary>
	/// Constructor initializing delegate methods of the library
	/// </summary>
	/// <param name="filePath">The file path to the dll</param>
	/// <returns></returns>
	public Grid_AStarLinker(string filePath) : base(filePath)
	{
		setup = (setupGrid)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "setup"), typeof(setupGrid));
		add = (addGridPoint)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "addPoint"), typeof(addGridPoint));
		get = (getGridPoint)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "getPoint"), typeof(getGridPoint));
		path = (getPathList)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "path"), typeof(getPathList));
		blur = (blurWeights)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "blur"), typeof(blurWeights));
	}

	/// <summary>
	/// Set up grid method
	/// </summary>
	/// <param name="sizeX">The width of the grid space</param>
	/// <param name="sizeY">The depth of the grid space</param>
	/// <param name="_minPenalty">The minimum movement penalty</param>
	/// <param name="_maxPenalty">The maximum movement penalty</param>
	public void setUpGrid(int sizeX, int sizeY, int _minPenalty, int _maxPenalty)
	{
		setup(sizeX, sizeY, _minPenalty, _maxPenalty);
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

	/// <summary>
	/// Gets a point from the grid
	/// </summary>
	/// <param name="gridX">The x coordinate of the grid</param>
	/// <param name="gridY">The y coordinate of the grid</param>
	/// <returns>A Tuple representing the grid point of world position movement pentalty and walkablity</returns>
	public (Vector3, int, bool) getPoint(int gridX, int gridY)
	{
		IntPtr  pointPtr = get(gridX, gridY);
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
	public (int, int) blurMap(int blurSize)
	{
		var blurPtr = blur(blurSize);
		int[] returnArray = new int[2];
		Marshal.Copy(blurPtr, returnArray, 0, 2);
		return (returnArray[0], returnArray[1]);
	}
}

/// <summary>
/// Struct representing a line and related methods
/// </summary>
public struct Line {
	public float gradient, perpGradient, yIntercept;
	public Vector2 pointLine1, pointLine2;
	public bool approachSide;

	/// <summary>
	/// Constructor initializing the Line.
	/// </summary>
	/// <param name="gradient">The gradient of the line</param>
	/// <param name="perpGradient">The perpendicular gradient of the line</param>
	/// <param name="point1">The point on the line</param>
	/// <param name="point2">The point perpendicular to the line</param>
	/// <param name="approachSide">The current approach side</param>
	public Line(float gradient, float perpGradient, Vector2 point1, Vector2 point2, int approachSide) {
		this.gradient = gradient;
		this.perpGradient = perpGradient;
		this.pointLine1 = point1;
		this.pointLine2 = point2;
		this.approachSide = approachSide == 1;
		yIntercept = point1.y - gradient * point1.x;
	}

	/// <summary>
	/// Getter method that gets the side of the passed point.
	/// </summary>
	/// <param name="point">The passed point</param>
	/// <returns>The side the point represented as a bool (infront or behind)</returns>
	public bool GetSide(Vector2 point) {
		return (point.x - pointLine1.x) * (pointLine2.y - pointLine1.y) > (point.y - pointLine1.y) * (pointLine2.x - pointLine1.x);
	}

	/// <summary>
	/// Determines whether the passed point has crossed the line.
	/// </summary>
	/// <param name="point">The passed point</param>
	/// <returns>boolean of whether the point has crossed the line</returns>
	public bool HasCrossedLine(Vector2 point) {
		return GetSide(point) != approachSide;
	}

	/// <summary>
	/// Gets the distance of the line to the passed point.
	/// </summary>
	/// <param name="point">The passed point</param>
	/// <returns>The distance of the line to the point</returns>
	public float DistanceFromPoint(Vector2 point) {
		float yInterceptPerp = point.y - perpGradient * point.x;
		float xIntersect = (yInterceptPerp - yIntercept) / (gradient - perpGradient);
		float yIntersect = gradient * xIntersect + yIntercept;
		return point.DistanceTo(new Vector2(xIntersect, yIntersect));
	}
}
