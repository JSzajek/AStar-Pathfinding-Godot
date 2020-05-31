#ifndef CYCLINGVECTOR_HPP
#define CYCLINGVECTOR_HPP

#include <vector>

using namespace std;

namespace triangulation
{
	/// <summary>
	/// Represents a generic cycling vector that
	/// loops back the index when it reaches either
	/// the end or front.
	/// </summary>
	template <class T>
	class CyclingVector
	{
	private:
		vector<T> list;
	public:
		CyclingVector() { }

		CyclingVector(vector<T> collection) {
			list = vector<T>(collection);
		}

		T operator[](int index) {
			while (index < 0) {
				index = list.size() + index;
			}
			if (index >= list.size()) {
				index %= list.size();
			}
			return list[index];
		}

		int Size() {
			return list.size();
		}

		void RemoveAt(int index) {
			list.erase(this[index]);
		}

		void Add(T item) {
			list.push_back(item);
		}

		bool Contains(T item) {
			if (find(list.begin(), list.end(), item) != list.end()) {
				return true;
			}
			return false;
		}

		void Remove(T item) {
			typename vector<T>::iterator iter = find(list.begin(), list.end(), item);
			if (iter != list.end()) {
				list.erase(iter);
			}
		}

		int IndexOf(T item) {
			for (int i = 0; i < list.size(); i++) {
				if (this->operator[](i) == item) {
					return i;
				}
			}
			return -1;
		}
	};
}


#endif