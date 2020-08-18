using Godot;
using AStar;

/// <summary>
/// Canvas class for navmesh astar
/// </summary>
public class NavmeshCanvas : Control {
    private NavmeshAStar aStar;
    private Panel visualizePanel;
    private Button visualizeToggle, visualizeGrid, visualizeNetwork;
    private Tween visualizeTween;
    private float initialX;

    /// <summary>
    /// Initializing parameters
    /// </summary>
    public override void _Ready()
	{
		visualizePanel = this.Get<Panel>("visualize_panel");
		initialX = visualizePanel.RectPosition.x;
        visualizeToggle = visualizePanel.Get<Button>("visualize_toggle");
        visualizeGrid = visualizePanel.Get<Button>("visualize_grid");
        visualizeNetwork = visualizePanel.Get<Button>("visualize_network");
		visualizeTween = visualizePanel.Get<Tween>("Tween");
        aStar = this.Get<NavmeshAStar>("../AStar_Linker");
    }

    /// <summary>
	/// On pressed toggle button capture method
	/// </summary>
	public void _on_visualize_toggle_pressed() {
		if (visualizeTween.IsActive()) {
			return;
		}
		var displacement = visualizePanel.RectPosition.x > initialX ? initialX : visualizePanel.RectSize.x - visualizeToggle.RectSize.x + visualizePanel.RectPosition.x;
        visualizeToggle.Text = initialX == 0 ? ">" : "<";
		var goal = new Vector2(displacement, visualizePanel.RectPosition.y);
		visualizeTween.InterpolateProperty(visualizePanel, "rect_position", visualizePanel.RectPosition, goal, 1.0f);
		visualizeTween.Start();
	}

    /// <summary>
    /// On pressed visualization of navmesh for astar navmesh
    /// </summary>
    public void _on_visualize_grid_pressed() {
        if (aStar != null) {
            aStar.VisualizeGrid = !aStar.VisualizeGrid;
            visualizeGrid.Text = aStar.VisualizeGrid ? "hide triangles" : "show triangles";
        }
    }

    /// <summary>
    /// On pressed visualization of network for astar navmesh
    /// </summary>
    public void _on_visualize_network_pressed() {
        if (aStar != null) {
            aStar.VisualizeNetwork = !aStar.VisualizeNetwork;
            visualizeNetwork.Text = aStar.VisualizeNetwork ? "hide network" : "show network";
        }
    }
}