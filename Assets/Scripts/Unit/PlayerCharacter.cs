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
	[HideInInspector] public bool compassEnabled = false;
    [HideInInspector] public Dictionary<string, PlayerAbility> PlayerAbilities;

    public List<BodyPartData> defaultBodyParts;

	[HideInInspector] public Slime slime;
    public SlimeData slimeData;

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
            slime.TryUpdateSlimeSplit();
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
		uiManager = UIManager.instance;

        // Init Camera
        playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        Vector3 camPos = playerCamera.transform.position;
        playerCamera.transform.position = new Vector3(camPos.x + transform.position.x, camPos.y + transform.position.y, camPos.z + transform.position.z);
        playerCamera.transform.parent = transform;

        // Init PlayerCharacter BodyParts
        InitBodyPartData("Head");
        InitBodyPartData("Body");
        InitBodyPartData("Legs");

        // Init PlayerAbilities
        PlayerAbilities = new Dictionary<string, PlayerAbility>();
        foreach (BodyPartData data in BodyParts)
        {
            PlayerAbilities.Add(data.partType, null);
        }

        // Init Slime
		slime = gameObject.AddComponent<Slime>();
		GameObject.Find("SlimeButton").GetComponent<UISlimeButton>().slime = slime;
		Utilities.TryCatchError((slimeData == null), "Player Character doesn't have start Slime Data.");
		slime.UpdateSlimeData(this, slimeData);

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
    public override void AssignMustHaveBodyParts()
    {
        foreach (BodyPartData data in defaultBodyParts)
        {
            if (data.mustHave)
                AssignBodyPart(data.part);
        }
    }

    public override void BodyPartUpdatedEvent(string partType)
    {
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
        else if (tile.item.itemType == ItemType.HealthPack)
        {
            RestoreHealth(tile.item.healAmount);
            tile.DestroyTileItem();
        }
        else if (tile.item.itemType == ItemType.BodyPart)
		{
			UpdateBodyPart(tile.item.bodyPart);
			tile.DestroyTileItem();
		}
		else if (tile.item.itemType == ItemType.Compass)
		{
			uiManager.ActivateCompass(tile.item.compassDuration);
			tile.DestroyTileItem();
		}
	}
}
