using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEditor;


public class LevelManager : MonoBehaviour
{

    [SerializeField][HideInInspector]
    private Tilemap tilemap;

    [SerializeField][HideInInspector]
    private Tilemap tilemap2;

    [SerializeField][HideInInspector]
    private Grid grid;

    [SerializeField][HideInInspector]
    private SpriteAtlas atlas;

    [SerializeField]
    private Vector2Int size;

    [SerializeField]
    private GameObject player;

    [SerializeField][HideInInspector]
    private PlayerController playerController;

    // i use this instead of taking the grid center to world, as the grid center looks off. instead i take the tilemap tile center
    // which uses the pivot as the center. then i add an offset to the y axis to move the tile center to the middle of the tile.
    private Vector3 tileCenterOffset;

    // Start is called before the first frame update
    void Start()
    {
        tileCenterOffset = new Vector3(0,0.7f,0);
        generate();
    }

    void Update()
    {
        int playerHeight = calculatePlayerHeight();
        playerController.MoveCharacter(playerHeight);

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

        // takes tile position in local space, not world
         //tilemap2.SetTile(new Vector3Int(0,0,3), tile);
         //tilemap2.SetTile(new Vector3Int(1,0,3), tile);
         //tilemap2.SetTile(new Vector3Int(0, 0, 3), tile);

        //Debug.Log(tilemap2.GetCellCenterWorld(new Vector3Int(0, 0, 3)));

        

        playerController.setPosition(tilemap.GetCellCenterWorld(Vector3Int.zero) + tileCenterOffset);


    }

    private void initialSetup()
    {
        player = Instantiate(player, new Vector3(2, 2, 3.01f), Quaternion.identity);
        playerController = player.GetComponent<PlayerController>();
        playerController.setup();

        grid = new GameObject("Level").AddComponent<Grid>();

        tilemap = new GameObject("Terrain").AddComponent<Tilemap>();

        tilemap.gameObject.AddComponent<TilemapRenderer>();
        tilemap.transform.SetParent(grid.gameObject.transform);
        tilemap.tileAnchor = new Vector3(0, 0, -2);

        tilemap2 = new GameObject("Level1").AddComponent<Tilemap>();

        tilemap2.gameObject.AddComponent<TilemapRenderer>();
        tilemap2.transform.SetParent(grid.gameObject.transform);
        tilemap2.tileAnchor = new Vector3(0, 0, -2);

        var gridComponent = grid.GetComponent<Grid>();

        gridComponent.cellSize = new Vector3(1, 0.5f, 1);
        gridComponent.cellLayout = GridLayout.CellLayout.IsometricZAsY;

        var tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();

        tilemapRenderer.mode = TilemapRenderer.Mode.Individual;

        var tilemapRenderer2 = tilemap2.GetComponent<TilemapRenderer>();

        tilemapRenderer2.mode = TilemapRenderer.Mode.Individual;

        atlas = (SpriteAtlas)AssetDatabase.LoadAssetAtPath("Assets/GoldenSkullStudios/2D/2D_Iso_Tile_Pack_Starter/Atlas/2D_Iso_Starter_Atlas.spriteatlas", typeof(SpriteAtlas));
    }



    private void OnApplicationQuit()
    {

        if (grid != null)
        {
            Destroy(grid);
            Destroy(player);
        }

    }
}
