using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlimeButton : MonoBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    UISprite slimeSprite;

    public Slime slime;

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
		if (slime.slimeState == SlimeStateType.Eatting)
        {
			slime.slimeState = SlimeStateType.Splitting;
            slimeSprite.spriteName = "Slime_Split";
        }  
		else if (slime.slimeState == SlimeStateType.Splitting)
        {
			slime.slimeState = SlimeStateType.Eatting;
            slimeSprite.spriteName = "Slime_Eat";
        }
    }
}
