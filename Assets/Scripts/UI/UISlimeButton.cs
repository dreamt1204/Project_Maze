using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlimeButton : MonoBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    UISprite slimeSprite;
    UISprite slimeStateSprite;

    //=======================================
    //      Functions
    //=======================================
    public void Init()
    {
        slimeSprite = transform.parent.Find("Sprite_Slime").GetComponent<UISprite>();
        slimeStateSprite = transform.parent.Find("Sprite_SlimeState").GetComponent<UISprite>();

        PlayerCharacter.OnSlimeSwapped += UpdateSlimeSprite;
    }

    //---------------------------------------
    //      Slime Sprite
    //---------------------------------------
    void UpdateSlimeSprite(PlayerCharacter player, Slime newSlime)
    {
        slimeSprite.spriteName = player.GetSlime().slimeSprite;
    }

    //---------------------------------------
    //      Slime State
    //---------------------------------------
    void OnClick()
    {
        if (LevelManager.instance.playerCharacter.IsPlayerControlDisabled())
            return;

        ToggleSlimeState();
    }

    void ToggleSlimeState()
    {
        PlayerCharacter player = LevelManager.instance.playerCharacter;

        if (player.slimeState == SlimeStateType.Eatting)
        {
            player.slimeState = SlimeStateType.Splitting;
            slimeStateSprite.spriteName = "Slime_Split";
        }  
		else if (player.slimeState == SlimeStateType.Splitting)
        {
            player.slimeState = SlimeStateType.Eatting;
            slimeStateSprite.spriteName = "Slime_Eat";
        }
    }
}
