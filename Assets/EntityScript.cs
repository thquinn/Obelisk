using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coor = System.Tuple<int, int>;

public class EntityScript : MonoBehaviour
{
    static float SHADOW_ALPHA = .133f;
    static float LERP_MOVEMENT = .15f;
    static float LERP_SHADOW = .25f;
    static float GRAVITY = .02f;
    static Color COLOR_PLAYER_MARKER = new Color(1, 0, 0, 0);

    public MeshRenderer meshRenderer;
    public SpriteRenderer spriteRenderer, shadowRenderer;
    public SpriteRenderer[] markerRenderers;
    public GameObject spritePivot;

    FloorScript floorScript;
    public Entity entity;
    public bool fallMode = false;
    float dy;

    public void Initialize(FloorScript floorScript, Entity entity) {
        this.floorScript = floorScript;
        this.entity = entity;
        if (entity.type == EntityType.Player) {
            spriteRenderer.enabled = false;
            foreach (SpriteRenderer sr in markerRenderers) {
                sr.enabled = true;
                sr.color = COLOR_PLAYER_MARKER;
            }
        } else {
            meshRenderer.enabled = false;
            spritePivot.transform.localRotation = Camera.main.transform.localRotation;
        }
        Update(1, 1);
    }

    void Update() {
        Update(LERP_MOVEMENT, LERP_SHADOW);
    }
    void Update(float lerpMovement, float lerpShadow) {
        // Movement.
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
        // Shadows.
        bool showShadow = entity.ShowShadow() && dy == 0;
        float shadowAlpha = Mathf.Lerp(shadowRenderer.color.a, showShadow ? SHADOW_ALPHA : 0, lerpShadow);
        if (shadowAlpha < .001f) {
            shadowRenderer.enabled = false;
        } else {
            shadowRenderer.enabled = true;
            shadowRenderer.color = new Color(0, 0, 0, shadowAlpha);
        }
        // Markers.
        float dAlpha = -.1f;
        if (entity.type == EntityType.Player && entity.tile.floor.number > 0 && !fallMode) {
            dAlpha = .1f;
        }
        foreach (SpriteRenderer sr in markerRenderers) {
            Color c = sr.color;
            c.a = Mathf.Clamp01(c.a + dAlpha);
            sr.color = c;
        }
    }
}
