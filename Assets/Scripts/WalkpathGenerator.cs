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

    private readonly string snowPathTileName = "ISO_Tile_Snow_02_ToStone_01";
    private readonly string pathTileName = "ISO_Tile_Stone_02";
    private readonly string lavaPathTileName = "ISO_Tile_Brick_Stone_02_01";

    private int walkpathMaxCount;

    public enum NumberOfWalkpaths { Low, Medium, High }

    WalkpathPathOptions.WalkpathSettings walkpathSettings;

    private const float wMultiplier = 0.0034f;

    // river gen currently only for square and rectangular levels
    public WalkpathGenerator(SpriteAtlas atlas)
    {
        pathTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        Tile pathTile = setupTile(atlas, pathTileName);

        pathTilesByType.Add(TerrainGenerator.TerrainType.Greenery, pathTile);
        pathTilesByType.Add(TerrainGenerator.TerrainType.Dessert, pathTile);

        Tile lavaPathTile = setupTile(atlas, lavaPathTileName);

        pathTilesByType.Add(TerrainGenerator.TerrainType.Lava, lavaPathTile);

        Tile snowPathTile = setupTile(atlas, snowPathTileName);

        pathTilesByType.Add(TerrainGenerator.TerrainType.Snow, snowPathTile);
    }

    private Tile setupTile(SpriteAtlas atlas, string tilename)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = atlas.GetSprite(tilename);
        tile.colliderType = Tile.ColliderType.Grid;

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

            walkpathMaxCount = (int)Math.Ceiling(map.area * (wMultiplier * ((int)walkpathSettings.wNum + 1)));

            Heap<CellPair> cellPairs = new Heap<CellPair>(walkpathMaxCount);

            for (int count = 0; count < walkpathMaxCount; count++)
            {
                CellPair pair = getReachableCells(map, boundaryCellPositions, cellPairs, walkpathSettings.intersectionsEnabled);

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

                bool done = findAStarPath(map, cellPair.startCell, cellPair.endCell, Cell.CellStatus.WalkpathCell, walkpathSettings.intersectionsEnabled);
            }
        
    }

    public void randomlyGenerate()
    {

    }
}
