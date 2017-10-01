using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    public Tile[,] tile;

    //=======================================
    //      Functions
    //=======================================
	public bool TilesAreNeighbors(Tile tile_1, Tile tile_2)
	{
		return ((Mathf.Abs(tile_1.X - tile_2.X) + Mathf.Abs(tile_1.Z - tile_2.Z)) == 1);	
	}

	public bool WallInBetween(Tile tile_1, Tile tile_2)
	{
		int dir = GetTileDir (tile_1, tile_2);

		return tile_1.wall [dir];
	}

	public int GetTileDir(Tile org, Tile taget)
	{
		int dir = 0;

		if (org.X == taget.X)
		{
			if ((taget.Z - org.Z) >= 0)
				dir = 0;
			else
				dir = 2;
		}
		else if (org.Z == taget.Z)
		{
			if ((taget.X - org.X) >= 0)
				dir = 1;
			else
				dir = 3;
		}

		return dir;
	}
}
