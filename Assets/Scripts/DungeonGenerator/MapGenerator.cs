using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DungeonGenerator;
using Assets.Scripts.Extension;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public Tilemap  WallTilemap;
    public Tilemap  GroundTilemap;
    public TileBase WallTile;
    public TileBase GroundTile;
    public TileBase PathTile;
    public int      OuterCount = 50;

    private int[,] _mapArray;

    public int RoomMinWidth     = 10;
    public int RoomMaxWidth     = 30;
    public int RoomMinHeight    = 10;
    public int RoomMaxHeight    = 30;
    public int RoomMinCount     = 5;
    public int RoomMaxCount     = 10;
    public int MinDistanceRooms = 15;
    public int MaxDistanceRooms = 30;
    public int MinPassWidth     = 1;
    public int MaxPassWidth     = 3;
    public int PassClearPercent = 50;
    public int Seed             = -1;

    // Start is called before the first frame update
    private void Start()
    {
        GenerateMapAndSetPlayer();
    }

    private void GenerateMapAndSetPlayer()
    {
        GroundTilemap.ClearAllTiles();
        WallTilemap.ClearAllTiles();
        var map = new Map(RoomMinWidth..RoomMaxWidth, RoomMinHeight..RoomMaxHeight, RoomMinCount..RoomMaxCount, MinDistanceRooms..MaxDistanceRooms, MinPassWidth..MaxPassWidth, PassClearPercent, Seed);
        //var area = map.Rooms.Select(_ => _.Bounds).UnionRectIntList();
        _mapArray = map.ToArray();
        for (var i = -OuterCount; i < map.Width + OuterCount; i++)
        for (var j = -OuterCount; j < map.Height + OuterCount; j++)
        {
            if (i < 0 || i >= map.Width || j < 0 || j >= map.Height)
            {
                WallTilemap.SetTile(new Vector3Int(i, -j), WallTile);
            }
            else
            {
                switch (_mapArray[i, j])
                {
                    case 0:
                        WallTilemap.SetTile(new Vector3Int(i, -j), WallTile);
                        break;
                    case 1:
                        GroundTilemap.SetTile(new Vector3Int(i, -j), GroundTile);
                        break;
                    case 2:
                        GroundTilemap.SetTile(new Vector3Int(i, -j), PathTile);
                        break;
                }
            }
        }

        GameObject.FindWithTag("Player").transform.position = new Vector3(map.Rooms[0].MidPoint.x, -map.Rooms[0].MidPoint.y);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            GenerateMapAndSetPlayer();
    }
}
