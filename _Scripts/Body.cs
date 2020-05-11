using Godot;

public class Body : Spatial
{
	private Spatial torso;

	private Spatial head;

	private Position3D headForwardPoint;

	[Export]
	private int headOffsetAngle {get; set;} = -90;

    
	public override void _Ready()
	{
		torso = this.Get<Spatial>("Torso");
		head = this.Get<Spatial>("Head");
		headForwardPoint = GetParent().Get<Position3D>("Forward");
	}

	private void HeadLookAt(Basis targetBasis) {
		if (targetBasis.GetEuler().y - GetGlobalForwardLookAt().GetEuler().y + Mathf.Deg2Rad(headOffsetAngle) < 0) {
			targetBasis = GetGlobalForwardLookAt();
		}
		var targetQuat = head.GlobalTransform.basis.Quat().Normalized().Slerp(targetBasis.Quat(), 0.1f);
		head.GlobalTransform = new Transform(new Basis(targetQuat), head.GlobalTransform.origin);
	}

	private Basis GetGlobalForwardLookAt() {
		return head.GlobalTransform.LookingAt(headForwardPoint.GlobalTransform.origin, Vector3.Up).Rotated(Vector3.Up, Mathf.Deg2Rad(headOffsetAngle)).basis;
	}

	public void HeadLookAt(Vector3 target) {
		HeadLookAt(head.GlobalTransform.LookingAt(target, Vector3.Up).Rotated(Vector3.Up, Mathf.Deg2Rad(headOffsetAngle)).basis);
	}

	public void HeadLookAt() {
		HeadLookAt(GetGlobalForwardLookAt());
	}
}
