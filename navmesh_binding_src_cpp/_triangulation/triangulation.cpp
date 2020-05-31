#include "pch.h"
#include "triangulation.hpp"

using namespace std;
using namespace triangulation;

// Heavily based on triangulation by ear clipping
// described in: https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf

// Triangulates the given input vertices based on the given winding order
vector<Triangle> Triangulation::Triangulate(vector<Vector3> inputVerts, WindingOrder order)
{
	vector<Triangle> triangles;
	if (DetermineWindingOrder(inputVerts) == WindingOrder::CW) {
		inputVerts = ReverseWindingOrder(inputVerts);
	}
	polygonVertices = new CyclingLinkedList<Vertex>();
	earVertices = new CyclingLinkedList<Vertex>();
	convexVertices = new CyclingVector<Vertex>();
	reflexVertices = new CyclingVector<Vertex>();

	for (int i = 0; i < inputVerts.size(); i++) {
		polygonVertices->AddLast(Vertex(inputVerts[i], i));
	}
	FindConvexAndReflexVertices();
	FindEarVertices();

	while (polygonVertices->Size() > 3 && earVertices->Size() > 0) {
		ClipNextEar(triangles);
	}

	if (polygonVertices->Size() == 3) {
		triangles.push_back(Triangle(
			polygonVertices->operator[](0)->data,
			polygonVertices->operator[](1)->data,
			polygonVertices->operator[](2)->data
		));
	}

	vector<Triangle> result;
	if (order == WindingOrder::CCW) {
		for (int i = 0; i < triangles.size(); i++) {
			result.push_back(Triangle(triangles[i].A, triangles[i].B, triangles[i].C));
		}
	}
	else {
		for (int i = 0; i < triangles.size(); i++) {
			result.push_back(Triangle(triangles[i].C, triangles[i].B, triangles[i].A));
		}
	}
	
	// Clean up 
	delete polygonVertices;
	delete earVertices;
	delete convexVertices;
	delete reflexVertices;

	return result;
}

// Cuts a hole of the passed hole polygon vertices from the passed input vertices representing
// the polygon to cut from.
vector<Vector3> Triangulation::CutHoleInShape(vector<Vector3> inputVerts, vector<Vector3> holeVerts) 
{
	inputVerts = EnsureWindingOrder(inputVerts, WindingOrder::CCW);
	holeVerts = EnsureWindingOrder(holeVerts, WindingOrder::CW);

	polygonVertices = new CyclingLinkedList<Vertex>();
	earVertices = new CyclingLinkedList<Vertex>();
	convexVertices = new CyclingVector<Vertex>();
	reflexVertices = new CyclingVector<Vertex>();

	for (int i = 0; i < inputVerts.size(); i++) {
		polygonVertices->AddLast(Vertex(inputVerts[i], i));
	}

	CyclingVector<Vertex>* holePolygon = new CyclingVector<Vertex>();
	for (int i = 0; i < holeVerts.size(); i++) {
		holePolygon->Add(Vertex(holeVerts[i], i + polygonVertices->Size()));
	}

	FindConvexAndReflexVertices();
	FindEarVertices();

	Vertex rightMost = holePolygon->operator[](0);
	for (int i = 0; i < holePolygon->Size(); i++) {
		if (holePolygon->operator[](i).Position.x > rightMost.Position.x) {
			rightMost = holePolygon->operator[](i);
		}
	}

	vector<Edge> toTest;
	for (int i = 0; i < polygonVertices->Size(); i++) {
		Vertex a = polygonVertices->operator[](i)->data;
		Vertex b = polygonVertices->operator[](i+1)->data;
		
		if ((a.Position.x > rightMost.Position.x || b.Position.x > rightMost.Position.x) &&
		   ((a.Position.y >= rightMost.Position.y && b.Position.y <= rightMost.Position.y) ||
		    (a.Position.y <= rightMost.Position.y && b.Position.y >= rightMost.Position.y))) {
			toTest.push_back(Edge(a, b));
		}
	}

	float closestPoint = NULL;
	Edge closest = Edge();
	
	for (Edge segment : toTest) {
		bool success = true;
		float intersection = segment.intersectWithRay(rightMost.Position, Vector3(1, 0, 0), success);
		if (success) {
			if (closestPoint == NULL || closestPoint > intersection) {
				closestPoint = intersection;
				closest = segment;
			}
		}
	}

	if (closestPoint == NULL) {
		return inputVerts;
	}

	Vector3 I = rightMost.Position + (Vector3(1, 0, 0) * closestPoint);
	Vertex P = (closest.A.Position.x > closest.B.Position.x) ? closest.A : closest.B;

	Triangle mip = Triangle(rightMost, Vertex(I, 1), P);

	vector<Vertex> interiorReflexVertices;
	for (int i = 0; i < reflexVertices->Size(); i++) {
		if (mip.ContainsPoint(reflexVertices->operator[](i))) {
			interiorReflexVertices.push_back(reflexVertices->operator[](i));
		}
	}

	if (interiorReflexVertices.size() > 0) {
		float closestDot = -1;
		for (Vertex vert : interiorReflexVertices) {
			Vector3 d = (vert.Position - rightMost.Position).Normalize();
			float dot = Vector3(1, 0, 0).Dot(d);
			if (dot > closestDot) {
				closestDot = dot;
				P = vert;
			}
		}
	}

	int mIndex = holePolygon->IndexOf(rightMost);
	int injectPoint = polygonVertices->IndexOf(P);

	for (int i = mIndex; i <= mIndex + holePolygon->Size(); i++) {
		polygonVertices->AddAfter(polygonVertices->operator[](injectPoint++), holePolygon->operator[](i));
	}
	polygonVertices->AddAfter(polygonVertices->operator[](injectPoint), P);
	
	vector<Vector3> newShape;
	for (int i = 0; i < polygonVertices->Size(); i++) {
		newShape.push_back(polygonVertices->operator[](i)->data.Position);
	}

	// Clean up
	delete polygonVertices;
	delete earVertices;
	delete convexVertices;
	delete reflexVertices;
	delete holePolygon;

	return newShape;
}

// Helper method that ensures the vertices are in the passed winding order
vector<Vector3> Triangulation::EnsureWindingOrder(vector<Vector3> vertices, WindingOrder order)
{
	if (DetermineWindingOrder(vertices) != order) {
		return ReverseWindingOrder(vertices);
	}
	return vertices;
}

// Helper method that reverses the winding order of the vertices
vector<Vector3> Triangulation::ReverseWindingOrder(vector<Vector3> vertices)
{
	vector<Vector3> newVertices;
	newVertices.push_back(vertices[0]);
	for (int i = 1; i < vertices.size(); i++) {
		newVertices.push_back(vertices[vertices.size() - i]);
	}
	return newVertices;
}

// Determines the winding order of the passed vertices by counting
// TODO: look into better method??
WindingOrder Triangulation::DetermineWindingOrder(vector<Vector3> vertices)
{
	int cwCount = 0;
	int ccwCount = 0;
	Vector3 p1 = vertices[0];
	for (int i = 1; i < vertices.size(); i++) {
		Vector3 p2 = vertices[i];
		Vector3 p3 = vertices[(i + 1) % vertices.size()];
		Vector3 e1 = p1 - p2;
		Vector3 e2 = p3 - p2;
		if (e1.x * e2.z - e1.z * e2.x >= 0) {
			cwCount++;
		}
		else {
			ccwCount++;
		}
		p1 = p2;
	}
	return (cwCount > ccwCount) ? WindingOrder::CW : WindingOrder::CCW;
}

// Helper method that clips the ear from the triangles
void Triangulation::ClipNextEar(vector<Triangle>& triangles)
{
	Vertex ear = earVertices->operator[](0)->data;
	Vertex prev = polygonVertices->operator[](polygonVertices->IndexOf(ear) - 1)->data;
	Vertex next = polygonVertices->operator[](polygonVertices->IndexOf(ear) + 1)->data;
	triangles.push_back(Triangle(ear, next, prev));

	earVertices->RemoveAt(0);
	polygonVertices->RemoveAt(polygonVertices->IndexOf(ear));

	ValidateAdjacentVertex(prev);
	ValidateAdjacentVertex(next);
}

// Helper method that validates the adjacent vertices from the passed vertex
void Triangulation::ValidateAdjacentVertex(Vertex vertex)
{
	if (reflexVertices->Contains(vertex)) {
		if (IsConvex(vertex)) {
			reflexVertices->Remove(vertex);
			convexVertices->Add(vertex);
		}
	}
	if (convexVertices->Contains(vertex)) {
		bool wasEar = earVertices->Contains(vertex);
		bool isEar = IsEar(vertex);
		if (wasEar && !isEar) {
			earVertices->RemoveAt(earVertices->IndexOf(vertex));
		}
		else if (!wasEar && isEar) {
			earVertices->AddFirst(vertex);
		}
	}
}

// Helper method that finds all the convex and reflex vertices
void Triangulation::FindConvexAndReflexVertices()
{
	for (int i = 0; i < polygonVertices->Size(); i++) {
		Vertex vert = polygonVertices->operator[](i)->data;
		if (IsConvex(vert)) {
			convexVertices->Add(vert);
		}
		else {
			reflexVertices->Add(vert);
		}
	}
}

// Helper method that finds all the ear vertices
void Triangulation::FindEarVertices()
{
	for (int i = 0; i < convexVertices->Size(); i++) {
		Vertex c = convexVertices->operator[](i);
		if (IsEar(c)) {
			earVertices->AddLast(c);
		}
	}
}

// Determines whether the passed vertex is a ear vertex
bool Triangulation::IsEar(Vertex c)
{
	Vertex prev = polygonVertices->operator[](polygonVertices->IndexOf(c) - 1)->data;
	Vertex next = polygonVertices->operator[](polygonVertices->IndexOf(c) + 1)->data;
	for (int i = 0; i < reflexVertices->Size(); i++) {
		Vertex t = reflexVertices->operator[](i);
		if (t == prev || t == c || t == next) {
			continue;
		}
		if (Triangle(prev, c, next).ContainsPoint(t)) {
			return false;
		}
	}
	return true;
}

// Determines whether the passed vertex is a convex vertex
bool Triangulation::IsConvex(Vertex c)
{
	Vertex prev = polygonVertices->operator[](polygonVertices->IndexOf(c) - 1)->data;
	Vertex next = polygonVertices->operator[](polygonVertices->IndexOf(c) + 1)->data;
	Vector3 d1 = (c.Position - prev.Position).Normalize();
	Vector3 d2 = (next.Position - c.Position).Normalize();
	Vector3 n2 = Vector3(-d2.z, 0, d2.x);
	return d1.Dot(n2) <= 0;
}