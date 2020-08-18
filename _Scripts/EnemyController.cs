using Godot;

/// <summary>
/// Enemy Controller representing a basic enemy logic 
/// of following a target
/// </summary>
public class EnemyController : EntityController
{
	#region Fields

	private Entity target;

	#endregion Fields

	#region Properties

	/// <summary>
	/// Gets or sets the target as a NodePath of the enemy. 
	/// </summary>
	[Export]
	public NodePath TargetPath {get; set;}

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="Enemy"/> class.
	/// </summary>
	public override void _Ready()
	{
		base._Ready();

		if (TargetPath != null) {
			StartTracking(GetNode<Entity>(TargetPath));
		}
	}
	
	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Stops tracking the current target
	/// </summary>
	public void StopTracking() {
		//TODO: Implement stopping	
	}

	/// <summary>
	/// Starts tracking of passed entity.
	/// </summary>
	/// <param name="newTarget">The entity to follow</param>
	public void StartTracking(Entity newTarget)
	{
		target = newTarget;
		RequestPath(target.GlobalPosition);
	}

	/// <summary>
	/// Physics Process called every frame
	/// </summary>
	/// <param name="delta">Time delta</param>
	public override void _PhysicsProcess(float delta) {
		base._PhysicsProcess(delta);

		if (target != null) {
			if (HeadDistanceSquaredTo(target) < StoppingDistance * StoppingDistance) {
				GetBody()?.HeadLookAt(target.HeadPosition);
				
				if (DistanceSquaredTo(target) < StoppingDistance) {
					base.movementVelocity = Vector3.Zero;
				}
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

	#endregion Public Methods
}