using Godot;

/// <summary>
/// Class reprensenting the base features of what comprises a body.
/// </summary>
public class Body : Spatial
{
	#region Fields

	private Spatial torso;
	private Spatial head;
	private Position3D headForwardPoint;

	#endregion Fields

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="Body"/> class.
	/// </summary>
	public override void _Ready()
	{
		torso = this.Get<Spatial>("Torso");
		head = this.Get<Spatial>("Head");
		headForwardPoint = GetParent().Get<Position3D>("Forward");
	}

	#endregion Constructors

	#region Public Properties

	/// <summary>
	/// Gets or Sets the Head's offset angle.
	/// </summary>
	[Export] 
	public int HeadOffsetAngle {get; set;} = -90;
    
	#endregion Public Properties

	#region Public Methods

	/// <summary>
	/// Turns the head towards the passed target position with the
	/// the Head Offset Angle applied when necessary.
	/// </summary>
	/// <param name="target">The target position the head will look at.</param>
	public void HeadLookAt(Vector3 target) {
		HeadLookAt(head.GlobalTransform.LookingAt(target, Vector3.Up).Rotated(Vector3.Up, Mathf.Deg2Rad(HeadOffsetAngle)).basis);
	}

	/// <summary>
	/// Turns the head to look at the forward position, or in other
	/// words look straight ahead.
	/// </summary>
	public void HeadLookAt() {
		HeadLookAt(GetGlobalForwardLookAt());
	}

	#endregion Public Methods

	#region Private Methods

	/// <summary>
	/// Turns the head Spatial based on the passed rotation transform.
	/// </summary>
	/// <param name="targetBasis">The rotation transform to apply to the head Spatial</param>
	private void HeadLookAt(Basis targetBasis) {
		if (targetBasis.GetEuler().y - GetGlobalForwardLookAt().GetEuler().y + Mathf.Deg2Rad(HeadOffsetAngle) < 0) {
			targetBasis = GetGlobalForwardLookAt();
		}
		//var targetQuat = head.GlobalTransform.basis.Quat().Normalized().Slerp(targetBasis.Quat(), 0.1f);
		//head.GlobalTransform = new Transform(new Basis(targetQuat), head.GlobalTransform.origin);
	}

	/// <summary>
	/// Retrieves the global rotation pointing towards the forward position.
	/// </summary>
	/// <returns></returns>
	private Basis GetGlobalForwardLookAt() {
		return head.GlobalTransform.LookingAt(headForwardPoint.GlobalTransform.origin, Vector3.Up).Rotated(Vector3.Up, Mathf.Deg2Rad(HeadOffsetAngle)).basis;
	}

	#endregion Private Methods
}