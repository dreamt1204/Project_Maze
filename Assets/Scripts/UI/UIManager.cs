﻿//============================== Class Definition ==============================
// 
// This is a helper class that control all the UI logic.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityButtonState
{
	Enabled,
	Disable,
	CoolDown
}

public class UIManager : MonoBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    public static UIManager instance = null;

    private Camera cam;
    private UIWidget joyStickArea;
	[HideInInspector] public  UIJoyStick joyStick;
	[HideInInspector] public bool joyStickEnabled;
	[HideInInspector] public Dictionary<string, abilityButton> abilityButtons;

	private UIProgressBar healthBar;

    [HideInInspector] public UIWidget slimeWidget;
    [HideInInspector] public UISprite slimeDragButtonSprite;
    [HideInInspector] public UIWidget slimeStateSelectWidget;


    //=======================================
    //      Struct
    //=======================================
    public struct abilityButton
    {
		public AbilityButtonState iconState;

		public GameObject buttonObj;
        public UISprite mainSprite;
        public UISprite iconSprite;
    }

    //=======================================
    //      Functions
    //=======================================
    void Awake()
    {
        // This enforces our singleton pattern, meaning there can only ever be one instance of a LevelManager.
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        cam = GameObject.Find("UICamera").GetComponent<Camera>();
        joyStick = GameObject.Find("JoyStick").GetComponent<UIJoyStick>();
        joyStickArea = GameObject.Find("Widget_JoyStickArea").GetComponent<UIWidget>();

		healthBar = GameObject.Find("HealthBar").GetComponent<UIProgressBar>();

        slimeWidget = GameObject.Find("Widget_Slime").GetComponent<UIWidget>();
        slimeDragButtonSprite = GameObject.Find("Sprite_Slime").GetComponent<UISprite>();
        slimeStateSelectWidget = GameObject.Find("Widget_SlimeStateSelect").GetComponent<UIWidget>();
        slimeStateSelectWidget.alpha = 0;
        
        abilityButtons = new Dictionary<string, abilityButton>();
        InitNewButton("Head");
        InitNewButton("Body");
        InitNewButton("Legs");
    }

	void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
			joyStickEnabled = true;
			TryUpdateJoyStickPos();
        }
        else if (Input.GetMouseButtonUp(0))
        {
			joyStickEnabled = false;
        }
    }

    //---------------------------------------
    //      JoyStick
    //---------------------------------------
    void TryUpdateJoyStickPos()
    {
        // Check if player clicks/taps in the joystick area
        Vector3 clickPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3[] corners = joyStickArea.worldCorners;
        Rect newRect = new Rect(corners[0].x, corners[0].y, (corners[3].x - corners[0].x), (corners[1].y - corners[0].y));
        if (!newRect.Contains(clickPos))
            return;

        // Enable and update joystick position, then process mouse to click/taps on it
        joyStick.transform.parent.transform.position = clickPos;
        joyStick.EnableJoyStick(true);
        cam.GetComponent<UICamera>().ProcessMouse();
    }

    //---------------------------------------
    //      Ability Buttons
    //---------------------------------------
    void InitNewButton(string partType)
    {
        abilityButton newButton = new abilityButton();
        newButton.buttonObj = GameObject.Find("Sprite_ability_" + partType);
        newButton.mainSprite = newButton.buttonObj.GetComponent<UISprite>();
        newButton.iconSprite = newButton.buttonObj.transform.Find("Sprite_ability_button").GetComponent<UISprite>();

        abilityButtons.Add(partType, newButton);
    }

	void UpdateabilityButtonsIconSate(abilityButton button, AbilityButtonState newState)
	{
		abilityButton newButton = new abilityButton();
		newButton = button;
		newButton.iconState = newState;
		button = newButton;
	}

    public void UpdateAbilityIcon(string partType)
    {
        abilityButtons[partType].buttonObj.SetActive(true);
        UpdateabilityButtonsIconSate (abilityButtons [partType], AbilityButtonState.Enabled);

		string newSprite = LevelManager.instance.playerCharacter.PlayerAbilities[partType].spriteName;
        if ((newSprite != null) && (AtlasHasSprite(abilityButtons[partType].iconSprite.atlas, newSprite)))
        {
			abilityButtons[partType].iconSprite.spriteName = LevelManager.instance.playerCharacter.PlayerAbilities[partType].spriteName;
        }
        else
        {
            abilityButtons[partType].iconSprite.spriteName = "Icon_" + partType + "_default";
        }  
    }

    public void ClearAbilityIcon(string partType)
    {
		UpdateabilityButtonsIconSate (abilityButtons [partType], AbilityButtonState.Disable);
		abilityButtons[partType].buttonObj.SetActive(false);
    }

    bool AtlasHasSprite(UIAtlas atlas, string newSprite)
    {
        bool hasSprite = false;

        foreach (UISpriteData data in atlas.spriteList)
        {
            if (data.name == newSprite)
            {
                hasSprite = true;
                break;
            }
        }

        return hasSprite;
    }

	//---------------------------------------
	//      Slime Health
	//---------------------------------------
	public void UpdateHealthBar(float value)
	{
		healthBar.value = value;
	}

    //---------------------------------------
    //      Slime Drag Button
    //---------------------------------------
    public void UpdateSlimeDragButtonSprite(SlimeStateType state)
    {
        if (state == SlimeStateType.None)
            slimeDragButtonSprite.spriteName = "Slime";
        else if (state == SlimeStateType.Splitting)
            slimeDragButtonSprite.spriteName = "Slime_Split";
        else if(state == SlimeStateType.Eatting)
            slimeDragButtonSprite.spriteName = "Slime_Eat";
    }
}
