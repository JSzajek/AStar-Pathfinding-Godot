#pragma once

#include "Vec2.h"

#define VERT_LINE_GRAD 100000

class Line
{
private:
	float m_gradient, m_yIntercept, m_perpendicularGradient;
	Vec2 m_pointOnLine1;
	Vec2 m_pointOnLine2;
	bool m_approachSide;
public:
	Line();
	Line(Vec2 pointOnLine, Vec2 pointPerpToLine);
	
	inline const float GetYIntercept() const { return m_yIntercept; }
	inline const bool GetApproachSide() const { return m_approachSide; }
	inline const float GetGradient() const { return m_gradient; }
	inline const float GetPerpGradient() const { return m_perpendicularGradient; }
	inline const Vec2& GetPointOnLine1() const { return m_pointOnLine1; }
	inline const Vec2& GetPointOnLine2() const { return m_pointOnLine2; }

	const bool GetSide(Vec2 point);
	const bool HasCrossedLine(Vec2 point);
	const float DistanceFromPoint(Vec2 point);
};