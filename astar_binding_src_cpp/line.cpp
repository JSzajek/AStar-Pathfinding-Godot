#include "pch.h"

#include "Line.h"

Line::Line(Vec2 pointOnLine, Vec2 pointPerpToLine)
{
	float dx = pointOnLine.x - pointPerpToLine.x;
	float dy = pointOnLine.y - pointPerpToLine.y;

	if (dx == 0) 
	{
		m_perpendicularGradient = VERT_LINE_GRAD;
	}
	else 
	{
		m_perpendicularGradient = dy / dx;
	}

	if (m_perpendicularGradient == 0) 
	{
		m_gradient = VERT_LINE_GRAD;
	}
	else 
	{
		m_gradient = -1.0f / m_perpendicularGradient;
	}

	//this->perpendicularGradient = dx == 0 ? VERT_LINE_GRAD : dy / dx;
	//this->gradient = perpendicularGradient == 0 ? VERT_LINE_GRAD : -1.0f / perpendicularGradient;

	m_yIntercept = pointOnLine.y - m_gradient * pointOnLine.x;
	m_pointOnLine1 = pointOnLine;
	m_pointOnLine2 = pointOnLine + Vec2(1, m_gradient);

	m_approachSide = false;
	m_approachSide = GetSide(pointPerpToLine);
}

const bool Line::GetSide(Vec2 point)
{
	return (point.x - m_pointOnLine1.x) * (m_pointOnLine2.y - m_pointOnLine1.y)
				> (point.y - m_pointOnLine1.y) * (m_pointOnLine2.x - m_pointOnLine1.x);
}

const bool Line::HasCrossedLine(Vec2 point)
{
	return GetSide(point) != m_approachSide;
}

const float Line::DistanceFromPoint(Vec2 point)
{
	float yInterceptPerp = point.y - m_perpendicularGradient * point.x;
	float intersectX = (yInterceptPerp - m_yIntercept) / (m_gradient - m_perpendicularGradient);
	float intersectY = m_gradient * intersectX + m_yIntercept;
	return point.DistanceTo(Vec2(intersectX, intersectY));
}