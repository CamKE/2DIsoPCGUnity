/// <summary>
/// The settings to be used for lake generation.
/// </summary>
public struct LakeSettings
{
    /// <summary>
    /// The type of terrain.
    /// </summary>
    readonly public TerrainGenerator.TerrainType tType;
    /// <summary>
    /// Whether lake generation is enabled.
    /// </summary>
    readonly public bool lGenerationEnabled;
    /// <summary>
    /// The scale for the number of lakes.
    /// </summary>
    readonly public LakeGenerator.NumberOfLakes lNum;
    /// <summary>
    /// The scale for the maximum lake size.
    /// </summary>
    readonly public LakeGenerator.MaxLakeSize lMaxSize;

    /// <summary>
    /// The constructor for the lake user settings. Sets all variables.
    /// </summary>
    /// <param name="tType">The type of terrain.</param>
    /// <param name="lGenerationEnabled">Whether lake generation is enabled.</param>
    /// <param name="lNum">The scale for the number of lakes.</param>
    /// <param name="lMaxSize">The scale for the maximum lake size.</param>
    public LakeSettings(TerrainGenerator.TerrainType tType, bool lGenerationEnabled, LakeGenerator.NumberOfLakes lNum, LakeGenerator.MaxLakeSize lMaxSize)
    {
        this.tType = tType;
        this.lGenerationEnabled = lGenerationEnabled;
        this.lNum = lNum;
        this.lMaxSize = lMaxSize;
    }

    /// <summary>
    /// The constructor for the lake randomised settings. Sets all variables.
    /// </summary>
    /// <param name="tType">The type of terrain.</param>
    public LakeSettings(TerrainGenerator.TerrainType tType)
    {
        this.tType = tType;
        // 5/11 chance on, 6/11 chance off
        lGenerationEnabled = UnityEngine.Random.value > 0.5f;
        // pick enum from range
        lNum = (LakeGenerator.NumberOfLakes)UnityEngine.Random.Range(0, LakeGenerator.numberOfLakesCount);
        lMaxSize = (LakeGenerator.MaxLakeSize)UnityEngine.Random.Range(0, LakeGenerator.maxLakeSizeCount);
    }
}

