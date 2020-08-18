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
	class HashFunction
	{
	public:
		size_t operator()(const PathNode& node) const
		{
			return node.hash;
		}
	};

	class AStar
	{
	private:
		Grid* grid;
		KDTree* tree;
		Vector3 offset;
		int minPenalty;
		int maxPenalty;
		float nodeRadius;

		void importGrid(float* nodes, int d1);
		
		// Clean up - reset the nodes costs for next cycle
		void CleanUp(MinHeap<PathNode>& open, unordered_set<PathNode, HashFunction>& closed)
		{
			open.CleanUp();
			for (PathNode p : closed)
			{
				p.CleanUp();
			}
		}

	public:
		AStar(float* nodes, int d1);

		AStar(float _nodeRadius, int _minPenalty, int _maxPenalty);
		AStar(Vector2 gridSize, int _minPenalty, int _maxPenalty, Vector3 offset);
		~AStar();
		void clearGridNodes();
		void addGridNode(tuple<Vector3, bool, int> point);
		void addGridNode(PathNode* node);
		void addGridNodes(deque<tuple<Vector3, bool, int>> points);
		void addGridNodes(float* nodes, int d1);
		void removeGridNode(Vector3 point);
		void removeGridNodes(deque<Vector3> points);
		PathNode* getGridNode(Vector3 position);
		PathNode* getGridNode(int gridX, int gridY);
		const tuple<int, int> getPenalties() { return make_tuple(minPenalty, maxPenalty); }
		vector<Vector3> getNearestNeighbors(Vector3 position);
		const vector<std::Vector3> findPath(std::Vector3 _startPosition, std::Vector3 _targetPosition);
		const vector<std::Vector3> retracePath(PathNode _startNode, PathNode _endNode);
		const vector<std::Vector3> simplifyPath(const vector<PathNode> _pathNodes);
		string GridToString(); // For testing
		const tuple<int, int> blurWeights(int _blurSize);

		tuple<vector<PathNode>, Vector3, int, int, int, int> exportGrid();
	};
}

#endif // !ASTAR_HPP