using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Describes the layout of the level. Set by the generators, and then used to populate the tilemaps.
/// </summary>
public class Map
{
    // 2D array of cells; the main component of the map
    private Cell[,] map;

    // the positions of the cells on the edge of the map
    private List<Vector2Int> boundaryCellPositions;

    /// <summary>
    /// Getter and setter for the current number of terrain cells on the map.
    /// </summary>
    public int terrainCellCount { get; private set; }

    /// <summary>
    /// The width of the map's 2D array.
    /// </summary>
    public readonly int width;

    /// <summary>
    /// The height of the map's 2D array.
    /// </summary>
    public readonly int height;

    /// <summary>
    /// The maps width multiplied by the maps height.
    /// </summary>
    public readonly int area;

    /// <summary>
    /// The shape of the map's terrain.
    /// </summary>
    public readonly TerrainGenerator.TerrainShape shape;

    /// <summary>
    /// Constructor for the Map. Fills the map with new cells with the given status (optional).
    /// </summary>
    /// <param name="mapWidth">The width of the map's 2D array.</param>
    /// <param name="mapHeight">The height of the map's 2D array.</param>
    /// <param name="shape">The shape of the map's terrain.</param>
    /// <param name="status">The status to set all cells to initially.</param>
    public Map(int mapWidth, int mapHeight, TerrainGenerator.TerrainShape shape, Cell.CellStatus status = Cell.CellStatus.ValidCell)
    {
        // create the 2D array of cells
        map = new Cell[mapWidth, mapHeight];

        // set the dimensions
        width = mapWidth;
        height = mapHeight;
        area = mapWidth * mapHeight;

        // give the level the shape
        this.shape = shape;

        // create the cells and set their statuses
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                addCell(new Vector2Int(x,y), status);
            }
        }

        // create the list of boundary cell positions
        boundaryCellPositions = new List<Vector2Int>();
    }

    /// <summary>
    /// Update the given cells status to a new status, and optinally specify whether intersections are on for cells along paths. 
    /// </summary>
    /// <param name="currentCell">The cell to be updated.</param>
    /// <param name="status">The new status.</param>
    /// <param name="intersectionsEnabled">Whether or not a cell can be intersected. Used for path generation.</param>
    public void updateCellStatus(Cell currentCell, Cell.CellStatus status, bool intersectionsEnabled = false)
    {
        // if the new status is not the same as the old
        if (currentCell.status != status)
        {
            // if the new status is a terrain cell
            if (status == Cell.CellStatus.TerrainCell)
            {
                // increment the terrain cell count
                terrainCellCount++;
            }

            // if the old status is a terrain cell
            if (currentCell.status == Cell.CellStatus.TerrainCell)
            {
                // decrement the count
                terrainCellCount--;
            }
        }

        // if the status of the cell is a river or lake cell
        if (status == Cell.CellStatus.RiverCell || status == Cell.CellStatus.LakeCell)
        {
            // foreach of the cells neighbours
            foreach (Cell neighbour in getNeighbours(currentCell))
            {
                // if the status of the neighbour is a terraincell
                if (neighbour.status == Cell.CellStatus.TerrainCell)
                {
                    // set it as a water boundary
                    neighbour.isWaterBound = true;
                }
            }
        }

        // set the new cell status and whether or not intersections are enabled
        currentCell.setCellStatus(status, intersectionsEnabled);
    }

    /// <summary>
    /// Get a cell from the map at the given Vector2Int position.
    /// </summary>
    /// <param name="cellPosition">The postion of the cell in VectorInt format.</param>
    /// <returns>The cell at the position.</returns>
    public Cell getCell(Vector2Int cellPosition)
    {
        return map[cellPosition.x, cellPosition.y];
    }

    /// <summary>
    /// Get a cell from the map at the given x and y int positions.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Cell getCell(int x, int y)
    {
        return map[x, y];
    }

    private void addCell(Vector2Int cellPosition, Cell.CellStatus status)
    {
        map[cellPosition.x, cellPosition.y] = new Cell((Vector3Int)cellPosition, status);
    }

    public Cell[,] getAll()
    {
        return map;
    }

    public List<Cell> getNeighbours(Cell currentNode)
    {
        List<Cell> neighbours = new List<Cell>();

        Vector3Int currentNodePosition = currentNode.position;

        // left
        if (currentNodePosition.x > 0)
        {
            Cell cell = getCell(currentNodePosition.x - 1, currentNodePosition.y);
            neighbours.Add(cell);

        }

        // right
        if (currentNodePosition.x < width - 1)
        {
            Cell cell = getCell(currentNodePosition.x + 1, currentNodePosition.y);
            neighbours.Add(cell);
        }

        // top
        if (currentNodePosition.y < height - 1)
        {
            Cell cell = getCell(currentNodePosition.x, currentNodePosition.y + 1);
            neighbours.Add(cell);
        }

        // bottom
        if (currentNodePosition.y > 0)
        {
            Cell cell = getCell(currentNodePosition.x, currentNodePosition.y - 1);
            neighbours.Add(cell);
        }

        return neighbours;
    }

    // finds the minimum depth (z value) from surrounding tiles
    public int getTerrainMinDepth(Cell currentNode)
    {
        int minDepth = currentNode.position.z;

        foreach (Cell neighbour in getNeighbours(currentNode))
        {
            if (neighbour.status == Cell.CellStatus.TerrainCell)
            {
                int neighbourDepth = neighbour.position.z;
                minDepth = neighbourDepth < minDepth ? neighbourDepth : minDepth;
            }
        }

        return minDepth;
    }

    public int getMinDepth(Cell currentNode)
    {
        int minDepth = currentNode.position.z;

        foreach (Cell neighbour in getNeighbours(currentNode))
        {
            if (neighbour.status != Cell.CellStatus.InvalidCell)
            {
                int neighbourDepth = neighbour.position.z;
                minDepth = neighbourDepth < minDepth ? neighbourDepth : minDepth;
            }

        }

        return minDepth;
    }

    public void checkForBoundaryCellRandom(Cell cell)
    {
        int validTileCount = 0;

        List<Cell> neighbours = getNeighbours(cell);

        if (neighbours.Count < 4)
        {
            // its a boundary tile
            addBoundaryCellPosition((Vector2Int)cell.position);
        }
        else
        {
            foreach (Cell neighbour in neighbours)
            {
                if (neighbour.status == Cell.CellStatus.ValidCell || neighbour.status == Cell.CellStatus.TerrainCell)
                {
                    validTileCount++;
                }
            }
            if (validTileCount < 4)
            {
                // its a boundary tile
                addBoundaryCellPosition((Vector2Int)cell.position);
            }
        }
    }

    public void checkForBoundaryCell(Cell cell)
    {
        List<Cell> neighbours = getNeighbours(cell);
        // remove this if want to revert

        if (neighbours.Count < 4)
        {
            // its a boundary tile
            addBoundaryCellPosition((Vector2Int)cell.position);
        }

    }


    public void addBoundaryCellPosition(Vector2Int position)
    {
        boundaryCellPositions.Add(position);
        getCell(position).onBoundary = true;
    }

    public List<Vector2Int> getBoundaryCellPositions()
    {
        return boundaryCellPositions;
    }

    public Vector3Int getRandomTerrainCellPosition()
    {
        while (true)
        {
            // random range is max exclusive
            Vector2Int cellPosition = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));

            if (getCell(cellPosition).status == Cell.CellStatus.TerrainCell)
            {
                return getCell(cellPosition).position;
            }
        }
    }

    public bool isValidCell(Vector2Int cellPosition)
    {
        if (cellPosition.x >= 0 && cellPosition.x < width)
        {
            if (cellPosition.y >= 0 && cellPosition.y < height)
            {
                Cell cell = getCell(cellPosition);
                if (cell.status == Cell.CellStatus.TerrainCell && !isBoundaryCell(cell) && !cell.isWaterBound)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool isBoundaryCell(Cell cell)
    {
        return boundaryCellPositions.Contains((Vector2Int)cell.position);
    }
}
