using Godot;

/// <summary>
/// Viewer class for 3d rts-like camera viewer
/// </summary>
public class Viewer : KinematicBody
{
    private const int MIN_SCROLL = -5;
	private const int MAX_SCROLL = -35;
    private float mouse_sensitivity = 0.0075f;

    private float panSpeed = 15;

    private const int rayLength = 1000;
    private const int rotationAmountDegrees = 10;

    private Canvas canvas;
    private Spatial swivel;
    private Spatial pivot;
    private Camera camera;
    private Spatial target;

    private PhysicsDirectSpaceState spaceState;

    private bool rotating = false;

    /// <summary>
    /// Initializing parameters
    /// </summary>
    public override void _Ready() {
        spaceState = GetWorld().DirectSpaceState;
        canvas = this.Get<Canvas>("/root/Main/Canvas/Canvas");
        pivot = this.Get<Spatial>("Pivot");
        swivel = this.Get<Spatial>("Pivot/Swivel");
        camera = this.Get<Camera>("Pivot/Swivel/Camera");
        target = this.Get<Spatial>("/root/Main/Target");
    }

    /// <summary>
    /// Capturing unhandled input for camera rotation controls
    /// </summary>
    public override void _UnhandledInput(InputEvent @event){
        // Pivot (left and right) changes around the y axis 
        // Swivel (up and down) changes around the x axis
        if (@event is InputEventMouse mouse) {
            if (rotating && @event is InputEventMouseMotion motion) {
                pivot.RotateY(-motion.Relative.x * mouse_sensitivity);
                swivel.RotateX(motion.Relative.y * mouse_sensitivity);
                swivel.RotationDegrees = new Vector3(Mathf.Clamp(swivel.RotationDegrees.x, 15, 90), swivel.RotationDegrees.y, swivel.RotationDegrees.z);
            }
            if (@event is InputEventMouseButton button && button.IsPressed()) {
                if (button.ButtonIndex.Equals((int)ButtonList.Left)) {
                    var vec_from = camera.ProjectRayOrigin(button.Position);
                    var vec_to = camera.ProjectRayNormal(button.Position) * rayLength;
                    var result = spaceState.IntersectRay(vec_from, vec_to);
                    if (result.Count > 0) {
                        MoveTargetToPosition((Vector3)result["position"]);
                    }
                    return;
                }

                var pos = camera.Transform.origin;
                if (button.ButtonIndex.Equals((int)ButtonList.WheelUp)) {
				   pos = pos.LinearInterpolate(new Vector3(pos.x, pos.y, MIN_SCROLL), 0.1f);
                }
                if (button.ButtonIndex.Equals((int)ButtonList.WheelDown)) {
				   pos = pos.LinearInterpolate(new Vector3(pos.x, pos.y, MAX_SCROLL), 0.1f);
                }
				camera.Transform = new Transform(camera.Transform.basis, pos);
            }
        }
    }

    /// <summary>
    /// Process method called every frame
    /// </summary>
    /// <param name="delta">time delta</param>
    public override void _Process(float delta) {
        GetInput(); // Capture unhandled user input
    }

    /// <summary>
    /// Captures user input
    /// </summary>
    private void GetInput() {
        rotating = Input.IsMouseButtonPressed(2); // Right button pressed
        
        // Scrolling controls
        var pos = camera.Transform.origin;
        if (Input.IsMouseButtonPressed(5)) {
            pos = pos.LinearInterpolate(new Vector3(pos.x, pos.y, MIN_SCROLL), 0.1f);
        }
        else if(Input.IsMouseButtonPressed(4)) {
            pos = pos.LinearInterpolate(new Vector3(pos.x, pos.y, MAX_SCROLL), 0.1f);
        }
        camera.Transform = new Transform(camera.Transform.basis, pos);

        // Movement controls
        Vector3 movement = Vector3.Zero;
        if (Input.IsActionPressed("ui_up")) {
            movement += Vector3.Forward;
        }
        if (Input.IsActionPressed("ui_down")) {
            movement += Vector3.Back;
        }
        if (Input.IsActionPressed("ui_left")) {
            movement += Vector3.Left;
        }
        if (Input.IsActionPressed("ui_right")) {
            movement += Vector3.Right;
        }
        MoveAndSlide(movement.Rotated(Vector3.Up, pivot.Rotation.y) * panSpeed, Vector3.Up);
        var clamped = new Vector3(Mathf.Clamp(Transform.origin.x, -18, 18), Transform.origin.y, Mathf.Clamp(Transform.origin.z, -14, 14));
        Transform = new Transform(Transform.basis, clamped);
    }

    /// <summary>
    /// Helper method to move target to passed position.
    /// </summary>
    /// <param name="position">The new target position</param>
    private void MoveTargetToPosition(Vector3 position) {
        target.GlobalTransform = new Transform(target.GlobalTransform.basis, position + (Vector3.Up * 0.25f));
    }
}
