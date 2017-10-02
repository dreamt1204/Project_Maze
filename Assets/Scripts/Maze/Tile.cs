using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState
{
	None,			// No state and can't be selected
	Walkable,		// Unit can walk to this tile
	Selectable		// Can be selected while using abilities
};

public class Tile : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	private GameManager gameManager;

	public int X;
    public int Z;
    public bool[] wall = new bool[4];
    public GameObject[] wall_obj = new GameObject[4];
	private TileState state = TileState.None;

	//---------- Properties -----------
	public TileState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
		}
	}

    //=======================================
    //      Functions
    //=======================================
	void Start()
	{
		gameManager = GameObject.Find ("GameManager").GetComponent<GameManager>();

		BoxCollider tile_collider = this.gameObject.AddComponent<BoxCollider> ();
		tile_collider.size = new Vector3 (10, 2, 10);
	}

	void OnMouseDown()
	{
		if (state == TileState.None)
			return;

		if (state == TileState.Walkable)
			gameManager.playerCharacter.TryMoveToTile (this);
	}
}
