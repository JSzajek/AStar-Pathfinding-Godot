using Godot;

public interface IAStar {
    void AddNode(Node node);

    void ConnectToTimer(Godot.Object target, string method);

    void GenerateMap();

    void RequestPath(PathRequest request);
}