using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSearch : MonsterBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    const float searchSpeedMultiplier = 0.7f;
    Unit actionTarget;

    //=======================================
    //      Functions
    //=======================================
    protected override void Start()
    {
        base.Start();

        AddActiveAction("SearchAlertedTargetCoroutine");
    }

    protected override void ResetBehaviour()
    {
        actionTarget = owner.alertedTarget;
    }

    //---------------------------------------
    //      Action
    //---------------------------------------
    IEnumerator SearchAlertedTargetCoroutine()
    {
        Tile previousTile = owner.CurrentTile;

        // Smart search till the first intersection
        List<Tile> path = MazeUTL.GetShortestPath(owner.CurrentTile, actionTarget.CurrentTile, (owner.detectRange * 2));
        for (int i = 0; i < path.Count; i++)
        {
            previousTile = owner.CurrentTile;
            owner.TryMoveToTile(path[i]);
            do
            {
                yield return new WaitForSeconds(0.05f);
            } while (owner.CurrentAction == ActionType.Walking);

            if ((owner.detectState == DetectState.Alerted))
                break;

            if (MazeUTL.GetNeighborTilesWithoutWall(owner.CurrentTile).Count >= 3)
                break;
        }

        // Random search during Warning state
        while (owner.detectState == DetectState.Warning)
        {
            List<Tile> neighbors = MazeUTL.GetNeighborTilesWithoutWall(owner.CurrentTile);
            if (neighbors.Count > 1)
                neighbors.Remove(previousTile);

            previousTile = owner.CurrentTile;
            owner.TryMoveToTile(MazeUTL.GetRandomTileFromList(neighbors));
            do
            {
                yield return new WaitForSeconds(0.01f);
            } while (owner.CurrentAction == ActionType.Walking);
        }

        owner.StopWalkingAnim();

        SetActionFinished("SearchAlertedTargetCoroutine", true);
        yield return null;
    }
}
