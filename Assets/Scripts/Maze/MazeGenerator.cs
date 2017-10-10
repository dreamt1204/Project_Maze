﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator {
    //=======================================
    //      Variables
    //=======================================
    private LevelManager levelManager;

    // Maze Layout
    private int width;
    private int length;
    private MazeSetting m_setting;

    // Game mode
    private GameObject startPrefab;
    private GameObject objPrefab;

    //=======================================
    //      Struct
    //=======================================
    public MazeGenerator(int mazeWidth, int mazeLength, MazeSetting mazeSetting, GameObject startPointPrefab, GameObject levelObjectivePrefa)
    {
        levelManager = LevelManager.GetLevelManager();

        width = mazeWidth;
        length = mazeLength;
        m_setting = mazeSetting;
        startPrefab = startPointPrefab;
        objPrefab = levelObjectivePrefa;
    }

    //=======================================
    //      Functions
    //======================================= 
    public Maze GenerateMaze()
    {
        Maze maze = BuildPlainMaze();
        GenerateGameModeObjects(maze);
        GenerateMazeItem(maze);

        return maze;
    }

    #region Generate maze layout
    // Public function that generates empty maze with wall layout
    public Maze BuildPlainMaze()
    {
        MazeBlueprint mazeBP = new MazeBlueprint(width, length);
		GameObject mazeObj = new GameObject (){name = "Maze"};
        //mazeObj.transform.position = new Vector3(0, -0.5f, 0);
        Maze maze = new Maze(width, length);

        for (int i = 0; i < width; i++)
        {
			for (int j = 0; j < length; j++)
            {
				maze.tile[i, j] = GenerateTile (i, j, mazeBP);
				maze.tile[i, j].transform.parent = mazeObj.transform;
                maze.tileList.Add(maze.tile[i, j]);
            }
        }

		return maze;
    }

    // Generate script tile based on maze blueprint
    public Tile GenerateTile(int X, int Z, MazeBlueprint mazeBP)
    {
		bool[] wall = new bool[4];
		int nbWalls = 0;
		WallLayout wallLayout;
		GameObject wallLayoutObj;
		int rotCount = 0;

        // Get wall info from maze blueprint
		wall[0] = mazeBP.wall_h [X, Z + 1];	// N
		wall[1] = mazeBP.wall_v [X + 1, Z];	// E
		wall[2] = mazeBP.wall_h [X, Z];		// S
		wall[3] = mazeBP.wall_v [X, Z];        // W

        // Get number of walls
		for (int i = 0; i < wall.Length; i++)
		{
			if (wall[i])
				nbWalls++;
		}

        // Get wall layout, then setup object
		if ((wall [0] != wall [1]) && (wall [0] == wall [2]) && (wall [1] == wall [3]))
			wallLayout = WallLayout.II;
		else
			wallLayout = (WallLayout)nbWalls;

        wallLayoutObj = m_setting.GetWallLayoutObj(wallLayout);

        // Get rotation count for based on wall layout so we can rotate the wall layout later.
        rotCount = GetLayoutRotationCount(wall, wallLayout);

        // Spawn tile object
        GameObject tileObj = GameObject.Instantiate (wallLayoutObj, new Vector3 (X * 10, -0, Z * 10), Quaternion.Euler (0, 90 * rotCount, 0));
        tileObj.name = "Tile [" + X + "]" + "[" + Z + "] " + "(" + wallLayout + ")";
        tileObj.AddComponent<Tile>();

        // Generate Tile class data
        Tile tile = tileObj.GetComponent<Tile>();
        tile.X = X;
        tile.Z = Z;
		tile.wall = wall;
        tile.wallLayout = wallLayout;
        AssignWallObjToTile(tile, wallLayout, rotCount);

        return tile;
    }

    // Instead of creating multiple type of wall layout, we rotate existing object to match the wall layout.
    // This function calculate the rotCount that can be used later to spawn the wall layout.
    // ex: Quaternion.Euler (0, 90 * rotCount, 0))
    private int GetLayoutRotationCount(bool[] walls, WallLayout wallLayout)
	{
		int count = 0;

		if (wallLayout == WallLayout.II)
		{
			int rnd = Random.Range (0, 2);
			if (walls [0] == true)
				count = 2 * rnd;
			else
				count = 2 * rnd + 1;

			return count;
		}

		for (int i = 0; i < walls.Length; i++) 
		{
			int wallID = i;
			bool match = false;
			for (int j = 0; j < (int)wallLayout; j++) 
			{
				if (!walls [wallID])
					break;

				if (j == ((int)wallLayout - 1))
					match = true;

				wallID = wallID + 1 < walls.Length ? (wallID + 1) : (wallID + 1 - walls.Length);
			}

			if (match)
				count = i;
		}

		return count;
	}

    // Store wall object in tile class
    private void AssignWallObjToTile(Tile tile, WallLayout wallLayout, int rotCount)
    {
		if (wallLayout == WallLayout.O)
			return;

		// Get wall objects
		Dictionary<int,GameObject> wall_obj_list = new Dictionary<int,GameObject>();

		foreach (Transform child in tile.gameObject.transform)
        {
            if (child.name.Contains("Wall_"))
            {
				int index = int.Parse(child.name.Substring(child.name.IndexOf ('_') + 1)) - 1;
				wall_obj_list.Add (index, child.gameObject);
            }
        }

        // Assign wall objects to Tile class based on wall layout and rotation
        int wallID = rotCount;
		tile.wall_obj [wallID] = wall_obj_list [0];

		if (wallLayout == WallLayout.II)
		{
			wallID = wallID + 2 < 4 ? (wallID + 2) : (wallID + 2 - 4);
			tile.wall_obj [wallID] = wall_obj_list [1];
		}
		else
		{
			for (int i = 1; i < wall_obj_list.Count; i++)
			{
				wallID = wallID + 1 < 4 ? (wallID + 1) : (wallID + 1 - 4);
				tile.wall_obj [wallID] = wall_obj_list [i];
			}
		}
    }
    #endregion

    #region Spawn Game Objects
    public void GenerateGameModeObjects(Maze maze)
    {
        Tile tileStart, tileObj;
        tileStart = Maze.GetRandomTileFromList(maze.tileList);

        // Make sure the objective is at least half map aways from the start point. Also, make it spawn at C shape wall layout. 
        List<Tile> orgs = new List<Tile>();
        orgs.Add(tileStart);
        List<Tile> tileList = Maze.UpdateTileListWithDistanceCondition(maze.tileList, orgs, (int)Mathf.Floor(maze.tile.GetLength(0) / 2));
        tileList = Maze.UpdateTileListWithDesiredWallLayout(tileList, WallLayout.C);
        tileObj = Maze.GetRandomTileFromList(tileList);

        tileStart.SpawnTileItem(startPrefab);
        tileObj.SpawnTileItem(objPrefab);

        levelManager.tileStart = tileStart;
        levelManager.tileObjective = tileObj;
    }
    #endregion

    #region Spawn Maze Items
    public void GenerateMazeItem(Maze maze)
    {
        // Calculate the number of items needed to spawn for this maze
        int numItems = (int)Mathf.Floor(Mathf.Sqrt((maze.tile.GetLength(0) * maze.tile.GetLength(1) / 25)));

        List<Tile> tiles = GetItemSpawnTiles(maze, numItems);
        List<BodyPart> partList = GetBodyPartList(numItems);

        for (int i = 0; i < numItems; i++)
        {
            TileItem item = tiles[i].SpawnTileItem(m_setting.bodyPartItem.gameObject);
            item.bodyPart = partList[i];
        }
    }

    public List<Tile> GetItemSpawnTiles(Maze maze, int numItems)
    {
        List<Tile> orgs = new List<Tile>();
        orgs.Add(levelManager.tileStart);
        orgs.Add(levelManager.tileObjective);

        List<Tile> exclusiveList = new List<Tile>();
        exclusiveList.Add(levelManager.tileStart);
        exclusiveList.Add(levelManager.tileObjective);

        List<Tile> tileList = Maze.UpdateTileListWithDistanceCondition(maze.tileList, orgs, (int)Mathf.Floor(maze.tile.GetLength(0) / 2));
        tileList = Maze.UpdateTileListWithExclusiveList(tileList, exclusiveList);
        tileList = Maze.UpdateTileListWithDesiredWallLayout(tileList, WallLayout.C);

        List<Tile> newList = new List<Tile>();
        int[] randomNumbers = Utilities.GetRandomUniqueNumbers(numItems, tileList.Count);
        for (int i = 0; i < numItems; i++)
        {
            newList.Add(tileList[randomNumbers[i]]);
        }

        return newList;
    }
    
    public List<BodyPart> GetBodyPartList(int numItems)
    {
        List<BodyPart> newList = new List<BodyPart>();

        int[] randomNumbers = Utilities.GetRandomUniqueNumbers(numItems, m_setting.bodyParts.Count);

        for (int i = 0; i < numItems; i++)
        {
            newList.Add(m_setting.bodyParts[randomNumbers[i]]);
        }

        return newList;
    }
    #endregion
}

// A maze blueprint class with constructor.
// Once taking maze size as input, it generates wall arrays using Kruskal's algorithm.
// The wall array can be used to build maze tile later.
public class MazeBlueprint
{
    public int m_length = 10;
    public int m_width = 10;
    public bool[,] wall_v;	// X, Z
	public bool[,] wall_h;	// X, Z
	public int[,] cell;		// X, Z  cell is the id of each inclosed section in Kruskal's algorithm.
    
    public MazeBlueprint(int width, int length)
    {
		// Setup maze blueprint with input size
		m_width = width;
		m_length = length;
		wall_v = new bool[m_width + 1, m_length];
		wall_h = new bool[m_width, m_length + 1];
		cell = new int[m_width, m_length];
        for (int i = 0; i < wall_v.GetLength(0); i++)
        {
            for (int j = 0; j < wall_v.GetLength(1); j++)
                wall_v[i, j] = true;
        }
        for (int i = 0; i < wall_h.GetLength(0); i++)
        {
            for (int j = 0; j < wall_h.GetLength(1); j++)
                wall_h[i, j] = true;
        }
        for (int i = 0; i < m_width; i++)
        {
            for (int j = 0; j < m_length; j++)
            {
                cell[i, j] = m_width * j + i;
            }
        }

		// Run Kruskal's algorithm to remove walls and get a perfect maze layout
        GenerateBPLayout();
    }

    // Generate maze blueprint using kruskal's algorithm
    private void GenerateBPLayout()
    {
        while (true)
        {
            int[] repl;

            if (Random.Range(0, 2) >= 1)
            {
                int x = Random.Range(1, wall_v.GetLength(0) - 1);
				int z = Random.Range(0, wall_v.GetLength(1));
				if (cell[x, z] == cell[x - 1, z])
                    continue;
				repl = new int[] { cell[x, z], cell[x - 1, z] };
                wall_v[x, z] = false;
            }
            else
            {
				int x = Random.Range(0, wall_h.GetLength(0));
				int z = Random.Range(1, wall_h.GetLength(1) - 1);
				if (cell[x, z] == cell[x, z - 1])
                    continue;
				repl = new int[] { cell[x, z], cell[x, z - 1] };
                wall_h[x, z] = false;
            }
            System.Array.Sort(repl);
            ReplaceIDs(repl);
            if (IDsAllZero(repl) == true)
                break;
        }
    }

    private void ReplaceIDs(int[] repl)
    {
        for (int w = 0; w < m_width; w++)
        {
            for (int l = 0; l < m_length; l++)
            {
                if (cell[w, l] == repl[1])
                    cell[w, l] = repl[0];
            }
        }
    }

    private bool IDsAllZero(int[] repl)
    {
        if (repl[0] != 0 && repl[1] != 0)
            return false;
        for (int w = 0; w < m_width; w++)
        {
            for (int l = 0; l < m_length; l++)
            {
                if (cell[w, l] != 0)
                    return false;
            }
        }
        return true;
    }
}