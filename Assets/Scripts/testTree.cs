using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Broccoli.Factory;
using Broccoli.Pipe;

public class testTree : MonoBehaviour
{

    bool once;
    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1) && !once)
        {
            once = true;
            string pipeName = "Lava";
            Pipeline treePipeline = DataTemporary.assetBundleTreePipeline.LoadAsset<Pipeline>(pipeName);

            string name = "Tree_Galax";
            Material mat = DataTemporary.treeBarkAssetBundle.LoadAsset<Material>(name);
            treePipeline._serializedPipeline.barkMappers[0].customMaterial = mat;

            Pipeline loadedPipeline = DataTemporary.assetBundleTreePipeline.LoadAsset<Pipeline>(pipeName);
            TreeFactory factory = GetComponent<TreeFactory>();
            factory.LoadPipeline(loadedPipeline.Clone(), true);
            factory.UnloadAndClearPipeline();
            Resources.UnloadAsset(loadedPipeline);
        }
    }
}
