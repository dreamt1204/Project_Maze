using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze  {
	//=======================================
	//      Variables
	//=======================================
	public static int width;
	public static int length;
	public static Tile[,] mazeTile;
    public static List<Tile> mazeTileList;
    private static List<Tile> inUsingTiles;

	public Maze(int mazeWidth, int mazeLength)
	{
		width = mazeWidth;
		length = mazeLength;
		mazeTile = new Tile[width, length];
		mazeTileList = new List<Tile>();
		inUsingTiles = new List<Tile> ();
	}

    #region Maze Static Functions
    //=======================================
    //      Maze Static Functions
    //=======================================
    // Check if there is a wall between these two tiles
    public static bool WallInBetween(Tile tile_1, Tile tile_2)
    {
        return tile_1.wall[GetNeighborTileDir(tile_1, tile_2)];
    }

    //---------------------------------------
    //      Neighbor functions
    //---------------------------------------
    // Check if the two tiles are neighbors
    public static bool TilesAreNeighbors(Tile tile_1, Tile tile_2)
    {
        return ((Mathf.Abs(tile_1.X - tile_2.X) + Mathf.Abs(tile_1.Z - tile_2.Z)) == 1);
    }

    // Return a list of neighbor tiles
    public static List<Tile> GetNeighborTiles(Tile org)
    {
        List<Tile> neighbors = new List<Tile>();

		if ((org.Z + 1) < mazeTile.GetLength(1))
			neighbors.Add(mazeTile[org.X, org.Z + 1]);
		if ((org.X + 1) < mazeTile.GetLength(0))
			neighbors.Add(mazeTile[org.X + 1, org.Z]);
        if ((org.Z - 1) >= 0)
			neighbors.Add(mazeTile[org.X, org.Z - 1]);
        if ((org.X - 1) >= 0)
			neighbors.Add(mazeTile[org.X - 1, org.Z]);

        return neighbors;
    }

    // Get direction of the target tile. 
    public static int GetNeighborTileDir(Tile org, Tile target)
	{
		int dir = 0;

		if (!TilesAreNeighbors (org, target))
			return dir;

		if (org.X == target.X)
		{
			if ((target.Z - org.Z) > 0)
				dir = 0;
			else
				dir = 2;
		}
		else if (org.Z == target.Z)
		{
			if ((target.X - org.X) > 0)
				dir = 1;
			else
				dir = 3;
		}

		return dir;
	}

    // Return the neighbor tile with input direction 
    public static Tile GetDirNeighborTile(Tile org, int dir)
    {
        int dir_x = dir == 1 ? 1 : dir == 3 ? -1 : 0;
        int dir_z = dir == 0 ? 1 : dir == 2 ? -1 : 0;
        int x = org.X + dir_x;
        int z = org.Z + dir_z;

		if ((x < 0) || (x > (mazeTile.GetLength(0) - 1)))
            return null;
		if ((z < 0) || (z > (mazeTile.GetLength(1) - 1)))
            return null;

		return mazeTile[x, z];
    }

    //---------------------------------------
    //      Get tile / tiles
    //---------------------------------------
    // Get a random tile from maze
    public static Tile GetRandomTile()
    {
		return mazeTile[Random.Range(0, mazeTile.GetLength(0)), Random.Range(0, mazeTile.GetLength(1))];
    }

    // Get a random tile from input list
    public static Tile GetRandomTileFromList(List<Tile> tmpList)
    {
        return tmpList[Random.Range(0, tmpList.Count)];
    }
    
    //---------------------------------------
    //      Update tile list
    //---------------------------------------
    // Get tiles 'distance' away from the org tiles
    public static List<Tile> UpdateTileListOutOfRange(List<Tile> oldList, List<Tile> orgs, int range)
    {
        if (orgs.Count <= 0)
            return oldList;

        List<Tile> newList = new List<Tile>();

        Tile org = orgs[orgs.Count - 1];
        foreach (Tile t in oldList)
        {
            int distance = Mathf.Abs(t.X - org.X) + Mathf.Abs(t.Z - org.Z);
            if (distance >= range)
                newList.Add(t);
        }
        orgs.Remove(org);

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

        if (newList.Count <= 0)
            Debug.LogError("ERROR: There is no such wall layout in the input list.");

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
    // Return a list of walkable neighbor tiles
    public static List<Tile> GetWalkableTiles(Tile org)
    {
        List<Tile> neighbors = GetNeighborTiles(org);
        List<Tile> walkables = new List<Tile>();

        foreach (Tile tile in neighbors)
        {
            if (!WallInBetween(org, tile))
                walkables.Add(tile);
        }

        return walkables;
    }

    // Set all the neighbor tiles to walkable state.
    public static void UpdateWalkableTiles(Tile org)
    {
        // Reset all the tile state and clear inUsingTiles
        foreach (Tile tile in inUsingTiles)
        {
			tile.ResetTileState ();
        }
        inUsingTiles.Clear();

        // Set walkable state to walkable neighbors and update inUsingTiles
        List<Tile> walkables = GetWalkableTiles(org);
        foreach (Tile tile in walkables)
        {
            if (tile.State != TileState.Walkable)
            {
                tile.State = TileState.Walkable;
                inUsingTiles.Add(tile);
            }
        }
    }

	// Reset all tiles' states to none
	public static void ResetAllTileState()
	{
		foreach (Tile tile in mazeTileList)
		{
			tile.ResetTileState ();
		}
	}
    #endregion

    //---------------------------------------
    //      Misc
    //---------------------------------------
    public static int GetMazeShortestSide()
    {
        int shortSideLength = 0;

        int mazeWidth = GameObject.Find("LevelManager").GetComponent<LevelManager>().mazeWidth;
        int mazeLength = GameObject.Find("LevelManager").GetComponent<LevelManager>().mazeLength;
        shortSideLength = mazeWidth <= mazeLength ? mazeWidth : mazeLength;

        return shortSideLength;
    }

	//---------------------------------------
	//      Get range tiles
	//---------------------------------------
	public static Tile GetMazeTile(int X, int Z)
	{
		if ((X > (width - 1)) || (X < 0))
			return null;
		if ((Z > (length - 1)) || (Z < 0))
			return null;

		return mazeTile [X, Z];
	}

	public static List<Tile> GetRangeTiles(Tile org, RangeData rangeData)
	{
		List<Tile> newList = new List<Tile> ();

		int startIndex = rangeData.range * -1;

		if (rangeData.outOfRange)
		{
			foreach (Tile t in mazeTileList)
			{
				if (CheckTargetOutOfRange (org, t, rangeData.range))
					newList.Add (t);
			}
		}
		else
		{
			for (int i = startIndex; i <= rangeData.range; i++)
			{
				for (int j = startIndex; j <= rangeData.range; j++)
				{
					Tile tile = GetMazeTile (org.X + i, org.Z + j);

					if (!CheckRangeDataCondition(org, tile, rangeData))
						continue;

					newList.Add (tile);
				}
			}
		}

		return newList;
	}

	static bool CheckRangeDataCondition(Tile org, Tile target, RangeData rangeData)
	{
		if (target == null)
			return false;

		if (!CheckTargetInRange (org, target, rangeData.range))
			return false;

		if (rangeData.excludeOrigin)
		{
			if (org == target)
				return false;
		}

		if (rangeData.includeRangeTilesOnly)
		{
			if (!CheckTargetIsRightOnRange (org, target, rangeData.range))
				return false;
		}

		return true;
	}

	static bool CheckTargetInRange(Tile org, Tile target, int range)
	{
		bool pass = false;
		int distance = Mathf.Abs(target.X - org.X) + Mathf.Abs(target.Z - org.Z);

		if ((distance < GetMazeShortestSide ()) && (distance >= 0))
			pass = (distance <= range);

		return pass;
	}

	static bool CheckTargetOutOfRange(Tile org, Tile target, int range)
	{
		bool pass = false;
		int distance = Mathf.Abs(target.X - org.X) + Mathf.Abs(target.Z - org.Z);

		if ((distance < GetMazeShortestSide ()) && (distance >= 0))
			pass = (distance > range);

		return pass;
	}

	static bool CheckTargetIsRightOnRange(Tile org, Tile target, int range)
	{
		int distance = Mathf.Abs(target.X - org.X) + Mathf.Abs(target.Z - org.Z);

		return (distance == range);
	}
}

[System.Serializable]
public class RangeData
{
	public int range = 0;
	public bool outOfRange = false;
	public bool excludeOrigin = false;
	public bool includeRangeTilesOnly = false;
}
