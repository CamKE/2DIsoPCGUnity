using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class PathGenerator
{
    // status which is exempt from traversable checks
    protected Cell.CellStatus statusToCheck;

    protected CellPair getReachableCells(Map map, List<Vector2Int> boundaryCellList, Heap<CellPair> cellPairs, bool intersectionsEnabled)
    {
        Vector2Int boundaryCellXYPosition;
        int traversableNeighbourCount;

        Cell[] cellPair = new Cell[2];
        bool riverNodesFound = false;

        List<Vector2Int> boundaryCellListClone;
        int searchCount = 0;

        // limit search count to prevent infinite searching for non-existent valid cell pair  (temporary solution)
        while (!riverNodesFound && searchCount < 1000)
        {

            boundaryCellListClone = new List<Vector2Int>(boundaryCellList);

            for (int x = 0; x < 2; x++)
            {
                while (true)
                {
                    if (boundaryCellListClone.Count == 0)
                    {
                        return null;
                    }
                    boundaryCellXYPosition = boundaryCellListClone[UnityEngine.Random.Range(0, boundaryCellListClone.Count - 1)];

                    Cell cellToCheck = map.getCell(boundaryCellXYPosition);

                    traversableNeighbourCount = 0;

                    List<Cell> neighbours = map.getNeighbours(cellToCheck);

                    foreach (Cell neighbour in neighbours)
                    {
                        if (neighbour.isTraversable && !neighbour.isWaterBound)
                        {
                            traversableNeighbourCount++;
                        }
                    }

                    Cell cell = map.getCell(boundaryCellXYPosition);

                    if (traversableNeighbourCount > 0 && cell.status == Cell.CellStatus.TerrainCell)
                    {
                        //Debug.Log(boundaryCellList.Count);

                        boundaryCellListClone.Remove(boundaryCellXYPosition);
                        // remove adjacent boundary cells from list, we dont want the possibility
                        // of a 2 cell river

                        foreach (Cell neighbour in map.getNeighbours(cell))
                        {
                            //no need to check contain first, will just return false if not in list
                            boundaryCellListClone.Remove((Vector2Int)neighbour.position);
                        }

                        cellPair[x] = cell;
                        break;
                    }

                    boundaryCellListClone.Remove(boundaryCellXYPosition);
                    boundaryCellList.Remove(boundaryCellXYPosition);
                }
            }


            riverNodesFound = true;

            for (int x = 0; x < cellPairs.Count; x++)
            {
                //bool intersects = lineSegmentsIntersect(cellPair[0].position, cellPair[1].position, rivers[x].First(), rivers[x].Last());
                if (!intersectionsEnabled && doIntersect(cellPair[0].position, cellPair[1].position, cellPairs.getItem(x).startCell.position, cellPairs.getItem(x).endCell.position))
                {
                    riverNodesFound = false;
                    //Debug.Log("there is an intersection, pick another pair of nodes");
                    break;
                }
            }

            searchCount++;
        }

        if (searchCount == 1000)
        {
            return null;
        }
        else
        {
            foreach (Cell cell in cellPair)
            {
                Vector2Int cellPosition = (Vector2Int)cell.position;

                boundaryCellList.Remove(cellPosition);
                // remove adjacent boundary cells from list, we dont want the possibility
                // of a 2 cell river
                foreach (Cell neighbour in map.getNeighbours(cell))
                {
                    //no need to check contain first, will just return false if not in list
                    boundaryCellList.Remove((Vector2Int)neighbour.position);
                }
            }

            return new CellPair(cellPair[0], cellPair[1], GetDistance(cellPair[0], cellPair[1]));
        }

    }

    // The main function that returns true if line segment 'p1q1'
    // and 'p2q2' intersect.
    private static Boolean doIntersect(Vector3Int p1, Vector3Int q1, Vector3Int p2, Vector3Int q2)
    {
        // Find the four orientations needed for general and
        // special cases
        int o1 = orientation(p1, q1, p2);
        int o2 = orientation(p1, q1, q2);
        int o3 = orientation(p2, q2, p1);
        int o4 = orientation(p2, q2, q1);

        // General case
        if (o1 != o2 && o3 != o4)
            return true;

        // Special Cases
        // p1, q1 and p2 are collinear and p2 lies on segment p1q1
        if (o1 == 0 && onSegment(p1, p2, q1)) return true;

        // p1, q1 and q2 are collinear and q2 lies on segment p1q1
        if (o2 == 0 && onSegment(p1, q2, q1)) return true;

        // p2, q2 and p1 are collinear and p1 lies on segment p2q2
        if (o3 == 0 && onSegment(p2, p1, q2)) return true;

        // p2, q2 and q1 are collinear and q1 lies on segment p2q2
        if (o4 == 0 && onSegment(p2, q1, q2)) return true;

        return false; // Doesn't fall in any of the above cases
    }

    // Given three collinear points p, q, r, the function checks if
    // point q lies on line segment 'pr'
    private static Boolean onSegment(Vector3Int p, Vector3Int q, Vector3Int r)
    {
        if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
            q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
            return true;

        return false;
    }

    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    private static int orientation(Vector3Int p, Vector3Int q, Vector3Int r)
    {
        // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
        // for details of below formula.
        int val = (q.y - p.y) * (r.x - q.x) -
                (q.x - p.x) * (r.y - q.y);

        if (val == 0) return 0; // collinear

        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }

    protected bool findAStarPath(Map map, Cell startNode, Cell endNode, Cell.CellStatus status, bool intersectionsEnabled)
    {
        Heap<Cell> openList = new Heap<Cell>(map.area);
        HashSet<Cell> closedList = new HashSet<Cell>();
        Cell currentNode;
  
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            currentNode = openList.RemoveFirst();

            closedList.Add(currentNode);

            if (currentNode == endNode)
            {
                currentNode = endNode;

                while (currentNode != startNode)
                {
                    // change the level cells map

                    map.updateCellStatus(currentNode, status, intersectionsEnabled);

                    currentNode = currentNode.parent;
                }
                // change the level cells map

                map.updateCellStatus(currentNode, status, intersectionsEnabled);

                return true;
            }

            foreach (Cell neighbourNode in map.getNeighbours(currentNode))
            {

                if (status == Cell.CellStatus.RiverCell)
                {
                    if (!neighbourNode.isTraversable || closedList.Contains(neighbourNode) || !intersectionsEnabled && neighbourNode.isWaterBound)
                    {
                            continue;
                    }
                } else
                {
                    if ((!neighbourNode.isTraversable && neighbourNode.status != Cell.CellStatus.RiverCell) || closedList.Contains(neighbourNode))
                    {
                            continue;
                    }
                }

                int newNeighbourGCost = currentNode.gCost + GetDistance(currentNode, neighbourNode);

                if (newNeighbourGCost < neighbourNode.gCost || !openList.Contains(neighbourNode))
                {

                    neighbourNode.gCost = newNeighbourGCost;

                    neighbourNode.hCost = GetDistance(neighbourNode, endNode);

                    neighbourNode.parent = currentNode;

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }
        return false;
    }

    private int GetDistance(Cell startNode, Cell endNode)
    {
        int xDistance = Mathf.Abs(startNode.position.x - endNode.position.x);
        int yDistance = Mathf.Abs(startNode.position.y - endNode.position.y);

        int travelCost;

        switch(endNode.status)
        {
            case Cell.CellStatus.RiverCell:
            case Cell.CellStatus.WalkpathCell:
                travelCost = 15;
                break;
            default:
                travelCost = 10;
                break;
        }

        return travelCost * (yDistance + xDistance);
    }
}
