using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Tilemaps;
using System;

public class Level : MonoBehaviour
{
    private Map levelMap;

    ///  whether or not a level is generated
    public bool isGenerated { get; private set; }

    // work on generalisation
    private TerrainGenerator terrainGenerator;

    private RiverGenerator riverGenerator;

    private LakeGenerator lakeGenerator;

    private WalkpathGenerator walkpathGenerator;

    // sprites packed for more efficient use
    [SerializeField]
    private SpriteAtlas atlas;

    // gameobject with grid component for aligning tiles
    private Grid grid;

    private LevelCameraController cameraController;

    private Tilemap[] tilemaps;

    List<Vector3Int>[] positions;

    List<TileBase>[] tiles;

    private enum TilemapNames { Terrain, TerrainOuterBounds1, TerrainOuterBounds2, Water }
    // Start is called before the first frame update
    void Start()
    {
        isGenerated = false;

        grid = transform.GetComponent<Grid>();

        int numEnum = Enum.GetValues(typeof(TilemapNames)).Length;
        tilemaps = new Tilemap[numEnum];
        positions = new List<Vector3Int>[numEnum];
        tiles = new List<TileBase>[numEnum];

        tilemaps[(int)TilemapNames.Terrain] = setupTilemap(grid, "Terrain", true);

        tilemaps[(int)TilemapNames.TerrainOuterBounds1] = setupCollidableTilemap(grid, "TerrainOuterBounds1", false, 0.785f);
        tilemaps[(int)TilemapNames.TerrainOuterBounds2] = setupCollidableTilemap(grid, "TerrainOuterBounds2", false);

        tilemaps[(int)TilemapNames.Water] = setupCollidableTilemap(grid, "Water", true, 1.125f);

        terrainGenerator = new TerrainGenerator(atlas);

        riverGenerator = new RiverGenerator(atlas);

        lakeGenerator = new LakeGenerator(atlas);

        walkpathGenerator = new WalkpathGenerator(atlas);

        cameraController = transform.GetChild(0).GetComponent<LevelCameraController>();

        cameraController.enabled = false;
    }

    private Tilemap setupTilemap(Grid grid, string name, bool tilemapRendererEnabled)
    {
        Tilemap tilemap = new GameObject(name).AddComponent<Tilemap>();

        tilemap.gameObject.AddComponent<TilemapRenderer>();
        tilemap.transform.SetParent(grid.gameObject.transform);
        // move tile anchor from the button of the tile, to the front point of the tile (in the z)
        tilemap.tileAnchor = new Vector3(0, 0, -2);

        var terrainTilemapRenderer = tilemap.GetComponent<TilemapRenderer>();

        terrainTilemapRenderer.enabled = tilemapRendererEnabled;
        terrainTilemapRenderer.mode = TilemapRenderer.Mode.Individual;

        return tilemap;
    }

    private Tilemap setupCollidableTilemap(Grid grid, string name, bool tilemapRendererEnabled, float colliderYOffset = 0)
    {
        Tilemap tilemap = setupTilemap(grid, name, tilemapRendererEnabled);
        tilemap.gameObject.AddComponent<TilemapCollider2D>();
        tilemap.gameObject.AddComponent<Rigidbody2D>();
        tilemap.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        //tilemap.gameObject.AddComponent<CompositeCollider2D>();
        tilemap.GetComponent<TilemapCollider2D>().offset = new Vector2(0, colliderYOffset);

        return tilemap;
    }

    public void setCameraActive(bool value)
    {
        cameraController.gameObject.SetActive(value);
    }

    public void generate(TerrainOptions.TerrainSettings terrainSettings, RiverOptions.RiverSettings riverSettings, LakeOptions.LakeSettings lakeSettings, WalkpathPathOptions.WalkpathSettings walkpathSettings)
    {
       // Debug.Log("NEW RUN");
        clear();
        Stopwatch sw = new Stopwatch();

        sw.Start();

        int numEnums = Enum.GetValues(typeof(TilemapNames)).Length;

        for (int x = 0; x < numEnums; x++)
        {
            positions[x] = new List<Vector3Int>();
            tiles[x] = new List<TileBase>();
        }

        terrainGenerator.setTerrainSettings(terrainSettings);

        levelMap = terrainGenerator.createMap();

        terrainGenerator.populateCells(levelMap);

        terrainGenerator.setOuterBounds(levelMap, ref positions[(int)TilemapNames.TerrainOuterBounds2], ref tiles[(int)TilemapNames.TerrainOuterBounds2], ref positions[(int)TilemapNames.TerrainOuterBounds1], ref tiles[(int)TilemapNames.TerrainOuterBounds1]);

        // if lake generation is enabled
        if (lakeSettings.lGenerationEnabled)
        {
            lakeGenerator.setLakeSettings(lakeSettings);
            // populate the levelCells 3d array with the lake cells
            lakeGenerator.populateCells(levelMap);
        }

        if (riverSettings.rGenerationEnabled)
        {
            riverGenerator.setRiverSettings(riverSettings);
            // populate the levelCells 3d array with the river cells
            riverGenerator.populateCells(levelMap);
        }

        if (walkpathSettings.wGenerationEnabled)
        {
            walkpathGenerator.setWalkpathSettings(walkpathSettings);
            // populate the levelCells 3d array with the river cells
            walkpathGenerator.populateCells(levelMap);
        }

        setTilemaps();

        sw.Stop();
        Debug.Log($"level generated: {sw.ElapsedMilliseconds} ms");

        isGenerated = true;

        updateCamera();


    }

    private void setTilemaps()
    {
        // set the array of positions and array of tiles from the level cells which are terrain
        // then populate the terrain tilemap with the tiles


        // terrain tiles
        Tile[] groundTiles = terrainGenerator.getGroundTiles();
        Tile[] accessoryTiles = terrainGenerator.getAccessoryTiles();

        // river tile
        Tile riverTile = riverGenerator.getTile();

        // walkpath tile
        Tile walkpathTile = walkpathGenerator.getTile();

        // lake tile
        Tile lakeTile = lakeGenerator.getTile();

        for (int x = 0; x < levelMap.width; x++)
        {
            for (int y = 0; y < levelMap.height; y++)
            {
                Cell currentCell = levelMap.getCell(x, y);
                switch (currentCell.status)
                {
                    case Cell.CellStatus.TerrainCell:

                        positions[(int)TilemapNames.Terrain].Add(currentCell.position);
                        // select random accessory tile at 30% chance
                        tiles[(int)TilemapNames.Terrain].Add(UnityEngine.Random.value > 0.2f ? groundTiles[currentCell.position.z % groundTiles.Length] : accessoryTiles[currentCell.position.x % accessoryTiles.Length]);
                        // add tiles below current position
                        for (int z = currentCell.position.z - 1; z >= 0; z--)
                        {
                            positions[(int)TilemapNames.Terrain].Add(new Vector3Int(currentCell.position.x, currentCell.position.y, z));
                            tiles[(int)TilemapNames.Terrain].Add(groundTiles[0]);
                        }
                        break;
                    case Cell.CellStatus.RiverCell:
                        // set river tile to be 1 lower than the lowest depth neighbour
                        currentCell.position.z = levelMap.getMinDepth(currentCell) - 1;
                        positions[(int)TilemapNames.Water].Add(currentCell.position);
                        tiles[(int)TilemapNames.Water].Add(riverTile);
                        break;
                    case Cell.CellStatus.LakeCell:
                        currentCell.position.z -= 1;
                        positions[(int)TilemapNames.Water].Add(currentCell.position);
                        // select random accessory tile at 30% chance
                        tiles[(int)TilemapNames.Water].Add(lakeTile);
                        break;
                    case Cell.CellStatus.WalkpathCell:
                        positions[(int)TilemapNames.Terrain].Add(currentCell.position);
                        // select random accessory tile at 30% chance
                        tiles[(int)TilemapNames.Terrain].Add(walkpathTile);
                        // add tiles below current position
                        for (int z = currentCell.position.z - 1; z >= 0; z--)
                        {
                            positions[(int)TilemapNames.Terrain].Add(new Vector3Int(currentCell.position.x, currentCell.position.y, z));
                            tiles[(int)TilemapNames.Terrain].Add(groundTiles[0]);
                        }
                        break;
                    default:
                        break;
                }
            }
        }


        foreach (TilemapNames tilemapName in Enum.GetValues(typeof(TilemapNames)))
        {
            int tilemapNameIndex = (int)tilemapName;
            tilemaps[tilemapNameIndex].SetTiles(positions[tilemapNameIndex].ToArray(), tiles[tilemapNameIndex].ToArray());
        }
    }

    // gives the camera the new center of the level and the new orthographic size
    private void updateCamera()
    {
        // enable the controller now that a level is generated
        cameraController.enabled = true;

        //BoundsInt terrainBounds = terrainGenerator.getTilemapBounds();
        BoundsInt terrainBounds = tilemaps[(int)TilemapNames.Terrain].cellBounds;

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

        cameraController.updateCamera(levelCenter, newOrthoSize);
    }

    public Vector3Int getRandomCell()
    {
        return levelMap.getRandomCell();
    }

    public Vector2 getGridPosition(Vector2Int cellPosition)
    {
        return grid.CellToWorld((Vector3Int)cellPosition);
    }


    private void clear()
    {
        isGenerated = false;

        foreach (Tilemap tilemap in tilemaps)
        {
            tilemap.ClearAllTiles();
        }
    }

    public int getCellZPosition(Vector2 worldPos)
    {
        Vector3Int gridPos = grid.WorldToCell(worldPos);

        gridPos.Clamp(new Vector3Int(0,0,0), new Vector3Int(levelMap.width - 1, levelMap.height - 1,TerrainGenerator.terrainMaxHeight));

        return levelMap.getCell((Vector2Int)gridPos).position.z;
    }

}
