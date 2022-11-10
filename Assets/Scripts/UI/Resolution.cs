using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Resolution : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CanvasScaler cs = GetComponent<CanvasScaler>();

        float ratioX = Screen.width / 1080.0f;// 600.0f;
        float ratioY = Screen.height / 1920.0f;

        if (ratioX < ratioY) cs.scaleFactor = ratioX;
        else cs.scaleFactor = ratioY;
    }

}
