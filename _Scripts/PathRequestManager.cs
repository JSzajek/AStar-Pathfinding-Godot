using System;
using Godot;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using GTimer = Godot.Timer;
using Thread = System.Threading.Thread;

/// <summary>
/// Struct representing a path request
/// </summary>
public struct PathRequest
{
    public Vector3 pathStart, pathEnd;
    public bool smooth;
    public float turnDist, stopDist;
    public int hash;
    public Action<Path, bool> callback;

    /// <summary>
    /// Constructor initializing a basic path request's parameters
    /// </summary>
    /// <param name="callback">The calllback method to return the results to</param>
    /// <param name="start">The start point of the path</param>
    /// <param name="end">The ending point of the path</param>
    public PathRequest(Action<Path, bool> callback, Vector3 start, Vector3 end, int hash)
    {
        this.hash = hash;
        this.pathStart = start;
        this.pathEnd = end;
        this.callback = callback;
        smooth = false;
        stopDist = turnDist = 0.0f;
    }

    /// <summary>
    /// Overloaded constructor initializing a smoothed path request
    /// </summary>
    /// <param name="callback">The callback method to return the results to</param>
    /// <param name="start">The starting point of the path</param>
    /// <param name="end">The ending point of the path</param>
    /// <param name="turnDist">The turn radius (amount of turning)</param>
    /// <param name="stopDist">The stop distance for slowdown and walking on actual end point</param>
    /// <returns></returns>
    public PathRequest(Action<Path, bool> callback, Vector3 start, Vector3 end, int hash, float turnDist, float stopDist) : this(callback, start, end, hash) {
        this.smooth = true;
        this.turnDist = turnDist;
        this.stopDist = stopDist;
    }
}

/// <summary>
/// Struct representing a path request result
/// </summary>
public struct PathResult
{
    public int hash;
    public Path path;
    public bool success;
    public Action<Path, bool> callback;

    /// <summary>
    /// Constructor initializing a path result
    /// </summary>
    /// <param name="path">The path of the path result</param>
    /// <param name="success">Whether it was a success</param>
    /// <param name="callback">The callback method to return the path to</param>
    public PathResult(Path path, bool success, int hash, Action<Path, bool> callback)
    {
        this.hash = hash;
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}

/// <summary>
/// Class representing a basic path of just waypoints
/// </summary>
public class Path {
    public Vector3[] points {get;}

    /// <summary>
    /// Constructor initializing a basic path
    /// </summary>
    /// <param name="points">The waypoints of the path</param>
    public Path(Vector3[] points) {
        this.points = points;
    }
}

/// <summary>
/// Class representing a path with smoothing
/// </summary>
public class SmoothPath : Path {
    
    public Line[] lines;
    public int finishIndex;
    public int slowIndex;

    /// <summary>
    /// Constructor initializing a smoothed path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="lines"></param>
    /// <param name="finishIndex"></param>
    /// <param name="slowIndex"></param>
    /// <returns></returns>
    public SmoothPath(Vector3[] path, Line[] lines, int finishIndex, int slowIndex) : base(path) {
        this.lines = lines;
        this.finishIndex = finishIndex;
        this.slowIndex = slowIndex;
    }
}

/// <summary>
/// Path Request manager class representing managment
/// of queued pathrequest and distributing the results
/// back after a constant poll rate.
/// </summary>
public class PathRequestManager : Node
{
    // How often to check if results are done calculating
    private const float POLL_RATE = 0.3f;
    
    static PathRequestManager instance;
    private IAStarLinker AStarLinker;
    private Queue<PathResult> results = new Queue<PathResult>();
    private GTimer timer;
    private Hashtable table;

    /// <summary>
    /// Constructor to initialize the pathrequest manager parameters
    /// </summary>
    /// <param name="linker">AStar algorithm linker that calculates the pathes</param>
    /// <param name="addNode">Action to add node into scene from outside scene</param>
    public PathRequestManager(IAStarLinker linker, Action<Node> addNode)
    {
        instance = this;
        table = new Hashtable();
        AStarLinker = linker;
        timer = new GTimer();
        addNode(timer); // Add timer node from outside scene
        timer.Connect("timeout", this, "poll_time_out");
        timer.Start(POLL_RATE);
    }

    public void Destroy() {
        // Wait for any on going threads to join main thread before destroying
        foreach(var s in table) {
            if (table[s] is Thread thread) {
                if (thread.IsAlive) {
                    thread.Join(1500);
                }
            }
        }
    }

    /// <summary>
    /// Polling method connected to timer timeout signal.
    /// For returning the path results.
    /// </summary>
    public void poll_time_out() 
    {
        if (results.Count > 0) {
            lock(results) {
                while (results.Count > 0) {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success);
                }
            }
        }
    }

    private void BackgroundTaskWithObject(System.Object stateInfo)  
    {  
        if (stateInfo is PathRequest request) {
            instance.AStarLinker.getPath(request, FinishedProcessingPath);
        }
    }  

    /// <summary>
    /// Requesting method to request a path request.
    /// </summary>
    /// <param name="request">The path request (basic/smooth)</param>
    public void RequestPath(PathRequest request)
    {
        // Prevent sequential pathrequest tasks from piling up.
        if (table.Contains(request.hash)) {
            return;
        }

        ThreadStart startThread = delegate {
           instance.AStarLinker.getPath(request, FinishedProcessingPath);
        };
        Thread t = new Thread(startThread);
        table.Add(request.hash, t);
        t.Start();
    }

    /// <summary>
    /// Callback method when path finished and will be enqueued in return
    /// queue.
    /// </summary>
    /// <param name="result"></param>
    public void FinishedProcessingPath(PathResult result)
    {
        lock(results) {
            results.Enqueue(result);
            //(table[result.hash] as Thread).Join();
            table.Remove(result.hash);
        }
    }
}