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
        riverTilemap.gameObject.AddComponent<TilemapCollider2D>();
        riverTilemap.gameObject.AddComponent<Rigidbody2D>();
        riverTilemap.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        riverTilemap.GetComponent<TilemapCollider2D>().offset = new Vector2(0, 1.125f);

        var terrainTilemapRenderer = riverTilemap.GetComponent<TilemapRenderer>();

        terrainTilemapRenderer.mode = TilemapRenderer.Mode.Individual;

        riverTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        Tile waterTile = ScriptableObject.CreateInstance<Tile>();
        waterTile.sprite = atlas.GetSprite(waterTileName);
        waterTile.colliderType = Tile.ColliderType.Grid;


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

        Cell[] cellPair;

        for (int riverCount = 0; riverCount < riverMaxCount; riverCount++)
        {

            cellPair = getReachableCells(map, ref boundaryCellList, riverCount);

            rivers[riverCount] = findAStarPath(map, cellPair[0], cellPair[1]);
        }

    }

    private Cell[] getReachableCells(Cell[,] map,ref List<Vector2Int> boundaryCellList, int riverCount)
    {
        Vector2Int boundaryCellXYPosition;
        int traversableNeighbourCount;

       Cell[] cellPair = new Cell[2];
        bool riverNodesFound = false;

        List<Vector2Int> boundaryCellListClone;

        while (!riverNodesFound)
        {
            boundaryCellListClone = new List<Vector2Int>(boundaryCellList);

            for (int x = 0; x < 2; x++)
            {
                while (true)
                {
                    boundaryCellXYPosition = boundaryCellListClone[UnityEngine.Random.Range(0, boundaryCellListClone.Count - 1)];

                    Debug.Log(boundaryCellXYPosition);

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
                        //Debug.Log(boundaryCellList.Count);

                        map[boundaryCellXYPosition.x, boundaryCellXYPosition.y].isTraversable = true;
                        boundaryCellListClone.Remove(boundaryCellXYPosition);
                        // remove adjacent boundary cells from list, we dont want the possibility
                        // of a 2 cell river

                        foreach (Cell neighbour in getNeighbours(map, map[boundaryCellXYPosition.x, boundaryCellXYPosition.y]))
                        {
                            //no need to check contain first, will just return false if not in list
                            boundaryCellListClone.Remove((Vector2Int)neighbour.position);
                        }

                        cellPair[x] = map[boundaryCellXYPosition.x, boundaryCellXYPosition.y];
                        break;
                    }

                    boundaryCellListClone.Remove(boundaryCellXYPosition);
                }
            }



            riverNodesFound = true;

            for (int x = 0; x < riverCount; x++)
            {
                bool intersects = lineSegmentsIntersect(cellPair[0].position, cellPair[1].position, rivers[x].First(), rivers[x].Last());
                if (intersects)
                {
                    riverNodesFound = false;
                    Debug.Log("there is an intersection, pick another pair of nodes");
                    break;
                }
            }
        }


        foreach (Cell cell in cellPair)
        {
            Vector2Int cellPosition = (Vector2Int)cell.position;
            map[cellPosition.x, cellPosition.y].isTraversable = true;

            boundaryCellList.Remove(cellPosition);
            // remove adjacent boundary cells from list, we dont want the possibility
            // of a 2 cell river
            foreach (Cell neighbour in getNeighbours(map, cell))
            {
                //no need to check contain first, will just return false if not in list
                boundaryCellList.Remove((Vector2Int)neighbour.position);
            }
        }

        return cellPair;
    }

    public static bool lineSegmentsIntersect(Vector3 lineOneA, Vector3 lineOneB, Vector3 lineTwoA, Vector3 lineTwoB) { return (((lineTwoB.y - lineOneA.y) * (lineTwoA.x - lineOneA.x) > (lineTwoA.y - lineOneA.y) * (lineTwoB.x - lineOneA.x)) != ((lineTwoB.y - lineOneB.y) * (lineTwoA.x - lineOneB.x) > (lineTwoA.y - lineOneB.y) * (lineTwoB.x - lineOneB.x)) && ((lineTwoA.y - lineOneA.y) * (lineOneB.x - lineOneA.x) > (lineOneB.y - lineOneA.y) * (lineTwoA.x - lineOneA.x)) != ((lineTwoB.y - lineOneA.y) * (lineOneB.x - lineOneA.x) > (lineOneB.y - lineOneA.y) * (lineTwoB.x - lineOneA.x))); }

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
                    map[position.x, position.y].isTraversable = false;
                    path.Add(map[position.x, position.y].position);
                    currentNode = currentNode.parent;
                }
                // change the level cells map
                position = currentNode.position;
                map[position.x, position.y].status = Cell.CellStatus.RiverCell;
                map[position.x, position.y].isTraversable = false;
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

    // work out what the z value of the river tile should be based on its
    // surrounding terrain tiles
    private int getMaxDepth(Cell[,] map, Cell currentNode)
    {
        int maxRiverDepth = currentNode.position.z;

        foreach (Cell neighbour in getNeighbours(map, currentNode))
        {
            if (neighbour.status == Cell.CellStatus.TerrainCell)
            {
                int neighbourDepth = neighbour.position.z;
                maxRiverDepth = neighbourDepth < maxRiverDepth ? neighbourDepth : maxRiverDepth;
            }
        }

        return maxRiverDepth - 1;
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

        foreach (Cell cell in map)
        {
            if (cell.status == Cell.CellStatus.RiverCell)
            {
                cell.position.z = getMaxDepth(map, cell);
                positions.Add(cell.position);
                tiles.Add(riverTilesByType[riverSettings.tType]);
            }
        }
        riverTilemap.SetTiles(positions.ToArray(), tiles.ToArray());
    }

    public void randomlyGenerate()
    {

    }
}
