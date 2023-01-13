using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class InstantiateRoom : MonoBehaviour
{
    public Room room;
    public Grid grid;
    public Tilemap groundTilemap;
    public Tilemap decoration1Tilemap;
    public Tilemap decoration2Tilemap;
    public Tilemap frontTilemap;
    public Tilemap collisionTilemap;
    public Tilemap minimapTilemap;
    public Bounds roomColliderBounds;

    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        roomColliderBounds = boxCollider2D.bounds;
    }

    public void Initialize(GameObject roomGameObject)
    {
        PopulateTilemapMemberVariables(roomGameObject);
    
        DisableCollisionTilemapRenderer();
    }

    private void PopulateTilemapMemberVariables(GameObject roomGameObject)
    {
        grid = roomGameObject.GetComponentInChildren<Grid>();
        Tilemap[] tilemaps = roomGameObject.GetComponentsInChildren<Tilemap>();

        foreach(Tilemap tilemap in tilemaps)
        {
            if(tilemap.gameObject.tag == "groundTilemap")
            {
                groundTilemap = tilemap;
            }
            else if(tilemap.gameObject.tag == "decoration1Tilemap")
            {
                decoration1Tilemap = tilemap;
            }
            else if(tilemap.gameObject.tag == "decoration2Tilemap")
            {
                decoration2Tilemap = tilemap;
            }
            else if(tilemap.gameObject.tag == "frontTilemap")
            {
                frontTilemap = tilemap;
            }
            else if(tilemap.gameObject.tag == "collisionTilemap")
            {
                collisionTilemap = tilemap;
            }
            else if(tilemap.gameObject.tag == "minimapTilemap")
            {
                minimapTilemap = tilemap;
            }
            
        }

    }

    private void DisableCollisionTilemapRenderer()
    {
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }


}
