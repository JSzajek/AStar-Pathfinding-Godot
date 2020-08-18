using Godot;
using System.Linq;

/// <summary>
/// Extension class for MeshInstance class.
/// </summary>
public static class MeshInstanceExtension {

	/// <summary>
	/// Gets the surface height of the MeshInstance
	/// </summary>
	/// <param name="instance">The MeshInstance</param>
	/// <returns>The surface height</returns>
	public static float GetSurfaceHeight(this MeshInstance instance) {
		return instance.GetTransformedAabb().GetEndpoint(2).y;
	}

	/// <summary>
	/// Retrieves the top corners of the MeshInstance
	/// </summary>
	/// <param name="instance">The MeshInstance</param>
	/// <param name="adjust">The adjusting padding value</param>
	/// <returns>An array of Vector3 top corner values</returns>
	public static Vector3[] GetTopCorners(this MeshInstance instance, float adjust = 0f) {

		var markers = instance.GetChildren<Position3D>();
		if (markers.Count > 0) {
			return markers.Select(pos => pos.GlobalTransform.origin).ToArray();
		}

		var aabb = instance.GetTransformedAabb();
		var center = instance.GlobalTransform.origin;
		var theta = instance.GlobalTransform.basis.GetEuler().y;
		return new Vector3[] {
			(aabb.GetEndpoint(2) - new Vector3(adjust, 0, adjust)).RotateAround(center, theta),
			(aabb.GetEndpoint(6) - new Vector3(-adjust, 0, adjust)).RotateAround(center, theta),
			(aabb.GetEndpoint(7) - new Vector3(-adjust, 0, -adjust)).RotateAround(center, theta),
			(aabb.GetEndpoint(3) - new Vector3(adjust, 0, -adjust)).RotateAround(center, theta)
		};
	}

	/// <summary>
	/// Retrieves the top corners of the MeshInstance constrained in the y axis by the passed value.
	/// </summary>
	/// <param name="instance">The MeshInstance</param>
	/// <param name="heightConstraint">The constrained y value</param>
	/// <param name="adjust">The adjusting padding value</param>
	/// <returns>An array of Vector3 constrained top corner values</returns>
	public static Vector3[] GetTopCornersConstrained(this MeshInstance instance, float heightConstraint, float adjust = 0f) {
		var corners = GetTopCorners(instance, adjust);
		for (int i = 0; i < corners.Length; i++) {
			corners[i] = new Vector3(corners[i].x, heightConstraint, corners[i].z);
		}
		return corners;
	}
}