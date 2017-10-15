using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager {
    //=======================================
    //      Variables
    //=======================================
    public LevelManager levelManager;
    public PlayerCharacter playerCharacter;

    public Dictionary<string, abilityButton> abilityButtons;
    

    //=======================================
    //      Struct
    //=======================================
    public UIManager()
    {
        levelManager = LevelManager.GetLevelManager();

        abilityButtons = new Dictionary<string, abilityButton>();
        InitNewButton("Head");
        InitNewButton("Arms");
        InitNewButton("Body");
        InitNewButton("Legs");
        InitNewButton("Misc");
    }

    public struct abilityButton
    {
        public GameObject buttonObj;
        public UISprite mainSprite;
        public UISprite iconSprite;
    }

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    void Start ()
    {
        
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

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
        if (newSprite!=null)
            abilityButtons[partType].iconSprite.spriteName = levelManager.playerCharacter.PlayerAbilities[partType].spriteName;
    }

    public void ClearAbilityIcon(string partType)
    {
        abilityButtons[partType].buttonObj.SetActive(false);
    }
}
