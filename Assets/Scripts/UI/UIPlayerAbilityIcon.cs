//============================== Class Definition ==============================
// 
// Custom DragDropItem class for Player Ability Icons.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayerAbilityIcon : UIDragDropItem {
	LevelManager level = LevelManager.instance;
	string partType;

    protected override void OnDragDropStart()
    {
        partType = gameObject.name.Substring (gameObject.name.LastIndexOf ("_") + 1);

        level.playerCharacter.PlayerAbilities [partType].DisplayRangeTiles ();

		base.OnDragDropStart();
    }

	protected override void OnDragDropRelease(GameObject surface)
	{
        base.OnDragDropRelease(surface);
        MazeUTL.ResetAllTileState();

        if ((surface == null) || (surface.GetComponent<Tile>() == null) || (surface.GetComponent<Tile>().State != TileState.Selectable))
            return;

        level.playerCharacter.PlayerAbilities[partType].ActivateAbility(surface.GetComponent<Tile>());
    }
}
