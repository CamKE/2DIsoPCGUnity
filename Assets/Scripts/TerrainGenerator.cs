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
    public struct TerrainUserSettings
    {
        readonly public int tSize;
        readonly public terrainType tType;
        readonly public int tMinHeight, tMaxHeight;

        readonly public int tExactHeight;
        readonly public terrainShape tShape;

        public TerrainUserSettings(int tSize, terrainType tType, int tMinHeight, int tMaxHeight, terrainShape tShape)
        {
            this.tSize = tSize;
            this.tType = tType;
            this.tMinHeight = tMinHeight;
            this.tMaxHeight = tMaxHeight;
            this.tShape = tShape;
            tExactHeight = -1;
        }

        public TerrainUserSettings(int tSize, terrainType tType, int tExactHeight, terrainShape tShape)
        {
            this.tSize = tSize;
            this.tType = tType;
            this.tExactHeight = tExactHeight;
            this.tShape = tShape;
            tMinHeight = -1;
            tMaxHeight = -1;
        }
    }
    /// <summary>
    /// The terrain shape options.
    /// </summary>
    public enum terrainShape { Square, Rectangle, Random };

    /// <summary>
    /// The terrain type options.
    /// </summary>
    public enum terrainType { Greenery, Icy, Lava };

    /// <summary>
    /// The minimum size of a level specified by tile count.
    /// </summary>
    public const int terrainMinSize = 10;

    /// <summary>
    /// The maximum size of a level specified by tile count.
    /// </summary>
    public const int terrainMaxSize = 1500;

    public const int terrainMinHeight = 0;

    public const int terrainMaxHeight = 5;

    // determines the resolution at which we sample the perlin noise
    // set such that the variation in values returned is gradual
    private const float perlinScale = 5.0f;

    private Tilemap terrainTilemap;

    private readonly string[] greeneryGroundTileNames = { "ISO_Tile_Dirt_01_Grass_01", "ISO_Tile_Dirt_01_Grass_02","ISO_Tile_Dirt_01_GrassPatch_01",
        "ISO_Tile_Dirt_01_GrassPatch_02", "ISO_Tile_Dirt_01_GrassPatch_03"};
    private Tile[] greeneryGroundTiles;

    Tile tile;

    public TerrainGenerator(Grid grid, SpriteAtlas atlas)
    {
        terrainTilemap = new GameObject("Terrain").AddComponent<Tilemap>();

        terrainTilemap.gameObject.AddComponent<TilemapRenderer>();
        terrainTilemap.transform.SetParent(grid.gameObject.transform);
        // move tile anchor from the button of the tile, to the front point of the tile (in the z)
        terrainTilemap.tileAnchor = new Vector3(0, 0, -2);

        var terrainTilemapRenderer = terrainTilemap.GetComponent<TilemapRenderer>();

        terrainTilemapRenderer.mode = TilemapRenderer.Mode.Individual;

        tile = ScriptableObject.CreateInstance<Tile>();

        tile.sprite = atlas.GetSprite("ISO_Tile_Dirt_01_Grass_01");

        int length = greeneryGroundTileNames.Length;

        greeneryGroundTiles = new Tile[length];

        for (int x = 0; x < length; x++)
        {
            greeneryGroundTiles[x] = ScriptableObject.CreateInstance<Tile>();
            greeneryGroundTiles[x].sprite = atlas.GetSprite(greeneryGroundTileNames[x]);
        }
    }

    public BoundsInt getTilemapBounds()
    {
        return terrainTilemap.cellBounds;
    }

    public void populateCells(TerrainUserSettings terrainUserSettings, LevelManager.levelCellStatus[,,] levelCells)
    {
        // define all the terrain cells

        // get width and height of the array
        int width = levelCells.GetLength(0);
        int height = levelCells.GetLength(1);

        if (terrainUserSettings.tExactHeight == -1)
        {
            setCellsRange(levelCells, width, height, terrainUserSettings.tMinHeight, terrainUserSettings.tMaxHeight);
        } else
        {
            setCellsExact(levelCells, width, height, terrainUserSettings.tExactHeight);
        }


    }

    private void setCellsRange(LevelManager.levelCellStatus[,,] levelCells, int levelCellsWidth, int levelCellsHeight, int minCellDepth, int maxCellDepth)
    {
        Vector2 perlinOffset = new Vector2(UnityEngine.Random.Range(0.0f, 999.0f), UnityEngine.Random.Range(0.0f, 999.0f));

        for (int x = 0; x < levelCellsWidth; x++)
        {
            for (int y = 0; y < levelCellsHeight; y++)
            {
                // check in the 2d z plane if we are able to put a tile on the cell
                // all tiles in the y plane at x,0 will also be valid cells
                if (levelCells[x, y, 0] == LevelManager.levelCellStatus.validCell)
                {
                    // work out which cell in the z plane should be populated
                    int zValue = calculateDepth(x, y, levelCellsWidth, levelCellsHeight, minCellDepth, maxCellDepth, perlinOffset);

                    // mark the cell as a terrain cell
                    levelCells[x, y, zValue] = LevelManager.levelCellStatus.terrainCell;

                    // if its a cell at the boundary towards the camera
                    if (x == 0 || y == 0)
                    {
                        // set all cells below it to be terrain
                        for (int z = zValue - 1; z >= 0; z--)
                        {
                            // mark the cell as a terrain cell
                            levelCells[x, y, z] = LevelManager.levelCellStatus.terrainCell;
                        }

                    }
                }
            }
        }
    }

    private void setCellsExact(LevelManager.levelCellStatus[,,] levelCells, int levelCellsWidth, int levelCellsHeight, int cellDepth)
    {
        for (int x = 0; x < levelCellsWidth; x++)
        {
            for (int y = 0; y < levelCellsHeight; y++)
            {
                // check in the 2d z plane if we are able to put a tile on the cell
                // all tiles in the y plane at x,0 will also be valid cells
                if (levelCells[x, y, 0] == LevelManager.levelCellStatus.validCell)
                {
                    // mark the cell as a terrain cell
                    levelCells[x, y, cellDepth] = LevelManager.levelCellStatus.terrainCell;

                        // if its a cell at the boundary towards the camera
                        if (x == 0 || y == 0)
                        {
                            // set all cells below it to be terrain
                            for(int z = cellDepth - 1; z >=0; z--)
                            {
                                // mark the cell as a terrain cell
                                levelCells[x, y, z] = LevelManager.levelCellStatus.terrainCell;
                            }

                        }
              
                }
            }
        }
    }

    private int calculateDepth(int x,int y, int width, int height, float minCellDepth, float maxCellDepth, Vector2 offset)
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
        float perlinNoiseValue = Mathf.Clamp(Mathf.PerlinNoise(xPos, yPos),0.0f,1.0f);

        // change the value range of the perlin noise value from 0.0-1.0 to an integer between terrainMinHeight and terrainMaxHeight
        int zValue = (int)Math.Round((perlinNoiseValue * (maxCellDepth - minCellDepth)) + minCellDepth, MidpointRounding.AwayFromZero);

        Debug.Log($"max cell depth {maxCellDepth}, min cell depth {minCellDepth}");
        Debug.Log($"xpos {xPos}, ypos {yPos}, perlin noise value {perlinNoiseValue}, z value {zValue}");
        return zValue;
    }

    public void generate(LevelManager.levelCellStatus[,,] levelCells)
    {
        // set the array of positions and array of tiles from the level cells which are terrain
        // then populate the terrain tilemap with the tiles

        // probably can use cellular automata here to choose the terrain tile to be used
        List<Vector3Int> positions = new List<Vector3Int>();
        List<TileBase> tiles = new List<TileBase>();

        for (int x = 0; x < levelCells.GetLength(0); x++)
        {
            for (int y = 0; y < levelCells.GetLength(1); y++)
            {
                for (int z = 0; z < levelCells.GetLength(2); z++)
                {
                    if (levelCells[x, y, z] == LevelManager.levelCellStatus.terrainCell)
                    {
                        positions.Add(new Vector3Int(x, y, z));
                        tiles.Add(greeneryGroundTiles[z % 2]);
                        // set tile
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
