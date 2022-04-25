using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Level : MonoBehaviour
{

    private Cell[,] map;

    ///  whether or not a level is generated
    public bool isGenerated { get; private set; }

    // work on generalisation
    private TerrainGenerator terrainGenerator;

    private RiverGenerator riverGenerator;

    private LakeGenerator lakeGenerator;

    // sprites packed for more efficient use
    [SerializeField]
    private SpriteAtlas atlas;

    // gameobject with grid component for aligning tiles
    private Grid grid;

    private LevelCameraController cameraController;


    // Start is called before the first frame update
    void Start()
    {
        isGenerated = false;

        grid = transform.GetComponent<Grid>();

        terrainGenerator = new TerrainGenerator(grid, atlas);

        riverGenerator = new RiverGenerator(grid, atlas);

        lakeGenerator = new LakeGenerator(grid, atlas);

        cameraController = transform.GetChild(0).GetComponent<LevelCameraController>();

        cameraController.enabled = false;
    }

    public void setCameraActive(bool value)
    {
        cameraController.gameObject.SetActive(value);
    }

    public void generate(TerrainOptions.TerrainSettings terrainSettings, RiverOptions.RiverSettings riverSettings, LakeOptions.LakeSettings lakeSettings)
    {
        clear();

        terrainGenerator.setTerrainSettings(terrainSettings);

        map = terrainGenerator.createMap();

        terrainGenerator.populateCells(map);

        if (riverSettings.rGenerationEnabled)
        {
            riverGenerator.setRiverSettings(riverSettings);
            // populate the levelCells 3d array with the river cells
            riverGenerator.populateCells(map, terrainGenerator.terrainCellList, terrainGenerator.boundaryCellList);
        }

        // if lake generation is enabled
        if (lakeSettings.lGenerationEnabled)
        {
            lakeGenerator.setLakeSettings(lakeSettings);
            // populate the levelCells 3d array with the lake cells
            lakeGenerator.populateCells(map);
        }

        // generate the terrain based on the current state of the levelCells array
        terrainGenerator.generate(map);

        // if river generation is enabled
        if (riverSettings.rGenerationEnabled)
        {
            // populate the levelCells 3d array with the river cells
            riverGenerator.generate(map);
        }

        // if lake generation is enabled
        if (lakeSettings.lGenerationEnabled)
        {
            // populate the levelCells 3d array with the lake cells
            lakeGenerator.generate(map);
        }

        isGenerated = true;

        updateCamera();
    }

    // gives the camera the new center of the level and the new orthographic size
    private void updateCamera()
    {
        // enable the controller now that a level is generated
        cameraController.enabled = true;

        BoundsInt terrainBounds = terrainGenerator.getTilemapBounds();

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
        bool cellFound = false;
        while (!cellFound)
        {
            Vector2Int cellPosition = new Vector2Int(UnityEngine.Random.Range(0, map.GetLength(0)-1), UnityEngine.Random.Range(0, map.GetLength(1)-1));

            if (map[cellPosition.x, cellPosition.y].status == Cell.CellStatus.TerrainCell)
            {
                cellFound = true;
                return map[cellPosition.x, cellPosition.y].position;
            }
        }

        return default;
    }

    public Vector2 getGridPosition(Vector2Int cellPosition)
    {
        return grid.CellToWorld((Vector3Int)cellPosition);
    }


    public void clear()
    {
        terrainGenerator.clearTilemap();
        riverGenerator.clearTilemap();
        lakeGenerator.clearTilemap();
        isGenerated = false;
    }

    public int getCellZPosition(Vector2 worldPos)
    {
        Vector3Int gridpos = grid.WorldToCell(worldPos);

        return map[gridpos.x, gridpos.y].position.z;
    }

}
