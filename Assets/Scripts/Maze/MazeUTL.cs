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

    public static bool CheckTargetInRangeAndDetectRegion(Tile org, Tile target, int range)
    {
        return (CheckTargetInRange(org, target, range) && CheckTargetInDetectRegion(org, target));
    }

	//---------------------------------------
	//      Region functions
	//---------------------------------------
	public static string GetTileAddress(int X, int Z)
	{
		return X + "," + Z + "/";
	}

	public static bool CheckTargetInDetectRegion(Tile org, Tile target)
	{
		return (GetSharedDetectRegion(org, target) != null);
	}

	public static bool CheckRegionHasAddress(string region, string targetAddress)
	{
		string[] addressList = region.Split('/');
		foreach (string address in addressList)
		{
			if ((address + "/") == targetAddress)
				return true;
		}

		return false;
	}

	public static string GetSharedDetectRegion(Tile org, Tile target)
	{
		string targetAddress = MazeUTL.GetTileAddress(target.X, target.Z);
		foreach (string region in org.detectRegions)
		{
			if (CheckRegionHasAddress(region, targetAddress))
				return region;
		}
		return null;
	}

	public static List<Tile> GetTilesFromRegion(string region)
	{
		List<Tile> tileList = new List<Tile> ();
		string[] addressList = region.Split('/');

		foreach (string address in addressList)
		{
			if (address == "")
				continue;

			string[] XZ = address.Split(',');
			int X = int.Parse(XZ[0]);
			int Z = int.Parse(XZ[1]);

			tileList.Add (LevelManager.instance.maze.mazeTile [X, Z]);
		}

		return tileList;
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

	public static List<Tile> GetTilesRightOnRange(Tile org, List<Tile> tileList, int range)
	{
		List<Tile> newList = new List<Tile> ();

		foreach (Tile tile in tileList)
		{
			if (CheckTargetIsRightOnRange (org, tile, range))
				newList.Add (tile);
		}

		return newList;
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

    public static List<Tile> GetNeighborTilesWithoutWall(Tile org)
    {
        List<Tile> newList = new List<Tile>();
        List <Tile> neighbors = GetNeighborTiles(org);

        foreach (Tile tile in neighbors)
        {
            if (!WallBetweenNeighborTiles(org, tile))
                newList.Add(tile);
        }

        return newList;
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

		List<Tile> newOrgs = new List<Tile>();
		foreach (Tile tile in orgs)
		{
			if (tile != org)
				newOrgs.Add (tile);
		}

        // Recursively go over all the org and update the list
		newList = UpdateTileListOutOfRange(newList, newOrgs, range);

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
	//      Path finding
	//---------------------------------------
	public static List<Tile> GetShortestPath(Tile org, Tile target, int SearchRange)
	{
		List<Tile> shortestPath = new List<Tile> ();

		List<List<Tile>> paths = GetPathsToTarget(org, target, SearchRange, new List<Tile>());
		if (paths.Count <= 0)
			return shortestPath;

		shortestPath = paths[0];

		foreach (List<Tile> path in paths)
		{
			if (path.Count < shortestPath.Count)
				shortestPath = path;
		}

		return shortestPath;
	}

	public static List<List<Tile>> GetPathsToTarget(Tile org, Tile target, int SearchRange, List<Tile> currentPath)
	{
		List<Tile> newCurrentPath = new List<Tile>(currentPath);
		newCurrentPath.Add(org);

		List<List<Tile>> paths = new List<List<Tile>>();

		// If we reach the target, stop finding and return the paths
		if (org == target)
		{
			paths.Add(newCurrentPath);
            newCurrentPath.RemoveAt(0); // Remove the start tile for unit movement
            return paths;
		}

		// If reach the search range but not reach the target yet, stop finding and return null
		SearchRange--;
		if (SearchRange < 0)
			return null;

		// Get all possible continue finding directions
		List<int> continueDir = new List<int>();
		for (int i = 0; i < 4; i++)
		{
			if (WallOnDir(org, i))
				continue;

			if (newCurrentPath.Contains((GetDirNeighborTile(org, i))))
				continue;

			continueDir.Add(i);
		}

		// If reach dead end but not reach the target yet, stop finding and return null
		if (continueDir.Count == 0)
			return null;

		// Recursive search for are possible directions
		foreach (int dir in continueDir)
		{
			List<List<Tile>> newPaths = GetPathsToTarget(GetDirNeighborTile(org, dir), target, SearchRange, newCurrentPath);

			if (newPaths == null)
				continue;

			foreach (List<Tile> newPath in newPaths)
			{
				paths.Add(newPath);
			}
		}

		return paths;
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
}