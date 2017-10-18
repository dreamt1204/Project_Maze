//============================== Class Definition ==============================
// 
// This is the base class for Player ability.
// Have to create prefab with this attached for future use.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerAbility : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	LevelManager level;

    public string abilityName;
    public string spriteName;

	[Header("Range Data")]
	public RangeData rangeData;

    //=======================================
    //      Functions
    //=======================================
	public virtual void Init()
	{
        level = LevelManager.instance;
	}

	public void DisplayRangeTiles()
	{
		List<Tile> tiles = MazeUTL.GetRangeTiles (level.playerCharacter.CurrentTile, rangeData);

		foreach (Tile tile in tiles)
		{
			tile.State = TileState.Selectable;
		}
	}

    public virtual void ActivateAbility(Tile selectedTile)
    {

    }
}