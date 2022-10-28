using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingData : MonoBehaviour
{
    NativeLoadData nativeLoad = new NativeLoadData();
    private void Start()
    {
        //Native Data Load
        nativeLoad.LoadNativeData();    
        //UserData Load
        
        //LandData Load

        //AssetBundle Load

        //TreeData Load

    }
}
