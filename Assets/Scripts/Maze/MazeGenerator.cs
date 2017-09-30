﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour {

    public int width = 10;
    public int length = 10;
    public MazeSetting m_setting;

    [HideInInspector]
    public Tile[,] Maze;


    // Use this for initialization
    void Start ()
    {
        BuildMaze();
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void BuildMaze()
    {
        MazeBlueprint MazeBP = new MazeBlueprint(width, length);

        Maze = new Tile[width, length];

		for (int i = 0; i < width; i++)
        {
			for (int j = 0; j < length; j++)
            {
				Maze [i, j] = GenerateTile (i, j, MazeBP);
            }
        }
    }

	// Generate script tile based on maze blueprint
    Tile GenerateTile(int X, int Z, MazeBlueprint MazeBP)
    {
		bool[] walls = new bool[4];
		int nb_walls = 0;
		GeoType geo_type;
		GameObject geo_obj;
		int rot_count = 0;

        // Get wall info from maze blueprint
        walls[0] = MazeBP.wall_h [X, Z + 1];	// N
		walls[1] = MazeBP.wall_v [X + 1, Z];	// E
		walls[2] = MazeBP.wall_h [X, Z];		// S
		walls[3] = MazeBP.wall_v [X, Z];        // W

        // Get number of walls
        for (int i = 0; i < walls.Length; i++)
		{
			if (walls[i])
				nb_walls++;
		}

        // Get geo type, then setup geo object
		if ((walls [0] != walls [1]) && (walls [0] == walls [2]) && (walls [1] == walls [3]))
			geo_type = GeoType.II;
		else
			geo_type = (GeoType)nb_walls;
		
		geo_obj = m_setting.GetGeoObj (geo_type);

        // Get rotation count for based on wall layout so we can rotate the geo later.
        rot_count = GetGeoRotationCount(walls, geo_type);

        // Spawn tile object
        GameObject tile_object = (GameObject)Instantiate (geo_obj, new Vector3 (X * 10, 0, Z * 10), Quaternion.Euler (0, 90 * rot_count, 0));
        tile_object.name = "Tile [" + X + "]" + "[" + Z + "] " + "(" + geo_type + ")";
        tile_object.AddComponent<Tile>();

        // Generate Tile class data
        Tile tile = tile_object.GetComponent<Tile>();
        tile.X = X;
        tile.Z = Z;
        tile.wall = walls;
        AssignWallObjToTile(tile, geo_type, rot_count);

        return tile;
    }
    
	// Instead of creating multiple type of geo, we rotate existing geo to match the wall layout.
	// This function calculate the rot_count that can be used later to spawn the tile geo.
	// ex: Quaternion.Euler (0, 90 * rot_count, 0))
	int GetGeoRotationCount(bool[] walls, GeoType geo_type)
	{
		int count = 0;

		if (geo_type == GeoType.II)
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
			int wall_index = i;
			bool match = false;
			for (int j = 0; j < (int)geo_type; j++) 
			{
				if (!walls [wall_index])
					break;

				if (j == ((int)geo_type - 1))
					match = true;

				wall_index = wall_index + 1 < walls.Length ? (wall_index + 1) : (wall_index + 1 - walls.Length);
			}

			if (match)
				count = i;
		}

		return count;
	}

	// Store wall object in tile class
    void AssignWallObjToTile(Tile tile, GeoType geo_type, int rot_count)
    {
		if (geo_type == GeoType.O)
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

		// Assign wall objects to Tile class based on geo type and geo rotation
		int wall_index = rot_count;
		tile.wall_obj [wall_index] = wall_obj_list [0];

		if (geo_type == GeoType.II)
		{
			wall_index = wall_index + 2 < 4 ? (wall_index + 2) : (wall_index + 2 - 4);
			tile.wall_obj [wall_index] = wall_obj_list [1];
		}
		else
		{
			for (int i = 1; i < wall_obj_list.Count; i++)
			{
				wall_index = wall_index + 1 < 4 ? (wall_index + 1) : (wall_index + 1 - 4);
				tile.wall_obj [wall_index] = wall_obj_list [i];
			}
		}
    }
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
    void GenerateBPLayout()
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

    void ReplaceIDs(int[] repl)
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

    bool IDsAllZero(int[] repl)
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