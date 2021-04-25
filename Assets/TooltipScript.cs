using Assets;
using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipScript : MonoBehaviour
{
    static Dictionary<SkillType, string> SKILL_TOOLTIPS = new Dictionary<SkillType, string> {
        { SkillType.Phase, "You pass through walls during your next move." },
        { SkillType.Wait, "Do nothing." },
    };
    static Dictionary<EntityTrait, string> TRAIT_NAMES = new Dictionary<EntityTrait, string> {
        { EntityTrait.DoubleDamage, "Double Damage" },
        { EntityTrait.Flying, "Flight" },
        { EntityTrait.UpVision, "Third Eye" },
    };
    static Dictionary<EntityTrait, string> TRAIT_TOOLTIPS = new Dictionary<EntityTrait, string> {
        { EntityTrait.DoubleDamage, "This enemy deals 20 damage." },
        { EntityTrait.Flying, "This enemy can fly over pitfalls and traps." },
        { EntityTrait.UpVision, "This enemy can see you from the floor below." },
    };
    static string USES_TURN = "\n<color=#FF4040>(Uses your turn.)</color>";
    static float POST_TITLE_OFFSET = -10;
    static float POST_BODY_OFFSET = 10;

    public GameObject tooltipPrefab, iconPrefab;
    public TMP_FontAsset fontBold, fontRegular;
    public LayerMask layerMaskTile;
    public Sprite[] traitIcons;

    List<TextMeshProUGUI> texts;
    List<Image> icons;
    Skill lastSkill;
    Tile lastTile;

    private void Start() {
        texts = new List<TextMeshProUGUI>();
        icons = new List<Image>();
    }

    private void Update() {
        Collider collider = Util.GetMouseCollider(layerMaskTile);
        Tile tile = collider == null ? null : FloorScript.TILE_LOOKUP[collider];
        if (tile == lastTile) {
            return;
        }
        Clear();
        if (tile == null) {
            return;
        }
        lastTile = tile;
        float totalHeight = 0;
        foreach (Entity entity in tile.entities) {
            if (entity.type == EntityType.Player) {
                continue;
            }
            foreach (EntityTrait trait in entity.traits) {
                Image icon = MakeIcon();
                foreach (Sprite sprite in traitIcons) {
                    if (sprite.name == trait.ToString()) {
                        icon.sprite = sprite;
                        break;
                    }
                }
                icon.transform.Translate(0, -totalHeight, 0, Space.World);
                TextMeshProUGUI header = MakeText();
                header.text = TRAIT_NAMES[trait];
                header.transform.Translate(0, -totalHeight, 0);
                totalHeight += header.preferredHeight + POST_TITLE_OFFSET;
                TextMeshProUGUI body = MakeText();
                body.text = TRAIT_TOOLTIPS[trait];
                body.font = fontRegular;
                body.fontSize *= .8f;
                body.transform.Translate(0, -totalHeight, 0);
                totalHeight += body.preferredHeight + POST_BODY_OFFSET;
            }
        }
    }

    public void HoverSkill(Skill skill) {
        Clear();
        lastSkill = skill;
        TextMeshProUGUI tmp = MakeText();
        tmp.text = SkillSlotScript.SKILL_NAMES[skill.type];
        float height = tmp.preferredHeight;
        tmp = MakeText();
        string text = SKILL_TOOLTIPS[skill.type];
        if (Skill.USES_TURN.Contains(skill.type)) {
            text += USES_TURN;
        }
        tmp.text = text;
        tmp.font = fontRegular;
        tmp.fontSize *= .8f;
        tmp.transform.Translate(0, -height - POST_TITLE_OFFSET, 0);
    }
    public void UnhoverSkill(Skill skill) {
        if (lastSkill != skill) {
            return;
        }
        Clear();
    }

    TextMeshProUGUI MakeText() {
        TextMeshProUGUI tmp = Instantiate(tooltipPrefab, transform).GetComponent<TextMeshProUGUI>();
        texts.Add(tmp);
        return tmp;
    }
    Image MakeIcon() {
        Image icon = Instantiate(iconPrefab, transform).GetComponent<Image>();
        icons.Add(icon);
        return icon;
    }
    public void Clear() {
        lastSkill = null;
        lastTile = null;
        foreach (TextMeshProUGUI tmp in texts) {
            Destroy(tmp.gameObject);
        }
        texts.Clear();
        foreach (Image icon in icons) {
            Destroy(icon.gameObject);
        }
        icons.Clear();
    }
}
