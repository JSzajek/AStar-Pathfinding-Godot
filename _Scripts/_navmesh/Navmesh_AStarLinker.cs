using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// Navmesh AStarLinker dll class representing an 
/// library for AStar pathfinding functionality.
/// </summary>
public class Navmesh_AStarLinker : Dll, IAStarLinker {
	
	#region Delegates

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void setupGrid();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void startMesh([In][MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.R4)] float[,] points, int d1);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void clipEdge([In][MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.R4)] float[,] points, int d1);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void clipHole([In][MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.R4)] float[,] points, int d1);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr getDebugMesh(int index);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void endMesh();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr findPath(float startX, float startY, float startZ, float endX, float endY, float endZ);
	
	#endregion Delegates

	#region  Private Parameters

	private setupGrid setup;
	private startMesh start;
	private clipEdge clipEdgePoly;
	private clipHole clipHolePoly;
	private getDebugMesh debugMesh;
	private endMesh end;
	private findPath path;

	#endregion Private Parameters

	/// <summary>
	/// Constructor initializing delegate methods of the library
	/// </summary>
	/// <param name="filePath">The file path to the dll</param>
	/// <returns></returns>
	public Navmesh_AStarLinker(string filePath) : base(filePath)
	{
		setup = (setupGrid)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "setup"), typeof(setupGrid));
		start = (startMesh)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "startMesh"), typeof(startMesh));
		clipEdgePoly = (clipEdge)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "clipEdge"), typeof(clipEdge));
		clipHolePoly = (clipHole)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "clipHole"), typeof(clipHole));
		debugMesh = (getDebugMesh)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "getDebugMesh"), typeof(getDebugMesh));
		end = (endMesh)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "endMesh"), typeof(endMesh));
		path = (findPath)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "getPath"), typeof(findPath));
	}

	/// <summary>
	/// Set up grid method
	/// </summary>
	public void setUpGrid()
	{
		setup();
	}

	/// <summary>
	/// Starts the creation of a navigation mesh with passed edge vertices
	/// </summary>
	/// <param name="vertices">The edge vertices of the new navmesh</param>
	public void startNavMesh(List<Vector3> vertices) {
		List<float[]> verts = new List<float[]>();
		foreach (var v in vertices) {
			verts.Add(new [] {v.x, v.y, v.z});
		}
		start(verts.ToArray().To2D(), vertices.Count);
	}

	/// <summary>
	/// Clips an edge polygon from the current navmesh
	/// </summary>
	/// <param name="vertices">The edgevertices of the edge polygon</param>
	public void clipEdgePolygon(List<Vector3> vertices) {
		List<float[]> verts = new List<float[]>();
		foreach (var v in vertices) {
			verts.Add(new [] {v.x, v.y, v.z});
		}
		clipEdgePoly(verts.ToArray().To2D(), vertices.Count);
	}

	/// <summary>
	/// Clips an hole polygon from the current navmesh
	/// </summary>
	/// <param name="vertices">The edgevertices of the hole polygon</param>
	public void clipHolePolygon(List<Vector3> vertices) {
		List<float[]> verts = new List<float[]>();
		foreach (var v in vertices) {
			verts.Add(new [] {v.x, v.y, v.z});
		}
		clipHolePoly(verts.ToArray().To2D(), vertices.Count);
	}

	/// <summary>
	/// Gets the debug triangles from the navmesh stored at the passed index
	/// </summary>
	/// <param name="index">The index of the desired navmesh</param>
	/// <returns>The triangles of the navmesh</returns>
	public Triangle[] getDebug(int index = 0) {
		IntPtr pathPtr = debugMesh(index);

		float[] sizeArray = new float[1];
		Marshal.Copy(pathPtr, sizeArray, 0, 1);
		int size = (int)sizeArray[0];

		
		float[] points = new float[size];
		Marshal.Copy(pathPtr, points, 0, size);

		Triangle[] triangles = new Triangle[(size-1) / 9];
		for (int i = 1; i < size; i += 9) {
			var A = new Vector3(points[i], points[i + 1], points[i + 2]);
			var B = new Vector3(points[i + 3], points[i + 4], points[i + 5]);
			var C = new Vector3(points[i + 6], points[i + 7], points[i + 8]);
			triangles[i/9] = new Triangle(new Vertex(A, 0), new Vertex(B, 1), new Vertex(C, 2));
		}
		return triangles;
	}

	/// <summary>
	/// Endes the creation navmesh and triggers cleanup and triangulation
	/// </summary>
	public void endNavMesh() {
		end();
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
			IntPtr pathPtr = path(start.x, start.y, start.z, end.x, end.y, end.z);
		
			float[] sizeArray = new float[1];
			Marshal.Copy(pathPtr, sizeArray, 0, 1);
			int size = (int)sizeArray[0];

			if (size <= 1)
			{
				// TODO: Fix Empty Path Bug -- requester becomes stuck on an unwalkable node
				callback(new PathResult(null, false, request.hash, request.callback));
				return;
			}
			
			float[] points = new float[size];
			Marshal.Copy(pathPtr, points, 0, size);

			Vector3[] waypoints = new Vector3[(size-1) / 3];
			for (int i = 1; i < size; i += 3) {
				waypoints[i/3] = new Vector3(points[i], points[i + 1], points[i + 2]);
			}

			waypoints[0] = request.pathStart;
			waypoints[waypoints.Length - 1] = request.pathEnd;

			callback(new PathResult(new Path(waypoints), true, request.hash, request.callback));
		}
	}
}