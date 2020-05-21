#include "pch.h"
#include "astarlinker.hpp"

using namespace astarlinker;
using namespace astar;
using namespace std;

//fstream AstarLinker::debugFile;
AStar* AstarLinker::astar;

// Destructor
AstarLinker::~AstarLinker()
{
	// clean up astar
	//free(_astar);
	//debugFile.close();
}

// Initializes the astar grid
void AstarLinker::setupGrid(float _nodeRadius, int _minPenalty, int _maxPenalty)
{
	//debugFile = fstream();
	//debugFile.open("astarlinker_debug.txt", fstream::in | fstream::out | fstream::trunc);
	astar = new AStar(_nodeRadius, _minPenalty, _maxPenalty);
}

// Clears the astar grid
void AstarLinker::clearGrid() 
{
	astar->clearGridNodes();
}

//void AstarLinker::writeToDebug(std::string str)
//{
//	debugFile << str << endl;
//}

// Adds the passed point to the astar grid
void AstarLinker::addGridPoint(Vector3 pos, bool walkable, int movePenalty)
{
	astar->addGridNode(make_tuple(pos, walkable, movePenalty));
}

// Adds the passed points to the astar grid
void AstarLinker::addGridPoints(float* points, int d1, int d2)
{
	// TODO: Look into passing along float pointer and utilizing it instead of rewrapping the array.
	deque<tuple<Vector3, bool, int>> deque;
	for (int i = 0; i < d1; i++) {
		deque.push_back(make_tuple(Vector3(points[(i * d2) + 0], points[(i * d2) + 1], points[(i * d2) + 2]), points[(i * d2) + 3] == 1, (int)points[(i * d2) + 4]));
	}
	astar->addGridNodes(deque);
}

// Removes the passed point from the astar grid
void AstarLinker::removeGridPoint(Vector3 pos)
{
	astar->removeGridNode(pos);
}

// Removes the passed points from the astar grid
void AstarLinker::removeGridPoints(float* points, int d1, int d2)
{
	deque<Vector3> deque;
	for (int i = 0; i < d1; i++) {
		deque.push_back(Vector3(points[(i * d2) + 0], points[(i * d2) + 1], points[(i * d2) + 2]));
	}
	astar->removeGridNodes(deque);
}

// Gets the grid point closest to the passed point
float* AstarLinker::getGridPoint(std::Vector3 position)
{
	PathNode* node = astar->getGridNode(position);
	// Convert to int array with following format:
	// [worldPos.x, worldPos.y, worldPos.z, movePenalty, walkable (0 - false, 1 - true)]
	
	std::Vector3 worldPos = node->position;
	return new float[5] {
		worldPos.x,
		worldPos.y,
		worldPos.z,
		(float)node->getMovementPenalty(),
		(float)(node->getWalkable() ? 1 : 0)
	};
}

// Gets a list of the nearest neighbors to the passed point
float* AstarLinker::getNearestNeighbor(Vector3 position)
{
	vector<Vector3> neighbors = astar->getNearestNeighbors(position);
	return unpackRegularPath(neighbors);
}

// Finds the path from the start point to the end point
float* AstarLinker::findPath(std::Vector3 start, std::Vector3 end, bool smooth, float turnDist, float stopDist)
{
	std::vector<std::Vector3> waypoints = astar->findPath(start, end);
	if (smooth) {
		return unpackSmoothPath(new SmoothPath(waypoints, start, turnDist, stopDist));
	}
	else {
		return unpackRegularPath(waypoints);
	}
}

// Unpacks the smooth path object into an array of floats representing the path
float* AstarLinker::unpackSmoothPath(SmoothPath* path) {
	std::vector<float> unpacked_waypoints = vector<float>();
	vector<std::Vector3> points = path->getLookPoints();
	vector<Line> turns = path->getTurnBoundaries();

	// First index as indicator for size of array
	unpacked_waypoints.push_back((points.size() * 3) + (turns.size() * 7) + 3);

	// Push back finish line and slow down indices
	unpacked_waypoints.push_back(path->getFinishLineIndex());
	unpacked_waypoints.push_back(path->getSlowDownIndex());

	for (int i = 0; i < points.size(); i++) {
		// Adding waypoint vector3
		unpacked_waypoints.push_back(points[i].x);
		unpacked_waypoints.push_back(points[i].y);
		unpacked_waypoints.push_back(points[i].z);

		// Adding Line values
		unpacked_waypoints.push_back(turns[i].getGradient());
		unpacked_waypoints.push_back(turns[i].getPerpGradient());
		unpacked_waypoints.push_back(turns[i].getPointOnLine1().x);
		unpacked_waypoints.push_back(turns[i].getPointOnLine1().y);
		unpacked_waypoints.push_back(turns[i].getPointOnLine2().x);
		unpacked_waypoints.push_back(turns[i].getPointOnLine2().y);
		unpacked_waypoints.push_back(turns[i].getApproachSide() ? 1.0f : 0.0f);
	}
	
	float* data = new float[unpacked_waypoints.size()];
	std::copy(unpacked_waypoints.begin(), unpacked_waypoints.end(), data);
	return data;
}

// Unpacks a regular path into an array of floats representing the path
float* AstarLinker::unpackRegularPath(std::vector<std::Vector3> points) {
	std::vector<float> unpacked_waypoints = vector<float>();
	// First index as indicator for size of array
	unpacked_waypoints.push_back((points.size() * 3) + 1);
	for (int i = 0; i < points.size(); i++)
	{
		unpacked_waypoints.push_back(points[i].x);
		unpacked_waypoints.push_back(points[i].y);
		unpacked_waypoints.push_back(points[i].z);
	}

	float* data = new float[unpacked_waypoints.size()];
	std::copy(unpacked_waypoints.begin(), unpacked_waypoints.end(), data);
	return data;
}

// Debugging to string method
string AstarLinker::ToString()
{
	return astar->GridToString();
}

// Blurs the weights of the grid (not implemented for kdtree)
//std::tuple<int, int> AstarLinker::blurWeight(int _blurSize)
//{
//	return astar->blurWeights(_blurSize);
//}