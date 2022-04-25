using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class LakeGenerator
{
    public enum NumberOfLakes { Low, Medium, High }

    public enum MaxLakeSize { Small, Medium, Large }

    LakeOptions.LakeSettings lakeSettings;

    public LakeGenerator(Grid grid, SpriteAtlas atlas)
    {

    }

    public void setLakeSettings(LakeOptions.LakeSettings lakeSettings)
    {
        this.lakeSettings = lakeSettings;
    }

    public void populateCells(Cell[,] map)
    {

    }

    public void generate(Cell[,] map)
    {
        // set the array of positions and array of tiles from the level cells which are terrain
        // then populate the terrain tilemap with the tiles
        List<Vector3Int> positions = new List<Vector3Int>();
        List<TileBase> tiles = new List<TileBase>();

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                    if (map[x, y].status == Cell.CellStatus.LakeCell)
                    {
                        positions.Add(map[x, y].position);
                        // set tile
                    }
            }
        }
        //terrainTilemap.SetTiles(positions.ToArray(), tiles.ToArray());
    }

    public void clearTilemap()
    {

    }

    public void randomlyGenerate()
    {

    }
}
