using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityDragDropItem : UIDragDropItem {
	LevelManager levelManager;
	string partType;

    protected override void OnDragDropStart()
    {
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		partType = gameObject.name.Substring (gameObject.name.LastIndexOf ("_") + 1);

		levelManager.playerCharacter.PlayerAbilities [partType].DisplayRangeTiles ();

		base.OnDragDropStart();
    }

	protected override void OnDragDropRelease(GameObject surface)
	{
        base.OnDragDropRelease(surface);

        if ((surface == null) || (surface.GetComponent<Tile>() == null) || (surface.GetComponent<Tile>().State != TileState.Selectable))
        {
            Maze.ResetAllTileState();
            return;
        }

        Maze.ResetAllTileState();
        levelManager.playerCharacter.PlayerAbilities[partType].ActivateAbility(surface.GetComponent<Tile>());
    }
}
