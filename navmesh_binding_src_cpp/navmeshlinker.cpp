#include "pch.h"
#include "navmeshlinker.hpp"

using namespace navmesh;

KDTree* NavmeshLinker::tree;
NavMesh* NavmeshLinker::mesh;
vector<NavMesh*>* NavmeshLinker::meshes;
map<Vector3, NavMesh*>* NavmeshLinker::meshDictionary;
VertexGraph* NavmeshLinker::meshGraph;
//fstream NavmeshLinker::debugFile;

// Destructor
NavmeshLinker::~NavmeshLinker()
{
	tree->clear();
	//debugFile.close();
	delete meshes;
	delete meshDictionary;
	delete meshGraph;
}

// Sets up the relevent parameters involved in navmesh
void NavmeshLinker::setupGrid()
{
	//debugFile = fstream();
	//debugFile.open("navmeshlinker_debug.txt", fstream::in | fstream::out | fstream::trunc);
	tree = new KDTree(3);
	meshes = new vector<NavMesh*>();
	meshDictionary = new map<Vector3, NavMesh*>();
	meshGraph = new VertexGraph();
}

//void NavmeshLinker::writeToDebug(std::string str)
//{
//	debugFile << str << endl;
//}

// Starts the creation of a navmesh
void NavmeshLinker::startMesh(vector<Vector3> vertices)
{
	// Starting a mesh without ending the previous mesh will
	// result in deletion of in process mesh (change?)
	if (mesh != NULL) {
		delete mesh;
	}

	mesh = new NavMesh(vertices);
}

// Clips a edge polygon from the current navmesh
void NavmeshLinker::clipEdge(vector<Vector3> vertices)
{
	mesh->clipEdge(vertices);
}

// Clips a hole polygon from the current navmesh
void NavmeshLinker::clipHole(vector<Vector3> vertices)
{
	mesh->clipHole(vertices);
}

// Gets the debug triangles from the navmesh at the passed index
float* NavmeshLinker::getDebugMesh(int index) {

	if (index < 0 || index >= meshes->size() || meshes->at(index) == NULL || meshes->at(index)->triangles == NULL) {
		return new float[1]{ 1 };
	}

	vector<float> result;
	result.push_back((meshes->at(index)->triangles->size() * 9) + 1);
	for (Triangle tri : *meshes->at(index)->triangles) {
		result.push_back(tri.A.Position.x);
		result.push_back(tri.A.Position.y);
		result.push_back(tri.A.Position.z);
		result.push_back(tri.B.Position.x);
		result.push_back(tri.B.Position.y);
		result.push_back(tri.B.Position.z);
		result.push_back(tri.C.Position.x);
		result.push_back(tri.C.Position.y);
		result.push_back(tri.C.Position.z);
	}
	float* data = new float[result.size()];
	std::copy(result.begin(), result.end(), data);
	return data;
}

// Finishes the current navmesh, by triangulation and storing in navmesh dictionary
// as well as in a vertex graph for pathing between meshes
void NavmeshLinker::endMesh()
{
	// Triangulate the current mesh
	mesh->triangulateMesh();
	finishMeshes();
	mesh = NULL;
}

// Helper method that finishes the mesh creation and checks for edge connection
// between other meshes
void NavmeshLinker::finishMeshes()
{
	// Add mesh to hash map of other meshes
	meshes->push_back(mesh);

	// Put the meshes into a dictionary of the equivalent centriod to navmesh pairing
	meshDictionary->insert(make_pair(mesh->centriod, mesh));

	meshGraph->AddVertex(mesh->centriod);

	// Then connect those meshes by edges - checking if any edges intersect within another
	// mesh therefore indicating that those meshes have a connection.
	for (int i = 0; i < meshes->size() - 1; i++) {
		for (int j = i + 1; j < meshes->size(); j++) {
			if (i == j) { continue; }

			bool connected = false;
			for (Vector3 point : *meshes->at(i)->edgeVertices)
			{
				if (meshes->at(j)->hasPoint(point)) {
					connected = true;
					break;
				}
			}
			if (connected) {
				meshGraph->AddEdge(make_tuple(meshes->at(i)->centriod, meshes->at(j)->centriod));
			}
		}
	}
}

// Gets the shortest path from the start coordinate to the end coordinate
float* NavmeshLinker::getPath(Vector3 start, Vector3 end)
{
	// Use a tree to find the mesh it is currently on?
	NavMesh* startMesh = NULL;
	NavMesh* endMesh = NULL;

	for (NavMesh* mesh : *meshes) {
		if (mesh->hasPoint(start)) {
			startMesh = mesh;
		}
		if (mesh->hasPoint(end)) {
			endMesh = mesh;
		}
	}

	if (startMesh == NULL || endMesh == NULL) {
		return new float[1]{ 1 }; // Shouldn't happen??
	}

	vector<Vector3> path;
	if (startMesh == endMesh) {
		path = startMesh->FindPath(start, end);
	}
	else {
		// Find the shortest path between the centriods and the connections
		// Retranslate the path back into vector of navmeshes centriods
		// Find the path for each navmesh starting the from the
		// start point and moving into each mesh utilizing the old ending point as the new start
		// and closest point to next navmesh as the end point
		// concatenating it all together to form the final path.
		vector<Vector3> meshPath = meshGraph->FindShortest(startMesh->centriod, endMesh->centriod, true);

		vector<Vector3> result;
		for (int i = 0; i < meshPath.size(); i++) {
			
			// Start
			if (i == 0) {
				result = (*meshDictionary->find(meshPath.at(i))).second->FindPath(start, meshPath.at(i+1));
			}

			// End
			else if (i + 1 == meshPath.size()) {
				if (path.size() == 0) {
					result = (*meshDictionary->find(meshPath.at(i))).second->FindPath(start, end);
				}
				else {
					result = (*meshDictionary->find(meshPath.at(i))).second->FindPath(path.at(path.size() - 1), end);
				}
			}

			// Middle
			else {
				// Add check to make it more natural and less through the middle of each mesh?
				if (path.size() == 0) {
					result = (*meshDictionary->find(meshPath.at(i))).second->FindPath(start, meshPath.at(i + 1));
				}
				else {
					result = (*meshDictionary->find(meshPath.at(i))).second->FindPath(path.at(path.size() - 1), meshPath.at(i + 1));
				}
			}
			path.insert(path.end(), result.begin(), result.end());
		}
	}

	std::vector<float> unpacked_waypoints = vector<float>();
	// First index as indicator for size of array
	unpacked_waypoints.push_back((path.size() * 3) + 1);
	for (int i = 0; i < path.size(); i++)
	{
		unpacked_waypoints.push_back(path[i].x);
		unpacked_waypoints.push_back(path[i].y);
		unpacked_waypoints.push_back(path[i].z);
	}

	float* data = new float[unpacked_waypoints.size()];
	std::copy(unpacked_waypoints.begin(), unpacked_waypoints.end(), data);
	return data;
}