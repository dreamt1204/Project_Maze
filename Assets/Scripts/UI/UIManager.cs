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

    private UILabel timerLabel;

    private Camera cam;
    private UIWidget joyStickArea;
	[HideInInspector] public  UIJoyStick joyStick;
	[HideInInspector] public bool joyStickEnabled;
	[HideInInspector] public Dictionary<string, abilityButton> abilityButtons;

	private UIProgressBar healthBar;

    [HideInInspector] public UIWidget compassWidget;
	[HideInInspector] public UISprite compassSprite;


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

		healthBar = GameObject.Find("HealthBar").GetComponent<UIProgressBar>();

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
