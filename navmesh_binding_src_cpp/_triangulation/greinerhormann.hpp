#ifndef GREINERHORMANN_HPP
#define GREINERHORMANN_HPP

#include "../vector3.hpp"
#include "geometryhelper.hpp"
#include <vector>

using namespace std;

namespace triangulation
{
	/// <summary>
	/// Enum representing boolean operations
	/// </summary>
	enum class BooleanOperation {
		Interseciton, Difference, ExclusiveOr, Union
	};

	/// <summary>
	/// Vertex described in usage of Greiner Hormann algorithm
	/// </summary>
	class ClipVertex {
	public:
		Vector3 coordinate;
		ClipVertex* next = NULL;
		ClipVertex* prev = NULL;
		ClipVertex* nextPoly = NULL;
	    ClipVertex* neighbor = NULL;
		bool isIntersection = false;
		bool isEntry = false;
		bool isTakenByFinal = false;
		float alpha = 0;

		ClipVertex(Vector3 coordinate) : coordinate(coordinate) { }
	};

	/// <summary>
	/// Greiner Hormann algorithm based upon: https://www.inf.usi.ch/hormann/papers/Greiner.1998.ECO.pdf
	/// </summary>
	class GreinerHormann {
	private:
		// Adds a polygon to the list
		void AddPolygonToList(vector<ClipVertex*> toAdd, vector<vector<Vector3>*>& finalPoly, bool reverse)
		{
			vector<Vector3>* thisPolyList = new vector<Vector3>();
			finalPoly.push_back(thisPolyList);
			for (int i = 0; i < toAdd.size(); i++) {
				ClipVertex* vert = toAdd[i];
				thisPolyList->push_back(vert->coordinate);
				if (vert->nextPoly != NULL) {
					if (reverse) {
						std::reverse(thisPolyList->begin(), thisPolyList->end());
					}
					thisPolyList = new vector<Vector3>();
					finalPoly.push_back(thisPolyList);
				}
			}
			if (reverse) {
				std::reverse(finalPoly[finalPoly.size() - 1]->begin(), finalPoly[finalPoly.size() - 1]->end());
			}
		}

		// Gets the clipped polygon in terms of collection of vertices after clipping 
		// the passed polygon
		vector<ClipVertex*> GetClippedPolygon(vector<ClipVertex*> poly, bool getIntersection)
		{
			vector<ClipVertex*> finalPolygon;

			ResetVertices(poly);

			ClipVertex* current = FindFirstEntryVertex(poly);

			ClipVertex* first = current;

			finalPolygon.push_back(current);
			current->isTakenByFinal = true;
			current->neighbor->isTakenByFinal = true;

			bool isMoving = getIntersection ? true : false;
			current = getIntersection ? current->next : current->prev;
			int safety = 0;
			while (true) {
				if (current == first || (current->neighbor != NULL && current->neighbor == first)) {
					ClipVertex* next = FindFirstEntryVertex(poly);
					if (next == NULL) {
						break;
					}
					else {
						finalPolygon[finalPolygon.size() - 1]->nextPoly = next;
						current = next;
						first = next;
						finalPolygon.push_back(current);
						current->isTakenByFinal = true;
						current->neighbor->isTakenByFinal = true;

						isMoving = getIntersection ? true : false;
						current = getIntersection ? current->next : current->prev;
					}
				}

				if (!current->isIntersection) {
					finalPolygon.push_back(current);
					current = isMoving ? current->next : current->prev;
				}
				else {
					current->isTakenByFinal = true;
					current->neighbor->isTakenByFinal = true;
					current = current->neighbor;
					finalPolygon.push_back(current);

					if (getIntersection) {
						isMoving = current->isEntry ? true : false;
						current = current->isEntry ? current->next : current->prev;
					}
					else {
						isMoving = !isMoving;
						current = isMoving ? current->next : current->prev;
					}
				}
				safety++;
				if (safety > 10000) {
					break; // Found an endless loop
				}
			}
			return finalPolygon;
		}

		// Resets the vertices of the passed polygon
		void ResetVertices(vector<ClipVertex*> poly)
		{
			ClipVertex* resetVertex = poly[0];

			int safety = 0;
			while (true) {
				resetVertex->isTakenByFinal = false;
				resetVertex->nextPoly = NULL;

				if (resetVertex->isIntersection) {
					resetVertex->neighbor->isTakenByFinal = false;
				}
				resetVertex = resetVertex->next;

				if (resetVertex == poly[0]) {
					break;
				}

				safety++;

				if (safety > 10000) {
					break; // Found an endless loop
				}
			}
		}

		// Determines whether the polygons are within one another
		bool IsPolygonInsidePolygon(vector<Vector3> polyOne, vector<Vector3> polyTwo)
		{
			for (int i = 0; i < polyOne.size(); i++) {
				if (PointPolygon(polyTwo, polyOne[i])) {
					return true;
				}
			}
			return false;
		}

		// Determines if the point is within the polygon
		bool PointPolygon(vector<Vector3> polygonPoints, Vector3 point)
		{
			Vector3 maxXPosVertex = polygonPoints[0];
			for (int i = 1; i < polygonPoints.size(); i++) {
				if (polygonPoints[i].x > maxXPosVertex.x) {
					maxXPosVertex = polygonPoints[i];
				}
			}

			Vector3 outsidePoint = maxXPosVertex + Vector3(1, 0, 0.01f);
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

		// Finds the first entry vertex of the polygon
		ClipVertex* FindFirstEntryVertex(vector<ClipVertex*> poly)
		{
			ClipVertex* current = poly[0];
			ClipVertex* first = current;
			int safety = 0;

			while (true) {
				if (current->isIntersection && current->isEntry && !current->isTakenByFinal) {
					break;
				}
				current = current->next;

				if (current == first) {
					current = NULL;
					break;
				}
				safety++;
				if (safety > 10000) {
					break; // Found infinite loop
				}
			}
			return current;
		}

		// Finds the insertion intersection of the vertex
		ClipVertex* InsertIntersectionVertex(Vector3 a, Vector3 b, Vector3 point, ClipVertex* current)
		{
			float alpha = (a - point).SqrMagnitude2D() / (a - b).SqrMagnitude2D();

			ClipVertex* intersection = new ClipVertex(point);
			intersection->isIntersection = true;
			intersection->alpha = alpha;

			ClipVertex* after = current;

			int safety = 0;

			while (true) {
				if (after->next->alpha > alpha || !after->next->isIntersection) {
					break;
				}
				after = after->next;
				if (safety > 10000) {
					break; // Found infinite loop
				}
			}

			intersection->next = after->next;
			intersection->prev = after;
			after->next->prev = intersection;
			after->next = intersection;
			return intersection;
		}

		// Marks the entry exit of the polygon as it follows the edges
		// Essentially ties the points for usage based on boolean operation
		void MarkEntryExit(vector<ClipVertex*>& poly, vector<Vector3> clipPolyVector)
		{
			bool inside = IsPointInPolygon(clipPolyVector, poly[0]->coordinate);
			ClipVertex* current = poly[0];
			ClipVertex* first = current;

			int safety = 0;
			while (true) {
				if (current->isIntersection) {
					current->isEntry = inside ? false : true;
					inside = !inside;
				}
				current = current->next;

				if (current == first) {
					break;
				}
				safety++;
				if (safety > 10000) {
					break; // Found infinite loop
				}
			}
		}

		// Sets up the data structure with the polygon vertices
		vector<ClipVertex*> SetupDataStructure(vector<Vector3> polyVectors)
		{
			vector<ClipVertex*> poly;
			for (int i = 0; i < polyVectors.size(); i++) {
				poly.push_back(new ClipVertex(polyVectors[i]));
			}

			for (int i = 0; i < poly.size(); i++) {
				int iPlus = ClampListIndex(i + 1, poly.size());
				int iMinus = ClampListIndex(i - 1, poly.size());
				poly[i]->next = poly[iPlus];
				poly[i]->prev = poly[iMinus];
			}
			return poly;
		}
		
	public:
		// Clips the passed polygon of the passed clipping polygon vertices based on the passed
		// boolean operation
		vector<vector<Vector3>*> ClipPolygons(vector<Vector3> polyVectors, vector<Vector3> clipVectors, BooleanOperation operation = BooleanOperation::Difference)
		{
			vector<vector<Vector3>*> finalPolygons;
			
			vector<ClipVertex*> poly = SetupDataStructure(polyVectors);
			vector<ClipVertex*> clipPoly = SetupDataStructure(clipVectors);

			bool intersection = false;
			for (int i = 0; i < poly.size(); i++) {
				ClipVertex* current = poly[i];
				int iPlus = ClampListIndex(i + 1, poly.size());
				Vector3 p1 = poly[i]->coordinate;
				Vector3 p2 = poly[iPlus]->coordinate;

				for (int j = 0; j < clipPoly.size(); j++) {
					int jPlus = ClampListIndex(j + 1, clipPoly.size());
					Vector3 p3 = clipPoly[j]->coordinate;
					Vector3 p4 = clipPoly[jPlus]->coordinate;
					if (LineIntersection(p1, p2, p3, p4, true)) {
						intersection = true;
						Vector3 intersectionPoint = GetLineIntersectionPoint(p1, p2, p3, p4);
						ClipVertex* vertexOnPoly = InsertIntersectionVertex(p1, p2, intersectionPoint, current);
						ClipVertex* vertexOnClip = InsertIntersectionVertex(p3, p4, intersectionPoint, clipPoly[j]);
						vertexOnPoly->neighbor = vertexOnClip;
						vertexOnClip->neighbor = vertexOnPoly;
					}
				}
			}
			if (intersection) {
				MarkEntryExit(poly, clipVectors);
				MarkEntryExit(clipPoly, polyVectors);
			
				if (operation == BooleanOperation::Interseciton) {
					vector<ClipVertex*> intersectionVerts = GetClippedPolygon(poly, true);
					AddPolygonToList(intersectionVerts, finalPolygons, false);
				}
				else if (operation == BooleanOperation::Difference) {
					vector<ClipVertex*> outsideVerts = GetClippedPolygon(poly, false);
					AddPolygonToList(outsideVerts, finalPolygons, true);
				}
				else if (operation == BooleanOperation::ExclusiveOr) {
					vector<ClipVertex*> outsideVerts = GetClippedPolygon(poly, false);
					AddPolygonToList(outsideVerts, finalPolygons, true);
					vector<ClipVertex*> outsideClipVerts = GetClippedPolygon(clipPoly, false);
					AddPolygonToList(outsideVerts, finalPolygons, true);
				}
				else if (operation == BooleanOperation::Union) {
					vector<ClipVertex*> intersectionVerts = GetClippedPolygon(poly, true);
					AddPolygonToList(intersectionVerts, finalPolygons, false);
					vector<ClipVertex*> outsideVerts = GetClippedPolygon(poly, false);
					AddPolygonToList(outsideVerts, finalPolygons, true);
					vector<ClipVertex*> outsideClipVerts = GetClippedPolygon(clipPoly, false);
					AddPolygonToList(outsideVerts, finalPolygons, true);
				}
			}
			return finalPolygons;
		}
	};
}

#endif // !GREINERHORMANN_HPP
