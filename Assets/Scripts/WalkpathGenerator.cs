using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

/// <summary>
/// Responsible for creating the walkpaths in the level.
/// </summary>
public class WalkpathGenerator : PathGenerator
{
    // the tiles to be used for walkpath generation, sorted by the terrain type
    private readonly Dictionary<TerrainGenerator.TerrainType, Tile> pathTilesByType;

    // the names of the tiles
    private readonly string pathTileName = "ISO_Tile_Stone_02";
    private readonly string lavaPathTileName = "ISO_Tile_Stone_01";
    private readonly string desertPathTileName = "ISO_Tile_Stone_03";
    private readonly string skinPathTileName = "ISO_Deco_Tooth_01_";

    // the maximum possible amount of walkpaths
    private int walkpathMaxCount;

    /// <summary>
    /// The scale for amount of walkpaths, relative to the level size.
    /// </summary>
    public enum NumberOfWalkpaths { Low, Medium, High }
    /// <summary>
    /// The number of options in the NumberOfWalkpaths enumeration.
    /// </summary>
    public static int numberOfWalkpathsCount = Enum.GetValues(typeof(NumberOfWalkpaths)).Length;

    // the settings to be used for walkpath generation
    private WalkpathSettings walkpathSettings;

    // the multipler for the amount of walkpaths
    private const float countMultiplier = 0.0025f;
    // a reference to the level generation information for the level
    List<string> generationInfo;

    /// <summary>
    /// The constructor for the WalkpathGenerator. Sets all the tiles according to their types, and set the
    /// reference to the level generation info.
    /// </summary>
    /// <param name="atlas">A SpriteAtlas, holding all the sprites for the project.</param>
    /// <param name="generationInfo">A reference to the level generation information for the level.</param>
    public WalkpathGenerator(SpriteAtlas atlas, List<string> generationInfo)
    {
        // create a new dictionary object
        pathTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        // create the path tile
        Tile pathTile = setupTile(atlas, pathTileName);
        // add the path tile as the tile for the following level types:
        pathTilesByType.Add(TerrainGenerator.TerrainType.Greenery, pathTile);
        pathTilesByType.Add(TerrainGenerator.TerrainType.Snow, pathTile);

        // create the desert path tile and add it to the desert level type
        pathTilesByType.Add(TerrainGenerator.TerrainType.Desert, setupTile(atlas, desertPathTileName));

        // create the lava path tile and add it to the lava level type
        pathTilesByType.Add(TerrainGenerator.TerrainType.Lava, setupTile(atlas, lavaPathTileName));

        // create the skin path tile and add it to the skin level type
        pathTilesByType.Add(TerrainGenerator.TerrainType.Skin, setupTile(atlas, skinPathTileName));

        // set the reference to the level gen info
        this.generationInfo = generationInfo;
    }

    // retrieve a tile from the atlas
    private Tile setupTile(SpriteAtlas atlas, string tilename)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = atlas.GetSprite(tilename);

        return tile;
    }

    /// <summary>
    /// Set the reference to the walkpath settings.
    /// </summary>
    /// <param name="walkpathSettings">The walkpath settings to set a reference to.</param>
    public void setWalkpathSettings(WalkpathSettings walkpathSettings)
    {
        this.walkpathSettings = walkpathSettings;
    }

    /// <summary>
    /// Gets the walkpath tile to be used based on the type of terrain.
    /// </summary>
    /// <returns>The walkpath tile.</returns>
    public Tile getTile()
    {
        return pathTilesByType[walkpathSettings.tType];
    }

    /// <summary>
    /// Update the map by adding the walkpaths.
    /// </summary>
    /// <param name="map">A reference to the Map object, which represents the state of all cells in the level.</param>
    public void populateCells(Map map)
    {
        // calculate the maximum walkpath count based on the multiplier, the number of terrain cells in the level, and the number of walkpaths setting
        walkpathMaxCount = (int)Math.Ceiling(map.terrainCellCount * (countMultiplier * ((int)walkpathSettings.wNum + 1)));
        // add the generation step to the info list
        generationInfo.Add(walkpathMaxCount + " max walkpath count calculated based on terrain size and " + walkpathSettings.wNum + " walkpath amount setting");

        // create the walkpaths
        int count = createPaths(map, walkpathMaxCount, walkpathSettings.intersectionsEnabled, Cell.CellStatus.WalkpathCell);

        // add the generation step to the info list
        generationInfo.Add(count + " walkpaths generated");
    }
}
