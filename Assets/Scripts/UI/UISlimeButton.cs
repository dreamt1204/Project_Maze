using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlimeButton : MonoBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    UISprite slimeSprite;

    public Slime currentSlime;

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
        if (currentSlime.slimeState == SlimeStateType.Eatting)
        {
            currentSlime.slimeState = SlimeStateType.Splitting;
            slimeSprite.spriteName = "Slime_Split";
        }  
        else if (currentSlime.slimeState == SlimeStateType.Splitting)
        {
            currentSlime.slimeState = SlimeStateType.Eatting;
            slimeSprite.spriteName = "Slime_Eat";
        }
    }
}
