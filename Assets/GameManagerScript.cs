using Assets;
using Assets.Model;
using Assets.Model.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    static float FLOOR_HEIGHT = 6f;
    static float FLOOR_HEIGHT_OFFSET = 3f;
    static float LERP_FLOOR = .05f;
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
        Floor floor = null;
        int attempts = 0;
        do {
            floor = new Floor(nextFloorNumber);
            attempts++;
        }
        while (attempts < 100 && floors.Count > 0 && !floor.IsSuitableAfter(floors[floors.Count - 1]));
        if (attempts < 100) {
            Debug.Log(string.Format("Generated suitable level after {0} attempts.", attempts));
        } else {
            Debug.Log(string.Format("Failed to generate suitable level after {0} attempts.", attempts));
        }
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
        if (!playerScript.fallMode && cameraMoveFrames > 0) {
            float t = cameraMoveFrames / (float)CAMERA_MOVE_FRAMES;
            for (int i = 0; i < floorScripts.Count; i++) {
                int floorNumber = nextFloorNumber - floorScripts.Count + i;
                FloorScript floorScript = floorScripts[i];
                float floorY = -FLOOR_HEIGHT * floorNumber;
                if (i == 0 && floorScripts.Count > 2) {
                    floorY += FLOOR_HEIGHT_OFFSET * t;
                }
                floorScript.transform.localPosition = Vector3.Lerp(floorScript.transform.localPosition, new Vector3(0, floorY, 0), LERP_FLOOR);
            }
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
        }
    }
}
