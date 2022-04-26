using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : IHeapItem<Cell>
{

    public Vector3Int position;
    public Cell parent;
    public int hCost;
    public int gCost;
    public bool isTraversable;
    public enum CellStatus { ValidCell, InvalidCell, TerrainCell, LakeCell, RiverCell, OutOfBounds }
    public CellStatus status;
    int heapIndex;
    
    // update bool based on cell status
    public void setCellStatus(CellStatus newStatus)
    {
        status = newStatus;
        switch (newStatus)
        {
            case CellStatus.ValidCell:
            case CellStatus.TerrainCell:
                isTraversable = true;
                break;
            default:
                isTraversable = false;
                break;
        }
    }
    

    public Cell(Vector3Int position, bool isTraversable)
    {
        this.position = position;
        this.isTraversable = isTraversable;
    }

    public int fCost()
    {
        return gCost + hCost;
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

   
    public int CompareTo(Cell otherCell)
    {
        int compare = fCost().CompareTo(otherCell.fCost());
        if (compare == 0)
        {
            compare = hCost.CompareTo(otherCell.hCost);
        }
        // we want items with higher priority (larger values) to 
        // go lower down the tree and vice versa, so we return 
        // the inverse
        return -compare;
    }
}
