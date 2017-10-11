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
}
