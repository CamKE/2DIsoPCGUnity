using System;

/// <summary>
/// A binary heap written by Sebastian Lague. 
/// <seealso href="https://www.youtube.com/watch?v=3Dw5d7PlcTM">Heap video</seealso>
/// </summary>
/// <typeparam name="T">The type of object to be sorted in the heap.</typeparam>
public class Heap<T> where T : IHeapItem<T>
{
	// array holding the items in the heap
	private T[] items;
	// the number of items in the heap
	private int currentItemCount;

	/// <summary>
	/// Constructor taking in the max size of the heap.
	/// </summary>
	/// <param name="maxHeapSize">The maxiumum possible size of the heap.</param>
	public Heap(int maxHeapSize)
	{
		// create the array with size given
		items = new T[maxHeapSize];
	}

	/// <summary>
	/// Adds a new item to the heap and sorts it.
	/// </summary>
	/// <param name="item">The item to be added to the heap.</param>
	public void Add(T item)
	{
		// set the index of the item to the current count (e.g. 0 when array is empty)
		item.HeapIndex = currentItemCount;
		// put the item in the array at its index
		items[currentItemCount] = item;
		// the item is at the end of the array,
		// move the item into the correct position in the heap
		SortUp(item);
		// increment the item count
		currentItemCount++;
	}

	/// <summary>
	/// Retrieve an item from the heap at the given index.
	/// </summary>
	/// <param name="index">The index position of the item to retrieve.</param>
	/// <returns></returns>
	public T getItem(int index)
    {
		return items[index];
    }

	/// <summary>
	/// Removes the first item from the heap and sorts the heap.
	/// </summary>
	/// <returns>The first item in the heap.</returns>
	public T RemoveFirst()
	{
		// get the first item
		T firstItem = items[0];
		// decrease the count
		currentItemCount--;
		// put the item at the end of the heap into the first position
		items[0] = items[currentItemCount];
		// set the items heap index to 0
		items[0].HeapIndex = 0;
		// sort the item down the heap
		SortDown(items[0]);
		// return the first item
		return firstItem;
	}

	/// <summary>
	/// Updates the heap position of the item given.
	/// </summary>
	/// <param name="item">The item to update the position for.</param>
	public void UpdateItem(T item)
	{
		SortUp(item);
	}

	/// <summary>
	/// Gets the number of items in the heap.
	/// </summary>
	public int Count
	{
		get
		{
			return currentItemCount;
		}
	}

	/// <summary>
	/// Checks if the given item is in the heap.
	/// </summary>
	/// <param name="item">The item to find.</param>
	/// <returns>True if the item is in the heap, false if it is not.</returns>
	public bool Contains(T item)
	{
		// check if the item at the index of the item to be checked is
		// the same as the item to be checked
		return Equals(items[item.HeapIndex], item);
	}

	// sort the items position down the heap
	private void SortDown(T item)
	{
		while (true)
		{
			// get the left child index
			int childIndexLeft = item.HeapIndex * 2 + 1;
			// get the right child index
			int childIndexRight = item.HeapIndex * 2 + 2;
			
			int swapIndex;

			// check that the left child has children
			// if the index of the left child is less than the
			// current number of items
			if (childIndexLeft < currentItemCount)
			{
				// set swap index to left childs index
				swapIndex = childIndexLeft;

				// check that the left child has children
				if (childIndexRight < currentItemCount)
				{
					// compare the left and right index.
					// if the left has lower priority
					if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
					{
						// set swap index to right childs index
						swapIndex = childIndexRight;
					}
				}

				// we now have the child with the highest prioity
				// now we compare the parent to the highest priority child

				// if the parent is lower priority than the child
				if (item.CompareTo(items[swapIndex]) < 0)
				{
					// swap position of parent and child
					Swap(item, items[swapIndex]);
				}
				else
				{
					// otherwise, the parent is already in the correct
					// position relative to its children
					return;
				}
			}
			else
			{
				// the parent has no children, no need to sort
				return;
			}
		}
	}

	// sort the items position up the heap
	private void SortUp(T item)
	{
		// get the index of the parent
		int parentIndex = (item.HeapIndex - 1) / 2;

		while (true)
		{
			// get the parent
			T parentItem = items[parentIndex];

			// compareto returns 1 if priority is higher,
			// 0 if the same, and -1 if lower
			// if item priority is higher
			if (item.CompareTo(parentItem) > 0)
			{
				// then swap the item
				Swap(item, parentItem);
			}
			else
			{
				// get out of the loop
				break;
			}
			// get the new parent of the item
			parentIndex = (item.HeapIndex - 1) / 2;
		}
	}

	// swaps two items positions in the heap
	private void Swap(T itemA, T itemB)
	{
		// put item b at item a's index
		items[itemA.HeapIndex] = itemB;
		// put item a at item b's index
		items[itemB.HeapIndex] = itemA;
		// temp hold of item a's index
		int itemAIndex = itemA.HeapIndex;
		// change item a's index to be item b's
		itemA.HeapIndex = itemB.HeapIndex;
		// give item b item a's index
		itemB.HeapIndex = itemAIndex;
	}
}

/// <summary>
/// An interface to make items in the heap required to store an index which will then be compared by priority for ordering.
/// </summary>
/// <typeparam name="T">The type of object to be sorted in the heap.</typeparam>
public interface IHeapItem<T> : IComparable<T>
{
	/// <summary>
	/// Getter and setter for the heap index.
	/// </summary>
	public int HeapIndex
	{
		get;
		set;
	}
}