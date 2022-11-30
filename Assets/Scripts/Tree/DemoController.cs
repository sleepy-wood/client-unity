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
            t.treePipeline = t.assetBundle.LoadAsset<Pipeline>("Galaxy");
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
            goodFall.Play();
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


        // Bad Change
        int num = 0;
        if (Input.GetKeyDown(KeyCode.B))
        {
            t.OnDemoBadChange();
            if (num == 0)
            {
                goodFall.Stop();
                badFall.Play();
                num++;
                SproutSeed sproutSeed = new SproutSeed();
                t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
                t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[1].groupId = 5;

                // 길이 조정
                for (int i = 0; i < 4; i++)
                {
                    StructureGenerator.StructureLevel branchPipe = t.treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                    branchPipe.maxLengthAtBase -= 7;
                    branchPipe.maxLengthAtTop -= 7;
                }

            }
            else if (num == 1)
            {
                num++;
                SproutSeed sproutSeed = new SproutSeed();
                t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
                t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[2].groupId = 6;

                // 길이 조정
                for (int i = 0; i < 4; i++)
                {
                    StructureGenerator.StructureLevel branchPipe = t.treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                    branchPipe.maxLengthAtBase -= 7;
                    branchPipe.maxLengthAtTop -= 7;
                }
            }
            else if (num == 2)
            {
                num++;
                SproutSeed sproutSeed = new SproutSeed();
                t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds.Add(sproutSeed);
                t.treePipeline._serializedPipeline.sproutGenerators[0].sproutSeeds[3].groupId = 7;

                // 길이 조정
                for (int i = 0; i < 4; i++)
                {
                    StructureGenerator.StructureLevel branchPipe = t.treePipeline._serializedPipeline.structureGenerators[0].flatStructureLevels[i];
                    branchPipe.maxLengthAtBase -= 7;
                    branchPipe.maxLengthAtTop -= 7;
                }
            }
            
        }



    }


}
