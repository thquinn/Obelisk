using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScript : MonoBehaviour
{
    public GameManagerScript gmScript;
    public CanvasGroup canvasGroup;

    private void Start() {
        canvasGroup.alpha = 0;
    }

    void Update()
    {
        if (gmScript.player != null && gmScript.player.destroyed) {
            canvasGroup.alpha += .01f;
        }
    }
}
