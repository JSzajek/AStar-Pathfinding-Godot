#pragma once

#include <iostream>
#include "Vec2.h"

struct Vec3
{
public:
	float x, y, z;
public:
	Vec3();
	Vec3(float _x, float _y, float _z);
	Vec3(const Vec3& other);

	float& operator[](int index);
	Vec3 operator+(const Vec3& other) const;
	Vec3 operator-(const Vec3& other) const;
	Vec3 operator*(const float& scalar) const;
	float operator*(const Vec3& other) const;
	bool operator==(const Vec3& other) const;
	bool operator!=(const Vec3& other) const;
	
	float Dot(const Vec3& other) const;
	float DistanceTo(const Vec3& other) const;
	float ManhattenDistanceTo(const Vec3& other) const;
	float Magnitude() const;
	void Normalize();
	float DirectionTo(const Vec3& other) const;
	std::string ToString() const;

	Vec2 ToVec2() const;

	friend std::ostream& operator<<(std::ostream& stream, const Vec3& other)
	{
		stream << "(" << other.x << ", " << other.y << ", " << other.z << ")";
		return stream;
	}
};