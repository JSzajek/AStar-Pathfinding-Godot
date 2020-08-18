using Godot;

public class MainMenuCanvas : Control
{
    private SceneController _sceneController;

    public override void _Ready()
    {  
        _sceneController = Navigator.SceneController;
        
        this.Get<Button>("AStarGrid").Connect("pressed", this, "OnAStarGridPressed");
        this.Get<Button>("AStarKDTree").Connect("pressed", this, "OnAStarKDTreePressed");
        this.Get<Button>("AStarNavMesh").Connect("pressed", this, "OnAStarNavMeshPressed");
    }

    public override void _ExitTree()
    {
        this.Get<Button>("AStarGrid").Disconnect("pressed", this, "OnAStarGridPressed");
        this.Get<Button>("AStarKDTree").Disconnect("pressed", this, "OnAStarKDTreePressed");
        this.Get<Button>("AStarNavMesh").Disconnect("pressed", this, "OnAStarNavMeshPressed");
    }

    private void OnAStarGridPressed() => _sceneController.GotoScene("res://_Scenes/AStar_Grid.tscn");

    private void OnAStarKDTreePressed() => _sceneController.GotoScene("res://_Scenes/AStar_KDTree.tscn");

    private void OnAStarNavMeshPressed() => _sceneController.GotoScene("res://_Scenes/AStar_Navmesh.tscn");
}
