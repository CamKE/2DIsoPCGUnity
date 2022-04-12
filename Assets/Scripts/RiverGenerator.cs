using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class RiverGenerator
{
    public struct RiverUserSettings
    {
        readonly public bool generationEnabled;
        readonly public numRivers rNum;
        readonly public bool intersectionsEnabled;
        readonly public bool bridgesEnabled;
        readonly public TerrainGenerator.terrainType tType;

        public RiverUserSettings(TerrainGenerator.terrainType tType, bool generationEnabled, numRivers rNum, bool intersectionsEnabled, bool bridgesEnabled)
        {
            this.tType = tType;
            this.generationEnabled = generationEnabled;
            this.rNum = rNum;
            this.intersectionsEnabled = intersectionsEnabled;
            this.bridgesEnabled = bridgesEnabled;
        }
    }

    private Tilemap riverTilemap;

    Dictionary<TerrainGenerator.terrainType, Tile> riverTilesByType;

    private readonly string waterTileName = "ISO_Tile_Water_01";
    private readonly string iceTileName = "ISO_Tile_Water_01";
    private readonly string lavaTileName = "ISO_Tile_Lava_01-06";

    private TerrainGenerator.terrainType selectedType;

    private int riverMaxCount;

    public enum numRivers { Low, Medium, High }

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

    public void populateCells(RiverUserSettings riverUserSettings, LevelManager.levelCellStatus[,,] levelCells)
    {
        selectedType = riverUserSettings.tType;
        int levelArea = levelCells.GetLength(0) * levelCells.GetLength(1);

        riverMaxCount = (int)Math.Ceiling(levelArea * (0.01f * ((int)riverUserSettings.rNum + 1)));

        for (int riverCount = 0; riverCount < riverMaxCount; riverCount++)
        {

        }
        //rando test
        levelCells[4, 0, 0] = LevelManager.levelCellStatus.riverCell;
        levelCells[4, 1, 0] = LevelManager.levelCellStatus.riverCell;
        levelCells[4, 2, 0] = LevelManager.levelCellStatus.riverCell;
        levelCells[4, 3, 0] = LevelManager.levelCellStatus.riverCell;
        levelCells[4, 4, 0] = LevelManager.levelCellStatus.riverCell;
        levelCells[4, 5, 0] = LevelManager.levelCellStatus.riverCell;

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

        for (int x = 0; x < levelCells.GetLength(0); x++)
        {
            for (int y = 0; y < levelCells.GetLength(1); y++)
            {
                for (int z = 0; z < levelCells.GetLength(2); z++)
                {
                    if (levelCells[x, y, z] == LevelManager.levelCellStatus.riverCell)
                    {
                        positions.Add(new Vector3Int(x, y, z-1));
                        tiles.Add(riverTilesByType[selectedType]);
                    }
                }
            }
        }
        riverTilemap.SetTiles(positions.ToArray(), tiles.ToArray());
    }

    public void randomlyGenerate()
    {

    }
}
