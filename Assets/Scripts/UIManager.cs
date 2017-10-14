using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager {
    //=======================================
    //      Variables
    //=======================================
    public Dictionary<string, GameObject> abilityIcons;

    //=======================================
    //      Struct
    //=======================================
    public UIManager()
    {
        abilityIcons = new Dictionary<string, GameObject>();
        abilityIcons.Add("Head", GameObject.Find("Sprite_ability_Head"));
        abilityIcons.Add("Arms", GameObject.Find("Sprite_ability_Arms"));
        abilityIcons.Add("Body", GameObject.Find("Sprite_ability_Body"));
        abilityIcons.Add("Legs", GameObject.Find("Sprite_ability_Legs"));
        abilityIcons.Add("Misc", GameObject.Find("Sprite_ability_Misc"));
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

    public void UpdateAbilityIcon(string partType)
    {
        abilityIcons[partType].SetActive(true);
    }

    public void ClearAbilityIcon(string partType)
    {
        abilityIcons[partType].SetActive(false);
    }
}
