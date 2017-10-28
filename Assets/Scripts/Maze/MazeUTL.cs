//============================== Class Definition ==============================
// 
// This static class contains all the maze and tile related functions.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class MazeUTL {
    //=======================================
    //      Maze Static Functions
    //=======================================

    //---------------------------------------
    //      Get level maze info
    //---------------------------------------
    public static int GetMazeWidth()
    {
        return LevelManager.instance.mazeWidth;
    }

    public static int GetMazeLength()
    {
        return LevelManager.instance.mazeLength;
    }

    public static Tile[,] GetMazeTile()
    {
        return LevelManager.instance.maze.mazeTile;
    }

    public static List<Tile> GetMazeTileList()
    {
        return LevelManager.instance.maze.mazeTileList;
    }


    //---------------------------------------
    //      Check functions
    //---------------------------------------
    // Check if the two tiles are neighbors
    public static bool TilesAreNeighbors(Tile tile_1, Tile tile_2)
    {
        return (GetDistanceBetweenTiles(tile_1, tile_2) == 1);
    }

    // Check if there is a wall between two tiles
    public static bool WallBetweenNeighborTiles(Tile tile_1, Tile tile_2)
    {
        Utilities.TryCatchError(!TilesAreNeighbors(tile_1, tile_2), tile_1.gameObject.name + " and " + tile_2.gameObject.name + " are not neighbors. Can't check wall.");

        return tile_1.wall[GetNeighborTileDir(tile_1, tile_2)];
    }

    // Check if there is a wall on the direction of the current tile
    public static bool WallOnDir(Tile org, int dir)
    {
        return org.wall[dir];
    }

    public static bool CoordinatesOutOfMazeSize(int X, int Z)
    {
        bool outside = false;

        if (X > (GetMazeWidth() - 1))
            outside = true;
        else if (X < 0)
            outside = true;
        else if (Z > (GetMazeLength() - 1))
            outside = true;
        else if (Z < 0)
            outside = true;

        return outside;
    }


    //---------------------------------------
    //      Range check functions
    //---------------------------------------
    public static bool CheckTargetInRange(Tile org, Tile target, int range)
    {
        return (GetDistanceBetweenTiles(org, target) <= range);
    }

    public static bool CheckTargetOutOfRange(Tile org, Tile target, int range)
    {
        return (GetDistanceBetweenTiles(org, target) > range);
    }

    public static bool CheckTargetIsRightOnRange(Tile org, Tile target, int range)
    {
        return (GetDistanceBetweenTiles(org, target) == range);
    }


    //---------------------------------------
    //      Get direction functions
    //---------------------------------------
    // Get direction of the target tile. 
    public static int GetNeighborTileDir(Tile org, Tile target)
    {
        Utilities.TryCatchError(!TilesAreNeighbors(org, target), org.gameObject.name + " and " + target.gameObject.name + " are not neighbors. Can't get their relative direction.");

        int dir = 0;

        if (org.X == target.X)
            dir = (target.Z - org.Z) > 0 ? 0 : 2;
        else if (org.Z == target.Z)
            dir = (target.X - org.X) > 0 ? 1 : 3;

        return dir;
    }


    //---------------------------------------
    //      Get tile
    //---------------------------------------
    // Get a random tile from maze
    public static Tile GetRandomTile()
    {
        return GetMazeTile()[Random.Range(0, GetMazeWidth()), Random.Range(0, GetMazeLength())];
    }

    // Get a random tile from input list
    public static Tile GetRandomTileFromList(List<Tile> tileList)
    {
        return tileList[Random.Range(0, tileList.Count)];
    }

    // Return the neighbor tile with input direction 
    public static Tile GetDirNeighborTile(Tile org, int dir)
    {
        int dir_x = dir == 1 ? 1 : dir == 3 ? -1 : 0;
        int dir_z = dir == 0 ? 1 : dir == 2 ? -1 : 0;
        int x = org.X + dir_x;
        int z = org.Z + dir_z;

        if ((x < 0) || (x > (GetMazeWidth() - 1)))
            return null;
        if ((z < 0) || (z > (GetMazeLength() - 1)))
            return null;

        return GetMazeTile()[x, z];
    }


    //---------------------------------------
    //      Get tiles
    //---------------------------------------
    // Return a list of neighbor tiles
    public static List<Tile> GetNeighborTiles(Tile org)
    {
        List<Tile> neighbors = new List<Tile>();

        if ((org.Z + 1) < GetMazeLength())
            neighbors.Add(GetMazeTile()[org.X, org.Z + 1]);
        if ((org.X + 1) < GetMazeWidth())
            neighbors.Add(GetMazeTile()[org.X + 1, org.Z]);
        if ((org.Z - 1) >= 0)
            neighbors.Add(GetMazeTile()[org.X, org.Z - 1]);
        if ((org.X - 1) >= 0)
            neighbors.Add(GetMazeTile()[org.X - 1, org.Z]);

        return neighbors;
    }

    // Get tiles using Range Data
    public static List<Tile> GetRangeTiles(Tile org, RangeData rangeData)
    {
        List<Tile> newList = new List<Tile>();

        int startIndex = rangeData.range * -1;

        // If outOfRange is true, only check targetTiles condition
        if (rangeData.outOfRange)
        {
            List<Tile> targetTileList;

            if ((rangeData.targetTiles != null) && (rangeData.targetTiles.Count > 0))
                targetTileList = rangeData.targetTiles;
            else
                targetTileList = GetMazeTileList();

            foreach (Tile t in targetTileList)
            {
                if (CheckTargetOutOfRange(org, t, rangeData.range))
                    newList.Add(t);
            }
        }
        // Find all the tiles within 'square' range of org and do the range condition check
        else
        {
            for (int i = startIndex; i <= rangeData.range; i++)
            {
                for (int j = startIndex; j <= rangeData.range; j++)
                {
                    if (CoordinatesOutOfMazeSize(org.X + i, org.Z + j))
                        continue;

                    Tile tile = GetMazeTile()[org.X + i, org.Z + j];

                    if (!RangeData.CheckRangeDataCondition(org, tile, rangeData))
                        continue;

                    newList.Add(tile);
                }
            }
        }

        return newList;
    }


    //---------------------------------------
    //      Update tile list
    //---------------------------------------
    // Get tiles 'distance' away from the org tiles
    public static List<Tile> UpdateTileListOutOfRange(List<Tile> oldList, List<Tile> orgs, int range)
    {
        if (orgs.Count <= 0)
            return oldList;

        Tile org = orgs[orgs.Count - 1];

        // Make a rangeData for tiles out of range
        RangeData rangeData = new RangeData();
        rangeData.range = range;
        rangeData.outOfRange = true;
        rangeData.targetTiles = oldList;

        List<Tile> newList = GetRangeTiles(org, rangeData);
        orgs.Remove(org);

        // Recursively go over all the org and update the list
        newList = UpdateTileListOutOfRange(newList, orgs, range);

        return newList;
    }

    // Get tiles with desired wall layout from input list
    public static List<Tile> UpdateTileListWithDesiredWallLayout(List<Tile> oldList, WallLayout desiredWallLayout)
    {
        List<Tile> newList = new List<Tile>();

        foreach (Tile t in oldList)
        {
            if (t.wallLayout == desiredWallLayout)
                newList.Add(t);
        }

        Utilities.TryCatchError((newList.Count <= 0), "There is no such wall layout in the input list.");

        return newList;
    }

    // Update tile list with exclusive list
    public static List<Tile> UpdateTileListWithExclusiveList(List<Tile> oldList, List<Tile> exclusiveList)
    {
        List<Tile> newList = new List<Tile>();

        foreach (Tile t in oldList)
        {
            if (!exclusiveList.Contains(t))
                newList.Add(t);
        }

        return newList;
    }


    //---------------------------------------
    //      Tile state
    //---------------------------------------
    // Reset all tiles' states to none
    public static void ResetAllTileState()
    {
        foreach (Tile tile in GetMazeTileList())
        {
            tile.ResetTileState();
        }
    }


    //---------------------------------------
    //      Misc
    //---------------------------------------
    public static int GetMazeShortestSide()
    {
        return (int)Mathf.Min(GetMazeWidth(), GetMazeLength());
    }

    public static int GetDistanceBetweenTiles(Tile tile_1, Tile tile_2)
    {
        return (int)Mathf.Abs(tile_2.X - tile_1.X) + Mathf.Abs(tile_2.Z - tile_1.Z);
    }


	// ===== JY added for path-finding =====

	// Writes address to each tile in maze
	// Currently assumes maze has no circular route (e.g. minSpanningTree); otherwise goes into infinite loop
	public static void AssignAddressToTiles(Maze maze)
	{
		// Origin is the tile where all addresses are based on; any tile works, but center node is most efficient
		Tile origin = maze.mazeTile [(maze.mazeWidth / 2) - 1, (maze.mazeLength / 2) - 1];

		origin.addr = new List<Tile> ();
		origin.addr.Add (origin);
		WriteAddressToNeighbors (origin, null); // Recursive; calls itself until all tiles in maze is assigned an address

		// Debug.Log ("Origin is at X = " + origin.X + "; Z = " + origin.Z);
	}

	// Recursive Helper 
	public static void WriteAddressToNeighbors(Tile current, Tile parent)
	{
		if (current.addr == null) {
			if (current.addr.Count == 0) {
				Debug.LogError ("Assigner tile has no address");
			}
		}
		for (int dir = 0; dir < 4; dir ++)
		{
			Tile child = GetDirNeighborTile (current, dir);
			if (child != null) {
				if (!WallBetweenNeighborTiles(current, child)) {
					if (child != parent) {
						child.addr = new List<Tile> (current.addr);
						child.addr.Add (child);
						WriteAddressToNeighbors (child, current); // Recursive call
					}
				}
			}
		}
	}

	public static List<Tile> GetNeighborTilesWithoutWall (Tile t) {
		List<Tile> tiles = MazeUTL.GetNeighborTiles (t);
		tiles.RemoveAll (tile0 => MazeUTL.WallBetweenNeighborTiles (t, tile0));

		if (tiles.Count == 0)
			Debug.LogError ("No possible tiles to walk towards.");

		return tiles;
	}

	// Finds path based on address method 
	// (vulnerable to maze modification after initialization)
	// For convience of using, output does NOT include starting tile;  Output DOES include target tile
	// e.g. A--B--C--D--E
	// FindPathByAddress(A, E) returns B--C--D--E
	// Returns empty list if A == E

	public static List<Tile> FindPathByAddress (Tile startTile, Tile endTile)
	{
		// Return empty list if start == end
		if (startTile == endTile) {
			return new List<Tile> ();
		}

		// Find the last common element in address
		int index = 0;
		while (true) 
		{
			// This case happens when startTile or endTile is right on the address of the other tile.
			// e.g. startTile.addr is A--B--C
			//      endTile.addr   is A--B--C--D--E
			// There will be no "tile of first differing index"
			if (startTile.addr.Count < index + 1 | endTile.addr.Count < index + 1) {
				index = index - 1;
				break;
			}

			if (startTile.addr [index] != endTile.addr [index]) {
				index = index - 1; // the last common index is one before the first differing index 
				break;
			}
			index++;
		}

		// Debug.Log ("Index of common node is " + index + "; copied list counts are " + (startTile.addr.Count - index) + " and " + (endTile.addr.Count - index));

		// Get Tile List from common element to end
		List<Tile> subPath1 = startTile.addr.GetRange (index, startTile.addr.Count - index); 
		List<Tile> subPath2 = endTile.addr.GetRange (index, endTile.addr.Count - index);

		subPath1.RemoveAt (subPath1.Count-1); // Return list should not contain the startTile (list starts with next tile)
		subPath1.Reverse();
		// Remove the shared element; if startTile is origin, then subPath1 will be empty, so in that case take away from subPath2
		if (subPath1.Count > 0) {
			subPath1.RemoveAt (subPath1.Count-1);  
		} else {
			subPath2.RemoveAt (0);
		}
		List<Tile> path = subPath1;
		path.AddRange (subPath2);

		return path;
	}

	public static void PrintAddress (Tile t)
	{
		if (t.addr == null)
			Debug.LogError ("Tile has no address");
		if (t.addr.Count == 0)
			Debug.LogError ("Tile address has no elements");

		Debug.Log ("=== Printing address of Tile X = " + t.X + "; Z = " + t.Z + "===");
		for (int i = 0; i < t.addr.Count; i++) {
			Debug.Log ("Element#" + (i + 1) + " : X = " + t.addr [i].X + "; Z = " + t.addr [i].Z);
		}
	}

	public static void PrintTileList (List<Tile> tileList, string listName)
	{
		Debug.Log ("=== Printing Tile List '" + listName + "' ===");
		if (tileList == null) {
			Debug.Log ("TileList is null.");
		}
		else {
			if (tileList.Count == 0) {
				Debug.Log ("TileList is empty.");
			} else {
				for (int i = 0; i < tileList.Count; i++) {
					Debug.Log ("Tile#" + (i + 1) + " : X = " + tileList [i].X + "; Z = " + tileList [i].Z);
				}
			}
		}
	}

}