using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    public LevelManager levelManager;
    public PlayerCharacter playerCharacter;

    private Camera cam;
    private UIWidget joyStickArea;
    private UIJoyStick joyStick;

    public Dictionary<string, abilityButton> abilityButtons;
    

    //=======================================
    //      Struct
    //=======================================
    public struct abilityButton
    {
        public GameObject buttonObj;
        public UISprite mainSprite;
        public UISprite iconSprite;
    }

    //=======================================
    //      Functions
    //=======================================
    void Start()
    {
        levelManager = LevelManager.GetLevelManager();

        cam = GameObject.Find("UICamera").GetComponent<Camera>();
        joyStick = GameObject.Find("JoyStick").GetComponent<UIJoyStick>();
        joyStickArea = GameObject.Find("Widget_JoyStickArea").GetComponent<UIWidget>();

        abilityButtons = new Dictionary<string, abilityButton>();
        InitNewButton("Head");
        InitNewButton("Arms");
        InitNewButton("Body");
        InitNewButton("Legs");
        InitNewButton("Misc");
    }

	void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryUpdateJoyStickPos();
        }

        if (Input.GetMouseButtonUp(0))
        {
            levelManager.playerCharacter.keepWalkingAnim = false;
        }

        if (joyStick.joyStickDir != -1)
        {
            levelManager.playerCharacter.keepWalkingAnim = true;
            levelManager.playerCharacter.TryMoveToDirTile(joyStick.joyStickDir);
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
        newButton.iconSprite = newButton.buttonObj.transform.Find("Sprite_ability_icon").GetComponent<UISprite>();

        abilityButtons.Add(partType, newButton);
    }

    public void UpdateAbilityIcon(string partType)
    {
        abilityButtons[partType].buttonObj.SetActive(true);

        string newSprite = levelManager.playerCharacter.PlayerAbilities[partType].spriteName;
        if ((newSprite != null) && (AtlasHasSprite(abilityButtons[partType].iconSprite.atlas, newSprite)))
        {
            abilityButtons[partType].iconSprite.spriteName = levelManager.playerCharacter.PlayerAbilities[partType].spriteName;
        }
        else
        {
            abilityButtons[partType].iconSprite.spriteName = "Icon_" + partType + "_default";
        }  
    }

    public void ClearAbilityIcon(string partType)
    {
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
}
