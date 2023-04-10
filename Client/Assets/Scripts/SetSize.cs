using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSize : MonoBehaviour
{
    void Awake()
    {
        Screen.SetResolution(640,480,false);
    }

    // Start is called before the first frame update
    void Start()
    {
    }
}
