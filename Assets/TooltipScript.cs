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
        { SkillType.Autophage, "Lose 20 HP and gain 10 MP." },
        { SkillType.Empower, "Your attacks deal double damage this turn." },
        { SkillType.FastForward, "Advances other skills on cooldown by 1 turn." },
        { SkillType.Leech, "You gain 2 MP whenever you kill an enemy." },
        { SkillType.MaxHP20, "You have 20 additional HP." },
        { SkillType.MaxHP40, "You have 40 additional HP." },
        { SkillType.Phase, "You pass through walls during your next move." },
        { SkillType.Quicken, "Take two turns in a row." },
        { SkillType.Regeneration, "You gain 5 HP upon dropping to a new floor." },
        { SkillType.Shield, "You take no damage until your next turn." },
        { SkillType.Wait, "Do nothing." },
    };
    static Dictionary<EntityTrait, string> TRAIT_NAMES = new Dictionary<EntityTrait, string> {
        { EntityTrait.DoubleDamage, "Double Damage" },
        { EntityTrait.DoubleMove, "Double Time" },
        { EntityTrait.Flying, "Flight" },
        { EntityTrait.ManaBurn, "Mana Burn" },
        { EntityTrait.Radiant, "Radiant" },
        { EntityTrait.TripleDamage, "Triple Damage" },
        { EntityTrait.UpVision, "Third Eye" },
    };
    static Dictionary<EntityTrait, string> TRAIT_TOOLTIPS = new Dictionary<EntityTrait, string> {
        { EntityTrait.DoubleDamage, "This enemy deals double damage." },
        { EntityTrait.DoubleMove, "This enemy moves twice every turn." },
        { EntityTrait.Flying, "This enemy can fly over pitfalls and traps." },
        { EntityTrait.ManaBurn, "You lose 3 MP whenever this enemy attacks you." },
        { EntityTrait.Radiant, "Deals 5 damage when it ends its turn next to you." },
        { EntityTrait.TripleDamage, "This enemy deals triple damage." },
        { EntityTrait.UpVision, "This enemy can see you from the floor below." },
    };
    static string USES_TURN = "\n<color=#FF8080>(Uses your turn.)</color>";
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
            if (entity.type == EntityType.Enemy) {
                foreach (EntityTrait trait in entity.traits) {
                    Sprite sprite = null;
                    foreach (Sprite s in traitIcons) {
                        if (s.name == trait.ToString()) {
                            sprite = s;
                            break;
                        }
                    }
                    totalHeight = MakeBlock(totalHeight, sprite, TRAIT_NAMES[trait], TRAIT_TOOLTIPS[trait]);
                }
            } else if (entity.type == EntityType.Trap) {
                totalHeight = MakeBlock(totalHeight, null, "Trap", "Damages anything that walks over it.");
            }
        }
    }
    float MakeBlock(float totalHeight, Sprite sprite, string headerText, string bodyText) {
        if (sprite != null) {
            Image icon = MakeIcon();
            icon.sprite = sprite;
            icon.transform.Translate(0, -totalHeight, 0, Space.World);
        }
        TextMeshProUGUI header = MakeText();
        header.text = headerText;
        header.transform.Translate(0, -totalHeight, 0);
        totalHeight += header.preferredHeight + POST_TITLE_OFFSET;
        TextMeshProUGUI body = MakeText();
        body.text = bodyText;
        body.font = fontRegular;
        body.fontSize *= .8f;
        body.transform.Translate(0, -totalHeight, 0);
        totalHeight += body.preferredHeight + POST_BODY_OFFSET;
        return totalHeight;
    }

    public void HoverSkill(Skill skill) {
        Clear();
        lastSkill = skill;
        string text = SKILL_TOOLTIPS[skill.type];
        if (Skill.USES_TURN.Contains(skill.type)) {
            text += USES_TURN;
        }
        MakeBlock(0, null, Skill.NAMES[skill.type], text);
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
