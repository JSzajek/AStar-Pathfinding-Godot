using Godot;
using AStar;

/// <summary>
/// Canvas class for kdtree astar
/// </summary>
public class KDTreeCanvas : Control
{
    private KDTreeAStar aStar;
    private Panel visualizePanel;
    private Button visualizeToggle, visualizeGrid;
    private Tween visualizeTween;
    private float initialX;

    /// <summary>
    /// Initializes Parameters
    /// </summary>
    public override void _Ready()
	{
		visualizePanel = this.Get<Panel>("visualize_panel");
		initialX = visualizePanel.RectPosition.x;
        visualizeToggle = visualizePanel.Get<Button>("visualize_toggle");
        visualizeGrid = visualizePanel.Get<Button>("visualize_grid");
		visualizeTween = visualizePanel.Get<Tween>("Tween");
        aStar = this.Get<KDTreeAStar>("../AStar_Linker");
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
    /// On pressed visualization for astar kdtree
    /// </summary>
    public void _on_visualize_grid_pressed() {
        if (aStar != null) {
            aStar.VisualizeGrid = !aStar.VisualizeGrid;
            visualizeGrid.Text = aStar.VisualizeGrid ? "hide grid" : "show grid";
        }
    }
}
