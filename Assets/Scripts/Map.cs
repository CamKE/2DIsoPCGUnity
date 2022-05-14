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
    /// <param name="x">The x position of the cell.</param>
    /// <param name="y">The y position of the cell.</param>
    /// <returns></returns>
    public Cell getCell(int x, int y)
    {
        return map[x, y];
    }

    /// <summary>
    /// Creates a new cell at the position with the given status.
    /// </summary>
    /// <param name="cellPosition">The position to create the cell.</param>
    /// <param name="status">The status of the cell.</param>
    private void addCell(Vector2Int cellPosition, Cell.CellStatus status)
    {
        map[cellPosition.x, cellPosition.y] = new Cell((Vector3Int)cellPosition, status);
    }

    /// <summary>
    /// Retrieve the  von Neumann neighborhood neighbours of the cells given at a
    /// manhattan distance of 1.
    /// </summary>
    /// <param name="currentCell">The cell to get the neighbours for.</param>
    /// <returns>The list of all neighbouring cells.</returns>
    public List<Cell> getNeighbours(Cell currentCell)
    {
        // create the neighbouring cells list
        List<Cell> neighbours = new List<Cell>();

        // get the position of the current cell
        Vector3Int currentCellPosition = currentCell.position;

        /*
         * Retrieve neighbour in each of the 4 directions and add them to the list.
         */

        // left neighbour
        if (currentCellPosition.x > 0)
        {
            Cell cell = getCell(currentCellPosition.x - 1, currentCellPosition.y);
            neighbours.Add(cell);

        }

        // right neighbour
        if (currentCellPosition.x < width - 1)
        {
            Cell cell = getCell(currentCellPosition.x + 1, currentCellPosition.y);
            neighbours.Add(cell);
        }

        // top neighbour
        if (currentCellPosition.y < height - 1)
        {
            Cell cell = getCell(currentCellPosition.x, currentCellPosition.y + 1);
            neighbours.Add(cell);
        }

        // bottom neighbour
        if (currentCellPosition.y > 0)
        {
            Cell cell = getCell(currentCellPosition.x, currentCellPosition.y - 1);
            neighbours.Add(cell);
        }

        return neighbours;
    }

    /// <summary>
    /// Finds the minimum depth (z value) from the cells surrounding the cell given.
    /// </summary>
    /// <param name="currentCell">The cell to find the minimum depth for.</param>
    /// <param name="onlyTerrainCells">Whether or not only terrain cells should be checked.</param>
    /// <returns>The minimum depth amongst the cell and its neighbours.</returns>
    public int getMinDepth(Cell currentCell, bool onlyTerrainCells = false)
    {
        // set the initial minimum depth to be the current cells depth
        int minDepth = currentCell.position.z;
        // flag to determine if the neighbours depth should be checked
        bool validNeighbour;

        // for each of the cells neighbours
        foreach (Cell neighbour in getNeighbours(currentCell))
        {
            // set the flag based on the onlyterraincells bool
            validNeighbour = onlyTerrainCells ? neighbour.status == Cell.CellStatus.TerrainCell : neighbour.status != Cell.CellStatus.InvalidCell;

            // if the neighbouring cell is a valid neighbour
            if (validNeighbour)
            {
                // compare the neighbours depth to the current minimum depth.
                // set the min depth to the lowest value
                minDepth = neighbour.position.z < minDepth ? neighbour.position.z : minDepth;
            }

        }
        return minDepth;
    }

    /// <summary>
    /// Check if a given cell on a randomly shaped map is a boundary cell. 
    /// </summary>
    /// <param name="cell">The cell to be checked.</param>
    public void checkForBoundaryCellRandom(Cell cell)
    {
        // number of valid tiles amongst the cells neighbours
        int validNeighbourCount = 0;
        
        // get the neighbours of the cell
        List<Cell> neighbours = getNeighbours(cell);

        // if there are not 4 neighbours
        if (neighbours.Count != 4)
        {
            // its a boundary tile
            addBoundaryCellPosition(cell);
        }
        else
        // otherwise
        {
            // for each of the cells neighbour
            foreach (Cell neighbour in neighbours)
            {
                // if the cell is invalid
                if (neighbour.status == Cell.CellStatus.InvalidCell)
                {
                    // there can no longer be 4 valid neighbours, break.
                    break;
                }
                // its a valid neighbour, increment the count
                validNeighbourCount++;
            }
            // if there are not 4 valid neighbours
            if (validNeighbourCount != 4)
            {
                // its a boundary tile
                addBoundaryCellPosition(cell);
            }
        }
    }

    /// <summary>
    /// Check if a given cell on a square or rectangular map is a boundary cell.
    /// </summary>
    /// <param name="cell">The cell to be checked.</param>
    public void checkForBoundaryCell(Cell cell)
    {
        // get the neighbours of the cell
        List<Cell> neighbours = getNeighbours(cell);

        // if there are not 4 neighbours
        if (neighbours.Count != 4)
        {
            // its a boundary tile
            addBoundaryCellPosition(cell);
        }
    }

    // add a boundary cell's position to the list of boundary cell positions
    // also set the cells onBoundary flag to true
    private void addBoundaryCellPosition(Cell cell)
    {
        boundaryCellPositions.Add((Vector2Int)cell.position);
        cell.onBoundary = true;
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
