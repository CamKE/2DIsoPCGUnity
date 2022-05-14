using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

/// <summary>
/// Responsible for creating the lakes in the level.
/// </summary>
public class LakeGenerator
{
    /// <summary>
    /// The scale for amount of lakes, relative to the level size.
    /// </summary>
    public enum NumberOfLakes { Low, Medium, High }
    /// <summary>
    /// The number of options in the NumberOfLakes enumeration.
    /// </summary>
    public static int numberOfLakesCount = Enum.GetValues(typeof(NumberOfLakes)).Length;
    /// <summary>
    /// The scale for maximum size of a lake, relative to the level size.
    /// </summary>
    public enum MaxLakeSize { Small, Medium, Large }
    /// <summary>
    /// The number of options in the MaxLakeSize enumeration.
    /// </summary>
    public static int maxLakeSizeCount = Enum.GetValues(typeof(MaxLakeSize)).Length;
    // the settings to be used for lake generation
    private LakeSettings lakeSettings;
    // the tiles to be used for lake generation, sorted by the terrain type
    private readonly Dictionary<TerrainGenerator.TerrainType, Tile> lakeTilesByType;

    // the names of the tiles
    private readonly string waterTileName = "ISO_Tile_Water_01";
    private readonly string iceTileName = "ISO_Tile_Ice_01-06";
    private readonly string lavaTileName = "ISO_Tile_Lava_01";

    // the minimum possible size of a lake
    private const int lakeMinSize = 4;
    // the maximum possible size of a lake
    private int lakeMaxSize;
    // the maximum amount of lakes
    private int lakeMaxCount;
    // the multipler for the amount of lakes
    private const float countMultiplier = 0.002f;
    // the multipler for the maximum lake size
    private const float sizeMultiplier = 0.027f;

    // the lakes which have already been added to the level. Used to prevent overlaps
    private List<Rect> existingLakes;
    // a reference to the level generation information for the level
    private List<string> generationInfo;

    /// <summary>
    /// The constructor for the LakeGenerator. Sets all the tiles according to their types, and set the
    /// reference to the level generation info
    /// </summary>
    /// <param name="atlas">A SpriteAtlas, holding all the sprites for the project.</param>
    /// <param name="generationInfo">A reference to the level generation information for the level.</param>
    public LakeGenerator(SpriteAtlas atlas, List<string> generationInfo)
    {
        // create a new dictionary object
        lakeTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        // create the water tile
        Tile waterTile = ScriptableObject.CreateInstance<Tile>();
        waterTile.sprite = atlas.GetSprite(waterTileName);
        waterTile.colliderType = Tile.ColliderType.Grid;

        // add the water tile as the tile for the following level types:
        lakeTilesByType.Add(TerrainGenerator.TerrainType.Greenery, waterTile);
        lakeTilesByType.Add(TerrainGenerator.TerrainType.Dessert, waterTile);
        lakeTilesByType.Add(TerrainGenerator.TerrainType.Skin, waterTile);

        // create the lava tile and add it to the lava level type
        lakeTilesByType.Add(TerrainGenerator.TerrainType.Lava, ScriptableObject.CreateInstance<Tile>());
        lakeTilesByType[TerrainGenerator.TerrainType.Lava].sprite = atlas.GetSprite(lavaTileName);
        lakeTilesByType[TerrainGenerator.TerrainType.Lava].colliderType = Tile.ColliderType.Grid;

        // create the ice tile and add it to the ice level type
        lakeTilesByType.Add(TerrainGenerator.TerrainType.Snow, ScriptableObject.CreateInstance<Tile>());
        lakeTilesByType[TerrainGenerator.TerrainType.Snow].sprite = atlas.GetSprite(iceTileName);
        lakeTilesByType[TerrainGenerator.TerrainType.Snow].colliderType = Tile.ColliderType.Grid;

        // set the reference to the level gen info
        this.generationInfo = generationInfo;
    }

    /// <summary>
    /// Set the reference to the lake settings
    /// </summary>
    /// <param name="lakeSettings"></param>
    public void setLakeSettings(LakeSettings lakeSettings)
    {
        this.lakeSettings = lakeSettings;
    }

    /// <summary>
    /// Update the map by adding the lakes.
    /// </summary>
    /// <param name="map">A reference to the Map object, which represents the state of all cells in the level.</param>
    public void populateCells(Map map)
    {
        // calculate the maximum lake count based on the multiplier, the number of terrain cells in the level, and the number of lakes setting
        lakeMaxCount = (int)Math.Ceiling((map.terrainCellCount * countMultiplier * ((int)lakeSettings.lNum + 1)));
        // add the generation step to the info list
        generationInfo.Add(lakeMaxCount + " max lake count calculated based on terrain size and " + lakeSettings.lNum + " lake amount setting");

        // calculate the maximum lake size based on the multiplier, the number of terrain cells in the level, and the max lake size setting
        lakeMaxSize = (int)Math.Ceiling(map.terrainCellCount * (sizeMultiplier * ((int)lakeSettings.lMaxSize + 1)));
        // add the generation step to the info list
        generationInfo.Add(lakeMaxSize + " max lake size calculated based on terrain size and" + lakeSettings.lMaxSize + " lake size setting");

        // max lake size must not be below minimum size
        lakeMaxSize = lakeMaxSize < lakeMinSize ? lakeMinSize : lakeMaxSize;

        // create a new exising lake list
        existingLakes = new List<Rect>();

        // for count from 0 to lakeMaxCount-1
        int count;
        for (count = 0; count < lakeMaxCount; count++)
        {
            // get the dimensions of the lake. Rectangular shaped (2:1 width to height ratio)
            Vector2Int lakeDimensions = getDimensions(UnityEngine.Random.Range(lakeMinSize, lakeMaxSize), 2);

            // if the lake is not added
            if (!addLake(map, lakeDimensions))
            {
                // stop trying to add lakes, as this may be an indication there is no spce
                break;
            }

        }
        // add the generation step to the info list
        generationInfo.Add(count + " lakes generated");
    }

    // adds a lake to the map, provided there is space
    private bool addLake(Map map, Vector2Int lakeDimension)
    {
        // start and end positions for the lake
        Vector2Int startPosition;
        Vector2Int endPosition;
        // the number of searches done
        int searchCount = 0;

        // while the search count is less than the number of terrain cells on the map
        while (searchCount < map.terrainCellCount)
        {
            // set the start position
            startPosition = (Vector2Int)map.getRandomTerrainCellPosition();

            // if the cell at the start position is not valid
            if (!map.isValidCellPosition(startPosition))
            {
                // move onto the next iteration, find another start position
                continue;
            }

            // check the validity of lake shapes in all directions based on end cell position:
            // top left (-1,1), top right (1,1), bottom left (-1,-1), and bottom right (1,-1)
            for (int x = -1; x < 2; x += 2)
            {
                for (int y = -1; y < 2; y += 2)
                {
                    // the current direction of the lake from the start position
                    Vector2Int direction = new Vector2Int(x, y);

                    // workout what the x and y offset should be for the Rect object
                    // based on the current x and y values
                    // (tilemap grid and rect have different cell pivots)
                    int xOffset = direction.x == 1 ? 0 : -direction.x;

                    int yOffset = direction.y == 1 ? 0 : -direction.y;

                    // create the lake shape is a rect
                    Rect newLake = new Rect(startPosition.x + xOffset, startPosition.y + yOffset, (lakeDimension.x) * direction.x, (lakeDimension.y) * direction.y);

                    // work out the end position
                    endPosition = new Vector2Int(startPosition.x + ((lakeDimension.x - 1) * direction.x), startPosition.y + ((lakeDimension.y - 1) * direction.y));

                    // if the end position is valid and the lake does not overlap others and the lake is not on the edge of the terrain
                    if (map.isValidCellPosition(endPosition) && !lakesOverlapOrAdjacent(newLake) && !lakeOnEdge(map, newLake, xOffset, yOffset))
                    {
                        // put the lake in the map
                        placeLake(map, startPosition, endPosition, direction);

                        // add the lake to the existing lakes list
                        existingLakes.Add(newLake);

                        // lake successfully added, return true
                        return true;
                    }
                }
            }
            // increment the search count
            searchCount++;
        }
        // lake not added, return false
        return false;
    }

    // check if the lake is on the edge of the terrain
    private bool lakeOnEdge(Map map, Rect lake, int xOffset, int yOffset)
    {
        // only check if the lake is on the edge if the map is a randomly shaped one
        // for other shapes, it is not a possibility, so return false by default
        if (map.shape == TerrainGenerator.TerrainShape.Random)
        {
            // get the boundary cell positions
            List<Vector2Int> boundaryCellPositions = map.getBoundaryCellPositions();
            // get the offset for the cell to be compared to the rect position
            Vector2 offset = new Vector2(xOffset, yOffset);
            // for each boundary cell position
            foreach (Vector2 boundaryCellPosition in boundaryCellPositions)
            {
                // if the lake contains the boundary cell position
                if (lake.Contains(boundaryCellPosition + offset, true))
                {
                    // the lake is on the edge, return true
                    return true;
                }
            }
        }
        // no boundary cells inside the lake, return false
        return false;
    }

    // place the lake on the map
    private void placeLake(Map map, Vector2Int startPosition, Vector2Int endPosition, Vector2Int direction)
    {
        // loop through all the cell positions between start and end
        for (int x = startPosition.x; x != endPosition.x + (1 * direction.x); x += (1 * direction.x))
        {
            for (int y = startPosition.y; y != endPosition.y + (1 * direction.y); y += (1 * direction.y))
            {
                // get the cell at the position
                Cell cell = map.getCell(x, y);
                // update its z position to be the minimum depth amongst
                // itself and its neighbours
                cell.position.z = map.getMinDepth(cell);
                // change it to a lake cell
                map.updateCellStatus(cell, Cell.CellStatus.LakeCell);
            }
        }
    }

    // check if any lakes ovelap or are directly adjacent to the new one
    private bool lakesOverlapOrAdjacent(Rect newLake)
    {
        // for eeach of the existing lakes
        foreach (Rect existingLake in existingLakes)
        {
            // check if existing lake is inside new lake
            if (newLake.Overlaps(existingLake, true))
            {
                return true;
            }

            /*
             * check for adjacency on all sides
             */

            // check left side of new lake against existing lakes sides
            if (isTouchingSide(newLake.xMin, existingLake))
            {
                return true;
            }

            // check right side of new lake against existing lakes sides
            if (isTouchingSide(newLake.xMax, existingLake))
            {
                return true;
            }

            // check bottom side of new lake against existing lakes sides
            if (isTouchingSide(newLake.yMin, existingLake))
            {
                return true;
            }

            // check top side of new lake against existing lakes sides
            if (isTouchingSide(newLake.yMax, existingLake))
            {
                return true;
            }
        }

        // does not overlap and is not adjacent
        return false;
    }

    // check if a side is touching any of the sides of the lake to be checked
    private bool isTouchingSide(float side, Rect checkLake)
    {
        // if the left right bottom or top of the lake to be checked is touching the side to be checked
        if (side == checkLake.xMin || side == checkLake.xMax || side == checkLake.yMin || side == checkLake.yMax)
        {
            return true;
        }
        // no touching sides
        return false;
    }

    /// <summary>
    /// Gets the lake tile to be used based on the type of terrain.
    /// </summary>
    /// <returns>The lake tile.</returns>
    public Tile getTile()
    {
        return lakeTilesByType[lakeSettings.tType];
    }

    // calculates with width and length based on the terrainsize and the width to length ratio
    private Vector2Int getDimensions(int terrainSize, int ratio = 1)
    {
        // the absolute length of one side
        double rawLength = Math.Sqrt((float)terrainSize / ratio);
        // the raw length rounded down
        int length = (int)Math.Floor(rawLength);

        // if the area is less than the lake minimum size
        if ((length * length * ratio) < lakeMinSize)
        {
            // round the raw length up
            length = (int)Math.Ceiling(rawLength);
        }

        // random orientation of width and height
        return UnityEngine.Random.value > 0.5f ? new Vector2Int(length, length * ratio) : new Vector2Int(length * ratio, length);
    }
}
