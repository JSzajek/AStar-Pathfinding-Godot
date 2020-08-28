#pragma once

#include "Vec2.h"

#define VERT_LINE_GRAD 100000

/// <summary>
/// Class representing a 2-dimensional line between
/// two points.
/// </summary>
class Line
{
private:
	float m_gradient, m_yIntercept, m_perpendicularGradient;
	Vec2 m_pointOnLine1;
	Vec2 m_pointOnLine2;
	bool m_approachSide;
public:

	/// <summary>
	/// Initializes a new instance of the <see cref="Line"/> class.
	/// </summary>
	/// <param name="pointOnLine">The first point on the line</param>
	/// <param name="pointPerpToLine">The second point perpendicular to the line</param>
	Line(Vec2 pointOnLine, Vec2 pointPerpToLine);
	
	/// <summary>
	/// Retrieves the y intercept of the line.
	/// </summary>
	/// <returns>The y intercept</returns>
	inline const float GetYIntercept() const { return m_yIntercept; }
	
	/// <summary>
	/// Retrieves the approach side of the line.
	/// </summary>
	/// <returns>The approach side</returns>
	inline const bool GetApproachSide() const { return m_approachSide; }
	
	/// <summary>
	/// Retrieves the gradient of the line.
	/// </summary>
	/// <returns>The gradient</returns>
	inline const float GetGradient() const { return m_gradient; }
	
	/// <summary>
	/// Retrieves the perpendicular gradient of the line.
	/// </summary>
	/// <returns>The perpendicular gradient</returns>
	inline const float GetPerpGradient() const { return m_perpendicularGradient; }
	
	/// <summary>
	/// Retrieves the first point on the line.
	/// </summary>
	/// <returns>The first point</returns>
	inline const Vec2& GetPointOnLine1() const { return m_pointOnLine1; }
	
	/// <summary>
	/// Retrieves the second point on the line.
	/// </summary>
	/// <returns>The second point</returns>
	inline const Vec2& GetPointOnLine2() const { return m_pointOnLine2; }

	/// <summary>
	/// Determines whether the point has crossed the line.
	/// </summary>
	/// <param name="point">The point to check</param>
	/// <returns>Whether the point has crossed the line</returns>
	const bool HasCrossedLine(Vec2 point);
	
	/// <summary>
	/// Calculates the distance from the passed point.
	/// </summary>
	/// <param name="point">The point to check</param>
	/// <returns>The distance from the point</returns>
	const float DistanceFromPoint(Vec2 point);
	
private:

	/// <summary>
	/// Determines which side the passed point is on.
	/// </summary>
	/// <param name="point">The point to check</param>
	/// <returns>The side the point is on</returns>
	const bool GetSide(Vec2 point);
};