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
    static float FLOOR_HEIGHT_OFFSET = 3f;
    static float LERP_FLOOR = .1f;
    static float CAMERA_SIZE = 5.75f;
    static int WAIT_FRAMES = 5;
    static int CAMERA_MOVE_FRAMES = 80;

    public GameObject floorPrefab;
    public HUDScript hudScript;

    Camera cam;
    public Player player;
    EntityScript playerScript;
    public List<Floor> floors;
    List<FloorScript> floorScripts;
    int nextFloorNumber;
    int waitFrames;
    Vector3 cameraInitialPosition;
    Vector3 cameraTargetXZ;
    float cameraMoveStartY;
    int cameraMoveFrames;

    public static Skill clickedSkill;

    void Start()
    {
        // TEST
        for (int i = 0; i < 1000; i++) {
            Player p = new Player(null);
            p.GainXP(10);
            Debug.Log(p.skills[0].type.ToString());
        }
        // END TEST

        Application.targetFrameRate = 60;
        cam = Camera.main;
        cameraInitialPosition = cam.transform.localPosition;
        cameraTargetXZ = cam.transform.localPosition + cam.transform.right * 3;
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
        Floor floor = nextFloorNumber == 0 ? new Floor(0, null) : Floor.Generate(nextFloorNumber, floors[floors.Count - 1], 100);
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
        if (ReadyForMove()) {
            if (PlayerMovement() || PlayerSkills()) {
                player.OnTurnEnd();
                CleanupDestroyed();
                waitFrames = WAIT_FRAMES;
            }
        } else if (--waitFrames == 0) {
            FallAndEnemyMoves();
            CleanupDestroyed();
        }
    }
    bool ReadyForMove() {
        return waitFrames <= 0 && !playerScript.fallMode && player.replacementSkill == SkillType.None;
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
            float targetY = (nextFloorNumber - 1.5f) * -FLOOR_HEIGHT + cameraInitialPosition.y;
            float x = nextFloorNumber == 3 ? EasingFunctions.EaseInOutQuad(cameraInitialPosition.x, cameraTargetXZ.x, t) : cameraTargetXZ.x;
            float y = EasingFunctions.EaseInOutQuad(cameraMoveStartY, targetY, t);
            float z = nextFloorNumber == 3 ? EasingFunctions.EaseInOutQuad(cameraInitialPosition.z, cameraTargetXZ.z, t) : cameraTargetXZ.z;
            cam.transform.localPosition = new Vector3(x, y, z);
            cam.orthographicSize = EasingFunctions.EaseInOutQuad(nextFloorNumber == 3 ? 4.5f : CAMERA_SIZE, CAMERA_SIZE, t);
            cameraMoveFrames++;
            if (cameraMoveFrames > CAMERA_MOVE_FRAMES) {
                if (floors.Count > 2) {
                    floors.RemoveAt(0);
                    floors[0].previous = null;
                    floors[0].entrance = null;
                    Destroy(floorScripts[0].gameObject);
                    floorScripts.RemoveAt(0);
                }
                cameraMoveFrames = 0;
            }
        }
    }

    bool PlayerMovement() {
        if (player.destroyed) {
            return false;
        }
        // TODO: Simultaneous movement
        MoveResult result = MoveResult.NoMove;
        int dx = 0, dy = 0;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            dx = -1;
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            dx = 1;
        } else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            dy = -1;
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            dy = 1;
        } else {
            return false;
        }
        result = player.TryMove(dx, dy);
        if (result == MoveResult.Attack) {
            Tile targetTile = player.tile.GetDelta(dx, dy);
            Entity blockingEntity = targetTile.GetBlockingEntity();
            player.Attack(blockingEntity);
        }
        return result != MoveResult.NoMove;
    }
    bool PlayerSkills() {
        if (clickedSkill == null) {
            return false;
        }
        bool usedTurn = clickedSkill.Use();
        clickedSkill = null;
        return usedTurn;
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
            MoveResult result = kvp.Key.TryMove(dx, dy);
            if (result == MoveResult.Attack) {
                Tile targetTile = kvp.Key.tile.floor.tiles[kvp.Value.Item1, kvp.Value.Item2];
                Entity blockingEntity = targetTile.GetBlockingEntity();
                kvp.Key.Attack(blockingEntity);
            }
            kvp.Key.OnTurnEnd();
        }
    }

    void CleanupDestroyed() {
        foreach (Floor floor in floors) {
            floor.CleanupDestroyed();
        }
    }
}
