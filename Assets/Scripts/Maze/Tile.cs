//============================== Class Definition ==============================
// 
// The maze is formed with this class.
// Each tile has its own properties and action event.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState
{
	None,			// No state and can't be selected
	Selectable		// Can be selected while using abilities
};

public class Tile : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	LevelManager level;

    // Coordinate info
	public int X;
    public int Z;

    // Geo object
    public bool[] wall = new bool[4];
    public GameObject[] wall_obj = new GameObject[4];
    public WallLayout wallLayout;
    public GameObject floor_obj;

    // State
    TileState state = TileState.None;
	TileState statePrevious = TileState.None;

    // Item
    public TileItem item;

    // Selectable highlight variables
    float pulseTime = 0;
	const float pulsePeriod = 0.5f;
	bool pulseDir;
    Color originalFloorColor = Color.white;
    Color pulseStartColor  = new Color(1f, 1f, 0.1f);
	Color pulseEndColor  = new Color(1.5f, 1.5f, 0.2f);

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
			if (state != value)
			{
				statePrevious = state;

				if ((value != TileState.Selectable) && (statePrevious == TileState.Selectable))
					ResetTileColor ();

				state = value;
			}
		}
	}

    //=======================================
    //      Functions
    //=======================================
    void Awake()
	{
        level = LevelManager.instance;

        // Init box collider for this tile
        BoxCollider tile_collider = this.gameObject.AddComponent<BoxCollider> ();
		tile_collider.size = new Vector3 (10, 2, 10);
	}

	void Update()
	{
		TryApplyHighlightPulse();
	}

    //---------------------------------------
    //      Tile highlight
    //---------------------------------------
    public void ResetTileState()
    {
        if (statePrevious != TileState.None)
            State = statePrevious;
        else
            State = TileState.None;

        statePrevious = TileState.None;
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

    //---------------------------------------
    //      Tile action
    //---------------------------------------
    // Apply tile action when stepped on
	// Note: This is only for modifing tile. For unit, check UnitTileAction.
    public void TileAction()
	{
        
    }

    //---------------------------------------
    //      Tile highlight
    //---------------------------------------
    // Highlight the tile if its state is Selectable.
    void TryApplyHighlightPulse()
    {
        if (State == TileState.Selectable)
        {
            Material material = floor_obj.GetComponent<MeshRenderer>().material;
            material.color = pulseStartColor;

            pulseTime += Time.deltaTime / pulsePeriod;
            if (pulseDir)
            {
                material.color = Color.Lerp(pulseStartColor, pulseEndColor, pulseTime);
            }
            else
            {
                material.color = Color.Lerp(pulseEndColor, pulseStartColor, pulseTime);
            }
            if (pulseTime > 1)
            {
                pulseTime = 0;
                pulseDir = !pulseDir;
            }
        }
    }

    void ResetHighlightPulseVars()
    {
        pulseTime = 0;
        pulseDir = false;
    }

    void ResetTileColor()
    {
        floor_obj.GetComponent<MeshRenderer>().material.color = originalFloorColor;
        ResetHighlightPulseVars();
    }
}
