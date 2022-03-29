using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEditor;


public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var levelGrid = new GameObject("LevelGrid").AddComponent<Grid>();

        var tilemap = new GameObject("Level").AddComponent<Tilemap>();

        tilemap.gameObject.AddComponent<TilemapRenderer>();
        tilemap.transform.SetParent(levelGrid.gameObject.transform);
        tilemap.tileAnchor = new Vector3(0,0,0);

        var grid = levelGrid.GetComponent<Grid>();

        grid.cellSize = new Vector3(1, 0.5f, 1);
        grid.cellLayout = GridLayout.CellLayout.IsometricZAsY;

        var tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();

        tilemapRenderer.mode = TilemapRenderer.Mode.Individual;

        var atlas = (SpriteAtlas)AssetDatabase.LoadAssetAtPath("Assets/GoldenSkullStudios/2D/2D_Iso_Tile_Pack_Starter/Atlas/2D_Iso_Starter_Atlas.spriteatlas", typeof(SpriteAtlas));

        var tile = ScriptableObject.CreateInstance<Tile>();
        var tile2 = ScriptableObject.CreateInstance<Tile>();

        tile.sprite = atlas.GetSprite("ISO_Tile_Brick_Brick_02");
        tile2.sprite = atlas.GetSprite("ISO_Tile_Dirt_01_Grass_01");

        tilemap.SetTile(new Vector3Int(1, 0, 0), tile2);

        tilemap.SetTile(new Vector3Int(0, 0, 0), tile);
        tilemap.SetTile(new Vector3Int(0, 0, 3), tile);
        tilemap.SetTile(new Vector3Int(0, 0, 6), tile);



    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
