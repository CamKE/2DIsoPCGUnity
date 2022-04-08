using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for generating the terrain. Used by the Level Manager.
/// </summary>
public class TerrainGenerator
{
    /// <summary>
    /// The terrain shape options.
    /// </summary>
    public enum terrainShape { Square, Rectangle, Random };

    /// <summary>
    /// The minimum size of a level specified by tile count.
    /// </summary>
    public const int terrainMinSize = 10;

    /// <summary>
    /// The maximum size of a level specified by tile count.
    /// </summary>
    public const int terrainMaxSize = 2000;
}
