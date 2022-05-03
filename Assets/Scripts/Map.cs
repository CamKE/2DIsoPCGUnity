using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    private Cell[,] map;

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
    }

    public void updateCellStatus(Vector2Int cellPosition, Cell.CellStatus status, bool intersectionsEnabled = false)
    {
        Cell currentCell = map[cellPosition.x, cellPosition.y];

        switch (status)
        {
            case Cell.CellStatus.TerrainCell:
                if (currentCell.status == Cell.CellStatus.TerrainCell)
                {
                    return;
                }
                terrainCellCount++;
                break;
            default:
                if (currentCell.status == Cell.CellStatus.TerrainCell)
                {
                    terrainCellCount--;
                }
                break;
        }

        currentCell.setCellStatus(status, intersectionsEnabled);
        //foreach get neighbours here
    }

    public void updateCellStatus(int x, int y , Cell.CellStatus status, bool intersectionsEnabled = false)
    {
        Cell currentCell = map[x, y];

        switch (status)
        {
            case Cell.CellStatus.TerrainCell:
                if (currentCell.status == Cell.CellStatus.TerrainCell)
                {
                    return;
                }
                terrainCellCount++;
                break;
            default:
                if (currentCell.status == Cell.CellStatus.TerrainCell)
                {
                    terrainCellCount--;
                }
                break;
        }

        currentCell.setCellStatus(status, intersectionsEnabled);
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
}
