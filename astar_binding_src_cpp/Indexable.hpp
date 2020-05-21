#ifndef INDEXABLE_HPP
#define INDEXABLE_HPP

// Class representing an indexable object
class Indexable
{
public:
	int index;

	// Default constructor
	Indexable()
{
		index = -1;
	}

	// Constructor with values
	Indexable(int index)
	{
		this->index = index;
	}

	// Clean up method
	virtual void CleanUp() {
	}

	// Comparison method
	virtual int operator<(Indexable* comparison)
	{
		return this->index - comparison->index;
	}

	// Equality method
	virtual bool operator==(Indexable* comparison)
	{
		return this->index == comparison->index;
	}

	// Inequality method
	virtual bool operator!=(Indexable* comparison)
	{
		return this->index != comparison->index;
	}
};

#endif