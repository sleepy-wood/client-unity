using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSky : MonoBehaviour
{
    public Material daySky;
    public Material nightSky;
    public Material sunsetSky;

    public Light lt;

    public void Day()
    {
        RenderSettings.skybox = daySky;
        lt.color = new Color(1, 0.9568627f, 0.8392157f);
    }

    public void Night()
    {
        RenderSettings.skybox = nightSky;
        lt.color = new Color(0.7058824f, 0.5764706f, 0.6941177f);
    }

    public void Sunset()
    {
        RenderSettings.skybox = sunsetSky;
        lt.color = new Color(0.9245283f, 0.7477447f, 0.6140263f);
    }
}
