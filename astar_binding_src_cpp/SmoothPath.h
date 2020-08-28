#pragma once

#include <vector>
#include "Vec2.h"
#include "Vec3.h"
#include "Line.h"

/// <summary>
/// Class representing a smoothed path.
/// </summary>
class SmoothPath
{
private:
	std::vector<Vec3> m_lookPoints;
	std::vector<Line> m_turnBoundaries;
	int m_slowDownIndex, m_finishLineIndex;
public:

	/// <summary>
	/// Initializes a new instance of the <see cref="SmoothPath"/> class.
	/// </summary>
	/// <param name="points">The points of the path to smooth</param>
	/// <param name="start">The start coordinate</param>
	/// <param name="turnDist">The turn distance</param>
	/// <param name="stoppingDist">The stopping distance</param>
	SmoothPath(std::vector<Vec3> points, Vec3 start, float turnDist, float stoppingDist);

	/// <summary>
	/// Retrieves the look points of the smooth path.
	/// </summary>
	/// <returns>The collection of look points</returns>
	inline const std::vector<Vec3> GetLookPoints() const { return m_lookPoints; }
	
	/// <summary>
	/// Retrieves the turn boundaries of the smooth path.
	/// </summary>
	/// <returns>The collection of turn boundaries</returns>
	inline const std::vector<Line> GetTurnBoundaries() const { return m_turnBoundaries; }

	/// <summary>
	/// Retrieves size of the turn boundaries of the smooth path.
	/// </summary>
	/// <returns>The turn boundaries size</returns>
	inline const int GetTurnBoundariesSize() const { return m_turnBoundaries.size(); }
	
	/// <summary>
	/// Retrieves the slow down index of the smooth path. 
	/// </summary>
	/// <returns>The slow down index</returns>
	inline const int GetSlowDownIndex() const { return m_slowDownIndex; }
	
	/// <summary>
	/// Retrieves the finish line index of the smooth path.
	/// </summary>
	/// <returns>The finish line index</returns>
	inline const int GetFinishLineIndex() const { return m_finishLineIndex; }
};