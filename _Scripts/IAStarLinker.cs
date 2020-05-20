using System;

/// <summary>
/// Interface representing the core methods of AStarLinker class implementations,
/// this is mainly for splitting the astar between three different
/// implementation - otherwise serves no real purpose.
/// </summary>
public interface IAStarLinker {
    void getPath(PathRequest request, Action<PathResult> callback);
}