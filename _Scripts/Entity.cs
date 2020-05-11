using Godot;

/// <summary>
/// Abstract Entity class representing core features
/// relevent to all entities such as head and/or body.
/// </summary>
public abstract class Entity : KinematicBody 
{
	protected readonly Vector3 gravity = Vector3.Down * 12;
	protected Position3D head;
	protected Body body;

	[Export]
	public bool hasBody {get; set;} = true;

	/// <summary>
	/// Initializes entity parameters
	/// </summary>
	public override void _Ready()
	{
		head = this.Get<Position3D>("Head");
		if (hasBody) {
			body = this.Get<Body>("Body");
		}
	}

	/// <summary>
	/// Getter method for the head position (global)
	/// </summary>
	/// <returns>The global head position of the entity</returns>
	public Vector3 HeadPosition()
	{
		if (head == null) {
			return GlobalTransform.origin;
		}
		return head.GlobalTransform.origin;
	}

	/// <summary>
	/// Getter method for the body of the entity
	/// </summary>
	/// <returns>The body of the entity</returns>
	public Body GetBody() {
		return body;
	}

	/// <summary>
	/// Getter method for the global position of the entity
	/// </summary>
	/// <returns>The global position of the entity</returns>
	public Vector3 GetGlobalPosition()
	{
		return GlobalTransform.origin;
	}

	/// <summary>
	/// Gets the distance squared to the passed entity
	/// </summary>
	/// <param name="other">The comparison entity</param>
	/// <returns>The distance squared to the entity or null if passed entity is null</returns>
	public float? DistanceSquaredTo(Entity other) {
		if (other == null) {
			return null;
		}
		return GetGlobalPosition().DistanceSquaredTo(other.GetGlobalPosition());
	}

	/// <summary>
	/// Gets the distance squared between the heads of this entity
	/// and the passed entity.
	/// </summary>
	/// <param name="other">The comparison entity</param>
	/// <returns>The distance squared between the heads or null if passed entity is null</returns>
	public float? HeadDistanceSquaredTo(Entity other) {
		if (other == null) {
			return null;
		}
		return HeadPosition().DistanceSquaredTo(other.HeadPosition());
	}
}
