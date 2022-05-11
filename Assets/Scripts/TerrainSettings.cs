
public class TerrainSettings
{
    readonly public TerrainGenerator.TerrainType tType;
    public int tSize { get; private set; }
    readonly public int tMinHeight, tMaxHeight;
    readonly public int tExactHeight;
    readonly public TerrainGenerator.TerrainShape tShape;
    readonly public bool heightRangeEnabled;

    // user settings
    public TerrainSettings(TerrainGenerator.TerrainType tType, bool heightRangeEnabled, int tSize, TerrainGenerator.TerrainShape tShape, int tMinHeight, int tMaxHeight, int tExactHeight)
    {
        this.tSize = tSize;
        this.tType = tType;
        this.tShape = tShape;
        this.heightRangeEnabled = heightRangeEnabled;

        if (heightRangeEnabled)
        {
            this.tMinHeight = tMinHeight;
            this.tMaxHeight = tMaxHeight;
            this.tExactHeight = -1;
        }
        else
        {
            this.tExactHeight = tExactHeight;
            this.tMinHeight = -1;
            this.tMaxHeight = -1;
        }
    }

    // randomised settings
    public TerrainSettings()
    {
        tSize = UnityEngine.Random.Range(TerrainGenerator.terrainMinSize, TerrainGenerator.terrainMaxSize + 1);
        tType = (TerrainGenerator.TerrainType)UnityEngine.Random.Range(0, TerrainGenerator.terrainTypeCount);
        tShape = (TerrainGenerator.TerrainShape)UnityEngine.Random.Range(0, TerrainGenerator.terrainShapeCount);
        heightRangeEnabled = UnityEngine.Random.value > 0.5f;

        if (heightRangeEnabled)
        {
            // TerrainGenerator.terrainMinHeight to TerrainGenerator.terrainMaxHeight-1
            tMinHeight = UnityEngine.Random.Range(TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);

            // Greater than tMinHeight, up to the terrainMaxHeight
            tMaxHeight = UnityEngine.Random.Range(tMinHeight + 1, TerrainGenerator.terrainMaxHeight + 1);

            tExactHeight = -1;
        }
        else
        {
            tExactHeight = UnityEngine.Random.Range(TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight + 1);
            tMinHeight = -1;
            tMaxHeight = -1;
        }
    }

    public bool heightRangeIsOnAndInvalid()
    {
        return heightRangeEnabled && tMinHeight >= tMaxHeight;
    }

    public void updateTerrainSize(int tSize)
    {
        this.tSize = tSize;
    }
}
