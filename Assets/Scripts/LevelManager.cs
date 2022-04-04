using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEditor;


public class LevelManager : MonoBehaviour
{

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Camera levelCamera;

    [SerializeField][HideInInspector]
    private Tilemap tilemap;

    [SerializeField][HideInInspector]
    private SpriteAtlas atlas;

    [SerializeField]
    private Vector2Int size;

    private PlayerController playerController;

    private LevelCameraController levelCameraController;


    // i use this instead of taking the grid center to world, as the grid center looks off. instead i take the tilemap tile center
    // which uses the pivot as the center. then i add an offset to the y axis to move the tile center to the middle of the tile.
    private Vector3 tileCenterOffset;

    // Start is called before the first frame update
    void Start()
    {
        tileCenterOffset = new Vector3(0,0.7f,0);
        levelCameraController = levelCamera.GetComponent<LevelCameraController>();
    }

    void Update()
    {
        if (playerController != null)
        {
            int playerHeight = calculatePlayerHeight();
            playerController.MoveCharacter(playerHeight);
        }    


        // calc which tile player is on
        // get the tiles height
        // if height difference between current and previous tiles does not exceed e.g. 1,
        // set player height to tile height.
    }

    private int calculatePlayerHeight()
    {
        //Debug.Log(playerController.transform.position);
        var cellPos = tilemap.WorldToCell(new Vector3(playerController.transform.position.x, playerController.transform.position.y,0));

        //i depends on terrain height variations.
        for (int i = 0; i <= 2; i++)
        {
            cellPos.z = i;
            if (tilemap.GetTile(cellPos) != null)
            {
                //Debug.Log("found at: " + cellPos);
                return i;
            }
        }
        Debug.Log("no tile found");
        return 1;
    }

    public void clearLevel()
    {
        tilemap.ClearAllTiles();
    }

    public void generate()
    {
        if (tilemap == null)
        {
            initialSetup();
        } else
        {
            clearLevel();
            size = new Vector2Int(10, 10);
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
    }

    private void updateLevelCamera()
    {
        BoundsInt terrainBounds = tilemap.cellBounds;

        Vector3 mapLocalDimension = grid.CellToLocal(new Vector3Int(terrainBounds.xMax, terrainBounds.yMax, 0));

        Vector3 levelCenter = grid.LocalToWorld(mapLocalDimension / 2.0f);
        // z value to ensure level is always in view
        levelCenter.z = -50.0f;

        int maxDimension = terrainBounds.xMax > terrainBounds.yMax ? terrainBounds.xMax : terrainBounds.yMax;
        float newOrthoSize = 0.4f * maxDimension;

        Vector3 cameraOffset = new Vector3(newOrthoSize * 0.5f, 0, 0);

        levelCameraController.updateCamera(levelCenter, cameraOffset, newOrthoSize);
    }

    // check each object exists! if not, then create it
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

        atlas = (SpriteAtlas)AssetDatabase.LoadAssetAtPath("Assets/GoldenSkullStudios/2D/2D_Iso_Tile_Pack_Starter/Atlas/2D_Iso_Starter_Atlas.spriteatlas", typeof(SpriteAtlas));

        //setupPlayer(tilemap.GetCellCenterWorld(Vector3Int.zero) + tileCenterOffset);
    }

    public void setupPlayer(Vector3 position)
    {
        player = Instantiate(player, new Vector3(2, 2, 3.01f), Quaternion.identity);
        playerController = player.GetComponent<PlayerController>();
        playerController.setup();

        playerController.setPosition(position);

    }

    private void OnApplicationQuit()
    {
        if (PrefabUtility.GetPrefabInstanceStatus(player) != PrefabInstanceStatus.NotAPrefab)
        {
            Destroy(player);
        }

    }
}
