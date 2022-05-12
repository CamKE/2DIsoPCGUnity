using UnityEngine;

/// <summary>
/// Represents a single space on the map for a tile to be potentially placed.
/// </summary>
public class Cell : IHeapItem<Cell>
{
    /// <summary>
    /// The position of the cell on the Map, including its depth (z).
    /// </summary>
    public Vector3Int position;
    /// <summary>
    /// The parent of the current cell. Used for path generation.
    /// </summary>
    public Cell parent;
    /// <summary>
    /// The estimated cost of traversal from the cell to the end goal cell.
    /// </summary>
    public int hCost;
    /// <summary>
    /// The cost of traversal between the start cell and the cell.
    /// </summary>
    public int gCost;
    /// <summary>
    /// Denotes whether the cell is able to be travelled across.
    /// </summary>
    public bool isTraversable;
    /// <summary>
    /// Enumeration of cell states. Used to distinguish the various cells from eachother.
    /// </summary>
    public enum CellStatus { ValidCell, InvalidCell, TerrainCell, LakeCell, RiverCell, WalkpathCell }
    /// <summary>
    /// Denotes the cells current state.
    /// </summary>
    public CellStatus status;
    // the position of the cell in the heap
    private int heapIndex;
    /// <summary>
    /// Denotes whether the cell is on the boundary; that is, if the cell neighours an invalid cell or is at the end of the map.
    /// </summary>
    public bool onBoundary;
    /// <summary>
    /// Denotes whether the cell is a water boundary; that is , if the cell is adjacent to water.
    /// </summary>
    public bool isWaterBound;

    /// <summary>
    /// Sets the cell's status to a new status.
    /// </summary>
    /// <param name="newStatus">The new status to set the cell to.</param>
    /// <param name="intersectionsEnabled">Whether intersections are enabled. For path generation. Optional parameter.</param>
    public void setCellStatus(CellStatus newStatus, bool intersectionsEnabled = false)
    {
        // set the new status
        status = newStatus;

        // check the status
        switch (newStatus)
        {
            // if it is a valid or terrain cell
            case CellStatus.ValidCell:
            case CellStatus.TerrainCell:
                // if the cell is on the boundary, then it cannot be traversed
                isTraversable = !onBoundary;
                break;
            // if it is a river or walkpath cell
            case CellStatus.RiverCell:
            case CellStatus.WalkpathCell:
                // if intersections are enabled, then the cell can be traversed
                isTraversable = intersectionsEnabled;
                break;
            // otherwise for any other cell status (invalid, lake)
            default:
                // set to false
                isTraversable = false;
                break;
        }
    }

    /// <summary>
    /// Constructor for Cell. Must set an initial postions and status.
    /// </summary>
    /// <param name="position">The position of the cell on the Map, including its depth (z).</param>
    /// <param name="status">Denotes the cells current state.</param>
    public Cell(Vector3Int position, CellStatus status = CellStatus.ValidCell)
    {
        this.position = position;
        setCellStatus(status);
    }

    /// <summary>
    /// Calculates the f cost, which is a sum of the g cost and h cost.
    /// </summary>
    /// <returns>The f cost.</returns>
    public int fCost()
    {
        return gCost + hCost;
    }

    /// <summary>
    /// Getter and setter for the heap index.
    /// </summary>
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

   /// <summary>
   /// Used to order cells in the heap based on lowest fCost or hCost first. Used during path generation.
   /// </summary>
   /// <param name="otherCell">The cell to compare to.</param>
   /// <returns>The priority of the cell relative to the other cell. -1 for lower priority, 0 for same priority, and 1 for higher priority.</returns>
    public int CompareTo(Cell otherCell)
    {
        // compare the f cost of both cells
        int compare = fCost().CompareTo(otherCell.fCost());
        // if they have the same f cost
        if (compare == 0)
        {
            // compare g costs
            compare = hCost.CompareTo(otherCell.hCost);
        }

        // we want items with higher priority (larger values) to 
        // go lower down the tree and vice versa, so we return 
        // the inverse
        return -compare;
    }
}
