using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for creating all path related elements in the level.
/// </summary>
public class PathGenerator
{
    // status which is exempt from traversable checks
    protected Cell.CellStatus statusToCheck;

    /// <summary>
    /// Gets a pair of start and end cells for which a path can be generated.
    /// </summary>
    /// <param name="map">The levels 2D array of cells.</param>
    /// <param name="existingCellPairs">The current chosen cell pairs. Used to ensure paths do not cross if
    /// intersections are disabled.</param>
    /// <param name="intersectionsEnabled">Whether or not intersections are enabled.</param>
    /// <returns>A pair of start and end cells for which a path can be generated.</returns>
    protected CellPair getReachableCells(Map map, Heap<CellPair> existingCellPairs, bool intersectionsEnabled)
    {
        // the x,y position of the boundary cell to be checked
        Vector2Int boundaryCellXYPosition;
        // the number of traversable neighbours a cell has
        int traversableNeighbourCount;

        // get the boundary cell positions
        List<Vector2Int> boundaryCellList = map.getBoundaryCellPositions();

        // temporary storage for the cells to be validated
        Cell[] cellPair = new Cell[2];
        // whether of not a pair of cells have been found
        bool pairFound = false;

        // a clone of the list of boundary cells. we want to make modifications to the list which can be undone
        List<Vector2Int> boundaryCellListClone = null;
        // the number of searches done
        int searchCount = 0;

        // limit search count to an abritrary value to prevent infinite searching for non-existent valid cell pair.
        // not an optimal solution but works well. Need to revisit
        while (!pairFound && searchCount < 1000)
        {
            // make a clone of the boundary cell list
            boundaryCellListClone = new List<Vector2Int>(boundaryCellList);

            // get 2 cells
            for (int x = 0; x < 2; x++)
            {
                // loop until cell is found or boundary cell count reaches 0 (at which point we cannot find a pair, so return null result)
                while (true)
                {
                    // if the list is empty
                    if (boundaryCellListClone.Count == 0)
                    {
                        // no valid cell could be found, return null result
                        return null;
                    }
                    // retrieve a random boundary cell
                    boundaryCellXYPosition = boundaryCellListClone[UnityEngine.Random.Range(0, boundaryCellListClone.Count - 1)];
                    // get the cell at the position
                    Cell cellToCheck = map.getCell(boundaryCellXYPosition);
                    // set the number of traversable neighbours to 0
                    traversableNeighbourCount = 0;
                    // get the neighbours of the cell to check
                    List<Cell> neighbours = map.getNeighbours(cellToCheck);
                    
                    // for each of the neighbours
                    foreach (Cell neighbour in neighbours)
                    {
                        // if the neighbour is traversable and not a water bound
                        if (neighbour.isTraversable && !neighbour.isWaterBound)
                        {
                            // increment the count
                            traversableNeighbourCount++;
                        }
                    }

                    //if the count is more than 0 and the cell is a terrain cell
                    if (traversableNeighbourCount > 0 && cellToCheck.status == Cell.CellStatus.TerrainCell)
                    {
                        // remove the cell from the temporary clone list
                        boundaryCellListClone.Remove(boundaryCellXYPosition);

                        // remove adjacent boundary cells from list, we dont want the possibility
                        // of retrieving them
                        foreach (Cell neighbour in neighbours)
                        {
                            // no need to check contain first, will just return false if not in list
                            boundaryCellListClone.Remove((Vector2Int)neighbour.position);
                        }

                        // add the cell to the temporary cell pair array
                        cellPair[x] = cellToCheck;
                        // break out of the while loop
                        break;
                    }

                    // otherwise, it is an invalid cell. remove it from both the temporary
                    // list and the actual list (omit from future searches)
                    boundaryCellListClone.Remove(boundaryCellXYPosition);
                    boundaryCellList.Remove(boundaryCellXYPosition);
                }
            }
            // set path found to true by default
            pairFound = true;

            // if intersections are not enabled (not allowed)
            if (!intersectionsEnabled)
            {
                // for each of the cell pairs in the heap
                for (int x = 0; x < existingCellPairs.Count; x++)
                {
                    // if this cell pair intersects with the existing cell pair at position x in the heap
                    if (doIntersect(cellPair[0].position, cellPair[1].position, existingCellPairs.getItem(x).startCell.position, existingCellPairs.getItem(x).endCell.position))
                    {
                        // then it cannot be a valid pair, as we are not allowing intersections
                        pairFound = false;
                        // break out of the loop
                        break;
                    }
                }
            }

            // increment search count
            searchCount++;
        }

        // if a pair is not found
        if (!pairFound)
        {
            // no valid pair could be found, return null result
            return null;
        }
        else
        // otherwise
        {
            // update the boundary cell list
            map.updateBoundaryCellPositionList(boundaryCellListClone);

            // return the pair
            return new CellPair(cellPair[0], cellPair[1], GetDistance(cellPair[0], cellPair[1]));
        }

    }

    // Credit/Author: https://www.geeksforgeeks.org/orientation-3-ordered-points/
    // for details of below formula.
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

    // Credit/Author: https://www.geeksforgeeks.org/orientation-3-ordered-points/
    // Given three collinear points p, q, r, the function checks if
    // point q lies on line segment 'pr'
    private static Boolean onSegment(Vector3Int p, Vector3Int q, Vector3Int r)
    {
        if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
            q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
            return true;

        return false;
    }

    // Credit/Author: https://www.geeksforgeeks.org/orientation-3-ordered-points/
    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    private static int orientation(Vector3Int p, Vector3Int q, Vector3Int r)
    {
        int val = (q.y - p.y) * (r.x - q.x) -
                (q.x - p.x) * (r.y - q.y);

        if (val == 0) return 0; // collinear

        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }

    /// <summary>
    /// Based on the a* algorithm. Used to find a path between a pair of cells.
    /// </summary>
    /// <param name="map"></param>
    /// <param name="startCell"></param>
    /// <param name="endCell"></param>
    /// <param name="status"></param>
    /// <param name="intersectionsEnabled"></param>
    /// <returns></returns>
    protected bool findAStarPath(Map map, Cell startCell, Cell endCell, Cell.CellStatus status, bool intersectionsEnabled)
    {
        Heap<Cell> openList = new Heap<Cell>(map.area);
        HashSet<Cell> closedList = new HashSet<Cell>();
        Cell currentNode;
  
        openList.Add(startCell);

        while (openList.Count > 0)
        {
            currentNode = openList.RemoveFirst();

            closedList.Add(currentNode);

            if (currentNode == endCell)
            {
                currentNode = endCell;

                while (currentNode != startCell)
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

                    neighbourNode.hCost = GetDistance(neighbourNode, endCell);

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
