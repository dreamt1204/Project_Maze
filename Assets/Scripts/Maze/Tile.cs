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
	LevelManager levelManager;

    // Coordinate info
	public int X;
    public int Z;

    // Walls
    public bool[] wall = new bool[4];
    public GameObject[] wall_obj = new GameObject[4];
    public WallLayout wallLayout;

    // State
	TileState state = TileState.None;

    // Item
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

        // Init box collider for this tile
		BoxCollider tile_collider = this.gameObject.AddComponent<BoxCollider> ();
		tile_collider.size = new Vector3 (10, 2, 10);
	}

	//---------------------------------------
	//      Tile select detection
	//---------------------------------------
	void OnMouseDown()
	{
		if (state == TileState.None)
			return;

		if (state == TileState.Walkable)
			levelManager.playerCharacter.TryMoveToTile (this);
	}

	//---------------------------------------
	//      Tile item
	//---------------------------------------
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

        if (item.itemType == ItemType.Objective)
        {
            levelManager.playerCharacter.hasObjective = true;
            DestroyTileItem();
        }
        else if (item.itemType == ItemType.StartPoint)
        {
            levelManager.CheckWinningCondition();
        }
        else if (item.itemType == ItemType.BodyPart)
        {
            levelManager.playerCharacter.UpdateBodyPart(item.bodyPart);
            DestroyTileItem();
        }
    }
}
