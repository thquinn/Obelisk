using Assets.Model;
using Assets.Model.Entities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlotScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    static int SPACING = 80;
    public static Dictionary<SkillType, string> SKILL_NAMES = new Dictionary<SkillType, string> {
        { SkillType.Wait, "Wait" }
    };

    public Sprite spriteSlot, spritePassive, spriteActive;

    Player player;
    int index;
    public CanvasGroup canvasGroup;
    public Image backImage, skipImage;
    public TextMeshProUGUI nameText, costText;
    public Collider2D collidre;
    public Skill skill;
    Vector3 originalNameOffset;
    TooltipScript tooltipScript;

    private void Start() {
        originalNameOffset = nameText.gameObject.transform.localPosition;
        tooltipScript = Object.FindObjectOfType<TooltipScript>();
    }

    public void Initialize(Player player, int index) {
        this.player = player;
        this.index = index;
        transform.localPosition = new Vector3(0, SPACING * index, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (player.skills[index] != skill) {
            skill = player.skills[index];
            ResetSkill();
        }
    }

    void ResetSkill() {
        if (skill == null) {
            backImage.overrideSprite = spriteSlot;
            backImage.type = Image.Type.Tiled;
            nameText.text = "";
            costText.text = "";
            return;
        } else if (Skill.COOLDOWNS.ContainsKey(skill.type)) {
            backImage.overrideSprite = spriteActive;
            costText.text = string.Format("{0} MP", Skill.COSTS[skill.type]);
        } else {
            backImage.overrideSprite = spritePassive;
            costText.text = "";
        }
        backImage.type = Image.Type.Sliced;
        nameText.text = SKILL_NAMES[skill.type];
        skipImage.enabled = Skill.USES_TURN.Contains(skill.type);
        nameText.gameObject.transform.localPosition = originalNameOffset + (skipImage.enabled ? new Vector3(40, 0, 0) : Vector3.zero);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (skill != null) {
            GameManagerScript.clickedSkill = skill;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (skill != null) {
            tooltipScript.HoverSkill(skill);
        }
    }
    public void OnPointerExit(PointerEventData eventData) {
        if (skill != null) {
            tooltipScript.UnhoverSkill(skill);
        }
    }
}
