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

	public static Monster SpawnMonster (int monsterType, Tile targetTile)
	{
		GameObject monsterPrefab = new GameObject();
		switch (monsterType) {
		case 0:
			monsterPrefab = LevelManager.instance.prefabA;
			break;
		case 1:
			monsterPrefab = LevelManager.instance.prefabB;
			break;
		case 2:
			monsterPrefab = LevelManager.instance.prefabC;
			break;
		}

		return (Monster)SpawnUnit (monsterPrefab, targetTile);
	}

	public static List<Monster> SpawnInitMonsters(List<spawningInfo> spawnList)
	{
		List<Monster> monsterList = new List<Monster> ();
		foreach (spawningInfo sInfo in spawnList) {
			monsterList.Add (SpawnMonster (sInfo.monsterType, sInfo.tile));
		}
		return monsterList;
	}
		

}
