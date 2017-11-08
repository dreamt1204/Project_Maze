using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlimeButton : MonoBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    UISprite slimeSprite;

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    void Start ()
    {
        slimeSprite = transform.parent.Find("Sprite_Slime").GetComponent<UISprite>();
    }
	
    void OnClick()
    {
        ToggleSlimeState();
    }

    void ToggleSlimeState()
    {
        PlayerCharacter player = LevelManager.instance.playerCharacter;

        if (player.slimeState == SlimeStateType.Eatting)
        {
            player.slimeState = SlimeStateType.Splitting;
            slimeSprite.spriteName = "Slime_Split";
        }  
		else if (player.slimeState == SlimeStateType.Splitting)
        {
            player.slimeState = SlimeStateType.Eatting;
            slimeSprite.spriteName = "Slime_Eat";
        }
    }
}
