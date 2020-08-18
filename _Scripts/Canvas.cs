using Godot;

/// <summary>
/// Canvas class containing connected function signals.
/// </summary>
public class Canvas : Control
{
	private Panel propPanel, debugPanel;
	private Button propToggle, debugToggle;
	private Tween propTween, debugTween;
	private Label fps;

	/// <summary>
	/// Constructor loading in the main menu scene for backwards scene navigation
	/// </summary>
	public Canvas() {
	}

	/// <summary>
	/// Back button capture method
	/// </summary>
	public void _on_back_button_pressed() 
	{
		Navigator.SceneController.GoToMainMenu();
	}

	/// <summary>
	/// Initializing parameters
	/// </summary>
	public override void _Ready()
	{
		propPanel = this.Get<Panel>("prop_panel");
		propToggle = propPanel.Get<Button>("prop_toggle");

		debugPanel = this.Get<Panel>("debug_panel");
		debugToggle = debugPanel.Get<Button>("debug_toggle");
		
		propTween = propPanel.Get<Tween>("Tween");
		debugTween = debugPanel.Get<Tween>("Tween");
		
		fps = this.Get<Label>("fps");
	}

	public override void _Process(float delta)
	{
		fps.Text = "FPS: " + Engine.GetFramesPerSecond();
	}

	/// <summary>
	/// On pressed prop toggle button capture method
	/// </summary>
	public void _on_prop_toggle_pressed() {
		if (propTween.IsActive()) {
			return;
		}
		var displacement = (propPanel.RectSize.x - propToggle.RectSize.x) * (propPanel.RectPosition.x < 0 ? 0 : -1);
        propToggle.Text = displacement < 0 ? ">" : "<";
		var goal = new Vector2(displacement, propPanel.RectPosition.y);
		propTween.InterpolateProperty(propPanel, "rect_position", propPanel.RectPosition, goal, 1.0f);
		propTween.Start();
	}

	/// <summary>
	/// On pressed debug toggle button capture method
	/// </summary>
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
