using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    public Tile[,] tile;

    public Maze(int width, int length)
    {
        tile = new Tile[width, length];
    }

    public Maze(Tile[,] tile_list)
    {
        tile = tile_list;
    }

    //=======================================
    //      Functions
    //=======================================
}
