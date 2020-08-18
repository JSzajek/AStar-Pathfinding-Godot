using Godot;

/// <summary>
/// Extension class for Vector3 class.
/// </summary>
public static class Vector3Extension {
    
    /// <summary>
	/// Extended Rotate method that rotates the vector around a given center point.
	/// </summary>
	/// <param name="vec">The vector to rotate</param>
	/// <param name="center">The center point</param>
	/// <param name="theta">The amount of rotation to apply</param>
	/// <returns>Rotated Vector3</returns>
	public static Vector3 RotateAround(this Vector3 vec, Vector3 center, float theta) {
		return (vec-center).Rotated(Vector3.Up, theta) + center;
	}
}