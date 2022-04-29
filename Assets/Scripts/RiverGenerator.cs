using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;

public class RiverGenerator
{
    private Tilemap riverTilemap;

    Dictionary<TerrainGenerator.TerrainType, Tile> riverTilesByType;

    private readonly string waterTileName = "ISO_Tile_Water_01";
    private readonly string iceTileName = "ISO_Tile_Ice_01-06";
    private readonly string lavaTileName = "ISO_Tile_Lava_01";

    private int riverMaxCount;

    private List<Vector3Int>[] rivers;

    public enum NumberOfRivers { Low, Medium, High }

    public Cell[,] grid;

    RiverOptions.RiverSettings riverSettings;

    private const float rMultiplier = 0.0034f;

    int nodesearchcount;

    // river gen currently only for square and rectangular levels
    public RiverGenerator(Grid grid, SpriteAtlas atlas)
    {
        riverTilemap = new GameObject("River").AddComponent<Tilemap>();

        riverTilemap.gameObject.AddComponent<TilemapRenderer>();
        riverTilemap.transform.SetParent(grid.gameObject.transform);
        // move tile anchor from the button of the tile, to the front point of the tile (in the z)
        riverTilemap.tileAnchor = new Vector3(0, 0, -2);

        var terrainTilemapRenderer = riverTilemap.GetComponent<TilemapRenderer>();

        terrainTilemapRenderer.mode = TilemapRenderer.Mode.Individual;

        riverTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        Tile waterTile = ScriptableObject.CreateInstance<Tile>();
        waterTile.sprite = atlas.GetSprite(waterTileName);

        riverTilesByType.Add(TerrainGenerator.TerrainType.Greenery, waterTile);
        riverTilesByType.Add(TerrainGenerator.TerrainType.Dessert, waterTile);

        riverTilesByType.Add(TerrainGenerator.TerrainType.Lava, ScriptableObject.CreateInstance<Tile>());
        riverTilesByType[TerrainGenerator.TerrainType.Lava].sprite = atlas.GetSprite(lavaTileName);

        riverTilesByType.Add(TerrainGenerator.TerrainType.Snow, ScriptableObject.CreateInstance<Tile>());
        riverTilesByType[TerrainGenerator.TerrainType.Snow].sprite = atlas.GetSprite(iceTileName);
    }

    public void setRiverSettings(RiverOptions.RiverSettings riverSettings)
    {
        this.riverSettings = riverSettings;
    }

    public void populateCells(Cell[,] map, List<Vector3Int> terrainCellList, List<Vector2Int> boundaryCellList)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int levelArea = width * height;

        riverMaxCount = (int)Math.Ceiling(levelArea * (rMultiplier * ((int)riverSettings.rNum + 1)));

        rivers = new List<Vector3Int>[riverMaxCount];

        // List<int> boundaryXPositions = Enumerable.Range(0, width-1).ToList();
        // List<int> boundaryYPositions = Enumerable.Range(0, height-1).ToList();

        Cell startNode;
        Cell endNode;

        for (int riverCount = 0; riverCount < riverMaxCount; riverCount++)
        {
            startNode = getReachableCell(map,ref boundaryCellList);

            endNode = getReachableCell(map,ref boundaryCellList);

            rivers[riverCount] = findAStarPath(map, startNode, endNode);
        }

    }

    private Cell getReachableCell(Cell[,] map,ref List<Vector2Int> boundaryCellList)
    {
        Vector2Int boundaryCellXYPosition;
        int traversableNeighbourCount;

        while (true)
        {
            boundaryCellXYPosition = boundaryCellList[UnityEngine.Random.Range(0, boundaryCellList.Count - 1)];

            Cell cellToCheck = map[boundaryCellXYPosition.x, boundaryCellXYPosition.y];

            traversableNeighbourCount = 0;

            List<Cell> neighbours = getNeighbours(map, cellToCheck);

            foreach (Cell neighbour in neighbours)
            {
                if (neighbour.isTraversable)
                {
                    traversableNeighbourCount++;
                }
            }

            if (traversableNeighbourCount > 0)
            {
                map[boundaryCellXYPosition.x, boundaryCellXYPosition.y].isTraversable = true;
                map[boundaryCellXYPosition.x, boundaryCellXYPosition.y].status = Cell.CellStatus.OutOfBounds;
                boundaryCellList.Remove(boundaryCellXYPosition);
                // remove adjacent boundary cells from list, we dont want the possibility
                // of a 2 cell river
                foreach (Cell neighbour in neighbours)
                {
                    //no need to check contain first, will just return false if not in list
                   boundaryCellList.Remove((Vector2Int)neighbour.position);
                }
                return map[boundaryCellXYPosition.x, boundaryCellXYPosition.y];
            }

            boundaryCellList.Remove(boundaryCellXYPosition);
        }
    }

    private List<Vector3Int> findAStarPath(Cell[,] map, Cell startNode, Cell endNode)
    {
        nodesearchcount = 0;
        Heap<Cell> openList = new Heap<Cell>(map.Length);
        HashSet<Cell> closedList = new HashSet<Cell>();
        Cell currentNode;

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            nodesearchcount++;
            currentNode = openList.RemoveFirst();

            closedList.Add(currentNode);

            if (currentNode == endNode)
            {
                List<Vector3Int> path = new List<Vector3Int>();
                currentNode = endNode;

                Vector3Int position;
                while (currentNode != startNode)
                {
                    // change the level cells map
                    position = currentNode.position;
                    map[position.x, position.y].status = Cell.CellStatus.RiverCell;
                    map[position.x, position.y].position += Vector3Int.back;
                    path.Add(map[position.x, position.y].position);
                    currentNode = currentNode.parent;
                }
                // change the level cells map
                position = currentNode.position;
                map[position.x, position.y].status = Cell.CellStatus.RiverCell;
                map[position.x, position.y].position += Vector3Int.back;
                // add start node
                path.Add(map[position.x, position.y].position);

                path.Reverse();

                return path;
            }

            foreach (Cell neighbourNode in getNeighbours(map, currentNode))
            {

                if (!neighbourNode.isTraversable || closedList.Contains(neighbourNode))
                {
                    continue;
                }


                int newNeighbourGCost = currentNode.gCost + GetDistance(currentNode, neighbourNode);

                if (newNeighbourGCost < neighbourNode.gCost || !openList.Contains(neighbourNode))
                {

                    neighbourNode.gCost = newNeighbourGCost;

                    neighbourNode.hCost = GetDistance(neighbourNode, endNode);

                    neighbourNode.parent = currentNode;

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }
        return null;
    }

    private int GetDistance(Cell startNode, Cell endNode)
    {
        int xDistance = Mathf.Abs(startNode.position.x - endNode.position.x);
        int yDistance = Mathf.Abs(startNode.position.y - endNode.position.y);

        return 10 * (yDistance + xDistance);
    }

    private List<Cell> getNeighbours(Cell[,] grid, Cell currentNode)
    {
        List<Cell> neighbours = new List<Cell>();
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        Vector3Int currentNodePosition = currentNode.position;

        // left
        if(currentNodePosition.x > 0)
        {
            neighbours.Add(grid[currentNodePosition.x - 1, currentNodePosition.y]);
        }

        // right
        if(currentNodePosition.x < width - 1)
        {
            neighbours.Add(grid[currentNodePosition.x + 1, currentNodePosition.y]);
        }

        // top
        if (currentNodePosition.y < height - 1)
        {
            neighbours.Add(grid[currentNodePosition.x, currentNodePosition.y + 1]);
        }

        // bottom
        if (currentNodePosition.y > 0)
        {
            neighbours.Add(grid[currentNodePosition.x, currentNodePosition.y - 1]);
        }

        return neighbours;
    }

    public void clearTilemap()
    {
        riverTilemap.ClearAllTiles();
    }

    public void generate(Cell[,] map)
    {
        // set the array of positions and array of tiles from the level cells which are terrain
        // then populate the terrain tilemap with the tiles
        List<Vector3Int> positions = new List<Vector3Int>();
        List<TileBase> tiles = new List<TileBase>();

        foreach (List<Vector3Int> river in rivers)
        {

            foreach (Vector3Int riverPos in river)
            {
                Vector3Int newRiverPos = riverPos;
                positions.Add(newRiverPos);
                tiles.Add(riverTilesByType[riverSettings.tType]);
            }
        }

        riverTilemap.SetTiles(positions.ToArray(), tiles.ToArray());
    }

    public void randomlyGenerate()
    {

    }
}
