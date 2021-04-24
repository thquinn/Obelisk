using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coor = System.Tuple<int, int>;

public class EntityScript : MonoBehaviour
{
    static float SHADOW_ALPHA = .066f;
    static float LERP_MOVEMENT = .15f;
    static float LERP_SHADOW = .25f;
    static float GRAVITY = .03f;

    public SpriteRenderer shadowRenderer;

    FloorScript floorScript;
    public Entity entity;
    public bool fallMode = false;
    float dy;

    public void Initialize(FloorScript floorScript, Entity entity) {
        this.floorScript = floorScript;
        this.entity = entity;
        Update(1, 1);
    }

    void Update() {
        Update(LERP_MOVEMENT, LERP_SHADOW);
    }
    void Update(float lerpMovement, float lerpShadow) {
        Vector2 targetXZ = floorScript.GetXZ(entity.tile.Coor());
        float x = Mathf.Lerp(transform.localPosition.x, targetXZ.x, lerpMovement);
        float z = Mathf.Lerp(transform.localPosition.z, targetXZ.y, lerpMovement);
        float y = 0;
        if (fallMode) {
            dy += GRAVITY;
            y = transform.localPosition.y - dy;
            if (y <= 0) {
                y = 0;
                dy = 0;
                fallMode = false;
            }
        }
        transform.localPosition = new Vector3(x, y, z);
        bool showShadow = entity.ShowShadow() && dy == 0;
        float shadowAlpha = Mathf.Lerp(shadowRenderer.color.a, showShadow ? SHADOW_ALPHA : 0, lerpShadow);
        if (shadowAlpha < .001f) {
            shadowRenderer.enabled = false;
        } else {
            shadowRenderer.enabled = true;
            shadowRenderer.color = new Color(0, 0, 0, shadowAlpha);
        }
    }
}
