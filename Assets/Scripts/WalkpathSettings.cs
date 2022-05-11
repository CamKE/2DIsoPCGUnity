
public struct WalkpathSettings
{
    readonly public TerrainGenerator.TerrainType tType;
    readonly public bool wGenerationEnabled;
    readonly public WalkpathGenerator.NumberOfWalkpaths wNum;
    readonly public bool intersectionsEnabled;

    public WalkpathSettings(TerrainGenerator.TerrainType tType, bool wGenerationEnabled, WalkpathGenerator.NumberOfWalkpaths wNum, bool intersectionsEnabled)
    {
        this.tType = tType;
        this.wGenerationEnabled = wGenerationEnabled;
        this.wNum = wNum;
        this.intersectionsEnabled = intersectionsEnabled;
    }

    public WalkpathSettings(TerrainGenerator.TerrainType tType)
    {
        this.tType = tType;

        wGenerationEnabled = UnityEngine.Random.value > 0.5f;
        intersectionsEnabled = UnityEngine.Random.value > 0.5f;
        wNum = (WalkpathGenerator.NumberOfWalkpaths)UnityEngine.Random.Range(0, WalkpathGenerator.numberOfWalkpathsCount);
    }
}
