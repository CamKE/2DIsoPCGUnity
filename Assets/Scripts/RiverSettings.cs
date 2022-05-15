/// <summary>
/// Holds the settings used for river generation.
/// </summary>
public struct RiverSettings
{
    // the different settings
    /// <summary>
    /// The type of terrain to be generated.
    /// </summary>
    readonly public TerrainGenerator.TerrainType tType;
    /// <summary>
    /// Turn on or off the creation of rivers.
    /// </summary>
    readonly public bool rGenerationEnabled;
    /// <summary>
    /// The number of rivers relative to the remaining terrain size.
    /// </summary>
    readonly public RiverGenerator.NumberOfRivers rNum;
    /// <summary>
    /// Turn on or off whether rivers are able to cross paths.
    /// </summary>
    readonly public bool intersectionsEnabled;

    /// <summary>
    /// Constructor for RiverSettings. Responsible for setting all the varables from user options.
    /// </summary>
    /// <param name="tType">The type of terrain to be generated.</param>
    /// <param name="rGenerationEnabled">Turn on or off the creation of rivers.</param>
    /// <param name="rNum">The number of rivers relative to the remaining terrain size.</param>
    /// <param name="intersectionsEnabled">Turn on or off whether rivers are able to cross paths.</param>
    public RiverSettings(TerrainGenerator.TerrainType tType, bool rGenerationEnabled, RiverGenerator.NumberOfRivers rNum, bool intersectionsEnabled)
    {
        this.tType = tType;
        this.rGenerationEnabled = rGenerationEnabled;
        this.rNum = rNum;
        this.intersectionsEnabled = intersectionsEnabled;
    }

    /// <summary>
    /// Constructor for RiverSettings. Responsible for setting all the varables randomly (except the terrain type).
    /// </summary>
    /// <param name="tType">The type of terrain to be generated.</param>
    public RiverSettings(TerrainGenerator.TerrainType tType)
    {
        this.tType = tType;
        rGenerationEnabled = UnityEngine.Random.value > 0.5f;
        rNum = (RiverGenerator.NumberOfRivers)UnityEngine.Random.Range(0, RiverGenerator.numberOfRiversCount);
        intersectionsEnabled = UnityEngine.Random.value > 0.5f;
    }
}