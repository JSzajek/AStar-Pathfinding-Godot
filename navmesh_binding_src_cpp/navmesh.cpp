#include "pch.h"
#include "navmesh.hpp"
#include "_triangulation/geometryhelper.hpp"

using namespace triangulation;
using namespace navmesh;

// Initializes the navmesh with a collection of edge vertices
NavMesh::NavMesh(vector<Vector3> vertices)
{
	// Add check to make sure vertices are CCW?
	edgeVertices = new vector<Vector3>(vertices);
	triangles = NULL;
}

// The destructor cleaning up pointers
NavMesh::~NavMesh()
{
	delete vertices;
	delete edgeVertices;
	delete triangles;

	tree->clear();
	delete tree;
	delete edges;
	delete network;
}

// Determines whether the polygon has the passed point, either within or along
// the edge of the polygon.
bool NavMesh::hasPoint(Vector3 point)
{
	return IsPointInPolygon(*edgeVertices, point) || IsPointOnPolygonEdge(*edgeVertices, point);
}

// Clips an edge polygon from the navmesh
void NavMesh::clipEdge(vector<Vector3> vertices) 
{
	GreinerHormann algo;
	vector<vector<Vector3>*> result = algo.ClipPolygons(*this->edgeVertices, vertices);
	if (result.size() != 1) {
		return;
	}

	delete edgeVertices;
	delete this->vertices;

	edgeVertices = new vector<Vector3>(*result[0]);
	this->vertices = new vector<Vector3>(*result[0]);
}

// Helper method that cleans the edges by trimming vertices that are too close or
// below the vertex threshold
void NavMesh::cleanEdges()
{
	// Walk along edge to determine if combining vertices is possible
	vector<int> indicesToDelete;
	for (int i = 0; i < vertices->size() - 1; i++) {
		if (vertices->operator[](i).DistanceTo(vertices->operator[](i + 1)) < VertexThreshold) {
			vertices->operator[](i + 1) = (vertices->operator[](i) + vertices->operator[](i + 1)) / 2.0f;
			indicesToDelete.push_back(i);
		}
	}
	int cnt = 0;
	for (int index : indicesToDelete) {
		vertices->erase(vertices->begin() + index - cnt);
		cnt++;
	}
}

// Clips a hole polygon from the navmesh
void NavMesh::clipHole(vector<Vector3> vertices)
{
	// Completely untouched mesh
	if (this->vertices == NULL) {
		this->vertices = new vector<Vector3>(*this->edgeVertices);
	}

	if (!cleaned) {
		cleanEdges();
		cleaned = true;
	}

	Triangulation algo;
	vector<Vector3> result = algo.CutHoleInShape(*this->vertices, vertices);
	this->vertices = new vector<Vector3>(result);
}

// Triangulates the navmesh polygon as well as storing the vertices into the kdtree for
// quick point comparison and nearest neighbor
void NavMesh::triangulateMesh()
{
	// Completely untouched mesh
	if (vertices == NULL) {
		vertices = new vector<Vector3>(*this->edgeVertices);
	}

	CalculateCentriod();

	Triangulation algo;
	this->triangles = new vector<Triangle>(algo.Triangulate(*this->vertices, WindingOrder::CCW));
	this->tree = new KDTree(3);
	this->edges = new vector<tuple<Vector3, Vector3>>();
	vertices->clear();

	list<Vector3>* set = new list<Vector3>();

	for (Triangle trig : *this->triangles) {
		addToTree(set, trig);
	}
	this->network = new VertexGraph(*this->vertices, *this->edges);
}

// Helper method that addes the vertices and edges of the triangle into the kdtree of the navmesh
void NavMesh::addToTree(list<Vector3>* added, Triangle trig)
{
	tuple<Vector3, Vector3, Vector3> verts = trig.GetVertices();
	Vector3 v1 = get<0>(verts);
	Vector3 v2 = get<1>(verts);
	Vector3 v3 = get<2>(verts);

	tuple<Vector3, Vector3, Vector3> mids = trig.GetMidPoints();
	Vector3 m1 = get<0>(mids);
	Vector3 m2 = get<1>(mids);
	Vector3 m3 = get<2>(mids);

	AddTreeNode(added, v1);
	AddTreeNode(added, v2);
	AddTreeNode(added, v3);
	AddTreeNode(added, m1);
	AddTreeNode(added, m2);
	AddTreeNode(added, m3);

	edges->push_back(make_tuple(m1, v1));
	edges->push_back(make_tuple(v1, m3));

	edges->push_back(make_tuple(m3, v3));
	edges->push_back(make_tuple(v3, m2));
	
	edges->push_back(make_tuple(m2, v2));
	edges->push_back(make_tuple(v2, m1));

	if (trig.GetArea() > TriangleThreshold) {
		// Centriod should be unique amongst triangles
		Vector3 centriod = trig.GetCentriod();
		tree->AddNode(make_tuple(centriod, true, 0));
		vertices->push_back(centriod);
		
		edges->push_back(make_tuple(m1, centriod));
		edges->push_back(make_tuple(m2, centriod));
		edges->push_back(make_tuple(m3, centriod));
		
		edges->push_back(make_tuple(centriod, v1));
		edges->push_back(make_tuple(centriod, v2));
		edges->push_back(make_tuple(centriod, v3));
	}
	else {
		edges->push_back(make_tuple(m1, m2));
		edges->push_back(make_tuple(m2, m3));
		edges->push_back(make_tuple(m3, m1));
	}
}

// Helper method that adds a coordinate the kdtree and vertices collection
void NavMesh::AddTreeNode(list<Vector3>* added, Vector3 toAdd)
{
	for (Vector3 vec : *added) {
		if (vec == toAdd) {
			return;
		}
	}

	added->push_back(toAdd);
	vertices->push_back(toAdd);
	tree->AddNode(make_tuple(toAdd, true, 0));
}

// Finds the shortest path from the start coordinate to the target coordinate across the navmesh
vector<Vector3> NavMesh::FindPath(Vector3 start, Vector3 target)
{
	PathNode* fromNode = tree->Nearest(start);
	PathNode* toNode = tree->Nearest(target);
	
	if (fromNode == NULL || toNode == NULL) {
		return {}; // Should never happen
	}
	return network->FindShortest(fromNode->position, toNode->position);
}

// Finds the closest point in the navmesh to the passed point
Vector3 NavMesh::GetClosestPoint(Vector3 point)
{
	PathNode* result = tree->Nearest(point);
	if (result == NULL) {
		return INF;
	}
	return result->position;
}

// Calcuates the centriod of the navmesh
void NavMesh::CalculateCentriod() {
	Vector3 sum;
	for (Vector3 vec : *this->edgeVertices) {
		sum += vec;
	}
	this->centriod = sum / this->edgeVertices->size();
}

// String representation of the navmesh for debugging
string NavMesh::ToString() {
	string str = "Nav Mesh:\n";
	for (Triangle tri : *triangles) {
		str += "Triangle: (area=" + to_string(tri.GetArea()) + ")\n";
		str += "A: " + tri.A.Position.ToString() + "\n";
		str += "B: " + tri.B.Position.ToString() + "\n";
		str += "C: " + tri.C.Position.ToString() + "\n";
	}
	return str;
}