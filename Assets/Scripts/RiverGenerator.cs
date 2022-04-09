using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RiverGenerator
{
    public struct RiverUserSettings
    {
        readonly public bool generationEnabled;
        readonly public numRivers rNum;
        readonly public bool intersectionsEnabled;
        readonly public bool bridgesEnabled;

        public RiverUserSettings(bool generationEnabled, numRivers rNum, bool intersectionsEnabled, bool bridgesEnabled)
        {
            this.generationEnabled = generationEnabled;
            this.rNum = rNum;
            this.intersectionsEnabled = intersectionsEnabled;
            this.bridgesEnabled = bridgesEnabled;
        }
    }

    public enum numRivers { Low, Medium, High }

    public RiverGenerator()
    {

    }

    public void populateCells(RiverUserSettings riverUserSettings, LevelManager.levelCellStatus[,,] levelCells)
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
                    if (levelCells[x, y, z] == LevelManager.levelCellStatus.riverCell)
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
