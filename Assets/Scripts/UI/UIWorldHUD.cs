using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldHUDType
{
	Sprite,
	Label
}

public class UIWorldHUD : MonoBehaviour
{
    bool spawned;

    public GameObject hudPrefab;
    public int widgetWidth = 100;
	public int widgetHeight = 100;

    protected GameObject hudObj;
    UIWidget widget;
    UISprite sprite;
    UILabel label;
    protected UITweener[] tweeners;

	void Start()
	{
        if (spawned)
            return;

        SpawnHUD();
    }

    public void SpawnHUD()
    {
        hudObj = Instantiate(hudPrefab.gameObject);
        hudObj.name = gameObject.name + "_WorldHUD";
        tweeners = hudObj.GetComponents<UITweener>();

        widget = hudObj.GetComponent<UIWidget>();
        widget.width = widgetWidth;
        widget.height = widgetHeight;

        label = hudObj.GetComponent<UILabel>();
        sprite = hudObj.GetComponent<UISprite>();

        if (hudObj.GetComponent<UIDestroyAfterTweener>())
            widget.alpha = 0;

        spawned = true;
    }

	void Update()
	{
		UpdateHUDPos();
	}

	void UpdateHUDPos()
	{
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
        foreach (UITweener tween in tweeners)
        {
            if (tween.tweenGroup == tweenGroup)
            {
                tween.ResetToBeginning();
                tween.PlayForward();
            }
        }

        if (hudObj.GetComponent<UIDestroyAfterTweener>())
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
