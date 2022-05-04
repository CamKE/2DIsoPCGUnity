using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map
{
    private Cell[,] map;

    private List<Vector2Int> boundaryCellPositions;

    private List<Vector2Int>[] boundaryCellSides;

    public enum BoundaryCellSide { Up, Down, Left, Right }

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

        int length = Enum.GetValues(typeof(BoundaryCellSide)).Length;
        boundaryCellSides = new List<Vector2Int>[length];

        for (int x = 0; x < length; x++)
        {
            boundaryCellSides[x] = new List<Vector2Int>();
        }
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
            Cell cell = getCell(currentNodePosition.x - 1, currentNodePosition.y);
            cell.direction = BoundaryCellSide.Left;
            neighbours.Add(cell);

        }

        // right
        if (currentNodePosition.x < width - 1)
        {
            Cell cell = getCell(currentNodePosition.x + 1, currentNodePosition.y);
            cell.direction = BoundaryCellSide.Right;
            neighbours.Add(cell);
        }

        // top
        if (currentNodePosition.y < height - 1)
        {
            Cell cell = getCell(currentNodePosition.x, currentNodePosition.y + 1);
            cell.direction = BoundaryCellSide.Up;
            neighbours.Add(cell);
        }

        // bottom
        if (currentNodePosition.y > 0)
        {
            Cell cell = getCell(currentNodePosition.x, currentNodePosition.y - 1);
            cell.direction = BoundaryCellSide.Down;
            neighbours.Add(cell);
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
        List<BoundaryCellSide> sides = Enum.GetValues(typeof(BoundaryCellSide)).Cast<BoundaryCellSide>().ToList();

        if (neighbours.Count < 4)
        {
            // remove this if want to revert
            foreach (Cell neighbour in neighbours)
            {
                sides.Remove(neighbour.direction);
            }
            // its a boundary tile
            addBoundaryCellPosition((Vector2Int)cell.position);
            // remove this if want to revert
            addBoundaryCellPosition((Vector2Int)cell.position, sides.First());
        }

    }


    public void addBoundaryCellPosition(Vector2Int position)
    {
        boundaryCellPositions.Add(position);
        getCell(position).onBoundary = true;
    }

    // remove this if want to revert
    public void addBoundaryCellPosition(Vector2Int position, BoundaryCellSide side)
    {
        boundaryCellSides[(int)side].Add(position);
        getCell(position).onBoundary = true;
    }


    public List<Vector2Int> getBoundaryCellPositions()
    {
        return boundaryCellPositions;
    }
}
