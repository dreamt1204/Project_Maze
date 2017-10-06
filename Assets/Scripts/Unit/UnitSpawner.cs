using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    private LevelManager levelManager;

    [Header("Player Character")]
    public GameObject playerCharacterPrefab;

    //=======================================
    //      Functions
    //=======================================
    void Awake()
    {
        levelManager = GetComponent<LevelManager>();
    }

    public PlayerCharacter SpawnStartingCharacter(Tile targetTile)
    {
        PlayerCharacter character = SpawnPlayerCharacter(playerCharacterPrefab, targetTile);

        return character;
    }

    public PlayerCharacter SpawnPlayerCharacter(GameObject prefab, Tile targetTile)
    {
        PlayerCharacter character = Instantiate(prefab, targetTile.transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<PlayerCharacter>();
        character.Init(levelManager, targetTile);

        return character;
    }
}
