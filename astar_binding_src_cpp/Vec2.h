#pragma once

#include <iostream>

struct Vec2
{
public:
	float x, y;
public:
	Vec2();
	Vec2(float _x, float _y);
	Vec2(const Vec2& other);

	float& operator[](int index);
	Vec2 operator+(const Vec2& other) const;
	Vec2 operator-(const Vec2& other) const;
	Vec2 operator*(const float& scalar) const;
	float operator*(const Vec2& other) const;
	bool operator==(const Vec2& other) const;
	bool operator!=(const Vec2& other) const;

	float Dot(const Vec2& other) const;
	float DistanceTo(const Vec2& other) const;
	float ManhattenDistanceTo(const Vec2& other) const;
	float Magnitude() const;
	void Normalize();
	float DirectionTo(const Vec2& other);

	friend std::ostream& operator<<(std::ostream& stream, const Vec2& other)
	{
		stream << "(" << other.x << ", " << other.y << ")";
		return stream;
	}
};