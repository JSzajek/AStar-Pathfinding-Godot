#include "pch.h"

#include <vector>
#include "SmoothPath.h"

SmoothPath::SmoothPath(std::vector<Vec3> points, Vec3 start, float turnDist, float stoppingDist)
	: m_lookPoints(points), m_turnBoundaries(std::vector<Line>()), 
		m_finishLineIndex(points.size() - 1), m_slowDownIndex(0)
{
	Vec2 prevPoint = start.ToVec2();
	for (int i = 0; i < m_lookPoints.size(); i++)
	{
		Vec2 currentPoint = m_lookPoints.at(i).ToVec2();
		Vec2 dirToCurrentPoints = currentPoint - prevPoint;
		dirToCurrentPoints.Normalize();
		Vec2 turnBoundaryPoint = i == m_finishLineIndex ? currentPoint : currentPoint - (dirToCurrentPoints * turnDist);
		m_turnBoundaries.push_back(Line(turnBoundaryPoint, prevPoint - (dirToCurrentPoints * turnDist)));
		prevPoint = turnBoundaryPoint;
	}

	float distFromEndPoint = 0;
	for (int i = m_lookPoints.size() - 1; i > 0; i--)
	{
		distFromEndPoint += m_lookPoints.at(i).DistanceTo(m_lookPoints.at(i - 1));
		if (distFromEndPoint > stoppingDist)
		{
			m_slowDownIndex = i;
			break;
		}
	}
}