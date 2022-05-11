using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.Diagnostics;
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

    [SerializeField]
    private Tilemap[] tilemaps;

    List<Vector3Int>[] positions;

    List<TileBase>[] tiles;

    int tilemapNamesCount = Enum.GetValues(typeof(TilemapNames)).Length;

    // can be static across generalisation of generators (if i do it)
    private List<string> generationInfo;

    private enum TilemapNames { Terrain, TerrainOuterBounds1, TerrainOuterBounds2, Water }
    // Start is called before the first frame update

    bool fromInspector;

    void Start()
    {
        setup();
    }

    public void setup(bool fromInspector = false)
    {
            isGenerated = false;

            grid = transform.GetComponent<Grid>();

            int numEnum = Enum.GetValues(typeof(TilemapNames)).Length;
            positions = new List<Vector3Int>[numEnum];
            tiles = new List<TileBase>[numEnum];

            generationInfo = new List<string>();

            terrainGenerator = new TerrainGenerator(atlas, generationInfo);

            riverGenerator = new RiverGenerator(atlas, generationInfo);

            lakeGenerator = new LakeGenerator(atlas, generationInfo);

            walkpathGenerator = new WalkpathGenerator(atlas, generationInfo);

            for (int x = 0; x < tilemapNamesCount; x++)
            {
                positions[x] = new List<Vector3Int>();
                tiles[x] = new List<TileBase>();
            }

            if (!fromInspector)
            {
            cameraController = transform.GetChild(0).GetComponent<LevelCameraController>();

            cameraController.enabled = false;
            }

            this.fromInspector = fromInspector;
            clear();
    }

    public void setCameraActive(bool value)
    {
        cameraController.gameObject.SetActive(value);
    }

    public void generate(TerrainSettings terrainSettings, RiverSettings riverSettings, LakeSettings lakeSettings, WalkpathSettings walkpathSettings)
    {
       // Debug.Log("NEW RUN");
        clear();
        Stopwatch sw = new Stopwatch();

        sw.Start();

        generationInfo.Add("Terrain Generator:");
        terrainGenerator.setTerrainSettings(terrainSettings);

        levelMap = terrainGenerator.createMap();

        terrainGenerator.populateCells(levelMap);

        terrainGenerator.setOuterBounds(levelMap, ref positions[(int)TilemapNames.TerrainOuterBounds2], ref tiles[(int)TilemapNames.TerrainOuterBounds2], ref positions[(int)TilemapNames.TerrainOuterBounds1], ref tiles[(int)TilemapNames.TerrainOuterBounds1]);

        // if lake generation is enabled
        if (lakeSettings.lGenerationEnabled)
        {
            generationInfo.Add("\n Lake Generator:");
            lakeGenerator.setLakeSettings(lakeSettings);
            // populate the levelCells 3d array with the lake cells
            lakeGenerator.populateCells(levelMap);
        }

        if (riverSettings.rGenerationEnabled)
        {
            generationInfo.Add("\n River Generator:");
            riverGenerator.setRiverSettings(riverSettings);
            // populate the levelCells 3d array with the river cells
            riverGenerator.populateCells(levelMap);
        }

        if (walkpathSettings.wGenerationEnabled)
        {
            generationInfo.Add("\n Walkpath Generator:");
            walkpathGenerator.setWalkpathSettings(walkpathSettings);
            // populate the levelCells 3d array with the river cells
            walkpathGenerator.populateCells(levelMap);
        }

        generationInfo.Add("\n Set Tilemaps:");
        setTilemaps();

        sw.Stop();
        generationInfo.Add("Level generated in " + sw.ElapsedMilliseconds + " ms");

        isGenerated = true;

        if (!fromInspector)
        {
            updateCamera();
        }
    }

    private void setTilemaps()
    {
        // set the array of positions and array of tiles from the level cells which are terrain
        // then populate the terrain tilemap with the tiles

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

        for (int x = 0; x < levelMap.width; x++)
        {
            for (int y = 0; y < levelMap.height; y++)
            {
                Cell currentCell = levelMap.getCell(x, y);
                Vector3Int currentCellPostion = currentCell.position;
                switch (currentCell.status)
                {
                    case Cell.CellStatus.TerrainCell:
                        
                        positions[(int)TilemapNames.Terrain].Add(currentCellPostion);
                        // select random accessory tile at 30% chance
                        tiles[(int)TilemapNames.Terrain].Add(UnityEngine.Random.value > 0.2f ? groundTiles[currentCellPostion.z % groundTiles.Length] : accessoryTiles[currentCellPostion.x % accessoryTiles.Length]);
                        // add tiles below current position
                     
                        if (currentCellPostion.z > 0)
                        {
                            setTilesBelowPosition(currentCellPostion, lowerGroundTile, (int)TilemapNames.Terrain);
                        }
                        break;
                    case Cell.CellStatus.RiverCell:
                        // set river tile to be 1 lower than the lowest depth neighbour
                        currentCellPostion.z = levelMap.getTerrainMinDepth(currentCell) - 1;
                        positions[(int)TilemapNames.Water].Add(currentCellPostion);
                        tiles[(int)TilemapNames.Water].Add(riverTile);
                        if (currentCellPostion.z > 0)
                        {
                            setTilesBelowPosition(currentCellPostion, lowerGroundTile, (int)TilemapNames.Terrain);
                        }
                        break;
                    case Cell.CellStatus.LakeCell:
                        currentCellPostion.z -= 1;
                        positions[(int)TilemapNames.Water].Add(currentCellPostion);
                        // select random accessory tile at 30% chance
                        tiles[(int)TilemapNames.Water].Add(lakeTile);
                        break;
                    case Cell.CellStatus.WalkpathCell:
                        positions[(int)TilemapNames.Terrain].Add(currentCellPostion);
                        // select random accessory tile at 30% chance
                        tiles[(int)TilemapNames.Terrain].Add(walkpathTile);
                        // add tiles below current position
                        if (currentCellPostion.z > 0)
                        {
                            setTilesBelowPosition(currentCellPostion, lowerGroundTile, (int)TilemapNames.Terrain);
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

    private void setTilesBelowPosition(Vector3Int position, Tile tile, int tilemapIndex)
    {
        int test = (position.z % 3);

        if (test == 0)
        {
            positions[tilemapIndex].Add(new Vector3Int(position.x, position.y, test));
            tiles[tilemapIndex].Add(tile);
        }
        else
        {
            for (int z = position.z - test; z >= 0; z -= 3)
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

    public List<string> getGenerationInfo()
    {
        return generationInfo;
    }

    private void clear()
    {
        isGenerated = false;

        foreach (Tilemap tilemap in tilemaps)
        {
            tilemap.ClearAllTiles();
        }


        for (int x = 0; x < tilemapNamesCount; x++)
        {
            positions[x].Clear();
            tiles[x].Clear();
        }

        generationInfo.Clear();
    }

    public int getCellZPosition(Vector2 worldPos)
    {
        Vector3Int gridPos = grid.WorldToCell(worldPos);

        gridPos.Clamp(new Vector3Int(0,0,0), new Vector3Int(levelMap.width - 1, levelMap.height - 1,TerrainGenerator.terrainMaxHeight));

        return levelMap.getCell((Vector2Int)gridPos).position.z;
    }
}
