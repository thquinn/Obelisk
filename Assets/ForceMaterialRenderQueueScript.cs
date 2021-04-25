using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceMaterialRenderQueueScript : MonoBehaviour
{
    public MeshRenderer mr;

    void Update()
    {
        mr.sharedMaterial.renderQueue = 3001;
    }
}
