#include"pch.h"
#include "smoothpath.hpp"
#include "astarlinker.hpp"
#include <string>

using namespace std;
using namespace astar;

/// <summary>
/// Initializes a new instance of SmoothPath
/// </summary>
/// <param waypoints>The list of world position points</param>
/// <param startPos>The starting world position position</param>
/// <param turnDist>The turn distance for the new path</param>
/// <param stoppingDist>The stopping distance for the new path</param>
SmoothPath::SmoothPath(vector<std::Vector3> waypoints, std::Vector3 startPos, float turnDist, float stoppingDist)
{
	this->lookPoints = waypoints;
	this->turnBoundaries = vector<astar::Line>();
	this->finishLineIndex = this->lookPoints.size() - 1;

	Vector2 prevPoint = V3toV2(startPos);
	for (int i = 0; i < lookPoints.size(); i++)
	{
		Vector2 currentPoint = V3toV2(lookPoints.at(i));
		Vector2 dirToCurrentPoints = (currentPoint - prevPoint).Normalize();
		Vector2 turnBoundaryPoint = i == finishLineIndex ? currentPoint : currentPoint - (dirToCurrentPoints * turnDist);
		turnBoundaries.push_back(Line(turnBoundaryPoint, prevPoint - (dirToCurrentPoints * turnDist)));
		prevPoint = turnBoundaryPoint;
	}

	float distFromEndPoint = 0;
	for (int i = lookPoints.size() - 1; i > 0; i--)
	{
		distFromEndPoint += lookPoints.at(i).DistanceTo(lookPoints.at(i - 1));
		if (distFromEndPoint > stoppingDist)
		{
			slowDownIndex = i;
			break;
		}
	}
}

/// <summary>
/// SmoothPath destructor
/// </summary>
SmoothPath::~SmoothPath() { }

/// <summary>
/// Converts a Vector 3 to Vector 2
/// </summary>
/// <param vector>The Vector 3 to be converted</param>
Vector2 SmoothPath::V3toV2(Vector3 vector)
{
	return Vector2(vector.x, vector.z);
}