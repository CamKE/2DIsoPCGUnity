using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEditor;

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


    [SerializeField][HideInInspector]
    private Tilemap tilemap;

    // sprites packed for more efficient use
    [SerializeField]
    private SpriteAtlas atlas;


    [SerializeField]
    private Vector2Int size;

    // the playerController component of the player
    private PlayerController playerController;

    // whether or not the player character has been instantiated
    private bool playerIsInstantiated;

    /// <summary>
    ///  whether or not a level is generated
    /// </summary>
    public bool levelisGenerated { get; private set; }


    // i use this instead of taking the grid center to world, as the grid center looks off. instead i take the tilemap tile center
    // which uses the pivot as the center. then i add an offset to the y axis to move the tile center to the middle of the tile.
    private Vector3 tileCenterOffset;

    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // set the offset of the tile center to a value relative to the center position of the sprites (0.205)
        // , given sprite sizes are normalised to 1x1
        tileCenterOffset = new Vector3(0,0.7f,0);

        // ensure the playerIsInstantiated bool to false by default
        playerIsInstantiated = false;
        // ensure the isGenerated bool is false by default
        levelisGenerated = false;
    }

    // update is called every frame when the script is enabled
    private void Update()
    {
        // if there is a player character
        if (playerIsInstantiated)
        {
            // work out what the player's z value should be
            int playerZValue = calculatePlayerZValue();

            // allow the user to control the player character, 
            // and give the character the new height
            playerController.MoveCharacter(playerZValue);
        }    
    }

    // work out what the z value for the player should be based on their current position
    // to ensure they are ordered correctly relative to the other sprites
    private int calculatePlayerZValue()
    {
        // the grid cell the player is on 
        var cellPos = tilemap.WorldToCell(new Vector3(playerController.transform.position.x, playerController.transform.position.y,0));

        // go through and find z value of the tile which exists in the x,y position of the player
        // can flip the for loop to ensure tile which the highest z is found first
        for (int terrainZValue = 0; terrainZValue <= 2; terrainZValue++)
        {
            // set the cells z to be the z to be checked
            cellPos.z = terrainZValue;
            // if there is a tile at the z to be checked
            if (tilemap.GetTile(cellPos) != null)
            {
                // we have found the z value for the tile the player is on
                return terrainZValue;
            }
        }

        // no tile found, return invalid value
        return -1;
    }

    /// <summary>
    /// Clear the tiles in all tilemaps in the level.
    /// </summary>
    public void clearLevel()
    {
        // tilemaps are not deleted even if they are not used, as it
        // is more efficient to keep them instead of continuously creating
        // and deleting them

        // clear the current tilemap (temporary setup)
        tilemap.ClearAllTiles();
        // set is generated to be false
        levelisGenerated = false;
    }

    /// <summary>
    /// Generate the level. temporary setup.
    /// </summary>
    public void generate()
    {
        if (tilemap == null)
        {
            initialSetup();
        } else
        {
            clearLevel();
        }
      
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
            positions[index] = new Vector3Int(index % size.x, index / size.y, Random.Range(0, 2));
            tileArray[index] = index % 2 == 0 ? tile : tile2;
        }
        

        tilemap.SetTiles(positions, tileArray);

        updateLevelCamera();
        levelisGenerated = true;
    }

    // gives the camera the new center of the level and the new orthographic size
    private void updateLevelCamera()
    {
        // get the boundary information of the terrain tilemap
        BoundsInt terrainBounds = tilemap.cellBounds;
        
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

        tilemap = new GameObject("Terrain").AddComponent<Tilemap>();

        tilemap.gameObject.AddComponent<TilemapRenderer>();
        tilemap.transform.SetParent(grid.gameObject.transform);
        tilemap.tileAnchor = new Vector3(0, 0, -2);

        var gridComponent = grid.GetComponent<Grid>();

        gridComponent.cellSize = new Vector3(1, 0.5f, 1);
        gridComponent.cellLayout = GridLayout.CellLayout.IsometricZAsY;

        var tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();

        tilemapRenderer.mode = TilemapRenderer.Mode.Individual;

        //setupPlayer(tilemap.GetCellCenterWorld(Vector3Int.zero) + tileCenterOffset);
    }

    /// <summary>
    /// Create the player and put them into the level at the given position.
    /// </summary>
    /// <param name="position">Where the player should be placed on the level</param>
    public void setupPlayer(Vector3 position)
    {
        // Instatiate the player
        player = Instantiate(player, new Vector3(2, 2, 3.01f), Quaternion.identity);
        // store the ref to the player component playerController
        playerController = player.GetComponent<PlayerController>();
        // set the players intial position on the level
        playerController.setPosition(position);
        // player is now instantiated, set the bool to true
        playerIsInstantiated = true;
    }

    // runs before the application is quit 
    private void OnApplicationQuit()
    {
        // clear the level
        clearLevel();

        // if the player exists
        if (playerIsInstantiated)
        {
            // delete the player
            Destroy(player);
        }

    }
}
