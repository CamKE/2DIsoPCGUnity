using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellPair : IHeapItem<CellPair>
{

    int heapIndex;

    int distance;
    public readonly Cell startCell;
    public readonly Cell endCell;

    public CellPair(Cell startCell, Cell endCell, int distance = 0)
    {
        this.startCell = startCell;
        this.endCell = endCell;
        this.distance = distance;
    }


    public CellPair(Cell startCell, Cell endCell)
    {
        this.startCell = startCell;
        this.endCell = endCell;
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }


    public int CompareTo(CellPair otherCell)
    {
        int compare = distance.CompareTo(otherCell.distance);

        // we want items with higher priority (larger values) to 
        // go lower down the tree and vice versa, so we return 
        // the inverse
        return -compare;
    }
}
