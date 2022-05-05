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

    int lakeMinSize = 4;

    int lakeMaxSize;

    int lakeMaxCount;

    float lMultiplier = 0.0034f;

    float sizeMultiplier = 0.05f;

    List<CellPair> existingLakes;

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
        lakeMaxCount = (int)Math.Ceiling(map.terrainCellCount * (lMultiplier * ((int)lakeSettings.lNum + 1)));

        lakeMaxSize = (int)Math.Ceiling(map.terrainCellCount * (sizeMultiplier * ((int)lakeSettings.lMaxSize + 1)));

        for (int count = 0; count < lakeMaxCount; count++)
        {
            Vector2Int lakeDimensions = getDimensions(5, 2);

            if (!addLake(map, lakeDimensions))
            {
                break;
            }
            
        }
    }

    private bool addLake(Map map, Vector2Int lakeDimension)
    {
        Vector2Int startPosition;
        Cell startCell;
        Vector2Int endPosition;
        Cell endCell;
        int searchCount = 0;

        while (searchCount < map.terrainCellCount)
        {
            startPosition = (Vector2Int)map.getRandomCell();
            startCell = map.getCell(startPosition);

            if (map.isBoundaryCell(startPosition) || startCell.isWaterBound)
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
                        Vector2Int direction = new Vector2Int(x, y);
                        endCell = map.getCell(endPosition);
                        CellPair newLake = new CellPair(startCell, endCell);


                        // put the lake in the position

                        placeLake(map, startPosition, endPosition, direction);
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

    private bool lakesOverlap(CellPair newLake, Vector2Int direction)
    {
        Rect lake = new Rect();
        foreach (CellPair existingLake in existingLakes)
        {
            // check if existing lake is inside new lake
            if (existingLake.startCell.position.x > newLake.startCell.position.x && existingLake.startCell.position.y > newLake.startCell.position.y)
            {
                if (existingLake.endCell.position.x < newLake.endCell.position.x && existingLake.endCell.position.y < newLake.endCell.position.y)
                {
                    // existing lake is inside new lake
                    return true;
                }
            }

           // if (newLake.startCell.position.x - )
            {

            }
        }
        return true;
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
        if ((length * length * ratio) < lakeMinSize)
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
