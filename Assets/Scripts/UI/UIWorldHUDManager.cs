using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldHUDType
{
	Sprite,
	Label
}

public class UIWorldHUDManager : MonoBehaviour
{
    public GameObject hudPrefab;
    public int widgetWidth = 100;
	public int widgetHeight = 100;

    [HideInInspector] public UIWorldHUD hud;
    UIWidget widget;
    UISprite sprite;
    public string startSprite;
    UILabel label;
    public string startLabel;

	void Start()
	{
        if (hud != null)
            return;

        SpawnHUD();
    }

    public void SpawnHUD()
    {
        Utilities.TryCatchError((hudPrefab == null), "WorldHUDManager(" + gameObject + ") doesn't have hudPrefab to spawn World HUD.");
        hud = Instantiate(hudPrefab.gameObject).GetComponent<UIWorldHUD>();
        GameObject hudObj = hud.gameObject;
        Utilities.TryCatchError((hud == null), "WorldHUD(" + hudObj + ") doesn't have UIWorldHUD script attached.");

        hudObj.name = gameObject.name + "_WorldHUD";

        widget = hudObj.GetComponent<UIWidget>();
        widget.width = widgetWidth;
        widget.height = widgetHeight;

        label = hudObj.GetComponent<UILabel>();
        if (startLabel != "")
            UpdateLabel(startLabel);

        sprite = hudObj.GetComponent<UISprite>();
        if (startSprite != "")
            UpdateSprite(startSprite);

        if (hud.hiddenInBeginning)
            widget.alpha = 0;
    }

	void Update()
	{
		UpdateHUDPos();
	}

	void UpdateHUDPos()
	{
        GameObject hudObj = hud.gameObject;

        if (hudObj == null)
            return;

        Vector3 worldPos;
		worldPos = Camera.main.WorldToViewportPoint(transform.position);
		worldPos = UICamera.mainCamera.ViewportToWorldPoint(worldPos);
		worldPos = new Vector3(worldPos.x, worldPos.y, 0);
		Transform parent = hudObj.transform.parent;
        hudObj.transform.localPosition = (parent != null) ? parent.InverseTransformPoint(worldPos) : worldPos;
	}

    public void PlayTweeners(int tweenGroup)
    {
        foreach (UITweener tween in hud.tweeners)
        {
            if (tween.tweenGroup == tweenGroup)
            {
                tween.ResetToBeginning();
                tween.PlayForward();
            }
        }

        if (hud.destroyAfterTweener)
            SpawnHUD();
    }

    //---------------------------------------
    //      Sprite
    //---------------------------------------
    public void UpdateSprite(string spriteName)
    {
        Utilities.TryCatchError((sprite == null), "This world HUD doesn't have UISprite component.");
        sprite.spriteName = spriteName;
    }

    public void UpdateSpriteAlpha(float alpha)
    {
        Utilities.TryCatchError((sprite == null), "This world HUD doesn't have UISprite component.");
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
    }

    //---------------------------------------
    //      Label
    //---------------------------------------
    public void UpdateLabel(string text)
    {
        Utilities.TryCatchError((label == null), "This world HUD doesn't have UILabel component.");
        label.text = text;
    }

    public void UpdateLabelColor(Color newColor)
    {
        Utilities.TryCatchError((label == null), "This world HUD doesn't have UILabel component.");
        label.color = newColor;
    }
}
