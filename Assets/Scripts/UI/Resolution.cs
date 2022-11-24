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

        float ratioX = Screen.width / 1290;// 600.0f;
        float ratioY = Screen.height / 2796;

        if (ratioX < ratioY) cs.scaleFactor = ratioX;
        else cs.scaleFactor = ratioY;
    }

}
