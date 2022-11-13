using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using System;

public class SkyController : MonoBehaviour
{
    public Material daySky;
    public Material nightSky;
    public Material sunsetSky;

    public Light lt;

    public PostProcessVolume postProcessVolume;
    public PostProcessProfile dayPp;
    public PostProcessProfile sunsetPp;
    public PostProcessProfile nightPp;

    public Text txtMain;
    public Text txtSub;


    private void Start()
    {
        // 현재 시간 고려해 SkyBox 세팅
        // 낮 : 오전 7시~ / 일몰 : 오후 5시~ / 저녁 : 오후 6시~
        if (!GameManager.Instance.treeController.demoMode)
        {
            if (DateTime.Now.Hour > 7 && DateTime.Now.Hour < 17) Day();
            else if (DateTime.Now.Hour > 17 && DateTime.Now.Hour < 18)
            {
                txtMain.color = Color.white;
                txtMain.color = Color.white;
                Sunset();
            }
            else
            {
                txtMain.color = Color.white;
                txtMain.color = Color.white;
                Night();
            }
        }
        //else
        //{
        //    firstPlantDate = DateTime.Now;
        //}
    }

    public void Day()
    {
        print("Day Sky");
        RenderSettings.skybox = daySky;
        //ltSetting = daySetting;
        //postProcessVolume.profile = dayPp;
        lt.color = new Color(1, 0.9568627f, 0.8392157f);
        lt.intensity = 1.6f;
    }

    public void Night()
    {
        print("Day Night");
        RenderSettings.skybox = nightSky;
        //ltSetting = sunsetSetting;
        //postProcessVolume.profile = nightPp;
        lt.color = new Color(0.7058824f, 0.5764706f, 0.6941177f);
        lt.intensity = 1.6f;
    }

    public void Sunset()
    {
        print("Day Sunset");
        RenderSettings.skybox = sunsetSky;
        //ltSetting = sunsetSetting;
        //postProcessVolume.profile = sunsetPp;
        lt.color = new Color(0.9245283f, 0.7477447f, 0.6140263f);
        lt.intensity = 1.19f;
    }
}
