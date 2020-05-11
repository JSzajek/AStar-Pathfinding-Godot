using Godot;

/// <summary>
/// Enemy Controller representing a basic enemy logic 
/// of following a target
/// </summary>
public class Enemy : EntityController
{
	[Export]
	public float moveSpeed {get; set;} = 1;
	
	[Export]
	public bool pathSmooth {get; set;} = false;

	[Export]
	public float turningSpeed {get; set;} = 3;
	
	[Export]
	public float turningDist {get; set;} = 5;
	
	[Export]
	public float stoppingDist {get; set;} = 10;

	[Export]
	public string targetPath {get; set;}

	[Export]
	public bool showPath {get; set;} = true;

	private Entity target;

	/// <summary>
	/// Initializes enemy paramters
	/// </summary>
	public override void _Ready()
	{
		base._Ready();
		base.SetUp(pathSmooth, moveSpeed, turningSpeed, turningDist, stoppingDist, showPath); // Look into passing through constructor

		if (targetPath != null) {
			StartTracking(GetNode<Entity>(targetPath));
		}
	}

	/// <summary>
	/// Starts tracking of passed entity.
	/// </summary>
	/// <param name="newTarget">The entity to follow</param>
	public void StartTracking(Entity newTarget)
	{
		target = newTarget;
		RequestPath(target.GetGlobalPosition());
	}

	/// <summary>
	/// Physics Process called every frame
	/// </summary>
	/// <param name="delta">Time delta</param>
	public override void _PhysicsProcess(float delta) {
		base._PhysicsProcess(delta);

		if (target != null) {
			if (HeadDistanceSquaredTo(target) < stoppingDist * stoppingDist) {
				GetBody()?.HeadLookAt(target.HeadPosition());
			}
			else {
				GetBody()?.HeadLookAt(); // Look Forward
			}
		}
		MoveAndSlide(base.movementVelocity + (gravity * delta), Vector3.Up);
	}

	/// <summary>
	/// Overriden get target method to get updated target position.
	/// </summary>
	/// <returns>Updated target position (global)</returns>
	public override Vector3 GetTarget() {
		return target.GlobalTransform.origin;
	}
}
