#pragma once

#include "pch.h"
#include <vector>

#define DEFAULT_SIZE 25

/// <summary>
/// Wrapper class holding the templated object with
/// an index used within the Minimum Heap.
/// </summary>
/// <typeparam name="T">The type of object</typeparam>
template <typename T>
class HeapItem
{
private:
	T m_valueRef;
	unsigned int m_index;
public:

	/// <summary>
	/// Initializes a new instance of the <see cref="HeapItem"/> class.
	/// </summary>
	/// <param name="valueRef">The value to wrap</param>
	/// <param name="index">The index of the heap item</param>
	HeapItem(T valueRef, unsigned int index) : m_valueRef(valueRef), m_index(index) {}

	/// <summary>
	/// Copy constructor of the <see cref="HeapItem"/> class.
	/// </summary>
	/// <param name="oldItem">The other to copy</param>
	HeapItem(const HeapItem<T>& oldItem) : m_valueRef(oldItem.GetCopy()), m_index(oldItem.GetIndex()) { }

	/// <summary>
	/// Retrieves a copy of the wrapped item.
	/// </summary>
	/// <returns>The copy of the wrapped item</returns>
	T GetCopy() const { return m_valueRef; }

	/// <summary>
	/// Retrieves a reference to the wrapped item.
	/// </summary>
	/// <returns>The wrapped item reference</returns>
	const T& GetItem() const { return m_valueRef; }
	
	/// <summary>
	/// The pointer to the wrapped item.
	/// </summary>
	/// <returns>The pointer of the item</returns>
	T* GetItemPtr() { return &m_valueRef; }

	/// <summary>
	/// Retrieves the heap item index.
	/// </summary>
	/// <returns>The index of the heap item</returns>
	const unsigned int GetIndex() const { return m_index; }

	/// <summary>
	/// Sets the index value of the heap item.
	/// </summary>
	/// <param name="index">The new index value</param>
	void SetIndex(const unsigned int index) { m_index = index; }

	/// <summary>
	/// Less than operator override.
	/// </summary>
	/// <param name="other">The other object to compare to</param>
	/// <returns>Whether the path point is less than the passed path point</returns>
	const bool operator<(const HeapItem<T>& other)
	{
		return m_valueRef < other.GetItem();
	}

	/// <summary>
	/// Greater than operator override.
	/// </summary>
	/// <param name="other">The other object to compare to</param>
	/// <returns>Whether the path point is greater than the passed path point</returns>
	const bool operator>(const HeapItem<T>& other)
	{
		return m_valueRef > other.GetItem();
	}

	/// <summary>
	/// Equality operator override.
	/// </summary>
	/// <param name="other">The other object to compare to</param>
	/// <returns>Whether they are are equal</returns>
	const bool operator==(const HeapItem<T>& other)
	{
		return m_valueRef == other.GetItem();
	}

	/// <summary>
	/// Inequality operator override.
	/// </summary>
	/// <param name="other">The other object to compare to</param>
	/// <returns>Whether they are are inequal</returns>
	const bool operator!=(const HeapItem<T>& other)
	{
		return !(*this == other);
	}
};

/// <summary>
/// Class representing a Minimum Heap data structure,
/// in which the smallest element is the first one out.
/// </summary>
/// <typeparam name="T">The type of object within the data structure</typeparam>
template <typename T>
class MinHeap 
{
private:
	int m_currSize;
	std::vector<HeapItem<T>> values;
public:

	/// <summary>
	/// Initializes a new instance of the <see cref="MinHeap"/> class.
	/// </summary>
	MinHeap() 
		: m_currSize(0)
	{
		values.reserve(DEFAULT_SIZE);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MinHeap"/> class.
	/// </summary>
	/// <param name="max_size">The initial maximum size of the heap</param>
	MinHeap(size_t max_size) 
		: m_currSize(0)
	{
		values.reserve(max_size);
	}

	/// <summary>
	/// Retrieves the current size of the heap.
	/// </summary>
	/// <returns>The size of the heap</returns>
	const size_t Size() const { return values.size(); }

	/// <summary>
	/// Adds the passed object into the minimum heap and
	/// resorting as necessary.
	/// </summary>
	/// <param name="item">The object to add</param>
	void Add(T item) 
	{
		values.emplace_back(item, m_currSize);
		SortUp(values[m_currSize]);
		m_currSize++;
	}

	/// <summary>
	/// Pops the first element from the minimum heap and
	/// resorts to fill in the gap.
	/// </summary>
	/// <returns>The top minimum element</returns>
	T RemoveFirst()
	{
		auto first = values[0].GetItem();
		Swap(0, m_currSize - 1);
		values.pop_back();
		m_currSize--;
		if (m_currSize != 0)
		{
			SortDown(values[0]);
		}
		return first;
	}

	/// <summary>
	/// Determines whether the minimum heap contains the 
	/// passed object.
	/// </summary>
	/// <param name="item">The item to find</param>
	/// <returns>Whether the heap contains the element</returns>
	bool Contains(T item)
	{
		return Find(item) != NULL;
	}

	/// <summary>
	/// Attempts to find the element within the minimum heap.
	/// Returning NULL if it is not found.
	/// </summary>
	/// <param name="item">The item to find</param>
	/// <returns>The pointer to the object or NULL if not found</returns>
	HeapItem<T>* Find(T item)
	{
		for (int i = 0; i < m_currSize; i++)
		{
			if (values[i].GetItem() == item)
			{
				return &values[i];
			}
		}
		return NULL;
	}

	/// <summary>
	/// Updates the position of the item after its values have been changed.
	/// </summary>
	/// <param name="ptr">The heap item to update</param>
	void UpdateItem(HeapItem<T>* ptr)
	{
		SortUp(*ptr);
	}

private:

	/// <summary>
	/// Retrieves the parent index of the passed heap item.
	/// </summary>
	/// <param name="item">The item to get the parent of</param>
	/// <returns>The parent index</returns>
	const int Parent(HeapItem<T> item) { return (item.GetIndex() - 1) / 2.0f; }
	
	/// <summary>
	/// Retrieves the left child index of the passed heap item.
	/// </summary>
	/// <param name="item">The item to get the left child of</param>
	/// <returns>The left child index</returns>
	const int Left(HeapItem<T> item) { return (2 * item.GetIndex()) + 1; }

	/// <summary>
	/// Retrieves the right child index of the passed heap item.
	/// </summary>
	/// <param name="item">The item to get the right child of</param>
	/// <returns>The right child index</returns>
	const int Right(HeapItem<T> item) { return (2 * item.GetIndex()) + 2; }

	/// <summary>
	/// Sorts the passed heap item upwards.
	/// </summary>
	/// <param name="item">The heap item to sort</param>
	void SortUp(HeapItem<T> item)
	{
		int parentIndex = Parent(item);
		while (parentIndex >= 0)
		{
			if (item < values[parentIndex])
			{
				Swap(item.GetIndex(), parentIndex);
				item = values[parentIndex];
			}
			else
			{
				break;
			}
			parentIndex = Parent(item);
		}
	}

	/// <summary>
	/// Sorts the passed heap item downwards.
	/// </summary>
	/// <param name="item">The heap item to sort</param>
	void SortDown(HeapItem<T> item)
	{
		while (true)
		{
			int leftIndex = Left(item);
			int rightIndex = Right(item);
			int swapIndex = 0;
			if (leftIndex < Size()) 
			{
				swapIndex = leftIndex;
				if (rightIndex < Size())
				{
					if (values[leftIndex] > values[rightIndex])
					{
						swapIndex = rightIndex;
					}
				}
				if (item > values[swapIndex])
				{
					Swap(item.GetIndex(), swapIndex);
					item = values[swapIndex];
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}
	}

	/// <summary>
	/// Swaps the elements at the passed indices.
	/// </summary>
	/// <param name="first">The first index</param>
	/// <param name="second">The second index</param>
	void Swap(int first, int second)
	{
		// Look into not using std::swap to avoid copying
		std::swap(values[first], values[second]);
		values[first].SetIndex(first);
		values[second].SetIndex(second);
	}
};