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
    static KeyCode[] KEYBINDS = new KeyCode[] { KeyCode.L, KeyCode.K, KeyCode.J, KeyCode.H };

    public Sprite spriteSlot, spritePassive, spriteActive;

    Player player;
    int index;
    public CanvasGroup canvasGroupSlot, canvasGroupCooldown;
    public Image backImage, skipImage;
    public GameObject keyObject;
    public TextMeshProUGUI nameText, costText, cooldownText, keyText;
    public Collider2D collidre;
    public Skill skill;
    Vector3 originalNameOffset;
    TooltipScript tooltipScript;

    private void Start() {
        originalNameOffset = nameText.gameObject.transform.localPosition;
        tooltipScript = Object.FindObjectOfType<TooltipScript>();
        keyObject.SetActive(false);
    }

    public void Initialize(Player player, int index) {
        this.player = player;
        this.index = index;
        transform.localPosition = new Vector3(0, SPACING * index, 0);
    }

    void Update()
    {
        if (player.skills[index] != skill) {
            skill = player.skills[index];
            ResetSkill();
        }
        if (skill == null) {
            return;
        }
        if (skill.cooldown > 0) {
            canvasGroupSlot.alpha = Mathf.Max(.1f, canvasGroupSlot.alpha - .1f);
            canvasGroupCooldown.alpha += .1f;
        } else {
            canvasGroupSlot.alpha += .1f;
            canvasGroupCooldown.alpha -= .1f;
        }
        
        cooldownText.text = skill.cooldown.ToString();

        // Keybinds.
        if (Input.GetKeyDown(KEYBINDS[index])) {
            OnPointerClick(null);
        }
    }

    void ResetSkill() {
        if (skill == null) {
            backImage.overrideSprite = spriteSlot;
            backImage.type = Image.Type.Tiled;
            nameText.text = "";
            costText.text = "";
            keyObject.SetActive(false);
            return;
        } else if (Skill.COOLDOWNS.ContainsKey(skill.type)) {
            backImage.overrideSprite = spriteActive;
            costText.text = string.Format("{0} MP", Skill.COSTS[skill.type]);
            keyObject.SetActive(true);
            keyText.text = "LKJH"[index].ToString();
        } else {
            backImage.overrideSprite = spritePassive;
            costText.text = "";
            keyObject.SetActive(false);
        }
        backImage.type = Image.Type.Sliced;
        nameText.text = Skill.NAMES[skill.type];
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
