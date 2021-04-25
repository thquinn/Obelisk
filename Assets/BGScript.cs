using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGScript : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam.transform.localPosition.y < transform.localPosition.y) {
            transform.Translate(0, -100 / meshRenderer.material.mainTextureScale.x, 0);
        }
    }
}
