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
	public List<Tile> FindShortestPath(Tile org, Tile target, int SearchRange)
	{
        List<List<Tile>> paths = FindPathsToTarget(org, target, SearchRange, new List<Tile>());
        if (paths.Count <= 0)
            return null;

        List<Tile> shortestPath = paths[0];

        foreach (List<Tile> path in paths)
        {
            if (path.Count < shortestPath.Count)
                shortestPath = path;
        }

        shortestPath.RemoveAt(0);

        return shortestPath;
    }

    List<List<Tile>> FindPathsToTarget(Tile org, Tile target, int SearchRange, List<Tile> currentPath)
    {
        List<Tile> newCurrentPath = new List<Tile>(currentPath);
        newCurrentPath.Add(org);

        List<List<Tile>> paths = new List<List<Tile>>();

        // If we reach the target, stop finding and return the paths
        if (org == target)
        {
            paths.Add(newCurrentPath);
            return paths;
        }

        // If reach the search range but not reach the target yet, stop finding and return null
        SearchRange--;
        if (SearchRange < 0)
            return null;

        // Get all possible continue finding directions
        List<int> continueDir = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            if (MazeUTL.WallOnDir(org, i))
                continue;

            if (newCurrentPath.Contains((MazeUTL.GetDirNeighborTile(org, i))))
                continue;

            continueDir.Add(i);
        }

        // If reach dead end but not reach the target yet, stop finding and return null
        if (continueDir.Count == 0)
            return null;

        // Recursive search for are possible directions
        foreach (int dir in continueDir)
        {
            List<List<Tile>> newPaths = FindPathsToTarget(MazeUTL.GetDirNeighborTile(org, dir), target, SearchRange, newCurrentPath);

            if (newPaths == null)
                continue;

            foreach (List<Tile> newPath in newPaths)
            {
                paths.Add(newPath);
            }
        }

        return paths;
    }


    /*
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
    */
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
