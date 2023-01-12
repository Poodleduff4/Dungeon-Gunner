using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    public RoomNodeTypeListSO roomNodeTypeList;
    public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();


    private void Awake()
    {
        LoadRoomNodeDictionary();
    }

    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();
        foreach (RoomNodeSO node in roomNodeList)
        {
            roomNodeDictionary.Add(node.id, node);
        }
    }


    public RoomNodeSO GetRoomNode(RoomNodeTypeSO roomNodeType)
    {
        foreach(RoomNodeSO roomNode in roomNodeList)
        {
            if(roomNode.roomNodeType == roomNodeType)
            {
                return roomNode;
            }
        }
        return null;
    }

    public IEnumerable<RoomNodeSO> GetChildRoomNodes(RoomNodeSO parentRoomnode)
    {
        foreach(string childRoomNodeID in parentRoomnode.childRoomNodeIDList)
        {
            yield return GetRoomNode(childRoomNodeID);
        }
    }


    public RoomNodeSO GetRoomNode(string roomNodeID)
    {
        if(roomNodeDictionary.TryGetValue(roomNodeID, out RoomNodeSO roomNode))
        {
            return roomNode;
        }
        return null;
    }



    #region Editor Code

    #if UNITY_EDITOR

    [HideInInspector] public RoomNodeSO roomNodeToDrawLineFrom = null;
    [HideInInspector] public Vector2 linePosition;

    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 pos)
    {
        roomNodeToDrawLineFrom = node;
        linePosition = pos;
    }

    #endif

    #endregion Editor Code
    
}
