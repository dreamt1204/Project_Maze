using System.Collections;
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

    /*
    Tile GenerateTile(int X, int Z, MazeBlueprint MazeBP)
    {
        GameObject tile_object = GenerateTileObecjt(X, Z, MazeBP);
        tile_object.AddComponent<Tile>();
        
        Tile tile = tile_object.GetComponent<Tile>();
        tile.X = X;
        tile.Z = Z;

        return tile;
    }
    */


    Tile GenerateTile(int X, int Z, MazeBlueprint MazeBP)
    {
        bool[] walls = new bool[4];
        int nb_walls = 0;
        GameObject geo = m_setting.TileGeoO [Random.Range(0, m_setting.TileGeoO.Length)];
        string geo_type = "O";
		int rot_count = 0;

        // Get wall info from Maze Blueprint
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

        // Get geo object and type
		switch (nb_walls)
		{
			default:
            {
                geo = m_setting.TileGeoO[Random.Range(0, m_setting.TileGeoO.Length)];
                geo_type = "O";
                break;
            }
            case 1:
			{
				geo = m_setting.TileGeoI [Random.Range(0, m_setting.TileGeoI.Length)];
                geo_type = "I";
                break;
			}
			case 2:
			{
				if ( (walls[0] == walls[2]) && (walls[1] == walls[3]) )
				{
					geo = m_setting.TileGeoII [Random.Range(0, m_setting.TileGeoII.Length)];
                    geo_type = "II";
                    break;
				}
				else
				{
					geo = m_setting.TileGeoL [Random.Range(0, m_setting.TileGeoL.Length)];
                    geo_type = "L";
                    break;
				}
			}
			case 3:
			{
				geo = m_setting.TileGeoC [Random.Range(0, m_setting.TileGeoC.Length)];
                geo_type = "C";
				break;
			}
		}

        // Get rotation count for based on wall layout so we can rotate the geo later.
        rot_count = GetGeoRotationCount(walls, geo_type);

        // Spawn tile object
        GameObject tile_object = (GameObject)Instantiate (geo, new Vector3 (X * 10, 0, Z * 10), Quaternion.Euler (0, 90 * rot_count, 0));
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
    
	int GetGeoRotationCount(bool[] walls, string geo_type)
	{
		int count = 0;

        if (geo_type == "I")
        {
            for (int i = 0; i < walls.Length; i++)
            {
                if (walls[i])
                {
                    count = i;
                    break;
                }
            }
        }
        else if (geo_type == "II") 
		{
			int rnd = Random.Range (0, 2);
			if (walls [0] == true)
				count = 2 * rnd;
			else
				count = 2 * rnd + 1;
		}
		else if (geo_type == "L") 
		{
			for (int i = 0; i < walls.Length; i++) 
			{
				int j = i + 1 < walls.Length ? (i + 1) : (i + 1 - walls.Length);
				if (walls [i] && walls [j])
				{
					count = i;
					break;
				}
			}
		}
		else if (geo_type == "C") 
		{
			for (int i = 0; i < walls.Length; i++) 
			{
				int j = i + 1 < walls.Length ? (i + 1) : (i + 1 - walls.Length);
				int k = j + 1 < walls.Length ? (j + 1) : (j + 1 - walls.Length);

				if (walls [i] && walls [j] && walls [k])
				{
					count = i;
					break;
				}
			}
		}

		return count;
	}

    void AssignWallObjToTile(Tile tile, string geo_type, int rot_count)
    {
        GameObject tile_obj = tile.gameObject;
        //Transform[] wall_obj = new Transform[3];

        foreach (Transform child in tile_obj.transform)
        {
            if (child.name.Contains("Wall_"))
            {

            }
        }
    }
}

public class MazeBlueprint
{
    public int m_length = 10;
    public int m_width = 10;
    public bool[,] wall_v;	// X, Z
	public bool[,] wall_h;	// X, Z
	public int[,] cell;		// X, Z
    
    public MazeBlueprint(int width, int length)
    {
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

        GenerateBPLayout();
    }

    // Generate BP maze using kruskal's algorithm
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