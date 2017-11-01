using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertedIcon : MonoBehaviour
{
    Monster parentMonster;
    GameObject AlertedIconObj;
    UISprite sprite;

    public void Init(Monster monster)
    {
        parentMonster = monster;
        AlertedIconObj = parentMonster.transform.Find("AlertedIconObj").gameObject;
        sprite = gameObject.GetComponent<UISprite>();

        UpdateIconSprite(parentMonster.detectingState);
    }

    void Update()
    {
        if (parentMonster == null)
            return;

        if (AlertedIconObj == null)
            return;

        UpdateIconPos();
    }

    void UpdateIconPos()
    {
        Vector3 worldPos;
        worldPos = Camera.main.WorldToViewportPoint(AlertedIconObj.transform.position);
        worldPos = UICamera.mainCamera.ViewportToWorldPoint(worldPos);
        worldPos = new Vector3(worldPos.x, worldPos.y, 0);
        Transform parent = transform.parent;
        transform.localPosition = (parent != null) ? parent.InverseTransformPoint(worldPos) : worldPos;
    }

    public void UpdateIconSprite(DetectingState state)
    {
        string spriteName = "";

        if (state == DetectingState.Warning)
        {
            spriteName = "Icon-Warning";
        }
        else if (state == DetectingState.Alerted)
        {
            spriteName = "Icon-Alerted";
        }
        else
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
            return;
        }

        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1);
        sprite.spriteName = spriteName;
    }
}
