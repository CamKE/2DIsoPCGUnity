using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.Diagnostics;
using UnityEngine.Tilemaps;
using System;

/// <summary>
/// Handles all aspects to do with the level.
/// </summary>
public class Level : MonoBehaviour
{
    // a 2D representation of the level
    private Map levelMap;

    // generators for terrain, river, lake and walkpath
    private TerrainGenerator terrainGenerator;
    private RiverGenerator riverGenerator;
    private LakeGenerator lakeGenerator;
    private WalkpathGenerator walkpathGenerator;

    // sprites packed for more efficient use
    [SerializeField]
    private SpriteAtlas atlas;

    // gameobject with grid component for aligning tiles
    private Grid grid;

    // the controller for the level camera
    private LevelCameraController cameraController;

    // all the tilemaps that make up the level
    [SerializeField]
    private Tilemap[] tilemaps;

    // the position lists for the tilemaps
    private List<Vector3Int>[] positions;

    // the tile lists for the tilemaps
    private List<TileBase>[] tiles;

    // information about the level generation steps
    private List<string> generationInfo;

    // the different tilemaps of the level
    private enum TilemapNames { Terrain, TerrainOuterBounds1, TerrainOuterBounds2, Water }
    // the number of tilemap names
    private readonly int tilemapNamesCount = Enum.GetValues(typeof(TilemapNames)).Length;

    // whether or not a call is from the inspector
    private bool fromInspector;

    // the height of a single tile
    private const int tileHeight = 3;

    // Start is called before the first frame update
    void Start()
    {
        // setup the level
        setup();
    }

    /// <summary>
    /// Used to setup the level and its components.
    /// </summary>
    /// <param name="fromInspector">Whether or not a call is from the inspector.</param>
    public void setup(bool fromInspector = false)
    {
        // get a ref to the grid component
        grid = transform.GetComponent<Grid>();

        // create the levelgen info
        generationInfo = new List<string>();

        // create the generators
        terrainGenerator = new TerrainGenerator(atlas, generationInfo);
        riverGenerator = new RiverGenerator(atlas, generationInfo);
        lakeGenerator = new LakeGenerator(atlas, generationInfo);
        walkpathGenerator = new WalkpathGenerator(atlas, generationInfo);

        // create array of tilemap elements
        positions = new List<Vector3Int>[tilemapNamesCount];
        tiles = new List<TileBase>[tilemapNamesCount];

        // set each tilemap element to a new instance
        for (int x = 0; x < tilemapNamesCount; x++)
        {
            positions[x] = new List<Vector3Int>();
            tiles[x] = new List<TileBase>();
        }

        // only do these steps if the call is not from the inspector
        if (!fromInspector)
        {
            // get the level camera controller
            cameraController = transform.GetChild(0).GetComponent<LevelCameraController>();
            // disable it by default
            cameraController.enabled = false;
        }

        // set the fromInspector bool
        this.fromInspector = fromInspector;
    }

    /// <summary>
    /// Set the active status of the level camera.
    /// </summary>
    /// <param name="value">The value to set the level cameras active status to.</param>
    public void setCameraActive(bool value)
    {
        cameraController.gameObject.SetActive(value);
    }

    /// <summary>
    /// Responsible for generating the level. It uses the settings to determine how the level is generated.
    /// </summary>
    /// <param name="terrainSettings">The settings for the terrain generator.</param>
    /// <param name="riverSettings">The settings for the river generator.</param>
    /// <param name="lakeSettings">The settings for the lake generator.</param>
    /// <param name="walkpathSettings">The settings for the walkpath generator.</param>
    public void generate(TerrainSettings terrainSettings, RiverSettings riverSettings, LakeSettings lakeSettings, WalkpathSettings walkpathSettings)
    {
        // clear the level
        clear();

        // time the generation
        Stopwatch sw = new Stopwatch();
        sw.Start();

        // add the generation heading to the info list
        generationInfo.Add("Terrain Generator:");
        // set the terrain settings in the terrain generator
        terrainGenerator.setTerrainSettings(terrainSettings);

        // create the level map based on the terrain shape setting
        levelMap = terrainGenerator.createMap();
        // populate the map with terrain cells 
        terrainGenerator.populateCells(levelMap);
        // set the the outer bounds to create the map boundary
        terrainGenerator.setOuterBounds(levelMap, ref positions[(int)TilemapNames.TerrainOuterBounds2], ref tiles[(int)TilemapNames.TerrainOuterBounds2], ref positions[(int)TilemapNames.TerrainOuterBounds1], ref tiles[(int)TilemapNames.TerrainOuterBounds1]);

        // if lake generation is enabled
        if (lakeSettings.lGenerationEnabled)
        {
            // add the generation heading to the info list
            generationInfo.Add("\n Lake Generator:");
            // set the lake settings in the lake generator
            lakeGenerator.setLakeSettings(lakeSettings);
            // populate the map with lake cells 
            lakeGenerator.populateCells(levelMap);
        }

        // if river generation is enabled
        if (riverSettings.rGenerationEnabled)
        {
            // add the generation heading to the info list
            generationInfo.Add("\n River Generator:");
            // set the river settings in the river generator
            riverGenerator.setRiverSettings(riverSettings);
            // populate the map with river cells 
            riverGenerator.populateCells(levelMap);
        }

        // if walkpath generation is enabled
        if (walkpathSettings.wGenerationEnabled)
        {
            // add the generation heading to the info list
            generationInfo.Add("\n Walkpath Generator:");
            // set the walkpath settings in the walkpath generator
            walkpathGenerator.setWalkpathSettings(walkpathSettings);
            // populate the map with walkpath cells 
            walkpathGenerator.populateCells(levelMap);
        }

        // add the generation heading to the info list
        generationInfo.Add("\n Set Tilemaps:");
        // set the tilemaps according to the maps state
        setTilemaps();

        // stop the level generation timer
        sw.Stop();
        // add the level generation time to the info list
        generationInfo.Add("Level generated in " + sw.ElapsedMilliseconds + " ms");

        // only do these steps if the call is not from the inspector
        if (!fromInspector)
        {
            // update the levels camera
            updateCamera();
        }
    }

    // goes through the map, checking the tile type and sets the corresponding tilemap according
    // to the information on the map
    private void setTilemaps()
    {
        // add the generation step to the info list
        generationInfo.Add("Setting the tiles in the tilemap based on the map object populated");

        // terrain tiles
        Tile[] groundTiles = terrainGenerator.getGroundTiles();
        Tile[] accessoryTiles = terrainGenerator.getAccessoryTiles();
        Tile lowerGroundTile = terrainGenerator.getLowerGroundTile();
        // river tile
        Tile riverTile = riverGenerator.getTile();
        // walkpath tile
        Tile walkpathTile = walkpathGenerator.getTile();
        // lake tile
        Tile lakeTile = lakeGenerator.getTile();

        // temporary variables used during setting tilemaps
        Tile currentTile = null;
        int currentTilemapIndex = -1;
        bool setTile;

        // go through all cells in the map
        for (int x = 0; x < levelMap.width; x++)
        {
            for (int y = 0; y < levelMap.height; y++)
            {
                // get the cell at the x,y position
                Cell currentCell = levelMap.getCell(x, y);
                // get the cells position
                Vector3Int currentCellPosition = currentCell.position;
                // we want to set the tile being searched (by default)
                setTile = true;

                // check the cell status
                switch (currentCell.status)
                {
                    // if its a terrain cell
                    case Cell.CellStatus.TerrainCell:
                        // set the terrain tile to use for the cell on the map. select random accessory tile at 30% chance
                        currentTile = UnityEngine.Random.value > 0.2f ? groundTiles[currentCellPosition.z % groundTiles.Length] : accessoryTiles[currentCellPosition.x % accessoryTiles.Length];
                        // set the index to the terrain tilemap
                        currentTilemapIndex = (int)TilemapNames.Terrain;
                        break;
                    // if its a river cell
                    case Cell.CellStatus.RiverCell:
                        // set the river tile to use for the cell on the map
                        currentTile = riverTile;
                        // set the index to the water tilemap
                        currentTilemapIndex = (int)TilemapNames.Water;
                        // set river tile to be 1 lower than the lowest depth terrain neighbour
                        currentCellPosition.z = levelMap.getTerrainMinDepth(currentCell) - 1;
                        break;
                    // if its a lake cell
                    case Cell.CellStatus.LakeCell:
                        // set the lake tile to use for the cell on the map
                        currentTile = lakeTile;
                        // set the index to the water tilemap
                        currentTilemapIndex = (int)TilemapNames.Water;
                        // set lake tile z value 1 below its current value
                        currentCellPosition.z -= 1;
                        break;
                    // if its a walkpath cell
                    case Cell.CellStatus.WalkpathCell:
                        // set the walkpath tile to use for the cell on the map
                        currentTile = walkpathTile;
                        // set the index to the terrain tilemap
                        currentTilemapIndex = (int)TilemapNames.Terrain;
                        break;
                    default:
                        // none of the cell statuses above, we do not want to set a tile for it
                        setTile = false;
                        break;
                }

                // if we are setting the tile for the cell
                if (setTile)
                {
                    // add the position of the cell to the corresponding tilemap
                    positions[currentTilemapIndex].Add(currentCellPosition);
                    // add the tile to the corresponding tilemap
                    tiles[currentTilemapIndex].Add(currentTile);
                    // set terrain tiles for the cells below the current cell. only done if it is a boundary cell
                    setTerrainTilesBelowBoundary(currentCell, lowerGroundTile);
                }
            }
        }

        // for each tilemap in the array of tilemaps
        for (int index = 0; index < tilemapNamesCount; index++)
        {
            // set the corresponding positions array and tiles array
            tilemaps[index].SetTiles(positions[index].ToArray(), tiles[index].ToArray());
        }
    }

    // set terrain tiles for the cells below the current cell if it is a boundary cell
    private void setTerrainTilesBelowBoundary(Cell cell, Tile tile)
    {
        // the index for the terrain tilemap
        int tilemapIndex = (int)TilemapNames.Terrain;

        // the position of the cell
        Vector3Int position = cell.position;

        // if the cell is a boundary cell and its depth is greater than 0 
        if (cell.onBoundary && position.z > 0)
        {
            // there is space below the cell, calculate the depth of the cell 
            // to be placed directly below the current cell
            int cellBelowZValue = (position.z % 3);

            // place tile at and below the cell below
            for (int z = cellBelowZValue == 0 ? position.z - tileHeight : position.z - cellBelowZValue; z >= 0; z -= tileHeight)
            {
                positions[tilemapIndex].Add(new Vector3Int(position.x, position.y, z));
                tiles[tilemapIndex].Add(tile);
            }
        }
    }

    // gives the camera the new center of the level and the new orthographic size
    private void updateCamera()
    {
        // enable the controller now that a level is generated
        cameraController.enabled = true;

        // get the bounds of the terrain
        BoundsInt terrainBounds = tilemaps[(int)TilemapNames.Terrain].cellBounds;

        // find the local position (relative to the grid) of the cell on the grid at the x most tile value and the 
        // y most tile value
        Vector3 mapLocalDimension = grid.CellToLocal(new Vector3Int(terrainBounds.xMax, terrainBounds.yMax, 0));

        // find the world position of the center of the level
        Vector3 levelCenter = grid.LocalToWorld(mapLocalDimension / 2.0f);
        // set the z value to ensure the level is always in view
        levelCenter.z = -50.0f;

        // set the max dimension to be the largest dimension of the terrain
        int maxDimension = terrainBounds.xMax > terrainBounds.yMax ? terrainBounds.xMax : terrainBounds.yMax;

        // set the new camera orthographic size to be 40% of the maximum terrain dimension
        float newOrthoSize = 0.4f * maxDimension;

        // give the camera the new settings
        cameraController.updateCamera(levelCenter, newOrthoSize);
    }

    /// <summary>
    /// Get a random terrain cell position from the map.
    /// </summary>
    /// <returns>The position of a terrain cell.</returns>
    public Vector3Int getRandomTerrainCellPosition()
    {
        return levelMap.getRandomTerrainCellPosition();
    }

    /// <summary>
    /// Get the world position of a cell on the map.
    /// </summary>
    /// <param name="cellPosition">The cell to get the world position for.</param>
    /// <returns>The world position of the cell.</returns>
    public Vector2 getGridWorldPosition(Vector2Int cellPosition)
    {
        return grid.CellToWorld((Vector3Int)cellPosition);
    }

    /// <summary>
    /// Get all the level generation information.
    /// </summary>
    /// <returns>A list of level generation steps.</returns>
    public List<string> getGenerationInfo()
    {
        return generationInfo;
    }

    /// <summary>
    /// Clear the level and its componenets.
    /// </summary>
    public void clear()
    {
        // clear all tilemaps
        foreach (Tilemap tilemap in tilemaps)
        {
            tilemap.ClearAllTiles();
        }

        // clear all tilemap elements
        for (int x = 0; x < tilemapNamesCount; x++)
        {
            positions[x].Clear();
            tiles[x].Clear();
        }

        // clear the level gen info
        generationInfo.Clear();
    }

    /// <summary>
    /// Get the z position (depth) at the x,y world position on the map
    /// </summary>
    /// <param name="worldPos">The world position to find the z value for.</param>
    /// <returns>The z position (depth) of the world position.</returns>
    public int getMapZPosition(Vector2 worldPos)
    {
        // get the cell position on the level grid
        Vector3Int cellPosOnGrid = grid.WorldToCell(worldPos);

        // ensure the cellPosition stays within the bounds of the map.
        cellPosOnGrid.Clamp(new Vector3Int(0, 0, 0), new Vector3Int(levelMap.width - 1, levelMap.height - 1, TerrainGenerator.terrainMaxHeight));

        // return the z position at the cell position
        return levelMap.getCell((Vector2Int)cellPosOnGrid).position.z;
    }
}
