using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
	//=======================================
	//      Variables
	//=======================================

	public LevelManager levelManager;

	public enum InitSpawnMethod // 2 types for now, can append in future
	{
		Random, // random x, random y
		InGridRandom // divide maze into grids, then random location inside each maze.  More uniform than completely random
	}


	// public initEnemySpawnSetting m_setting;
	public float initSpawnDensity = 0.04f; // roughly equals to number of enemies per tile
	public int startTileNoSpawnRadius = 5;
	public InitSpawnMethod initSpawnMethod = InitSpawnMethod.Random;

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

	//=======================================
	//      Functions
	//======================================= 
	public void SpawnInitEnemies()
	{
		List<Tile> spawningTiles = new List<Tile>();

		// Generate spawning locations, taking into account maze restrictions (e.g. playerChar's initial location)
		switch (initSpawnMethod) {
		case InitSpawnMethod.Random:

			// Figure out non-spawnable tiles; okay for repeating items in list
			List<Tile> nonSpawnableTiles = levelManager.maze.AllTilesAround (levelManager.startTile, startTileNoSpawnRadius);

			// Remove non-spawnable tiles: proximity to playerChar's initial location, physical objects, etc.
			List<Tile> spawnableTiles = levelManager.maze.tileList; // all tiles
			foreach (Tile t in nonSpawnableTiles) {
				spawnableTiles.Remove (t);
			}

			// Calculate number of enemies to spawn
			int numInitEnemy = Mathf.RoundToInt (1.0f / initSpawnDensity);

			Debug.Log("number of enemy to spawn initially = " + numInitEnemy);

			for (int i = 0; i < numInitEnemy; i++) {

				Tile tileToSpawn = spawnableTiles[ Mathf.FloorToInt(Random.Range(0,spawnableTiles.Count)) ];
			
				spawningTiles.Add(tileToSpawn);
				spawnableTiles.Remove(tileToSpawn);
			}
				
			break;
		}


		// Generate type of enemy on each location
		Debug.Log("list of spawningTiles list = " + spawningTiles.Count);

		List<float> spawnProb = new List<float>();

		//////////////// Soft code this for variable length enemy list
		spawnProb.Add(0.6f);
		spawnProb.Add(0.3f);
		spawnProb.Add(0.1f);

		float[] cumulativeProb = new float[ spawnProb.Count ];
		float cumProb = 0;

		// Generate cumulative probability
		for (int i = 0; i < cumulativeProb.Length; i++)
		{
			cumProb = cumProb + spawnProb[i];
			cumulativeProb[i] = cumProb;
			Debug.Log ("cumulativeProb[" + i + "] is " + cumProb);
		}

		// Determine enemy type by comparing rand(0,1) to cumulative prob list
		int[] spawningEnemyType = new int[ spawningTiles.Count ];
		for (int i = 0; i < spawningTiles.Count; i++)
		{
			float randNum = Random.Range (0.0f, 1.0f);
			for (int j = 0; j < cumulativeProb.Length; j++) 
			{
				if (randNum < cumulativeProb[j])
				{
					spawningEnemyType[i] = j;
					Debug.Log("Random num is " + randNum + ", cumulativeProb is " + cumulativeProb[j]);
					Debug.Log("EnemySpawning at (X = " + spawningTiles[i].X + ", Z = " + spawningTiles[i].Z + ") is Type #" + j);
					break;
				}
			}
		}

		// Instantiation of enemies on locations based on plan


	}

}
