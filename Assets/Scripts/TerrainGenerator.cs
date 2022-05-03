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

    public TerrainGenerator(SpriteAtlas atlas)
    {
        test = ScriptableObject.CreateInstance<Tile>();
        test.sprite = atlas.GetSprite("ISO_Tile_Flesh_01");
        //test.colliderType = Tile.ColliderType.Grid;

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

    public Tile[] getGroundTiles()
    {
        return terrainTilesByType[terrainSettings.tType].groundTiles;
    }

    public Tile[] getAccessoryTiles()
    {
        return terrainTilesByType[terrainSettings.tType].accessoryTiles;
    }

    public void setTerrainSettings(TerrainOptions.TerrainSettings terrainSettings)
    {
        this.terrainSettings = terrainSettings;
    }

    public void populateCells(Map map)
    {
        // define all the terrain cells
        if (terrainSettings.heightRangedEnabled)
        {
            setCellsRange(map, terrainSettings.tMinHeight, terrainSettings.tMaxHeight); ;
        }
        else
        {
            setCellsExact(map, terrainSettings.tExactHeight);
        }

    }

    public Map createMap()
    {
        Vector2Int mapDimensions;
        Map map;

        // check the terrain shape chosen
        switch (terrainSettings.tShape)
        {
            // for rectangular shape
            case TerrainGenerator.TerrainShape.Rectangle:
                // 2:1, 3:1, or 4:1 ratio
                mapDimensions = getDimensions(terrainSettings.tSize, UnityEngine.Random.Range(2, 4));
                map = new Map(mapDimensions.x, mapDimensions.y);
                map.setBoundaryCells();
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
                map = new Map(mapDimensions.x, mapDimensions.y);
                map.setBoundaryCells();
                break;
        }

        return map;
    }

    private Map createRandomLevelShapeBFS(int terrainSize)
    {
        // get the square size.
        Vector2Int dimensions = getDimensions(terrainSize);
        
        // set all cells invalid initially
        Map map = new Map(dimensions.x, dimensions.x, Cell.CellStatus.InvalidCell);

        Cell centerCell = map.getCell(new Vector2Int(dimensions.x / 2, dimensions.x / 2));

       map.updateCellStatus(centerCell, Cell.CellStatus.ValidCell);

        Queue<Cell> openSet = new Queue<Cell>();
        HashSet<Cell> closedSet = new HashSet<Cell>();

        foreach (Cell neighbour in map.getNeighbours(centerCell))
        {
            openSet.Enqueue(neighbour);
        }

        closedSet.Add(centerCell);

        Cell currentCell;

        float validTileChance = 1.0f;
        // the rate at which each additional tile reduces the valid tile chance 
        float chanceFalloffRate = 0.1f;

        while (openSet.Count > 0)
        {
            currentCell = openSet.Dequeue();

            List<Cell> neighbours = map.getNeighbours(currentCell);
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
                        if (closedSet.Contains(neighbour))
                        {
                            invalidTileCount++;
                        }
                        break;
                }
            }

            //make valid based on random prob
            if (UnityEngine.Random.value < validTileChance && (validTileCount > 0 && invalidTileCount == 0 ))                                                                                                                                                                                                                                                                                                                                                   
            {
               map.updateCellStatus(currentCell, Cell.CellStatus.ValidCell);
                validTileChance -= (chanceFalloffRate / (float)terrainSize);

            }


            if (currentCell.status == Cell.CellStatus.ValidCell)
            {

                foreach (Cell neighbour in neighbours)
                {
                    if (!openSet.Contains(neighbour) && !closedSet.Contains(neighbour))
                    {
                        openSet.Enqueue(neighbour);
                    }
                }

            }

            closedSet.Add(currentCell);

        }

        // set the boundary cells
        foreach (Cell cell in map.getAll())
        {
            if (cell.status == Cell.CellStatus.ValidCell)
            {
                int invalidTileCount = 0;
                int validTileCount = 0;
                int neighbourCount = 0;


                foreach (Cell neighbour in map.getNeighbours(cell))
                {

                    switch (neighbour.status)
                    {
                        case Cell.CellStatus.ValidCell:
                            validTileCount++;
                            break;
                        //invalid
                        default:
                            if (closedSet.Contains(neighbour))
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
                    map.addBoundaryCellPosition((Vector2Int)cell.position);
                }
            }
        }

        return map;
    }

    // calculates the square for 1:1 length to width ratio.
    private Vector2Int getDimensions(int terrainSize, int ratio = 1)
    {

        double rawLength = Math.Sqrt((float)terrainSize / ratio);

        int length = (int)Math.Round(rawLength, MidpointRounding.AwayFromZero);

        int totalArea = length * length * ratio;

        if (totalArea < TerrainGenerator.terrainMinSize)
        {
            length = (int)Math.Ceiling(rawLength);
        }
        else if (totalArea > TerrainGenerator.terrainMaxSize)
        {
           length = (int)Math.Floor(rawLength);
        }

        // random orientation of width and height
        return UnityEngine.Random.value > 0.5f ? new Vector2Int(length, length * ratio) : new Vector2Int(length * ratio, length);
    }

    private void setCellsRange(Map map, int minCellDepth, int maxCellDepth)
    {
        Vector2 perlinOffset = new Vector2(UnityEngine.Random.Range(0.0f, 999.0f), UnityEngine.Random.Range(0.0f, 999.0f));

        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                // check in the 2d z plane if we are able to put a tile on the cell
                // all tiles in the y plane at x,0 will also be valid cells
                Cell currentCell = map.getCell(x, y);
                if (currentCell.status == Cell.CellStatus.ValidCell)
                {
                    // work out which cell in the z plane should be populated
                    int zValue = calculateDepth(x, y, map.width, map.height, minCellDepth, maxCellDepth, perlinOffset);

                    // mark the cell as a terrain cell
                   map.updateCellStatus( currentCell, Cell.CellStatus.TerrainCell);
                    currentCell.position.z = zValue;
                }
            }
        }
    }

    private void setCellsExact(Map map, int cellDepth)
    {
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                // check in the 2d z plane if we are able to put a tile on the cell
                // all tiles in the y plane at x,0 will also be valid cells
                Cell currentCell = map.getCell(x, y);
                if (currentCell.status == Cell.CellStatus.ValidCell)
                {
                    // mark the cell as a terrain cell
                   map.updateCellStatus( currentCell, Cell.CellStatus.TerrainCell);
                    currentCell.position.z = cellDepth;
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

    public void setOuterBounds(Map map, ref List<Vector3Int> positions, ref List<TileBase> tiles, ref List<Vector3Int> positions2, ref List<TileBase> tiles2)
    {
        foreach (Vector2Int boundaryCellPosition in map.getBoundaryCellPositions())
        {
            // if x+1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.x + 1 >  map.width-1 || map.getCell(boundaryCellPosition).status == Cell.CellStatus.InvalidCell)
            {
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x + 1, boundaryCellPosition.y, map.getCell(boundaryCellPosition).position.z);
                if (!positions2.Contains(outerBoundPosition) && !positions.Contains(outerBoundPosition))
                {
                    positions2.Add(outerBoundPosition);
                    tiles2.Add(test);

                }
            }

            // if x-1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.x - 1 < 0 || map.getCell(boundaryCellPosition.x - 1, boundaryCellPosition.y).status == Cell.CellStatus.InvalidCell)
            {
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x - 1, boundaryCellPosition.y, map.getCell(boundaryCellPosition).position.z);
                if (!positions.Contains(outerBoundPosition) && !positions2.Contains(outerBoundPosition))
                {
                    positions.Add(outerBoundPosition);
                    tiles.Add(test);
                }
            }

            // if y+1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.y + 1 > map.height - 1 || map.getCell(boundaryCellPosition.x, boundaryCellPosition.y+1).status == Cell.CellStatus.InvalidCell)
            {
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x, boundaryCellPosition.y+1, map.getCell(boundaryCellPosition).position.z);
                if (!positions2.Contains(outerBoundPosition) && !positions.Contains(outerBoundPosition))
                {
                    positions2.Add(outerBoundPosition);
                    tiles2.Add(test);

                }
            }

            // if x-1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.y - 1 < 0 || map.getCell(boundaryCellPosition.x, boundaryCellPosition.y-1).status == Cell.CellStatus.InvalidCell)
            {
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x, boundaryCellPosition.y-1, map.getCell(boundaryCellPosition).position.z);
                if (!positions.Contains(outerBoundPosition) && !positions2.Contains(outerBoundPosition))
                {
                    positions.Add(outerBoundPosition);
                    tiles.Add(test);
                }
            }
        }
    }

    public void randomlyGenerate()
    {

    }
}
