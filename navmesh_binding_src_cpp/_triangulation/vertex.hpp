#ifndef VERTEX_HPP
#define VERTEX_HPP

#include "../vector3.hpp"
#include <string>

using namespace std;

namespace triangulation
{
	class Vertex
	{
	public:
		Vector3 Position;
		int Index;
		
		Vertex() {}
		Vertex(Vector3 position, int index) : Position(position), Index(index) { }
		
		bool Equals(Vertex other) {
			return other.Position.Equals(Position) && other.Index == Index;
		}

		bool operator==(Vertex vert)
		{
			return Equals(vert);
		}

		string ToString() {
			return "Position: " + Position.ToString() + " (" + to_string(Index) + ")";
		}
	};
}


#endif
