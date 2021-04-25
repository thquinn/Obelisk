using Assets;
using Assets.Model;
using Assets.Model.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coor = System.Tuple<int, int>;

public class GameManagerScript : MonoBehaviour
{
    static float FLOOR_HEIGHT = 6f;
    static float FLOOR_HEIGHT_OFFSET = 2.2f;
    static float LERP_FLOOR = .1f;
    static float CAMERA_SIZE = 6;
    static int WAIT_FRAMES = 5;
    static int CAMERA_MOVE_FRAMES = 40;

    public GameObject floorPrefab;

    Camera cam;
    Player player;
    EntityScript playerScript;
    List<Floor> floors;
    List<FloorScript> floorScripts;
    int nextFloorNumber;
    int waitFrames;
    float cameraInitialY;
    float cameraMoveStartY;
    int cameraMoveFrames;

    void Start()
    {
        Application.targetFrameRate = 60;
        cam = Camera.main;
        cameraInitialY = cam.transform.localPosition.y;
        floors = new List<Floor>();
        floorScripts = new List<FloorScript>();
        for (int i = 0; i < 2; i++) {
            MakeNewFloor();
        }
        foreach (EntityScript entityScript in GameObject.FindObjectsOfType<EntityScript>()) {
            if (entityScript.entity.type == EntityType.Player) {
                player = (Player)entityScript.entity;
                playerScript = entityScript;
                break;
            }
        }
    }

    void MakeNewFloor() {
        Floor floor = nextFloorNumber == 0 ? new Floor(0) : Floor.Generate(nextFloorNumber, floors[floors.Count - 1], 100);
        floors.Add(floor);
        FloorScript floorScript = Instantiate(floorPrefab).GetComponent<FloorScript>();
        floorScript.Initialize(floor);
        float y = -FLOOR_HEIGHT * nextFloorNumber;
        if (nextFloorNumber > 0) {
            y -= FLOOR_HEIGHT_OFFSET;
        }
        floorScript.transform.localPosition = new Vector3(0, y, 0);
        floorScripts.Add(floorScript);
        nextFloorNumber++;
    }

    void Update() {
        UpdateCameraAndFloors();
        if (waitFrames <= 0 && !playerScript.fallMode) {
            if (PlayerMovement()) {
                waitFrames = WAIT_FRAMES;
            }
        } else if (--waitFrames == 0) {
            FallAndEnemyMoves();
        }
    }

    void UpdateCameraAndFloors() {
        if (nextFloorNumber == 2) {
            return;
        }
        if (!playerScript.fallMode) {
            for (int i = 0; i < floorScripts.Count; i++) {
                int floorNumber = nextFloorNumber - floorScripts.Count + i;
                FloorScript floorScript = floorScripts[i];
                if (i == 0 && floors.Count > 2) {
                    floorScript.dy += .03f;
                }
                float floorY = -FLOOR_HEIGHT * floorNumber + floorScript.dy;
                floorScript.transform.localPosition = Vector3.Lerp(floorScript.transform.localPosition, new Vector3(0, floorY, 0), LERP_FLOOR);
            }
        }
        if (!playerScript.fallMode && cameraMoveFrames > 0) {
            float t = cameraMoveFrames / (float)CAMERA_MOVE_FRAMES;
            float targetY = (nextFloorNumber - 1.5f) * -FLOOR_HEIGHT + cameraInitialY;
            float y = EasingFunctions.EaseInOutQuad(cameraMoveStartY, targetY, t);
            cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, y, cam.transform.localPosition.z);
            cam.orthographicSize = Mathf.Lerp(nextFloorNumber == 3 ? 4.5f : CAMERA_SIZE, CAMERA_SIZE, t);
            cameraMoveFrames++;
            if (cameraMoveFrames > CAMERA_MOVE_FRAMES) {
                if (floors.Count > 2) {
                    floors.RemoveAt(0);
                    Destroy(floorScripts[0].gameObject);
                    floorScripts.RemoveAt(0);
                }
                cameraMoveFrames = 0;
            }
        }
    }

    bool PlayerMovement() {
        // TODO: Simultaneous movement
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (player.TryMove(-1, 0)) {
                return true;
            }
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            if (player.TryMove(1, 0)) {
                return true;
            }
        } else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            if (player.TryMove(0, -1)) {
                return true;
            }
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            if (player.TryMove(0, 1)) {
                return true;
            }
        }
        return false;
    }
    void FallAndEnemyMoves() {
        if (player.tile.type == TileType.Exit) {
            playerScript.transform.parent = floorScripts[floorScripts.Count - 1].transform;
            playerScript.fallMode = true;
            cameraMoveFrames = 1;
            cameraMoveStartY = cam.transform.localPosition.y;
            Floor nextFloor = floors[floors.Count - 1];
            Tile landingTile = nextFloor.tiles[player.tile.x, player.tile.y];
            player.MoveTo(landingTile);
            MakeNewFloor();
            return;
        }
        // Enemies choose where they want to move.
        Dictionary<Entity, Coor> intents = new Dictionary<Entity, Coor>();
        for (int i = Mathf.Max(0, floors.Count - 2); i < floors.Count; i++) {
            foreach (Tile tile in floors[i].tiles) {
                foreach (Entity entity in tile.entities) {
                    if (entity.type != EntityType.Enemy) {
                        continue;
                    }
                    Coor intent = entity.GetMove();
                    if (intent != null) {
                        intents[entity] = intent;
                    }
                }
            }
        }
        // TODO: All enemies simultaneously move to maximize the number moved.
        // For now, let's just try to move stupidly.
        foreach (var kvp in intents) {
            int dx = kvp.Value.Item1 - kvp.Key.tile.x;
            int dy = kvp.Value.Item2 - kvp.Key.tile.y;
            kvp.Key.TryMove(dx, dy);
        }
    }
}
