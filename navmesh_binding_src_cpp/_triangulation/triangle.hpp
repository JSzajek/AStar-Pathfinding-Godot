#ifndef TRIANGLE_HPP
#define TRIANGLE_HPP

#include <tuple>
#include "../vector3.hpp"
#include "vertex.hpp"

using namespace std;

namespace triangulation
{
	class Triangle
	{
	public:
		Vertex A;
		Vertex B;
		Vertex C;

		Triangle(Vertex a, Vertex b, Vertex c) : A(a), B(b), C(c) {}
		
		tuple<Vector3, Vector3, Vector3> GetVertices() {
			return make_tuple(A.Position, B.Position, C.Position);
		}
		
		Vector3 GetCentriod() {
			return (A.Position + B.Position + C.Position) / 3.0f;
		}

		tuple<Vector3, Vector3, Vector3> GetMidPoints() {
			return make_tuple(
				(A.Position + B.Position) / 2.0f,
				(B.Position + C.Position) / 2.0f,
				(C.Position + A.Position) / 2.0f
			);
		}

		float GetArea() {
			tuple<Vector3, Vector3, Vector3> verts = GetVertices();
			Vector3 p1 = get<0>(verts);
			Vector3 p2 = get<1>(verts);
			Vector3 p3 = get<2>(verts);
			return fabs((p1.x * (p2.z - p3.z) + p2.x * (p3.z - p2.z) + p3.x * (p1.z - p2.z)) / 2.0f);
		}

		bool ContainsPoint(Vertex point) {
			if (point.Equals(A) || point.Equals(B) || point.Equals(C)) {
				return true;
			}

			bool oddNodes = false;
			if (CheckPointToSegment(C, A, point)) {
				oddNodes = !oddNodes;
			}
			if (CheckPointToSegment(A, B, point)) {
				oddNodes = !oddNodes;
			}
			if (CheckPointToSegment(B, C, point)) {
				oddNodes = !oddNodes;
			}
			return oddNodes;
		}
		
		bool CheckPointToSegment(Vertex segA, Vertex segB, Vertex point) {
			if ((segA.Position.z < point.Position.z && segB.Position.z >= point.Position.z) ||
				(segB.Position.z < point.Position.z && segA.Position.z >= point.Position.z)) {
				float x = segA.Position.x + (point.Position.z - segA.Position.z) / (segB.Position.z - segA.Position.z) * (segB.Position.x - segA.Position.x);
				if (x < point.Position.x) {
					return true;
				}
			}
			return false;
		}

		bool Equals(Triangle other) {
			return other.A.Equals(A) && other.B.Equals(B) && other.A.Equals(C);
		}
	};
}


#endif
