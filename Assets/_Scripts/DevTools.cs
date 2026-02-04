using System;
using UnityEngine;

public class DevTools : MonoBehaviour
{
    public MaskManager manager;
    public MaskData[] masks;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
                manager.AddMaskToStack(masks[0]);
        if (Input.GetKeyDown(KeyCode.Alpha2))
                manager.AddMaskToStack(masks[1]);
        if (Input.GetKeyDown(KeyCode.Alpha3))
                manager.AddMaskToStack(masks[2]);
        if (Input.GetKeyDown(KeyCode.Alpha4)) 
            manager.AddMaskToStack(masks[3]);
    }
}