//============================== Class Definition ==============================
// 
// Custom DragDropItem class for Slime State.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlimeDragButton : UIDragDropItem
{
    protected override void OnPress(bool isDown)
    {
        base.OnPress(isDown);

        if (isDown)
            UIManager.instance.slimeStateSelectWidget.alpha = 1;
        else
            UIManager.instance.slimeStateSelectWidget.alpha = 0;
    }

    protected override void OnDragDropStart()
    {
        UIManager.instance.slimeStateSelectWidget.alpha = 1;

        base.OnDragDropStart();
    }

    protected override void OnDragDropRelease(GameObject surface)
    {
        base.OnDragDropRelease(surface);
        UIManager.instance.slimeStateSelectWidget.alpha = 0;
        this.transform.localPosition = new Vector3(0, 0, 0);

        SlimeStateType newState = SlimeStateType.None;

		if (surface.name == "Sprite_Slime_None")
            newState = SlimeStateType.None;
        else if (surface.name == "Sprite_Slime_Splitting")
            newState = SlimeStateType.Splitting;
        else if (surface.name == "Sprite_Slime_Eatting")
            newState = SlimeStateType.Eatting;
        else
            return;

        UIManager.instance.UpdateSlimeDragButtonSprite(newState);
        LevelManager.instance.playerCharacter.SlimeState = newState;
    }
}
