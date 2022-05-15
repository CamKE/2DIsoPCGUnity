using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

/// <summary>
/// Responsible for creating the rivers in the level.
/// </summary>
public class RiverGenerator : PathGenerator
{
    // the tiles to be used for river generation, sorted by the terrain type
    private readonly Dictionary<TerrainGenerator.TerrainType, Tile> riverTilesByType;

    // the names of the tiles
    private readonly string waterTileName = "ISO_Tile_Water_01";
    private readonly string iceTileName = "ISO_Tile_Ice_01-06";
    private readonly string lavaTileName = "ISO_Tile_Lava_01";

    // the maximum possible amount of rivers in a level
    private int riverMaxCount;

    /// <summary>
    /// The scale for amount of rivers, relative to the level size.
    /// </summary>
    public enum NumberOfRivers { Low, Medium, High }
    /// <summary>
    /// The number of options in the NumberOfRivers enumeration.
    /// </summary>
    public static int numberOfRiversCount = Enum.GetValues(typeof(NumberOfRivers)).Length;
    // the settings to be used for river generation
    private RiverSettings riverSettings;
    // the multipler for the amount of rivers
    private const float countMultiplier = 0.0015f;
    // a reference to the level generation information for the level
    List<string> generationInfo;

    /// <summary>
    /// The constructor for the RiverGenerator. Sets all the tiles according to their types, and set the
    /// reference to the level generation info.
    /// </summary>
    /// <param name="atlas">A SpriteAtlas, holding all the sprites for the project.</param>
    /// <param name="generationInfo">A reference to the level generation information for the level.</param>
    public RiverGenerator(SpriteAtlas atlas, List<string> generationInfo)
    {
        // create a new dictionary object
        riverTilesByType = new Dictionary<TerrainGenerator.TerrainType, Tile>();

        // create the water tile
        Tile waterTile = setupTile(atlas, waterTileName);

        // add the water tile as the tile for the following level types:
        riverTilesByType.Add(TerrainGenerator.TerrainType.Greenery, waterTile);
        riverTilesByType.Add(TerrainGenerator.TerrainType.Desert, waterTile);
        riverTilesByType.Add(TerrainGenerator.TerrainType.Skin, waterTile);

        // create the lava tile and add it to the lava level type
        riverTilesByType.Add(TerrainGenerator.TerrainType.Lava, setupTile(atlas, lavaTileName));

        // create the ice tile and add it to the ice level type
        riverTilesByType.Add(TerrainGenerator.TerrainType.Snow, setupTile(atlas, iceTileName));

        // set the reference to the level gen info
        this.generationInfo = generationInfo;
    }

    // retrieve a tile from the atlas and set its collider type
    private Tile setupTile(SpriteAtlas atlas, string tilename)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = atlas.GetSprite(tilename);
        tile.colliderType = Tile.ColliderType.Grid;

        return tile;
    }

    /// <summary>
    /// Set the reference to the river settings.
    /// </summary>
    /// <param name="riverSettings">The river settings to set a reference to.</param>
    public void setRiverSettings(RiverSettings riverSettings)
    {
        this.riverSettings = riverSettings;
    }

    /// <summary>
    /// Gets the river tile to be used based on the type of terrain.
    /// </summary>
    /// <returns>The river tile.</returns>
    public Tile getTile()
    {
        return riverTilesByType[riverSettings.tType];
    }


    /// <summary>
    /// Update the map by adding the rivers.
    /// </summary>
    /// <param name="map">A reference to the Map object, which represents the state of all cells in the level.</param>
    public void populateCells(Map map)
    {
        // calculate the maximum river count based on the multiplier, the number of terrain cells in the level, and the number of rivers setting
        riverMaxCount = (int)Math.Ceiling(map.terrainCellCount * (countMultiplier * ((int)riverSettings.rNum + 1)));
        // add the generation step to the info list
        generationInfo.Add(riverMaxCount + " max river count calculated based on terrain size and " + riverSettings.rNum + " river amount setting");

        // create the rivers
        int count = createPaths(map, riverMaxCount, riverSettings.intersectionsEnabled, Cell.CellStatus.RiverCell);

        // add the generation step to the info list
        generationInfo.Add(count + " rivers generated");
    }

}
