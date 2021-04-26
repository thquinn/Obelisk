using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXScript : MonoBehaviour
{
    static AudioSource hitStatic;
    public AudioSource hit;

    private void Start() {
        hitStatic = hit;
    }

    public static void SFXHit() {
        hitStatic.Play();
    }
}
