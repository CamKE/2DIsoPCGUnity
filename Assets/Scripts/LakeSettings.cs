
public struct LakeSettings
{
    readonly public TerrainGenerator.TerrainType tType;
    readonly public bool lGenerationEnabled;
    readonly public LakeGenerator.NumberOfLakes lNum;
    readonly public LakeGenerator.MaxLakeSize lMaxSize;

    public LakeSettings(TerrainGenerator.TerrainType tType, bool lGenerationEnabled, LakeGenerator.NumberOfLakes lNum, LakeGenerator.MaxLakeSize lMaxSize)
    {
        this.tType = tType;
        this.lGenerationEnabled = lGenerationEnabled;
        this.lNum = lNum;
        this.lMaxSize = lMaxSize;
    }

    public LakeSettings(TerrainGenerator.TerrainType tType)
    {
        this.tType = tType;
        lGenerationEnabled = UnityEngine.Random.value > 0.5f;
        lNum = (LakeGenerator.NumberOfLakes)UnityEngine.Random.Range(0, LakeGenerator.numberOfLakesCount);
        lMaxSize = (LakeGenerator.MaxLakeSize)UnityEngine.Random.Range(0, LakeGenerator.maxLakeSizeCount);
    }
}

