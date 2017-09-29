using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enum Direction { North, East, South, West };
//enum WallState { Empty, Wall, Edge, Broken };

public class Tile : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    public int X;
    public int Z;
    public bool[] wall = new bool[4];
    public GameObject[] wall_obj = new GameObject[4];

    //=======================================
    //      Functions
    //=======================================   
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
