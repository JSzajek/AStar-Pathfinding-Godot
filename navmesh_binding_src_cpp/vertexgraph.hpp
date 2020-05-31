#ifndef VERTEXGRAPH_HPP
#define VERTEXGRAPH_HPP

#include <tuple>
#include <map>
#include <set>
#include <list>
#include <unordered_set>
#include <queue>
#include "vector3.hpp"
#include "minheap.hpp"
#include "navmesh.hpp"

namespace std
{
	class GraphEdge {
	public:
		Vector3 from;
		Vector3 to;
		float distance;
		int penalty = 0;
		int fcost = 0;
		int hcost = 0;
		int gcost = 0;
		GraphEdge* previous = NULL; 
		size_t id;

		GraphEdge(GraphEdge* edge) : from(edge->from), to(edge->to), distance(edge->distance),
			penalty(edge->penalty), id(edge->id) {}

		GraphEdge(Vector3 from, Vector3 to) : from(from), to(to) 
		{
			distance = from.DistanceTo(to);

			id = hash<float>()(to.x) ^ hash<float>()(to.y) << 2 ^ hash<float>()(to.z) >> 2;
			id = (id * 397) ^ hash<float>()(from.x) ^ hash<float>()(from.y) << 2 ^ hash<float>()(from.z) >> 2;
		}

		bool operator==(const GraphEdge& t) const {
			return this->id == t.id;
		}
	};

	struct GraphEdgeComparer {
		size_t operator()(const GraphEdge& t) const
		{
			return t.id;
		}

		bool operator()(GraphEdge const& g1, GraphEdge const& g2) const
		{
			return g1.id < g2.id;
		}
	};

	typedef std::pair<Vector3, unordered_set<GraphEdge, GraphEdgeComparer>*> MyPair;

	class Graph {
	public:
		map<Vector3, unordered_set<GraphEdge, GraphEdgeComparer>*, Vector3Comparer>* Adjacencies;

		Graph() {
			Adjacencies = new map<Vector3, unordered_set<GraphEdge, GraphEdgeComparer>*, Vector3Comparer>();
		}

		Graph(vector<Vector3> vertices, vector<tuple<Vector3, Vector3>> edges): Graph() {
			for (Vector3 vert : vertices) {
				AddVertex(vert);
			}

			for (tuple<Vector3, Vector3> edge : edges) {
				AddEdge(edge);
			}
		}

		void AddVertex(Vector3 vertex) 
		{
			Adjacencies->insert(MyPair(vertex, new unordered_set<GraphEdge, GraphEdgeComparer>()));
		}

		void AddEdge(tuple<Vector3, Vector3> edge) {
			Vector3 to = get<0>(edge);
			Vector3 from = get<1>(edge);

			if (Adjacencies->find(to) != Adjacencies->end() && 
				Adjacencies->find(from) != Adjacencies->end()) 
			{
				Adjacencies->operator[](to)->insert(GraphEdge(to, from));
				Adjacencies->operator[](from)->insert(GraphEdge(from, to));
			}
		}
	};

	struct VertexNode {
	public:
		Vector3 position;
		int penalty = 0;
		int fcost = 0;
		int hcost = 0;
		int gcost = 0;
		VertexNode* previous = NULL;
		size_t id;

		VertexNode(Vector3 position) : position(position) 
		{ 
			id = hash<float>()(position.x) ^ hash<float>()(position.y) << 2 ^ hash<float>()(position.z) >> 2;
		}

		void updateCost(int gcost, int hcost) {
			this->gcost = gcost;
			this->hcost = hcost;
			this->fcost = gcost + hcost;
		}

		bool operator==(const VertexNode* t) const {
			return id == t->id;
		}

		bool operator==(const VertexNode& t) const {
			return id == t.id;
		}
	};

	struct VertexEquality {
		size_t operator()(const VertexNode& t) const
		{
			return t.id;
		}
	};

	struct VertexComparer {
		bool operator()(VertexNode const* v1, VertexNode const* v2) const
		{
			int compare = v1->fcost - v2->fcost;
			return compare == 0 ? v1->hcost - v2->hcost < 0 : compare < 0;
		}
	};

	class VertexGraph {
	public:
		Graph* graph;

		VertexGraph() 
		{
			graph = new Graph();
		}

		VertexGraph(vector<Vector3> vertices, vector<tuple<Vector3, Vector3>> edges) {
			graph = new Graph(vertices, edges);
		}

		void AddVertex(Vector3 vertex) {
			graph->AddVertex(vertex);
		}

		void AddEdge(tuple<Vector3, Vector3> edge) {
			graph->AddEdge(edge);
		}

		vector<Vector3> FindShortest(Vector3 start, Vector3 target, bool exact = false) {
			int safety = 0;
			bool path_success = false;
			set<VertexNode*, VertexComparer> openSet;
			unordered_set<VertexNode, VertexEquality> opened;
			unordered_set<VertexNode, VertexEquality> closedSet;

			openSet.insert(new VertexNode(start));
			while (!openSet.empty()) {
				if (safety > 10000) {
					break;
				}

				VertexNode* current = *openSet.begin();
				openSet.erase(current);
				opened.erase(*current);
				closedSet.insert(*current);

				if (current->position == target) {
					path_success = true;
					break;
				}

				for (GraphEdge edgeNeighbor : *graph->Adjacencies->operator[](current->position)) {
					VertexNode* neighbor = new VertexNode(edgeNeighbor.to);
					if (closedSet.find(*neighbor) != closedSet.end()) {
						continue;
					}
					int newMoveCost = current->gcost + edgeNeighbor.distance + current->penalty;
					unordered_set<VertexNode>::iterator search = opened.find(*neighbor);
					if (search == opened.end()) {
						neighbor->updateCost(newMoveCost, neighbor->position.DistanceTo(target));
						neighbor->previous = current;
						openSet.insert(neighbor);
						opened.insert(*neighbor);
					}
					else {
						VertexNode* toUpdate = new VertexNode(*search);
						if (newMoveCost < toUpdate->gcost) {
							openSet.erase(toUpdate);
							toUpdate->updateCost(newMoveCost, neighbor->position.DistanceTo(target));
							toUpdate->previous = current;
							openSet.insert(toUpdate);
						}	
					}
				}
				safety++;
			}

			if (path_success) {
				unordered_set<VertexNode>::iterator end = closedSet.find(VertexNode(target));
				return retracePath(start, *end, exact);
			}
			else {
				return {};
			}
		}

		/// <summary>
		/// Retraces the path.
		/// </summary>
		/// <param _startNode>The start node</param>
		/// <param _endNode>The end node</param>
		const vector<Vector3> retracePath(Vector3 start, VertexNode end, bool exact)
		{
			vector<Vector3> points = vector<Vector3>();
			VertexNode currentNode = end;
			while (!(currentNode.position == start)) {
				points.push_back(currentNode.position);
				currentNode = *currentNode.previous;
			}

			if (exact) {
				points.push_back(currentNode.position);
				reverse(points.begin(), points.end());
				return points;
			}
			vector<Vector3> waypoints = simplifyPath(points);
			reverse(waypoints.begin(), waypoints.end());
			return waypoints;
		}

		/// <summary>
		/// Simplifies the passed path list of world points. 
		/// Removing duplicates of same slope.
		/// </summary>
		/// <param _pathNodes>The path or the list of nodes</param>
		const vector<Vector3> simplifyPath(vector<Vector3> points)
		{
			vector<Vector3> waypoints = vector<Vector3>();
			float directionOld = -5; // An impossible direction
			for (size_t i = 0; i < points.size(); i++)
			{
				if (i == 0) {
					waypoints.push_back(points.at(i));
					continue;
				}

				Vector3 prior = points.at(i - 1);
				Vector3 current = points.at(i);
				float directionNew = prior.DirectionTo(current);

				if (!(abs(directionOld - directionNew) < FLT_EPSILON))
				{
					waypoints.push_back(points.at(i));
				}
				directionOld = directionNew;
			}
			return waypoints;
		}

		// Shortest path based on BFS (finds all paths and then follows back from
		// target...
		vector<Vector3> ShortestPath(Vector3 start, Vector3 target) {
			map<Vector3, Vector3, Vector3Comparer> previous;
			list<Vector3> queue;

			queue.push_back(start);

			int safety = 0;

			while (queue.size() != 0) {
				Vector3 current = queue.front();
				queue.pop_front();
				for (GraphEdge neighbor : *graph->Adjacencies->operator[](current)) {
					if (previous.find(neighbor.to) != previous.end()) {
						continue;
					}
					previous.insert(pair<Vector3, Vector3>(neighbor.to, current));
					queue.push_back(neighbor.to);
				}
				safety++;
				if (safety > 100000) {
					return vector<Vector3>();
				}
			}

			vector<Vector3> path;
			Vector3 current = target;
			while (!(current == start)) {
				path.push_back(current);
				current = previous[current];
			}
			path.push_back(start);
			std::reverse(path.begin(), path.end());
			return path;
		}
	};

	
}

#endif // !BIDIRECTIONALGRAPH_HPP
