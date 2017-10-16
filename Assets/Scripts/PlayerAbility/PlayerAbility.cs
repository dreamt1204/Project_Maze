using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerAbility : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	private LevelManager levelManager;
	private PlayerCharacter playerCharacter;

    public string abilityName;
    public string spriteName;

	[Header("Range Data")]
	public RangeData rangeData;

    //=======================================
    //      Functions
    //=======================================
	public virtual void Init()
	{
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		playerCharacter = levelManager.playerCharacter;
	}

	public void DisplayRangeTiles()
	{
		List<Tile> tiles = Maze.GetRangeTiles (playerCharacter.CurrentTile, rangeData);

		foreach (Tile tile in tiles)
		{
			tile.State = TileState.Selectable;
		}
	}

    public virtual void ActivateAbility(Tile selectedTile)
    {

    }
}