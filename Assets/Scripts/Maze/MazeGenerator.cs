//============================== Class Definition ==============================
// 
// This is a helper class to generate the maze.
// Most functions here take LevelManager's instance as input and output.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    LevelManager level;

    List<Tile> TilesWithItem = new List<Tile>();

    // Custom maze object variables
    List<GameObject> customTileObjList = new List<GameObject>();
    List<TileItemSpawner> customTileItemSpawners = new List<TileItemSpawner>();
    public List<MonsterSpawner> customMonsterSpawnerList = new List<MonsterSpawner>();

    //=======================================
    //      Functions
    //=======================================
    void Awake()
	{
		level = LevelManager.instance;
	}

    public void GenerateMaze()
    {
        Maze newMaze = BuildMaze();

		level.maze = newMaze;
		GenerateDetectRegions(newMaze);
		GenerateMazeItems(newMaze);
    }

    #region Build Maze Layout
    // Public function that generates empty maze with wall layout
    Maze BuildMaze()
    {
        Maze maze;

		if (level.customMazeObject != null)
            maze = GenerateMaze_Custom();
        else
            maze = GenerateMaze_Random();

        return maze;
    }

    // Generate random maze using Kruskal's algorithm
    Maze GenerateMaze_Random()
    {
		if (!level.customMazeSize)
		{
			level.mazeWidth = Formula.CalculateMazeSideSize(level.mazeDifficulty);
			level.mazeLength = Formula.CalculateMazeSideSize(level.mazeDifficulty);
		}

		Maze maze = new Maze(level.mazeWidth, level.mazeLength);
        GameObject mazeObj = new GameObject() { name = "Maze" };
		MazeBlueprint mazeBP = new MazeBlueprint(level.mazeWidth, level.mazeLength);

		for (int i = 0; i < level.mazeWidth; i++)
        {
			for (int j = 0; j < level.mazeLength; j++)
            {
                maze.mazeTile[i, j] = GenerateTileWithBlueprint(i, j, mazeBP);
                maze.mazeTile[i, j].transform.parent = mazeObj.transform;
                maze.mazeTileList.Add(maze.mazeTile[i, j]);
            }
        }

        return maze;
    }

    // Generate maze using custom game object
    Maze GenerateMaze_Custom()
    {
		// Init object list for the custom maze
		InitCustomMazeObjList();

        int width = 0;
        int length = 0;
        foreach (GameObject obj in customTileObjList)
        {
            // Gather tile data from preset tile object
            int X = GetObjX(obj);
            width = width > (X + 1) ? width : (X + 1);
            int Z = GetObjZ(obj);
            length = length > (Z + 1) ? length : (Z + 1);
        }
        level.mazeWidth = width;
        level.mazeLength = length;

        Maze maze = new Maze(level.mazeWidth, level.mazeLength);

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
            maze.mazeTile[X, Z] = tile;
        }

		for (int i = 0; i < level.mazeWidth; i++)
        {
			for (int j = 0; j < level.mazeLength; j++)
            {
                maze.mazeTileList.Add(maze.mazeTile[i, j]);
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

		wallLayoutObj = level.mazeSetting.GetWallLayoutObj(wallLayout);
        Utilities.TryCatchError((wallLayoutObj == null), "'TileLayout" + wallLayout + "' has wrong setup in current Maze Setting.");

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
    public class MazeBlueprint
    {
        public int m_length = 10;
        public int m_width = 10;
        public bool[,] wall_v;  // X, Z
        public bool[,] wall_h;  // X, Z
        public int[,] cell;     // X, Z  cell is the id of each inclosed section in Kruskal's algorithm.
        public List<Vector2> tileInSquares;

        public MazeBlueprint(int width, int length)
        {
            // Setup maze blueprint with input size
            m_width = width;
            m_length = length;
            wall_v = new bool[m_width + 1, m_length];
            wall_h = new bool[m_width, m_length + 1];
            cell = new int[m_width, m_length];
            tileInSquares = new List<Vector2>();

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
        void GenerateBPLayout()
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

            AddSquares();
        }

        void ReplaceIDs(int[] repl)
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

        bool IDsAllZero(int[] repl)
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

        void AddSquares()
        {
            int squares2x2 = Formula.Calculate2x2SquareNum(LevelManager.instance.mazeDifficulty);
            int squares3x3 = Formula.Calculate3x3SquareNum(LevelManager.instance.mazeDifficulty);

            for (int i = 0; i < squares2x2; i++)
            {
                AddSquare(2);
            }

            for (int i = 0; i < squares3x3; i++)
            {
                AddSquare(3);
            }
        }

        void AddSquare(int size)
        {
            int x, z;

            do
            {
                x = Random.Range(0, (m_width - size));
                z = Random.Range(0, (m_length - size));

            } while ((tileInSquares.Contains(new Vector2(x, z))) || (tileInSquares.Contains(new Vector2(x + size - 1, z + size - 1))));

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if ((i + 1) < size )
                        wall_v[x + i + 1, z + j] = false;
                    if ((j + 1) < size)
                        wall_h[x + i , z + j + 1] = false;
                }
            }

            for (int i = (x - 1); i <= (x + size) ; i++)
            {
                for (int j = (z - 1); j <= (z + size); j++)
                {
                    tileInSquares.Add(new Vector2(i, j));
                }
            }
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

		foreach (Transform child in tile.transform)
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

    #region Generate Detect Regions
    void GenerateDetectRegions(Maze maze)
    {
        // Generate regions
		List<string> regionList = new List<string>();

        for (int i = 0; i < maze.mazeLength; i++)
        {
            for (int j = 0; j < maze.mazeWidth; j++)
            {
                TryUpdateLinearRegionForTile(0, maze.mazeTile[i, j], regionList, maze);
                TryUpdateLinearRegionForTile(1, maze.mazeTile[i, j], regionList, maze);
                TryUpdateRectRegionForTile(maze.mazeTile[i, j], regionList, maze);
            }
        }

        maze.detectRegions = regionList;

		// Assign regions to each tile
        foreach (Tile tile in maze.mazeTileList)
        {
            string tileAddress = MazeUTL.GetTileAddress(tile.X, tile.Z);
            foreach (string region in regionList)
            {
				if (MazeUTL.CheckRegionHasAddress(region, tileAddress))
                    tile.detectRegions.Add(region);
            }
        }
    }

    void TryUpdateLinearRegionForTile(int linearDir, Tile tile, List<string> regionList, Maze maze)
    {
        string region = GetLinearRegion(linearDir, tile, maze);

        if (IsUniqueRegion(region, regionList))
            regionList.Add(region);
    }

    void TryUpdateRectRegionForTile(Tile tile, List<string> regionList, Maze maze)
    {
        if ((tile.X + 1) >= maze.mazeWidth)
            return;

        string testRegion = GetLinearRegionConditions(0, tile, maze, 0, new List<int>() { 1 });
        if (IsOneTileRegion(testRegion))
            return;

        string region = testRegion;
        int regionHeight = testRegion.Length;

        for (int i = tile.X + 1; i < maze.mazeWidth; i++)
        {
            string newRegion = "";
            newRegion = GetLinearRegionConditions(0, maze.mazeTile[i, tile.Z], maze, regionHeight, new List<int>() { 3 });

            if (IsOneTileRegion(newRegion))
                break;

            if (newRegion.Length < regionHeight)
            {
                TryUpdateRectRegionForTileWithHeight(tile, regionList, maze, newRegion.Length);
                break;
            }   

            region += newRegion;
        }

        if (IsUniqueRegion(region, regionList))
            regionList.Add(region);
    }

    void TryUpdateRectRegionForTileWithHeight(Tile tile, List<string> regionList, Maze maze, int height)
    {
        string region = GetLinearRegionConditions(0, tile, maze, height, new List<int>() { 1 });
        if (region.Length != height)
            return;

        for (int i = tile.X + 1; i < maze.mazeWidth; i++)
        {
            string newRegion = "";
            newRegion = GetLinearRegionConditions(0, maze.mazeTile[i, tile.Z], maze, height, new List<int>() { 3 });

            if (IsOneTileRegion(newRegion))
                break;

            if (newRegion.Length != height)
                break;

            region += newRegion;
        }

        if (IsUniqueRegion(region, regionList))
            regionList.Add(region);
    }

    string GetLinearRegion(int linearDir, Tile tile, Maze maze)
    {
        return GetLinearRegionConditions(linearDir, tile, maze, 0, new List<int>());
    }

    string GetLinearRegionConditions(int linearDir, Tile tile, Maze maze, int height, List<int> wallsShouldntContain)
    {
        Utilities.TryCatchError(((linearDir < 0) || (linearDir > 1)), "Input parameter 'linearDir' can only be North or East");
        if (wallsShouldntContain.Contains(linearDir))
            wallsShouldntContain.Remove(linearDir);

        string region = "";
        int startID;
        int length;
        if (linearDir == 0)
        {
            startID = tile.Z;
            length = maze.mazeLength;
        }
        else
        {
            startID = tile.X;
            length = maze.mazeWidth;
        }

        for (int i = startID; i < length; i++)
        {
            Tile currTile;
            bool containWrongWalls = false;

            // Get current tile based on linear direction
            if (linearDir == 0)
                currTile = maze.mazeTile[tile.X, i];
            else
                currTile = maze.mazeTile[i, tile.Z];

            // Check if this tile contains walls we don't want
            foreach (int wall in wallsShouldntContain)
            {
                if (MazeUTL.WallOnDir(currTile, wall))
                {
                    containWrongWalls = true;
                    break;
                }
            }
            if (containWrongWalls)
                break;

            // Pass check, add the tile to region
            region += MazeUTL.GetTileAddress(currTile.X, currTile.Z);

            // After adding, check if we can keep going to the next tile
            if (region.Length == height)
                break;
            if (MazeUTL.WallOnDir(currTile, linearDir))
                break;
        }

        return region;
    }

    bool IsOneTileRegion(string region)
    {
        string[] addressList = region.Split('/');
        return (addressList.Length <= 2);
    }

    bool IsUniqueRegion(string region, List<string> regionList)
    {
        if (IsOneTileRegion(region))
            return false;

        foreach (string existRegion in regionList)
        {
            if (existRegion.Contains(region))
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Generate Maze Items
    void GenerateMazeItems(Maze maze)
    {
		if (level.customMazeObject != null)
		{
			GenerateCustomTileItems (maze);

			if (level.tileStart == null)
				GenerateStartPoint_Random(maze);

			if (level.tileObjective == null)
				GenerateObjective_Random(maze);
		}
		else
		{
			GenerateStartPoint_Random(maze);
			GenerateObjective_Random(maze);
			GenerateHealthPack_Random(maze);
            GenerateSlimeElement_Random(maze);
            GenerateBodyPartChest_Random(maze);
			GenerateCompass_Random(maze);
		}
    }

    #region Custom Maze Items
    void GenerateCustomTileItems(Maze maze)
    {
        // Custom Tile Item from spawner
        foreach (TileItemSpawner spawner in customTileItemSpawners)
        {
            TileItem item = spawner.GetTileItem();

            Tile tile = GetObjLocatedTile(maze, spawner.gameObject);
            tile.SpawnTileItem(item.gameObject);

            TilesWithItem.Add(tile);

            // Custom item setup
            CustomTileItemSetup(item.itemType, item, tile);
        }
    }

    void CustomTileItemSetup(ItemType itemType, TileItem item, Tile tile)
    {
        if (itemType == ItemType.StartPoint)
        {
            level.tileStart = tile;
        }
        else if (itemType == ItemType.Objective)
        {
            level.tileObjective = tile;
        }
        else if (itemType == ItemType.BodyPart)
        {
            if (item.bodyPart == null)
            {
                item.bodyPart = level.mazeSetting.bodyParts[Random.Range(0, level.mazeSetting.bodyParts.Count)];
            }
        }
    }
    #endregion

    #region Random Maze Items
    void GenerateStartPoint_Random(Maze maze)
    {
        Tile tile = MazeUTL.GetRandomTileFromList(maze.mazeTileList);

        tile.SpawnTileItem(level.startPointPrefab);
        level.tileStart = tile;
        TilesWithItem.Add(tile);
    }

    void GenerateObjective_Random(Maze maze)
    {
        // Make sure the objective is at least half map aways from the start point. Also, make it spawn at C shape wall layout. 
        List<Tile> orgs = new List<Tile>();
        orgs.Add(level.tileStart);
        List<Tile> tileList = MazeUTL.UpdateTileListOutOfRange(maze.mazeTileList, orgs, Formula.CalculateObjectiveLeastDistance(LevelManager.instance.mazeDifficulty));
		tileList = MazeUTL.UpdateTileListWithDesiredWallLayout(tileList, WallLayout.C);
        Tile tile = MazeUTL.GetRandomTileFromList(tileList);

        tile.SpawnTileItem(level.objectivePrefab);
        level.tileObjective = tile;
        TilesWithItem.Add(tile);
    }

    void GenerateHealthPack_Random(Maze maze)
    {
        int numPacks = Formula.CalculateHealthPackNum(level.mazeDifficulty);

		List<Tile> tiles = GetItemSpawnRandomTiles(numPacks, Formula.CalculateItemLeastDistance(LevelManager.instance.mazeDifficulty), new List<Tile>(), maze.mazeTileList, TilesWithItem);
		Utilities.TryCatchError((tiles.Count < numPacks), "Can't find enough tiles to spawn Health Packs. Please check the range condition.");

		for (int i = 0; i < numPacks; i++)
        {
            tiles[i].SpawnTileItem(level.HealthPackPrefab);
            TilesWithItem.Add(tiles[i]);
        }
    }

    void GenerateSlimeElement_Random(Maze maze)
    {
        // Calculate the number of items needed to spawn for this maze
        int numElements = Formula.CalculateSlimeElementNum(level.mazeDifficulty);

        List<Tile> tileList = MazeUTL.UpdateTileListWithDesiredWallLayout(maze.mazeTileList, WallLayout.C);
        List<Tile> tiles = GetItemSpawnRandomTiles(numElements, Formula.CalculateItemLeastDistance(LevelManager.instance.mazeDifficulty), new List<Tile>(), tileList, TilesWithItem);
        Utilities.TryCatchError((tiles.Count < numElements), "Can't find enough tiles to spawn Body Part Chests. Please check the range condition.");

        TileItem spawningElement = level.mazeSetting.SlimeElements[Random.Range(0, level.mazeSetting.SlimeElements.Count)];

        for (int i = 0; i < numElements; i++)
        {
            TileItem item = tiles[i].SpawnTileItem(spawningElement.gameObject);
            TilesWithItem.Add(tiles[i]);
        }
    }

    void GenerateBodyPartChest_Random(Maze maze)
    {
        // Calculate the number of items needed to spawn for this maze
		int numChests = Formula.CalculateBodyPartChestNum(level.mazeDifficulty);
		numChests = numChests < level.mazeSetting.bodyParts.Count ? numChests : level.mazeSetting.bodyParts.Count;

        List<Tile> tileList = MazeUTL.UpdateTileListWithDesiredWallLayout(maze.mazeTileList, WallLayout.C);
		List <Tile> tiles = GetItemSpawnRandomTiles(numChests, Formula.CalculateItemLeastDistance(LevelManager.instance.mazeDifficulty), new List<Tile>(), tileList, TilesWithItem);
		Utilities.TryCatchError((tiles.Count < numChests), "Can't find enough tiles to spawn Body Part Chests. Please check the range condition.");
		List<BodyPart> partList = GetBodyPartList(numChests);

        for (int i = 0; i < numChests; i++)
        {
			TileItem item = tiles[i].SpawnTileItem(level.BodyPartChestPrefab);
            TilesWithItem.Add(tiles[i]);
            item.bodyPart = partList[i];
        }
    }

	void GenerateCompass_Random(Maze maze)
	{
		int numCompass = Formula.CalculateCompassNum(level.mazeDifficulty);

		List<Tile> tileList = MazeUTL.UpdateTileListWithDesiredWallLayout(maze.mazeTileList, WallLayout.C);
		List<Tile> tiles = GetItemSpawnRandomTiles(numCompass, Formula.CalculateItemLeastDistance(LevelManager.instance.mazeDifficulty), new List<Tile>(), tileList, TilesWithItem);
		Utilities.TryCatchError((tiles.Count < numCompass), "Can't find enough tiles to spawn Compasses. Please check the range condition.");

		for (int i = 0; i < numCompass; i++)
		{
			tiles[i].SpawnTileItem(level.CompassPrefab);
			TilesWithItem.Add(tiles[i]);
		}
	}
    #endregion

    #region Misc functions for generating maze
    void InitCustomMazeObjList()
    {
        foreach (Transform child in level.customMazeObject.transform)
        {
            TileItem item = child.gameObject.GetComponent<TileItem>();
            TileItemSpawner tileItemSpawner = child.gameObject.GetComponent<TileItemSpawner>();
            MonsterSpawner monsterSpawner = child.gameObject.GetComponent<MonsterSpawner>();

            // Add all the layout object from "Maze" parent object
            if (child.gameObject.name == "Maze")
            {
                foreach (Transform layout in child.transform)
                {
                    customTileObjList.Add(layout.gameObject);
                }
            }
            // Add spawner to list if the spawner script is attached
            else if (tileItemSpawner != null)
            {
                customTileItemSpawners.Add(tileItemSpawner);
            }
            else if (monsterSpawner != null)
            {
                customMonsterSpawnerList.Add(monsterSpawner);
            }
        }
    }

    Tile GetObjLocatedTile(Maze maze, GameObject obj)
    {
        return maze.mazeTile[GetObjX(obj), GetObjZ(obj)];
    }

    List<Tile> GetItemSpawnRandomTiles(int numItems, int range, List<Tile> tileList, List<Tile> tilesLeft, List<Tile> exclusiveTiles)
    {
        if (numItems <= 0)
            return tileList;

        tilesLeft = MazeUTL.UpdateTileListOutOfRange(tilesLeft, exclusiveTiles, range);
        Tile tile = MazeUTL.GetRandomTileFromList(tilesLeft);
        tileList.Add(tile);

		List<Tile> newExclusiveTiles = new List<Tile>();
		foreach(Tile t in exclusiveTiles)
		{
			newExclusiveTiles.Add(t);
		}
		newExclusiveTiles.Add(tile);

        numItems--;

		return GetItemSpawnRandomTiles(numItems, range, tileList, tilesLeft, newExclusiveTiles);
    }

    List<BodyPart> GetBodyPartList(int numItems)
    {
        List<BodyPart> newList = new List<BodyPart>();

        int[] randomNumbers = Utilities.GetRandomUniqueNumbers(numItems, level.mazeSetting.bodyParts.Count);

        for (int i = 0; i < numItems; i++)
        {
            newList.Add(level.mazeSetting.bodyParts[randomNumbers[i]]);
        }

        return newList;
    }
    #endregion

    #endregion

    #region Misc.
    int GetObjX(GameObject obj)
    {
        return Mathf.FloorToInt(obj.transform.position.x / 10);
    }

    int GetObjZ(GameObject obj)
    {
        return Mathf.FloorToInt(obj.transform.position.z / 10);
    }
    #endregion
}