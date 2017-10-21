﻿//============================== Class Definition ==============================
// 
// This class contains all the player character specific properties and functions.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlimeStateType
{
    None,
    Splitting,
    Eatting
}

public class PlayerCharacter : Unit {
    //=======================================
    //      Variables
    //=======================================
	[HideInInspector] private UIManager uiManager;

    [HideInInspector] public Camera playerCamera;
	[HideInInspector] public bool hasObjective = false;
    [HideInInspector] public Dictionary<string, PlayerAbility> PlayerAbilities;

    public GameObject SlimeSplitPrefab;
    [HideInInspector] private SlimeStateType slimeState;
    [HideInInspector] private float splittingSlimeDamage = 1;
	[HideInInspector] private float eattingSlimeRecover = 1;

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
			uiManager.UpdateHealthBar (Health/healthMax);
			level.CheckLevelFailedCondition ();
		}
	}

    public override Tile CurrentTile
    {
        get
        {
            return base.CurrentTile;
        }
        set
        {
            base.CurrentTile = value;
            TryUpdateSlimeSplit();
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

    public SlimeStateType SlimeState
    {
        get
        {
            return slimeState;
        }
        set
        {
            slimeState = value;
            TryUpdateSlimeSplit();
        }
    }

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    public override void Init (Tile spawnTile)
	{
		uiManager = UIManager.instance;

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
        /* (WIP FIX)
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
        */
    }

    public override void BodyPartUpdatedEvent(string partType)
    {
        /* (WIP FIX)
        BodyPart part = GetBodyPartWithType(partType);

        // If the part doesn't exist, clear part ability
        if ((part == null) || (part.playerAbility == null))
        {
            PlayerAbilities[partType] = null;
            UIManager.instance.ClearAbilityIcon(partType);
        }
        // Update PlayerAbilities while body part gets updated
        else
        {
            PlayerAbilities[partType] = part.playerAbility;
			part.playerAbility.Init ();
            UIManager.instance.UpdateAbilityIcon(partType);
        }
        */
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
			level.CheckLevelPassedCondition();
		}
		else if (tile.item.itemType == ItemType.BodyPart)
		{
			level.playerCharacter.UpdateBodyPart(tile.item.bodyPart);
			tile.DestroyTileItem();
		}
	}

    //---------------------------------------
    //      Slime Split/Eat Ability
    //---------------------------------------
    public void TryUpdateSlimeSplit()
    {
        if (SlimeState == SlimeStateType.None)
            return;

        if (SlimeState == SlimeStateType.Splitting)
        {
            if (CurrentTile.SlimeSplit != null)
                return;

            if ((Health - splittingSlimeDamage) <= 0)
                return;

            GenerateSlimeSplit();
        }
		else if (SlimeState == SlimeStateType.Eatting)
		{
			if (CurrentTile.SlimeSplit == null)
				return;

			EatSlimeSplit();
		}

    }

    public void GenerateSlimeSplit()
    {
        RecieveDamage(splittingSlimeDamage);

        GameObject newSplit = Instantiate(SlimeSplitPrefab, CurrentTile.transform.position, Quaternion.Euler(0, 0, 0));
        CurrentTile.SlimeSplit = newSplit;
    }

	public void EatSlimeSplit()
	{
		RestoreHealth(eattingSlimeRecover);

		Destroy(CurrentTile.SlimeSplit);
	}
}
