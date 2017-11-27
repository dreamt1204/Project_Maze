//============================== Class Definition ==============================
// 
// This a container class of basic maze info and maze tiles.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze
{
	//=======================================
	//      Variables
	//=======================================
	public int width;
	public int length;
    public GameObject groupObj;
	public Tile[,] tile;
    public List<Tile> tileList;

    public Dictionary<string, Room> roomList;
    public Dictionary<string, Hallway> hallwayList;
    
    public List<string> detectRegions;

    public SortedDictionary<int, List<Tile>> DeadEnds;
    public SortedDictionary<int, List<Tile>> DeadEndsWithItem;

    public Maze(int mazeWidth, int mazeLength)
	{
		width = mazeWidth;
		length = mazeLength;
        groupObj = new GameObject() { name = "Maze" };
        tile = new Tile[width, length];
		tileList = new List<Tile>();

        roomList = new Dictionary<string, Room>();
        hallwayList = new Dictionary<string, Hallway>();

        detectRegions = new List<string>();

        DeadEnds = new SortedDictionary<int, List<Tile>>();
        DeadEndsWithItem = new SortedDictionary<int, List<Tile>>();
}
}

public class Room
{
    public List<Tile> tileList;

    public Room()
    {
        tileList = new List<Tile>();
    }
}

public class Hallway
{
    public List<Tile> tileList;

    public Hallway()
    {
        tileList = new List<Tile>();
    }
}
