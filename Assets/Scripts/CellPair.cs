/// <summary>
/// Represents the start and end cell of a path.
/// </summary>
public class CellPair : IHeapItem<CellPair>
{
    // the position of the cell in the heap
    private int heapIndex;
    // the distance between the start and end cells
    private int distance;
    /// <summary>
    /// The start cell of the path.
    /// </summary>
    public readonly Cell startCell;
    /// <summary>
    /// The end cell of the path.
    /// </summary>
    public readonly Cell endCell;

    /// <summary>
    /// Constructor for the CellPair. Distance is optional.
    /// </summary>
    /// <param name="startCell">The start cell of the path.</param>
    /// <param name="endCell">The end cell of the path.</param>
    /// <param name="distance">The distance between the start and end cells.</param>
    public CellPair(Cell startCell, Cell endCell, int distance = 0)
    {
        // set the variables
        this.startCell = startCell;
        this.endCell = endCell;
        this.distance = distance;
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
    /// Used to order the CellPairs in the heap based on the lowest distance first.
    /// </summary>
    /// <param name="otherCell">The cell to compare to.</param>
    /// <returns></returns>
    public int CompareTo(CellPair otherCell)
    {
        // compare the distance of both cells
        int compare = distance.CompareTo(otherCell.distance);

        // we want items with higher priority (larger values) to 
        // go lower down the tree and vice versa, so we return 
        // the inverse
        return -compare;
    }
}
