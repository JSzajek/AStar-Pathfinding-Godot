using Godot;
using System;

public class MainMenuCanvas : Control
{
    private PackedScene gridScene ;
    private PackedScene kdtreeScene;
    private PackedScene navmeshScene;

    public override void _Ready()
    {  
        gridScene = ResourceLoader.Load<PackedScene>("res://_Scenes/AStar_Grid.tscn");
        kdtreeScene = ResourceLoader.Load<PackedScene>("res://_Scenes/AStar_KDTree.tscn");
        navmeshScene = ResourceLoader.Load<PackedScene>("res://_Scenes/AStar_Navmesh.tscn");
    }

    public void _on_AStar_Grid_pressed() {
        GetTree().ChangeSceneTo(gridScene);
    }

    public void _on_AStar_KDTree_pressed() {
        GetTree().ChangeSceneTo(kdtreeScene);
    }

    public void _on_AStar_NavMesh_pressed() {
        GetTree().ChangeSceneTo(navmeshScene);
    }
}
