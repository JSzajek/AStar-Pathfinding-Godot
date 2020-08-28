#pragma once

#include "pch.h"
#include <vector>

template <typename T>
class HeapItem
{
private:
	T m_valueRef;
	unsigned int m_index;
public:
	HeapItem(T valueRef, unsigned int index) : m_valueRef(valueRef), m_index(index) {}

	HeapItem(const HeapItem<T>& oldItem) : m_valueRef(oldItem.GetCopy()), m_index(oldItem.GetIndex()) { }

	T GetCopy() const { return m_valueRef; }

	const T& GetItem() const { return m_valueRef; }
	
	T* GetItemPtr() { return &m_valueRef; }

	const unsigned int GetIndex() const { return m_index; }

	void SetIndex(const unsigned int index) { m_index = index; }

	const bool operator<(const HeapItem<T>& other)
	{
		return m_valueRef < other.GetItem();
	}

	const bool operator>(const HeapItem<T>& other)
	{
		return m_valueRef > other.GetItem();
	}

	const bool operator==(const HeapItem<T>& other)
	{
		return m_valueRef == other.GetItem();
	}

	const bool operator!=(const HeapItem<T>& other)
	{
		return !(*this == other);
	}
};

#define DEFAULT_SIZE 25

template <typename T>
class MinHeap 
{
private:
	int m_currSize;
	std::vector<HeapItem<T>> values;
public:
	MinHeap() : m_currSize(0)
	{
		values.reserve(DEFAULT_SIZE);
	}

	MinHeap(size_t size) : m_currSize(0)
	{
		values.reserve(size);
	}

	~MinHeap()
	{
	}
	
	const size_t Size() const { return values.size(); }

	const T Get(unsigned int index) { return values[index].GetItem(); }

	void Add(T item) 
	{
		values.emplace_back(item, m_currSize);
		SortUp(values[m_currSize]);
		m_currSize++;
	}

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

	bool Contains(T item)
	{
		return Find(item) != NULL;
	}

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

	void UpdateItem(HeapItem<T>* ptr)
	{
		SortUp(*ptr);
	}

private:
	const int Parent(HeapItem<T> item) { return (item.GetIndex() - 1) / 2.0f; }
	
	const int Left(HeapItem<T> item) { return (2 * item.GetIndex()) + 1; }

	const int Right(HeapItem<T> item) { return (2 * item.GetIndex()) + 2; }

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

	void Swap(int first, int second)
	{
		// Look into not using std::swap to avoid copying
		std::swap(values[first], values[second]);
		values[first].SetIndex(first);
		values[second].SetIndex(second);
	}
};