#ifndef VECTOR2_HPP
#define VECTOR2_HPP

#include <string>

namespace std
{
	class Vector2
	{
	public:
		float x;
		float y;

		Vector2();
		Vector2(float _x, float _y);
		~Vector2();

		Vector2& operator+(Vector2 vec);
		Vector2& operator-(Vector2 vec);
		Vector2& operator*(float scalar);
		float DistanceTo(Vector2 vec);
		float Magnitude();
		Vector2& Normalize();
		float Dot(Vector2 vec);
		bool Equals(Vector2 vec);

		string ToString();
	};
}

#endif // !VECTOR2_HPP
