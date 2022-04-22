using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEditor;
using System;

/// <summary>
/// Responsible for all operations to do with creating, managing and destroying the level and elements within it.
/// </summary>
public class LevelManager : MonoBehaviour
{
    // '[SerializeField] private' show up in the inspector but are not accessible by other scripts

    // gameobject with grid component for aligning tiles
    [SerializeField]
    private Grid grid;

    // player character to be used in demo mode
    [SerializeField]
    private GameObject player;

    // the levelCameraController component
    private LevelCameraController levelCameraController;

    // sprites packed for more efficient use
    [SerializeField]
    private SpriteAtlas atlas;

    [SerializeField]
    private Level level;

    // the playerController component of the player
    private PlayerController playerController;

    private TerrainGenerator terrainGenerator;

    private RiverGenerator riverGenerator;

    private LakeGenerator lakeGenerator;

    private bool rangeHeightEnabled;
    private bool demoModeEnabled;

    // the status of each cell in a grid of cells
    public enum levelCellStatus { validCell, invalidCell, terrainCell, lakeCell, riverCell, outOfBounds }

    // a 3-dimensional array of cells in the level, denoting the status of each cell
    private levelCellStatus[,,] levelCells;

    /// <summary>
    ///  whether or not a level is generated
    /// </summary>
    public bool levelisGenerated { get; private set; }
    
    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // set the offset of the tile center to a value relative to the center position of the sprites (0.205)
        // , given sprite sizes are normalised to 1x1

        // ensure the isGenerated bool is false by default
        levelisGenerated = false;
        demoModeEnabled = false;

        levelCameraController = level.transform.GetChild(0).GetComponent<LevelCameraController>();

        initialSetup();
    }

    // update is called every frame when the script is enabled
    private void Update()
    {
        // if there is a player character
        if (demoModeEnabled && isPlayerInstatiated())
        {
            if(rangeHeightEnabled)
            {

                // float playerZValue = calculatePlayerZValue();

                //  playerController.MoveCharacter(playerZValue);
                Vector3Int gridpos = grid.WorldToCell(playerController.getWorldPosition());
                playerController.movePlayer(riverGenerator.grid[gridpos.x, gridpos.y].position.z);
            }
            else
            {
                playerController.movePlayer();
            }

        }    
    }

    /// <summary>
    /// Clear the tiles in all terrainTilemaps in the level.
    /// </summary>
    public void clearLevel()
    {
        // terrainTilemaps are not deleted even if they are not used, as it
        // is more efficient to keep them instead of continuously creating
        // and deleting them

        // clear the current terrainTilemap (temporary setup)
        //terrainTilemap.ClearAllTiles();
        terrainGenerator.clearTilemap();
        riverGenerator.clearTilemap();
        // set is generated to be false
        levelisGenerated = false;
    }

    

    /// <summary>
    /// Generate the level. temporary setup.
    /// </summary>
    public void generate(TerrainOptions.TerrainSettings terrainSettings, RiverOptions.RiverSettings riverSettings, LakeOptions.LakeSettings lakeSettings)
    {
        // clear the level tilemaps
        clearLevel();

        rangeHeightEnabled = terrainSettings.heightRangedEnabled;

        terrainGenerator.setTerrainSettings(terrainSettings);

        levelCells = terrainGenerator.createLevelCells();

        // populate the levelCells 3d array with the terrain cells
        terrainGenerator.populateCells(levelCells);

        // if river generation is enabled
        if (riverSettings.rGenerationEnabled)
        {
            riverGenerator.setRiverSettings(riverSettings);
            // populate the levelCells 3d array with the river cells
            riverGenerator.populateCells(levelCells, terrainGenerator.terrainCellList, terrainGenerator.boundaryCellList);
        }

        // if lake generation is enabled
        if (lakeSettings.lGenerationEnabled)
        {
            lakeGenerator.setLakeSettings(lakeSettings);
            // populate the levelCells 3d array with the lake cells
            lakeGenerator.populateCells(levelCells);
        }

        // generate the terrain based on the current state of the levelCells array
        terrainGenerator.generate(levelCells);

        // if river generation is enabled
        if (riverSettings.rGenerationEnabled)
        {
            // populate the levelCells 3d array with the river cells
            riverGenerator.generate(levelCells);
        }

        // if lake generation is enabled
        if (lakeSettings.lGenerationEnabled)
        {
            // populate the levelCells 3d array with the lake cells
            lakeGenerator.generate(levelCells);
        }

        updateLevelCamera(terrainGenerator.getTilemapBounds());
        levelisGenerated = true;
    }

    // gives the camera the new center of the level and the new orthographic size
    private void updateLevelCamera(BoundsInt tilemapBounds)
    {
        // get the boundary information of the terrain terrainTilemap
        BoundsInt terrainBounds = tilemapBounds;
        
        // find the local position of the cell on the grid at the x most tile value and the 
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

        // update the camera with the new values
        levelCameraController.updateCamera(levelCenter, newOrthoSize);
    }

    // the initial setup for the level. temporary setup
    private void initialSetup()
    {
        terrainGenerator = new TerrainGenerator(grid, atlas);

        riverGenerator = new RiverGenerator(grid, atlas);

        lakeGenerator = new LakeGenerator();
        //setupPlayer(terrainTilemap.GetCellCenterWorld(Vector3Int.zero) + tileCenterOffset);
    }

    // whether or not the player character has been instantiated
    // had a bool var before, but this seems to make more sense, as we cannot
    // store the playercontroller before instatiation as the instance controller 
    // will be different from the one before instantiation, therefore by default
    // we know the player is instantiated if its playercontroller var is set
    private bool isPlayerInstatiated()
    {
        return playerController != null ? true : false;
    }

    /// <summary>
    /// Create the player and put them into the level at the given position.
    /// </summary>
    /// <param name="position">Where the player should be placed on the level</param>
    public void setupPlayer()
    {
        // if the player is not instantiated
        if (!isPlayerInstatiated())
        {
            // Instatiate the player
            player = Instantiate(player, new Vector3(2, 2, 3.01f), Quaternion.identity);

            // store the ref to the player component playerController
            playerController = player.GetComponent<PlayerController>();
            Debug.Log(isPlayerInstatiated());

        }

        // make sure the player object is enabled
        setPlayerActive(true);

        bool cellFound = false;
        while (!cellFound)
        {
            Vector2Int cellPosition = new Vector2Int(UnityEngine.Random.Range(0, levelCells.GetLength(0)), UnityEngine.Random.Range(0, levelCells.GetLength(1)));
            
            for (int z = 0; z < levelCells.GetLength(2); z++)
            {
                if (levelCells[cellPosition.x,cellPosition.y,z] == levelCellStatus.terrainCell)
                {
                    if ((z+1) == levelCells.GetLength(2) || levelCells[cellPosition.x, cellPosition.y, z + 1] == levelCellStatus.validCell)
                    {
                        playerController.setWorldPosition(grid.CellToWorld(new Vector3Int(cellPosition.x, cellPosition.y, z)));
                        playerController.updatePlayerPosition(riverGenerator.grid[1,0].position.z);
                        
                        //playerController.setPosition(grid.CellToWorld(currentCell));
                        cellFound = true;
                    }
                    break;
                }
            }
        }
        // set the players intial position on the level
 
    }

    /// <summary>
    /// Used by the UI manager to update the level camera's active status when swapping between
    /// level generation and demo user interfaces
    /// </summary>
    /// <param name="value">The boolean value to set the level camera's active status to.</param>
    public void setLevelCameraActive(bool value)
    {
        // set the level camera's active status to the given value
        levelCameraController.gameObject.SetActive(value);
    }

    /// <summary>
    /// Used by the UI manager to update the player's active status when swapping between
    /// level generation and demo user interfaces
    /// </summary>
    /// <param name="value">The boolean value to set the player's active status to.</param>
    public void setPlayerActive(bool value)
    {
        // set the player's active status to the given value
        player.SetActive(value);
        demoModeEnabled = value;
    }

    // runs before the application is quit 
    private void OnApplicationQuit()
    {

        // if the player exists
        if (isPlayerInstatiated())
        {
            // delete the player
            Destroy(player);
        }

    }
}
