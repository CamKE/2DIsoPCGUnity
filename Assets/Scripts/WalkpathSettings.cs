/// <summary>
/// Holds the settings used for walkpath generation.
/// </summary>
public struct WalkpathSettings
{
    // the different settings
    /// <summary>
    /// The type of terrain to be generated.
    /// </summary>
    readonly public TerrainGenerator.TerrainType tType;
    /// <summary>
    /// Turn on or off the creation of walkpaths.
    /// </summary>
    readonly public bool wGenerationEnabled;
    /// <summary>
    /// Turn on or off whether walkpaths are able to cross paths.
    /// </summary>
    readonly public bool intersectionsEnabled;
    /// <summary>
    /// The number of walkpaths relative to the remaining terrain size.
    /// </summary>
    readonly public WalkpathGenerator.NumberOfWalkpaths wNum;

    /// <summary>
    /// Constructor for WalkpathSettings. Responsible for setting all the varables from user options.
    /// </summary>
    /// <param name="tType">The type of terrain to be generated.</param>
    /// <param name="wGenerationEnabled">Turn on or off the creation of walkpaths.</param>
    /// <param name="wNum">The number of walkpaths relative to the remaining terrain size.</param>
    /// <param name="intersectionsEnabled">Turn on or off whether walkpaths are able to cross paths.</param>
    public WalkpathSettings(TerrainGenerator.TerrainType tType, bool wGenerationEnabled, WalkpathGenerator.NumberOfWalkpaths wNum, bool intersectionsEnabled)
    {
        this.tType = tType;
        this.wGenerationEnabled = wGenerationEnabled;
        this.wNum = wNum;
        this.intersectionsEnabled = intersectionsEnabled;
    }

    /// <summary>
    /// Constructor for WalkpathSettings. Responsible for setting all the varables randomly (except the terrain type).
    /// </summary>
    /// <param name="tType">The type of terrain to be generated.</param>
    public WalkpathSettings(TerrainGenerator.TerrainType tType)
    {
        this.tType = tType;
        wGenerationEnabled = UnityEngine.Random.value > 0.5f;
        intersectionsEnabled = UnityEngine.Random.value > 0.5f;
        wNum = (WalkpathGenerator.NumberOfWalkpaths)UnityEngine.Random.Range(0, WalkpathGenerator.numberOfWalkpathsCount);
    }
}
