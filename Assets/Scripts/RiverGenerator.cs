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
    Dictionary<TerrainGenerator.TerrainType, Tile> riverTilesByType;

    private readonly string waterTileName = "ISO_Tile_Water_01";
    private readonly string iceTileName = "ISO_Tile_Ice_01-06";
    private readonly string lavaTileName = "ISO_Tile_Lava_01";

    private int riverMaxCount;

    public enum NumberOfRivers { Low, Medium, High }

    public Cell[,] grid;

    RiverOptions.RiverSettings riverSettings;

    private const float rMultiplier = 0.0034f;

    // river gen currently only for square and rectangular levels
    public RiverGenerator(SpriteAtlas atlas)
    {
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

    public Tile getTile()
    {
        return riverTilesByType[riverSettings.tType];
    }

    public void populateCells(Cell[,] map, List<Vector3Int> terrainCellList, List<Vector2Int> boundaryCellList)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int levelArea = width * height;

        riverMaxCount = (int)Math.Ceiling(levelArea * (rMultiplier * ((int)riverSettings.rNum + 1)));

        Heap<CellPair> cellPairs = new Heap<CellPair>(riverMaxCount);

        for (int riverCount = 0; riverCount < riverMaxCount; riverCount++)
        {
            CellPair pair = getReachableCells(map, ref boundaryCellList, riverCount, cellPairs, riverSettings.intersectionsEnabled);

            if (pair == null)
            {
                break;
            }

            cellPairs.Add(pair);

        }

        while (cellPairs.Count > 0)
        {
            CellPair cellPair = cellPairs.RemoveFirst();
            cellPair.startCell.isTraversable = true;
            cellPair.endCell.isTraversable = true;

            bool done = findAStarPath(map, cellPair.startCell, cellPair.endCell, riverSettings.intersectionsEnabled);
        }
    }


    private CellPair getReachableCells(Cell[,] map,ref List<Vector2Int> boundaryCellList, int riverCount, Heap<CellPair> cellPairs, bool intersectionsEnabled)
    {
        Vector2Int boundaryCellXYPosition;
        int traversableNeighbourCount;

       Cell[] cellPair = new Cell[2];
        bool riverNodesFound = false;

        List<Vector2Int> boundaryCellListClone;
        int searchCount = 0;

        // limit search count to prevent infinite searching for non-existent valid cell pair  (temporary solution)
        while (!riverNodesFound && searchCount < 1000)
        {

            boundaryCellListClone = new List<Vector2Int>(boundaryCellList);

            for (int x = 0; x < 2; x++)
            {
                while (true)
                {

                    boundaryCellXYPosition = boundaryCellListClone[UnityEngine.Random.Range(0, boundaryCellListClone.Count - 1)];

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
                //bool intersects = lineSegmentsIntersect(cellPair[0].position, cellPair[1].position, rivers[x].First(), rivers[x].Last());
                if (!intersectionsEnabled && doIntersect(cellPair[0].position, cellPair[1].position, cellPairs.getItem(x).startCell.position, cellPairs.getItem(x).endCell.position))
                {
                    riverNodesFound = false;
                    //Debug.Log("there is an intersection, pick another pair of nodes");
                    break;
                }
            }

            searchCount++;
        }

        if (searchCount == 1000)
        {
            return null;
        } else
        {
            foreach (Cell cell in cellPair)
            {
                Vector2Int cellPosition = (Vector2Int)cell.position;

                boundaryCellList.Remove(cellPosition);
                // remove adjacent boundary cells from list, we dont want the possibility
                // of a 2 cell river
                foreach (Cell neighbour in getNeighbours(map, cell))
                {
                    //no need to check contain first, will just return false if not in list
                    boundaryCellList.Remove((Vector2Int)neighbour.position);
                }
            }

            return new CellPair(cellPair[0], cellPair[1], GetDistance(cellPair[0], cellPair[1]));
        }

    }

    // The main function that returns true if line segment 'p1q1'
    // and 'p2q2' intersect.
    static Boolean doIntersect(Vector3Int p1, Vector3Int q1, Vector3Int p2, Vector3Int q2)
    {
        // Find the four orientations needed for general and
        // special cases
        int o1 = orientation(p1, q1, p2);
        int o2 = orientation(p1, q1, q2);
        int o3 = orientation(p2, q2, p1);
        int o4 = orientation(p2, q2, q1);

        // General case
        if (o1 != o2 && o3 != o4)
            return true;

        // Special Cases
        // p1, q1 and p2 are collinear and p2 lies on segment p1q1
        if (o1 == 0 && onSegment(p1, p2, q1)) return true;

        // p1, q1 and q2 are collinear and q2 lies on segment p1q1
        if (o2 == 0 && onSegment(p1, q2, q1)) return true;

        // p2, q2 and p1 are collinear and p1 lies on segment p2q2
        if (o3 == 0 && onSegment(p2, p1, q2)) return true;

        // p2, q2 and q1 are collinear and q1 lies on segment p2q2
        if (o4 == 0 && onSegment(p2, q1, q2)) return true;

        return false; // Doesn't fall in any of the above cases
    }

    // Given three collinear points p, q, r, the function checks if
    // point q lies on line segment 'pr'
    static Boolean onSegment(Vector3Int p, Vector3Int q, Vector3Int r)
    {
        if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
            q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
            return true;

        return false;
    }

    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    static int orientation(Vector3Int p, Vector3Int q, Vector3Int r)
    {
        // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
        // for details of below formula.
        int val = (q.y - p.y) * (r.x - q.x) -
                (q.x - p.x) * (r.y - q.y);

        if (val == 0) return 0; // collinear

        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }

    private bool findAStarPath(Cell[,] map, Cell startNode, Cell endNode, bool intersectionsEnabled)
    {
        Heap<Cell> openList = new Heap<Cell>(map.Length);
        HashSet<Cell> closedList = new HashSet<Cell>();
        Cell currentNode;

        openList.Add(startNode);

        while (openList.Count > 0)
        {
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

                    if (!intersectionsEnabled)
                    {
                        map[position.x, position.y].isTraversable = false;
                        foreach (Cell neighbour in getNeighbours(map, currentNode))
                        {
                            map[neighbour.position.x, neighbour.position.y].isTraversable = false;
                        }
                    }
                    currentNode = currentNode.parent;
                }
                // change the level cells map
                position = currentNode.position;
                map[position.x, position.y].status = Cell.CellStatus.RiverCell;

                if (!intersectionsEnabled)
                {
                    map[position.x, position.y].isTraversable = false;
                    foreach (Cell neighbour in getNeighbours(map, currentNode))
                    {
                        map[neighbour.position.x, neighbour.position.y].isTraversable = false;
                    }
                }
                return true;
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
        return false;
    }

    // work out what the z value of the river tile should be based on its
    // surrounding terrain tiles
    public int getMaxDepth(Cell[,] map, Cell currentNode)
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

    public void randomlyGenerate()
    {

    }
}
