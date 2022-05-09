using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class LakeGenerator
{
    public enum NumberOfLakes { Low, Medium, High }
    public static int numberOfLakesCount = Enum.GetValues(typeof(NumberOfLakes)).Length;

    public enum MaxLakeSize { Small, Medium, Large }
    public static int maxLakeSizeCount = Enum.GetValues(typeof(MaxLakeSize)).Length;

    LakeOptions.LakeSettings lakeSettings;

    Dictionary<TerrainGenerator.TerrainType, Tile> lakeTilesByType;

    private readonly string waterTileName = "ISO_Tile_Water_01";
    private readonly string iceTileName = "ISO_Tile_Ice_01-06";
    private readonly string lavaTileName = "ISO_Tile_Lava_01";

    int lakeMinSize = 4;

    int lakeMaxSize;

    int lakeMaxCount;

    float countMultiplier = 0.002f;

    float sizeMultiplier = 0.027f;

    List<Rect> existingLakes;

    List<string> generationInfo;

    // lake depth is an issue
    public LakeGenerator(SpriteAtlas atlas, List<string> generationInfo)
    {
        lakeTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        Tile waterTile = ScriptableObject.CreateInstance<Tile>();
        waterTile.sprite = atlas.GetSprite(waterTileName);
        waterTile.colliderType = Tile.ColliderType.Grid;


        lakeTilesByType.Add(TerrainGenerator.TerrainType.Greenery, waterTile);
        lakeTilesByType.Add(TerrainGenerator.TerrainType.Dessert, waterTile);

        lakeTilesByType.Add(TerrainGenerator.TerrainType.Lava, ScriptableObject.CreateInstance<Tile>());
        lakeTilesByType[TerrainGenerator.TerrainType.Lava].sprite = atlas.GetSprite(lavaTileName);
        lakeTilesByType[TerrainGenerator.TerrainType.Lava].colliderType = Tile.ColliderType.Grid;

        lakeTilesByType.Add(TerrainGenerator.TerrainType.Snow, ScriptableObject.CreateInstance<Tile>());
        lakeTilesByType[TerrainGenerator.TerrainType.Snow].sprite = atlas.GetSprite(iceTileName);
        lakeTilesByType[TerrainGenerator.TerrainType.Snow].colliderType = Tile.ColliderType.Grid;

        this.generationInfo = generationInfo;
    }

    public void setLakeSettings(LakeOptions.LakeSettings lakeSettings)
    {
        this.lakeSettings = lakeSettings;
    }

    public void populateCells(Map map)
    {
        lakeMaxCount = (int)Math.Ceiling((map.terrainCellCount * countMultiplier * ((int)lakeSettings.lNum + 1)));
        generationInfo.Add(lakeMaxCount + " max lake count calculated based on terrain size and " + lakeSettings.lNum + " lake amount setting");

        lakeMaxSize = (int)Math.Ceiling(map.terrainCellCount * (sizeMultiplier * ((int)lakeSettings.lMaxSize + 1)));
        generationInfo.Add(lakeMaxSize + " max lake size calculated based on terrain size and" + lakeSettings.lMaxSize + " lake size setting");

        lakeMaxSize = lakeMaxSize < lakeMinSize ? lakeMinSize : lakeMaxSize;

        existingLakes = new List<Rect>();

        int count;
        for (count = 0; count < lakeMaxCount; count++)
        {
            Vector2Int lakeDimensions = getDimensions(UnityEngine.Random.Range(lakeMinSize, lakeMaxSize), 2);

            if (!addLake(map, lakeDimensions))
            {
                break;
            }
            
        }
        generationInfo.Add(count + " lakes generated");
    }

    private bool addLake(Map map, Vector2Int lakeDimension)
    {
        Vector2Int startPosition;
        Cell startCell;
        Vector2Int endPosition;
        int searchCount = 0;

        while (searchCount < map.terrainCellCount)
        {
            startPosition = (Vector2Int)map.getRandomCell();
            startCell = map.getCell(startPosition);

            if (map.isBoundaryCell(startPosition) || startCell.isWaterBound)
            {
                continue;
            }

;            // convert this from using cell positiions, to using rect, with min and max returned as cell poses
            // then i can use the overlaps method to check if a rectangle is in another.
            // we may need to randomise this (e.g. put all the pairs into a list and randomly remove as used)
            for (int x = -1; x < 2; x +=2)
            {
                for (int y = -1; y < 2; y += 2)
                {
                    int xOffset = x == 1 ? 0 : -x;

                    int yOffset = y == 1 ? 0 : -y;

                    // rect max returns the max corner relative to the rectangle position (assuming rectangle pos is from 0,0)
                    // rectint max returns the upper right corner relative to the rectangle position
                    Rect newLake = new Rect(startPosition.x+xOffset, startPosition.y+yOffset, (lakeDimension.x) * x, (lakeDimension.y) * y);

                    endPosition = new Vector2Int(startPosition.x + ((lakeDimension.x-1) * x), startPosition.y + ((lakeDimension.y-1) * y));
                    if (map.checkCell(endPosition) && !lakesOverlapOrAdjacent(newLake))
                    {
                        Vector2Int direction = new Vector2Int(x, y);

                        existingLakes.Add(newLake);

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
        for (int x = startPosition.x; x != endPosition.x+ (1 * direction.x); x += (1 * direction.x))
        {
            for (int y = startPosition.y; y != endPosition.y+(1 * direction.y); y += (1 * direction.y))
            {
                Cell cell = map.getCell(x, y);
                // make sure that if lake is on the edge, we do not
                // try to turn invalid cells into lake cells (for random shape map)
                if (cell.status == Cell.CellStatus.TerrainCell)
                {
                    cell.position.z = map.getMinDepth(cell);
                    map.updateCellStatus(cell, Cell.CellStatus.LakeCell);
                }

            }
        }
    }

    // check if any lakes ovelap or are directly adjacent to the new one
    private bool lakesOverlapOrAdjacent(Rect newLake)
    {
        foreach (Rect existingLake in existingLakes)
        {
            // check if existing lake is inside new lake
            // overlaps should not allow adjacency
            if (newLake.Overlaps(existingLake))
            {
                return true;
            }

            // check left side
            if (isTouchingSide(newLake.xMin, existingLake))
            {
                return true;
            }

            // check right side
            if (isTouchingSide(newLake.xMax, existingLake))
            {
                return true;
            }

            // check bottom
            if (isTouchingSide(newLake.yMin, existingLake))
            {
                return true;
            }

            // check top
            if (isTouchingSide(newLake.yMax, existingLake))
            {
                return true;
            }
        }
        return false;
    }

    public bool isTouchingSide(float side, Rect checkLake)
    {
        // if the left right bottom or top of the lake to be checked is touching the side to be checked
        if (side == checkLake.xMin || side == checkLake.xMax || side == checkLake.yMin || side == checkLake.yMax)
        {
            return true;
        }
        return false;
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
}
