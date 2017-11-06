using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPatrol : MonsterBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    float idleTimer;

    //=======================================
    //      Functions
    //=======================================
    protected override void Start()
    {
        base.Start();

        AddActiveAction("PatrolToNeighborCoroutine");
    }

    //---------------------------------------
    //      Action
    //---------------------------------------
    IEnumerator PatrolToNeighborCoroutine()
    {
        // Idle for random seconds
        idleTimer = Random.Range(owner.idleTimeMin, owner.idleTimeMax);
        while (idleTimer > 0)
        {
            //Debug.Log(idleTimer);
            if (owner.detectState != DetectState.Idle)
                break;

            idleTimer -= Time.deltaTime;
            yield return null;
        }

        // Patrol to a random neighbor
        if (owner.detectState == DetectState.Idle)
        {
            List<Tile> neighbors = MazeUTL.GetNeighborTilesWithoutWall(owner.CurrentTile);
            Tile targetTile = MazeUTL.GetRandomTileFromList(neighbors);
            owner.TryMoveToTile(targetTile);
            do
            {
                yield return new WaitForSeconds(0.01f);
            } while (owner.CurrentAction == ActionType.Walking);
            owner.StopWalkingAnim();
        }

        SetActionFinished("PatrolToNeighborCoroutine", true);
        yield return null;
    }
}
