#include"pch.h"
#include "line.hpp"

using namespace std;
using namespace astar;

/// <summary>
/// Initializes a new default instance of Line
/// </summary>
Line::Line()
{
	Line(Vector2(), Vector2());
}

/// <summary>
/// Initializes a new instance of Line
/// </summary>
/// <param _pointOnLine>The point on the line</param>
/// <param _pointPerpToLine>The point perpendicular to the line</param>
Line::Line(Vector2 _pointOnLine, Vector2 _pointPerpToLine)
{
	float dx = _pointOnLine.x - _pointPerpToLine.x;
	float dy = _pointOnLine.y - _pointPerpToLine.y;

	if (dx == 0) {
		this->perpendicularGradient = VERT_LINE_GRAD;
	}
	else {
		this->perpendicularGradient = dy / dx;
	}

	if (this->perpendicularGradient == 0) {
		this->gradient = VERT_LINE_GRAD;
	}
	else {
		this->gradient = -1.0f / perpendicularGradient;
	}

	//this->perpendicularGradient = dx == 0 ? VERT_LINE_GRAD : dy / dx;
	//this->gradient = perpendicularGradient == 0 ? VERT_LINE_GRAD : -1.0f / perpendicularGradient;

	this->yIntercept = _pointOnLine.y - gradient * _pointOnLine.x;
	pointOnLine1 = _pointOnLine;
	pointOnLine2 = _pointOnLine + Vector2(1, gradient);

	this->approachSide = false;
	this->approachSide = getSide(_pointPerpToLine);
}

/// <summary>
/// Line destructor
/// </summary>
Line::~Line() { }

/// <summary>
/// Gets whether the point is on side of the Line.
/// </summary>
/// <param point>The point</param>
bool Line::getSide(Vector2 point)
{
	return (point.x - pointOnLine1.x) * (pointOnLine2.y - pointOnLine1.y) 
				> (point.y - pointOnLine1.y) * (pointOnLine2.x - pointOnLine1.x);
}

/// <summary>
/// Gets whether the point is has crossed the Line.
/// </summary>
/// <param point>The point</param>
bool Line::hasCrossedLine(Vector2 point)
{
	return getSide(point) != approachSide;
}

/// <summary>
/// Gets whether the distance of the Line from the point.
/// </summary>
/// <param point>The point</param>
float Line::distanceFromPoint(Vector2 point)
{
	float yInterceptPerp = point.y - perpendicularGradient * point.x;
	float intersectX = (yInterceptPerp - yIntercept) / (gradient - perpendicularGradient);
	float intersectY = gradient * intersectX + yIntercept;
	return point.DistanceTo(Vector2(intersectX, intersectY));
}