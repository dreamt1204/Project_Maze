//============================== Class Definition ==============================
// 
// This a container class of basic maze info and maze tiles.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze  {
	//=======================================
	//      Variables
	//=======================================
	public int mazeWidth;
	public int mazeLength;
	public Tile[,] mazeTile;
    public List<Tile> mazeTileList;

	public Maze(int width, int length)
	{
		mazeWidth = width;
		mazeLength = length;
		mazeTile = new Tile[mazeWidth, mazeLength];
		mazeTileList = new List<Tile>();
	}
}
