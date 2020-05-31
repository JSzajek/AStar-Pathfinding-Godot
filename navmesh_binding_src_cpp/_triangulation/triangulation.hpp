#ifndef TRIANGULATION_HPP
#define TRIANGULATION_HPP

#include "cyclinglinkedlist.hpp"
#include "cyclingvector.hpp"
#include "vertex.hpp"
#include "triangle.hpp"
#include "edge.hpp"
#include "../vector3.hpp"

using namespace std;

namespace triangulation
{
	enum class WindingOrder {
		CW, CCW
	};

	class Triangulation 
	{
	private:
		CyclingLinkedList<Vertex> *polygonVertices;
		CyclingLinkedList<Vertex> *earVertices;
		CyclingVector<Vertex> *convexVertices;
		CyclingVector<Vertex> *reflexVertices;
		vector<Vector3> EnsureWindingOrder(vector<Vector3> vertices, WindingOrder order);
		vector<Vector3> ReverseWindingOrder(vector<Vector3> vertices);
		WindingOrder DetermineWindingOrder(vector<Vector3> vertices);
		void ClipNextEar(vector<Triangle>& triangles);
		void ValidateAdjacentVertex(Vertex vertex);
		void FindConvexAndReflexVertices();
		void FindEarVertices();
		bool IsEar(Vertex c);
		bool IsConvex(Vertex c);
	public:
		vector<Triangle> Triangulate(vector<Vector3> inputVerts, WindingOrder order = WindingOrder::CCW);
		vector<Vector3> CutHoleInShape(vector<Vector3> inputVerts, vector<Vector3> holeVerts);
	};
}


#endif // !TRIANGULATION_HPP
