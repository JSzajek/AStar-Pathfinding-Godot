#ifndef SMOOTHPATH_HPP
#define SMOOTHPATH_HPP

#include <vector>
#include "vector3.hpp"
#include "vector2.hpp"
#include "line.hpp"

using namespace std;

namespace astar
{
	class SmoothPath
	{
	private:
		vector<std::Vector3> lookPoints;
		vector<Line> turnBoundaries;
		int finishLineIndex;
		int slowDownIndex;
	public:
		SmoothPath(vector<std::Vector3> waypoints, std::Vector3 startPos, float turnDist, float stoppingDist);
		~SmoothPath();

		const vector<std::Vector3> getLookPoints() { return lookPoints; }
		const vector<Line> getTurnBoundaries() { return turnBoundaries; }

		int getTurnBoundariesSize() { return turnBoundaries.size(); }
		int getSlowDownIndex() { return slowDownIndex; }
		int getFinishLineIndex() { return finishLineIndex; }

		Line getLine(int _index) { return turnBoundaries.at(_index); }

		float distanceFromPoint(int _index, std::Vector2 position) { return turnBoundaries.at(_index).distanceFromPoint(position); }
		bool crossedTurnBoundary(int _index, std::Vector2 position) { return turnBoundaries.at(_index).hasCrossedLine(position); }
		std::Vector3 getLookPoint(int _index) { return lookPoints.at(_index); }

		std::Vector2 V3toV2(std::Vector3 vector);
	};
}

#endif // !SMOOTHPATH_HPP
