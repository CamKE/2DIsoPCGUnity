/// <summary>
/// Holds the settings used for terrain generation.
/// </summary>
public class TerrainSettings
{
    // the different settings
    /// <summary>
    /// The type of terrain to be generated.
    /// </summary>
    readonly public TerrainGenerator.TerrainType tType;
    /// <summary>
    /// The size of terrain.
    /// </summary>
    public int tSize { get; private set; }
    /// <summary>
    /// The minimum possible height of the terrain.
    /// </summary>
    readonly public int tMinHeight;
    /// <summary>
    /// The maximum possible height of the terrain.
    /// </summary>
    readonly public int tMaxHeight;
    /// <summary>
    /// The exact height of the terrain. Used if height range is not enabled.
    /// </summary>
    readonly public int tExactHeight;
    /// <summary>
    /// The shape of the terrain.
    /// </summary>
    readonly public TerrainGenerator.TerrainShape tShape;
    /// <summary>
    /// Determines if the exact height is used or the minimum and maximum height values.
    /// </summary>
    readonly public bool heightRangeEnabled;

    // user settings
    /// <summary>
    /// Constructor for TerrainSettings. Responsible for setting all the varables from user options.
    /// </summary>
    /// <param name="tType">The type of terrain to be generated.</param>
    /// <param name="heightRangeEnabled">Determines if the exact height is used or the minimum and maximum height values.</param>
    /// <param name="tSize">The size of terrain.</param>
    /// <param name="tShape">The shape of the terrain.</param>
    /// <param name="tMinHeight">The minimum possible height of the terrain.</param>
    /// <param name="tMaxHeight">The maximum possible height of the terrain.</param>
    /// <param name="tExactHeight">The exact height of the terrain. Used if height range is not enabled.</param>
    public TerrainSettings(TerrainGenerator.TerrainType tType, bool heightRangeEnabled, int tSize, TerrainGenerator.TerrainShape tShape, int tMinHeight, int tMaxHeight, int tExactHeight)
    {
        // set the settings
        this.tSize = tSize;
        this.tType = tType;
        this.tShape = tShape;
        this.heightRangeEnabled = heightRangeEnabled;

        // if height range is on
        if (heightRangeEnabled)
        {
            // set the minimum and maximum height values
            this.tMinHeight = tMinHeight;
            this.tMaxHeight = tMaxHeight;

            // not in use, set to invalid value
            this.tExactHeight = -1;
        }
        else
        // otherwise
        {
            // set the exact height value
            this.tExactHeight = tExactHeight;

            // not in use, set to invalid values
            this.tMinHeight = -1;
            this.tMaxHeight = -1;
        }
    }

    /// <summary>
    /// Constructor for TerrainSettings. Responsible for setting all the varables randomly.
    /// </summary>
    public TerrainSettings()
    {
        // set the settings at random
        tSize = UnityEngine.Random.Range(TerrainGenerator.terrainMinSize, TerrainGenerator.terrainMaxSize + 1);
        tType = (TerrainGenerator.TerrainType)UnityEngine.Random.Range(0, TerrainGenerator.terrainTypeCount);
        tShape = (TerrainGenerator.TerrainShape)UnityEngine.Random.Range(0, TerrainGenerator.terrainShapeCount);
        heightRangeEnabled = UnityEngine.Random.value > 0.5f;

        // if height range is on
        if (heightRangeEnabled)
        {
            // set the minimum and maximum height values at random

            // TerrainGenerator.terrainMinHeight to TerrainGenerator.terrainMaxHeight-1
            tMinHeight = UnityEngine.Random.Range(TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);

            // Greater than tMinHeight, up to the terrainMaxHeight
            tMaxHeight = UnityEngine.Random.Range(tMinHeight + 1, TerrainGenerator.terrainMaxHeight + 1);

            // not in use, set to invalid value
            tExactHeight = -1;
        }
        else
        {
            // set the exact height value at random
            tExactHeight = UnityEngine.Random.Range(TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight + 1);

            // not in use, set to invalid values
            tMinHeight = -1;
            tMaxHeight = -1;
        }
    }

    /// <summary>
    /// Used to ensure that if the user has specified a range of height, the minimum value
    /// is less than the maximum value.
    /// </summary>
    /// <returns>Whether or not the height range is on and is set to valid values.</returns>
    public bool heightRangeIsOnAndInvalid()
    {
        return heightRangeEnabled && tMinHeight >= tMaxHeight;
    }

    /// <summary>
    /// When the terrain is generated, the actual size of the level must be updated in the settings.
    /// </summary>
    /// <param name="tSize">The actual terrain size used by the terrain generator.</param>
    public void updateTerrainSize(int tSize)
    {
        this.tSize = tSize;
    }
}
