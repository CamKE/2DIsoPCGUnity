using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var levelGrid = new GameObject("LevelGrid").AddComponent<Grid>();

        var tilemap = new GameObject("Tilemap").AddComponent<Tilemap>();

        var grid = levelGrid.GetComponent<Grid>();

        grid.cellSize = new Vector3(1, 0.5f, 1);
        grid.cellLayout = GridLayout.CellLayout.IsometricZAsY;

        tilemap.gameObject.AddComponent<TilemapRenderer>();
        tilemap.transform.SetParent(levelGrid.gameObject.transform);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
