using System.Collections.Generic;

/// <summary>
/// Class reprenting a list that wraps around on its index.
/// </summary>
/// <typeparam name="T">The type of objects stored</typeparam>
public class CyclingList<T> : List<T>
{
    /// <summary>
    /// Overriding index operator to perform index wrapping
    /// </summary>
    /// <value>The object at the passed index</value>
    public new T this[int index]
    {
        get
        {
            //perform the index wrapping
            while (index < 0)
                index = Count + index;
            if (index >= Count)
                index %= Count;

            return base[index];
        }
        set
        {
            //perform the index wrapping
            while (index < 0)
                index = Count + index;
            if (index >= Count)
                index %= Count;

            base[index] = value;
        }
    }

    /// <summary>
    /// Constructor Initializing a blank list
    /// </summary>
    public CyclingList() { }

    /// <summary>
    /// Removes the object at the passed index
    /// </summary>
    /// <param name="index">The index to remove at</param>
    public new void RemoveAt(int index)
    {
        Remove(this[index]);
    }
}