using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]

public class DungeonLevelSO : ScriptableObject
{
    public string levelName;

    public List<RoomTemplateSO> roomTemplatesList;

    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region Validation]
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplatesList), roomTemplatesList))
            return;
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
            return;

        bool isEntrance = false;
        bool isCorridorNS = false;
        bool isCorridorEW = false;

        foreach (RoomTemplateSO roomTemplate in roomTemplatesList)
        {
            if (roomTemplate == null)
                return;

            if (roomTemplate.roomNodeType.isCorridorEW)
                isCorridorEW = true;

            if (roomTemplate.roomNodeType.isCorridorNS)
                isCorridorNS = true;

            if (roomTemplate.roomNodeType.isEntrance)
                isEntrance = true;
        }

        if (isCorridorEW == false)
            Debug.Log("In " + this.name.ToString() + " : No E/W Corridor room type specified");

        if (isCorridorNS == false)
            Debug.Log("In " + this.name.ToString() + " : No N/S Corridor room type specified");

        if (isEntrance == false)
            Debug.Log("In " + this.name.ToString() + " : No Entrance Corridor room type specified");


        foreach (RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null)
                return;

            foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
            {
                if (roomNode == null)
                    continue;

                if (roomNode.roomNodeType.isEntrance || roomNode.roomNodeType.isCorridorEW || roomNode.roomNodeType.isCorridorNS ||
                 roomNode.roomNodeType.isCorridor || roomNode.roomNodeType.isNone)
                    continue;

                bool isRoomNodeTypeFound = false;

                foreach (RoomTemplateSO roomTemplate in roomTemplatesList)
                {
                    if (roomTemplate == null)
                        return;

                    if (roomTemplate.roomNodeType == roomNode.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }
                }

                if (!isRoomNodeTypeFound)
                {
                    Debug.Log("In " + this.name.ToString() + " : No room template " + roomNode.roomNodeType.name.ToString() +
                    " found for node graph " + roomNodeGraph.name.ToString());
                }
            }
        }
    }
#endif
    #endregion Validation
}
