using Godot;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Extension class for MeshInstance class.
/// </summary>
public static class MeshInstanceExtension {

	public static List<(Vector3, bool, int)> GetPoints(this MeshInstance instance) {
		return null;
	}

	public static Vector3[] GetTopCorners(this MeshInstance instance, float adjust = 0f) {

		var markers = instance.GetChildren<Position3D>();
		if (markers.Count > 0) {
			return markers.Select(pos => pos.GlobalTransform.origin).ToArray();
		}

		var aabb = instance.GetAabb();
		var scale = instance.Scale;
		var theta = instance.Rotation.y;
		var center = instance.GlobalTransform.origin;
		return new Vector3[] {
			((aabb.GetEndpoint(2) + new Vector3(-adjust, 0, -adjust)) * scale).RotateAround(center, theta),
			((aabb.GetEndpoint(6) + new Vector3(adjust, 0, -adjust)) * scale).RotateAround(center, theta),
			((aabb.GetEndpoint(7) + new Vector3(adjust, 0, adjust)) * scale).RotateAround(center, theta),
			((aabb.GetEndpoint(3) + new Vector3(-adjust, 0, adjust)) * scale).RotateAround(center, theta),
		};
	}

	/// <summary>
	/// Helper method to rotate the vector around the center point
	/// </summary>
	/// <param name="vec">The vector to rotate</param>
	/// <param name="center">The center point</param>
	/// <param name="theta">The amount of rotation to apply</param>
	/// <returns>Rotated Vector3</returns>
	private static Vector3 RotateAround(this Vector3 vec, Vector3 center, float theta) {
		return vec.Rotated(Vector3.Up, theta) + center;
	}

	// TODO: Move to own extension class
	// Reference: https://stackoverflow.com/questions/26291609/converting-jagged-array-to-2d-array-c-sharp/51450057#51450057
	/// <summary>
	/// Converts the potential jagged 2 dimensional into a rectangular array 
	/// </summary>
	/// <param name="source">The jagged 2 dimensional array</param>
	/// <typeparam name="T">The type of the array</typeparam>
	/// <returns>Rectangular array of same type</returns>
	public static T[,] To2D<T>(this T[][] source) {
		try {
			var FirstDim = source.Length;
			var SecondDim = source.GroupBy(row => row.Length).Single().Key;

			var result = new T[FirstDim, SecondDim];
			for (var i = 0; i < FirstDim; ++i)
			for (var j = 0; j < SecondDim; ++j)
				result[i, j] = source[i][j];
			return result;
		}
		catch (InvalidOperationException) {
			throw new InvalidOperationException("The given jagged array is not rectangular.");
		}
	}

	// TODO: Move to own extension class
	/// <summary>
	/// Converts a Vector3 into a Vector2 based on XZ
	/// </summary>
	/// <param name="vector">The Vector3 to convert</param>
	/// <returns>A Vector2 representation</returns>
	public static Vector2 To2D(this Vector3 vector) {
		return new Vector2(vector.x, vector.z);
	}
}