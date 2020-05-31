#ifndef NAVMESH_HPP
#define NAVMESH_HPP

#define TriangleThreshold 10
#define VertexThreshold 0.8f

#include "_triangulation/triangulation.hpp"
#include "_triangulation/greinerhormann.hpp"
#include "kdtree.hpp"
#include "vertexgraph.hpp"
#include "vector3.hpp"
#include <string>
#include <vector>
#include <unordered_set>
#include <string>
#include <list>

using namespace std;
using namespace astar;
using namespace triangulation;

namespace navmesh {

	class VertexComparer {
	public:
		size_t operator()(const Vector3& t) const
		{
			return hash<float>()(t.x) ^ hash<float>()(t.y) << 2 ^ hash<float>()(t.z) >> 2;
		}
	};

	class NavMesh {
	private:
		KDTree* tree = NULL;
		vector<tuple<Vector3, Vector3>>* edges = NULL;
		VertexGraph* network;
		
		bool cleaned = false;
		void cleanEdges();
		void addToTree(list<Vector3>* added, Triangle trig);
		void AddTreeNode(list<Vector3>* added, Vector3 toAdd);
	public:
		vector<Vector3>* edgeVertices;
		vector<Vector3>* vertices;
		vector<Triangle>* triangles;
		Vector3 centriod;

		~NavMesh();
		NavMesh(vector<Vector3> vertices);

		bool hasPoint(Vector3 point);

		void clipEdge(vector<Vector3> vertices);
		void clipHole(vector<Vector3> vertices);
		void triangulateMesh();

		Vector3 GetClosestPoint(Vector3 point);
		vector<Vector3> FindPath(Vector3 start, Vector3 target);
		void CalculateCentriod();

		string ToString();
	};
}


#endif // !NAVEMESH_HPP
