using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GeoType
{
	O,
	I,
	L,
	C,
	II
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
    // Use this for initialization
    void Start()
    {
		
    }

    // Update is called once per frame
    void Update()
    {

    }

	public GameObject GetGeoObj(GeoType geo_type)
	{
		GameObject geo_obj;

		switch ((int)geo_type)
		{
		default:
		case 0:
			geo_obj = TileGeoO [Random.Range (0, TileGeoO.Length)];
			break;
		case 1:
			geo_obj = TileGeoI [Random.Range (0, TileGeoI.Length)];
			break;
		case 2:
			geo_obj = TileGeoL [Random.Range (0, TileGeoL.Length)];
			break;
		case 3:
			geo_obj = TileGeoC [Random.Range (0, TileGeoC.Length)];
			break;
		case 4:
			geo_obj = TileGeoII [Random.Range (0, TileGeoII.Length)];
			break;
		}

		return geo_obj;
	}
}
