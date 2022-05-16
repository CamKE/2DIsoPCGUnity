using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for creating all path related elements in the level.
/// </summary>
public class PathGenerator
{
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
        int searchLimit = map.terrainCellCount;
        // limit search count to an abritrary value to prevent infinite searching for non-existent valid cell pair.
        // not an optimal solution but works well. Need to revisit
        while (!pairFound && searchCount < searchLimit)
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

    // Credit/Author: https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
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

    // Credit/Author: https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
    // Given three collinear points p, q, r, the function checks if
    // point q lies on line segment 'pr'
    private static Boolean onSegment(Vector3Int p, Vector3Int q, Vector3Int r)
    {
        if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
            q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
            return true;

        return false;
    }

    // Credit/Author: https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
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
    /// <param name="map">The map to find the path on.</param>
    /// <param name="startCell">The start of the path.</param>
    /// <param name="endCell">The end goal of the path.</param>
    /// <param name="status">Defines the type of path to be generated.</param>
    /// <param name="intersectionsEnabled">Whether or not intersections are enabled for the path.</param>
    /// <returns>Whether or not the path was generated.</returns>
    protected bool findAStarPath(Map map, Cell startCell, Cell endCell, Cell.CellStatus status, bool intersectionsEnabled)
    {
        // make the start and end cells traversable
        // need to be done as the cells are boundary cells
        // which are not traversable by default
        startCell.isTraversable = true;
        endCell.isTraversable = true;
        // create a heap with size of the maps area. this is the maximum amount of cells
        // that could possibly be in the open list
        Heap<Cell> openList = new Heap<Cell>(map.area);
        // create the closed list as a hashset to ensure no duplicate cells.
        HashSet<Cell> closedList = new HashSet<Cell>();
        // variable for the current cell
        Cell currentCell;
  
        // add the start cell to the open list
        openList.Add(startCell);

        // as long as the open list is not empty
        while (openList.Count > 0)
        {
            // get the first cell is the open list heap (sorted by lowest cost)
            currentCell = openList.RemoveFirst();
            // add the cell to the closed list as we are now visiting it
            closedList.Add(currentCell);

            // if we have found a path to the end cell
            if (currentCell == endCell)
            {
                // trace path back to start

                // while we have not reached the start cell
                while (currentCell != startCell)
                {
                    // update the cells on the map to the status and intersection enabled flag
                    map.updateCellStatus(currentCell, status, intersectionsEnabled);

                    // move onto the preceding cell
                    currentCell = currentCell.parent;
                }
                // were at the start cell now, update it
                map.updateCellStatus(currentCell, status, intersectionsEnabled);

                // we found a path and update the corresponding cells, success
                return true;
            }

            // for each of neighbours of the current cell
            foreach (Cell neighbour in map.getNeighbours(currentCell))
            {
                // if the path status is river
                if (status == Cell.CellStatus.RiverCell)
                {
                    // if the neighbour is not traversable or the neighbour is in the closed list, or intersections are disabled
                    // and the cell is a water bound
                    if (!neighbour.isTraversable || closedList.Contains(neighbour) || !intersectionsEnabled && neighbour.isWaterBound)
                    {
                            // skip this iteration, continue to the next
                            continue;
                    }
                } 
                else
                // otherwise
                {
                    // if the neighbour is not traversable and the status is not a river cell or the neighbour is in the closed llist
                    if ((!neighbour.isTraversable && neighbour.status != Cell.CellStatus.RiverCell) || closedList.Contains(neighbour))
                    {
                        // skip this iteration, continue to the next
                        continue;
                    }
                }

                // wokout the neighbours new g csost
                int newNeighbourGCost = currentCell.gCost + GetDistance(currentCell, neighbour);
                // if the new g cost is less than old g cost or the cell is not already in the openlist
                if (newNeighbourGCost < neighbour.gCost || !openList.Contains(neighbour))
                {
                    // set the neighbours g cost to the new value
                    neighbour.gCost = newNeighbourGCost;

                    // calculate the cells h cost
                    neighbour.hCost = GetDistance(neighbour, endCell);

                    // set the parent of the neighbour to be the current cell being evaluated
                    neighbour.parent = currentCell;

                    // if the cell is not already in the open list
                    if (!openList.Contains(neighbour))
                    {
                        // add the neighbour to the open list
                        openList.Add(neighbour);
                    }
                }
            }
        }
        // could not find a path, make the boundary cells false again
        startCell.isTraversable = false;
        endCell.isTraversable = false;
        return false;
    }

    // get the distance between two cells
    private int GetDistance(Cell startCell, Cell endCell)
    {
        // get the absolute difference between the start and end x and y positions
        int xDistance = Mathf.Abs(startCell.position.x - endCell.position.x);
        int yDistance = Mathf.Abs(startCell.position.y - endCell.position.y);

        // cost to travel across a cell
        int travelCost;

        switch(endCell.status)
        {
            // higher cost to travel across a river or walkpath cell
            case Cell.CellStatus.RiverCell:
            case Cell.CellStatus.WalkpathCell:
                travelCost = 15;
                break;
            default:
                travelCost = 10;
                break;
        }

        // calculate the distance
        return travelCost * (yDistance + xDistance);
    }

    protected int createPaths(Map map, int maxCount, bool intersectionsEnabled, Cell.CellStatus type)
    {
        // create the new cellpair heap
        Heap<CellPair> cellPairs = new Heap<CellPair>(maxCount);

        // find cell pairs up to the max river count
        int count;
        for (count = 0; count < maxCount; count++)
        {
            // get a pair of reachable cells
            CellPair pair = getReachableCells(map, cellPairs, intersectionsEnabled);

            // if we could not get a reachable pair
            if (pair == null)
            {
                // stop searching for cell pairs
                break;
            }

            // add the pair found to the list of pairs
            cellPairs.Add(pair);
        }

        // while there are cellpairs in the heap
        while (cellPairs.Count > 0)
        {
            // we find paths for shortest distance cells first to avoid longer distance
            //  pairs preventing paths being found for shorter distance pairs when intersections are
            // disabled

            // remove the first pair of cells according to shortest distance between pair
            CellPair cellPair = cellPairs.RemoveFirst();

            // path is not found between the cell pair
            if (!findAStarPath(map, cellPair.startCell, cellPair.endCell, type, intersectionsEnabled))
            {
                // reduce the count
                count--;
            }
        }

        return count;
    }
}
