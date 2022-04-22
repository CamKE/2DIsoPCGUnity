using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Level : MonoBehaviour
{

    private Cell[,] map;

    private bool isGenerated;

    // work on generalisation
    private TerrainGenerator terrainGenerator;

    private RiverGenerator riverGenerator;

    private LakeGenerator lakeGenerator;

    // sprites packed for more efficient use
    [SerializeField]
    private SpriteAtlas atlas;

    // gameobject with grid component for aligning tiles
    private Grid grid;

    // Start is called before the first frame update
    void Start()
    {
        isGenerated = false;

        grid = transform.GetComponent<Grid>();

        terrainGenerator = new TerrainGenerator(grid, atlas);

        riverGenerator = new RiverGenerator(grid, atlas);

        lakeGenerator = new LakeGenerator();
    }

}
