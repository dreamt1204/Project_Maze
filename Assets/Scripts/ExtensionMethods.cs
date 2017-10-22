using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ExtensionMethods {



	// Shortest complex distance (regarding wall) between tiles, by connectivity 4
	public static int ComplexDistBtwTiles (){
		return 0;
		// Relatively easy to implement if maze stays like a minimally spanning tree
		// Computationally expensive if not -- basically no invariants about the maze so need to search a lot of possibilities

	}

	public static List<Tile> PathOfShortestDist() {
		return null;
	}


	public static void PrintTileList (string string0, List<Tile> tileList){
		Debug.Log("PrintTileList called by *" + string0 + "*, tiles in list are:");
		for (int n = 0; n < tileList.Count; n++){
			int tileNum = n + 1;
			Debug.Log("Tile #" + (tileNum) + " is at X = " + tileList[n].X + ", Z = " + tileList[n].Z);
		}
	}

	public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val)
	{
		return values.Select((b,i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
	}


}