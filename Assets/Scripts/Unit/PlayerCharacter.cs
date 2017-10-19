using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Unit {
	//=======================================
	//      Variables
	//=======================================
	private Camera playerCamera;
	[HideInInspector]
	public bool hasObjective = false;
    [HideInInspector]
    public Dictionary<string, PlayerAbility> PlayerAbilities;

	int stepNoiseLevel = 2;
	protected Tile tileWalkingTowards = null;

    //---------------------------------------
    //      Properties
    //---------------------------------------
    public override Tile CurrentTile
	{
        get
		{
			return currentTile;
		}
		set
		{
			currentTile = value;
            currentTile.CheckTileAction();
            currentTile.CheckPlayerTileAction();
            Maze.UpdateWalkableTiles(currentTile);
        }
	}

	public virtual Tile TileWalkingTowards {get; set;}

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    public override void Init (LevelManager gm, Tile spawnTile)
	{
		base.Init(gm, spawnTile);
        playerCamera = GetComponentInChildren<Camera>();
    }

	// Update function
	public override void Update()
	{
        base.Update();

        HoldingMove ();
	}

    //---------------------------------------
    //      Movement
    //---------------------------------------
    // This feature lets player move to the next time while player is holding the mouse during movement.
    // The direction is calculate based on the mouse and character origin.
    public void HoldingMove()
	{
        // Check if player is holding the mouse during movement. If true, set KeepWalking flag.
        if (!KeepWalking)
		{
            if (isWalking && Input.GetMouseButton(0))
                KeepWalking = true;
        }
        // If KeepWalking is set...
        else
        {
            // Unset KeepWalking while player release the mouse.
            if (Input.GetMouseButtonUp (0))
			{
                KeepWalking = false;
            }
			// If player finished his movement and is ready for the next move, move the character to the walkable tile with holding direction.
			else if (ArrivedNextTile)
			{
				Tile nextTile = Maze.GetDirNeighborTile (currentTile, GetHoldingMoveDir());
                if ((nextTile != null) && (nextTile.State == TileState.Walkable))
                    TryMoveToTile(nextTile);
                else
                    isWalking = false;
            }
		}
    }

	// Get the direction calculated based on the mouse and character origin.
	public int GetHoldingMoveDir()
	{
		int dir = 0;

		Vector3 charScreenPos = playerCamera.WorldToScreenPoint (transform.position);
		charScreenPos = new Vector3 (charScreenPos.x, charScreenPos.y, 0);

		float length_x = (Input.mousePosition.x - charScreenPos.x);
		float length_y = (Input.mousePosition.y - charScreenPos.y);

		if (Mathf.Abs (length_x) >= Mathf.Abs (length_y))
		{
			if (length_x >= 0)
				dir = 1;
			else
				dir = 3;
		}
		else
		{
			if (length_y >= 0)
				dir = 0;
			else
				dir = 2;
		}

		return dir;
	}

    //---------------------------------------
    //      Body Part
    //---------------------------------------
    public override void InitBodyParts()
    {
        // Init PlayerCharacter BodyParts
        InitBodyPartData("Head");
        InitBodyPartData("Arms");
        InitBodyPartData("Body");
        InitBodyPartData("Legs");
        InitBodyPartData("Misc");

        // Init PlayerAbilities
        PlayerAbilities = new Dictionary<string, PlayerAbility>();
        foreach (BodyPartData data in BodyParts)
        {
            PlayerAbilities.Add(data.partType, null);
        }
    }

    public override void BodyPartUpdatedEvent(string partType)
    {
        BodyPart part = GetBodyPartWithType(partType);

        // Update PlayerAbilities while body part gets updated
        PlayerAbilities[partType] = part != null ? GetBodyPartWithType(partType).playerAbility : null;
    }

	// ================
	//     JY Added 
	// ================

	public override void MoveToTile(Tile targetTile)
	{
		base.MoveToTile (targetTile);
		levelManager.eventManager.makeNoise (currentTile, stepNoiseLevel);
		TileWalkingTowards = targetTile;
		//Debug.Log ("(1) TileWalkingTowards X = " + TileWalkingTowards.X + "; Z = " + TileWalkingTowards.Z);

		// Important!!! current not removing TileWalkingTowards after movement completes
	}

}
