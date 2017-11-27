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
    Maze currentMaze;

    const int roomDistance = 2;

    List<Tile> TilesWithItem = new List<Tile>();

    List<PossibleItemSpawnPoint> possibleItemSpawnPointList = new List<PossibleItemSpawnPoint>();

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
        level.maze = BuildMaze();

        UpdateDeadEndList();
        GenerateDetectRegions();
		GenerateMazeItems();
    }

    #region Build Maze Layout
    // Public function that generates empty maze with wall layout
    Maze BuildMaze()
    {
		if (level.customMazeObject != null)
            currentMaze = GenerateMaze_Custom();
        else
            currentMaze = GenerateMaze_Random();

        return currentMaze;
    }

    // Generate random maze
    Maze GenerateMaze_Random()
    {
        // Init maze and maze size
        if (!level.customMazeSize)
        {
            level.mazeWidth = Formula.CalculateMazeSideSize(level.mazeDifficulty);
            level.mazeLength = Formula.CalculateMazeSideSize(level.mazeDifficulty);
        }
        currentMaze = new Maze(level.mazeWidth, level.mazeLength);

        // Generate rooms
        GenerateRooms();

        // Generate hallways
        GenerateHallways();

        // Init tile list with coordinates
        for (int i = 0; i < level.mazeWidth; i++)
        {
            for (int j = 0; j < level.mazeLength; j++)
            {
                currentMaze.mazeTileList.Add(currentMaze.mazeTile[i, j]);
            }
        }

        return currentMaze;
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

    #region Random Layout

    #region Room
    //---------------------------------------
    //      Room
    //---------------------------------------
    void GenerateRooms()
    {
        InitRoomData();

        if (currentMaze.roomList.Count == 0)
            return;

        AllocateRoom(currentMaze.roomList, currentMaze.allocatedRoomList, new List<AreaForRoom>() { GetStartMazeArea(roomDistance) });
        foreach (Room room in currentMaze.allocatedRoomList)
        {
            GenerateRoom(room);
        }
    }

    void InitRoomData()
    {
        foreach (GameObject obj in level.mazeSetting.Rooms)
        {
            GameObject roomObj = obj.transform.Find("Room").gameObject;
            Room newRoom = LoadRoomObject(roomObj);
            currentMaze.roomList.Add(newRoom);
        }
    }

    Room LoadRoomObject(GameObject roomObj)
    {
        Room newRoom = new Room();

        newRoom.prefab = roomObj;

        // Init room size
        int width = 0;
        int length = 0;

        newRoom.rot = Random.Range(0, 4);
        
        foreach (Transform child in roomObj.transform)
        {
            GameObject tileObj = child.gameObject;
            int X = GetObjX(tileObj);
            int Z = GetObjZ(tileObj);

            width = width < (X + 1) ? (X + 1) : width;
            length = length < (Z + 1) ? (Z + 1) : length;
        }

        newRoom.width = (newRoom.rot == 1 || newRoom.rot == 3) ? length : width;
        Utilities.TryCatchError(width > level.mazeWidth, "The width of room cannot be larger than the maze size.");
        newRoom.length = (newRoom.rot == 1 || newRoom.rot == 3) ? width : length;
        Utilities.TryCatchError(length > level.mazeLength, "The length of room cannot be larger than the maze size.");

        return newRoom;
    }

    void AllocateRoom(List<Room> roomList, List<Room> allocatedRoomList, List<AreaForRoom> areaList)
    {
        List<Room> tmpRoomList = new List<Room>(roomList);
        Room room = tmpRoomList[Random.Range(0, tmpRoomList.Count)];
        AreaForRoom area;

        // Find the area fits this room 
        List<AreaForRoom> tmpAreaList = new List<AreaForRoom>(areaList);

        while (true)
        {
            if (tmpAreaList.Count <= 0)
            {
                return;
            }

            area = tmpAreaList[Random.Range(0, tmpAreaList.Count)];
            if (RoomFitsArea(room, area))
                break;

            tmpAreaList.Remove(area);
        }

        // Allocate the room in area
        room.left = area.left + Random.Range(0, area.width - room.width);
        room.bot = area.bot + Random.Range(0, area.length - room.length);
        room.right = room.left + room.width - 1;
        room.top = room.bot + room.length - 1;
        allocatedRoomList.Add(room);

        // Update areas
        areaList = UpdateRoomAreaList(areaList, room);

        // Update room list
        tmpRoomList.Remove(room);
        if (tmpRoomList.Count <= 0)
            return;

        // Allocate next room
        AllocateRoom(tmpRoomList, allocatedRoomList, areaList);
    }

    bool RoomFitsArea(Room room, AreaForRoom area)
    {
        return ((room.width <= area.width) && (room.length <= area.length));
    }

    void GenerateRoom(Room room)
    {
        GameObject roomGroupObj = new GameObject() { name = "Room " + room.left + "," + room.top + "," + room.right + "," + room.bot };
        roomGroupObj.transform.parent = currentMaze.mazeGroupObj.transform;

        GameObject rotObj = Instantiate(room.prefab);
        LoadItemSpawnerToObject(rotObj, room.prefab);

        int rot = room.rot;
        rotObj.transform.rotation = Quaternion.Euler(0, 90 * rot, 0);
        if (rot == 1)
            rotObj.transform.position = new Vector3(0, 0, (room.length - 1) * 10);
        else if (rot == 2)
            rotObj.transform.position = new Vector3((room.width - 1) * 10, 0, (room.length - 1) * 10);
        else if (rot == 3)
            rotObj.transform.position = new Vector3((room.width - 1) * 10, 0, 0);

        foreach (Transform child in rotObj.transform)
        {
            if (child.GetComponent<PossibleItemSpawnPoint>() != null)
            {
                child.transform.parent = null;
                PossibleItemSpawnPoint newSpawnPoint = child.GetComponent<PossibleItemSpawnPoint>();
                newSpawnPoint.X = room.left + GetObjX(child.gameObject);
                newSpawnPoint.Z = room.bot + GetObjZ(child.gameObject);
                child.transform.position = new Vector3(newSpawnPoint.X * 10, 0, newSpawnPoint.Z * 10);
                possibleItemSpawnPointList.Add(newSpawnPoint);
                continue;
            }

            GameObject tilePrefab = child.gameObject;

            // Gather tile data from preset tile object
            int X = room.left + GetObjX(tilePrefab);
            int Z = room.bot + GetObjZ(tilePrefab);

            int rotCount = Mathf.FloorToInt(tilePrefab.transform.eulerAngles.y / 90);
            rotCount = rotCount < 4 ? rotCount : (rotCount - 4);
            WallLayout wallLayout = GetWallLayoutFromObj(tilePrefab);
            bool[] wall = GetWallInfoFromObj(tilePrefab, rotCount);

            // Spawn tile object
            GameObject tileObj = GameObject.Instantiate(tilePrefab, new Vector3(X * 10, -0, Z * 10), Quaternion.Euler(0, 90 * rotCount, 0));
            tileObj.name = "Tile [" + X + "]" + "[" + Z + "] " + "(" + wallLayout + ")";
            Tile tile = tileObj.AddComponent<Tile>();

            // Generate Tile class data
            tile.X = X;
            tile.Z = Z;
            tile.wall = wall;
            tile.wallLayout = wallLayout;
            AssignWallFloorObjToTile(tile, wallLayout, rotCount);
            tile.areaID = "Room " + room.left + "," + room.top + "," + room.right + "," + room.bot;

            // Group tile
            tile.transform.parent = roomGroupObj.transform;
            currentMaze.mazeTile[X, Z] = tile;
        }

        Destroy(rotObj);
    }

    void LoadItemSpawnerToObject(GameObject obj, GameObject roomPrefab)
    {
        PossibleItemSpawnPoint[] spawnPoints = roomPrefab.transform.parent.GetComponentsInChildren<PossibleItemSpawnPoint>();
        foreach (PossibleItemSpawnPoint spawnPoint in spawnPoints)
        {
            GameObject spawnPointObj = Instantiate(spawnPoint.gameObject);
            spawnPointObj.transform.parent = obj.transform;
        }
    }
    #endregion

    #region Hallway
    //---------------------------------------
    //      Hallway
    //---------------------------------------

    // Each hallway is generated using Kruskal's algorithm
    void GenerateHallways()
    {
        // Generate areas for hallways
        List<AreaForRoom> hallwayAreaList = new List<AreaForRoom>();
        hallwayAreaList.Add(GetStartMazeArea(0));

        if (currentMaze.allocatedRoomList.Count > 0)
            hallwayAreaList = GetHallwayAreaList(currentMaze.allocatedRoomList, hallwayAreaList);

        // Generate Kruskal's algorithm maze for each area
        foreach (AreaForRoom area in hallwayAreaList)
        {
            GenerateHallway(area);
        }

        // Connect rooms and hallways
        ConnectHallways();
    }

    List<AreaForRoom> GetHallwayAreaList(List<Room> roomList, List<AreaForRoom> areaList)
    {
        List<Room> tmpRoomList = new List<Room>(roomList);
        Room room = tmpRoomList[Random.Range(0, tmpRoomList.Count)];

        areaList = UpdateHallwayAreaList(areaList, room);

        tmpRoomList.Remove(room);
        if (tmpRoomList.Count <= 0)
            return areaList;

        areaList = GetHallwayAreaList(tmpRoomList, areaList);
        return areaList;
    }

    void GenerateHallway(AreaForRoom area)
    {
        if (!currentMaze.hallwayTileList.ContainsKey(GetAreaID(area, AreaType.Hallway)))
            currentMaze.hallwayTileList.Add(GetAreaID(area, AreaType.Hallway), new List<Tile>());

        GameObject groupObj = new GameObject();
        groupObj.name = GetAreaID(area, AreaType.Hallway);
        groupObj.transform.parent = currentMaze.mazeGroupObj.transform;

        MazeBlueprint hallwayBP = new MazeBlueprint(area.width, area.length);

        for (int i = 0; i < area.width; i++)
        {
            for (int j = 0; j < area.length; j++)
            {
                Tile tile = GenerateTileWithBlueprint(i, j, hallwayBP);
                int X = area.left + i;
                int Z = area.bot + j;

                tile.gameObject.name = "Tile [" + X + "]" + "[" + Z + "] " + "(" + tile.wallLayout + ")";
                tile.gameObject.transform.position = new Vector3(X * 10, 0, Z * 10);
                tile.X = X;
                tile.Z = Z;
                tile.areaID = GetAreaID(area, AreaType.Hallway);

                tile.gameObject.transform.parent = groupObj.transform;
                currentMaze.mazeTile[X, Z] = tile;
                currentMaze.hallwayTileList[GetAreaID(area, AreaType.Hallway)].Add(tile);
            }
        }
    }

    void ConnectHallways()
    {
        foreach (string areaID in currentMaze.hallwayTileList.Keys)
        {
            string sizeString = areaID.Substring(areaID.LastIndexOf(" ") + 1);
            string[] edges = sizeString.Split(',');
            int left = int.Parse(edges[0]);
            int top = int.Parse(edges[1]);
            int right = int.Parse(edges[2]);
            int bot = int.Parse(edges[3]);

            List<string> ConnectedIDList = new List<string>();
            Dictionary<string, List<Tile>> NeedToConnectIDList = new Dictionary<string, List<Tile>>();
            Dictionary<string, int> NeedToConnectDirList = new Dictionary<string, int>();

            foreach (Tile tile in currentMaze.hallwayTileList[areaID])
            {
                if ((tile.X == left) && ((tile.X - 1) >= 0))
                {
                    UpdateNeedToConnectList(tile, currentMaze.mazeTile[tile.X - 1, tile.Z], ConnectedIDList, NeedToConnectIDList, NeedToConnectDirList);
                }

                if ((tile.Z == top) && ((tile.Z + 1) < currentMaze.mazeLength))
                {
                    UpdateNeedToConnectList(tile, currentMaze.mazeTile[tile.X, tile.Z + 1], ConnectedIDList, NeedToConnectIDList, NeedToConnectDirList);
                }

                if ((tile.X == right) && ((tile.X + 1) < currentMaze.mazeWidth))
                {
                    UpdateNeedToConnectList(tile, currentMaze.mazeTile[tile.X + 1, tile.Z], ConnectedIDList, NeedToConnectIDList, NeedToConnectDirList);
                }

                if ((tile.Z == bot) && ((tile.Z - 1) >= 0))
                {
                    UpdateNeedToConnectList(tile, currentMaze.mazeTile[tile.X, tile.Z - 1], ConnectedIDList, NeedToConnectIDList, NeedToConnectDirList);
                }
            }

            foreach (string NeedToConnectID in NeedToConnectIDList.Keys)
            {
                List<Tile> tileList = new List<Tile>();
                if (NeedToConnectID.Contains("Room"))
                    tileList = NeedToConnectIDList[NeedToConnectID];
                else
                    tileList.Add(NeedToConnectIDList[NeedToConnectID][Random.Range(0, NeedToConnectIDList[NeedToConnectID].Count)]);

                foreach (Tile tile in tileList)
                {
                    int dir = NeedToConnectDirList[NeedToConnectID];
                    Tile reverseTile;
                    if (dir == 0)
                        reverseTile = currentMaze.mazeTile[tile.X, tile.Z + 1];
                    else if (dir == 1)
                        reverseTile = currentMaze.mazeTile[tile.X + 1, tile.Z];
                    else if (dir == 2)
                        reverseTile = currentMaze.mazeTile[tile.X, tile.Z - 1];
                    else
                        reverseTile = currentMaze.mazeTile[tile.X - 1, tile.Z];

                    RemoveTileWall(tile, dir, NeedToConnectIDList);
                    if (MazeUTL.WallOnDir(reverseTile, MazeUTL.GetReverseDir(dir)))
                        RemoveTileWall(reverseTile, MazeUTL.GetReverseDir(dir), NeedToConnectIDList);
                }
            }
        }
    }

    void UpdateNeedToConnectList(Tile org, Tile target, List<string> ConnectedIDList, Dictionary<string, List<Tile>> NeedToConnectIDList, Dictionary<string, int> NeedToConnectDirList)
    {
        string targetID = target.areaID;

        if (ConnectedIDList.Contains(targetID))
            return;

        int tryConnectDir = MazeUTL.GetNeighborTileDir(org, target);

        if (targetID.Contains("Room"))
        {
            int reverseDir = MazeUTL.GetReverseDir(tryConnectDir);
            if (MazeUTL.WallOnDir(target, reverseDir))
                return;
        }
        else if (!MazeUTL.WallOnDir(org, tryConnectDir))
        {
            if (!ConnectedIDList.Contains(targetID))
                ConnectedIDList.Add(targetID);

            if (NeedToConnectIDList.ContainsKey(targetID))
                NeedToConnectIDList.Remove(targetID);

            if (NeedToConnectDirList.ContainsKey(targetID))
                NeedToConnectDirList.Remove(targetID);

            return;
        }

        if (!NeedToConnectIDList.ContainsKey(targetID))
            NeedToConnectIDList.Add(targetID, new List<Tile>());

        NeedToConnectIDList[targetID].Add(org);

        if (!NeedToConnectDirList.ContainsKey(targetID))
            NeedToConnectDirList.Add(targetID, tryConnectDir);
    }

    void RemoveTileWall(Tile oldTile, int dir, Dictionary<string, List<Tile>> NeedToConnectIDList)
    {
        GameObject oldTileObj = oldTile.gameObject;

        int X = oldTile.X;
        int Z = oldTile.Z;
        bool[] wall = oldTile.wall;
        int nbWalls = 0;
        WallLayout wallLayout;
        GameObject wallLayoutObj;
        int rotCount = 0;

        wall[dir] = false;

        // Get number of walls
        for (int i = 0; i < wall.Length; i++)
        {
            if (wall[i])
                nbWalls++;
        }

        // Get wall layout, then setup object
        if ((wall[0] != wall[1]) && (wall[0] == wall[2]) && (wall[1] == wall[3]))
            wallLayout = WallLayout.II;
        else
            wallLayout = (WallLayout)nbWalls;

        wallLayoutObj = level.mazeSetting.GetWallLayoutObj(wallLayout);
        Utilities.TryCatchError((wallLayoutObj == null), "'TileLayout" + wallLayout + "' has wrong setup in current Maze Setting.");

        // Get rotation count for based on wall layout so we can rotate the wall layout later.
        rotCount = GetLayoutRotationCount(wall, wallLayout);

        // Spawn tile object
        GameObject tileObj = GameObject.Instantiate(wallLayoutObj, new Vector3(X * 10, -0, Z * 10), Quaternion.Euler(0, 90 * rotCount, 0));
        tileObj.name = "Tile [" + X + "]" + "[" + Z + "] " + "(" + wallLayout + ")";
        Tile newTile = tileObj.AddComponent<Tile>();
        newTile.X = X;
        newTile.Z = Z;
        newTile.wall = wall;
        newTile.wallLayout = wallLayout;
        AssignWallFloorObjToTile(newTile, wallLayout, rotCount);

        // Group tile
        newTile.transform.parent = oldTileObj.transform.parent;
        currentMaze.mazeTile[X, Z] = newTile;

        // Replace instance of old Tile with new tile
        int oldIndex = currentMaze.hallwayTileList[oldTile.areaID].IndexOf(oldTile);
        newTile.areaID = oldTile.areaID;
        currentMaze.hallwayTileList[oldTile.areaID][oldIndex] = newTile;

        foreach (string NeedToConnectID in NeedToConnectIDList.Keys)
        {
            for (int i = 0; i < NeedToConnectIDList[NeedToConnectID].Count; i++)
            {
                if (NeedToConnectIDList[NeedToConnectID][i] == oldTile)
                    NeedToConnectIDList[NeedToConnectID][i] = newTile;
            }
        }

        // Remove Old Tile
        Destroy(oldTileObj);
    }
    #endregion

    #region Area
    //---------------------------------------
    //      Area
    //---------------------------------------
    struct AreaForRoom
    {
        public int left;
        public int top;
        public int right;
        public int bot;
        public int width;
        public int length;
    }

    AreaForRoom GetStartMazeArea(int boundarySize)
    {
        AreaForRoom area = new AreaForRoom();
        area.left = 0 + boundarySize;
        area.top = level.mazeLength - 1 - boundarySize;
        area.right = level.mazeWidth - 1 - boundarySize;
        area.bot = 0 + boundarySize;
        area.width = level.mazeWidth - (boundarySize * 2);
        area.length = level.mazeLength - (boundarySize * 2);

        return area;
    }

    enum AreaType
    {
        Room,
        Hallway
    }

    string GetAreaID(AreaForRoom area, AreaType areaType)
    {
        string id = area.left + "," + area.top + "," + area.right + "," + area.bot;

        if (areaType == AreaType.Room)
            id = "Room " + id;
        else if (areaType == AreaType.Hallway)
            id = "Hallway " + id;

        return id;
    }

    List<AreaForRoom> UpdateRoomAreaList(List<AreaForRoom> areaList, Room room)
    {
        List<AreaForRoom> newList = new List<AreaForRoom>();
        int left = room.left - roomDistance;
        int top = room.top + roomDistance;
        int right = room.right + roomDistance;
        int bot = room.bot - roomDistance;

        foreach (AreaForRoom area in areaList)
        {
            UpdateAreaListFromArea(newList, area, left, top, right, bot);
        }

        return newList;
    }

    void UpdateAreaListFromArea(List<AreaForRoom> areaList, AreaForRoom area, int left, int top, int right, int bot)
    {
        bool interLeft = ((left >= area.left) && (left <= area.right));
        bool interTop = ((top >= area.bot) && (top <= area.top));
        bool interRight = ((right >= area.left) && (right <= area.right));
        bool interBot = ((bot >= area.bot) && (bot <= area.top));
        bool withinWidth = ((left <= area.left) && (right >= area.right));
        bool withinLength = ((top >= area.top) && (bot <= area.bot));
        bool intersect = false;

        if ((interLeft) && (interTop || interBot || withinLength))
        {
            intersect = true;
            AddLeftArea(areaList, area, left);
        }
        if ((interTop) && (interLeft || interRight || withinWidth))
        {
            intersect = true;
            AddTopArea(areaList, area, top);
        }
        if ((interRight) && (interTop || interBot || withinLength))
        {
            intersect = true;
            AddRightArea(areaList, area, right);
        }
        if ((interBot) && (interLeft || interRight || withinWidth))
        {
            intersect = true;
            AddBotArea(areaList, area, bot);
        }
        if (withinWidth && withinLength)
            intersect = true;

        if (!intersect)
            areaList.Add(area);
    }

    void AddLeftArea(List<AreaForRoom> areaList, AreaForRoom area, int left)
    {
        AreaForRoom newArea;
        newArea.left = area.left;
        newArea.top = area.top;
        newArea.right = left - 1;
        newArea.bot = area.bot;
        newArea.width = newArea.right - newArea.left + 1;
        newArea.length = newArea.top - newArea.bot + 1;

        if ((newArea.width > 0) && (newArea.length > 0))
            areaList.Add(newArea);
    }

    void AddTopArea(List<AreaForRoom> areaList, AreaForRoom area, int top)
    {
        AreaForRoom newArea;
        newArea.left = area.left;
        newArea.top = area.top;
        newArea.right = area.right;
        newArea.bot = top + 1;
        newArea.width = newArea.right - newArea.left + 1;
        newArea.length = newArea.top - newArea.bot + 1;

        if ((newArea.width > 0) && (newArea.length > 0))
            areaList.Add(newArea);
    }

    void AddRightArea(List<AreaForRoom> areaList, AreaForRoom area, int right)
    {
        AreaForRoom newArea;
        newArea.left = right + 1;
        newArea.top = area.top;
        newArea.right = area.right;
        newArea.bot = area.bot;
        newArea.width = newArea.right - newArea.left + 1;
        newArea.length = newArea.top - newArea.bot + 1;

        if ((newArea.width > 0) && (newArea.length > 0))
            areaList.Add(newArea);
    }

    void AddBotArea(List<AreaForRoom> areaList, AreaForRoom area, int bot)
    {
        AreaForRoom newArea;
        newArea.left = area.left;
        newArea.top = bot - 1;
        newArea.right = area.right;
        newArea.bot = area.bot;
        newArea.width = newArea.right - newArea.left + 1;
        newArea.length = newArea.top - newArea.bot + 1;

        if ((newArea.width > 0) && (newArea.length > 0))
            areaList.Add(newArea);
    }

    List<AreaForRoom> UpdateHallwayAreaList(List<AreaForRoom> areaList, Room room)
    {
        List<AreaForRoom> newList = new List<AreaForRoom>();
        int left = room.left;
        int top = room.top;
        int right = room.right;
        int bot = room.bot;

        foreach (AreaForRoom area in areaList)
        {
            UpdateAreaListFromAreaNoOverlap(newList, area, left, top, right, bot);
        }

        return newList;
    }

    void UpdateAreaListFromAreaNoOverlap(List<AreaForRoom> areaList, AreaForRoom area, int left, int top, int right, int bot)
    {
        bool interLeft = ((left >= area.left) && (left <= area.right));
        bool interTop = ((top >= area.bot) && (top <= area.top));
        bool interRight = ((right >= area.left) && (right <= area.right));
        bool interBot = ((bot >= area.bot) && (bot <= area.top));
        bool withinWidth = ((left <= area.left) && (right >= area.right));
        bool withinLength = ((top >= area.top) && (bot <= area.bot));
        bool intersect = false;
        List<int> overlapArea = new List<int>();

        if ((interLeft) && (interTop || interBot || withinLength))
        {
            intersect = true;
            AddLeftAreaNoOverlap(areaList, area, left, top, right, bot, overlapArea);
        }
        if ((interTop) && (interLeft || interRight || withinWidth))
        {
            intersect = true;
            AddTopAreaNoOverlap(areaList, area, left, top, right, bot, overlapArea);
        }
        if ((interRight) && (interTop || interBot || withinLength))
        {
            intersect = true;
            AddRightAreaNoOverlap(areaList, area, left, top, right, bot, overlapArea);
        }
        if ((interBot) && (interLeft || interRight || withinWidth))
        {
            intersect = true;
            AddBotAreaNoOverlap(areaList, area, left, top, right, bot, overlapArea);
        }
        if (withinWidth && withinLength)
            intersect = true;

        if (!intersect)
            areaList.Add(area);
    }

    void AddLeftAreaNoOverlap(List<AreaForRoom> areaList, AreaForRoom area, int left, int top, int right, int bot, List<int> overlapArea)
    {
        AreaForRoom newArea;
        newArea.left = area.left;
        newArea.top = area.top;
        newArea.right = left - 1;
        newArea.bot = area.bot;

        if (area.top <= top)
        {
            newArea.top = area.top;
        }
        else if (Random.Range(0, 2) > 0)
        {
            overlapArea.Add(7);
        }
        else
        {
            newArea.top = top;
        }

        if (area.bot >= bot)
        {
            newArea.bot = area.bot;
        }
        else if (Random.Range(0, 2) > 0)
        {
            overlapArea.Add(1);
        }
        else
        {
            newArea.bot = bot;
        }

        newArea.width = newArea.right - newArea.left + 1;
        newArea.length = newArea.top - newArea.bot + 1;

        if ((newArea.width > 0) && (newArea.length > 0))
            areaList.Add(newArea);
    }

    void AddTopAreaNoOverlap(List<AreaForRoom> areaList, AreaForRoom area, int left, int top, int right, int bot, List<int> overlapArea)
    {
        AreaForRoom newArea;
        newArea.left = area.left;
        newArea.top = area.top;
        newArea.right = area.right;
        newArea.bot = top + 1;

        if (area.left >= left)
        {
            newArea.left = area.left;
        }
        else if (overlapArea.Contains(7))
        {
            newArea.left = left;
        }

        if (area.right <= right)
        {
            newArea.right = area.right;
        }
        else if (Random.Range(0, 2) > 0)
        {
            overlapArea.Add(9);
        }
        else
        {
            newArea.right = right;
        }

        newArea.width = newArea.right - newArea.left + 1;
        newArea.length = newArea.top - newArea.bot + 1;

        if ((newArea.width > 0) && (newArea.length > 0))
            areaList.Add(newArea);
    }

    void AddRightAreaNoOverlap(List<AreaForRoom> areaList, AreaForRoom area, int left, int top, int right, int bot, List<int> overlapArea)
    {
        AreaForRoom newArea;
        newArea.left = right + 1;
        newArea.top = area.top;
        newArea.right = area.right;
        newArea.bot = area.bot;

        if (area.top <= top)
        {
            newArea.top = area.top;
        }
        else if (overlapArea.Contains(9))
        {
            newArea.top = top;
        }

        if (area.bot >= bot)
        {
            newArea.bot = area.bot;
        }
        else if (Random.Range(0, 2) > 0)
        {
            overlapArea.Add(3);
        }
        else
        {
            newArea.bot = bot;
        }

        newArea.width = newArea.right - newArea.left + 1;
        newArea.length = newArea.top - newArea.bot + 1;

        if ((newArea.width > 0) && (newArea.length > 0))
            areaList.Add(newArea);
    }

    void AddBotAreaNoOverlap(List<AreaForRoom> areaList, AreaForRoom area, int left, int top, int right, int bot, List<int> overlapArea)
    {
        AreaForRoom newArea;
        newArea.left = area.left;
        newArea.top = bot - 1;
        newArea.right = area.right;
        newArea.bot = area.bot;

        if (area.right <= right)
        {
            newArea.right = area.right;
        }
        else if (overlapArea.Contains(3))
        {
            newArea.right = right;
        }

        if (area.left >= left)
        {
            newArea.left = area.left;
        }
        else if (overlapArea.Contains(1))
        {
            newArea.left = left;
        }

        newArea.width = newArea.right - newArea.left + 1;
        newArea.length = newArea.top - newArea.bot + 1;

        if ((newArea.width > 0) && (newArea.length > 0))
            areaList.Add(newArea);
    }
    #endregion

    #endregion

    #endregion

    #region Update Dead End List
    void UpdateDeadEndList()
    {
        for (int x = 0; x < currentMaze.mazeWidth; x++)
        {
            for (int z = 0; z < currentMaze.mazeWidth; z++)
            {
                Tile tile = currentMaze.mazeTile[x, z];

                if (tile.wallLayout != WallLayout.C)
                    continue;

                int deepLevel = GetDeadEndDeepLevel(tile);
                if (!currentMaze.DeadEnds.ContainsKey(deepLevel))
                {
                    currentMaze.DeadEnds.Add(deepLevel, new List<Tile>());
                    currentMaze.DeadEndsWithItem.Add(deepLevel, new List<Tile>());
                }

                currentMaze.DeadEnds[deepLevel].Add(tile);
                currentMaze.DeadEndsWithItem[deepLevel].Add(tile);
            }
        }
    }

    int GetDeadEndDeepLevel(Tile currTile)
    {
        return GetDeadEndDeepLevel_Inner(currTile, 0, -1);
    }

    int GetDeadEndDeepLevel_Inner(Tile currTile, int deepLevel, int comingDir)
    {
        List<int> openDirs = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            if (!MazeUTL.WallOnDir(currTile, i))
                openDirs.Add(i);
        }

        if (comingDir != -1)
            openDirs.Remove(comingDir);

        if (openDirs.Count != 1)
            return deepLevel;

        deepLevel++;

        int nextDir = openDirs[Random.Range(0, openDirs.Count)];
        comingDir = (nextDir + 2) < 4 ? nextDir + 2 : nextDir - 2;
        Tile nextTile = MazeUTL.GetDirNeighborTile(currTile, nextDir);
        return GetDeadEndDeepLevel_Inner(nextTile, deepLevel, comingDir);
    }
    #endregion

    #region Generate Detect Regions
    void GenerateDetectRegions()
    {
        // Generate regions
		List<string> regionList = new List<string>();

        for (int i = 0; i < currentMaze.mazeLength; i++)
        {
            for (int j = 0; j < currentMaze.mazeWidth; j++)
            {
                TryUpdateLinearRegionForTile(0, currentMaze.mazeTile[i, j], regionList);
                TryUpdateLinearRegionForTile(1, currentMaze.mazeTile[i, j], regionList);
                TryUpdateRectRegionForTile(currentMaze.mazeTile[i, j], regionList);
            }
        }

        currentMaze.detectRegions = regionList;

		// Assign regions to each tile
        foreach (Tile tile in currentMaze.mazeTileList)
        {
            string tileAddress = MazeUTL.GetTileAddress(tile.X, tile.Z);
            foreach (string region in regionList)
            {
				if (MazeUTL.CheckRegionHasAddress(region, tileAddress))
                    tile.detectRegions.Add(region);
            }
        }
    }

    void TryUpdateLinearRegionForTile(int linearDir, Tile tile, List<string> regionList)
    {
        string region = GetLinearRegion(linearDir, tile);

        if (IsUniqueRegion(region, regionList))
            regionList.Add(region);
    }

    void TryUpdateRectRegionForTile(Tile tile, List<string> regionList)
    {
        if ((tile.X + 1) >= currentMaze.mazeWidth)
            return;

        string testRegion = GetLinearRegionConditions(0, tile, 0, new List<int>() { 1 });
        if (IsOneTileRegion(testRegion))
            return;

        string region = testRegion;
        int regionHeight = testRegion.Length;

        for (int i = tile.X + 1; i < currentMaze.mazeWidth; i++)
        {
            string newRegion = "";
            newRegion = GetLinearRegionConditions(0, currentMaze.mazeTile[i, tile.Z], regionHeight, new List<int>() { 3 });

            if (IsOneTileRegion(newRegion))
                break;

            if (newRegion.Length < regionHeight)
            {
                TryUpdateRectRegionForTileWithHeight(tile, regionList, newRegion.Length);
                break;
            }   

            region += newRegion;
        }

        if (IsUniqueRegion(region, regionList))
            regionList.Add(region);
    }

    void TryUpdateRectRegionForTileWithHeight(Tile tile, List<string> regionList, int height)
    {
        string region = GetLinearRegionConditions(0, tile, height, new List<int>() { 1 });
        if (region.Length != height)
            return;

        for (int i = tile.X + 1; i < currentMaze.mazeWidth; i++)
        {
            string newRegion = "";
            newRegion = GetLinearRegionConditions(0, currentMaze.mazeTile[i, tile.Z], height, new List<int>() { 3 });

            if (IsOneTileRegion(newRegion))
                break;

            if (newRegion.Length != height)
                break;

            region += newRegion;
        }

        if (IsUniqueRegion(region, regionList))
            regionList.Add(region);
    }

    string GetLinearRegion(int linearDir, Tile tile)
    {
        return GetLinearRegionConditions(linearDir, tile, 0, new List<int>());
    }

    string GetLinearRegionConditions(int linearDir, Tile tile, int height, List<int> wallsShouldntContain)
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
            length = currentMaze.mazeLength;
        }
        else
        {
            startID = tile.X;
            length = currentMaze.mazeWidth;
        }

        for (int i = startID; i < length; i++)
        {
            Tile currTile;
            bool containWrongWalls = false;

            // Get current tile based on linear direction
            if (linearDir == 0)
                currTile = currentMaze.mazeTile[tile.X, i];
            else
                currTile = currentMaze.mazeTile[i, tile.Z];

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
    void GenerateMazeItems()
    {
		if (level.customMazeObject != null)
		{
			GenerateCustomTileItems();

			if (level.tileStart == null)
				GenerateStartPoint_Random();

			if (level.tileObjective == null)
				GenerateObjective_Random();
		}
		else
		{
			GenerateStartPoint_Random();
            GenerateObjective_Random();
            GenerateBodyPartChest_Random();
            GenerateSlimeElement_Random();
            GenerateCompass_Random();
			GenerateHealthPack_Random();
		}
    }

    #region Custom Maze Items
    void GenerateCustomTileItems()
    {
        // Custom Tile Item from spawner
        foreach (TileItemSpawner spawner in customTileItemSpawners)
        {
            TileItem item = spawner.GetTileItem();

            Tile tile = GetObjLocatedTile(spawner.gameObject);
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
    void GenerateStartPoint_Random()
    {
        //Tile tile = MazeUTL.GetRandomTileFromList(currentMaze.mazeTileList);
        Tile tile = GetPossibleItemSpawnTile();

        tile.SpawnTileItem(level.startPointPrefab);
        level.tileStart = tile;
        TilesWithItem.Add(tile);
    }

    void GenerateObjective_Random()
    {
        Tile tile = GetPossibleItemSpawnTile();

        tile.SpawnTileItem(level.objectivePrefab);
        level.tileObjective = tile;
        TilesWithItem.Add(tile);
    }

    void GenerateHealthPack_Random()
    {
        int numPacks = Formula.CalculateHealthPackNum(level.mazeDifficulty);

        for (int i = 0; i < numPacks; i++)
        {
            Tile tile = GetPossibleItemSpawnTile();
            tile.SpawnTileItem(level.HealthPackPrefab);
            TilesWithItem.Add(tile);
        }
    }

    void GenerateSlimeElement_Random()
    {
        // Calculate the number of items needed to spawn for this maze
        int numElements = Formula.CalculateSlimeElementNum(level.mazeDifficulty);
        TileItem spawningElement = level.mazeSetting.SlimeElements[Random.Range(0, level.mazeSetting.SlimeElements.Count)];

        for (int i = 0; i < numElements; i++)
        {
            Tile tile = GetPossibleItemSpawnTile();
            tile.SpawnTileItem(spawningElement.gameObject);
            TilesWithItem.Add(tile);
        }
    }

    void GenerateBodyPartChest_Random()
    {
        // Calculate the number of items needed to spawn for this maze
        int numChests = Formula.CalculateBodyPartChestNum(level.mazeDifficulty);
        numChests = numChests < level.mazeSetting.bodyParts.Count ? numChests : level.mazeSetting.bodyParts.Count;
        List<BodyPart> partList = GetBodyPartList(numChests);

        for (int i = 0; i < numChests; i++)
        {
            Tile tile = GetPossibleItemSpawnTile();
            TileItem item = tile.SpawnTileItem(level.BodyPartChestPrefab);
            TilesWithItem.Add(tile);
            item.bodyPart = partList[i];
        }
    }

    void GenerateCompass_Random()
    {
        int numCompass = Formula.CalculateCompassNum(level.mazeDifficulty);

        for (int i = 0; i < numCompass; i++)
        {
            Tile tile = GetPossibleItemSpawnTile();
            tile.SpawnTileItem(level.CompassPrefab);
            TilesWithItem.Add(tile);
        }
    }
    #endregion

    #endregion

    #region Misc functions for generating items
    Tile GetPossibleItemSpawnTile()
    {
        Tile tile = MazeUTL.GetDeepestEmptyDeadEnd();

        if (possibleItemSpawnPointList.Count <= 0)
            return tile;

        PossibleItemSpawnPoint spawnPoint = possibleItemSpawnPointList[Random.Range(0, possibleItemSpawnPointList.Count)];
        tile = currentMaze.mazeTile[spawnPoint.X, spawnPoint.Z];

        possibleItemSpawnPointList.Remove(spawnPoint);
        Destroy(spawnPoint.gameObject);

        return tile;
    }


    void InitCustomMazeObjList()
    {
        foreach (Transform child in level.customMazeObject.transform)
        {
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

    Tile GetObjLocatedTile(GameObject obj)
    {
        return currentMaze.mazeTile[GetObjX(obj), GetObjZ(obj)];
    }

    List<Tile> GetItemSpawnRandomTiles(int numItems, int range, List<Tile> tileList, List<Tile> tilesLeft, List<Tile> exclusiveTiles)
    {
        if (numItems <= 0)
            return tileList;

        tilesLeft = MazeUTL.UpdateTileListOutOfRange(tilesLeft, exclusiveTiles, range);
        Tile tile = MazeUTL.GetRandomTileFromList(tilesLeft);
        tileList.Add(tile);

        List<Tile> newExclusiveTiles = new List<Tile>();
        foreach (Tile t in exclusiveTiles)
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
    
    #region Misc.
    int GetObjX(GameObject obj)
    {
        return Mathf.RoundToInt(obj.transform.position.x / 10);
    }

    int GetObjZ(GameObject obj)
    {
        return Mathf.RoundToInt(obj.transform.position.z / 10);
    }
    #endregion
}