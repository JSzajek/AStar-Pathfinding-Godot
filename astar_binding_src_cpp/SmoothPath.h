#pragma once

#include <vector>
#include "Vec2.h"
#include "Vec3.h"
#include "Line.h"

class SmoothPath
{
private:
	std::vector<Vec3> m_lookPoints;
	std::vector<Line> m_turnBoundaries;
	int m_slowDownIndex, m_finishLineIndex;
public:
	SmoothPath(std::vector<Vec3> points, Vec3 start, float turnDist, float stoppingDist);

	inline const std::vector<Vec3> GetLookPoints() const { return m_lookPoints; }
	inline const std::vector<Line> GetTurnBoundaries() const { return m_turnBoundaries; }

	inline const int GetTurnBoundariesSize() const { return m_turnBoundaries.size(); }
	inline const int GetSlowDownIndex() const { return m_slowDownIndex; }
	inline const int GetFinishLineIndex() const { return m_finishLineIndex; }

	inline const Line& GetLine(int index) const { return m_turnBoundaries[index]; }
	inline const Vec3& GetLookPoint(int index) const { return m_lookPoints[index]; }

	float DistanceFromPoint(int index, Vec2 position);
	bool CrossedTurnBoundary(int index, Vec2 position);
};