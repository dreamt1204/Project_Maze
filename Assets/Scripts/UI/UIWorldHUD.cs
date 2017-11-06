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
	bool finishedInit;

    public GameObject hudPrefab;
    public int widgetWidth = 100;
	public int widgetHeight = 100;

    protected GameObject hudObj;
    protected UITweener[] tweeners;

	void Start()
	{
		if (finishedInit)
			return;

		Init();
	}

	public virtual void Init()
	{
		finishedInit = true;
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
		Transform parent = hudObj.transform.parent;
        hudObj.transform.localPosition = (parent != null) ? parent.InverseTransformPoint(worldPos) : worldPos;
	}

    public void PlayTweens(int tweenGroup)
    {
        foreach (UITweener tween in tweeners)
        {
            if (tween.tweenGroup == tweenGroup)
            {
                tween.ResetToBeginning();
                tween.PlayForward();
            }
        }
    }
}
