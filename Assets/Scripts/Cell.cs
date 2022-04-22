using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{

    public Vector3Int position;
    public Cell parent;
    public int hCost;
    public int gCost;
    public bool isTraversable;
    public enum CellStatus { ValidCell, InvalidCell, TerrainCell, LakeCell, RiverCell, OutOfBounds }
    public CellStatus status;

    public Cell(Vector3Int position, bool isTraversable)
    {
        this.position = position;
        this.isTraversable = isTraversable;
    }

    public int fCost()
    {
        return gCost + hCost;
    }
}
