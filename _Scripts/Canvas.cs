using Godot;

/// <summary>
/// Canvas class containing connected function signals.
/// </summary>
public class Canvas : Control
{
	private PackedScene mainMenu;

	private IAStar aStar;
	private Panel astarPanel, debugPanel;
	private Button astarToggle, debugToggle;
	private Tween astarTween, debugTween;

	public Canvas() {
		mainMenu = ResourceLoader.Load<PackedScene>("res://_Scenes/MainMenu.tscn");
	}

	public void _on_back_button_pressed() {
		GetTree().ChangeSceneTo(mainMenu);
	}

	/// <summary>
	/// Initializing parameters
	/// </summary>
	public override void _Ready()
	{
		astarPanel = this.Get<Panel>("astar_panel");
		astarToggle = astarPanel.Get<Button>("astar_toggle");

		debugPanel = this.Get<Panel>("debug_panel");
		debugToggle = debugPanel.Get<Button>("debug_toggle");
		
		astarTween = astarPanel.Get<Tween>("Tween");
		debugTween = debugPanel.Get<Tween>("Tween");
		aStar = this.Get<IAStar>("../../AStar_Linker");
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

	public void _on_debug_toggle_pressed() {
		if (debugTween.IsActive()) {
			return;
		}
		var displacement = (debugPanel.RectSize.x - debugToggle.RectSize.x) * (debugPanel.RectPosition.x < 0 ? 0 : -1);
        debugToggle.Text = displacement < 0 ? ">" : "<";
		var goal = new Vector2(displacement, debugPanel.RectPosition.y);
		debugTween.InterpolateProperty(debugPanel, "rect_position", debugPanel.RectPosition, goal, 1.0f);
		debugTween.Start();
	}
}
