using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner {
    //=======================================
    //      Variables
    //=======================================
    LevelManager levelManager;

    public UnitSpawner()
    {
        levelManager = LevelManager.GetLevelManager();
    }

    //=======================================
    //      Functions
    //=======================================
    public Unit SpawnUnit(GameObject prefab, Tile targetTile)
    {
        Unit unit = GameObject.Instantiate(prefab, targetTile.transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<Unit>();
        unit.Init(levelManager, targetTile);

        return unit;
    }

    public PlayerCharacter SpawnPlayerCharacter(Tile targetTile)
    {
        GameObject playerCharacterPrefab = levelManager.playerCharacterPrefab;

        PlayerCharacter character = GameObject.Instantiate(playerCharacterPrefab, targetTile.transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<PlayerCharacter>();
        character.Init(levelManager, targetTile);

        return character;
    }

	public Enemy SpawnEnemy (int enemyType, Tile targetTile)
	{
		GameObject prefabA = levelManager.prefabA;
		GameObject prefabB = levelManager.prefabB;
		GameObject prefabC = levelManager.prefabC;

		Enemy enemy = new Enemy ();

		switch (enemyType) {
		case 0:
			enemy = GameObject.Instantiate (prefabA, targetTile.transform.position, Quaternion.Euler (0, 0, 0)).GetComponent<Enemy>();
			break;
		case 1:
			enemy = GameObject.Instantiate (prefabB, targetTile.transform.position, Quaternion.Euler (0, 0, 0)).GetComponent<Enemy>();
			break;
		case 2:
			enemy = GameObject.Instantiate (prefabC, targetTile.transform.position, Quaternion.Euler (0, 0, 0)).GetComponent<Enemy>();
			break;
		}

		enemy.Init (levelManager, targetTile);
		return enemy;
	}

	public List<Enemy> SpawnInitEnemies(List<spawningInfo> spawnList)
	{
		List<Enemy> enemyList = new List<Enemy> ();
		foreach (spawningInfo sInfo in spawnList) {
			enemyList.Add (SpawnEnemy (sInfo.enemyType, sInfo.tile));
		}
		return enemyList;
	}
		

}
