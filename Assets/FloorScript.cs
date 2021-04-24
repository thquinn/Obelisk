﻿using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coor = System.Tuple<int, int>;

public class FloorScript : MonoBehaviour
{
    public GameObject prefabTile, prefabEntity;

    Floor floor;

    public void Initialize(Floor floor) {
        this.floor = floor;
        // Create tiles.
        for (int x = 0; x < floor.Width(); x++) {
            for (int y = 0; y < floor.Height(); y++) {
                if (floor.tiles[x, y].type == TileType.Exit) {
                    continue;
                }
                GameObject tile = Instantiate(prefabTile, transform);
                Vector2 xz = GetXZ(new Coor(x, y));
                tile.transform.localPosition = new Vector3(xz.x, 0, xz.y);
                foreach (Entity entity in floor.tiles[x, y].entities) {
                    Instantiate(prefabEntity, transform).GetComponent<EntityScript>().Initialize(this, entity);
                }
            }
        }
        // TODO: Create walls.
    }

    public Vector2 GetXZ(Coor coor) {
        float midX = floor.Width() / 2;
        float midZ = floor.Height() / 2;
        return new Vector2(coor.Item1 - midX, midZ - coor.Item2);
    }
}
