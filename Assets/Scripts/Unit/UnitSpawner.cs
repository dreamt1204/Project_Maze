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
		unit.Init(LevelManager.instance, targetTile);

        return unit;
    }

    public static PlayerCharacter SpawnPlayerCharacter(Tile targetTile)
    {
		return (PlayerCharacter)SpawnUnit(LevelManager.instance.playerCharacterPrefab, targetTile);
    }
}
