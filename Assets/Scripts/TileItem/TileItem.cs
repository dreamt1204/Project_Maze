//============================== Class Definition ==============================
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
    SlimeInventory,
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
        else if (itemType == ItemType.SlimeInventory)
            EnableSlimeSwapButton(false);
    }

    public void EnableSlimeSwapButton(bool enabled)
    {
        UIWorldHUDManager worldHUDManager = GetComponentInChildren<UIWorldHUDManager>();
        if (worldHUDManager.hud == null)
            worldHUDManager.SpawnHUD();

        worldHUDManager.hud.gameObject.SetActive(enabled);
    }

    void UpdateSlimeElementIcon()
    {
        UIWorldHUDManager hud = GetComponentInChildren<UIWorldHUDManager>();
        hud.startSprite = slime.slimeSprite;
    }

    public void DestroyWorldHUD()
    {
        UIWorldHUDManager worldHUDManager = GetComponentInChildren<UIWorldHUDManager>();
        if ((worldHUDManager != null) && (worldHUDManager.hud.gameObject != null))
            Destroy(worldHUDManager.hud.gameObject);
    }
}
