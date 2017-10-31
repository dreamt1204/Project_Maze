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
            if (LevelManager.instance.mazeGenerator.customMazeMonsterObjList.Count > 0)
                SpawnMonsters_Custom();
        }
        else
        {
            SpawnMonsters_Random();
        }
    }

    public static void SpawnMonsters_Custom()
    {
        List<Monster> MonsterObjList = LevelManager.instance.mazeGenerator.customMazeMonsterObjList;

        foreach (Monster monster in MonsterObjList)
        {
            int X = Mathf.FloorToInt(monster.gameObject.transform.position.x / 10);
            int Z = Mathf.FloorToInt(monster.gameObject.transform.position.z / 10);

            Unit unit = GameObject.Instantiate(monster.gameObject, monster.transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Unit>();
            unit.Init(LevelManager.instance.maze.mazeTile[X, Z]);
        }
    }

    public static void SpawnMonsters_Random()
    {
        List<Tile> spawnTiles = new List<Tile>();

        int monsterNum = Formula.CalculateMonsterNum(LevelManager.instance.mazeDifficulty);
        int distance = Formula.CalculateMonsterLeastDistance(LevelManager.instance.mazeDifficulty);
        List<Tile> oldList = LevelManager.instance.maze.mazeTileList;
        List<Tile> exclusiveTiles = new List<Tile>();

        exclusiveTiles.Add(LevelManager.instance.tileStart);

        for (int i = 0; i < monsterNum; i++)
        {
            oldList = MazeUTL.UpdateTileListOutOfRange(oldList, exclusiveTiles, distance);

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
