using Assets;
using Assets.Model;
using Assets.Model.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour
{
    public GameObject prefabSkillSlot;
    public LayerMask layerMaskSkillSlot;

    public GameManagerScript gmScript;
    public CanvasGroup canvasGroupHUD, canvasGroupSkills, canvasGroupMP;
    public GameObject skillAnchor;
    public Image hpImage, mpImage, xpImage;
    public TextMeshProUGUI floorText, hpText, mpText, lingerText;

    Player player;
    List<SkillSlotScript> skillSlotScripts;

    void Start()
    {
        canvasGroupHUD.alpha = 0;
        player = gmScript.player;
        skillSlotScripts = new List<SkillSlotScript>();
        for (int i = 0; i < player.skills.Length; i++) {
            SkillSlotScript skillSlotScript = Instantiate(prefabSkillSlot, skillAnchor.transform).GetComponent<SkillSlotScript>();
            skillSlotScript.Initialize(player, i);
            skillSlotScripts.Add(skillSlotScript);
        }
        Color c = lingerText.color;
        c.a = 0;
        lingerText.color = c;
    }

    void Update()
    {
        if (player.tile.floor.number > 0) {
            canvasGroupHUD.alpha += .04f;
        }
        if (player.skills[0] != null) {
            canvasGroupSkills.alpha += .04f;
        }
        if (player.HasMPSkills() || player.mp.Item1 < player.mp.Item2) {
            canvasGroupMP.alpha += .04f;
        }
        floorText.text = player.tile.floor.number + "B";
        hpText.text = string.Format("{0}/{1}", player.hp.Item1, player.hp.Item2);
        mpText.text = string.Format("{0}/{1}", player.mp.Item1, player.mp.Item2);
        Vector3 hpImageScale = hpImage.transform.localScale;
        hpImageScale.x = Mathf.Lerp(hpImageScale.x, player.hp.Item1 / (float)player.hp.Item2, .1f);
        hpImage.transform.localScale = hpImageScale;
        Vector3 mpImageScale = mpImage.transform.localScale;
        mpImageScale.x = Mathf.Lerp(mpImageScale.x, player.mp.Item1 / (float)player.mp.Item2, .1f);
        mpImage.transform.localScale = mpImageScale;
        Vector3 xpImageScale = xpImage.transform.localScale;
        xpImageScale.x = Mathf.Lerp(xpImageScale.x, Mathf.Max(0, player.xp.Item1) / player.xp.Item2, .1f);
        xpImage.transform.localScale = xpImageScale;
        Color c = lingerText.color;
        c.a = Mathf.Clamp01(c.a + (player.turnsOnFloor > Player.LINGER_TURNS ? .1f : -.1f));
        lingerText.color = c;
    }
}
