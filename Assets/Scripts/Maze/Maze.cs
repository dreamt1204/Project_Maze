using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze  {
	//=======================================
	//      Variables
	//=======================================
	public Tile[,] tile;
    public List<Tile> tileList;
    private List<Tile> inUsingTiles;

	public Maze(int width, int length)
	{
		tile = new Tile[width, length];
        tileList = new List<Tile>();
		inUsingTiles = new List<Tile> ();

		// JY removed step for populating tileList, since at instantiation "tile" array is pointing to empty values now (causing error)
		// Now, tileList is populated only after "tile" array is populated in BuildMaze function. (so reference is correct)
	}

	//=======================================
    //      Functions
    //=======================================
	// Check if the two tiles are neighbors
	public bool TilesAreNeighbors(Tile tile_1, Tile tile_2)
	{
		return ((Mathf.Abs(tile_1.X - tile_2.X) + Mathf.Abs(tile_1.Z - tile_2.Z)) == 1);	
	}

	// Check if there is a wall between these two tiles
	public bool WallInBetween(Tile tile_1, Tile tile_2)
	{
		return tile_1.wall [GetNeighborTileDir (tile_1, tile_2)];
	}

	// Return a list of neighbor tiles
	public List<Tile> GetNeighborTiles(Tile org)
	{
		List<Tile> neighbors = new List<Tile>();

		if ((org.Z + 1) < tile.GetLength(1))
			neighbors.Add (tile [org.X, org.Z + 1]);
		if ((org.X + 1) < tile.GetLength(0))
			neighbors.Add (tile [org.X + 1, org.Z]);
		if ((org.Z - 1) >= 0)
			neighbors.Add (tile [org.X, org.Z - 1]);
		if ((org.X - 1) >= 0)
			neighbors.Add (tile [org.X - 1, org.Z]);
		
		return neighbors;
	}

	// Get direction of the target tile. 
	public int GetNeighborTileDir(Tile org, Tile target)
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
	public Tile GetDirNeighborTile(Tile org, int dir)
	{
		int dir_x = dir == 1 ? 1 : dir == 3 ? -1 : 0;
		int dir_z = dir == 0 ? 1 : dir == 2 ? -1 : 0;
		int x = org.X + dir_x;
		int z = org.Z + dir_z;

		if ((x < 0) || (x > (tile.GetLength (0) - 1)))
			return null;
		if ((z < 0) || (z > (tile.GetLength (1) - 1)))
			return null;

		return tile [x, z];
	}

	// Return a list of walkable neighbor tiles
	public List<Tile> GetWalkableTiles(Tile org)
	{
		List<Tile> neighbors = GetNeighborTiles (org);
		List<Tile> walkables = new List<Tile>();

		foreach (Tile tile in neighbors)
		{
			if (!WallInBetween (org,tile))
				walkables.Add (tile);
		}

		return walkables;
	}

	// Set all the neighbor tiles to walkable state.
	public void UpdateWalkableTiles(Tile org)
	{
		// Reset all the tile state and clear inUsingTiles
		foreach (Tile tile in inUsingTiles)
		{
			tile.State = TileState.None;
		}
		inUsingTiles.Clear ();

		// Set walkable state to walkable neighbors and update inUsingTiles
		List<Tile> walkables = GetWalkableTiles (org);
		foreach (Tile tile in walkables)
		{
			if (tile.State != TileState.Walkable)
			{
				tile.State = TileState.Walkable;
				inUsingTiles.Add (tile);
			}	
		}
	}

    // Get a random tile from maze
    public Tile GetRandomTile()
    {
        return tile[Random.Range(0, tile.GetLength(0)), Random.Range(0, tile.GetLength(1))];
    }

    // Get a random tile from input list
    public Tile GetRandomTileFromList(List<Tile> tmpList)
    {
        return tmpList[Random.Range(0, tmpList.Count)];
    }

    //---------------------------------------
    //      Get tile list
    //---------------------------------------

    // Get tiles 'distance' away from the org tile
    public List<Tile> GetTileListOutOfDistance(Tile org, int distance)
    {
        List<Tile> newList = new List<Tile>();

        // Make sure distance is not out of bounds
        if (distance > (int)Mathf.Floor(tile.GetLength(0) / 2))
            distance = (int)Mathf.Floor(tile.GetLength(0) / 2);
        
        for (int i = 0; i < tile.GetLength(0); i++)
        {
            for (int j = 0; j < tile.GetLength(1); j++)
            {
                if (((i <= (org.X - distance)) || (i >= (org.X + distance))) ||
                    ((j <= (org.Z - distance)) || (j >= (org.Z + distance))))
                    newList.Add(tile[i, j]);
            }
        }

        return newList;
    }

    // Get tiles with desired wall layout from input list
    public List<Tile> GetTileListWithDesiredWallLayout(List<Tile> oldList, WallLayout desiredWallLayout)
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
    public List<Tile> UpdateTileListWithExclusiveList(List<Tile> oldList, List<Tile> exclusiveList)
    {
        List<Tile> newList = new List<Tile>();

        foreach (Tile t in oldList)
        {
            if (!exclusiveList.Contains(t))
                newList.Add(t);
        }

        return newList;
    }




	// =======================================
	// === Helper functions that Jay added ===
	// =======================================


	// Update tileList from tile array (has to be called everytime tile array changes);
	public void updateTileList(){
		for (int i = 0; i < tile.GetLength(0); i++)
		{
			for (int j = 0; j < tile.GetLength(1); j++)
			{
				tileList.Add(tile[i,j]);
			}
		}
	}


	// Distance between tiles by connectivity 4, without regard of wall (simple distance)
	public static int DistBtwTiles (Tile t1, Tile t2){
		int xDist = Mathf.RoundToInt (Mathf.Abs (t1.X - t2.X));
		int zDist = Mathf.RoundToInt (Mathf.Abs (t1.Z - t2.Z));
		return xDist + zDist;
	}

	public List<Tile> AllTilesAround(Tile centerTile, int distFrom){
		// e.g. if distFrom = 0, list length is 1 tile
		// e.g. if distFrom = 1, list length is 5 tiles
		List<Tile> tilesAround = new List<Tile>();

		foreach (Tile t in tileList) 
		{
			if (DistBtwTiles (t, centerTile) <= distFrom) 
			{
				tilesAround.Add (t);
			}
		}
		return tilesAround;
	}


}
