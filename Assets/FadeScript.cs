using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeScript : MonoBehaviour
{
    public Image image;
    public bool gameOver;

    void Start()
    {
        image.color = Color.black;
    }

    void Update()
    {
        Color c = image.color;
        c.a = Mathf.Clamp01(c.a + (gameOver ? .01f : -.01f));
        AudioListener.volume = 1 - c.a;
        image.color = c;
        if (c.a == 1) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
