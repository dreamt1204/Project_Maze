using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enum Direction { North, East, South, West };
//enum WallState { Empty, Wall, Edge, Broken };

public class Tile : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	private GameManager GM;
	public int X;
    public int Z;
    public bool[] wall = new bool[4];
    public GameObject[] wall_obj = new GameObject[4];

    //=======================================
    //      Functions
    //=======================================
	void Start()
	{
		GM = GameObject.Find ("GameManager").GetComponent<GameManager>();

		BoxCollider tile_collider = this.gameObject.AddComponent<BoxCollider> ();
		tile_collider.size = new Vector3 (10, 2, 10);
	}

	void OnMouseDown()
	{
		GM.playerCharacter.TryMoveToNeighborTile (this);
	}
}
