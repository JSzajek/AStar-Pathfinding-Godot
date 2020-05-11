using Godot;

/// <summary>
/// Canvas class containing connected function signals.
/// </summary>
public class Canvas : Control
{
	private AStar aStar;
	private Panel astarPanel;
	private Button astarToggle;
	private Tween astarTween;

	/// <summary>
	/// Initializing parameters
	/// </summary>
	public override void _Ready()
	{
		astarPanel = this.Get<Panel>("astar_panel");
		astarToggle = astarPanel.Get<Button>("astar_toggle");
		astarTween = astarPanel.Get<Tween>("Tween");
		aStar = this.Get<AStar>("/root/Main/AStar_Linker");
	}

	/// <summary>
	/// On pressed toggle button capture method
	/// </summary>
	public void _on_astar_toggle_pressed() {
		if (astarTween.IsActive()) {
			return;
		}
		var displacement = (astarPanel.RectSize.x - astarToggle.RectSize.x) * (astarPanel.RectPosition.x < 0 ? 0 : -1);
        astarToggle.Text = displacement < 0 ? ">" : "<";
		var goal = new Vector2(displacement, astarPanel.RectPosition.y);
		astarTween.InterpolateProperty(astarPanel, "rect_position", astarPanel.RectPosition, goal, 1.0f);
		astarTween.Start();
	}

	/// <summary>
	/// On pressed visualization button capture method
	/// </summary>
	public void _on_Visualize_pressed() {
		aStar.visualizeGrid = !aStar.visualizeGrid;
		astarPanel.Get<Button>("Visualize").Text = aStar.visualizeGrid ? "Hide Grid" : "Show Grid";
	}
}
