using UnityEngine;

/// <summary>
/// Represents a single space on the map for a tile to be potentially placed.
/// </summary>
public class Cell : IHeapItem<Cell>
{
    /// <summary>
    /// The position of the cell on the map, including its depth (z).
    /// </summary>
    public Vector3Int position;
    /// <summary>
    /// The parent of the current cell. Used for path generation.
    /// </summary>
    public Cell parent;
    public int hCost;
    public int gCost;
    public bool isTraversable;
    public enum CellStatus { ValidCell, InvalidCell, TerrainCell, LakeCell, RiverCell, WalkpathCell }
    public CellStatus status;
    int heapIndex;
    public bool onBoundary;
    public bool isWaterBound;

    // update bool based on cell status
    public void setCellStatus(CellStatus newStatus, bool intersectionsEnabled = false)
    {
        status = newStatus;
        switch (newStatus)
        {
            case CellStatus.ValidCell:
            case CellStatus.TerrainCell:
                isTraversable = onBoundary ? false : true;
                break;
            case CellStatus.RiverCell:
            case CellStatus.WalkpathCell:
                isTraversable = intersectionsEnabled;
                break;
            default:
                isTraversable = false;
                break;
        }
    }
    

    public Cell(Vector3Int position, CellStatus status = CellStatus.ValidCell)
    {
        this.position = position;
        setCellStatus(status);
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
