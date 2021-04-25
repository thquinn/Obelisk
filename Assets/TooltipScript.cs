using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipScript : MonoBehaviour
{
    static Dictionary<SkillType, string> SKILL_TOOLTIPS = new Dictionary<SkillType, string> {
        { SkillType.Wait, "Do nothing." }
    };
    static Dictionary<EntityTrait, string> TRAIT_NAMES = new Dictionary<EntityTrait, string> {
        { EntityTrait.UpVision, "Third Eye" }
    };
    static Dictionary<EntityTrait, string> TRAIT_TOOLTIPS = new Dictionary<EntityTrait, string> {
        { EntityTrait.UpVision, "This enemy can see you from the floor below." }
    };
    static string USES_TURN = "\n<color=#FF4040>(Uses your turn.)</color>";
    static float POST_TITLE_OFFSET = 10;

    public GameObject tooltipPrefab, iconPrefab;
    public TMP_FontAsset fontBold, fontRegular;

    List<TextMeshProUGUI> texts;
    List<SpriteRenderer> icons;
    Skill lastSkill;

    private void Start() {
        texts = new List<TextMeshProUGUI>();
        icons = new List<SpriteRenderer>();
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
        tmp.transform.Translate(0, -height + POST_TITLE_OFFSET, 0);
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
    public void Clear() {
        lastSkill = null;
        foreach (TextMeshProUGUI tmp in texts) {
            Destroy(tmp.gameObject);
        }
        texts.Clear();
    }
}
