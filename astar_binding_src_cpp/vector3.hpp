#ifndef VECTOR3_HPP
#define VECTOR3_HPP

#include <string>

namespace std
{
	class Vector3
	{
	public:
		float x = 0, y = 0, z = 0;

		Vector3();
		Vector3(float _x, float _y, float _z);
		Vector3(const Vector3& other);
		~Vector3();

		Vector3& operator+(Vector3 vec);
		Vector3& operator-(Vector3 vec);
		Vector3& operator*(float scalar);
		float operator[](int i);
		float DistanceTo(Vector3 vec);
		float ManhattenDistanceTo(Vector3 vec);
		float DirectionTo(Vector3 vec);
		float Magnitude();
		Vector3& Normalize();
		float Dot(Vector3 vec);
		bool Equals(Vector3 vec);
		Vector3* Clone();

		string ToString();
	};
}

#endif // !VECTOR3_HPP
