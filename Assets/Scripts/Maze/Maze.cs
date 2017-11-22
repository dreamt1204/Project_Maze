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
    public GameObject mazeGroupObj;
	public Tile[,] mazeTile;
    public List<Tile> mazeTileList;
    public List<Room> roomList;
    public Dictionary<string, List<Tile>> hallwayTileList = new Dictionary<string, List<Tile>>();
    [HideInInspector] public List<Room> allocatedRoomList;
    public SortedDictionary<int, List<Tile>> DeadEnds = new SortedDictionary<int, List<Tile>>();
    public SortedDictionary<int, List<Tile>> DeadEndsWithItem = new SortedDictionary<int, List<Tile>>();

    public List<string> detectRegions;

	public Maze(int width, int length)
	{
		mazeWidth = width;
		mazeLength = length;
        mazeGroupObj = new GameObject() { name = "Maze" };
        mazeTile = new Tile[mazeWidth, mazeLength];
		mazeTileList = new List<Tile>();
        roomList = new List<Room>();
        allocatedRoomList = new List<Room>();
        detectRegions = new List<string>();
    }
}

public class Room
{
    public GameObject prefab;

    public bool allocated;

    public int left;
    public int top;
    public int right;
    public int bot;
    public int width;
    public int length;

    public Tile[,] roomTile;
    public List<Tile> roomTileList;
}
