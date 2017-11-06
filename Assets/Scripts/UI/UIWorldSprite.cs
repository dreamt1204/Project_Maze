using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWorldSprite : UIWorldHUD
{
    UISprite sprite;

    public override void Init()
    {
        hudObj = Instantiate(hudPrefab.gameObject);
        hudObj.name = gameObject.name + "_WorldHUD";
        tweeners = hudObj.GetComponents<UITweener>();

        sprite = hudObj.GetComponent<UISprite>();
        sprite.width = widgetWidth;
        sprite.height = widgetHeight;
        
        base.Init();
    }

    public void UpdateSprite(string spriteName)
    {
        sprite.spriteName = spriteName;
    }

    public void UpdateSpriteAlpha(float alpha)
    {
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
    }
}
