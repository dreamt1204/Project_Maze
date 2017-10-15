using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator {
    //=======================================
    //      Variables
    //=======================================
    LevelManager levelManager;

    // Maze
    int m_difficulty;
    MazeSetting m_setting;

    // Custom Maze Option
    bool m_customMazeSize;
    int m_width;
    int m_length;
    GameObject m_customMazeObj;
    bool m_customGameModePosition;
    bool m_customBodyPartChestPosition;

    // Custom maze object variables
    List<GameObject> customTileObjList;
    GameObject customStartPointItem;
    GameObject customObjectiveItem;
    List<GameObject> customBodyPartChestList;

    // Game mode
    GameObject startPrefab;
    GameObject objPrefab;
    GameObject BodyPartChestPrefab;

    //=======================================
    //      Struct
    //=======================================
    public MazeGenerator()
    {
        levelManager = LevelManager.GetLevelManager();

        // Maze
        m_difficulty = levelManager.mazeDifficulty;
        m_setting = levelManager.mazeSetting;

        // Custom Maze Option
        m_customMazeSize = levelManager.customMazeSize;
        m_width = levelManager.mazeWidth;
        m_length = levelManager.mazeLength;
        m_customMazeObj = levelManager.customMazeObject;
        m_customGameModePosition = levelManager.customGameModePosition;
        m_customBodyPartChestPosition = levelManager.customBodyPartChestPosition;

        // Prefabs
        startPrefab = levelManager.startPointPrefab;
        objPrefab = levelManager.objectivePrefab;
        BodyPartChestPrefab = levelManager.BodyPartChestPrefab;
    }

    //=======================================
    //      Functions
    //======================================= 
    public Maze GenerateMaze()
    {
        Maze maze = BuildMaze();
        GenerateGameModeObjects(maze);
        GenerateMazeBodyPartChest(maze);

        return maze;
    }

    #region Build maze layout
    // Public function that generates empty maze with wall layout
    Maze BuildMaze()
    {
        Maze maze;

        if (m_customMazeObj != null)
            maze = GenerateMaze_Custom();
        else
            maze = GenerateMaze_Random();

        return maze;
    }

    // Generate random maze using Kruskal's algorithm
    Maze GenerateMaze_Random()
    {
        if (!m_customMazeSize)
            m_width = m_length = Formula.CalculateMazeSideSize(m_difficulty);

        Maze maze = new Maze(m_width, m_length);
        GameObject mazeObj = new GameObject() { name = "Maze" };
        MazeBlueprint mazeBP = new MazeBlueprint(m_width, m_length);

        for (int i = 0; i < m_width; i++)
        {
            for (int j = 0; j < m_length; j++)
            {
                maze.tile[i, j] = GenerateTileWithBlueprint(i, j, mazeBP);
                maze.tile[i, j].transform.parent = mazeObj.transform;
                maze.tileList.Add(maze.tile[i, j]);
            }
        }

        return maze;
    }

    // Generate maze using custom game object
    Maze GenerateMaze_Custom()
    {
        // Calculate the maze size then init a new maze
        Maze maze;
        int mazeWidth = 0;
        int mazeLength = 0;

        UpdateCustomMazeObjList();

        foreach (GameObject obj in customTileObjList)
        {
            // Gather tile data from preset tile object
            int X = GetObjX(obj);
            mazeWidth = mazeWidth > (X + 1) ? mazeWidth : (X + 1);
            int Z = GetObjZ(obj);
            mazeLength = mazeLength > (Z + 1) ? mazeLength : (Z + 1);
        }

        maze = new Maze(mazeWidth, mazeLength);

        // Spawn and init each preset tile from the custom maze object
        GameObject mazeObj = new GameObject() { name = "Maze" };
        foreach (GameObject obj in customTileObjList)
        {
            // Gather tile data from preset tile object
            int X = GetObjX(obj);
            int Z = GetObjZ(obj);
            int rotCount = Mathf.FloorToInt(obj.transform.eulerAngles.y / 90);
            rotCount = rotCount < 4 ? rotCount : (rotCount - 4);
            WallLayout wallLayout = GetWallLayoutFromObj(obj);
            bool[] wall = GetWallInfoFromObj(obj, rotCount);

            // Spawn tile object
            GameObject tileObj = GameObject.Instantiate(obj, new Vector3(X * 10, -0, Z * 10), Quaternion.Euler(0, 90 * rotCount, 0));
            tileObj.name = "Tile [" + X + "]" + "[" + Z + "] " + "(" + wallLayout + ")";
            tileObj.AddComponent<Tile>();

            // Generate Tile class data
            Tile tile = tileObj.GetComponent<Tile>();
            tile.X = X;
            tile.Z = Z;
            tile.wall = wall;
            tile.wallLayout = wallLayout;
            AssignWallFloorObjToTile(tile, wallLayout, rotCount);

            // Group tile
            tile.transform.parent = mazeObj.transform;
            maze.tile[X, Z] = tile;
        }

        for (int i = 0; i < mazeWidth; i++)
        {
            for (int j = 0; j < mazeLength; j++)
            {
                maze.tileList.Add(maze.tile[i, j]);
            }
        }

        return maze;
    }

    #region Misc functions for generating maze
    // Generate script tile based on maze blueprint
    Tile GenerateTileWithBlueprint(int X, int Z, MazeBlueprint mazeBP)
    {
		bool[] wall = new bool[4];
		int nbWalls = 0;
		WallLayout wallLayout;
		GameObject wallLayoutObj;
		int rotCount = 0;

        // Get wall info from maze blueprint
		wall[0] = mazeBP.wall_h [X, Z + 1];	// N
		wall[1] = mazeBP.wall_v [X + 1, Z];	// E
		wall[2] = mazeBP.wall_h [X, Z];		// S
		wall[3] = mazeBP.wall_v [X, Z];        // W

        // Get number of walls
		for (int i = 0; i < wall.Length; i++)
		{
			if (wall[i])
				nbWalls++;
		}

        // Get wall layout, then setup object
		if ((wall [0] != wall [1]) && (wall [0] == wall [2]) && (wall [1] == wall [3]))
			wallLayout = WallLayout.II;
		else
			wallLayout = (WallLayout)nbWalls;

        wallLayoutObj = m_setting.GetWallLayoutObj(wallLayout);

        // Get rotation count for based on wall layout so we can rotate the wall layout later.
        rotCount = GetLayoutRotationCount(wall, wallLayout);

        // Spawn tile object
        GameObject tileObj = GameObject.Instantiate (wallLayoutObj, new Vector3 (X * 10, -0, Z * 10), Quaternion.Euler (0, 90 * rotCount, 0));
        tileObj.name = "Tile [" + X + "]" + "[" + Z + "] " + "(" + wallLayout + ")";
        tileObj.AddComponent<Tile>();

        // Generate Tile class data
        Tile tile = tileObj.GetComponent<Tile>();
        tile.X = X;
        tile.Z = Z;
		tile.wall = wall;
        tile.wallLayout = wallLayout;
        AssignWallFloorObjToTile(tile, wallLayout, rotCount);

        return tile;
    }

    // A maze blueprint class with constructor.
    // Once taking maze size as input, it generates wall arrays using Kruskal's algorithm.
    // The wall array can be used to build maze tile later.
    class MazeBlueprint
    {
        public int m_length = 10;
        public int m_width = 10;
        public bool[,] wall_v;  // X, Z
        public bool[,] wall_h;  // X, Z
        public int[,] cell;     // X, Z  cell is the id of each inclosed section in Kruskal's algorithm.

        public MazeBlueprint(int width, int length)
        {
            // Setup maze blueprint with input size
            m_width = width;
            m_length = length;
            wall_v = new bool[m_width + 1, m_length];
            wall_h = new bool[m_width, m_length + 1];
            cell = new int[m_width, m_length];
            for (int i = 0; i < wall_v.GetLength(0); i++)
            {
                for (int j = 0; j < wall_v.GetLength(1); j++)
                    wall_v[i, j] = true;
            }
            for (int i = 0; i < wall_h.GetLength(0); i++)
            {
                for (int j = 0; j < wall_h.GetLength(1); j++)
                    wall_h[i, j] = true;
            }
            for (int i = 0; i < m_width; i++)
            {
                for (int j = 0; j < m_length; j++)
                {
                    cell[i, j] = m_width * j + i;
                }
            }

            // Run Kruskal's algorithm to remove walls and get a perfect maze layout
            GenerateBPLayout();
        }

        // Generate maze blueprint using kruskal's algorithm
        private void GenerateBPLayout()
        {
            while (true)
            {
                int[] repl;

                if (Random.Range(0, 2) >= 1)
                {
                    int x = Random.Range(1, wall_v.GetLength(0) - 1);
                    int z = Random.Range(0, wall_v.GetLength(1));
                    if (cell[x, z] == cell[x - 1, z])
                        continue;
                    repl = new int[] { cell[x, z], cell[x - 1, z] };
                    wall_v[x, z] = false;
                }
                else
                {
                    int x = Random.Range(0, wall_h.GetLength(0));
                    int z = Random.Range(1, wall_h.GetLength(1) - 1);
                    if (cell[x, z] == cell[x, z - 1])
                        continue;
                    repl = new int[] { cell[x, z], cell[x, z - 1] };
                    wall_h[x, z] = false;
                }
                System.Array.Sort(repl);
                ReplaceIDs(repl);
                if (IDsAllZero(repl) == true)
                    break;
            }
        }

        private void ReplaceIDs(int[] repl)
        {
            for (int w = 0; w < m_width; w++)
            {
                for (int l = 0; l < m_length; l++)
                {
                    if (cell[w, l] == repl[1])
                        cell[w, l] = repl[0];
                }
            }
        }

        private bool IDsAllZero(int[] repl)
        {
            if (repl[0] != 0 && repl[1] != 0)
                return false;
            for (int w = 0; w < m_width; w++)
            {
                for (int l = 0; l < m_length; l++)
                {
                    if (cell[w, l] != 0)
                        return false;
                }
            }
            return true;
        }
    }

    // Instead of creating multiple type of wall layout, we rotate existing object to match the wall layout.
    // This function calculate the rotCount that can be used later to spawn the wall layout.
    // ex: Quaternion.Euler (0, 90 * rotCount, 0))
    int GetLayoutRotationCount(bool[] walls, WallLayout wallLayout)
	{
		int count = 0;

		if (wallLayout == WallLayout.II)
		{
			int rnd = Random.Range (0, 2);
			if (walls [0] == true)
				count = 2 * rnd;
			else
				count = 2 * rnd + 1;

			return count;
		}

		for (int i = 0; i < walls.Length; i++) 
		{
			int wallID = i;
			bool match = false;
			for (int j = 0; j < (int)wallLayout; j++) 
			{
				if (!walls [wallID])
					break;

				if (j == ((int)wallLayout - 1))
					match = true;

				wallID = wallID + 1 < walls.Length ? (wallID + 1) : (wallID + 1 - walls.Length);
			}

			if (match)
				count = i;
		}

		return count;
	}

    // Store wall object in tile class
    void AssignWallFloorObjToTile(Tile tile, WallLayout wallLayout, int rotCount)
    {
		if (wallLayout == WallLayout.O)
			return;

		// Get wall objects / or assign floor object
		Dictionary<int,GameObject> wall_obj_list = new Dictionary<int,GameObject>();

		foreach (Transform child in tile.gameObject.transform)
        {
            if (child.name.Contains("Wall_"))
            {
                int index = int.Parse(child.name.Substring(child.name.IndexOf('_') + 1)) - 1;
                wall_obj_list.Add(index, child.gameObject);
            }
            else if (child.name.Contains("Floor"))
            {
                tile.floor_obj = child.gameObject;
            }
        }

        // Assign wall objects to Tile class based on wall layout and rotation
        int wallID = rotCount;
		tile.wall_obj [wallID] = wall_obj_list [0];

		if (wallLayout == WallLayout.II)
		{
			wallID = wallID + 2 < 4 ? (wallID + 2) : (wallID + 2 - 4);
			tile.wall_obj [wallID] = wall_obj_list [1];
		}
		else
		{
			for (int i = 1; i < wall_obj_list.Count; i++)
			{
				wallID = wallID + 1 < 4 ? (wallID + 1) : (wallID + 1 - 4);
				tile.wall_obj [wallID] = wall_obj_list [i];
			}
		}
    }

    int GetWallCountFromObj(GameObject tileObj)
    {
        int count = 0;

        foreach (Transform child in tileObj.transform)
        {
            if (child.name.Contains("Wall_"))
            {
                count++;
            }
        }

        return count;
    }

    WallLayout GetWallLayoutFromObj(GameObject tileObj)
    {
        WallLayout layout = WallLayout.O;

        int wallCount = GetWallCountFromObj(tileObj);

        if (wallCount == 3)
        {
            layout = WallLayout.C;
        }
        else if (wallCount == 2)
        {
            int wallAngle_1 = Mathf.FloorToInt(tileObj.transform.Find("Wall_1").transform.eulerAngles.y / 90);
            int wallAngle_2 = Mathf.FloorToInt(tileObj.transform.Find("Wall_2").transform.eulerAngles.y / 90);
            wallAngle_1 = wallAngle_1 > 180 ? (wallAngle_1 - 180) : wallAngle_1;
            wallAngle_2 = wallAngle_2 > 180 ? (wallAngle_2 - 180) : wallAngle_2;

            if (wallAngle_1 == wallAngle_2)
            {
                layout = WallLayout.II;
            }
            else
            {
                layout = WallLayout.L;
            }
        }
        else if (wallCount == 1)
        {
            layout = WallLayout.I;
        }

        return layout;
    }

    bool[] GetWallInfoFromObj(GameObject tileObj, int rotCount)
    {
        bool[] wall = new bool[4];

        int wallCount = GetWallCountFromObj(tileObj);
        WallLayout layout = GetWallLayoutFromObj(tileObj);

        if (layout == WallLayout.II)
        {
            wall[rotCount] = true;
            rotCount = rotCount + 2 < 4 ? (rotCount + 2) : (rotCount + 2 - 4);
            wall[rotCount] = true;
        }
        else
        {
            for (int i = 0; i < wallCount; i++)
            {
                wall[rotCount] = true;
                rotCount = rotCount + 1 < 4 ? (rotCount + 1) : (rotCount + 1 - 4);
            }
        }

        return wall;
    }
    #endregion

    #endregion

    #region Spawn Game Objects
    public void GenerateGameModeObjects(Maze maze)
    {
        if (m_customGameModePosition)
            GenerateGameModeObjects_Custom(maze);
        else
            GenerateGameModeObjects_Random(maze);
    }

    public void GenerateGameModeObjects_Random(Maze maze)
    {
        Tile tileStart, tileObj;
        tileStart = Maze.GetRandomTileFromList(maze.tileList);

        // Make sure the objective is at least half map aways from the start point. Also, make it spawn at C shape wall layout. 
        List<Tile> orgs = new List<Tile>();
        orgs.Add(tileStart);
        List<Tile> tileList = Maze.UpdateTileListOutOfRange(maze.tileList, orgs, Formula.CalculateObjectiveLeastDistance());
        tileList = Maze.UpdateTileListWithDesiredWallLayout(tileList, WallLayout.C);
        tileObj = Maze.GetRandomTileFromList(tileList);

        tileStart.SpawnTileItem(startPrefab);
        tileObj.SpawnTileItem(objPrefab);

        levelManager.tileStart = tileStart;
        levelManager.tileObjective = tileObj;
    }

    public void GenerateGameModeObjects_Custom(Maze maze)
    {
        Tile tileStart = GetObjLocatedTile(maze, customStartPointItem);
        Tile tileObjective = GetObjLocatedTile(maze, customObjectiveItem);

        tileStart.SpawnTileItem(customStartPointItem);
        tileObjective.SpawnTileItem(customObjectiveItem);

        levelManager.tileStart = tileStart;
        levelManager.tileObjective = tileObjective;
    }
    #endregion

    #region Spawn Maze Items (BodyPartChest, Pickups...etc.)
    public void GenerateMazeBodyPartChest(Maze maze)
    {
        if (m_customBodyPartChestPosition)
            GenerateMazeBodyPartChest_Custom(maze);
        else
            GenerateMazeBodyPartChest_Random(maze);
    }

    public void GenerateMazeBodyPartChest_Random(Maze maze)
    {
        // Calculate the number of items needed to spawn for this maze
        int numChests = Formula.CalculateBodyPartChestNum(m_difficulty);
        numChests = numChests < m_setting.bodyParts.Count ? numChests : m_setting.bodyParts.Count;

        List <Tile> tiles = GetItemSpawnTiles(maze, numChests);
        List<BodyPart> partList = GetBodyPartList(numChests);

        for (int i = 0; i < numChests; i++)
        {
            TileItem item = tiles[i].SpawnTileItem(BodyPartChestPrefab);
            item.bodyPart = partList[i];
        }
    }

    public void GenerateMazeBodyPartChest_Custom(Maze maze)
    {
        foreach (GameObject obj in customBodyPartChestList)
        {
            Tile tile = GetObjLocatedTile(maze, obj);
            tile.SpawnTileItem(obj);
        }
    }

    public List<Tile> GetItemSpawnTiles(Maze maze, int numChests)
    {
        List<Tile> orgs = new List<Tile>();
        orgs.Add(levelManager.tileStart);
        orgs.Add(levelManager.tileObjective);

        List<Tile> exclusiveList = new List<Tile>();
        exclusiveList.Add(levelManager.tileStart);
        exclusiveList.Add(levelManager.tileObjective);

        List<Tile> tileList = Maze.UpdateTileListOutOfRange(maze.tileList, orgs, Formula.CalculateBodyPartChestLeastDistance());
        tileList = Maze.UpdateTileListWithExclusiveList(tileList, exclusiveList);
        tileList = Maze.UpdateTileListWithDesiredWallLayout(tileList, WallLayout.C);

        List<Tile> newList = new List<Tile>();
        int[] randomNumbers = Utilities.GetRandomUniqueNumbers(numChests, tileList.Count);
        for (int i = 0; i < numChests; i++)
        {
            newList.Add(tileList[randomNumbers[i]]);
        }

        return newList;
    }
    
    public List<BodyPart> GetBodyPartList(int numItems)
    {
        List<BodyPart> newList = new List<BodyPart>();

        int[] randomNumbers = Utilities.GetRandomUniqueNumbers(numItems, m_setting.bodyParts.Count);

        for (int i = 0; i < numItems; i++)
        {
            newList.Add(m_setting.bodyParts[randomNumbers[i]]);
        }

        return newList;
    }
    #endregion

    #region Misc.
    void UpdateCustomMazeObjList()
    {
        customTileObjList = new List<GameObject>();
        customBodyPartChestList = new List<GameObject>();

        foreach (Transform child in m_customMazeObj.transform)
        {
            TileItem item = child.gameObject.GetComponent<TileItem>();

            if (item == null)
            {
                customTileObjList.Add(child.gameObject);
            }
            else
            {
                if (item.itemType == ItemType.StartPoint)
                    customStartPointItem = child.gameObject;

                if (item.itemType == ItemType.Objective)
                    customObjectiveItem = child.gameObject;

                if (item.itemType == ItemType.BodyPart)
                    if (item.bodyPart != null)
                        customBodyPartChestList.Add(child.gameObject);
            }
        }
    }

    int GetObjX(GameObject obj)
    {
        return Mathf.FloorToInt(obj.transform.position.x / 10);
    }

    int GetObjZ(GameObject obj)
    {
        return Mathf.FloorToInt(obj.transform.position.z / 10);
    }

    Tile GetObjLocatedTile(Maze maze, GameObject obj)
    {
        return maze.tile[GetObjX(obj), GetObjZ(obj)];
    }
    #endregion
}