//============================== Class Definition ==============================
// 
// This class contains all the player character specific properties and functions.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Unit {
    //=======================================
    //      Variables
    //=======================================
    [HideInInspector] public Camera playerCamera;
	[HideInInspector] public bool hasObjective = false;
    [HideInInspector] public Dictionary<string, PlayerAbility> PlayerAbilities;

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
        }
	}

	public virtual Tile TileWalkingTowards {get; set;}

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    public override void Init (Tile spawnTile)
	{
		base.Init(spawnTile);

        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        Vector3 camPos = playerCamera.gameObject.transform.position;
        playerCamera.gameObject.transform.position = new Vector3(camPos.x + transform.position.x, camPos.y + transform.position.y, camPos.z + transform.position.z);
        playerCamera.gameObject.transform.parent = transform;
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

        // If the part doesn't exist, clear part ability
        if ((part == null) || (part.playerAbility == null))
        {
            PlayerAbilities[partType] = null;
            LevelManager.instance.uiManager.ClearAbilityIcon(partType);
        }
        // Update PlayerAbilities while body part gets updated
        else
        {
            PlayerAbilities[partType] = part.playerAbility;
			part.playerAbility.Init ();
			LevelManager.instance.uiManager.UpdateAbilityIcon(partType);
        }
    }

	// ================
	//     JY Added 
	// ================

	public override void MoveToTile(Tile targetTile)
	{
		base.MoveToTile (targetTile);
		TileWalkingTowards = targetTile;
		//Debug.Log ("(1) TileWalkingTowards X = " + TileWalkingTowards.X + "; Z = " + TileWalkingTowards.Z);

		// Important!!! current not removing TileWalkingTowards after movement completes
	}

}
