#ifndef CYCLINGLINKEDLIST_HPP
#define CYCLINGLINKEDLIST_HPP

using namespace std;

namespace triangulation
{
	/// <summary>
	/// Representing a cycling linked list
	/// that loops back to the front or end
	/// when it reaches either.
	/// </summary>
	template <class T>
	struct Node {
		T data;
		Node<T>* next;

		Node() : next(NULL), data(T()){}
	};
	
	template <class T>
	class CyclingLinkedList {
	private:
		Node<T>* head;
		Node<T>* tail;
		int size = 0;
		
	public:
		CyclingLinkedList() {
			head = NULL;
			tail = NULL;
		}

		Node<T>* operator[](int index) {
			while (index < 0) {
				index = size + index;
			}
			if (index >= size) {
				index %= size;
			}

			Node<T>* node = head;
			for (int i = 0; i < index; i++) {
				node = node->next;
			}
			return node;
		}

		int Size() {
			return size;
		}

		void AddLast(T item) {
			Node<T>* temp = new Node<T>();
			temp->data = item;
			temp->next = NULL;

			if (head == NULL) {
				head = temp;
				tail = temp;
			}
			else {
				tail->next = temp;
				tail = tail->next;
			}
			size++;
		}

		void AddAfter(Node<T>* prev, T item) {
			Node<T>* temp = new Node<T>;
			temp->data = item;
			temp->next = prev->next;
			prev->next = temp;
			size++;
		}

		void AddFirst(T item) {
			Node<T>* temp = new Node<T>;
			temp->data = item;
			temp->next = head;
			head = temp;
			size++;
		}

		void Remove(Node<T>* deleting) {
			if (head == deleting) {
				if (head->next == NULL) {
					return;
				}
				head->data = head->next->data;
				deleting = head->next;
				head->next = head->next->next;
				delete deleting;
			}
			else {
				Node<T>* prev = head;
				while (prev->next != NULL && prev->next != deleting) {
					prev = prev->next;
				}
				if (prev->next == NULL) {
					return;
				}
				prev->next = prev->next->next;
				delete deleting;
			}
			size--;
		}

		void RemoveAt(int index) {
			Remove(this->operator[](index));
		}

		int IndexOf(T item) {
			for (int i = 0; i < size; i++) {
				if (this->operator[](i)->data == item) {
					return i;
				}
			}
			return -1;
		}

		bool Contains(T item) {
			Node<T>* temp = head;
			for (int i = 0; i < size; i++) {
				if (temp->data == item) {
					return true;
				}
				temp = temp->next;
			}
			return false;
		}

		
	};
}

#endif