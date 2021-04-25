using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour
{
    public GameManagerScript gmScript;
    public CanvasGroup canvasGroupHUD;
    public Image hpImage, mpImage;
    public TextMeshProUGUI hpText, mpText;

    void Start()
    {
        canvasGroupHUD.alpha = 0;
    }

    void Update()
    {
        if (gmScript.player == null) {
            return;
        }
        if (gmScript.player.tile.floor.number > 0) {
            canvasGroupHUD.alpha += .01f;
        }
        SetTexts();
        Vector3 hpImageScale = hpImage.transform.localScale;
        hpImageScale.x = Mathf.Lerp(hpImageScale.x, gmScript.player.hp.Item1 / (float)gmScript.player.hp.Item2, .1f);
        hpImage.transform.localScale = hpImageScale;
        Vector3 mpImageScale = mpImage.transform.localScale;
        mpImageScale.x = Mathf.Lerp(mpImageScale.x, gmScript.player.mp.Item1 / (float)gmScript.player.mp.Item2, .1f);
        mpImage.transform.localScale = mpImageScale;
    }
    void SetTexts() {
        hpText.text = string.Format("{0}/{1}", gmScript.player.hp.Item1, gmScript.player.hp.Item2);
        mpText.text = string.Format("{0}/{1}", gmScript.player.mp.Item1, gmScript.player.mp.Item2);
    }
}
