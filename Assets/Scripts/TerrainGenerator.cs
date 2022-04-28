using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

/// <summary>
/// Responsible for generating the terrain. Used by the Level Manager.
/// </summary>
public class TerrainGenerator
{
    /// <summary>
    /// The terrain shape options.
    /// </summary>
    public enum TerrainShape { Square, Rectangle, Random };

    /// <summary>
    /// The terrain type options.
    /// </summary>
    public enum TerrainType { Greenery, Dessert, Snow, Lava };

    /// <summary>
    /// The minimum size of a level specified by tile count.
    /// </summary>
    public const int terrainMinSize = 50;

    /// <summary>
    /// The maximum size of a level specified by tile count.
    /// </summary>
    public const int terrainMaxSize = 1500;

    public const int terrainMinHeight = 0;

    public const int terrainMaxHeight = 5;

    // determines the resolution at which we sample the perlin noise
    // set such that the variation in values returned is gradual
    private const float perlinScale = 3.0f;

    public Tilemap terrainTilemap;
    public Tilemap terrainTilemapOuterBound;
    public Tilemap terrainTilemap2;

    private readonly string[] greeneryGroundTileNames = { "ISO_Tile_Dirt_01_Grass_01", "ISO_Tile_Dirt_01_Grass_02" };

    private readonly string[] greeneryAccessoryTileNames = {"ISO_Tile_Dirt_01_GrassPatch_01",
        "ISO_Tile_Dirt_01_GrassPatch_02", "ISO_Tile_Dirt_01_GrassPatch_03"};

    private readonly string[] desertGroundTileNames = { "ISO_Tile_Sand_01", "ISO_Tile_Sand_02", "ISO_Tile_Sand_03", "ISO_Tile_Sand_04" };

    private readonly string[] desertAccessoryTileNames = {"ISO_Tile_Sand_01_ToStone_01",
        "ISO_Tile_Sand_01_ToStone_02", "ISO_Tile_Sand_02_ToStone_01", "ISO_Tile_Sand_02_ToStone_02", "ISO_Tile_Sand_03_ToStone_01", "ISO_Tile_Sand_03_ToStone_02"};

    private readonly string[] snowGroundTileNames = { "ISO_Tile_Snow_01", "ISO_Tile_Snow_02" };

    private readonly string[] snowAccessoryTileNames = {"ISO_Tile_Snow_01_ToStone_01",
        "ISO_Tile_Snow_01_ToStone_02", "ISO_Tile_Snow_02_ToStone_01", "ISO_Tile_Snow_02_ToStone_02"};

    private readonly string[] lavaGroundTileNames = { "ISO_Tile_LavaStone_01", "ISO_Tile_Tar_01" };

    private readonly string[] lavaAccessoryTileNames = { "ISO_Tile_LavaCracks_01", "ISO_Tile_LavaCracks_01 1" };

    Tile test;

    Dictionary<TerrainType, terrainTiles> terrainTilesByType;

    // temporary solution
    public List<Vector3Int> terrainCellList;

    public List<Vector2Int> boundaryCellList;

    struct terrainTiles
    {
        public Tile[] groundTiles;
        public Tile[] accessoryTiles;

        public terrainTiles(string[] groundTileNames, string[] accessoryTileNames, SpriteAtlas atlas)
        {
            groundTiles = new Tile[groundTileNames.Length];
            accessoryTiles = new Tile[accessoryTileNames.Length];

            setTiles(groundTileNames, groundTiles, atlas);
            setTiles(accessoryTileNames, accessoryTiles, atlas);
        }

        private void setTiles(string[] names, Tile[] tiles, SpriteAtlas atlas)
        {
            for (int x = 0; x < tiles.Length; x++)
            {
                tiles[x] = ScriptableObject.CreateInstance<Tile>();
                tiles[x].sprite = atlas.GetSprite(names[x]);
            }
        }
    }

    TerrainOptions.TerrainSettings terrainSettings;

    public TerrainGenerator(Grid grid, SpriteAtlas atlas)
    {
        test = ScriptableObject.CreateInstance<Tile>();
        test.sprite = atlas.GetSprite("ISO_Tile_Flesh_01");
        test.colliderType = Tile.ColliderType.Grid;

        terrainTilemap = setupTilemap(grid, "Terrain");

        terrainTilemapOuterBound = setupTilemap(grid, "TerrainOuterBound");
        terrainTilemapOuterBound.gameObject.AddComponent<TilemapCollider2D>();
        terrainTilemapOuterBound.GetComponent<TilemapCollider2D>().offset = new Vector2(0, 0.875f);

        terrainTilemap2 = setupTilemap(grid, "TerrainTestmap");

        terrainTilesByType = new Dictionary<TerrainType, terrainTiles>();

        terrainTiles greeneryTiles = new terrainTiles(greeneryGroundTileNames, greeneryAccessoryTileNames, atlas);
        terrainTilesByType.Add(TerrainType.Greenery, greeneryTiles);

        terrainTiles dessertTiles = new terrainTiles(desertGroundTileNames, desertAccessoryTileNames, atlas);
        terrainTilesByType.Add(TerrainType.Dessert, dessertTiles);

        terrainTiles snowTiles = new terrainTiles(snowGroundTileNames, snowAccessoryTileNames, atlas);
        terrainTilesByType.Add(TerrainType.Snow, snowTiles);

        terrainTiles lavaTiles = new terrainTiles(lavaGroundTileNames, lavaAccessoryTileNames, atlas);
        terrainTilesByType.Add(TerrainType.Lava, lavaTiles);

    }

    private Tilemap setupTilemap(Grid grid, string name)
    {
        Tilemap tilemap = new GameObject(name).AddComponent<Tilemap>();

        tilemap.gameObject.AddComponent<TilemapRenderer>();
        tilemap.transform.SetParent(grid.gameObject.transform);
        // move tile anchor from the button of the tile, to the front point of the tile (in the z)
        tilemap.tileAnchor = new Vector3(0, 0, -2);

        var terrainTilemapRenderer = tilemap.GetComponent<TilemapRenderer>();

        terrainTilemapRenderer.mode = TilemapRenderer.Mode.Individual;

        return tilemap;
    }

    public BoundsInt getTilemapBounds()
    {
        return terrainTilemap.cellBounds;
    }

    public void setTerrainSettings(TerrainOptions.TerrainSettings terrainSettings)
    {
        this.terrainSettings = terrainSettings;
    }

    public void populateCells(Cell[,] map)
    {
        // define all the terrain cells

        // get width and height of the array
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        terrainCellList = new List<Vector3Int>();

        if (terrainSettings.heightRangedEnabled)
        {
            setCellsRange(map, width, height, terrainSettings.tMinHeight, terrainSettings.tMaxHeight); ;
        }
        else
        {
            setCellsExact(map, width, height, terrainSettings.tExactHeight);
        }

        setOuterBounds(map);
    }

    public Cell[,] createMap()
    {
        Vector2Int mapDimensions = Vector2Int.zero;
        Cell[,] map;
        boundaryCellList = new List<Vector2Int>();

        // check the terrain shape chosen
        switch (terrainSettings.tShape)
        {
            // for rectangular shape
            case TerrainGenerator.TerrainShape.Rectangle:
                // 2:1, 3:1, or 4:1 ratio
                mapDimensions = getDimensions(terrainSettings.tSize, UnityEngine.Random.Range(2, 4));
                setBoundaryCells(mapDimensions);
                map = new Cell[mapDimensions.x, mapDimensions.y];
                for (int x = 0; x < mapDimensions.x; x++)
                {
                    for (int y = 0; y < mapDimensions.y; y++)
                    {
                        map[x, y] = new Cell(new Vector3Int(x, y, 0), true);
                    }
                }
                break;
            // for random shape
            case TerrainGenerator.TerrainShape.Random:
                // generate a random shape
                // possibly return some other 2d structure that can grow like a list
                // convert the 2d list of levelcellstatus to a 3d array 
                map = createRandomLevelShapeBFS(terrainSettings.tSize);

                break;
            // default shape is square 
            default:
                mapDimensions = getDimensions(terrainSettings.tSize);

                setBoundaryCells(mapDimensions);

                map = new Cell[mapDimensions.x, mapDimensions.y];

                for (int x = 0; x < mapDimensions.x; x++)
                {
                    for (int y = 0; y < mapDimensions.y; y++)
                    {
                        map[x, y] = new Cell(new Vector3Int(x, y, 0), true);
                    }
                }
                break;
        }

        return map;
    }

    //set boundary cells for rectangular and square levels
    private void setBoundaryCells(Vector2Int levelCells2DDimensions)
    {
        int width = levelCells2DDimensions.x;
        int height = levelCells2DDimensions.y;

        boundaryCellList.Add(new Vector2Int(0, 0));
        boundaryCellList.Add(new Vector2Int(width - 1, 0));
        boundaryCellList.Add(new Vector2Int(0, height - 1));
        boundaryCellList.Add(new Vector2Int(width - 1, height - 1));

        for (int x = 0; x < width; x += (width - 1))
        {
            for (int y = 1; y < height - 1; y++)
            {
                boundaryCellList.Add(new Vector2Int(x, y));
            }
        }
        for (int y = 0; y < height; y += (height - 1))
        {
            for (int x = 1; x < width - 1; x++)
            {
                boundaryCellList.Add(new Vector2Int(x, y));
            }
        }
    }

    private Cell[,] createRandomLevelShapeBFS(int terrainSize)
    {
        // get the square size.
        Vector2Int dimensions = getDimensions(terrainSize);

        int terrainLength = dimensions.x;

        Cell[,] newLevelShape = new Cell[terrainLength, terrainLength];

        // set all cells to be invalid initially
        for (int x = 0; x < terrainLength; x++)
        {
            for (int y = 0; y < terrainLength; y++)
            {
                newLevelShape[x, y] = new Cell(new Vector3Int(x, y, 0), false);
                newLevelShape[x, y].setCellStatus(Cell.CellStatus.InvalidCell);
            }
        }

        Vector2Int centerCell = new Vector2Int(terrainLength / 2, terrainLength / 2);

        newLevelShape[centerCell.x, centerCell.y].setCellStatus(Cell.CellStatus.ValidCell);

        Queue<Vector2Int> openSetPositions = new Queue<Vector2Int>();
        HashSet<Vector2Int> closedSetPositions = new HashSet<Vector2Int>();

        foreach (Vector2Int neighbour in getNeighbours(centerCell, dimensions))
        {
            openSetPositions.Enqueue(neighbour);
        }

        closedSetPositions.Add(centerCell);

        Vector2Int currentCellPosition;

        float validTileChance = 1.0f;
        // the rate at which each additional tile reduces the valid tile chance 
        float chanceFalloffRate = 0.1f;

        while (openSetPositions.Count > 0)
        {
            currentCellPosition = openSetPositions.Dequeue();

            List<Vector2Int> neighbours = getNeighbours(currentCellPosition, dimensions);
            int invalidTileCount = 0;
            int validTileCount = 0;

            foreach (Vector2Int neighbour in neighbours)
            {
                switch (newLevelShape[neighbour.x, neighbour.y].status)
                {
                    case Cell.CellStatus.ValidCell:
                        validTileCount++;
                        break;
                    //invalid
                    default:
                        if (closedSetPositions.Contains(neighbour))
                        {
                            invalidTileCount++;
                        }
                        break;
                }
            }

            //make valid based on random prob
            if (UnityEngine.Random.value < validTileChance && (validTileCount > 0 && invalidTileCount == 0 ))                                                                                                                                                                                                                                                                                                                                                   
            {
                newLevelShape[currentCellPosition.x, currentCellPosition.y].setCellStatus(Cell.CellStatus.ValidCell);
                validTileChance -= (chanceFalloffRate / (float)terrainSize);

            }


            if (newLevelShape[currentCellPosition.x, currentCellPosition.y].status == Cell.CellStatus.ValidCell)
            {

                foreach (Vector2Int neighbour in neighbours)
                {
                    if (!openSetPositions.Contains(neighbour) && !closedSetPositions.Contains(neighbour))
                    {
                        openSetPositions.Enqueue(neighbour);
                    }
                }

            }

            closedSetPositions.Add(currentCellPosition);

        }

        // set the boundary cells
        foreach (Cell cell in newLevelShape)
        {
            if (cell.status == Cell.CellStatus.ValidCell)
            {
                int invalidTileCount = 0;
                int validTileCount = 0;
                int neighbourCount = 0;

                foreach (Vector2Int neighbour in getNeighbours((Vector2Int)cell.position, dimensions))
                {

                    switch (newLevelShape[neighbour.x, neighbour.y].status)
                    {
                        case Cell.CellStatus.ValidCell:
                            validTileCount++;
                            break;
                        //invalid
                        default:
                            if (closedSetPositions.Contains(neighbour))
                            {
                                invalidTileCount++;
                            }
                            break;
                    }
                    neighbourCount++;
                }
                if (validTileCount > 0 && invalidTileCount > 0 || neighbourCount < 4)
                {
                    // its a boundary tile
                    boundaryCellList.Add((Vector2Int)cell.position);
                }
            }
        }

        return newLevelShape;
    }

    private List<Vector2Int> getNeighbours(Vector2Int currentPosition, Vector2Int mapDimensions)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();

        // left
        if (currentPosition.x > 0)
        {
            neighbours.Add(currentPosition + Vector2Int.left);
        }

        // right
        if (currentPosition.x < mapDimensions.x - 1)
        {
            neighbours.Add(currentPosition + Vector2Int.right);
        }

        // top
        if (currentPosition.y < mapDimensions.y - 1)
        {
            neighbours.Add(currentPosition + Vector2Int.up);
        }

        // bottom
        if (currentPosition.y > 0)
        {
            neighbours.Add(currentPosition + Vector2Int.down);
        }

        return neighbours;
    }

    // calculates the square for 1:1 length to width ratio.
    private Vector2Int getDimensions(int terrainSize, int ratio = 1)
    {

        double rawLength = Math.Sqrt((float)terrainSize / ratio);
        int length = (int)Math.Floor(rawLength);
        //tminsize wrong, need user defined one
        if ((length * length * ratio) < TerrainGenerator.terrainMinSize)
        {
            length = (int)Math.Ceiling(rawLength);
        }

        // random orientation of width and height
        return UnityEngine.Random.value > 0.5f ? new Vector2Int(length, length * ratio) : new Vector2Int(length * ratio, length);
    }

    private void setCellsRange(Cell[,] map, int mapWidth, int mapHeight, int minCellDepth, int maxCellDepth)
    {
        Vector2 perlinOffset = new Vector2(UnityEngine.Random.Range(0.0f, 999.0f), UnityEngine.Random.Range(0.0f, 999.0f));

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // check in the 2d z plane if we are able to put a tile on the cell
                // all tiles in the y plane at x,0 will also be valid cells
                if (map[x, y].status == Cell.CellStatus.ValidCell)
                {
                    // work out which cell in the z plane should be populated
                    int zValue = calculateDepth(x, y, mapWidth, mapHeight, minCellDepth, maxCellDepth, perlinOffset);

                    // mark the cell as a terrain cell
                    map[x, y].status = Cell.CellStatus.TerrainCell;
                    map[x, y].position = new Vector3Int(x, y, zValue);
                    // an implementation which will need to be refactored.
                    // when path gen is done, we will need to refactor.
                    terrainCellList.Add(map[x, y].position);
                }
            }
        }
    }

    private void setCellsExact(Cell[,] map, int mapWidth, int mapHeight, int cellDepth)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // check in the 2d z plane if we are able to put a tile on the cell
                // all tiles in the y plane at x,0 will also be valid cells
                if (map[x, y].status == Cell.CellStatus.ValidCell)
                {
                    // mark the cell as a terrain cell
                    map[x, y].status = Cell.CellStatus.TerrainCell;
                    map[x, y].position = new Vector3Int(x, y, cellDepth);

                    terrainCellList.Add(map[x, y].position);
                }
            }
        }
    }

    private int calculateDepth(int x, int y, int width, int height, float minCellDepth, float maxCellDepth, Vector2 offset)
    {
        // normalise the x and y positions to be between 0 and 1
        float xPos = (float)x / width;
        float yPos = (float)y / height;

        // then apply scale
        xPos *= perlinScale;
        yPos *= perlinScale;

        // and add random offset to get a different section of the noise each generation
        xPos += offset.x;
        yPos += offset.y;

        // perlin should return value between 0.0f and 1.0f, but the doc says it
        // may return values slightly outside the bounds, so clamp it
        float perlinNoiseValue = Mathf.Clamp(Mathf.PerlinNoise(xPos, yPos), 0.0f, 1.0f);

        // change the value range of the perlin noise value from 0.0-1.0 to an integer between terrainMinHeight and terrainMaxHeight
        int zValue = (int)Math.Round((perlinNoiseValue * (maxCellDepth - minCellDepth)) + minCellDepth, MidpointRounding.AwayFromZero);

        return zValue;
    }

    public void generate(Cell[,] map)
    {
        // set the array of positions and array of tiles from the level cells which are terrain
        // then populate the terrain tilemap with the tiles

        // probably can use cellular automata here to choose the terrain tile to be used
        List<Vector3Int> positions = new List<Vector3Int>();
        List<TileBase> tiles = new List<TileBase>();

        Tile[] groundTiles = terrainTilesByType[terrainSettings.tType].groundTiles;
        Tile[] accessoryTiles = terrainTilesByType[terrainSettings.tType].accessoryTiles;
        int groundTilesLength = groundTiles.Length;
        int accessoryTilesLength = accessoryTiles.Length;

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y].status == Cell.CellStatus.TerrainCell)
                {
                    positions.Add(map[x, y].position);
                    // select random accessory tile at 30% chance
                    tiles.Add(UnityEngine.Random.Range(0.0f, 10.0f) > 3.0f ? groundTiles[map[x, y].position.z % groundTilesLength] : accessoryTiles[map[x, y].position.z % accessoryTilesLength]);

                }

                // add the cells below ground level at the boundary
                // if its a cell at the boundary towards the camera
                if (x == 0 || y == 0)
                {
                    for (int z = map[x, y].position.z - 2; z >= 0; z--)
                    {
                        positions.Add(new Vector3Int(map[x, y].position.x, map[x, y].position.y, z));
                        tiles.Add(groundTiles[0]);
                    }
                }
            }
        }

        List<Vector3Int> positions2 = new List<Vector3Int>();
        List<TileBase> tiles2 = new List<TileBase>();

        foreach (Vector2Int boundarycell in boundaryCellList)
        {
            positions2.Add(new Vector3Int(map[boundarycell.x, boundarycell.y].position.x, map[boundarycell.x, boundarycell.y].position.y, 0));
            tiles2.Add(test);
        }

       // terrainTilemap2.SetTiles(positions2.ToArray(), tiles2.ToArray());

        terrainTilemap.SetTiles(positions.ToArray(), tiles.ToArray());
    }

    public void setOuterBounds(Cell[,] map)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        List<TileBase> tiles = new List<TileBase>();
        
        Vector2Int mapDimensions = new Vector2Int(map.GetLength(0), map.GetLength(1));
        foreach (Vector2Int boundaryCellPosition in boundaryCellList)
        {
            // if x+1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.x + 1 >  mapDimensions.x-1 || map[boundaryCellPosition.x + 1, boundaryCellPosition.y].status == Cell.CellStatus.InvalidCell)
            {
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x + 1, boundaryCellPosition.y, map[boundaryCellPosition.x, boundaryCellPosition.y].position.z);
                if (!positions.Contains(outerBoundPosition))
                {
                    positions.Add(outerBoundPosition);
                    tiles.Add(test);

                }
            }

            // if x-1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.x - 1 < 0 || map[boundaryCellPosition.x - 1, boundaryCellPosition.y].status == Cell.CellStatus.InvalidCell)
            {
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x - 1, boundaryCellPosition.y, map[boundaryCellPosition.x, boundaryCellPosition.y].position.z);
                if (!positions.Contains(outerBoundPosition))
                {
                    positions.Add(outerBoundPosition);
                    tiles.Add(test);
                }
            }

            // if y+1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.y + 1 > mapDimensions.y - 1 || map[boundaryCellPosition.x, boundaryCellPosition.y+1].status == Cell.CellStatus.InvalidCell)
            {
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x, boundaryCellPosition.y+1, map[boundaryCellPosition.x, boundaryCellPosition.y].position.z);
                if (!positions.Contains(outerBoundPosition))
                {
                    positions.Add(outerBoundPosition);
                    tiles.Add(test);

                }
            }

            // if x-1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.y - 1 < 0 || map[boundaryCellPosition.x, boundaryCellPosition.y-1].status == Cell.CellStatus.InvalidCell)
            {
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x, boundaryCellPosition.y-1, map[boundaryCellPosition.x, boundaryCellPosition.y].position.z);
                if (!positions.Contains(outerBoundPosition))
                {
                    positions.Add(outerBoundPosition);
                    tiles.Add(test);
                }
            }
        }

        terrainTilemapOuterBound.SetTiles(positions.ToArray(), tiles.ToArray());
    }

    public void clearTilemap()
    {
        terrainTilemap.ClearAllTiles();
        terrainTilemap2.ClearAllTiles();
        terrainTilemapOuterBound.ClearAllTiles();
    }

    public void randomlyGenerate()
    {

    }
}
