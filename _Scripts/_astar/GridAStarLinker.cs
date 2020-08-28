using Godot;
using System;
using System.Collections.Generic;
using Pathing;
using System.Linq;
using System.Runtime.InteropServices;

namespace AStar
{
    /// <summary>
    /// Grid AStarLinker dll class representing an 
    /// library for AStar pathfinding functionality.
    /// </summary>
    public class GridAStarLinker : Dll, IAStarLinker
    {
        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void setupGrid(int sizeX, int sizeY, int _minPenalty, int _maxPenalty, float offsetX, float offsetY, float offsetZ);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void clearGrid();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void addGridPoint(float pointX, float pointY, float pointZ, int gridX, int gridY, bool walkable, int movePenalty);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void addGridPoints([In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)] float[] points, int d1);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr getGridPoint(int gridX, int gridY);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr getPathList(float startX, float startY, float startZ, float endX, float endY, float endZ, bool smooth, float turnDist, float stopDist);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr blurWeights(int blurSize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr exportGrid();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void importGrid([In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)] float[] points, int d1);

        #endregion Delegates

        #region Fields

        private setupGrid _setupGrid;
        private clearGrid _clearGrid;
        private addGridPoint _addGridPoint;
        private addGridPoints _addGridPoints;
        private getGridPoint _getGridPoint;
        private getPathList _getPath;
        private blurWeights _blurWeights;
        private exportGrid _exportGrid;
        private importGrid _importGrid;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GridAStarLinker"/> class.
        /// </summary>
        /// <param name="filePath">The file path to the dll</param>
        public GridAStarLinker(string filePath) : base(filePath)
        {
            _setupGrid = (setupGrid)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "setupGrid"), typeof(setupGrid));
            _clearGrid = (clearGrid)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "clearPoints"), typeof(clearGrid));
            _addGridPoint = (addGridPoint)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "addGridPoint"), typeof(addGridPoint));
            _addGridPoints = (addGridPoints)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "addGridPoints"), typeof(addGridPoints));
            _getGridPoint = (getGridPoint)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "getGridPoint"), typeof(getGridPoint));
            _getPath = (getPathList)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "path"), typeof(getPathList));
            _blurWeights = (blurWeights)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "blur"), typeof(blurWeights));

            _exportGrid = (exportGrid)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "exportGrid"), typeof(exportGrid));
            _importGrid = (importGrid)Marshal.GetDelegateForFunctionPointer(NativeMethods.GetProcAddress(pDll, "importGrid"), typeof(importGrid));


        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Set up grid method.
        /// </summary>
        /// <param name="sizeX">The width of the grid space</param>
        /// <param name="sizeY">The depth of the grid space</param>
        /// <param name="_minPenalty">The minimum movement penalty</param>
        /// <param name="_maxPenalty">The maximum movement penalty</param>
        public void setUpGrid(int sizeX, int sizeY, int _minPenalty, int _maxPenalty, Vector3 offset)
        {
            _setupGrid(sizeX, sizeY, _minPenalty, _maxPenalty, offset.x, offset.y, offset.z);
        }

        /// <summary>
        /// Clears the astar map.
        /// </summary>
        public void clear()
        {
            _clearGrid();
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
            _addGridPoint(point.x, point.y, point.z, (int)grid.x, (int)grid.y, walkable, movePenalty);
        }

        /// <summary>
        /// Adds a collection of points to the grid in one call.
        /// </summary>
        /// <param name="points">The points to add to the astar map</param>
        public void addPoints(List<float> points)
        {
            _addGridPoints(points.ToArray(), points.Count());
        }

        /// <summary>
        /// Gets a point from the grid
        /// </summary>
        /// <param name="gridX">The x coordinate of the grid</param>
        /// <param name="gridY">The y coordinate of the grid</param>
        /// <returns>A Tuple representing the grid point of world position movement pentalty and walkablity</returns>
        public (Vector3, int, bool) getPoint(int gridX, int gridY)
        {
            IntPtr pointPtr = _getGridPoint(gridX, gridY);
            float[] arr = new float[5];
            Marshal.Copy(pointPtr, arr, 0, 5);
            _releaseMemory(pointPtr);

            return (new Vector3(arr[0], arr[1], arr[2]), (int)arr[3], (int)arr[4] == 1);
        }

        /// <summary>
        /// Gets a path based on passed path request and returns a path
        /// result to the passed callback.
        /// </summary>
        /// <param name="request">The path request repsenting the desired path</param>
        /// <param name="callback">The callback to return the result to</param>
        public void GetPath(PathRequest request, Action<PathResult> callback)
        {
            lock (_getPath)
            {
                (var start, var end) = (request.pathStart, request.pathEnd);
                IntPtr pathPtr = _getPath(start.x, start.y, start.z, end.x, end.y, end.z, request.smooth, request.turnDist, request.stopDist);

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

                _releaseMemory(pathPtr);

                if (request.smooth)
                {
                    var finishIndex = (int)points[1];
                    var slowIndex = (int)points[2];

                    Vector3[] waypoints = new Vector3[(size - 3) / 10];
                    Line[] lines = new Line[(size - 3) / 10];
                    for (int i = 3; i < size; i += 10)
                    {
                        waypoints[i / 10] = new Vector3(points[i], points[i + 1], points[i + 2]);
                        lines[i / 10] = new Line(points[i + 3], points[i + 4], new Vector2(points[i + 5], points[i + 6]), new Vector2(points[i + 7], points[i + 8]), (int)points[i + 9]);
                    }
                    callback(new PathResult(new SmoothPath(waypoints, lines, finishIndex, slowIndex), true, request.hash, request.callback));
                }
                else
                {
                    Vector3[] waypoints = new Vector3[(size - 1) / 3];
                    for (int i = 1; i < size; i += 3)
                    {
                        waypoints[i / 3] = new Vector3(points[i], points[i + 1], points[i + 2]);
                    }
                    callback(new PathResult(new Pathing.Path(waypoints), true, request.hash, request.callback));
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
            var blurPtr = _blurWeights(blurSize);
            int[] returnArray = new int[2];
            Marshal.Copy(blurPtr, returnArray, 0, 2);
            _releaseMemory(blurPtr);

            return (returnArray[0], returnArray[1]);
        }

        /// <summary>
        /// Exports the grid to the passed file path.
        /// </summary>
        /// <param name="filepath">The file path to export to</param>
        public void ExportGrid(string filepath)
        {
            IntPtr resultPtr = _exportGrid();

            float[] sizeArray = new float[1];
            Marshal.Copy(resultPtr, sizeArray, 0, 1);

            int size = (int)sizeArray[0];

            if (size == 1)
            {
                return;
            }

            float[] exportVals = new float[size + 1];
            Marshal.Copy(resultPtr, exportVals, 0, size + 1);
            _releaseMemory(resultPtr);

            using (var writer = new BinaryFile(filepath, BinaryFile.BinaryMode.WRITE))
            {
                foreach (var item in exportVals)
                {
                    writer.Write(item);
                }
                writer.Close();
            }
        }

        /// <summary>
        /// Imports the astar grid map from the passed file path.
        /// </summary>
        /// <param name="filepath">The file path to the file containing the astar map</param>
        /// <returns>The Width, Height grid size, and Min, Max movement penalties</returns>
        public (int, int, int, int) ImportGrid(string filepath)
        {
            List<float> importValues = new List<float>();

            using (var reader = new BinaryFile(filepath, BinaryFile.BinaryMode.READ))
            {
                importValues.Add(reader.ReadSingle());
                for (int i = 0; i < importValues.First(); i++)
                {
                    importValues.Add(reader.ReadSingle());
                }
                reader.Close();
            }
            if (importValues.Count > 9)
            {
                _importGrid(importValues.ToArray(), importValues.Count());
                return ((int)importValues[5], (int)importValues[6], (int)importValues[7], (int)importValues[8]);
            }
            return (0, 0, 0, 0);
        }

        #endregion Public Methods
    }
}
