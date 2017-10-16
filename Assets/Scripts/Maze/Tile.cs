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

    // Floor
    public GameObject floor_obj;
	private Color originalFloorColor;

    // State
    TileState state = TileState.None;
	TileState statePrevious = TileState.None;

	// Selectable highlight variables
	float pulseTime = 0;
	const float pulseFrequency = 1;
	bool pulseDir;
	Color pulseStartColor  = new Color(1f, 1f, 0.1f);
	Color pulseEndColor  = new Color(1.5f, 1.5f, 0.2f);

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
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager>();

        // Init box collider for this tile
		BoxCollider tile_collider = this.gameObject.AddComponent<BoxCollider> ();
		tile_collider.size = new Vector3 (10, 2, 10);
	}

	void Update()
	{
		TryApplyHighlightPulse();
	}

	// Highlight the tile if its state is Selectable.
	void TryApplyHighlightPulse()
	{
		if (State == TileState.Selectable)
		{
			Material material = floor_obj.GetComponent<MeshRenderer> ().material;
			material.color = pulseStartColor;

			pulseTime += Time.deltaTime/pulseFrequency;
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
		floor_obj.GetComponent<MeshRenderer> ().material.color = Color.white;
		ResetHighlightPulseVars ();
	}

	//---------------------------------------
	//      Tile select detection
	//---------------------------------------
    void OnClick()
    {
        if (state == TileState.None)
            return;

        if (state == TileState.Walkable)
        {
            if (!levelManager.playerCharacter.isWalking)
                levelManager.playerCharacter.TryMoveToTile(this);
        }  
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

	public void ResetTileState()
	{
		if (statePrevious != TileState.None)
			State = statePrevious;
		else
			State = TileState.None;

		statePrevious = TileState.None;
	}

	public void HighlightSelectableTile(bool highlight)
	{
		Material material = floor_obj.GetComponent<MeshRenderer> ().material;

		if (highlight)
		{
			material.color = Color.green;
		}
		else
		{
			material.color = Color.white;
		}
	}
}
