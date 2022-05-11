public struct RiverSettings
{
    readonly public TerrainGenerator.TerrainType tType;
    readonly public bool rGenerationEnabled;
    readonly public RiverGenerator.NumberOfRivers rNum;
    readonly public bool intersectionsEnabled;

    public RiverSettings(TerrainGenerator.TerrainType tType, bool rGenerationEnabled, RiverGenerator.NumberOfRivers rNum, bool intersectionsEnabled)
    {
        this.tType = tType;
        this.rGenerationEnabled = rGenerationEnabled;
        this.rNum = rNum;
        this.intersectionsEnabled = intersectionsEnabled;
    }

    public RiverSettings(TerrainGenerator.TerrainType tType)
    {
        this.tType = tType;
        rGenerationEnabled = UnityEngine.Random.value > 0.5f;
        rNum = (RiverGenerator.NumberOfRivers)UnityEngine.Random.Range(0, RiverGenerator.numberOfRiversCount);
        intersectionsEnabled = UnityEngine.Random.value > 0.5f;
    }
}