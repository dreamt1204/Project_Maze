using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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



}