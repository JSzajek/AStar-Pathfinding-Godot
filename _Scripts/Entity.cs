using Godot;

/// <summary>
/// Abstract Entity class representing core features
/// relevent to all entities such as head and/or body.
/// </summary>
public abstract class Entity : KinematicBody 
{
	#region Fields

	protected readonly Vector3 gravity = Vector3.Down * 12;
	protected Position3D head;
	protected Body body;

	#endregion Fields

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="Entity"/> class.
	/// </summary>
	public override void _Ready()
	{
		head = this.Get<Position3D>("Head");
		if (HasBody) {
			body = this.Get<Body>("Body");
		}
	}

	#endregion Constructors

	#region Properties

	/// <summary>
	/// Gets or sets whether the entity has a body
	/// </summary>
	[Export]
	public bool HasBody {get; set;} = true;

	#endregion Properties

	#region Public Methods

	/// <summary>
	/// Gets the head position (global)
	/// </summary>
	public Vector3 HeadPosition
	{
		get 
		{
			if (head == null) {
				return GlobalTransform.origin;
			}
			return head.GlobalTransform.origin;
		}
	}

	/// <summary>
	/// Getter method for the body of the entity
	/// </summary>
	/// <returns>The body of the entity</returns>
	public Body GetBody() {
		return body;
	}

	/// <summary>
	/// Gets the global position of the entity
	/// </summary>
	public Vector3 GlobalPosition => GlobalTransform.origin;

	/// <summary>
	/// Gets the distance squared to the passed entity
	/// </summary>
	/// <param name="other">The comparison entity</param>
	/// <returns>The distance squared to the entity or null if passed entity is null</returns>
	public float? DistanceSquaredTo(Entity other) {
		if (other == null) {
			return null;
		}
		return GlobalPosition.DistanceSquaredTo(other.GlobalPosition);
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
		return HeadPosition.DistanceSquaredTo(other.HeadPosition);
	}

	#endregion Public Methods
}