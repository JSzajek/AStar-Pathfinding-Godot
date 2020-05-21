#include"pch.h"
#include "astar.hpp"
#include "astarlinker.hpp"

using namespace std;
using namespace astar;

/// <summary>
/// Initializes a new instance of AStar
/// </summary>
/// <param _worldSize>The world size of the AStar algorithm</param>
/// <param _minPenalty>The minimum movement penalty</param>
/// <param _maxPenalty>The maximum movement penalty</param>
AStar::AStar(float _nodeRadius, int _minPenalty, int _maxPenalty)
	: minPenalty(_minPenalty), maxPenalty(_maxPenalty), nodeRadius(_nodeRadius) {
	this->tree = new KDTree(3);
}

/// <summary>
/// AStar destructor
/// </summary>
AStar::~AStar() 
{
	tree->~KDTree();
}

void AStar::clearGridNodes()
{
	tree->clear();
}

/// <summary>
/// Adds a new PathNode to the astar grid.
/// </summary>
/// <param point>The PathNode to be added</param>
void AStar::addGridNode(tuple<Vector3, bool, int> point)
{
	tree->AddNode(point);
	//this->gridMap->addToGrid(node->getGridX(), node->getGridY(), node);
}

/// <summary>
/// Adds the grid nodes from the grid. (KD tree only)
/// </summary>
/// <param points>The points to be added</param>
void AStar::addGridNodes(deque<tuple<Vector3, bool, int>> points)
{
	if (tree->IsEmpty()) {
		tree = new KDTree(points);
	}
	else {
		while (!points.empty()) {
			tuple<Vector3, bool, int> val = points.front();
			tree->AddNode(val);
			points.pop_front();
		}
	}
}

/// <summary>
/// Removes the grid node from the grid. (KD tree only)
/// </summary>
/// <param point>The world position</param>
void AStar::removeGridNode(Vector3 point)
{
	tree->RemoveNode(point);
}

/// <summary>
/// Removes the grid nodes from the grid. (KD tree only)
/// </summary>
/// <param points>The world positions</param>
void AStar::removeGridNodes(deque<Vector3> points)
{
	while (!points.empty()) {
		tree->RemoveNode(points.front());
		points.pop_front();
	}
}

/// <summary>
/// Gets the PathNode from the astar grid.
/// </summary>
/// <param position>The world position</param>
PathNode* AStar::getGridNode(Vector3 position)
{
	// return grid->nodeFromWorldPoint(position);
	return tree->Nearest(position);
}

/// <summary>
/// Nearest neighbors of the astar grid.
/// </summary>
/// <param position>The center position to get neighbors from</param>
vector<Vector3> AStar::getNearestNeighbors(Vector3 position) {
	//vector<PathNode> neighbors = grid->getNeighbors(grid->nodeFromWorldPoint(position));
	vector<PathNode*> neighbors = tree->Nearest(position, nodeRadius * 2);
	vector<Vector3> vecs;
	for (PathNode* node : neighbors) {
		vecs.push_back(node->position);
	}
	return vecs;
}

/// <summary>
/// Gets a path from the start point to the target point in the form of a list of world points.
/// </summary>
/// <param _startPosition>The start world position</param>
/// <param _targetPosition>The target world position</param>
const vector<Vector3> AStar::findPath(Vector3 _startPosition, Vector3 _targetPosition)
{
	int safety = 0;
	bool path_success = false;
	//PathNode* startNode = grid->nodeFromWorldPoint(_startPosition);
	PathNode* startNode = tree->Nearest(_startPosition);
	//PathNode* targetNode = grid->nodeFromWorldPoint(_targetPosition);
	PathNode* targetNode = tree->Nearest(_targetPosition);

	MinHeap<PathNode> openSet;
	unordered_set<PathNode, HashFunction> closedSet;

	if (startNode->getWalkable() && targetNode->getWalkable())
	{
		// A* Path finding algorithm
		openSet.Add(startNode);

		while (openSet.size() > 0)
		{
			if (safety > 10000) {
				// This path is taking far to long to compute so just quite - should handle differently?
				break;
			}
			PathNode* currentNode = openSet.RemoveFirst();
			closedSet.insert(*currentNode);

			if (currentNode->operator==(targetNode)) {
				path_success = true;
				break;
			}
			//vector<PathNode> neighbors = grid->getNeighbors(currentNode);
			vector<PathNode*> neighbors = tree->Nearest(currentNode->position, nodeRadius * 2);
			for (int i = 0; i < neighbors.size(); i++)
			{
				PathNode* neighbor = neighbors.at(i);
				if (!neighbor->getWalkable() || closedSet.find(*neighbor) != closedSet.end()) {
					continue;
				}
				int newMoveCostToNeighbor = (currentNode->getGCost() + getDistance(currentNode, neighbor)) + neighbor->getMovementPenalty();
				if (!openSet.Contains(neighbor) || newMoveCostToNeighbor < neighbor->getGCost()) {
					neighbor->setGCost(newMoveCostToNeighbor);
					neighbor->setHCost(getDistance(neighbor, targetNode));
					neighbor->setParent(currentNode);
					if (!openSet.Contains(neighbor)) {
						openSet.Add(neighbor);
					}
					else {
						openSet.UpdateItem(neighbor);
					}
				}
			}
			safety++;
		}
	}
	if (path_success) {
		vector<Vector3> path(retracePath(*startNode, *targetNode));

		// Clean up - reset the nodes costs for next cycle
		openSet.CleanUp();
		for (PathNode p : closedSet) {
			p.CleanUp();
		}
		return path;
	}
	else {
		// Clean up - reset the nodes costs for next cycle
		openSet.CleanUp();
		for (PathNode p : closedSet) {
			p.CleanUp();
		}
		return {};
	}
}

/// <summary>
/// Retraces the path.
/// </summary>
/// <param _startNode>The start node</param>
/// <param _endNode>The end node</param>
const vector<Vector3> AStar::retracePath(PathNode _startNode, PathNode _endNode)
{
	vector<PathNode> pathNodes = vector<PathNode>();
	PathNode currentNode = _endNode;
	while (currentNode.operator!=(&_startNode)) {
		pathNodes.push_back(currentNode);
		currentNode = *currentNode.getParent();
	}

	vector<Vector3> waypoints = simplifyPath(pathNodes);
	reverse(waypoints.begin(), waypoints.end());
	return waypoints;
}

/// <summary>
/// Simplifies the passed path list of world points. 
/// Removing duplicates of same slope.
/// </summary>
/// <param _pathNodes>The path or the list of nodes</param>
const vector<Vector3> AStar::simplifyPath(vector<PathNode> _pathNodes)
{
	vector<Vector3> waypoints = vector<Vector3>();
	float directionOld = -5; // An impossible direction
	for (size_t i = 0; i < _pathNodes.size(); i++)
	{
		if (i == 0) {
			waypoints.push_back(_pathNodes.at(i).position);
			continue;
		}

		PathNode prior = _pathNodes.at(i - 1);
		PathNode current = _pathNodes.at(i);
		float directionNew = prior.position.DirectionTo(current.position);

		if (!(abs(directionOld - directionNew) < FLT_EPSILON))
		{
			waypoints.push_back(_pathNodes.at(i).position);
		}
		directionOld = directionNew;
	}
	return waypoints;
}

/// <summary>
/// Gets the distance between two PathNodes.
/// </summary>
/// <param nodeA>The comparison node</param>
/// <param nodeB>The other node</param>
int AStar::getDistance(PathNode* nodeA, PathNode* nodeB)
{
	return ceil(nodeA->position.ManhattenDistanceTo(nodeB->position));
}

string AStar::GridToString()
{
	return tree->ToString();
}

// TODO: Implement blurring with the new tree points??

/// <summary>
/// BBlurs the movement penalties of the grid.
/// </summary>
/// <param _blurSize>The kernel blur size</param>
//const std::tuple<int, int> AStar::blurWeights(int _blurSize)
//{
//	
//
//	//return gridMap->blurPenaltyMap(_blurSize);
//	return null;
//}