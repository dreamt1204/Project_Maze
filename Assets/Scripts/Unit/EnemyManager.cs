using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Public enum most relevant to this class
public enum InitSpawnMethod // 2 types for now, can append in future
{
	Random, // random x, random y
	InGridRandom // divide maze into grids, then random location inside each maze.  More uniform than completely random
}

// SpawningInfo is a struct that holds (1) type of spawning enemy, and (2) spawning location
public struct spawningInfo
{
	public int enemyType;
	public Tile tile;

	public spawningInfo(int enemyType0, Tile tile0){
		enemyType = enemyType0;
		tile = tile0;
	}
}

public class EnemyManager {
	//=======================================
	//      Variables
	//=======================================

	public LevelManager levelManager;

	// public initEnemySpawnSetting m_setting;
	public int initSpawnQuantity; // roughly equals to number of enemies per tile
	public int safeRadius;
	public InitSpawnMethod initSpawnMethod;


	//=======================================
	//      Functions
	//======================================= 


	public EnemyManager (InitSpawnMethod spMethod, int spQuantity, int sfRadius)
	{
		levelManager = LevelManager.GetLevelManager();

		initSpawnQuantity = spQuantity;
		safeRadius = sfRadius;
		initSpawnMethod = spMethod;
	}

	public List<spawningInfo> GenerateInitSpawnList()
	{
		List<Tile> spawningTiles = new List<Tile> ();

		// Generate spawning locations, taking into account maze restrictions (e.g. playerChar's initial location)
		switch (initSpawnMethod) {
		case InitSpawnMethod.Random:

			// Figure out non-spawnable tiles; okay for repeating items in list
			levelManager.tileStart.DestroyTileItem ();
			List<Tile> nonSpawnableTiles = levelManager.maze.AllTilesAround (levelManager.tileStart, safeRadius);

			// Remove non-spawnable tiles: proximity to playerChar's initial location, physical objects, etc.
			List<Tile> spawnableTiles = new List<Tile> (levelManager.maze.tileList); // all tiles
			foreach (Tile t in nonSpawnableTiles) {
				spawnableTiles.Remove (t);
			}

			// Calculate number of enemies to spawn
//			int numInitEnemy = Mathf.RoundToInt (initSpawnDensity * spawnableTiles.Count);
			int numInitEnemy = initSpawnQuantity;
			Debug.Log ("Initial spawn quantity = " + initSpawnQuantity);

			for (int i = 0; i < numInitEnemy; i++) {

				Tile tileToSpawn = spawnableTiles [Mathf.FloorToInt (Random.Range (0, spawnableTiles.Count))];
			
				spawningTiles.Add (tileToSpawn);
				spawnableTiles.Remove (tileToSpawn);
			}
				
			break;
		}


		// Generate type of enemy on each location
		Debug.Log ("list of spawningTiles list = " + spawningTiles.Count);

		List<float> spawnProb = new List<float> ();

		//////////////// Soft code this for variable length enemy list
		spawnProb.Add (0.6f);
		spawnProb.Add (0.3f);
		spawnProb.Add (0.1f);

		float[] cumulativeProb = new float[ spawnProb.Count ];
		float cumProb = 0;

		// Generate cumulative probability
		for (int i = 0; i < cumulativeProb.Length; i++) {
			cumProb = cumProb + spawnProb [i];
			cumulativeProb [i] = cumProb;
//			Debug.Log ("cumulativeProb[" + i + "] is " + cumProb);
		}

		// Determine enemy type by comparing rand(0,1) to cumulative prob list
		int[] spawningEnemyType = new int[ spawningTiles.Count ];
		for (int i = 0; i < spawningTiles.Count; i++) {
			float randNum = Random.Range (0.0f, 1.0f);
			for (int j = 0; j < cumulativeProb.Length; j++) {
				if (randNum < cumulativeProb [j]) {
					spawningEnemyType [i] = j;
					Debug.Log ("Random num is " + randNum + ", cumulativeProb is " + cumulativeProb [j]);
					Debug.Log ("EnemySpawning at (X = " + spawningTiles [i].X + ", Z = " + spawningTiles [i].Z + ") is Type #" + j);
					break;
				}
			}
		}

		// Package type and location into SpawningInfo, for UnitSpawner to spawn
		List<spawningInfo> spawnList = new List<spawningInfo>();
		if (spawningTiles.Count == spawningEnemyType.Length) {	
			for (int i = 0; i < spawningTiles.Count; i++) {
				spawnList.Add (new spawningInfo (spawningEnemyType [i], spawningTiles [i]));
				//enemyList.Add(SpawnEnemy(spawningEnemyType [i], spawningTiles [i]));
			}
		} else {
			Debug.LogError ("Inconsistent legnth between spawningTiles and spawningEnemyType.");
		}

		return spawnList;

	}

}
