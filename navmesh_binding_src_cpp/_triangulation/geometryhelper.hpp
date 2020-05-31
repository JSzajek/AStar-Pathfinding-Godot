#ifndef GEOMETRYHELPER_HPP
#define GEOMETRYHELPER_HPP

#include <vector>
#include "../vector3.hpp"

using namespace std;

namespace triangulation
{
	// Clamps the index to the listsize
	inline int ClampListIndex(int index, int listSize)
	{
		return ((index % listSize) + listSize) % listSize;
	}

	// Checks if the two segments represented by two coordinates intersect
	inline bool LineIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, bool includeEnd)
	{
		float denominator = (p4.z - p3.z) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.z - p1.z);
		if (denominator != 0) {
			float u_a = ((p4.x - p3.x) * (p1.z - p3.z) - (p4.z - p3.z) * (p1.x - p3.x)) / denominator;
			float u_b = ((p2.x - p1.x) * (p1.z - p3.z) - (p2.z - p1.z) * (p1.x - p3.x)) / denominator;

			if (includeEnd) {
				if (u_a >= FLT_EPSILON && u_a <= 1 - FLT_EPSILON && u_b >= FLT_EPSILON && u_b <= 1 - FLT_EPSILON)
				{
					return true;
				}
			}
			else {
				if (u_a > FLT_EPSILON && u_a < 1 - FLT_EPSILON && u_b > FLT_EPSILON && u_b < 1 - FLT_EPSILON) {
					return true;
				}
			}
		}
		return false;
	}

	// Finds the point of intersection between the two passed segments represented by two coordinates.
	inline Vector3 GetLineIntersectionPoint(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
	{
		float denominator = (p4.z - p3.z) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.z - p1.z);
		float u_a = ((p4.x - p3.x) * (p1.z - p3.z) - (p4.z - p3.z) * (p1.x - p3.x)) / denominator;
		Vector3 result = p1 + ((p2 - p1) * u_a);
		return result;
	}

	// Determines whether the point is on the polygon's edge or is "close enough"
	inline bool IsPointOnPolygonEdge(vector<Vector3> polygonPoints, Vector3 point) {
		for (Vector3 edgePoint : polygonPoints) {
			if (point == edgePoint || point.DistanceTo(edgePoint) < 2.0f) // replace with constant
			{
				return true;
			}
		}
		return false;
	}

	// Determines whether the point is within the polygon
	inline bool IsPointInPolygon(vector<Vector3> polygonPoints, Vector3 point)
	{
		Vector3 maxXPosVertex = polygonPoints[0];
		for (int i = 1; i < polygonPoints.size(); i++) {
			if (polygonPoints[i].x > maxXPosVertex.x) {
				maxXPosVertex = polygonPoints[i];
			}
		}

		Vector3 outsidePoint = maxXPosVertex + Vector3(10, 0, 0);

		Vector3 p1 = point;
		Vector3 p2 = outsidePoint;

		int numIntersections = 0;
		for (int i = 0; i < polygonPoints.size(); i++) {
			Vector3 p3 = polygonPoints[i];
			int iPlus = ClampListIndex(i + 1, polygonPoints.size());
			Vector3 p4 = polygonPoints[iPlus];

			if (LineIntersection(p1, p2, p3, p4, true)) {
				numIntersections++;
			}
		}

		if (numIntersections == 0 || numIntersections % 2 == 0) {
			return false;
		}
		return true;
	}
}

#endif // !GEOMETRYHELPER_HPP
