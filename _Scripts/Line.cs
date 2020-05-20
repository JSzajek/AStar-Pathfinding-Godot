using Godot;

/// <summary>
/// Struct representing a line and related methods
/// </summary>
public struct Line {
	public float gradient, perpGradient, yIntercept;
	public Vector2 pointLine1, pointLine2;
	public bool approachSide;

	/// <summary>
	/// Constructor initializing the Line.
	/// </summary>
	/// <param name="gradient">The gradient of the line</param>
	/// <param name="perpGradient">The perpendicular gradient of the line</param>
	/// <param name="point1">The point on the line</param>
	/// <param name="point2">The point perpendicular to the line</param>
	/// <param name="approachSide">The current approach side</param>
	public Line(float gradient, float perpGradient, Vector2 point1, Vector2 point2, int approachSide) {
		this.gradient = gradient;
		this.perpGradient = perpGradient;
		this.pointLine1 = point1;
		this.pointLine2 = point2;
		this.approachSide = approachSide == 1;
		yIntercept = point1.y - gradient * point1.x;
	}

	/// <summary>
	/// Getter method that gets the side of the passed point.
	/// </summary>
	/// <param name="point">The passed point</param>
	/// <returns>The side the point represented as a bool (infront or behind)</returns>
	public bool GetSide(Vector2 point) {
		return (point.x - pointLine1.x) * (pointLine2.y - pointLine1.y) > (point.y - pointLine1.y) * (pointLine2.x - pointLine1.x);
	}

	/// <summary>
	/// Determines whether the passed point has crossed the line.
	/// </summary>
	/// <param name="point">The passed point</param>
	/// <returns>boolean of whether the point has crossed the line</returns>
	public bool HasCrossedLine(Vector2 point) {
		return GetSide(point) != approachSide;
	}

	/// <summary>
	/// Gets the distance of the line to the passed point.
	/// </summary>
	/// <param name="point">The passed point</param>
	/// <returns>The distance of the line to the point</returns>
	public float DistanceFromPoint(Vector2 point) {
		float yInterceptPerp = point.y - perpGradient * point.x;
		float xIntersect = (yInterceptPerp - yIntercept) / (gradient - perpGradient);
		float yIntersect = gradient * xIntersect + yIntercept;
		return point.DistanceTo(new Vector2(xIntersect, yIntersect));
	}
}
