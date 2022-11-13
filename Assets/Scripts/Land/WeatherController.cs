using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    [Serializable]
    public class WeatherData
    {
        public string weather;
        public string time;
    }
    WeatherData weatherData = new WeatherData();


    /* const weatherCategory = {'0': '맑음',
                                '1': '비',
                                '2': '비/눈',
                                '3': '눈',
                                '4': '소나기',}; */


    public ParticleSystem rainParticle;
    public ParticleSystem snowParticle;
    public ParticleSystem flowerParticle;

    void Start()
    {
        rainParticle.Stop();
        snowParticle.Stop();
        flowerParticle.Stop();
        GetWeatherData();
    }

    /// <summary>
    /// 현재 날씨 가져오기
    /// </summary>
    public async void GetWeatherData()
    {
        // 오늘의 날씨 가져오기
        ResultGetId<WeatherData> resultGet = await DataModule.WebRequestBuffer<ResultGetId<WeatherData>>(
            "/api/v1/utils/weather",
            DataModule.NetworkType.GET,
            DataModule.DataType.BUFFER
            );

        if (!resultGet.result)
        {
            Debug.LogError("WebRequestError : NetworkType[GET]");
        }
        else
        {
            Debug.Log("Weater Data Get 성공");
            Debug.Log($"Weather : {resultGet.data.weather}");
            Debug.Log($"Time : {resultGet.data.time}");
            weatherData.weather = resultGet.data.weather;
            weatherData.time = resultGet.data.time;
        }
    }


    /// <summary>
    /// 날씨 적용
    /// </summary>
    public void Sunny()
    {
        rainParticle.Stop();
        snowParticle.Stop();
    }
    public void Rain()
    {
        rainParticle.Play();
        snowParticle.Stop();
    }
    public void Snow()
    {
        rainParticle.Stop();
        snowParticle.Play();
        flowerParticle.Stop();
    }
    
    public void Flower()
    {
        flowerParticle.Play();
        rainParticle.Stop();
        snowParticle.Stop();
    }
}
