using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

// refactoring...
// paths should not start and end on the same side... maybe neither should rivers
public class WalkpathGenerator : PathGenerator
{
    Dictionary<TerrainGenerator.TerrainType, Tile> pathTilesByType;

    private readonly string snowPathTileName = "ISO_Tile_Stone_03";
    private readonly string pathTileName = "ISO_Tile_Stone_02";
    private readonly string lavaPathTileName = "ISO_Tile_Brick_Stone_02_01";

    private int walkpathMaxCount;

    public enum NumberOfWalkpaths { Low, Medium, High }
    public static int numberOfWalkpathsCount = Enum.GetValues(typeof(NumberOfWalkpaths)).Length;


    WalkpathPathOptions.WalkpathSettings walkpathSettings;

    private const float wMultiplier = 0.0025f;

    List<string> generationInfo;

    // river gen currently only for square and rectangular levels
    public WalkpathGenerator(SpriteAtlas atlas, List<string> generationInfo)
    {
        pathTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        Tile pathTile = setupTile(atlas, pathTileName);

        pathTilesByType.Add(TerrainGenerator.TerrainType.Greenery, pathTile);
        pathTilesByType.Add(TerrainGenerator.TerrainType.Dessert, pathTile);

        Tile lavaPathTile = setupTile(atlas, lavaPathTileName);

        pathTilesByType.Add(TerrainGenerator.TerrainType.Lava, lavaPathTile);

        Tile snowPathTile = setupTile(atlas, snowPathTileName);

        pathTilesByType.Add(TerrainGenerator.TerrainType.Snow, snowPathTile);

        this.generationInfo = generationInfo;
    }

    private Tile setupTile(SpriteAtlas atlas, string tilename)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = atlas.GetSprite(tilename);

        return tile;
    }

    public void setWalkpathSettings(WalkpathPathOptions.WalkpathSettings walkpathSettings)
    {
        this.walkpathSettings = walkpathSettings;
    }

    public Tile getTile()
    {
        return pathTilesByType[walkpathSettings.tType];
    }

    public void populateCells(Map map)
    {
        List<Vector2Int> boundaryCellPositions = map.getBoundaryCellPositions();

        walkpathMaxCount = (int)Math.Ceiling(map.terrainCellCount * (wMultiplier * ((int)walkpathSettings.wNum + 1)));
        generationInfo.Add(walkpathMaxCount + " max walkpath count calculated based on terrain size and " + walkpathSettings.wNum + " walkpath amount setting");

        Heap<CellPair> cellPairs = new Heap<CellPair>(walkpathMaxCount);

        int count;
        for (count = 0; count < walkpathMaxCount; count++)
        {
            CellPair pair = getReachableCells(map, boundaryCellPositions, cellPairs, walkpathSettings.intersectionsEnabled);

            if (pair == null)
            {
                break;
            }

            cellPairs.Add(pair);

        }

        statusToCheck = Cell.CellStatus.RiverCell;

        while (cellPairs.Count > 0)
        {
            CellPair cellPair = cellPairs.RemoveFirst();
            cellPair.startCell.isTraversable = true;
            cellPair.endCell.isTraversable = true;

            if (!findAStarPath(map, cellPair.startCell, cellPair.endCell, Cell.CellStatus.WalkpathCell, walkpathSettings.intersectionsEnabled))
            {
                count--;
                Debug.Log(cellPair.startCell.position + " to " + cellPair.endCell.position);
                map.updateCellStatus(cellPair.startCell, Cell.CellStatus.InvalidCell);
                map.updateCellStatus(cellPair.endCell, Cell.CellStatus.InvalidCell);

            }
        }
        generationInfo.Add(count + " walkpaths generated");
    }
}
