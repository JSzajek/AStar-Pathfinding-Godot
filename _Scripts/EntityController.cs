using Godot;

/// <summary>
/// Entity Controller class, basic controller for all entity
/// based classes that has pathing capabilities.
/// </summary>
public class EntityController : Entity 
{
	private const float PATH_UPDATE_MOVE_THRESHOLD = 0.5f;
	private const float sqr_move_threshold = PATH_UPDATE_MOVE_THRESHOLD * PATH_UPDATE_MOVE_THRESHOLD;
	
	private AStar AStar;
	private Vector3 target, targetOldPosition;
	
	private bool following, requesting, smoothPath = false, showPath = false;
	private float speed, turnSpeed, turnDist, stopDist;
	private float speedPercent = 1;
	private int pathIndex = 0;

	private Vector3[] waypoints;
	private Line[] turns;
	private int finishIndex, slowIndex;
	public Vector3 movementVelocity;

	/// <summary>
	/// Initialized entity controller parameters
	/// </summary>
	public override void _Ready()
	{
		base._Ready();
		AStar = this.Get<AStar>("/root/Main/AStar_Linker");
		AStar?.ConnectToTimer(this, "On_Path_Update_Timer_timeout");
	}

	/// <summary>
	/// Set up parameters based off of parameters passed down from super class.
	/// </summary>
	/// <param name="smoothPath">Whether to utilize path smoothing</param>
	/// <param name="speed">The speed of movement</param>
	/// <param name="turnSpeed">The turning speed</param>
	/// <param name="turnDist">The turning distance</param>
	/// <param name="stopDist">The stopping distance</param>
	/// <param name="showPath">Whether to show debug of path (for testing)</param>
	public void SetUp(bool smoothPath, float speed, float turnSpeed, float turnDist, float stopDist, bool showPath)
	{
		// TODO: Investigate a better way to pass down variables while also keeping Godot's editor export functionality
		this.smoothPath = smoothPath;
		this.speed = speed;
		this.turnSpeed = turnSpeed;
		this.turnDist = turnDist;
		this.stopDist = stopDist;
		this.showPath = showPath;
	}

	/// <summary>
	/// Path request method, requeting either basic or smooth path to the target position.
	/// </summary>
	/// <param name="target">The target position to move towards</param>
	public void RequestPath(Vector3 target)
	{
		this.target = target;
		targetOldPosition = target;
		if (smoothPath) {
			AStar?.RequestPath(new PathRequest(OnPathFound, GetGlobalPosition(), target, turnDist, stopDist));
		}
		else {
			AStar?.RequestPath(new PathRequest(OnPathFound, GetGlobalPosition(), target));
		}
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
	/// Move along the path in a basic manner, following the waypoints
	/// in sequence.
	/// </summary>
	public void MoveAlongPath()
	{
		if(!following || pathIndex == waypoints.Length) {
			return;
		}

		var currPoint = new Vector3(waypoints[pathIndex].x, GetGlobalPosition().y, waypoints[pathIndex].z);
		if (GetGlobalPosition().DistanceTo(currPoint) < 1) {
			pathIndex += 1;
			if (pathIndex > waypoints.Length) {
				following = false;
				movementVelocity = Vector3.Zero;
				return;
			}
		}
		Transform = Transform.LookingAt(currPoint, Vector3.Up);
		movementVelocity = -Transform.basis.z * speed * speedPercent;
	}

	/// <summary>
	/// Following the path waypoints in a smoothed manner utilizing calculated turn boundaries
	/// as well as slow down.
	/// </summary>
	public void MoveAlongSmootPath()
	{
		if (!following || pathIndex >= turns.Length) {
			return;
		}

		var position2D = new Vector2(GetGlobalPosition().x, GetGlobalPosition().z);
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
			if (pathIndex >= slowIndex && stopDist > 0) {
				speedPercent = Mathf.Clamp(turns[finishIndex].DistanceFromPoint(position2D) / stopDist, 0, 1);
				if (speedPercent < 0.2f) {
					following = false;
					movementVelocity = Vector3.Zero;
					return;
				}
			}
			var currPoint = new Vector3(waypoints[pathIndex].x, GetGlobalPosition().y, waypoints[pathIndex].z);
			var target_rot = Transform.LookingAt(currPoint, Vector3.Up).basis;
			var turned = Transform.basis.Quat().Slerp(target_rot.Quat(), turnSpeed / 5);
			Transform = new Transform(new Basis(turned), Transform.origin);
			movementVelocity = -Transform.basis.z * speed * speedPercent;
		}
	}

	/// <summary>
	/// Physics process method called every frame
	/// </summary>
	/// <param name="delta">Time delta</param>
	public override void _PhysicsProcess(float delta)
	{
		if (following) {
			if (smoothPath) {
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
		if (showPath) {
			DrawPath();
		}
	}

	/// <summary>
	/// Connected method catching the polling timeout, checking whether
	/// target has moved a significant amount such that need to recalculate path.
	/// </summary>
	public void On_Path_Update_Timer_timeout() {
		var newTarget = GetTarget();
		if ((newTarget - targetOldPosition).LengthSquared() > sqr_move_threshold)
		{
			requesting = true;
			RequestPath(newTarget);
			targetOldPosition = newTarget;
		}
	}

	/// <summary>
	/// Path drawing method for debug testing
	/// </summary>
	public void DrawPath() {
		var debug = AStar.Get<Spatial>("Debug");
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
			geometry.AddVertex(waypoints[i] + (Vector3.Up * HeadPosition().y));
			geometry.AddVertex(waypoints[i+1] + (Vector3.Up * HeadPosition().y));
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
}
