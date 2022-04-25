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
        terrainTilemap = new GameObject("Terrain").AddComponent<Tilemap>();

        terrainTilemap.gameObject.AddComponent<TilemapRenderer>();
        terrainTilemap.transform.SetParent(grid.gameObject.transform);
        // move tile anchor from the button of the tile, to the front point of the tile (in the z)
        terrainTilemap.tileAnchor = new Vector3(0, 0, -2);

        var terrainTilemapRenderer = terrainTilemap.GetComponent<TilemapRenderer>();

        terrainTilemapRenderer.mode = TilemapRenderer.Mode.Individual;

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

        Debug.Log($"width is {width} and height is {height}");

        for (int x = 0; x < width; x += (width - 1))
        {
            for (int y = 1; y < height - 1; y++)
            {
                Debug.Log($"point {x},{y}");
                boundaryCellList.Add(new Vector2Int(x, y));
            }
        }
        for (int y = 0; y < height; y += (height - 1))
        {
            for (int x = 1; x < width - 1; x++)
            {
                Debug.Log($"point {x},{y}");
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

        Queue<Vector2Int> neighbourCellPositions = new Queue<Vector2Int>();
        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();
        neighbourCellPositions.Enqueue(centerCell);

        Vector2Int currentCellPosition;

        float validTileChance = 1.0f;
        // the rate at which each additional tile reduces the valid tile chance 
        float chanceFalloffRate = 0.5f;

        while (neighbourCellPositions.Count != 0)
        {
            currentCellPosition = neighbourCellPositions.Dequeue();

            //make valid based on random prob
            if (neighbourCellPositions.Count != 0 && UnityEngine.Random.value < validTileChance)
            {
                newLevelShape[currentCellPosition.x, currentCellPosition.y].setCellStatus(Cell.CellStatus.ValidCell);
                validTileChance -= (chanceFalloffRate / (float)terrainSize);
            }

            if (!visitedPositions.Contains(currentCellPosition) && newLevelShape[currentCellPosition.x, currentCellPosition.y].status == Cell.CellStatus.ValidCell)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    int xPos = currentCellPosition.x + i;
                    Vector2Int adjacentXCellPosition = new Vector2Int(xPos, currentCellPosition.y);
                    if ((xPos >= 0 && xPos < terrainLength))
                    {
                        neighbourCellPositions.Enqueue(adjacentXCellPosition);
                    }
                    int yPos = currentCellPosition.y + i;
                    Vector2Int adjacentYCellPosition = new Vector2Int(currentCellPosition.x, yPos);
                    if ((yPos >= 0 && yPos < terrainLength))
                    {
                        neighbourCellPositions.Enqueue(adjacentYCellPosition);
                    }
                }

                visitedPositions.Add(currentCellPosition);
            }

        }

        return newLevelShape;
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

        Debug.Log($"max cell depth {maxCellDepth}, min cell depth {minCellDepth}");
        Debug.Log($"xpos {xPos}, ypos {yPos}, perlin noise value {perlinNoiseValue}, z value {zValue}");
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

        terrainTilemap.SetTiles(positions.ToArray(), tiles.ToArray());
    }

    public void clearTilemap()
    {
        terrainTilemap.ClearAllTiles();
    }

    public void randomlyGenerate()
    {

    }
}
