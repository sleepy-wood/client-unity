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

    public AudioSource night;
    public AudioSource sunset;
    public AudioSource day;


    private void Start()
    {
        // 현재 시간 고려해 SkyBox 세팅
        // 낮 : 오전 7시~ / 일몰 : 오후 5시~ / 저녁 : 오후 6시~
        //if (!GameManager.Instance.treeController.demoMode)
        //{
            if (DateTime.Now.Hour > 7 && DateTime.Now.Hour < 17) Day();
            else if (DateTime.Now.Hour > 17 && DateTime.Now.Hour < 18)
            {
                txtMain.color = Color.white;
                txtSub.color = Color.white;
                Sunset();
            }
            else
            {
                txtMain.color = Color.white;
                txtSub.color = Color.white;
                Night();
            }
        //}
        //else
        //{
        //    firstPlantDate = DateTime.Now;
        //}
    }

    public void AudioInit()
    {
        GetComponent<WeatherController>().rainAudio.Stop();
        GetComponent<WeatherController>().snowAudio.Stop();
        GetComponent<WeatherController>().sunnyAudio.Stop();
        night.Stop();
        sunset.Stop();
        day.Stop();
    }

    public void Day()
    {
        RenderSettings.skybox = daySky;
        //ltSetting = daySetting;
        postProcessVolume.profile = dayPp;
        lt.color = new Color(1, 0.9568627f, 0.8392157f);
        lt.intensity = 1.6f;
        AudioInit();
        day.Play();
    }

    public void Night()
    {
        RenderSettings.skybox = nightSky;
        //ltSetting = sunsetSetting;
        postProcessVolume.profile = nightPp;
        lt.color = new Color(0.7058824f, 0.5764706f, 0.6941177f);
        lt.intensity = 1.6f;
        AudioInit();
        night.Play();
    }

    public void Sunset() 
    {
        RenderSettings.skybox = sunsetSky;
        //ltSetting = sunsetSetting;
        postProcessVolume.profile = sunsetPp;
        lt.color = new Color(0.9245283f, 0.7477447f, 0.6140263f);
        lt.intensity = 1.19f;
        AudioInit();
        sunset.Play();
    }
}
