using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]

public class DungeonBuilder : SingletonMonoBehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    protected override void Awake()
    {
        base.Awake();

        LoadRoomNodeTypeList();

        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplatesList;

        LoadRoomTemplatesIntoDicitonary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttempts <= Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttempsForNodeGraph = 0;

            while (!dungeonBuildSuccessful && dungeonRebuildAttempsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                ClearDungeon();

                dungeonRebuildAttempsForNodeGraph++;

                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);


            }

            if (dungeonBuildSuccessful)
            {
                InstantiateRoomGameObjects();
            }
        }

        return dungeonBuildSuccessful;
    }

    private void LoadRoomTemplatesIntoDicitonary()
    {
        roomTemplateDictionary.Clear();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Template Key In " + roomTemplateList);
            }
        }
    }

    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No Entrance Node");
            return false;
        }


        bool noRoomOverlaps = true;

        noRoomOverlaps = ProcessRoomsInQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        return false;
    }

    private bool ProcessRoomsInQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverlaps;
    }


    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        bool roomOverlaps = true;

        while (roomOverlaps)
        {
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorwayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                return false;
            }
            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;

                room.isPositioned = true;
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }

        return true;
    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;

        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;

                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;

                case Orientation.none:
                    break;

                default:
                    break;
            }
        }
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomTemplate;
    }

    private bool PlaceTheRoom(Room parentRoom, Doorway parentDoorway, Room room)
    {
        Doorway doorway = GetOppositeDoorway(parentDoorway, room.doorwayList);

        if (doorway == null)
        {
            parentDoorway.isUnavailable = true;

            return false;
        }

        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + (parentDoorway.position - parentRoom.templateLowerBounds);

        Vector2Int adjustment = Vector2Int.zero;

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;

            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;

            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;

            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;

            case Orientation.none:
                break;

            default:
                break;
        }

        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + (room.templateUpperBounds - room.templateLowerBounds);

        Room overlappingRoom = CheckForRoomOverlap(room);

        if(overlappingRoom == null)
        {
            parentDoorway.isConnected = true;
            parentDoorway.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            return true;
        }
        else
        {
            parentDoorway.isUnavailable = true;

            return false;
        }
    }

    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorways)
    {
        foreach (Doorway doorwayToCheck in doorways)
        {


            if (Mathf.Abs((int)parentDoorway.orientation - (int)doorwayToCheck.orientation) == 2)
            {
                return doorwayToCheck;
            }
        }
        return null;
    }

    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach(KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            if(room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }

            if(IsOverlappingRoom(roomToTest, room))
            {
                return room;
            }

        }

        return null;
    }

    private bool IsOverlappingRoom(Room roomToTest, Room room)
    {
        if(IsOverlappingInterval(room.lowerBounds.x, room.upperBounds.x, roomToTest.lowerBounds.x, roomToTest.upperBounds.x) && 
        IsOverlappingInterval(room.lowerBounds.y, room.upperBounds.y, roomToTest.lowerBounds.y, roomToTest.upperBounds.y))
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    private bool IsOverlappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        if(Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> doorways)
    {
        foreach (Doorway doorway in doorways)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }

    private void InstantiateRoomGameObjects()
    {
        foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            GameObject roomGameObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            InstantiateRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiateRoom>();

            instantiatedRoom.room = room;

            instantiatedRoom.Initialize(roomGameObject);

            room.instantiateRoom = instantiatedRoom;
        }
    }

    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO nodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (roomTemplate.roomNodeType == nodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        if (matchingRoomTemplateList.Count == 0)
            return null;
        else
            return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new Room();
        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;
        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorwayList = CopyDoorwayList(roomTemplate.doorwayList);

        if (roomNode.parentRoomNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }

        return room;

    }

    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        return null;
    }

    private List<string> CopyStringList(List<string> list)
    {
        List<string> newList = new List<string>();
        foreach (string e in list)
        {
            newList.Add(e);
        }

        return newList;
    }

    private List<Doorway> CopyDoorwayList(List<Doorway> oldList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach (Doorway doorway in oldList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);
        }
        return newDoorwayList;
    }

    private void ClearDungeon()
    {
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
            {
                Room room = keyvaluepair.Value;

                if (room.instantiateRoom != null)
                {
                    Destroy(room.instantiateRoom.gameObject);
                }
            }
            dungeonBuilderRoomDictionary.Clear();
        }
    }

    public RoomTemplateSO GetRoomTemplate(string id)
    {
        if(roomTemplateDictionary.TryGetValue(id, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    public Room GetRoomByRoomID(string id)
    {
        if(dungeonBuilderRoomDictionary.TryGetValue(id, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }
}
