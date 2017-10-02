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
    public GameObject[] TileGeoO = new GameObject[1];
    public GameObject[] TileGeoI = new GameObject[1];
    public GameObject[] TileGeoII = new GameObject[1];
    public GameObject[] TileGeoL = new GameObject[1];
    public GameObject[] TileGeoC = new GameObject[1];

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
            obj = TileGeoO [Random.Range (0, TileGeoO.Length)];
			break;
		case 1:
            obj = TileGeoI [Random.Range (0, TileGeoI.Length)];
			break;
		case 2:
			obj = TileGeoL [Random.Range (0, TileGeoL.Length)];
			break;
		case 3:
			obj = TileGeoC [Random.Range (0, TileGeoC.Length)];
			break;
		case 4:
			obj = TileGeoII [Random.Range (0, TileGeoII.Length)];
			break;
		}

		return obj;
	}
}
