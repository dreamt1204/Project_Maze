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
	private LevelManager levelManager;

	public int X;
    public int Z;
    public bool[] wall = new bool[4];
    public GameObject[] wall_obj = new GameObject[4];
    public WallLayout wallLayout;
	private TileState state = TileState.None;
	public TileItem item;

    //---------------------------------------
    //      Properties
    //---------------------------------------
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
	void Awake()
	{
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager>();

		BoxCollider tile_collider = this.gameObject.AddComponent<BoxCollider> ();
		tile_collider.size = new Vector3 (10, 2, 10);
	}

	void OnMouseDown()
	{
		if (state == TileState.None)
			return;

		if (state == TileState.Walkable)
			levelManager.playerCharacter.TryMoveToTile (this);
	}

	public TileItem SpawnTileItem(GameObject itemPrefab)
	{
		item = Instantiate (itemPrefab, transform.position, Quaternion.Euler (0, 0, 0)).GetComponent<TileItem>();

        return item;
	}

    public void DestroyTileItem()
    {
        if (item == null)
            return;

        Destroy(item.gameObject);
        item = null;
    }

    // Apply tile action when stepped on
	public void CheckTileAction()
	{
        
    }

    // Player specific tile action
    public void CheckPlayerTileAction()
    {
        if (item == null)
            return;

        if (item.type == ItemType.Objective)
        {
            levelManager.playerCharacter.hasObjective = true;
            DestroyTileItem();
        }
        else if (item.type == ItemType.StartPortal)
        {
            levelManager.CheckWinningCondition();
        }
    }
}
