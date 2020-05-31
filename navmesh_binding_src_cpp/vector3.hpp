#ifndef VECTOR3_HPP
#define VECTOR3_HPP

#define INF Vector3(INFINITY, INFINITY, INFINITY);

#include <string>

namespace std
{
	class Vector3
	{
	public:
		float x = 0, y = 0, z = 0;

		Vector3();
		Vector3(float _x, float _y, float _z);
		~Vector3();

		Vector3& operator+(Vector3 vec);
		Vector3& operator-(Vector3 vec);
		Vector3& operator*(float scalar);
		Vector3& operator/(float scalar);
		Vector3& operator+=(Vector3 vec);
		bool operator==(Vector3 vec);
		bool operator==(const Vector3& t) const;

		bool operator<(const Vector3& vec) const;
		bool operator>(const Vector3& vec) const;
		float operator[](int i);
		float DistanceTo(Vector3 vec);
		float ManhattenDistanceTo(Vector3 vec);
		float DirectionTo(Vector3 vec);
		float Magnitude();
		float SqrMagnitude2D();
		Vector3& Normalize();
		float Dot(Vector3 vec);
		bool Equals(Vector3 vec);
		Vector3* Clone();

		string ToString();
	};

	struct Vector3Comparer {
		bool operator() (const Vector3& lhs, const Vector3& rhs) const
		{
			return (hash<float>()(lhs.x) ^ hash<float>()(lhs.y) << 2 ^ hash<float>()(lhs.z) >> 2)
				< (hash<float>()(rhs.x) ^ hash<float>()(rhs.y) << 2 ^ hash<float>()(rhs.z) >> 2);
		}
	};
}

#endif // !VECTOR3_HPP
