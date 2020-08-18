#include"pch.h"
#include "astar.hpp"
#include "astarlinker.hpp"

using namespace std;
using namespace astar;

/// <summary>
/// Initializes a new instance of AStar. (KDTree)
/// </summary>
/// <param _worldSize>The world size of the AStar algorithm</param>
/// <param _minPenalty>The minimum movement penalty</param>
/// <param _maxPenalty>The maximum movement penalty</param>
AStar::AStar(float _nodeRadius, int _minPenalty, int _maxPenalty)
	: minPenalty(_minPenalty), maxPenalty(_maxPenalty), nodeRadius(_nodeRadius), tree(new KDTree(3)),
      offset(Vector3()), grid(NULL)
{
}

/// <summary>
/// Initializes a new instance of AStar. (Grid)
/// </summary>
/// <param name="gridSize">The grid size of the AStar algorithm</param>
/// <param _minPenalty>The minimum movement penalty</param>
/// <param _maxPenalty>The maximum movement penalty</param>
/// <param name="_offset">The world offset value</param>
AStar::AStar(Vector2 gridSize, int _minPenalty, int _maxPenalty, Vector3 _offset) 
	: minPenalty(_minPenalty), maxPenalty(_maxPenalty), offset(_offset),
	  grid(new Grid(gridSize.x, gridSize.y, _minPenalty, _maxPenalty)),
	  nodeRadius(0), tree(NULL)
{
}

/// <summary>
/// Initializes a new instance of AStar. From imported grid data.
/// </summary>
/// <param name="nodes">The nodes and essential data</param>
/// <param name="d1">The dimension of the array</param>
AStar::AStar(float* nodes, int d1) 
{
	importGrid(nodes, d1);
}

/// <summary>
/// AStar destructor
/// </summary>
AStar::~AStar() 
{
	if (tree != NULL) {
		delete tree;
		this->tree = NULL;
	}
	if (grid != NULL) {
		delete grid;
		this->grid = NULL;
	}
}

void AStar::clearGridNodes()
{
	if (tree != NULL) {
		this->tree->Clear();
	}
	if (grid != NULL) {
		this->grid->clearGrid();
	}
}

/// <summary>
/// Adds a new PathNode to the astar grid (KDTree).
/// </summary>
/// <param point>The PathNode to be added</param>
void AStar::addGridNode(tuple<Vector3, bool, int> point)
{
	if (tree != NULL) {
		this->tree->AddNode(point);
	}
}

/// <summary>
/// Adds a new PathNode to the astar grid (Grid).
/// </summary>
/// <param name="node">The path node to add</param>
void AStar::addGridNode(PathNode* node) 
{
	if (grid != NULL) {
		this->grid->addToGrid(node->getGridX(), node->getGridY(), node);
	}
}

/// <summary>
/// Adds the grid nodes from the grid. (KDTree)
/// </summary>
/// <param points>The points to be added</param>
void AStar::addGridNodes(deque<tuple<Vector3, bool, int>> points)
{
	if (tree->IsEmpty()) {
		this->tree = new KDTree(points);
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
/// Adds the grid nodes from the grid. (Grid)
/// </summary>
/// <param points>The points to be added</param>
void AStar::addGridNodes(float* nodes, int d1)
{
	for (int i = 0; i < (nodes[0] - 1) / 7; i++) {
		int base = (i * 7) + 1;
		Vector2 pos = Vector2(nodes[base], nodes[base + 1]);
		grid->addToGrid(pos.x, pos.y,
			new PathNode(Vector3(nodes[base + 2], nodes[base + 3], nodes[base + 4]),
				pos,
				nodes[base + 5],
				nodes[base + 6]));
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
/// Gets the PathNode from the astar grid (KDTree).
/// </summary>
/// <param position>The world position</param>
PathNode* AStar::getGridNode(Vector3 position)
{
	if (tree != NULL) {
		return tree->Nearest(position);
	}
	if (grid != NULL) {
		return grid->nodeFromWorldPoint(position - offset);
	}
	return NULL;
}

/// <summary>
/// Gets the PathNode from the astar grid (Grid).
/// </summary>
/// <param position>The grid coordinates</param>
PathNode* AStar::getGridNode(int gridX, int gridY)
{
	if (grid != NULL) {
		return grid->getGridNode(gridX, gridY);
	}
	return NULL;
}

/// <summary>
/// Nearest neighbors of the astar grid.
/// </summary>
/// <param position>The center position to get neighbors from</param>
vector<Vector3> AStar::getNearestNeighbors(Vector3 position) 
{
	vector<PathNode*> neighbors;
	if (grid != NULL) {
		neighbors = grid->getNeighbors(grid->nodeFromWorldPoint(position - offset));
	}
	else if (tree != NULL) {
		neighbors = tree->Nearest(position, nodeRadius * 2);
	}
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
	PathNode* startNode = getGridNode(_startPosition);
	PathNode* targetNode = getGridNode(_targetPosition);

	MinHeap<PathNode> openSet;
	unordered_set<PathNode, HashFunction> closedSet;

	if (startNode->getWalkable() && targetNode->getWalkable())
	{
		// A* Path finding algorithm
		openSet.Add(startNode);

		while (openSet.size() > 0)
		{
			// This path is taking far to long to compute so just quit - handle differently?
			if (safety > 10000) { break; }

			PathNode* currentNode = openSet.RemoveFirst();
			closedSet.insert(*currentNode);

			if (currentNode->operator==(targetNode)) 
			{
				path_success = true;
				break;
			}

			vector<PathNode*> neighbors;
			if (grid != NULL) 
			{
				neighbors = grid->getNeighbors(currentNode);
			}
			else if (tree != NULL) 
			{
				neighbors = tree->Nearest(currentNode->position, nodeRadius * 2);
			}

			for (int i = 0; i < neighbors.size(); i++)
			{
				PathNode* neighbor = neighbors.at(i);
				if (!neighbor->getWalkable() || closedSet.find(*neighbor) != closedSet.end()) 
				{
					continue;
				}

				int newMoveCostToNeighbor = currentNode->getGCost() + currentNode->ManhattenDistanceTo(*neighbor) + neighbor->getMovementPenalty();
				if (!openSet.Contains(neighbor) || newMoveCostToNeighbor < neighbor->getGCost()) 
				{
					neighbor->setGCost(newMoveCostToNeighbor);
					neighbor->setHCost(neighbor->ManhattenDistanceTo(*targetNode));
					neighbor->setParent(currentNode);
					if (!openSet.Contains(neighbor)) 
					{
						openSet.Add(neighbor);
					}
					else 
					{
						openSet.UpdateItem(neighbor);
					}
				}
			}
			safety++;
		}
	}
	if (path_success) 
	{
		vector<Vector3> path(retracePath(*startNode, *targetNode));

		CleanUp(openSet, closedSet);
		return path;
	}
	else {
		CleanUp(openSet, closedSet);
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
	while (currentNode.operator!=(&_startNode)) 
	{
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
/// Converts the grid into a string useful for debugging,
/// currently only applicable for kd tree.
/// </summary>
/// <returns>The string of the kd tree</returns>
string AStar::GridToString()
{
	return tree->ToString();
}

// TODO: Implement blurring with the kd tree points.

/// <summary>
/// Blurs the movement penalties of the grid.
/// </summary>
/// <param _blurSize>The kernel blur size</param>
const tuple<int, int> AStar::blurWeights(int _blurSize)
{
	if (grid != NULL) {
		return grid->blurPenaltyMap(_blurSize);
	}
	return make_tuple(minPenalty, maxPenalty);
}

/// <summary>
/// Exports the essential values of the grid for saving into
/// a file.
/// </summary>
/// <returns>A tuple of nodes, offset, size, and penalties</returns>
tuple<vector<PathNode>, Vector3, int, int, int, int> AStar::exportGrid()
{
	if (grid != NULL) {
		return make_tuple(grid->exportGrid(), offset, 
						  grid->gridSizeX, grid->gridSizeY,
						  grid->minPenalty, grid->maxPenalty);
	}
	return make_tuple(vector<PathNode>(), Vector3(), 0, 0, 0, 0);
}

/// <summary>
/// Imports a grid from the passed node and essential values.
/// </summary>
/// <param name="nodes">The essential and nodes values</param>
/// <param name="d1">The dimension of the array</param>
void AStar::importGrid(float* nodes, int d1)
{
	if (grid != NULL) { delete grid; }
	offset = Vector3(nodes[2], nodes[3], nodes[4]);
	this->grid = new Grid(nodes[5], nodes[6], nodes[7], nodes[8]);
	
	for (int i = 0; i < (nodes[0] - 9) / 7; i++) 
	{
		int base = 9 + (i * 7);
		Vector2 pos = Vector2(nodes[base], nodes[base + 1]);
		grid->addToGrid(pos.x, pos.y,
						new PathNode(Vector3(nodes[base + 2], nodes[base + 3], nodes[base + 4]), 
										     pos, 
											 nodes[base + 5], 
											 nodes[base + 6]));
	}
}