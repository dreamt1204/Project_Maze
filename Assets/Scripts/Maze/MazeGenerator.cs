using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour {

    public int width = 10;
    public int length = 10;
    public MazeSetting MazeSetting;

    [HideInInspector]
    public Tile[,] Maze;

    // (1)-N, (2)-E, (3)-S, (4)-W
    // (5)-NS, (6)-EW
    // (7)-NE, (8)-ES, (9)-SW, (10)-WN
    // (11)-NES, (12)-ESW, (13)-SWN, (14)-WNE
    private int tile_geo_type;

    // Use this for initialization
    void Start ()
    {
        MazeBlueprint MazeBP = new MazeBlueprint(length, width);
        BuildMaze(MazeBP);
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void BuildMaze(MazeBlueprint MazeBP)
    {
        Maze = new Tile[length, width];

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Maze[i,j].Y = i;
                Maze[i,j].X = j;
            }
        }
    }

    int GetTileGeoType(int Y, int X, MazeBlueprint MazeBP)
    {
        int type = 0;
		// tests



        return type;
    }
}

public class MazeBlueprint
{
    public int m_length = 10;
    public int m_width = 10;
    public bool[,] wall_v;
    public bool[,] wall_h;
    public int[,] cell;
    
    public MazeBlueprint(int length, int width)
    {
        m_length = length;
        m_width = width;
        wall_v = new bool[m_length, m_width + 1];
        wall_h = new bool[m_length + 1, m_width];
        cell = new int[m_length, m_width];
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
        for (int i = 0; i < m_length; i++)
        {
            for (int j = 0; j < m_width; j++)
            {
                cell[i, j] = m_width * i + j;
            }
        }

        GenerateBPLayout();
    }

    // Generate BP maze using kruskal's algorithm
    public void GenerateBPLayout()
    {
        while (true)
        {
            int[] repl;

            if (Random.Range(0, 2) >= 1)
            {
                int y = Random.Range(0, wall_v.GetLength(0));
                int x = Random.Range(1, wall_v.GetLength(1) - 1);
                if (cell[y, x] == cell[y, x - 1])
                    continue;
                repl = new int[] { cell[y, x], cell[y, x - 1] };
                wall_v[y, x] = false;
            }
            else
            {
                int y = Random.Range(1, wall_h.GetLength(0) - 1);
                int x = Random.Range(0, wall_h.GetLength(1));
                if (cell[y, x] == cell[y - 1, x])
                    continue;
                repl = new int[] { cell[y, x], cell[y - 1, x] };
                wall_h[y, x] = false;
            }
            System.Array.Sort(repl);
            ReplaceIDs(repl);
            if (IDsAllZero(repl) == true)
                break;
        }
    }

    void ReplaceIDs(int[] repl)
    {
        for (int d = 0; d < m_length; d++)
        {
            for (int w = 0; w < m_width; w++)
            {
                if (cell[d, w] == repl[1])
                    cell[d, w] = repl[0];
            }
        }
    }

    bool IDsAllZero(int[] repl)
    {
        if (repl[0] != 0 && repl[0] != 0)
            return false;
        for (int k = 0; k < m_length; k++)
        {
            for (int j = 0; j < m_width; j++)
            {
                if (cell[k, j] != 0)
                    return false;
            }
        }
        return true;
    }
}