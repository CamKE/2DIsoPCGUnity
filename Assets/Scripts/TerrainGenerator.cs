using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

/// <summary>
/// Responsible for generating the terrain.
/// </summary>
public class TerrainGenerator
{
    /// <summary>
    /// The options for the terrain shape.
    /// </summary>
    public enum TerrainShape { Square, Rectangle, Random };
    /// <summary>
    /// The number of options in the TerrainShape enumeration.
    /// </summary>
    public static int terrainShapeCount = Enum.GetValues(typeof(TerrainShape)).Length;

    /// <summary>
    /// The options for the terrain type.
    /// </summary>
    public enum TerrainType { Greenery, Desert, Snow, Lava, Skin };
    /// <summary>
    /// The number of options in the TerrainType enumeration.
    /// </summary>
    public static int terrainTypeCount = Enum.GetValues(typeof(TerrainType)).Length;

    //increased from 50
    /// <summary>
    /// The minimum possible size of a level specified by tile count.
    /// </summary>
    public const int terrainMinSize = 150;

    // increased from 1500
    /// <summary>
    /// The maximum possible size of a level specified by tile count.
    /// </summary>
    public const int terrainMaxSize = 2000;

    /// <summary>
    /// The minimum possible height of the terrain.
    /// </summary>
    public const int terrainMinHeight = 0;

    /// <summary>
    /// The maximum possible height of the terrain.
    /// </summary>
    public const int terrainMaxHeight = 5;

    // determines the resolution at which we sample the perlin noise
    // set such that the variation in values returned is gradual
    private const float perlinScale = 3.0f;

    /*
     * The names of the tiles by the type. For each type; ground tiles and accessory tiles.
     */
    private readonly string[] greeneryGroundTileNames = { "ISO_Tile_Dirt_01_Grass_01", "ISO_Tile_Dirt_01_Grass_02" };

    private readonly string[] greeneryAccessoryTileNames = {"ISO_Tile_Dirt_01_GrassPatch_01",
        "ISO_Tile_Dirt_01_GrassPatch_02", "ISO_Tile_Dirt_01_GrassPatch_03"};

    private readonly string[] desertGroundTileNames = { "ISO_Tile_Sand_01", "ISO_Tile_Sand_02" };

    private readonly string[] desertAccessoryTileNames = {"ISO_Tile_Sand_01_ToStone_01",
        "ISO_Tile_Sand_01_ToStone_02", "ISO_Tile_Sand_02_ToStone_01", "ISO_Tile_Sand_02_ToStone_02", "ISO_Tile_Sand_03_ToStone_01", "ISO_Tile_Sand_03_ToStone_02"};

    private readonly string[] snowGroundTileNames = { "ISO_Tile_Snow_01", "ISO_Tile_Snow_02" };

    private readonly string[] snowAccessoryTileNames = {"ISO_Tile_Snow_01_ToStone_01",
        "ISO_Tile_Snow_01_ToStone_02", "ISO_Tile_Snow_02_ToStone_01", "ISO_Tile_Snow_02_ToStone_02"};

    private readonly string[] lavaGroundTileNames = { "ISO_Tile_LavaStone_01", "ISO_Tile_Tar_01" };

    private readonly string[] lavaAccessoryTileNames = { "ISO_Tile_LavaCracks_01", "ISO_Tile_LavaCracks_01 1" };

    private readonly string[] skinGroundTileNames = { "ISO_Tile_Flesh_01", "ISO_Tile_Skin_01" };

    private readonly string[] skinAccessoryTileNames = { "ISO_Tile_Flesh_01_Var01", "ISO_Tile_Skin_01_Smooth", "ISO_Tile_Skin_01_Smooth_Alt"};

    private readonly string[] lowerGroundTileNames = { "ISO_Tile_Dirt_01", "ISO_Tile_Sand_01", "ISO_Tile_Snow_01", "ISO_Tile_LavaStone_01", "ISO_Tile_Skin_01" };

    // the tile to be used for the outer bound. it is not rendered, but the shape is used
    // for the collider
    private readonly Tile outerBoundsTile;
    // the tiles to be used for terrain generation, sorted by the terrain type
    private readonly Dictionary<TerrainType, TerrainTiles> terrainTilesByType;
    // the check to be done to see if a given cell is a boundary cell
    private static Action<Cell> checkForBoundaryCell;
    // a reference to the level generation information for the level
    List<string> generationInfo;

    // the ground and accessory tiles to be used for the terrain. associated
    // with a terrain type in the dictionary
    private struct TerrainTiles
    {
        /// <summary>
        /// The ground tiles for the terrain.
        /// </summary>
        public Tile[] groundTiles;
        /// <summary>
        /// The accessory tiles for the terrain.
        /// </summary>
        public Tile[] accessoryTiles;

        /// <summary>
        /// Tile used below ground level.
        /// </summary>
        public Tile lowerGroundTile;
        /// <summary>
        /// The constructor for the TerrainTiles struct.
        /// </summary>
        /// <param name="groundTileNames">The name of the ground tiles.</param>
        /// <param name="accessoryTileNames">The name of the accessory tiles</param>
        /// <param name="atlas">Holds all the sprites for the project.</param>
        public TerrainTiles(string[] groundTileNames, string[] accessoryTileNames, string lowerGroundTileName, SpriteAtlas atlas)
        {
            // create the arrays with length relative to the number of tile names provided
            groundTiles = new Tile[groundTileNames.Length];
            accessoryTiles = new Tile[accessoryTileNames.Length];
            lowerGroundTile = null;

            lowerGroundTile = setTile(lowerGroundTileName, atlas);

            // set the tiles in each array
            setTiles(groundTileNames, groundTiles, atlas);
            setTiles(accessoryTileNames, accessoryTiles, atlas);
        }

        // set the tiles in the array using the names to find the sprites in the atlas
        private void setTiles(string[] names, Tile[] tiles, SpriteAtlas atlas)
        {
            // set all tiles
            for (int x = 0; x < tiles.Length; x++)
            {
                // set the tile at the array position to the tile with name at the corresponding
                // position
                tiles[x] = setTile(names[x], atlas);
            }
        }

        // retrieve tile and return it
        private Tile setTile(string tileName, SpriteAtlas atlas)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = atlas.GetSprite(tileName);

            return tile;
        }
    }

    // the settings to be used for terrain generation
    private TerrainSettings terrainSettings;

    /// <summary>
    /// The constructor for the TerrainGenerator. Sets all the tiles according to their types, and set the
    /// reference to the level generation info.
    /// </summary>
    /// <param name="atlas">A SpriteAtlas, holding all the sprites for the project.</param>
    /// <param name="generationInfo">A reference to the level generation information for the level.</param>
    public TerrainGenerator(SpriteAtlas atlas, List<string> generationInfo)
    {
        // set the outer bound tile
        outerBoundsTile = ScriptableObject.CreateInstance<Tile>();
        outerBoundsTile.sprite = atlas.GetSprite("ISO_Tile_Template_01");

        // create a new dictionary object
        terrainTilesByType = new Dictionary<TerrainType, TerrainTiles>();

        // create the greenery tiles and add them to the greenery level type
        TerrainTiles greeneryTiles = new TerrainTiles(greeneryGroundTileNames, greeneryAccessoryTileNames, lowerGroundTileNames[(int)TerrainType.Greenery], atlas);
        terrainTilesByType.Add(TerrainType.Greenery, greeneryTiles);

        // create the dessert tiles and add them to the dessert level type
        TerrainTiles dessertTiles = new TerrainTiles(desertGroundTileNames, desertAccessoryTileNames, lowerGroundTileNames[(int)TerrainType.Desert], atlas);
        terrainTilesByType.Add(TerrainType.Desert, dessertTiles);

        // create the snow tiles and add them to the snow level type
        TerrainTiles snowTiles = new TerrainTiles(snowGroundTileNames, snowAccessoryTileNames, lowerGroundTileNames[(int)TerrainType.Snow], atlas);
        terrainTilesByType.Add(TerrainType.Snow, snowTiles);

        // create the lava tiles and add them to the lava level type
        TerrainTiles lavaTiles = new TerrainTiles(lavaGroundTileNames, lavaAccessoryTileNames, lowerGroundTileNames[(int)TerrainType.Lava], atlas);
        terrainTilesByType.Add(TerrainType.Lava, lavaTiles);

        // create the skin tiles and add them to the skin level type
        TerrainTiles skinTiles = new TerrainTiles(skinGroundTileNames, skinAccessoryTileNames, lowerGroundTileNames[(int)TerrainType.Skin],  atlas);
        terrainTilesByType.Add(TerrainType.Skin, skinTiles);

        // set the reference to the level gen info
        this.generationInfo = generationInfo;
    }

    /// <summary>
    /// Get the ground tiles for the terrain type selected.
    /// </summary>
    /// <returns>The terrains ground tiles.</returns>
    public Tile[] getGroundTiles()
    {
        return terrainTilesByType[terrainSettings.tType].groundTiles;
    }

    /// <summary>
    /// Get the accessory tiles for the terrain type selected.
    /// </summary>
    /// <returns>The terrains accessory tiles.</returns>
    public Tile[] getAccessoryTiles()
    {
        return terrainTilesByType[terrainSettings.tType].accessoryTiles;
    }

    /// <summary>
    /// Get the lower ground tile for the terrain type selected.
    /// </summary>
    /// <returns>The terrains lower ground tile.</returns>
    public Tile getLowerGroundTile()
    {
        return terrainTilesByType[terrainSettings.tType].lowerGroundTile;
    }

    /// <summary>
    /// Set the reference to the terrain settings.
    /// </summary>
    /// <param name="terrainSettings">The terrain settings to set a reference to.</param>
    public void setTerrainSettings(TerrainSettings terrainSettings)
    {
        // add the generation step to the info list
        generationInfo.Add("Using " + terrainSettings.tType + " terrain type");
        this.terrainSettings = terrainSettings;
    }

    /// <summary>
    /// Update the map by adding the terrain cells.
    /// </summary>
    /// <param name="map">A reference to the Map object, which represents the state of all cells in the level.</param>
    public void populateCells(Map map)
    {
        // if height range is on
        if (terrainSettings.heightRangeEnabled)
        {
            // add the generation step to the info list
            generationInfo.Add("Populating the map with cell heights between " + terrainSettings.tMinHeight + " and " + terrainSettings.tMaxHeight);
            // set the cells with a range of height
            setCellsRange(map); ;
        }
        else
        {
            // add the generation step to the info list
            generationInfo.Add("Populating the map with cell exact height " + terrainSettings.tExactHeight);
            // set the cells with an exact height value
            setCellsExact(map);
        }

        // update the size setting with the actual size used for generation
        terrainSettings.updateTerrainSize(map.area);
        // add the generation step to the info list
        generationInfo.Add(map.getBoundaryCellPositions().Count + " boundary cells found from " + map.terrainCellCount + " terrain cells whilst populating map");
    }

    /// <summary>
    /// Create the level map based on the terrain shape selected.
    /// </summary>
    /// <returns>The level map.</returns>
    public Map createMap()
    {
        // the maps height and width
        Vector2Int mapDimensions;
        // the map created
        Map map;
        // ensure the action is clear
        checkForBoundaryCell = null;

        // check the terrain shape chosen
        switch (terrainSettings.tShape)
        {
            // for rectangular shape
            case TerrainGenerator.TerrainShape.Rectangle:
                // get dimensions for rectangular level. 2:1, 3:1, or 4:1 ratio
                mapDimensions = getDimensions(terrainSettings.tSize, UnityEngine.Random.Range(2, 5));
                // create map with the dimensions
                map = new Map(mapDimensions.x, mapDimensions.y, terrainSettings.tShape);
                // set the boundary cell check action
                checkForBoundaryCell += map.checkForBoundaryCell;
                break;
            // for random shape
            case TerrainGenerator.TerrainShape.Random:
                // create a random level shape
                map = createRandomLevelShapeBFS();
                // set the boundary cell check action
                checkForBoundaryCell += map.checkForBoundaryCellRandom;
                break;
            // default shape is square 
            default:
                // get dimensions for square level. 1:1 ratio default
                mapDimensions = getDimensions(terrainSettings.tSize);
                // create map with dimensions
                map = new Map(mapDimensions.x, mapDimensions.y, terrainSettings.tShape);
                // set the boundary cell check action
                checkForBoundaryCell += map.checkForBoundaryCell;
                break;
        }

        // add the generation steps to the info list
        generationInfo.Add("Creating "+ terrainSettings.tShape +" shaped map");
        generationInfo.Add("Terrain size " + terrainSettings.tSize + " rounded to square value " + map.area);
        generationInfo.Add("map dimension " + map.width + " by " + map.height);

        // return the map
        return map;
    }

    // uses breadth first search to create a randomly shaped level
    private Map createRandomLevelShapeBFS()
    {
        // get the square size.
        Vector2Int dimensions = getDimensions(terrainSettings.tSize);
        
        // set all cells invalid initially
        Map map = new Map(dimensions.x, dimensions.x, terrainSettings.tShape, Cell.CellStatus.InvalidCell);

        // get the closest cell to the map's center
        Cell centerCell = map.getCell(new Vector2Int(dimensions.x / 2, dimensions.x / 2));

        // update the status of the center cell to be valid
        map.updateCellStatus(centerCell, Cell.CellStatus.ValidCell);

        // create the open and closed sets
        Queue<Cell> openSet = new Queue<Cell>();
        HashSet<Cell> closedSet = new HashSet<Cell>();

        // put the neighbours of the center cell into the openset
        foreach (Cell neighbour in map.getNeighbours(centerCell))
        {
            openSet.Enqueue(neighbour);
        }

        // put the center cell into the closed list
        closedSet.Add(centerCell);

        // var to store current cell being evaluated
        Cell currentCell;

        // the chance that a cell is turned into a valid cell. 100% to
        // begin with
        float validCellChance = 1.0f;

        // the rate at which each additional tile reduces the valid tile chance 
        // we need this value to adjust with the terrain size. the larger the
        // terrain, the lower the falloff rate. Currently 4% rate works well.
        float chanceFalloffRate = 0.04f / (float)terrainSettings.tSize;

        // while the openset is not empty
        while (openSet.Count > 0)
        {
            // take the first cell from the open list
            currentCell = openSet.Dequeue();

            // get the neighbours of the current cell
            List<Cell> neighbours = map.getNeighbours(currentCell);

            // keep count of the number of valid and invalid tiles that
            // have been already visited.
            int visitedInvalidCellCount = 0;
            int visitedValidCellCount = 0;

            // count the number of valid and invalid visited cell
            // for each of the neighbours
            foreach (Cell neighbour in neighbours)
            {
                // if its a valid cell
                if (neighbour.status == Cell.CellStatus.ValidCell)
                {
                    // if a cell is valid, then it must have been 
                    // visited
                    // increment count
                    visitedValidCellCount++;
                }
                // otherwise its an invalid cell, check if it has
                // been visited
                else if (closedSet.Contains(neighbour))
                {
                    // it has been visited, increment count
                    visitedInvalidCellCount++;
                }
            }

            // ensure there are no visited invalid cells and there is atleast one visited
            // valid cell before attempting make the current cell valid based on random probability
            if ((visitedValidCellCount > 0 && visitedInvalidCellCount == 0 ) && UnityEngine.Random.value < validCellChance)                                                                                                                                                                                                                                                                                                                                                   
            {
               // update the cell status to a valid cell
               map.updateCellStatus(currentCell, Cell.CellStatus.ValidCell);
               // decrease the valid cell chance
               validCellChance -= chanceFalloffRate;

                // add the neighbours of the current cell to the open set 
                // if they are not already in the open or closed set
                foreach (Cell neighbour in neighbours)
                {
                    if (!openSet.Contains(neighbour) && !closedSet.Contains(neighbour))
                    {
                        openSet.Enqueue(neighbour);
                    }
                }
            }
            // add the current cell to the closed set
            closedSet.Add(currentCell);
        }
        // return the new map
        return map;
    }

    // get the dimensions for a given terrain size. rounded to the nearest
    // square within the limits of the terrain size
    // calculates the square for 1:1 length to width ratio (default)
    private Vector2Int getDimensions(int terrainSize, int ratio = 1)
    {
        // get the full square length
        double rawLength = Math.Sqrt((float)terrainSize / ratio);
        // round it to the nearest whole number. at 0.5, round up
        int length = (int)Math.Round(rawLength, MidpointRounding.AwayFromZero);
        // calculate the area using the rounded length
        int totalArea = length * length * ratio;

        // if the area is less than the minimum possible terrain size
        if (totalArea < TerrainGenerator.terrainMinSize)
        {
            // round the length up
            length = (int)Math.Ceiling(rawLength);
        }
        // if the area is more than the maximum possible terrain size
        else if (totalArea > TerrainGenerator.terrainMaxSize)
        {
           // round the length down
           length = (int)Math.Floor(rawLength);
        }

        // return the new dimensions. random orientation of width and height
        return UnityEngine.Random.value > 0.5f ? new Vector2Int(length, length * ratio) : new Vector2Int(length * ratio, length);
    }

    // set the cells for a terrain with a varying height
    private void setCellsRange(Map map)
    {
        // get a random offset to sample to sample the perlin noise map at a different position each time
        Vector2 perlinOffset = new Vector2(UnityEngine.Random.Range(0.0f, 999.0f), UnityEngine.Random.Range(0.0f, 999.0f));

        // go through the map, setting the height for all valid cells
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                // get the cell at x y
                Cell currentCell = map.getCell(x, y);
                // if the cell is a valid cell
                if (currentCell.status == Cell.CellStatus.ValidCell)
                {
                    // check if it is a boundary cell
                    checkForBoundaryCell(currentCell);

                    // calculate the new z value.
                    int zValue = calculateDepth(x, y, map.width, map.height, terrainSettings.tMinHeight, terrainSettings.tMaxHeight, perlinOffset);

                    // mark the cell as a terrain cell
                    map.updateCellStatus(currentCell, Cell.CellStatus.TerrainCell);
                    // update the cells z position
                    currentCell.position.z = zValue;
                }
            }
        }
    }

    // set the cells for a terrain with an exact height
    private void setCellsExact(Map map)
    {
        // go through the map, setting the height for all valid cells
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                // get the cell at x y
                Cell currentCell = map.getCell(x, y);

                // if the cell is a valid cell
                if (currentCell.status == Cell.CellStatus.ValidCell)
                {
                    // check if it is a boundary cell
                    checkForBoundaryCell(currentCell);
                    // mark the cell as a terrain cell
                    map.updateCellStatus( currentCell, Cell.CellStatus.TerrainCell);
                    // update the cells z position
                    currentCell.position.z = terrainSettings.tExactHeight;
                }
            }
        }
    }

    // calculates the depth of a cell at the given x y position using a perlin noise map
    private int calculateDepth(int x, int y, int width, int height, float minCellDepth, float maxCellDepth, Vector2 offset)
    {
        // normalise the x and y positions to be between 0 and 1
        float xPos = (float)x / width;
        float yPos = (float)y / height;

        // then apply scale
        xPos *= perlinScale;
        yPos *= perlinScale;

        // and add random offset to get a different section of the noise each generation
        xPos += offset.x;
        yPos += offset.y;

        // perlin should return value between 0.0f and 1.0f, but the doc says it
        // may return values slightly outside the bounds, so clamp it
        float perlinNoiseValue = Mathf.Clamp(Mathf.PerlinNoise(xPos, yPos), 0.0f, 1.0f);

        // change the value range of the perlin noise value from 0.0-1.0 to an integer between terrainMinHeight and terrainMaxHeight
        int zValue = (int)Math.Round((perlinNoiseValue * (maxCellDepth - minCellDepth)) + minCellDepth, MidpointRounding.AwayFromZero);

        // return the value
        return zValue;
    }

    /// <summary>
    /// Finds all the outer boundarys of the terrain. Used to limit player movement to only be on the
    /// generated level.
    /// </summary>
    /// <param name="map">A reference to the Map object, which represents the state of all cells in the level.</param>
    /// <param name="positions">List of outer bound positions for the first outer bounds tilemap.</param>
    /// <param name="tiles">List of outer bound tiles for the first outer bounds tilemap.</param>
    /// <param name="positions2">List of outer bound positions for the second outer bounds tilemap.</param>
    /// <param name="tiles2">List of outer bound tiles for the second outer bounds tilemap.</param>
    public void setOuterBounds(Map map, List<Vector3Int> positions, List<TileBase> tiles, List<Vector3Int> positions2, List<TileBase> tiles2)
    {
        // add the generation step to the info list
        generationInfo.Add("Finding and setting the outer bounds of the map based on the boundary cells");

        // for each of the boundary cell positions
        foreach (Vector2Int boundaryCellPosition in map.getBoundaryCellPositions())
        {
            // if x+1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.x + 1 >  map.width-1 || map.getCell(boundaryCellPosition.x + 1, boundaryCellPosition.y).status == Cell.CellStatus.InvalidCell)
            {
                // get the outer bound position and give it a depth equal to its neighbour boundary cell
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x + 1, boundaryCellPosition.y, map.getCell(boundaryCellPosition).position.z);
                // as long as the position is not already in any list
                if (!positions2.Contains(outerBoundPosition) && !positions.Contains(outerBoundPosition))
                {
                    // add it to the second list
                    positions2.Add(outerBoundPosition);
                    tiles2.Add(outerBoundsTile);

                }
            }

            // if x-1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.x - 1 < 0 || map.getCell(boundaryCellPosition.x - 1, boundaryCellPosition.y).status == Cell.CellStatus.InvalidCell)
            {
                // get the outer bound position and give it a depth equal to its neighbour boundary cell
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x - 1, boundaryCellPosition.y, map.getCell(boundaryCellPosition).position.z);
                // as long as the position is not already in any list
                if (!positions.Contains(outerBoundPosition) && !positions2.Contains(outerBoundPosition))
                {
                    // add it to the first list
                    positions.Add(outerBoundPosition);
                    tiles.Add(outerBoundsTile);
                }
            }

            // if y+1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.y + 1 > map.height - 1 || map.getCell(boundaryCellPosition.x, boundaryCellPosition.y+1).status == Cell.CellStatus.InvalidCell)
            {
                // get the outer bound position and give it a depth equal to its neighbour boundary cell
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x, boundaryCellPosition.y+1, map.getCell(boundaryCellPosition).position.z);
                // as long as the position is not already in any list
                if (!positions2.Contains(outerBoundPosition) && !positions.Contains(outerBoundPosition))
                {
                    // add it to the second list
                    positions2.Add(outerBoundPosition);
                    tiles2.Add(outerBoundsTile);

                }
            }

            // if x-1 is out the bounds of the array or its an invalid cell, its an outer bound
            if (boundaryCellPosition.y - 1 < 0 || map.getCell(boundaryCellPosition.x, boundaryCellPosition.y-1).status == Cell.CellStatus.InvalidCell)
            {
                // get the outer bound position and give it a depth equal to its neighbour boundary cell
                Vector3Int outerBoundPosition = new Vector3Int(boundaryCellPosition.x, boundaryCellPosition.y-1, map.getCell(boundaryCellPosition).position.z);
                // as long as the position is not already in any list
                if (!positions.Contains(outerBoundPosition) && !positions2.Contains(outerBoundPosition))
                {
                    // add it to the second list
                    positions.Add(outerBoundPosition);
                    tiles.Add(outerBoundsTile);
                }
            }
        }
    }
}
