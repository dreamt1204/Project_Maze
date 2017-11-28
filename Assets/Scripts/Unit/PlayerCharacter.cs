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
    Eatting,
    Splitting
}

public class PlayerCharacter : Unit {
    //=======================================
    //      Variables
    //=======================================
	[HideInInspector] private UIManager uiManager;

    [HideInInspector] public Camera playerCamera;
    bool controlDisabled = false;
    [HideInInspector] public bool hasObjective = false;
	[HideInInspector] public bool compassEnabled = false;
    [HideInInspector] public Dictionary<string, PlayerAbility> PlayerAbilities;

    public List<BodyPartData> defaultBodyParts;

    [Header("Slime")]
    public Slime startSlime;
    [Range(1, 5)] public int startLevel;
    Slime slime;
    [HideInInspector] public Slime slimeToSwapped;
    SlimeStateType slimeState_m;
    public Dictionary<SlimeType, PlayerSlimeData> slimeData = new Dictionary<SlimeType, PlayerSlimeData>();
    UIWorldHUDManager slimeExperienceHUD;

    //---------------------------------------
    //      Delegates / Events
    //---------------------------------------
    public delegate void SlimeSwapped(PlayerCharacter player, Slime newSlime);
    public static event SlimeSwapped OnSlimeSwapped;

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
            UpdateCurrentTileSlimeSplit();
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

    public SlimeStateType slimeState
    {
        get
        {
            return slimeState_m;
        }
        set
        {
            slimeState_m = value;
            UpdateCurrentTileSlimeSplit();
        }
    }

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    public override void Init (Tile spawnTile)
	{
        if (LevelManager.instance.playerCharacter == null)
            LevelManager.instance.playerCharacter = this;
        uiManager = UIManager.instance;

        // UI Delefate / Event
        UIManager.OnReplyed += WaitingForReply;

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
        Utilities.TryCatchError((startSlime == null), "Player Character doesn't have start Slime.");
        slimeData.Add(startSlime.slimeType, new PlayerSlimeData(startSlime, startLevel));
        uiManager.UpdateSlimeInventroy(this, startSlime.slimeType);
        skeletonAnim = GetComponentInChildren<Spine.Unity.SkeletonAnimation>();
        InitSlime(startSlime);

        if (transform.Find("SlimeExperienceHUD") != null)
            slimeExperienceHUD = transform.Find("SlimeExperienceHUD").GetComponent<UIWorldHUDManager>();

        base.Init(spawnTile);
    }

	public override void Update()
	{
        if (!finishedInit)
            return;

        base.Update ();

		// Update movement based on UIJoyStick
		if ((!IsPlayerControlDisabled()) && (uiManager.joyStickEnabled))
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
    //      Reply event
    //---------------------------------------
    public void WaitingForReply(string question, bool reply)
    {
        if (question == "SwapSlime")
        {
            if (reply)
                SwapSlime(slimeToSwapped);

            slimeToSwapped = null;
            uiManager.ShowSwitchToNewSlimeWidget(this, false);
        }
        else if (question == "SwapSlimeFromInventroy")
        {
            if (reply)
                SwapSlime(slimeToSwapped);

            slimeToSwapped = null;
            uiManager.ShowSlimeInventoryWidget(false);
        }
    }

    //---------------------------------------
    //      Control
    //---------------------------------------
    public bool IsPlayerControlDisabled()
    {
        return controlDisabled;
    }

    public void DisablePlayerControl()
    {
        controlDisabled = true;
    }

    public void EnablePlayerControl()
    {
        controlDisabled = false;
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
        if (partType != "Head")
            return;

        BodyPart part = GetBodyPartWithType(partType);

        // If the part doesn't exist, clear part ability
        if ((part == null) || (part.playerAbility == null))
        {
            PlayerAbilities[partType + "Ability"] = null;
            UIManager.instance.ClearAbilityIcon(partType + "Ability");
        }
        // Update PlayerAbilities while body part gets updated
        else
        {
            PlayerAbilities[partType + "Ability"] = part.playerAbility;
			part.playerAbility.Init();
            UIManager.instance.UpdateAbilityIcon(partType + "Ability");
        }
    }

    //---------------------------------------
    //      Tile Action
    //---------------------------------------
    public override void UnitTileAction(Tile tile)
	{
        PlayerTileItemAction(tile);

        base.UnitTileAction(tile);
    }

    void PlayerTileItemAction(Tile tile)
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
            if (level.CheckLevelPassedCondition())
                return;

            tile.item.EnableSlimeSwapButton(true);
        }
        else if (tile.item.itemType == ItemType.SlimeInventory)
        {
            tile.item.EnableSlimeSwapButton(true);
        }
        else if (tile.item.itemType == ItemType.BodyPart)
        {
            UpdateBodyPart(tile.item.bodyPart);
            tile.DestroyTileItem();
        }
        else if (tile.item.itemType == ItemType.SlimeElement)
        {
            AddSlimeExperience(tile.item.slime);
            tile.DestroyTileItem();
        }
        else if (tile.item.itemType == ItemType.HealthPack)
        {
            RestoreHealth(tile.item.healAmount);
            tile.DestroyTileItem();
        }
        else if (tile.item.itemType == ItemType.Compass)
        {
            uiManager.ActivateCompass(tile.item.compassDuration);
            tile.DestroyTileItem();
        }
    }

    public override void UnitTileLeftAction(Tile tile)
    {
        if (tile == null)
            return;

        if (tile.item == null)
            return;

        if ((tile.item.itemType == ItemType.StartPoint) || (tile.item.itemType == ItemType.SlimeInventory))
        {
            tile.item.EnableSlimeSwapButton(false);
        }
    }

    //---------------------------------------
    //      Slime
    //---------------------------------------
    public void AddSlimeExperience(Slime addingSlime)
    {
        if (!slimeData.ContainsKey(addingSlime.slimeType))
            slimeData.Add(addingSlime.slimeType, new PlayerSlimeData(addingSlime));

        PlayerSlimeData data = slimeData[addingSlime.slimeType];
        data.slimeExp += 1;

        uiManager.UpdateSlimeInventroy(this, addingSlime.slimeType);

        slimeExperienceHUD.UpdateLabel(GetSlimeAddingExperienceLabel(data));
        slimeExperienceHUD.PlayTweeners(0);

        if ((data.GetExperienceLeft() == 0) && (data.slimeLevel >= 1))
        {
            SlimeLevelUp(addingSlime);

            if ((data.slimeLevel == 1) && (slime != addingSlime))
            {
                slimeToSwapped = addingSlime;
                uiManager.ShowSwitchToNewSlimeWidget(this, true);
            }
        }
    }

    string GetSlimeAddingExperienceLabel(PlayerSlimeData data)
    {
        string label;

        if (data.slimeLevel >= data.slime.levelData.Length)
        {
            label = "Max Level";
        }
        else
        {
            float digit_1 = data.GetExperienceLeft();
            float digit_2 = data.slime.levelData[data.slimeLevel].requiredExp;

            if (digit_1 == 0)
            {
                if (data.slimeLevel == 1)
                    label = "Slime Unlocked";
                else
                    label = "Lv" + (data.slimeLevel - 1) + " -> Lv" + data.slimeLevel;
            }
            else
            {
                label = "(" + digit_1 + "/" + digit_2 + ")";
            }
        }

        return label;
    }

    public Slime GetSlime()
    {
        return slime;
    }

    public void InitSlime(Slime newSlime)
    {
        slime = newSlime;
        uiManager.slimeButtons.Init();
        OnSlimeSwapped(this, newSlime);
        UpdatSlimeAbilities(newSlime);
    }

    public void SwapSlime(Slime newSlime)
    {
        slime = newSlime;
        OnSlimeSwapped(this, newSlime);
        UpdateBody();
        UpdatSlimeAbilities(newSlime);
    }

    public void SlimeLevelUp(Slime levelingSlime)
    {
        PlayerSlimeData data = slimeData[levelingSlime.slimeType];
        SlimeLevelData lvData = data.slime.levelData[data.slimeLevel - 1];

        if (lvData.unlockAbility1)
        {
            data.ability_1_unlocked = true;
            UpdatSlimeAbilities(levelingSlime);
        }
        if (lvData.unlockAbility2)
        {
            data.ability_2_unlocked = true;
            UpdatSlimeAbilities(levelingSlime);
        }
    }

    public void UpdateCurrentTileSlimeSplit()
    {
        if (slimeState == SlimeStateType.Splitting)
        {
            if (CurrentTile.slimeSplit != null)
                return;

            if ((Health - slime.slimeSplit.splittingSlimeDamage) <= 0)
                return;

            GenerateSlimeSplit();
        }
        else if (slimeState == SlimeStateType.Eatting)
        {
            if (CurrentTile.slimeSplit == null)
                return;

            EatSlimeSplit();
        }
    }

    public void GenerateSlimeSplit()
    {
        RecieveDamage(slime.slimeSplit.splittingSlimeDamage);

        CurrentTile.SpawnTileSlimeSplit(slime.slimeSplit.gameObject);
    }

    public void EatSlimeSplit()
    {
        RestoreHealth(CurrentTile.slimeSplit.eattingSlimeRecover);

        CurrentTile.DestroyTileSlimeSplit();
    }

    public void UpdatSlimeAbilities(Slime updatedSlime)
    {
        for (int i = 1; i <= 2; i++)
        {
            bool abilityUnlocked = (i == 1) ? (slimeData[updatedSlime.slimeType].ability_1_unlocked) : (i == 2) ? (slimeData[updatedSlime.slimeType].ability_2_unlocked) : false;

            PlayerAbility ability = (i == 1) ? slime.Ability1 : (i == 2) ? slime.Ability2 : null;
            string abilityButtonType = "SlimeAbility_" + i;

            if ((!abilityUnlocked) || (ability == null))
            {
                PlayerAbilities[abilityButtonType] = null;
                UIManager.instance.ClearAbilityIcon(abilityButtonType);
            }
            else
            {
                PlayerAbilities[abilityButtonType] = ability;
                PlayerAbilities[abilityButtonType].Init();
                UIManager.instance.UpdateAbilityIcon(abilityButtonType);
            }
        }
    }
}
