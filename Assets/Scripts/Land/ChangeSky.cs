using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class ChangeSky : MonoBehaviour
{
    public Material daySky;
    public Material nightSky;
    public Material sunsetSky;

    public Light lt;

    public PostProcessVolume postProcessVolume;
    public PostProcessProfile dayPp;
    public PostProcessProfile sunsetPp;
    public PostProcessProfile nightPp;

    
    public void Day()
    {
        RenderSettings.skybox = daySky;
        //ltSetting = daySetting;
        postProcessVolume.profile = dayPp;
        lt.color = new Color(1, 0.9568627f, 0.8392157f);
        lt.intensity = 1.6f;
    }

    public void Night()
    {
        RenderSettings.skybox = nightSky;
        //ltSetting = sunsetSetting;
        postProcessVolume.profile = nightPp;
        lt.color = new Color(0.7058824f, 0.5764706f, 0.6941177f);
        lt.intensity = 1.6f;
    }

    public void Sunset()
    {
        RenderSettings.skybox = sunsetSky;
        //ltSetting = sunsetSetting;
        postProcessVolume.profile = sunsetPp;
        lt.color = new Color(0.9245283f, 0.7477447f, 0.6140263f);
        lt.intensity = 1.19f;
    }
}
