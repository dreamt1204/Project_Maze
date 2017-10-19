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
	[HideInInspector] private UIManager uiManager;

    [HideInInspector] public Camera playerCamera;
	[HideInInspector] public bool hasObjective = false;
    [HideInInspector] public Dictionary<string, PlayerAbility> PlayerAbilities;

    //---------------------------------------
    //      Properties
    //---------------------------------------
	public override float Health
	{
		get
		{
			return base.Health;
		}
		set
		{
			base.Health = value;
			level.CheckGameModeCondition ();
		}
	}

	public override ActionType CurrentAction
	{
		get
		{
			return base.CurrentAction;
		}
		set
		{
			base.CurrentAction = value;
		}
	}

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    public override void Init (Tile spawnTile)
	{
		uiManager = LevelManager.instance.uiManager;

        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        Vector3 camPos = playerCamera.gameObject.transform.position;
        playerCamera.gameObject.transform.position = new Vector3(camPos.x + transform.position.x, camPos.y + transform.position.y, camPos.z + transform.position.z);
        playerCamera.gameObject.transform.parent = transform;

		base.Init(spawnTile);
    }

	public override void Update()
	{
		if (!finishedInit)
			return;

		base.Update ();

		// Update movement based on UIJoyStick
		if (uiManager.joyStickEnabled)
		{
			if (uiManager.joyStick.joyStickDir != -1)
			{
				TryMoveToDirTile(uiManager.joyStick.joyStickDir);
			}  
		}
		else
		{
			StopKeepWalkingAnim ();
		} 
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

	//---------------------------------------
	//      Tile Action
	//---------------------------------------
	public override void UnitTileAction(Tile tile)
	{
		if (tile.item == null)
			return;

		if (tile.item.itemType == ItemType.Objective)
		{
			level.playerCharacter.hasObjective = true;
			tile.DestroyTileItem();
		}
		else if (tile.item.itemType == ItemType.StartPoint)
		{
			level.CheckGameModeCondition();
		}
		else if (tile.item.itemType == ItemType.BodyPart)
		{
			level.playerCharacter.UpdateBodyPart(tile.item.bodyPart);
			tile.DestroyTileItem();
		}
	}
}
