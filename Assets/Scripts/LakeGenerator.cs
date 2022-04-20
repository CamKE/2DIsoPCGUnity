using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LakeGenerator
{
    public struct LakeUserSettings
    {
        readonly public bool generationEnabled;
        readonly public numLakes lNum;
        readonly public maxLakeSize lMaxSize;

        public LakeUserSettings(bool generationEnabled, numLakes lNum, maxLakeSize lMaxSize)
        {
            this.generationEnabled = generationEnabled;
            this.lNum = lNum;
            this.lMaxSize = lMaxSize;
        }
    }

    public enum numLakes { Low, Medium, High }

    public enum maxLakeSize { Small, Medium, Large }

    LakeOptions.LakeSettings lakeSettings;

    public LakeGenerator()
    {

    }

    public void setLakeSettings(LakeOptions.LakeSettings lakeSettings)
    {
        this.lakeSettings = lakeSettings;
    }

    public void populateCells(LevelManager.levelCellStatus[,,] levelCells)
    {

    }

    public void generate(LevelManager.levelCellStatus[,,] levelCells)
    {
        // set the array of positions and array of tiles from the level cells which are terrain
        // then populate the terrain tilemap with the tiles
        List<Vector3Int> positions = new List<Vector3Int>();
        List<TileBase> tiles = new List<TileBase>();

        for (int x = 0; x < levelCells.GetLength(0); x++)
        {
            for (int y = 0; y < levelCells.GetLength(1); y++)
            {
                for (int z = 0; z < levelCells.GetLength(2); z++)
                {
                    if (levelCells[x, y, z] == LevelManager.levelCellStatus.lakeCell)
                    {
                        positions.Add(new Vector3Int(x, y, z));
                        // set tile
                    }
                }
            }
        }
        //terrainTilemap.SetTiles(positions.ToArray(), tiles.ToArray());
    }

    public void randomlyGenerate()
    {

    }
}
