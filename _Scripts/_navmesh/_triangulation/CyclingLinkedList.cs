using System;
using System.Collections.Generic;

/// <summary>
/// Class reprenting a linked list that wraps around on
/// its index.
/// </summary>
/// <typeparam name="T">The type of objects stored</typeparam>
public class CyclingLinkedList<T> : LinkedList<T> {

    /// <summary>
    /// Overriding index operator to perform index wrapping
    /// </summary>
    /// <value>The object at the passed index</value>
    public LinkedListNode<T> this[int index]
    {
        get
        {
            //perform the index wrapping
            while (index < 0)
                index = Count + index;
            if (index >= Count)
                index %= Count;

            //find the proper node
            LinkedListNode<T> node = First;
            for (int i = 0; i < index; i++)
                node = node.Next;

            return node;
        }
    }

    /// <summary>
    /// Removes the object at the passed index
    /// </summary>
    /// <param name="index">The index to remove at</param>
    public void RemoveAt(int index)
    {
        Remove(this[index]);
    }

    /// <summary>
    /// Gets the index of the passed object
    /// </summary>
    /// <param name="item">The index to get the index of</param>
    /// <returns>The index of the passed object</returns>
    public int IndexOf(T item)
    {
        for (int i = 0; i < Count; i++)
            if (this[i].Value.Equals(item))
                return i;

        return -1;
    }
}