//============================== Class Definition ==============================
// 
// This is a data class that stores all the maze specific setting for level to generate maze.
// Specific setting includes tile layout models, Item Set, Body Parts Set..etc.
// Have to create prefab with this attached for future use.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tile wall layout
public enum WallLayout
{
	O,	// No wall
	I,	// 1 wall
	L,	// 2 walls form L shape
	C,	// 3 walls form C shape
	II	// 2 walls parallel to each other
}

public class MazeSetting : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    [Header("Layout Set")]
    public GameObject[] TileLayoutO = new GameObject[1];
    public GameObject[] TileLayoutI = new GameObject[1];
    public GameObject[] TileLayoutII = new GameObject[1];
    public GameObject[] TileLayoutL = new GameObject[1];
    public GameObject[] TileLayoutC = new GameObject[1];

    [Header("Room Set")]
    public GameObject[] Rooms = new GameObject[1];

    [Header("Item Set")]
    public List<TileItem> SlimeElements;
    public List<BodyPart> bodyParts;

    [Header("Monster Set")]
    public List<Monster> Monsters;

    //=======================================
    //      Functions
    //=======================================
    public GameObject GetWallLayoutObj(WallLayout wallLayout)
	{
        GameObject[] objList;

        switch ((int)wallLayout)
		{
		default:
            objList = new GameObject[0];
            break;
        case 0:
            objList = TileLayoutO;
			break;
		case 1:
            objList = TileLayoutI;
			break;
		case 2:
            objList = TileLayoutL;
			break;
		case 3:
            objList = TileLayoutC;
			break;
		case 4:
             objList = TileLayoutII;
			break;
		}

        Utilities.TryCatchError((objList.Length <= 0), "'TileLayout" + wallLayout + "' has wrong setup in current Maze Setting.");

        return objList[Random.Range(0, objList.Length)];
	}
}
