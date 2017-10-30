using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehaviour : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	[HideInInspector] public Monster owner;

    public bool isPassiveBehavior = false;
    public DetectingState detectingStateType = DetectingState.Alerted;
    public int behaviourPriority;
    [HideInInspector] public bool isCoolingDown;

    //=======================================
    //      Functions
    //=======================================
    public void StartActiveBehaviour()
    {
		ExecuteActiveBehaviour ();
		ClearBehaviour ();
    }

	protected virtual void ExecuteActiveBehaviour()
	{

	}

	public void StartPassiveBehaviour()
	{

	}

	void ClearBehaviour()
	{
		owner.currentBehaviour = null;
	}

	//---------------------------------------
	//      Path finding
	//---------------------------------------
	public void TryReachTargetTile(Tile targetTile)
	{
		List<Tile> path = new List<Tile>();	// WOM

		for (int i = 0; i < path.Count; i++)
		{
			owner.TryMoveToTile (path [i]);

			while (true)
			{
				yield return null;
			}
		}
	}

	List<Tile> FindShortestPath(Tile startTile, Tile endTile, int SearchRange)
	{
		
	}



    public static List<Tile> FindShortestPath(Tile startTile, Tile endTile, int maxSearchSteps, string pathSelectionMethod)
    {
        // Compile all possible paths from start to end, within maxSearchSteps. 
        // Use isSimpleDirect to improve efficiency

        List<List<Tile>> paths = new List<List<Tile>>();
        List<int> permittedDir;

        List<Tile> pathSoFar = new List<Tile>();
        pathSoFar.Add(startTile);
        HandPathToNeighbors(startTile, pathSoFar, endTile, paths, permittedDir, maxSearchSteps);

        switch (pathSelectionMethod)
        {
            case "Random":
                break;
            case "Shortest_Random":
                int[] pathLength = new int[paths.Count];
                Debug.Log("Number of paths initial = " + paths.Count);
                for (int i = 0; i < paths.Count; i++)
                {
                    pathLength[i] = paths[i].Count;
                    Debug.Log("Path #" + (i + 1) + " length = " + pathLength[i]);
                }

                break;
            default:
                Debug.LogError("invalid argument for pathSelectionMethod");
                break;
        }

		// null output means that no paths is found given the filter
		if (paths.Count == 0)
		{
			return null; 
		}
        else
		{
            List<Tile> selectedPath = paths[Random.Range(0, paths.Count)];
            selectedPath.RemoveAt(0); // Remove first tile (startTile) for convention
            return selectedPath;
        }
    }

    // Recursive Helper
    public static void HandPathToNeighbors(Tile source, List<Tile> pathSofar, Tile endTile, List<List<Tile>> paths, List<int> permittedDir, int searchStepsLeft)
    {
        searchStepsLeft--;
        if (searchStepsLeft < 0)
            return;

        List<Tile> neighborTilesToContinue = new List<Tile>();

        for (int i = 0; i < permittedDir.Count; i++)
        { // Simple Direction Filter
            Tile t = MazeUTL.GetDirNeighborTile(source, permittedDir[i]);
            if (t != null)
            { // Edge Filter
                if (!MazeUTL.WallBetweenNeighborTiles(source, t))
                { // Wall Filter
                    if (!pathSofar.Contains(t))
                    { // Tile_has_already_been_in_path Filter
                        neighborTilesToContinue.Add(t);
                    }
                }
            }
        }

        for (int i = 0; i < neighborTilesToContinue.Count; i++)
        {
            Tile t = neighborTilesToContinue[i];
            List<Tile> path = new List<Tile>(pathSofar); // Clone pathSoFar
            path.Add(t);
            if (t == endTile)
            {
                paths.Add(path);
                return;
            }
            else {
                HandPathToNeighbors(t, path, endTile, paths, permittedDir, searchStepsLeft);
            }
        }

    }
}
