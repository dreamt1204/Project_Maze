//============================== Class Definition ==============================
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

    UILabel timerLabel;

    Camera cam;
    UIWidget joyStickArea;
	[HideInInspector] public UIJoyStick joyStick;
	[HideInInspector] public bool joyStickEnabled;

    [HideInInspector] public UISlimeButton slimeButtons;
    [HideInInspector] public Dictionary<string, abilityButton> abilityButtons;

	UIProgressBar healthBar;

    [HideInInspector] public UIWidget compassWidget;
	[HideInInspector] public UISprite compassSprite;

    UIWidget Widget_SwitchToNewSlime;
    UIWidget Widget_SlimeInventroy;
    UIGrid SlimeInventroyGrid;
    public GameObject slimeScrollItem;

    //---------------------------------------
    //      Delegates / Events
    //---------------------------------------
    public delegate void ReplyAction(string question, bool reply);
    public static event ReplyAction OnReplyed;

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

        timerLabel = GameObject.Find("Label_Timer").GetComponent<UILabel>();

        cam = GameObject.Find("UICamera").GetComponent<Camera>();
        joyStick = GameObject.Find("JoyStick").GetComponent<UIJoyStick>();
        joyStickArea = GameObject.Find("Widget_JoyStickArea").GetComponent<UIWidget>();

        slimeButtons = GameObject.Find("SlimeButton").GetComponent<UISlimeButton>();
    
        healthBar = GameObject.Find("HealthBar").GetComponent<UIProgressBar>();

        Widget_SwitchToNewSlime = GameObject.Find("Widget_SwitchToNewSlime").GetComponent<UIWidget>();
        Widget_SwitchToNewSlime.alpha = 0;
        Widget_SwitchToNewSlime.gameObject.SetActive(false);

        Widget_SlimeInventroy = GameObject.Find("Widget_SlimeInventroy").GetComponent<UIWidget>();
        SlimeInventroyGrid = Widget_SlimeInventroy.transform.GetComponentInChildren<UIGrid>();
        Widget_SlimeInventroy.transform.GetComponentInChildren<UICenterOnChild>().onCenter += OnSelectedSlimeInInventory;
        Widget_SlimeInventroy.alpha = 0;
        Widget_SlimeInventroy.gameObject.SetActive(false);

        compassWidget = GameObject.Find("Widget_Compass").GetComponent<UIWidget>();
		compassWidget.alpha = 0;
		compassSprite = GameObject.Find("Sprite_Compass").GetComponent<UISprite>();
        
        abilityButtons = new Dictionary<string, abilityButton>();
        InitNewButton("Head");
        InitNewButton("Body");
        InitNewButton("Legs");
    }

	void Update ()
    {
		UpdateTimer();
		UpdateJoyStick();
    }

    //---------------------------------------
    //      Timer
    //---------------------------------------
    void UpdateTimer()
	{
		timerLabel.text = GetTimerText(LevelManager.instance.timer);
	}

	public static string GetTimerText(float time)
    {
        int min, sec;
		string secString;

        min = Mathf.FloorToInt(time / 60f);
        sec = Mathf.FloorToInt(time - (min * 60f));
		if (sec < 10)
			secString = "0" + sec;
		else
			secString = "" + sec;

		return min + ":" + secString;
    }

    //---------------------------------------
    //      JoyStick
    //---------------------------------------
	void UpdateJoyStick()
	{
        if ((LevelManager.instance.playerCharacter == null) || (LevelManager.instance.playerCharacter.IsPlayerControlDisabled()))
        {
            joyStick.EnableJoyStick(false);
            return;
        }

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

    void TryUpdateJoyStickPos()
    {
        if (HoveredOnWorldHUD())
            return;
        
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

    bool HoveredOnWorldHUD()
    {
        if (UICamera.hoveredObject == null)
            return false;
        
        if (UICamera.hoveredObject.GetComponent<UISlimeInventroyButton>() == null)
            return false;

        return true;
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
	//      Slime
	//---------------------------------------
	public void UpdateHealthBar(float value)
	{
		healthBar.value = value;
	}

    public void ShowSwitchToNewSlimeWidget(PlayerCharacter player, bool enabled)
    {
        if (enabled)
        {
            UISprite previousSprite = Widget_SwitchToNewSlime.transform.Find("Sprite_PreviousSlime").GetComponent<UISprite>();
            UISprite newSprite = Widget_SwitchToNewSlime.transform.Find("Sprite_NewSlime").GetComponent<UISprite>();
            UILabel label = Widget_SwitchToNewSlime.transform.Find("Label_SwitchQuestion").GetComponent<UILabel>();
            
            previousSprite.spriteName = player.GetSlime().slimeSprite;
            newSprite.spriteName = player.slimeToSwapped.slimeSprite;
            label.text = "Do You want to switch to " + player.slimeToSwapped.slimeName + "?";

            Widget_SwitchToNewSlime.alpha = 1;
            LevelManager.instance.playerCharacter.DisablePlayerControl();
        }
        else
        {
            Widget_SwitchToNewSlime.alpha = 0;
            LevelManager.instance.playerCharacter.EnablePlayerControl();
        }

        Widget_SwitchToNewSlime.gameObject.SetActive(enabled);
    }

    public void SwitchToNewSlimeWidget_Yes()
    {
        OnReplyed("SwapSlime", true);
    }

    public void SwitchToNewSlimeWidget_No()
    {
        OnReplyed("SwapSlime", false);
    }

    public void OpenSlimeInventory()
    {
        ShowSlimeInventoryWidget(true);
    }

    public void ShowSlimeInventoryWidget(bool enabled)
    {
        UIWidget widget = Widget_SlimeInventroy;

        if (enabled)
        {
            widget.alpha = 1;
            LevelManager.instance.playerCharacter.DisablePlayerControl();
        }
        else
        {
            widget.alpha = 0;
            LevelManager.instance.playerCharacter.EnablePlayerControl();
        }

        widget.gameObject.SetActive(enabled);
    }

    public void ConfirmSwappingSlimeFromInventroy()
    {
        OnReplyed("SwapSlimeFromInventroy", true);
    }

    public void UpdateSlimeInventroy(PlayerCharacter player, SlimeType updatedSlimeType)
    {
        PlayerSlimeData slimeData = player.slimeData[updatedSlimeType];
        
        if (GetSlimeScrollItem(updatedSlimeType) == null)
        {
            GameObject obj = SlimeInventroyGrid.gameObject.AddChild(slimeScrollItem);
            SlimeInventroyGrid.Reposition();
            obj.GetComponent<UISlimeScrollItem>().slimeType = updatedSlimeType;
        }

        UISprite sprite = GetSlimeScrollItem(updatedSlimeType).GetComponentInChildren<UISprite>();
        sprite.spriteName = slimeData.slime.slimeSprite;

        if (!slimeData.unlocked)
            sprite.alpha = 0.25f;
        else
            sprite.alpha = 1f;
    }

    void OnSelectedSlimeInInventory(GameObject selectedObj)
    {
        PlayerSlimeData slimeData = LevelManager.instance.playerCharacter.slimeData[GetSlimeScrollItemSlimeType(selectedObj)];

        // Slime name
        UILabel Label_SlimeName = Widget_SlimeInventroy.transform.Find("Label_SlimeName").GetComponent<UILabel>();
        Label_SlimeName.text = slimeData.slime.slimeName;

        // Level Info
        UILabel Label_LevelInfo = Widget_SlimeInventroy.transform.Find("Label_LevelInfo").GetComponent<UILabel>();

        float digit_1 = slimeData.slimeExp - slimeData.CalculateSlimeExperience(slimeData.slimeLevel);
        float digit_2 = slimeData.slime.experienceData[slimeData.slimeLevel];

        Label_LevelInfo.text = "Lv: " + slimeData.slimeLevel + "   Exp: " + digit_1 + "/" + digit_2;

        // Description
        UILabel Label_Description = Widget_SlimeInventroy.transform.Find("Label_Description").GetComponent<UILabel>();
        if (slimeData.unlocked)
            Label_Description.text = slimeData.slime.slimeDescription;
        else
            Label_Description.text = "Gather more Slime Elements of this type of Slime to unlock it!!";

        // Enable/Disable Confirm button
        GameObject confirmButtonObj = Widget_SlimeInventroy.transform.Find("Sprite_Yes").gameObject;
        confirmButtonObj.SetActive(slimeData.unlocked);

        if (slimeData.unlocked)
            LevelManager.instance.playerCharacter.slimeToSwapped = slimeData.slime;
    }

    public SlimeType GetSlimeScrollItemSlimeType(GameObject obj)
    {
        return obj.GetComponent<UISlimeScrollItem>().slimeType;
    }

    public GameObject GetSlimeScrollItem(SlimeType thisType)
    {
        List<Transform> transList = SlimeInventroyGrid.GetChildList();

        foreach (Transform trans in transList)
        {
            if (GetSlimeScrollItemSlimeType(trans.gameObject) == thisType)
                return trans.gameObject;
        }

        return null;
    }

    //---------------------------------------
    //      Update Compass
    //---------------------------------------
    public void ActivateCompass(float duration)
	{
		StartCoroutine ("UpdateCompass", duration);
	}

	IEnumerator UpdateCompass(float duration)
	{
		float timer = 0;

		while(timer < duration)
		{
			compassWidget.alpha = 1;

			Vector3 playerPos = LevelManager.instance.playerCharacter.transform.position;
			Vector3 ObjetivePos = LevelManager.instance.tileObjective.transform.position;
			float angle = ((180 / Mathf.PI) * Mathf.Atan2(ObjetivePos.z - playerPos.z, ObjetivePos.x - playerPos.x)) - 90;
			compassSprite.transform.rotation =Quaternion.Euler(0, 0, angle);

			timer += Time.deltaTime;
			yield return null;
		}



		compassWidget.alpha = 0;
	}
}
