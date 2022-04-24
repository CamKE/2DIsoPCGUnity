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

    
    // update bool based on cell status
    public void setCellStatus(CellStatus newStatus)
    {
        status = newStatus;
        switch (newStatus)
        {
            case CellStatus.ValidCell:
            case CellStatus.TerrainCell:
                isTraversable = true;
                break;
            default:
                isTraversable = false;
                break;
        }
    }
    

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
