using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A maze blueprint class with constructor.
// Once taking maze size as input, it generates wall arrays using Kruskal's algorithm.
// The wall array can be used to build maze tile later.
public class MazeBlueprint
{
    public int m_length = 10;
    public int m_width = 10;
    public bool[,] wall_v;  // X, Z
    public bool[,] wall_h;  // X, Z
    public int[,] cell;     // X, Z  cell is the id of each inclosed section in Kruskal's algorithm.
    public List<Vector2> tileInSquares;

    public MazeBlueprint(int width, int length)
    {
        // Setup maze blueprint with input size
        m_width = width;
        m_length = length;
        wall_v = new bool[m_width + 1, m_length];
        wall_h = new bool[m_width, m_length + 1];
        cell = new int[m_width, m_length];
        tileInSquares = new List<Vector2>();
        
        for (int i = 0; i < (m_width + 1); i++)
        {
            for (int j = 0; j < m_length; j++)
            {
                wall_v[i, j] = true;
            }
        }
        for (int i = 0; i <m_width; i++)
        {
            for (int j = 0; j < m_length + 1; j++)
            {
                wall_h[i, j] = true;
            }
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
        if (m_width == 1)
        {
            for (int i = 1; i < m_length; i++)
            {
                wall_h[0, i] = false;
            }
        }
        else if (m_length == 1)
        {
            for (int i = 1; i < m_width; i++)
            {
                wall_v[i, 0] = false;
            }
        }
        else
        {
            while (true)
            {
                int[] repl;

                if (Random.Range(0, 2) >= 1)
                {
                    int x = Random.Range(1, m_width);
                    int z = Random.Range(0, m_length);
                    if (cell[x, z] == cell[x - 1, z])
                        continue;
                    repl = new int[] { cell[x, z], cell[x - 1, z] };
                    wall_v[x, z] = false;
                }
                else
                {
                    int x = Random.Range(0, m_width);
                    int z = Random.Range(1, m_length);
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

        //AddSquares();
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

    void AddSquares()
    {
        int squares2x2 = Formula.Calculate2x2SquareNum(LevelManager.instance.mazeDifficulty);
        int squares3x3 = Formula.Calculate3x3SquareNum(LevelManager.instance.mazeDifficulty);

        for (int i = 0; i < squares2x2; i++)
        {
            AddSquare(2);
        }

        for (int i = 0; i < squares3x3; i++)
        {
            AddSquare(3);
        }
    }

    void AddSquare(int size)
    {
        int x, z;

        do
        {
            x = Random.Range(0, (m_width - size));
            z = Random.Range(0, (m_length - size));

        } while ((tileInSquares.Contains(new Vector2(x, z))) || (tileInSquares.Contains(new Vector2(x + size - 1, z + size - 1))));

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if ((i + 1) < size)
                    wall_v[x + i + 1, z + j] = false;
                if ((j + 1) < size)
                    wall_h[x + i, z + j + 1] = false;
            }
        }

        for (int i = (x - 1); i <= (x + size); i++)
        {
            for (int j = (z - 1); j <= (z + size); j++)
            {
                tileInSquares.Add(new Vector2(i, j));
            }
        }
    }
}
