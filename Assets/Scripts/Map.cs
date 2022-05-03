using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    private Cell[,] map;

    private List<Vector2Int> boundaryCellPositions;

    public int terrainCellCount { get; private set; }

    public readonly int width;

    public readonly int height;

    public readonly int area;

    public Map(int mapWidth, int mapHeight, Cell.CellStatus status = Cell.CellStatus.ValidCell)
    {
        map = new Cell[mapWidth, mapHeight];
        width = mapWidth;
        height = mapHeight;
        area = mapWidth * mapHeight;

        // set all cells to be invalid initially
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                addCell(new Vector2Int(x,y), status);
            }
        }

        boundaryCellPositions = new List<Vector2Int>();
    }

    public void updateCellStatus(Cell currentCell, Cell.CellStatus status, bool intersectionsEnabled = false)
    {
        if (currentCell.status != status)
        {
            if (status == Cell.CellStatus.TerrainCell)
            {
                terrainCellCount++;
            }

            if (currentCell.status == Cell.CellStatus.TerrainCell)
            {
                terrainCellCount--;
            }

        }

        currentCell.setCellStatus(status, intersectionsEnabled);
        //foreach get neighbours here
    }

    public Cell getCell(Vector2Int cellPosition)
    {
        return map[cellPosition.x, cellPosition.y];
    }

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
            neighbours.Add(getCell(currentNodePosition.x - 1, currentNodePosition.y));
        }

        // right
        if (currentNodePosition.x < width - 1)
        {
            neighbours.Add(getCell(currentNodePosition.x + 1, currentNodePosition.y));
        }

        // top
        if (currentNodePosition.y < height - 1)
        {
            neighbours.Add(getCell(currentNodePosition.x, currentNodePosition.y + 1));
        }

        // bottom
        if (currentNodePosition.y > 0)
        {
            neighbours.Add(getCell(currentNodePosition.x, currentNodePosition.y - 1));
        }

        return neighbours;
    }

    // finds the minimum depth (z value) from surrounding tiles
    public int getMinDepth(Cell currentNode)
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

    //set boundary cells for rectangular and square levels
    public void setBoundaryCells()
    {
        Vector2Int position = new Vector2Int(0, 0);

        boundaryCellPositions.Add(position);
        getCell(position).onBoundary = true;

        position = new Vector2Int(width - 1, 0);
        boundaryCellPositions.Add(position);
        getCell(position).onBoundary = true;

        position = new Vector2Int(0, height - 1);
        boundaryCellPositions.Add(position);
        getCell(position).onBoundary = true;

        position = new Vector2Int(width - 1, height - 1);
        boundaryCellPositions.Add(position);
        getCell(position).onBoundary = true;

        for (int x = 0; x < width; x += (width - 1))
        {
            for (int y = 1; y < height - 1; y++)
            {
                position = new Vector2Int(x, y);
                boundaryCellPositions.Add(position);
                getCell(position).onBoundary = true;
            }
        }
        for (int y = 0; y < height; y += (height - 1))
        {
            for (int x = 1; x < width - 1; x++)
            {
                position = new Vector2Int(x, y);
                boundaryCellPositions.Add(position);
                getCell(position).onBoundary = true;
            }
        }
    }

    public void checkForBoundaryCell(Cell cell)
    {
        List<Cell> neighbours = getNeighbours(cell);

        if (neighbours.Count < 4)
        {
            // its a boundary tile
            addBoundaryCellPosition((Vector2Int)cell.position);
        }
        else
        {
            int invalidTileCount = 0;
            int validTileCount = 0;

            foreach (Cell neighbour in neighbours)
            {
                switch (neighbour.status)
                {
                    case Cell.CellStatus.ValidCell:
                        validTileCount++;
                        break;
                    //invalid
                    default:
                        //if (closedSet.Contains(neighbour))
                        {
                            invalidTileCount++;
                        }
                        break;
                }
            }

            if (validTileCount > 0 && invalidTileCount > 0)
            {
                // its a boundary tile
                addBoundaryCellPosition((Vector2Int)cell.position);
            }
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
}
