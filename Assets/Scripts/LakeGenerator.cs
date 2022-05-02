using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class LakeGenerator
{
    public enum NumberOfLakes { Low, Medium, High }

    public enum MaxLakeSize { Small, Medium, Large }

    LakeOptions.LakeSettings lakeSettings;

    Dictionary<TerrainGenerator.TerrainType, Tile> lakeTilesByType;

    private readonly string waterTileName = "ISO_Tile_Water_01";
    private readonly string iceTileName = "ISO_Tile_Ice_01-06";
    private readonly string lavaTileName = "ISO_Tile_Lava_01";

    public LakeGenerator(SpriteAtlas atlas)
    {
        lakeTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        Tile waterTile = ScriptableObject.CreateInstance<Tile>();
        waterTile.sprite = atlas.GetSprite(waterTileName);
        waterTile.colliderType = Tile.ColliderType.Grid;


        lakeTilesByType.Add(TerrainGenerator.TerrainType.Greenery, waterTile);
        lakeTilesByType.Add(TerrainGenerator.TerrainType.Dessert, waterTile);

        lakeTilesByType.Add(TerrainGenerator.TerrainType.Lava, ScriptableObject.CreateInstance<Tile>());
        lakeTilesByType[TerrainGenerator.TerrainType.Lava].sprite = atlas.GetSprite(lavaTileName);

        lakeTilesByType.Add(TerrainGenerator.TerrainType.Snow, ScriptableObject.CreateInstance<Tile>());
        lakeTilesByType[TerrainGenerator.TerrainType.Snow].sprite = atlas.GetSprite(iceTileName);
    }

    public void setLakeSettings(LakeOptions.LakeSettings lakeSettings)
    {
        this.lakeSettings = lakeSettings;
    }

    public void populateCells(Cell[,] map)
    {
        //continue here
    }

    public void randomlyGenerate()
    {

    }
}
