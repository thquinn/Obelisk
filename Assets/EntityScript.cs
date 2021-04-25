using Assets.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coor = System.Tuple<int, int>;

public class EntityScript : MonoBehaviour
{
    static float SHADOW_ALPHA = .066f;
    static float LERP_MOVEMENT = .15f;
    static float LERP_SHADOW = .25f;
    static float GRAVITY = .02f;
    static Color COLOR_PLAYER_MARKER = new Color(1, 0, 0, 0);
    static float PIP_OFFSET = .1f;
    static HashSet<EntityTrait> BEHIND_TRAITS = new HashSet<EntityTrait> { EntityTrait.DoubleMove, EntityTrait.Radiant };
    static HashSet<EntityTrait> KEEP_TINT_TRAITS = new HashSet<EntityTrait> { EntityTrait.DoubleMove };

    public GameObject hpPipPrefab;
    public Sprite[] traitSprites;

    public MeshRenderer meshRenderer;
    public SpriteRenderer spriteRenderer, shadowRenderer;
    public SpriteRenderer[] markerRenderers;
    public GameObject spritePivot, hpPivot;

    FloorScript floorScript;
    public Entity entity;
    public bool fallMode = false;
    float dy, hpPivotInitialY;
    List<SpriteRenderer> pips;
    int lastHP;
    SpriteRenderer radiantRenderer;

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
            SetSprites();
            MakeHPPips();
        }
        Update(1, 1);
    }
    void SetSprites() {
        GameObject spritePivot = spriteRenderer.transform.parent.gameObject;
        for (int i = spritePivot.transform.childCount - 1; i >= 1; i++) {
            Destroy(spritePivot.transform.GetChild(i).gameObject);
        }
        radiantRenderer = null;
        foreach (Sprite traitSprite in traitSprites) {
            EntityTrait trait;
            if (!Enum.TryParse<EntityTrait>(traitSprite.name, out trait) || !entity.traits.Has(trait)) {
                continue;
            }
            if (trait == EntityTrait.Flying) {
                spritePivot.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = traitSprite;
            } else {
                SpriteRenderer traitRenderer = Instantiate(spritePivot.transform.GetChild(0), spritePivot.transform).GetComponent<SpriteRenderer>();
                traitRenderer.sprite = traitSprite;
                if (BEHIND_TRAITS.Contains(trait)) {
                    traitRenderer.transform.Translate(0, 0, .01f);
                }
                if (!KEEP_TINT_TRAITS.Contains(trait)) {
                    traitRenderer.color = Color.white;
                }
                if (trait == EntityTrait.Radiant) {
                    radiantRenderer = traitRenderer;
                }
            }
        }
    }
    void MakeHPPips() {
        hpPivot.transform.localRotation = spritePivot.transform.localRotation;
        hpPivot.transform.Translate(0, .25f, -.5f);
        hpPivotInitialY = hpPivot.transform.localPosition.y;
        pips = new List<SpriteRenderer>();
        int pipCount = entity.hp.Item2;
        lastHP = pipCount;
        int perRow = 1;
        if (pipCount > 4) {
            perRow = 3;
        } else if (pipCount > 2) {
            perRow = 2;
        }
        int rows = Mathf.CeilToInt(pipCount / (float)perRow);
        for (int i = 0; i < pipCount; i++) {
            int row = i / perRow;
            int numInThisRow = row < rows - 1 ? perRow : pipCount % perRow;
            if (numInThisRow == 0) {
                numInThisRow = perRow;
            }
            int indexInThisRow = i % perRow;
            float rowOffset = rows / 2f - row - .5f;
            float colOffset = indexInThisRow - numInThisRow / 2f + .5f;
            GameObject pip = Instantiate(hpPipPrefab, hpPivot.transform);
            pip.transform.Translate(PIP_OFFSET * colOffset, PIP_OFFSET * rowOffset, 0);
            pips.Add(pip.transform.GetChild(0).GetComponentInChildren<SpriteRenderer>());
        }
    }

    void Update() {
        Update(LERP_MOVEMENT, LERP_SHADOW);
    }
    void Update(float lerpMovement, float lerpShadow) {
        if (entity.destroyed) {
            Destroy(gameObject);
            return;
        }
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
        // Radiant.
        if (radiantRenderer != null) {
            radiantRenderer.transform.Rotate(Vector3.forward, .5f);
        }
        // Floating.
        Vector3 spritePivotPosition = spritePivot.transform.localPosition;
        spritePivotPosition.y = entity.traits.Has(EntityTrait.Flying) ? Mathf.Sin(Time.frameCount * .02f) * .1f : 0;
        spritePivot.transform.localPosition = spritePivotPosition;
        Vector3 hpPivotPosition = hpPivot.transform.localPosition;
        hpPivotPosition.y = hpPivotInitialY + spritePivotPosition.y;
        hpPivot.transform.localPosition = hpPivotPosition;
        // Shadows.
        bool showShadow = entity.ShowShadow() && dy == 0;
        float shadowAlpha = Mathf.Lerp(shadowRenderer.color.a, showShadow ? SHADOW_ALPHA : 0, lerpShadow);
        if (shadowAlpha < .001f) {
            shadowRenderer.enabled = false;
        } else {
            shadowRenderer.enabled = true;
            shadowRenderer.color = new Color(0, 0, 0, shadowAlpha);
        }
        // HP pips.
        if (pips != null && entity.hp.Item1 != lastHP) {
            for (int i = 0; i < pips.Count; i++) {
                pips[i].color = i < lastHP - 1 ? Color.red : Color.black;
            }
            lastHP = entity.hp.Item1;
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
