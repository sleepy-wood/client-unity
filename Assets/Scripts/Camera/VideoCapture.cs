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
    private float startTime = 0;
    private long amountAudioFrame = 0;

    void Start()
    {
        texture = GetComponent<Camera>().targetTexture;
        // with no audio
        // Application.temporaryCachePath (IOS) = %ProvisioningProfile%/Library/Caches
        cachePath = "file://" + Application.temporaryCachePath + "/tmp.mov";
        Debug.Log($"cachePath: {cachePath}, {texture.width}x{texture.height}");
    }


    //public void VideoCaptureStart()
    //{
    //    if (!isRecording || !MediaCreator.IsRecording()) return;

    //    long time = (long)((Time.time - startTime) * 1_000_000) + startTimeOffset;

    //    Debug.Log($"write texture: {time}");

    //    MediaCreator.WriteVideo(texture, time);
    //}

    public void StartRecMovWithNoAudio()
    {
        if (isRecording) return;

        MediaCreator.InitAsMovWithNoAudio(cachePath, "h264", texture.width, texture.height);
        MediaCreator.Start(startTimeOffset);

        startTime = Time.time;

        isRecording = true;
        recordAudio = false;
    }

    public void StopRec()
    {
        if (!isRecording) return;

        //Microphone.End(null);

        MediaCreator.FinishSync();
        MediaSaver.SaveVideo(cachePath);

        isRecording = false;
    }
}
