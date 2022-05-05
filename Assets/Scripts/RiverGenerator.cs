using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;

public class RiverGenerator : PathGenerator
{
    Dictionary<TerrainGenerator.TerrainType, Tile> riverTilesByType;

    private readonly string waterTileName = "ISO_Tile_Water_01";
    private readonly string iceTileName = "ISO_Tile_Ice_01-06";
    private readonly string lavaTileName = "ISO_Tile_Lava_01";

    private int riverMaxCount;

    public enum NumberOfRivers { Low, Medium, High }

    RiverOptions.RiverSettings riverSettings;

    private const float rMultiplier = 0.0015f;

    // river gen currently only for square and rectangular levels
    public RiverGenerator(SpriteAtlas atlas)
    {
        riverTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        Tile waterTile = setupTile(atlas, waterTileName);

        riverTilesByType.Add(TerrainGenerator.TerrainType.Greenery, waterTile);
        riverTilesByType.Add(TerrainGenerator.TerrainType.Dessert, waterTile);

        Tile lavaTile = setupTile(atlas, lavaTileName);

        riverTilesByType.Add(TerrainGenerator.TerrainType.Lava, lavaTile);

        Tile iceTile = setupTile(atlas, iceTileName);

        riverTilesByType.Add(TerrainGenerator.TerrainType.Snow, iceTile);
    }

    private Tile setupTile(SpriteAtlas atlas, string tilename)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = atlas.GetSprite(tilename);
        tile.colliderType = Tile.ColliderType.Grid;

        return tile;
    }

    public void setRiverSettings(RiverOptions.RiverSettings riverSettings)
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

        Heap<CellPair> cellPairs = new Heap<CellPair>(riverMaxCount);

        for (int count = 0; count < riverMaxCount; count++)
        {
            CellPair pair = getReachableCells(map, map.getBoundaryCellPositions(), cellPairs, riverSettings.intersectionsEnabled);

            if (pair == null)
            {
                break;
            }

            cellPairs.Add(pair);

        }

      statusToCheck = Cell.CellStatus.WalkpathCell;

        while (cellPairs.Count > 0)
        {
            CellPair cellPair = cellPairs.RemoveFirst();
            cellPair.startCell.isTraversable = true;
            cellPair.endCell.isTraversable = true;

            bool done = findAStarPath(map, cellPair.startCell, cellPair.endCell, Cell.CellStatus.RiverCell, riverSettings.intersectionsEnabled);
        }
    }


    public void randomlyGenerate()
    {

    }
}
