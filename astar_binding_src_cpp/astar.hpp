#pragma once

#ifndef ASTAR_HPP
#define ASTAR_HPP

#include "vector2.hpp"
#include "vector3.hpp"
#include "pathnode.hpp"
#include "minheap.hpp"
#include "kdtree.hpp"
#include "grid.hpp"
#include <vector>
#include <list>
#include <algorithm>
#include <unordered_set>

using namespace std;

namespace astar
{
	class AStar
	{
	private:
		//Grid* grid;
		KDTree* tree;
		int minPenalty;
		int maxPenalty;
		float nodeRadius;
	public:
		AStar(float _nodeRadius, int _minPenalty, int _maxPenalty);
		~AStar();
		void clearGridNodes();
		void addGridNode(tuple<Vector3, bool, int> point);
		void addGridNodes(deque<tuple<Vector3, bool, int>> points);
		void removeGridNode(Vector3 point);
		void removeGridNodes(deque<Vector3> points);
		PathNode* getGridNode(Vector3 position);
		vector<Vector3> getNearestNeighbors(Vector3 position);
		const vector<std::Vector3> findPath(std::Vector3 _startPosition, std::Vector3 _targetPosition);
		const vector<std::Vector3> retracePath(PathNode _startNode, PathNode _endNode);
		const vector<std::Vector3> simplifyPath(const vector<PathNode> _pathNodes);
		int getDistance(PathNode* nodeA, PathNode* nodeB);
		string GridToString(); // For testing
		//const std::tuple<int, int> blurWeights(int _blurSize);
	};

	class HashFunction
	{
	public:
		size_t operator()(const PathNode& node) const
		{
			return node.hash;
		}
	};
}

#endif // !ASTAR_HPP