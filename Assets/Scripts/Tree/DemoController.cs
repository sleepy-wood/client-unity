using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Broccoli.Factory;
using Broccoli.Pipe;
using Broccoli.Generator;

public class DemoController : MonoBehaviour
{
    public TreeController t;
    public TimeManager timeManager;
    public WeatherController w;
    public SkyController s;

    int day;

    void Start()
    {
        
    }

    bool once;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            t.pipeName = "Galaxy";
            t.treePipeline = Resources.Load<Pipeline>("Galaxy");
            t.selectedTreeSetting = t.treeStores[5].treeSettings;
            // 1일차 기본 세팅
            t.PipelineSetting(0);
            // 씨앗심기
            t.SetTree(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            t.SetTree(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            t.SetTree(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            t.SetTree(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            t.SetTree(5);
        }

        // Sprout Particle
        if (Input.GetKeyDown(KeyCode.Z))
        {
            t.sproutParticle.Play();
        }


        // Sky
        if (Input.GetKeyDown(KeyCode.Q))
        {
            w.Sunny();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            w.Rain();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            w.Snow();
        }


        // Weather
        if (Input.GetKeyDown(KeyCode.R))
        {
            s.Day();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            s.Sunset();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            s.Night();
        }


        



    }


}
