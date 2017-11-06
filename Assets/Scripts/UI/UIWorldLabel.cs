using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWorldLabel : UIWorldHUD
{
    UILabel label;

    public override void Init()
    {
        hudObj = Instantiate(hudPrefab.gameObject);
        hudObj.name = gameObject.name + "_WorldHUD";
        tweeners = hudObj.GetComponents<UITweener>();

        label = hudObj.GetComponent<UILabel>();
        label.width = widgetWidth;
        label.height = widgetHeight;

        base.Init();
    }

    public void UpdateLabel(string text)
    {
        label.text = text;
    }

    public void UpdateLabelColor(Color newColor)
    {
        label.color = newColor;
    }
}
