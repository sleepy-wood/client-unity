using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cine_ChangeSkyBox : MonoBehaviour
{
    public bool isNight = true;
    public bool isSunSet;
    public bool isDay;

    [Space]
    public Material nightMat;
    public Material sunSetMat;
    public Material dayMat;
    private void Update()
    {
        if (isNight)
        {
            RenderSettings.skybox = nightMat;
        }
        else if (isSunSet)
        {
            RenderSettings.skybox = sunSetMat;
        }
        else
        {
            RenderSettings.skybox = dayMat;
        }
    }
}
