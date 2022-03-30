using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEditor;


public class LevelGenerator : MonoBehaviour
{
    [SerializeField][HideInInspector]
    private Tilemap tilemap;

    [SerializeField][HideInInspector]
    private Grid grid;

    [SerializeField][HideInInspector]
    private SpriteAtlas atlas;

    [SerializeField]
    private Vector2Int size;

    // Start is called before the first frame update
    void Start()
    {
        generate();
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

        Vector3Int[] positions = new Vector3Int[size.x * size.y];
        TileBase[] tileArray = new TileBase[positions.Length];

        for (int index = 0; index < positions.Length; index++)
        {
            positions[index] = new Vector3Int(index % size.x, index / size.y, 0);
            tileArray[index] = index % 2 == 0 ? tile : tile2;
        }

        tilemap.SetTiles(positions, tileArray);

    }

    private void initialSetup()
    {
        grid = new GameObject("LevelGrid").AddComponent<Grid>();

        tilemap = new GameObject("Level").AddComponent<Tilemap>();

        tilemap.gameObject.AddComponent<TilemapRenderer>();
        tilemap.transform.SetParent(grid.gameObject.transform);
        tilemap.tileAnchor = new Vector3(0, 0, 0);

        var gridComponent = grid.GetComponent<Grid>();

        gridComponent.cellSize = new Vector3(1, 0.5f, 1);
        gridComponent.cellLayout = GridLayout.CellLayout.IsometricZAsY;

        var tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();

        tilemapRenderer.mode = TilemapRenderer.Mode.Individual;

        atlas = (SpriteAtlas)AssetDatabase.LoadAssetAtPath("Assets/GoldenSkullStudios/2D/2D_Iso_Tile_Pack_Starter/Atlas/2D_Iso_Starter_Atlas.spriteatlas", typeof(SpriteAtlas));
    }

    private void OnApplicationQuit()
    {

        if (grid != null)
        {
            Destroy(grid);
        }

    }
}
