#ifndef EDGE_HPP
#define EDGE_HPP

#include "../vector3.hpp"
#include "vertex.hpp"
#include <algorithm>

using namespace std;

namespace triangulation
{
	class Edge
	{
	public:
		Vertex A;
		Vertex B;

		Edge() {}

		Edge(Vertex a, Vertex b) : A(a), B(b) {}
		
		Edge(Vector3 a, Vector3 b) : A(Vertex(a, 0)), B(Vertex(b, 0)) {}
		
		float intersectWithRay(Vector3 origin, Vector3 direction, bool& result) {
			float largest = max(A.Position.x - origin.x, B.Position.x - origin.x) * 2;
			Edge raySegment = Edge(Vertex(origin, 0), Vertex(origin + (direction * largest), 0));
			bool intersectionResult = true;
			Vector3 intersection = findIntersection(raySegment, intersectionResult);
			if (result) {
				return origin.DistanceTo(intersection);
			}
			result = false;
			return -1.0f;
		}

		Vector3 findIntersection(Edge other, bool& success) {
			float x1 = A.Position.x;
			float y1 = A.Position.z;
			float x2 = B.Position.x;
			float y2 = B.Position.z;
			float x3 = other.A.Position.x;
			float y3 = other.A.Position.z;
			float x4 = other.B.Position.x;
			float y4 = other.B.Position.z;

			float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

			float uaNum = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
			float ubNum = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);

			float ua = uaNum / denom;
			float ub = ubNum / denom;
			if (clamp(ua, 0, 1) != ua || clamp(ub, 0, 1) != ub) {
				success = false;
				return Vector3(INFINITY, INFINITY, INFINITY);
			}
			return A.Position + (B.Position - A.Position) * ua;
		}

		inline float clamp(float n, float lower, float upper)
		{
			return n <= lower ? lower : n <= upper ? n : upper;
		}
	};
}


#endif
