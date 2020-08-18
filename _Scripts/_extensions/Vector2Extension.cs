using Godot;

/// <summary>
/// Extension class for Vector2 class.
/// </summary>
public static class Vector2Extension {

    /// <summary>
	/// Converts a Vector3 into a Vector2 based on XZ
	/// </summary>
	/// <param name="vector">The Vector3 to convert</param>
	/// <returns>A Vector2 representation</returns>
	public static Vector2 To2D(this Vector3 vector) {
		return new Vector2(vector.x, vector.z);
	}
}