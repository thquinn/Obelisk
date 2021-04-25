using Assets.Model;
using Assets.Model.Entities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillReplaceScript : MonoBehaviour, IPointerClickHandler {
    public GameManagerScript gmScript;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI text;

    Player player;

    void Start()
    {
        player = gmScript.player;
    }

    void Update()
    {
        canvasGroup.alpha += gmScript.player.replacementSkill == SkillType.None ? -.1f : .1f;
        if (gmScript.player.replacementSkill != SkillType.None) {
            text.text = Skill.NAMES[gmScript.player.replacementSkill];
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        gmScript.player.replacementSkill = SkillType.None;
    }
}
