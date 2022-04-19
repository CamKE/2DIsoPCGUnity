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

    // the levelCameraController component of the player
    [SerializeField]
    private LevelCameraController levelCameraController;

    // sprites packed for more efficient use
    [SerializeField]
    private SpriteAtlas atlas;

    // the playerController component of the player
    private PlayerController playerController;

    // whether or not the player character has been instantiated
    private bool playerIsInstantiated;

    private TerrainGenerator terrainGenerator;

    private RiverGenerator riverGenerator;

    private LakeGenerator lakeGenerator;

    private bool rangeHeightEnabled;

    // the status of each cell in a grid of cells
    public enum levelCellStatus { validCell, invalidCell, terrainCell, lakeCell, riverCell, outOfBounds }

    // a 3-dimensional array of cells in the level, denoting the status of each cell
    private levelCellStatus[,,] levelCells;

    private Vector3Int currentCell;
    private Vector3 previousPosition;

    /// <summary>
    ///  whether or not a level is generated
    /// </summary>
    public bool levelisGenerated { get; private set; }
    
    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // set the offset of the tile center to a value relative to the center position of the sprites (0.205)
        // , given sprite sizes are normalised to 1x1

        // ensure the playerIsInstantiated bool to false by default
        playerIsInstantiated = false;
        // ensure the isGenerated bool is false by default
        levelisGenerated = false;

        currentCell = Vector3Int.zero;
        initialSetup();
    }

    // update is called every frame when the script is enabled
    private void Update()
    {
        // if there is a player character
        if (playerIsInstantiated)
        {
            if(rangeHeightEnabled)
            {

                // float playerZValue = calculatePlayerZValue();

                //  playerController.MoveCharacter(playerZValue);

                playerController.moveCharacter();
                Vector3Int gridpos = grid.WorldToCell(playerController.playerGridPosition);
                playerController.setStartPosition(riverGenerator.grid[gridpos.x, gridpos.y].position.z);

            }
            else
            {
                playerController.MoveCharacter();
            }

        }    
    }

    // work out what the z value for the player should be based on their current position
    // to ensure they are ordered correctly relative to the other sprites
    private float calculatePlayerZValue()
    {
        // work out what the player's z value should be
        Vector3 playerPosition = playerController.getPosition();

        if(!previousPosition.x.Equals(playerPosition.x) && !previousPosition.y.Equals(playerPosition.y))
        {
            previousPosition = playerController.getPosition();

            Vector3Int gridCell = grid.WorldToCell(new Vector3(playerPosition.x, playerPosition.y, 0));
            Debug.Log(gridCell);
            currentCell = riverGenerator.grid[gridCell.x, gridCell.y].position;

        // the grid cell the player is on 
        //var cellPos = grid.WorldToCell(new Vector3(playerController.transform.position.x, playerController.transform.position.y,0));
        int x = Mathf.Clamp(currentCell.x, 0, levelCells.GetLength(0)-1);
        int y = Mathf.Clamp(currentCell.y, 0, levelCells.GetLength(1)-1);

        for (int z = levelCells.GetLength(2)-1; z > -1; z--)
        {
            if (levelCells[x,y,z] == levelCellStatus.terrainCell)
            {
                currentCell = new Vector3Int(x, y, z);
                return z + 0.5f;
            }
        }

        }
        else
        {
            return playerPosition.z;
        }
        /*
         
        // go through and find z value of the tile which exists in the x,y position of the player
        // can flip the for loop to ensure tile which the highest z is found first
        for (int terrainZValue = TerrainGenerator.terrainMinHeight; terrainZValue <= TerrainGenerator.terrainMaxHeight; terrainZValue++)
        {
            // set the cells z to be the z to be checked
            cellPos.z = terrainZValue;
            // if there is a tile at the z to be checked
            if (terrainTilemap.GetTile(cellPos) != null)
            {
                // we have found the z value for the tile the player is on
                return terrainZValue +0.5f;
            }
        }

        */

        // no tile found, return invalid value
        return -1;
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
    public void generate(TerrainOptions terrainOptions, RiverGenerator.RiverUserSettings riverUserSettings, LakeGenerator.LakeUserSettings lakeUserSettings)
    {
        // clear the level tilemaps
        clearLevel();

        rangeHeightEnabled = terrainOptions.isRangedHeightEnabled();

        levelCells = terrainGenerator.createLevelCells(terrainOptions);

        // populate the levelCells 3d array with the terrain cells
        terrainGenerator.populateCells(terrainOptions, levelCells);

        // if river generation is enabled
        if (riverUserSettings.generationEnabled)
        {
            // populate the levelCells 3d array with the river cells
            riverGenerator.populateCells(riverUserSettings, levelCells, terrainGenerator.terrainCellList, terrainGenerator.boundaryCellList);
        }

        // if lake generation is enabled
        if (lakeUserSettings.generationEnabled)
        {
            // populate the levelCells 3d array with the lake cells
            lakeGenerator.populateCells(lakeUserSettings, levelCells);
        }

        // generate the terrain based on the current state of the levelCells array
        terrainGenerator.generate(levelCells);

        // if river generation is enabled
        if (riverUserSettings.generationEnabled)
        {
            // populate the levelCells 3d array with the river cells
            riverGenerator.generate(levelCells);
        }

        // if lake generation is enabled
        if (lakeUserSettings.generationEnabled)
        {
            // populate the levelCells 3d array with the lake cells
            lakeGenerator.generate(levelCells);
        }

        /*
        var tile = ScriptableObject.CreateInstance<Tile>();
        var tile2 = ScriptableObject.CreateInstance<Tile>();

        tile.sprite = atlas.GetSprite("ISO_Tile_Brick_Brick_02");
        tile2.sprite = atlas.GetSprite("ISO_Tile_Dirt_01_Grass_01");

        tile.colliderType = Tile.ColliderType.Grid;
        tile2.colliderType = Tile.ColliderType.Grid;

        Vector3Int[] positions = new Vector3Int[size.x * size.y];
        TileBase[] tileArray = new TileBase[positions.Length];

        
        for (int index = 0; index < positions.Length; index++)
        {
            positions[index] = new Vector3Int(index % size.x, index / size.y, UnityEngine.Random.Range(0, 2));
            tileArray[index] = index % 2 == 0 ? tile : tile2;
        }
        

        terrainTilemap.SetTiles(positions, tileArray);
        */

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

    /// <summary>
    /// Create the player and put them into the level at the given position.
    /// </summary>
    /// <param name="position">Where the player should be placed on the level</param>
    public void setupPlayer()
    {
        // if the player is not instantiated
        if (!playerIsInstantiated)
        {
            // Instatiate the player
            player = Instantiate(player, new Vector3(2, 2, 3.01f), Quaternion.identity);
            // store the ref to the player component playerController
            playerController = player.GetComponent<PlayerController>();
            // player is now instantiated, set the bool to true
            playerIsInstantiated = true;
        } else
        // otherwise
        {
            // player is instantiated, so enable the player object
            player.SetActive(true);
        }

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
                        currentCell = new Vector3Int(cellPosition.x, cellPosition.y, z);
                        playerController.setInitialGridPosition(grid.CellToWorld(new Vector3Int(1, 0, 0)));
                        playerController.setStartPosition(riverGenerator.grid[1,0].position.z);
                        
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
    }

    // runs before the application is quit 
    private void OnApplicationQuit()
    {

        // if the player exists
        if (playerIsInstantiated)
        {
            // delete the player
            Destroy(player);
        }

    }
}
