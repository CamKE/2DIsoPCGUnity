using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;


public class RiverGenerator : PathGenerator
{
    Dictionary<TerrainGenerator.TerrainType, Tile> riverTilesByType;

    private readonly string waterTileName = "ISO_Tile_Water_01";
    private readonly string iceTileName = "ISO_Tile_Ice_01-06";
    private readonly string lavaTileName = "ISO_Tile_Lava_01";

    private int riverMaxCount;

    public enum NumberOfRivers { Low, Medium, High }
    public static int numberOfRiversCount = Enum.GetValues(typeof(NumberOfRivers)).Length;

    RiverSettings riverSettings;

    private const float rMultiplier = 0.0015f;

    List<string> generationInfo;

    // river gen currently only for square and rectangular levels
    public RiverGenerator(SpriteAtlas atlas, List<string> generationInfo)
    {
        riverTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        Tile waterTile = setupTile(atlas, waterTileName);

        riverTilesByType.Add(TerrainGenerator.TerrainType.Greenery, waterTile);
        riverTilesByType.Add(TerrainGenerator.TerrainType.Dessert, waterTile);
        riverTilesByType.Add(TerrainGenerator.TerrainType.Skin, waterTile);

        Tile lavaTile = setupTile(atlas, lavaTileName);

        riverTilesByType.Add(TerrainGenerator.TerrainType.Lava, lavaTile);

        Tile iceTile = setupTile(atlas, iceTileName);

        riverTilesByType.Add(TerrainGenerator.TerrainType.Snow, iceTile);

        this.generationInfo = generationInfo;
    }

    private Tile setupTile(SpriteAtlas atlas, string tilename)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = atlas.GetSprite(tilename);
        tile.colliderType = Tile.ColliderType.Grid;

        return tile;
    }

    public void setRiverSettings(RiverSettings riverSettings)
    {
        this.riverSettings = riverSettings;
    }

    public Tile getTile()
    {
        return riverTilesByType[riverSettings.tType];
    }

    public void populateCells(Map map)
    {
        riverMaxCount = (int)Math.Ceiling(map.terrainCellCount * (rMultiplier * ((int)riverSettings.rNum + 1)));
        generationInfo.Add(riverMaxCount + " max river count calculated based on terrain size and " + riverSettings.rNum + " river amount setting");

        Heap<CellPair> cellPairs = new Heap<CellPair>(riverMaxCount);

        int count;
        for (count = 0; count < riverMaxCount; count++)
        {
            CellPair pair = getReachableCells(map, cellPairs, riverSettings.intersectionsEnabled);

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

            if (!findAStarPath(map, cellPair.startCell, cellPair.endCell, Cell.CellStatus.RiverCell, riverSettings.intersectionsEnabled))
            {
                count--;
            }
        }

        generationInfo.Add(count + " rivers generated");
    }

}
