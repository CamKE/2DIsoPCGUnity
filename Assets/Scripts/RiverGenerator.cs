using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class RiverGenerator
{
    private Tilemap riverTilemap;

    Dictionary<TerrainGenerator.terrainType, Tile> riverTilesByType;

    private readonly string waterTileName = "ISO_Tile_Water_01";
    private readonly string iceTileName = "ISO_Tile_Water_01";
    private readonly string lavaTileName = "ISO_Tile_Lava_01-06";

    private int riverMaxCount;

    private List<Vector3Int>[] rivers;

    public enum numRivers { Low, Medium, High }

    public Node[,] grid;

    RiverOptions.RiverSettings riverSettings;

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

        riverTilesByType = new Dictionary<TerrainGenerator.terrainType, Tile>();

        Tile waterTile = ScriptableObject.CreateInstance<Tile>();
        waterTile.sprite = atlas.GetSprite(waterTileName);

        riverTilesByType.Add(TerrainGenerator.terrainType.Greenery, waterTile);
        riverTilesByType.Add(TerrainGenerator.terrainType.Dessert, waterTile);

        riverTilesByType.Add(TerrainGenerator.terrainType.Lava, ScriptableObject.CreateInstance<Tile>());
        riverTilesByType[TerrainGenerator.terrainType.Lava].sprite = atlas.GetSprite(lavaTileName);

        riverTilesByType.Add(TerrainGenerator.terrainType.Snow, ScriptableObject.CreateInstance<Tile>());
        riverTilesByType[TerrainGenerator.terrainType.Snow].sprite = atlas.GetSprite(iceTileName);
    }

    public void setRiverSettings(RiverOptions.RiverSettings riverSettings)
    {
        this.riverSettings = riverSettings;
    }

    public void populateCells(LevelManager.levelCellStatus[,,] levelCells, List<Vector3Int> terrainCellList, List<Vector2Int> boundaryCellList)
    {
        Debug.Log("River generation started");
        int levelArea = levelCells.GetLength(0) * levelCells.GetLength(1);
        Debug.Log($"level area is {levelArea}. Used to calc the max rivercount");


        riverMaxCount = (int)Math.Ceiling(levelArea * (0.01f * ((int)riverSettings.rNum + 1)));
        Debug.Log($"Max river count is {riverMaxCount}");

        Debug.Log("Creating the grid...");
        grid = createGrid(levelCells, terrainCellList);
        Debug.Log("grid created");

        rivers = new List<Vector3Int>[riverMaxCount];

        Debug.Log($"riverCount from 0 to riverMaxCount-1 ({riverMaxCount-1})...");
        for (int riverCount = 0; riverCount < riverMaxCount; riverCount++)
        {
            Debug.Log($"at riverCount {riverCount}");
            Debug.Log("get random boundary cells xy pos from the boundary cell xy list to set a start node");
            Vector2Int boundaryCellXYPosition = boundaryCellList[UnityEngine.Random.Range(0,boundaryCellList.Count-1)];
            Debug.Log($"random cell {boundaryCellXYPosition} chosen, remove it from the list");

            boundaryCellList.Remove(boundaryCellXYPosition);
            Debug.Log("set the start node to that boundary cell by using the xy from the list");
            Node startNode = grid[boundaryCellXYPosition.x, boundaryCellXYPosition.y];
            Debug.Log($"to confirm, {startNode.position} and {boundaryCellXYPosition} should have the same xy positions");

            Debug.Log("get random boundary cells xy pos from the boundary cell xy list to set an end node");
            boundaryCellXYPosition = boundaryCellList[UnityEngine.Random.Range(0, boundaryCellList.Count - 1)];
            Debug.Log($"random cell {boundaryCellXYPosition} chosen, remove it from the list");

            boundaryCellList.Remove(boundaryCellXYPosition);

            Debug.Log("set the end node to that boundary cell by using the xy from the list");
            Node endNode = grid[boundaryCellXYPosition.x, boundaryCellXYPosition.y];
            Debug.Log($"to confirm, {endNode.position} and {boundaryCellXYPosition} should have the same xy positions");

            Debug.Log($"find the shortest path between the start and end nodes: {startNode.position} and {endNode.position}");
            rivers[riverCount] = findAStarPath(grid, startNode, endNode, levelCells);
        }

    }

    private Node[,] createGrid(LevelManager.levelCellStatus[,,] levelCells, List<Vector3Int> terrainCellList)
    {

        Debug.Log($"New node grid of dimension {levelCells.GetLength(0)} by {levelCells.GetLength(0)}");
        Node[,] grid = new Node[levelCells.GetLength(0), levelCells.GetLength(1)];

        Debug.Log("foreach terraincell position in terrain cell list...");
        foreach (Vector3Int terrainCellPosition in terrainCellList)
        {
            Debug.Log($"position {terrainCellPosition} added to node grid");
            grid[terrainCellPosition.x, terrainCellPosition.y] = new Node(terrainCellPosition, true);
        }

        Debug.Log("return the grid");
        return grid;
    }
 
    private List<Vector3Int> findAStarPath(Node[,] grid, Node startNode, Node endNode, LevelManager.levelCellStatus[,,] levelCells)
    {
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        Node currentNode;

        Debug.Log("Add the start node to the openlist");
        openList.Add(startNode);

        Debug.Log("while the openlist is not empty...");
        while (openList.Count > 0)
        {
            Debug.Log($"set the current node to be the first node in the openlist: {openList[0]}");
            currentNode = openList[0];

            Debug.Log($"i from 1 to openList.Count-1 ({openList.Count - 1})...");
            for (int i = 1; i < openList.Count; i++)
            {
                Debug.Log($"if the f cost of node i from the open list is less than or equal to the f cost of the current node ({openList[i].fCost()} <= {currentNode.fCost()}?)...");
                if (openList[i].fCost() <= currentNode.fCost())
                {
                    Debug.Log($"if the above is true, then if the h cost of node i from the open list is less than the h cost of the current node ({openList[i].hCost} <= {currentNode.hCost}?)...");
                    if (openList[i].hCost < currentNode.hCost)
                    {
                        Debug.Log("if the above is true, set the current node to be the node at i in the openlist");
                        currentNode = openList[i];
                    }
                }
            }

            Debug.Log("remove the current node from the open list");
            openList.Remove(currentNode);
            Debug.Log("add the current node from the closed list");
            closedList.Add(currentNode);

            Debug.Log($"if the current node is equal to the end node ({currentNode == endNode})");
            if (currentNode == endNode)
            {
                Debug.Log("if the above is true, set the current node to be the end node");
                List<Vector3Int> path = new List<Vector3Int>();
                currentNode = endNode;

                Debug.Log("trace the path backwards");

                Debug.Log("while the current node is not equal to the end node...");
                Vector3Int position;
                while (currentNode != startNode)
                {
                    Debug.Log($"add the current node position {currentNode.position} to the path");
                    position = currentNode.position;
                    path.Add(currentNode.position);
                    levelCells[position.x, position.y, position.z] = LevelManager.levelCellStatus.riverCell;
                    Debug.Log("set the parent of the current node to be the current node");
                    currentNode = currentNode.parent;
                }

                // add the start node
                path.Add(currentNode.position);
                // change the level cells map
                position = currentNode.position;
                levelCells[position.x, position.y, position.z] = LevelManager.levelCellStatus.riverCell;


                Debug.Log("entire path traced backwards, now reverse it");

                path.Reverse();

                return path;
            }

            Debug.Log($"for each neighbour of the current node: {currentNode.position}...");
            foreach (Node neighbourNode in getNeighbours(grid, currentNode))
            {
                Debug.Log($"At neighbour: {neighbourNode.position}");

                Debug.Log($"if the neighbou is not traversable: {!neighbourNode.isTraversable} or the closed list already has neighbour {closedList.Contains(neighbourNode)}");
                if (!neighbourNode.isTraversable || closedList.Contains(neighbourNode))
                {
                    Debug.Log("move onto the next neighbour");
                    continue;
                }

                int newNeighbourGCost = currentNode.gCost + GetDistance(currentNode,neighbourNode);

                Debug.Log($"otherwise set neighbours new gCost to current node g {currentNode.gCost} plus distance between " +
                    $"current and neighbour node {GetDistance(currentNode, neighbourNode)} which equals{newNeighbourGCost}");

                Debug.Log($"if the new neighbour G cost is less than the current node g cost ({newNeighbourGCost < currentNode.gCost }) or openlist does not contain neighbour ({!openList.Contains(neighbourNode)})");

                if (newNeighbourGCost < currentNode.gCost || !openList.Contains(neighbourNode))
                {

                    neighbourNode.gCost = newNeighbourGCost;
                    Debug.Log("set the neighbour g cost to their new g cost");

                    neighbourNode.hCost = GetDistance(neighbourNode, endNode);
                    Debug.Log($"set the neighbours distance from the end node (hcost): {neighbourNode.hCost}");

                    Debug.Log("set the parent of the neighbour to be the current node");
                    neighbourNode.parent = currentNode;

                    Debug.Log("if openlist does not contain neighbour ({!openList.Contains(neighbourNode)})");
                    if (!openList.Contains(neighbourNode))
                        Debug.Log("add neighbour to openlist");
                        openList.Add(neighbourNode);
                }
            }
            Debug.Log("finished with current node, onto the next node in the openlist");
        }
        return null;
    }

    private int GetDistance(Node startNode, Node endNode)
    {
        int xDistance = Mathf.Abs(startNode.position.x - endNode.position.x);
        int yDistance = Mathf.Abs(startNode.position.y - endNode.position.y);

        return 10 * xDistance + 10 * (yDistance - xDistance);
    }

    private List<Node> getNeighbours(Node[,] grid, Node currentNode)
    {
        List<Node> neighbours = new List<Node>();
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if ((i == 0 && j == 0))
                {
                    continue;
                }

                if (i == 0 || j == 0)
                {
                    int xPos = currentNode.position.x + i;
                    int yPos = currentNode.position.y + j;

                    if (xPos >= 0 && xPos < width && yPos >= 0 && yPos < height)
                    {
                        neighbours.Add(grid[xPos, yPos]);
                    }
                }
            }
        }
        return neighbours;
    }

    public void clearTilemap()
    {
        riverTilemap.ClearAllTiles();
    }

    public void generate(LevelManager.levelCellStatus[,,] levelCells)
    {
        // set the array of positions and array of tiles from the level cells which are terrain
        // then populate the terrain tilemap with the tiles
        List<Vector3Int> positions = new List<Vector3Int>();
        List<TileBase> tiles = new List<TileBase>();

        foreach (List<Vector3Int> river in rivers)
        {

            foreach (Vector3Int riverPos in river)
            {
                Vector3Int newRiverPos = riverPos + Vector3Int.back;
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
