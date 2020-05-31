#ifndef MINHEAP_HPP
#define MINHEAP_HPP

#include "Indexable.hpp"
#include <vector>

namespace std
{
	template <class Indexable>
	class MinHeap
	{
	private:
		vector<Indexable*> harr;
	public:
		// Constructor with capacity
		MinHeap()
		{
			harr = vector<Indexable*>();
		}

		// Destructor
		~MinHeap() { }

		// Gets the current size of the heap.
		inline int size() { return harr.size(); }

		// Gets the index of the parent.
		inline int parent(Indexable* item) { return (item->index - 1) / 2.0f; }

		// Gets the index of the left child.
		inline int left(Indexable* item) { return (2 * item->index) + 1; }

		// Gets the index of the right child.
		inline int right(Indexable* item) { return (2 * item->index) + 2; }

		// Gets heap item at the index.
		inline Indexable* getIndex(int index) { return &harr[index]; }

		// Adds a new heap item.
		void Add(Indexable* item)
		{
			item->index = harr.size();
			harr.push_back(item);
			SortUp(item);
		}

		// Removes and returns the first heap, then resorts the heap.
		Indexable* RemoveFirst()
		{
			Indexable* first = harr.at(0);
			std::swap(harr[0], harr.back());
			harr.pop_back();
			if (harr.size() != 0) {
				harr.back()->index = harr.size() - 1;
				harr[0]->index = 0;
				SortDown(harr[0]);
			}
			return first;
		}

		// Updates the position of the item in relation to the heap.
		void UpdateItem(Indexable* item)
		{
			SortUp(item);
		}

		// Determines if the heap contains the passed item.
		bool Contains(Indexable* item)
		{
			if (item->index < 0 || item->index >= size())
			{
				return false;
			}
			return item->operator==(harr[item->index]);
		}

		// Sorts the larger item down until it's in the correct position
		void SortDown(Indexable* item)
		{
			Indexable* tempItem = item;
			while (true)
			{
				int childIndexLeft = left(tempItem);
				int childIndexRight = right(tempItem);
				int swapIndex = 0;
				if (childIndexLeft < size())
				{
					swapIndex = childIndexLeft;
					if (childIndexRight < size())
					{
						if ((harr[childIndexLeft]->operator<(harr[childIndexRight])) > 0)
						{
							swapIndex = childIndexRight;
						}
					}
					if ((tempItem->operator<(harr[swapIndex])) > 0)
					{
						Swap(tempItem->index, swapIndex);
						tempItem = harr[swapIndex];
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

		// Sorts the smaller item upwards until it's in the correct position
		void SortUp(Indexable* item)
		{
			Indexable* tempItem = item;

			int parentIndex = parent(item);
			while (true)
			{
				if ((tempItem->operator<(harr[parentIndex])) < 0)
				{
					Swap(tempItem->index, parentIndex);
					tempItem = harr[parentIndex];
				}
				else
				{
					break;
				}
				parentIndex = parent(tempItem);
			}
		}

		// Swaps the heap items
		void Swap(int first, int second)
		{
			std::swap(harr[first], harr[second]);
			harr[first]->index = first;
			harr[second]->index = second;
		}

		// Cleans up the indexable objects in the heap before release
		void CleanUp()
		{
			for (Indexable* ptr : harr) {
				ptr->CleanUp();
			}
			harr.clear();
		}
	};
}

#endif // !MINHEAP_HPP
