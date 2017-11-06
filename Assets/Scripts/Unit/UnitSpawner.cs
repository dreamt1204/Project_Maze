//============================== Class Definition ==============================
// 
// This is a static class that contains all the unit spawn functions.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitSpawner {
    //=======================================
    //      Static Functions
    //=======================================
    public static Unit SpawnUnit(GameObject prefab, Tile targetTile)
    {
        Unit unit = GameObject.Instantiate(prefab, targetTile.transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Unit>();
		unit.Init(targetTile);

        return unit;
    }

    public static PlayerCharacter SpawnPlayerCharacter(Tile targetTile)
    {
		return (PlayerCharacter)SpawnUnit(LevelManager.instance.playerCharacterPrefab, targetTile);
    }

    public static Monster SpawnMonster(Tile targetTile)
    {
        List<Monster> monsterList = LevelManager.instance.mazeSetting.Monsters;
        Monster newMonster = monsterList[Random.Range(0, monsterList.Count)];

        return (Monster)SpawnUnit(newMonster.gameObject, targetTile);
    }

    public static void SpawnMonsters()
    {
        if (LevelManager.instance.customMazeObject != null)
        {
			if (LevelManager.instance.mazeGenerator.customMonsterSpawnerList.Count > 0)
                SpawnMonsters_Custom();
        }
        else
        {
            SpawnMonsters_Random();
        }
    }

    public static void SpawnMonsters_Custom()
    {
		List<MonsterSpawner> spawners = LevelManager.instance.mazeGenerator.customMonsterSpawnerList;

		foreach (MonsterSpawner spawner in spawners)
        {
			Monster monster = spawner.Monsters[Random.Range(0, spawner.Monsters.Count)];

			int X = Mathf.FloorToInt(spawner.transform.position.x / 10);
			int Z = Mathf.FloorToInt(spawner.transform.position.z / 10);

			Unit unit = GameObject.Instantiate(monster.gameObject, spawner.transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Unit>();
            unit.Init(LevelManager.instance.maze.mazeTile[X, Z]);
        }
    }

    public static void SpawnMonsters_Random()
    {
        List<Tile> spawnTiles = new List<Tile>();

        int monsterNum = Formula.CalculateMonsterNum(LevelManager.instance.mazeDifficulty);
		int distancePlayer = Formula.CalculateMonsterPlayerLeastDistance(LevelManager.instance.mazeDifficulty);
        int distanceMonster = Formula.CalculateMonsterLeastDistance(LevelManager.instance.mazeDifficulty);
        List<Tile> exclusiveTiles = new List<Tile>();
        exclusiveTiles.Add(LevelManager.instance.tileStart);

		List<Tile> oldList = MazeUTL.UpdateTileListOutOfRange(LevelManager.instance.maze.mazeTileList, exclusiveTiles, distancePlayer);

        for (int i = 0; i < monsterNum; i++)
        {
			oldList = MazeUTL.UpdateTileListOutOfRange(oldList, exclusiveTiles, distanceMonster);

            Tile spawnTile = oldList[Random.Range(0, oldList.Count)];
            spawnTiles.Add(spawnTile);
            exclusiveTiles.Add(spawnTile);
        }

        foreach (Tile tile in spawnTiles)
        {
            SpawnMonster(tile);
        }
    }
}
