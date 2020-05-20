using Godot;

/// <summary>
/// Interface representing the core methods of AStar class implementations,
/// this is mainly for splitting the astar between three different
/// implementation - otherwise serves no real purpose.
/// </summary>
public interface IAStar {
    void AddNode(Node node);
    void ConnectToTimer(Godot.Object target, string method);
    void GenerateMap();
    void RequestPath(PathRequest request);
}