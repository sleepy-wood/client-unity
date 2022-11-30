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
    public ParticleSystem goodFall;
    public ParticleSystem badFall;
    public UI_PlantName p;

    int day;

    void Start()
    {

    }

    bool once;


    //void Update()
    //{
    //if (Input.GetKeyDown(KeyCode.Alpha1))
    public void Day1()
    {
        //t.pipeName = "Galaxy";
        //t.treePipeline = t.assetBundle.LoadAsset<Pipeline>("Galaxy");
        //t.selectedTreeSetting = t.treeStores[5].treeSettings;
        // 1일차 기본 세팅
        t.PipelineSetting(0);
        // 씨앗심기
        t.SetTree(1);
    }
    public void Day2()
    {
        t.SetTree(2);
    }
    public void Day3()
    {
        t.SetTree(3);
    }
    public void Day4()
    {
        t.SetTree(4);
        goodFall.Play();
    }
    public void Day5()
    {
        t.SetTree(5);
        goodFall.Play();
    }


    // Sky
    public void Sunny()
    {
        w.Sunny();
    }
    public void Rainy()
    {
        w.Rain();
    }
    public void Snowy()
    {
        w.Snow();
    }


    // Weather
    public void Day()
    //if (Input.GetKeyDown(KeyCode.Keypad4))
    {
        s.Day();
    }
    public void Sunset()
    //if (Input.GetKeyDown(KeyCode.Keypad5))
    {
        s.Sunset();
    }
    public void Night()
    //if (Input.GetKeyDown(KeyCode.Keypad6))
    {
        s.Night();
    }

    public void GoodFall()
    {
        goodFall.Play();
    }
    public void BadFall()
    {
        badFall.Play();
    }


    // Bad Change
    int num = 0;
    //if (Input.GetKeyDown(KeyCode.KeypadMinus))
    public void BadTree()
    {

        if (num == 0)
        {
            goodFall.Stop();
            badFall.Play();
            num++;
            SproutSeed sproutSeed = new SproutSeed();
            t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
            t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[1].groupId = 5;
            t.treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes[4].width = 1.9f;

            // 길이 조정
            for (int i = 0; i < 4; i++)
            {
                StructureGenerator.StructureLevel branchPipe = t.treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                branchPipe.maxLengthAtBase -= 2;
                branchPipe.maxLengthAtTop -= 2;
            }


        }
        else if (num == 1)
        {
            num++;
            SproutSeed sproutSeed = new SproutSeed();
            t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
            t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[2].groupId = 6;
            t.treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes[5].width = 1.9f;

            // 길이 조정
            for (int i = 0; i < 4; i++)
            {
                StructureGenerator.StructureLevel branchPipe = t.treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                branchPipe.maxLengthAtBase -= 2;
                branchPipe.maxLengthAtTop -= 2;
            }
            t.treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[2].enabled = false;
        }
        else if (num == 2)
        {
            num++;
            SproutSeed sproutSeed = new SproutSeed();
            t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
            t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[3].groupId = 7;
            t.treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes[6].width = 1.9f;
            t.treePipeline._serializedPipeline.sproutMeshGenerators[0].sproutMeshes[7].width = 1.9f;

            // 길이 조정
            for (int i = 0; i < 4; i++)
            {
                StructureGenerator.StructureLevel branchPipe = t.treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                branchPipe.maxLengthAtBase -= 2;
                branchPipe.maxLengthAtTop -= 2;
            }

            t.treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[1].enabled = false;
        }
        t.OnDemoBadChange();

    }



    //}


}
