using System;
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

    private int lakeMaxsize;

    float lakeCoveragePercentage = 0.1f;

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

    public void populateCells(Map map)
    {
        //continue here
        int terrainCellCountThreshhold = (int)Math.Round(map.terrainCellCount * (1.0f - lakeCoveragePercentage), MidpointRounding.AwayFromZero);
        int lakeCount = 0;

        while (map.terrainCellCount > terrainCellCountThreshhold)
        {
            Vector2Int lakeDimensions = getDimensions(5, 2);

            if (!addLake(map, lakeDimensions))
            {
                break;
            }
            lakeCount++;
        }
    }

    private bool addLake(Map map, Vector2Int lakeDimension)
    {
        Vector2Int startPosition;
        Vector2Int endPosition;
        int searchCount = 0;

        while (searchCount < map.terrainCellCount)
        {
            startPosition = (Vector2Int)map.getRandomCell();
            
            if (map.isBoundaryCell(startPosition) || map.getCell(startPosition).isWaterBound)
            {
                continue;
            }

            // we may need to randomise this (e.g. put all the pairs into a list and randomly remove as used)
            for (int x = -1; x < 2; x+=2)
            {
                for (int y = -1; y < 2; y += 2)
                {
                    endPosition = new Vector2Int(startPosition.x + ((lakeDimension.x) * x), startPosition.y + ((lakeDimension.y) * y));
                    if (map.checkCell(endPosition))
                    {
                        // put the lake in the position
                        placeLake(map, startPosition, endPosition, new Vector2Int(x,y));
                        return true;
                    }
                }
            }
            searchCount++;
        }
        return false;
    }

    private void placeLake(Map map, Vector2Int startPosition, Vector2Int endPosition, Vector2Int direction)
    {
        for (int x = startPosition.x; x != endPosition.x; x += (1 * direction.x))
        {
            for (int y = startPosition.y; y != endPosition.y; y += (1 * direction.y))
            {
                Cell cell = map.getCell(x, y);
                // make sure that if lake is on the edge, we do not
                // try to turn invalid cells into lake cells (for random shape map)
                if (cell.status == Cell.CellStatus.TerrainCell)
                {
                    map.updateCellStatus(cell, Cell.CellStatus.LakeCell);
                }

            }
        }
    }

    public Tile getTile()
    {
        return lakeTilesByType[lakeSettings.tType];
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

    public void randomlyGenerate()
    {
        // fix dimension
        // fix lake amount
        // fix lake size
        // fix lakes intersecting
    }
}
