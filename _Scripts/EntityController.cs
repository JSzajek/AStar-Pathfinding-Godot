using Godot;
using Pathing;
using Path = Pathing.Path;
using AStar;

/// <summary>
/// Entity Controller class, basic controller for all entity
/// based classes that has pathing capabilities.
/// </summary>
public class EntityController : Entity 
{
	#region Constants

	private const float MOVE_THRESHOLD = 0.5f;
	private const float SQR_MOVE_THRESHOLD = MOVE_THRESHOLD * MOVE_THRESHOLD;

	#endregion Constants 

	#region Fields

	private IAStar AStar;
	private Vector3 target, targetOldPosition;
	
	private bool following, requesting;
	private float speedPercent = 1;
	private int pathIndex = 0;

	protected Vector3[] waypoints;
	private Line[] turns;
	private int finishIndex, slowIndex;
	public Vector3 movementVelocity;

	#endregion Fields

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="EntityController"/> class.
	/// </summary>
	public override void _Ready()
	{
		base._Ready();
		AStar = this.GetRoot().GetFirstChild<IAStar>(true);
		AStar?.ConnectToTimer(this, "On_Path_Update_Timer_timeout");
	}

	#endregion Constructors

	#region Public Properties

	/// <summary>
	/// Gets or sets the movement speed of the enemy. 
	/// </summary>
	[Export]
	public float MoveSpeed {get; set;}
	
	/// <summary>
	/// Gets or sets whether the enemy will use smoothed paths.
	/// </summary>
	[Export]
	public bool PathSmoothing {get; set;}

	/// <summary>
	/// Gets or sets the turning speed of the enemy.
	/// </summary>
	[Export]
	public float TurningSpeed {get; set;}
	
	/// <summary>
	/// Gets or sets the turning distance of the enemy.
	/// </summary>
	[Export]
	public float TurningDistance {get; set;}
	
	/// <summary>
	/// Gets or sets the stopping distance of the enemy.
	/// </summary>
	[Export]
	public float StoppingDistance {get; set;}

	/// <summary>
	/// Gets or sets whether the enemy will show the pathing - for debugging
	/// </summary>
	[Export]
	public bool ShowPath {get; set;}

	#endregion Public Properties

	#region Public Methods

	/// <summary>
	/// Path request method, requeting either basic or smooth path to the target position.
	/// </summary>
	/// <param name="target">The target position to move towards</param>
	public void RequestPath(Vector3 target)
	{
		this.target = target;
		targetOldPosition = target;
		if (PathSmoothing) {
			AStar?.RequestPath(new PathRequest(OnPathFound, GlobalPosition, target, GetHashCode(), TurningDistance, StoppingDistance));
		}
		else {
			AStar?.RequestPath(new PathRequest(OnPathFound, GlobalPosition, target, GetHashCode()));
		}
	}

	/// <summary>
	/// Generates a unique hashcode combined with the object's hashcode and the
	/// target's hashcode.
	/// </summary>
	/// <returns>The unique hashcode</returns>
	public override int GetHashCode() {
		var hash = 17;
		hash = hash * 31 + base.GetHashCode();
		hash = hash * 31 + target.GetHashCode();
		return hash;
	}

	/// <summary>
	/// Callback method when path has been found.
	/// </summary>
	/// <param name="path">The calculated path</param>
	/// <param name="success">Whether path was found</param>
	public void OnPathFound(Path path, bool success)
	{
		if (success) {
			SetPath(path);
		}
	}

	/// <summary>
	/// Stops the entity's pathing 
	/// </summary>
	public void Stop() {
		AStar?.DisconnectTimer(this, "On_Path_Update_Timer_timeout");
		following = false;
		pathIndex = 0;
		waypoints = null;
	}

	/// <summary>
	/// Performs cleanup upon exiting the scene.
	/// </summary>
	public override void _ExitTree() {
		Stop();
	}

	/// <summary>
	/// Move along the path in a basic manner, following the waypoints
	/// in sequence.
	/// </summary>
	public void MoveAlongPath()
	{
		if(!following || pathIndex == waypoints.Length) {
			movementVelocity = Vector3.Zero;
			return;
		}

		var currPoint = new Vector3(waypoints[pathIndex].x, GlobalPosition.y, waypoints[pathIndex].z);
		if ((pathIndex == waypoints.Length - 1 && GlobalPosition.DistanceTo(currPoint) < StoppingDistance) || GlobalPosition.DistanceTo(currPoint) < 1) {
			pathIndex += 1;
			if (pathIndex > waypoints.Length) {
				following = false;
				movementVelocity = Vector3.Zero;
				return;
			}
		}
		Transform = Transform.LookingAt(currPoint, Vector3.Up);
		movementVelocity = -Transform.basis.z * MoveSpeed * speedPercent;
	}

	/// <summary>
	/// Following the path waypoints in a smoothed manner utilizing calculated turn boundaries
	/// as well as slow down.
	/// </summary>
	public void MoveAlongSmootPath()
	{
		if (!following || pathIndex >= turns.Length) {
			movementVelocity = Vector3.Zero;
			return;
		}

		var position2D = new Vector2(GlobalPosition.x, GlobalPosition.z);
		while (turns[pathIndex].HasCrossedLine(position2D)) {
			if (pathIndex == finishIndex) {
				following = false;
				movementVelocity = Vector3.Zero;
				break;
			} else {
				pathIndex++;
			}
		}

		if (following) {
			speedPercent = 1;
			if (pathIndex >= slowIndex && StoppingDistance > 0) {
				speedPercent = Mathf.Clamp(turns[finishIndex].DistanceFromPoint(position2D) / StoppingDistance, 0, 1);
				if (speedPercent < 0.2f) {
					following = false;
					movementVelocity = Vector3.Zero;
					return;
				}
			}
			var currPoint = new Vector3(waypoints[pathIndex].x, GlobalPosition.y, waypoints[pathIndex].z);
			var target_rot = Transform.LookingAt(currPoint, Vector3.Up).basis;
			var turned = Transform.basis.Quat().Slerp(target_rot.Quat(), TurningSpeed / 5);
			Transform = new Transform(new Basis(turned), Transform.origin);
			movementVelocity = -Transform.basis.z * MoveSpeed * speedPercent;
		}
	}

	/// <summary>
	/// Physics process method called every frame
	/// </summary>
	/// <param name="delta">Time delta</param>
	public override void _PhysicsProcess(float delta)
	{
		if (following) {
			if (PathSmoothing) {
				MoveAlongSmootPath();
			}
			else {
				MoveAlongPath();
			}
		} 
	}

	/// <summary>
	/// Sets the path while reseting parameters required
	/// for path following.
	/// </summary>
	/// <param name="path"></param>
	public void SetPath(Path path) {
		this.waypoints = path.points;
		if (path is SmoothPath smoothed) {
			this.turns = smoothed.lines;
			this.finishIndex = smoothed.finishIndex;
			this.slowIndex = smoothed.slowIndex;
		}
		pathIndex = 0;
		requesting = false;
		following = true;
		if (ShowPath) {
			DrawPath();
		}
	}

	/// <summary>
	/// Connected method catching the polling timeout, checking whether
	/// target has moved a significant amount such that need to recalculate path.
	/// </summary>
	public void On_Path_Update_Timer_timeout() {
		var newTarget = GetTarget();
		if ((newTarget - targetOldPosition).LengthSquared() > SQR_MOVE_THRESHOLD)
		{
			requesting = true;
			RequestPath(newTarget);
			targetOldPosition = newTarget;
		}
	}

	/// <summary>
	/// Draws the current path - for debugging
	/// </summary>
	public void DrawPath() {
		var debug = this.Get<Spatial>("/root/Main/AStar_Linker/Debug");
		ImmediateGeometry geometry;
		if (debug.Get<ImmediateGeometry>(Name) is ImmediateGeometry geom) {
			geometry = geom;
		}
		else {
			var mat = new SpatialMaterial() {AlbedoColor = new Color(0, 0, 0)};
			geometry = new ImmediateGeometry() {Name = this.Name, MaterialOverride = mat};
			debug.AddChild(geometry);
			GD.Print("Adding Path Visualization For " + Name);
		}

		geometry.Clear();
		geometry.Begin(Mesh.PrimitiveType.Lines);
		for(int i = 0; i < waypoints.Length - 1; i++) {
			geometry.AddVertex(waypoints[i] + (Vector3.Up * HeadPosition.y));
			geometry.AddVertex(waypoints[i+1] + (Vector3.Up * HeadPosition.y));
		}
		geometry.End();
	}

	/// <summary>
	/// Virtual get target position (global) method
	/// </summary>
	/// <returns>The current target position</returns>
	public virtual Vector3 GetTarget() {
		return target;
	}

	#endregion Public Methods
}