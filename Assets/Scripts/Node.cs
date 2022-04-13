using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{

    public Vector3Int position;
    public Node parent;
    public int hCost;
    public int gCost;
    public bool isTraversable;

    public Node(Vector3Int position, bool isTraversable)
    {
        this.position = position;
        this.isTraversable = isTraversable;
    }

    public int fCost()
    {
        return gCost + hCost;
    }
}
