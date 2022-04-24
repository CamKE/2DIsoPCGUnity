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

    public void updateCamera(Vector3 newlevelCenter, float newOrthoSize)
    {
        cameraController.updateCamera(newlevelCenter, newOrthoSize);
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

        if (riverSettings.rGenerationEnabled)
        {
            riverGenerator.setRiverSettings(riverSettings);
            // populate the levelCells 3d array with the river cells
            riverGenerator.populateCells(map, terrainGenerator.terrainCellList, terrainGenerator.boundaryCellList);
        }

        terrainGenerator.populateCells(map);

        // generate the terrain based on the current state of the levelCells array
        terrainGenerator.generate(map);

        // if river generation is enabled
        if (riverSettings.rGenerationEnabled)
        {
            // populate the levelCells 3d array with the river cells
            riverGenerator.generate(map);
        }

        isGenerated = true;
        // enable the controller now that a level is generated
        cameraController.enabled = true;
    }

    public BoundsInt getTerrainBounds()
    {
        return terrainGenerator.getTilemapBounds();
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
