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
		Maze.ResetAllTileState ();

		if (surface == null)
			return;

		Tile selectedTile = surface.GetComponent<Tile> ();
		if (selectedTile == null)
			return;
		if (selectedTile.State != TileState.Selectable)
			return;

		Debug.Log (selectedTile.gameObject.name);
	}
}
