using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze  {
	//=======================================
	//      Variables
	//=======================================
	public Tile[,] tile;
	private List<Tile> inUsingTiles;

	public Maze(int width, int length)
	{
		tile = new Tile[width, length];
		inUsingTiles = new List<Tile> ();
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
}
