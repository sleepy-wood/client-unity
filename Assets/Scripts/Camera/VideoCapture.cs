using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VideoCreator;

public class VideoCapture : MonoBehaviour
{
    [SerializeField]
    private RenderTexture texture;

    private readonly long startTimeOffset = 6_000_000;

    private bool isRecording = false;
    private bool recordAudio = false;

    private string cachePath = "";
    private string cachePath2 = "";
    private float startTime = 0;

    void Start()
    {
        texture = GetComponent<Camera>().targetTexture;
        // with no audio
        // Application.temporaryCachePath (IOS) = %ProvisioningProfile%/Library/Caches
        cachePath = "file://" + Application.temporaryCachePath + "/tmp.mov";
        cachePath2 = $"{Application.persistentDataPath}/ScreenShot/Video/Video_{GameManager.Instance.treeController.treeId}.mov";
        print($"cachePath: {cachePath}");
        print($"cachePath2 : {cachePath2}");
    }


    bool once = false;
    bool once2 = false;
    void Update()
    {
        if (!isRecording || !MediaCreator.IsRecording()) return;

        long time = (long)((Time.time - startTime) * 1_000_000) + startTimeOffset;

        Debug.Log($"write texture: {time}");

        MediaCreator.WriteVideo(texture, time);

        if(Input.GetKeyDown(KeyCode.Alpha1)&&!once)
        {
            once = true;
            StartRecMovWithNoAudio();
            print("Start");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && !once2)
        {
            once2 = true;
            StopRec();
            print("Stop");
        }

    }

    public void StartRecMovWithNoAudio()
    {
        if (isRecording) return;

        MediaCreator.InitAsMovWithNoAudio(cachePath, "h264", texture.width, texture.height);
        MediaCreator.Start(startTimeOffset);

        startTime = Time.time;

        isRecording = true;
        recordAudio = false;

        print("Start Recording");
    }

    public void StopRec()
    {
        if (!isRecording) return;

        MediaCreator.FinishSync();
        MediaSaver.SaveVideo(cachePath);
        MediaSaver.SaveVideo(cachePath2);

        isRecording = false;

        print("Stop Recording");
    }
}
