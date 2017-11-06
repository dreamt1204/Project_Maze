using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldHUDType
{
	Sprite,
	Label
}

public class WorldHUD : MonoBehaviour
{
	bool finishedInit;

	public WorldHUDType type;
	public int widgetWidth = 100;
	public int widgetHeight = 100;

	GameObject HUDObj;
	UISprite sprite;
	UILabel label;

	void Start()
	{
		if (finishedInit)
			return;

		Init();
	}

	public void Init()
	{
		switch (type)
		{
			case WorldHUDType.Sprite:
				GenerateSprite();
				break;
			case WorldHUDType.Label:
				GenerateLabel();
				break;
		}

		finishedInit = true;
	}

	void GenerateSprite()
	{
		HUDObj = (GameObject)Instantiate(Resources.Load("Sprite_WorldHUD"));
		HUDObj.name = gameObject.name + "_WorldHUD";
		sprite = HUDObj.GetComponent<UISprite>();
		sprite.width = widgetWidth;
		sprite.height = widgetHeight;
	}

	void GenerateLabel()
	{
		HUDObj = (GameObject)Instantiate(Resources.Load("Label_WorldHUD"));
		HUDObj.name = gameObject.name + "_WorldHUD";
		label = HUDObj.GetComponent<UILabel>();
		label.width = widgetWidth;
		label.height = widgetHeight;
	}

	void Update()
	{
		UpdateHUDPos();
	}

	void UpdateHUDPos()
	{
		Vector3 worldPos;
		worldPos = Camera.main.WorldToViewportPoint(transform.position);
		worldPos = UICamera.mainCamera.ViewportToWorldPoint(worldPos);
		worldPos = new Vector3(worldPos.x, worldPos.y, 0);
		Transform parent = HUDObj.transform.parent;
		HUDObj.transform.localPosition = (parent != null) ? parent.InverseTransformPoint(worldPos) : worldPos;
	}

	//---------------------------------------
	//      Sprite
	//---------------------------------------
	public void UpdateSprite(string spriteName)
	{
		sprite.spriteName = spriteName;
	}

	public void UpdateSpriteAlpha(float alpha)
	{
		sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
	}
}
