using Godot;

/// <summary>
/// Extension class for Aabb class.
/// </summary>
public static class AABBExtension {

	/// <summary>
	/// Extended AABB method to get the transformed AABB of the given
	/// AABB and the given transform
	/// </summary>
	/// <param name="instance">The AABB</param>
	/// <param name="transform">The transform to apply</param>
	/// <returns>The transformed AABB</returns>
    public static AABB TransformedAABB(this AABB instance, Transform transform) {
        instance.Position += transform.origin;
        return instance;
    }
    
	/// <summary>
	/// Retrieves the top corners of the AABB
	/// </summary>
	/// <param name="instance">The AABB</param>
	/// <param name="transform">The transform to apply to the AABB</param>
	/// <param name="adjust">The adjusting padding value</param>
	/// <returns>An array of Vector3 top corner values</returns>
    public static Vector3[] GetTopCorners(this AABB instance, Transform transform, float adjust = 0f) {
		
        var center = transform.origin;
        var theta = transform.basis.GetEuler().y;
        return new Vector3[] {
			(instance.GetEndpoint(2) - new Vector3(adjust, 0, adjust)).RotateAround(center, theta),
			(instance.GetEndpoint(6) - new Vector3(-adjust, 0, adjust)).RotateAround(center, theta),
			(instance.GetEndpoint(7) - new Vector3(-adjust, 0, -adjust)).RotateAround(center, theta),
			(instance.GetEndpoint(3) - new Vector3(adjust, 0, -adjust)).RotateAround(center, theta)
		};
    }

	/// <summary>
	/// Retrieves the top corners of the AABB constrained in the y axis by the passed value.
	/// </summary>
	/// <param name="instance">The AABB</param>
	/// <param name="heightConstraint">The constrained y value</param>
	/// <param name="adjust">The adjusting padding value</param>
	/// <returns>An array of Vector3 constrained top corner values</returns>
    public static Vector3[] GetTopCornersConstrained(this AABB instance, Transform transform, float heightConstraint, float adjust = 0f) {
        var corners = instance.GetTopCorners(transform, adjust);
		for (int i = 0; i < corners.Length; i++) {
			corners[i] = new Vector3(corners[i].x, heightConstraint, corners[i].z);
		}
		return corners;
    }
}