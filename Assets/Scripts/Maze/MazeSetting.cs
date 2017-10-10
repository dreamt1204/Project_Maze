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

    [Header("Item Set")]
    public TileItem bodyPartItem;
    public List<BodyPart> bodyParts;

    //=======================================
    //      Functions
    //=======================================   
    public GameObject GetWallLayoutObj(WallLayout wallLayout)
	{
		GameObject obj;

		switch ((int)wallLayout)
		{
		default:
		case 0:
            obj = TileLayoutO [Random.Range (0, TileLayoutO.Length)];
			break;
		case 1:
            obj = TileLayoutI [Random.Range (0, TileLayoutI.Length)];
			break;
		case 2:
			obj = TileLayoutL [Random.Range (0, TileLayoutL.Length)];
			break;
		case 3:
			obj = TileLayoutC [Random.Range (0, TileLayoutC.Length)];
			break;
		case 4:
			obj = TileLayoutII [Random.Range (0, TileLayoutII.Length)];
			break;
		}

		return obj;
	}
}
