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

	public static Enemy SpawnEnemy (int enemyType, Tile targetTile)
	{
		GameObject enemyPrefab = new GameObject();
		switch (enemyType) {
		case 0:
			enemyPrefab = LevelManager.instance.prefabA;
			break;
		case 1:
			enemyPrefab = LevelManager.instance.prefabB;
			break;
		case 2:
			enemyPrefab = LevelManager.instance.prefabC;
			break;
		}

		return (Enemy)SpawnUnit (enemyPrefab, targetTile);
	}

	public static List<Enemy> SpawnInitEnemies(List<spawningInfo> spawnList)
	{
		List<Enemy> enemyList = new List<Enemy> ();
		foreach (spawningInfo sInfo in spawnList) {
			enemyList.Add (SpawnEnemy (sInfo.enemyType, sInfo.tile));
		}
		return enemyList;
	}
		

}
