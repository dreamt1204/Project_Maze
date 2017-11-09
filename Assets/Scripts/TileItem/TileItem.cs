﻿//============================== Class Definition ==============================
// 
// This a container class store all the info of tile item.
// Have to create prefab with this attached for future use.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
	StartPoint,
	Objective,
    BodyPart,
    SlimeElement,
    HealthPack,
    Compass
}

public class TileItem : MonoBehaviour
{
	//=======================================
	//      Variables
	//=======================================
	public ItemType itemType;

    [Header("Body Part")]
    public BodyPart bodyPart;

    [Header("Slime Element")]
    public Slime slime;

    [Header("Health Pack")]
    public float healAmount;

    [Header("Compass")]
    public float compassDuration;

    //=======================================
    //      Variables
    //=======================================
    void Start()
    {
        if (itemType == ItemType.SlimeElement)
            UpdateSlimeElementIcon();
    }

    public void EnableSlimeSwapButton(bool enabled)
    {
        UIWorldHUD hud = GetComponentInChildren<UIWorldHUD>();
        if (!hud.spawned)
            hud.SpawnHUD();

        if (enabled)
        {
            hud.hudObj.SetActive(true);
        }
        else
        {
            hud.hudObj.SetActive(false);
        }
    }

    void UpdateSlimeElementIcon()
    {
        UIWorldHUD hud = GetComponentInChildren<UIWorldHUD>();
        hud.startSprite = slime.slimeSprite;
    }

    public void DestroyWorldHUD()
    {
        UIWorldHUD hud = GetComponentInChildren<UIWorldHUD>();
        if ((hud != null) && (hud.hudObj))
            Destroy(hud.hudObj);
    }
}
