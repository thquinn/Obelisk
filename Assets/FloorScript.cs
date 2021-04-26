using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coor = System.Tuple<int, int>;

public class FloorScript : MonoBehaviour
{
    public static Dictionary<Collider, Tile> TILE_LOOKUP = new Dictionary<Collider, Tile>();

    public GameObject prefabTile, prefabEntity, prefabWall;

    Floor floor;
    public float dy = 0;
    HashSet<Collider> colliders;

    public void Initialize(Floor floor) {
        this.floor = floor;
        colliders = new HashSet<Collider>();
        // Create tiles.
        for (int x = 0; x < floor.Width(); x++) {
            for (int y = 0; y < floor.Height(); y++) {
                if (floor.tiles[x, y].type != TileType.Floor) {
                    continue;
                }
                GameObject tile = Instantiate(prefabTile, transform);
                Vector2 xz = GetXZ(new Coor(x, y));
                tile.transform.localPosition = new Vector3(xz.x, 0, xz.y);
                foreach (Entity entity in floor.tiles[x, y].entities) {
                    Instantiate(prefabEntity, transform).GetComponent<EntityScript>().Initialize(this, entity);
                }
                Collider collider = tile.GetComponent<Collider>();
                colliders.Add(collider);
                TILE_LOOKUP[collider] = floor.tiles[x, y];
            }
        }
        // Create walls.
        foreach (Coor coor in floor.wallsRight) {
            GameObject wall = Instantiate(prefabWall, transform);
            Vector2 xz = GetXZ(coor);
            wall.transform.localPosition = new Vector3(xz.x + .5f, 0, xz.y);
        }
        foreach (Coor coor in floor.wallsBelow) {
            GameObject wall = Instantiate(prefabWall, transform);
            Vector2 xz = GetXZ(coor);
            wall.transform.localPosition = new Vector3(xz.x, 0, xz.y - .5f);
            wall.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }
    }

    public Vector2 GetXZ(Coor coor) {
        float midX = floor.Width() / 2;
        float midZ = floor.Height() / 2;
        return new Vector2(coor.Item1 - midX, midZ - coor.Item2);
    }

    private void OnDestroy() {
        foreach (Collider collider in colliders) {
            TILE_LOOKUP.Remove(collider);
        }
    }
}
