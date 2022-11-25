                                                                                                                                                             using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Ionic.Zip;
using UnityEngine.Networking;
//using ICSharpCode.SharpZipLib.Zip;


# region 비트맵
class BitmapEncoder
{
    public static void WriteBitmap(Stream stream, int width, int height, byte[] imageData)
    {
        using (BinaryWriter bw = new BinaryWriter(stream))
        {

            // define the bitmap file header
            bw.Write((UInt16)0x4D42);                               // bfType;
            bw.Write((UInt32)(14 + 40 + (width * height * 4)));     // bfSize;
            bw.Write((UInt16)0);                                    // bfReserved1;
            bw.Write((UInt16)0);                                    // bfReserved2;
            bw.Write((UInt32)14 + 40);                              // bfOffBits;

            // define the bitmap information header
            bw.Write((UInt32)40);                               // biSize;
            bw.Write((Int32)width);                                 // biWidth;
            bw.Write((Int32)height);                                // biHeight;
            bw.Write((UInt16)1);                                    // biPlanes;
            bw.Write((UInt16)32);                                   // biBitCount;
            bw.Write((UInt32)0);                                    // biCompression;
            bw.Write((UInt32)(width * height * 4));                 // biSizeImage;
            bw.Write((Int32)0);                                     // biXPelsPerMeter;
            bw.Write((Int32)0);                                     // biYPelsPerMeter;
            bw.Write((UInt32)0);                                    // biClrUsed;
            bw.Write((UInt32)0);                                    // biClrImportant;

            // switch the image data from RGB to BGR
            for (int imageIdx = 0; imageIdx < imageData.Length; imageIdx += 3)
            {
                bw.Write(imageData[imageIdx + 2]);
                bw.Write(imageData[imageIdx + 1]);
                bw.Write(imageData[imageIdx + 0]);
                bw.Write((byte)255);
            }

        }
    }

}
# endregion

/// <summary>
/// Captures frames from a Unity camera in real time
/// and writes them to disk using a background thread.
/// </summary>
/// 
/// <description>
/// Maximises speed and quality by reading-back raw
/// texture data with no conversion and writing 
/// frames in uncompressed BMP format.
/// Created by Richard Copperwaite.
/// </description>
/// 
[RequireComponent(typeof(Camera))]
public class ScreenRecorder : MonoBehaviour
{
    // Public Properties
    public int maxFrames; // maximum number of frames you want to record in one video
    public int frameRate = 30; // number of frames to capture per second

    // The Encoder Thread
    public Thread encoderThread;

    // Texture Readback Objects
    public RenderTexture tempRenderTexture;
    public Texture2D tempTexture2D;

    // Timing Data
    public float captureFrameTime;
    public float lastFrameTime;
    public int frameNumber;
    public int savingFrameNumber;

    // Encoder Thread Shared Resources
    public Queue<byte[]> frameQueue;
    public string persistentDataPath;
    public int screenWidth;
    public int screenHeight;
    public bool threadIsProcessing;
    public bool terminateThreadWhenDone;

    void Start()
    {
        // Set target frame rate (optional)
        Application.targetFrameRate = frameRate;

        // Prepare the data directory
        string path = "/ScreenRecorder";
#if UNITY_STANDALONE
        persistentDataPath = Application.dataPath + path;
#elif UNITY_IOS || UNITY_ANDROID
        persistentDataPath = Application.persistentDataPath + path;
#endif
        print("Capturing to: " + persistentDataPath + "/");

        if (!System.IO.Directory.Exists(persistentDataPath))
        {
            System.IO.Directory.CreateDirectory(persistentDataPath);
        }

        // Prepare textures and initial values
        screenWidth = GetComponent<Camera>().pixelWidth;
        screenHeight = GetComponent<Camera>().pixelHeight;

        // Render Texture
        tempRenderTexture = new RenderTexture(screenWidth, screenHeight, 0);
        tempTexture2D = new Texture2D(screenWidth, screenHeight, TextureFormat.RGB24, false);
        frameQueue = new Queue<byte[]>();

        frameNumber = 0;
        savingFrameNumber = 0;

        captureFrameTime = 1.0f / (float)frameRate;
        lastFrameTime = Time.time;

        // Kill the encoder thread if running from a previous execution
        if (encoderThread != null && (threadIsProcessing || encoderThread.IsAlive))
        {
            threadIsProcessing = false;
            encoderThread.Join();
        }
    }


    void OnDisable()
    {
        // Reset target frame rate
        Application.targetFrameRate = -1;

        // Inform thread to terminate when finished processing frames
        terminateThreadWhenDone = true;
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (frameNumber <= maxFrames)
        {
            // Check if render target size has changed, if so, terminate
            if (source.width != screenWidth || source.height != screenHeight)
            {
                threadIsProcessing = false;
                this.enabled = false;
                throw new UnityException("ScreenRecorder render target size has changed!");
            }

            // Calculate number of video frames to produce from this game frame
            // Generate 'padding' frames if desired framerate is higher than actual framerate
            float thisFrameTime = Time.time;
            int framesToCapture = ((int)(thisFrameTime / captureFrameTime)) - ((int)(lastFrameTime / captureFrameTime));

            // Capture the frame
            if (framesToCapture > 0)
            {
                Graphics.Blit(source, tempRenderTexture);

                RenderTexture.active = tempRenderTexture;
                tempTexture2D.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                RenderTexture.active = null;
            }

            // Add the required number of copies to the queue
            for (int i = 0; i < framesToCapture && frameNumber <= maxFrames; ++i)
            {
                //frameQueue.Enqueue(tempTexture2D.GetRawTextureData());
                frameQueue.Enqueue(tempTexture2D.EncodeToJPG());
                frameNumber++;

                if (frameNumber % frameRate == 0)
                {
                    print("Frame " + frameNumber);
                }
            }

            lastFrameTime = thisFrameTime;

        }
        else //keep making screenshots until it reaches the max frame amount
        {
            // Inform thread to terminate when finished processing frames
            terminateThreadWhenDone = true;

            // Disable script
            this.enabled = false;
        }

        // Passthrough
        Graphics.Blit(source, destination);
    }

    public void EncodeAndSave()
    {
        print("Tree 영상 캡처 시작");

        while (threadIsProcessing)
        {
            if (frameQueue.Count > 0)
            {
                // Generate file path
                //string path = persistentDataPath + "/frame" + savingFrameNumber + ".bmp";

                // Dequeue the frame, encode it as a bitmap, and write it to the file
                //using (FileStream fileStream = new FileStream(path, FileMode.Create))
                //{
                //    BitmapEncoder.WriteBitmap(fileStream, screenWidth, screenHeight, frameQueue.Dequeue());
                //    fileStream.Close();
                //}

                string path = persistentDataPath + "/frame" + savingFrameNumber.ToString("D4") + ".jpg";
                File.WriteAllBytes(path, frameQueue.Dequeue());

                // Done
                savingFrameNumber++;
                print("Saved " + savingFrameNumber + " frames. " + frameQueue.Count + " frames remaining.");
            }
            else
            {
                if (terminateThreadWhenDone)
                {
                    break;
                }

                Thread.Sleep(1);
            }
        }

        terminateThreadWhenDone = false;
        threadIsProcessing = false;
        print("Tree 영상 캡처 완료");

        // 압축할 이미지가 들어있는 파일 경로
#if UNITY_STANDALONE
        from = $"{Application.dataPath}/ScreenRecorder";

#elif UNITY_IOS || UNITY_ANDROID
        from = $"{Application.persistentDataPath}/ScreenRecorder";
#endif


        // 압축 대상 폴더 경로의 길이 + 1 
        int TrimLength =
               (Directory.GetParent(from)).ToString().Length + 1;
        // 압축한 파일을 저장할 파일 경로
#if UNITY_STANDALONE
        //if (!System.IO.Directory.Exists(from))
        //{
        //    System.IO.Directory.CreateDirectory(from);
        //}
        string to = $"{Application.streamingAssetsPath}/Zipfiles";
#elif UNITY_IOS || UNITY_ANDROID
        to = Application.persistentDataPath + "/";
        //if (!System.IO.Directory.Exists(to))
        //{
        //    System.IO.Directory.CreateDirectory(to);
        //    print("1");
        //}
#endif
        // 이미지 압축
        ZipFile zip = new ZipFile();
        print("2");
        zip.AddDirectory(from);
        print("3");
        zipFilePath = $"TreeImgZip_{GameManager.Instance.treeController.treeId}.zip";
        print("4");
        zip.Save(to+zipFilePath);
        print("Zip Images");

        // 이미지 삭제
        Directory.Delete(from, true);
        print("Delete Caputure Image");

        //UploadZipFile();
    }
    public string zipFilePath;
    public string from;
    public string to;
    /// <summary>
    /// 이미지 압축 파일 웹에 업로드
    /// </summary>
    public async void UploadZipFile()
    {
        string saveUrl = "/api/v1/files/temp/image-to-video";
        List<IMultipartFormSection> treeCaptures = new List<IMultipartFormSection>();
#if UNITY_STANDALONE
        string path = $"{Application.streamingAssetsPath}/Zipfiles/TreeImgZip_{GameManager.Instance.treeController.treeId}.zip";
#elif UNITY_IOS || UNITY_ANDROID
        string path = $"{Application.streamingAssetsPath}/Zipfiles/TreeImgZip_{GameManager.Instance.treeController.treeId}.zip";
#endif
        treeCaptures.Add(new MultipartFormFileSection("files", File.ReadAllBytes(path), zipFilePath, "application/zip"));

        ResultPost<List<TreeFile>> resultPost = await DataModule.WebRequestBuffer<ResultPost<List<TreeFile>>>(
           saveUrl,
           DataModule.NetworkType.POST,
           DataModule.DataType.BUFFER,
           null,
           treeCaptures);

        if (!resultPost.result)
        {
            Debug.Log("Tree Video Zip File Upload : Fail");
            return;
        }
        else
        {
            Debug.Log("Tree Video Zip File Upload : Success");
        }
    }


    /// <summary>
    /// 파일 경로에 있는 모든 파일의 리스트 뽑기
    /// </summary>
    /// <param name="Dir"></param>
    /// <returns></returns>
    private static ArrayList GenerateFileList(string Dir)
    {
        ArrayList fils = new ArrayList();
        bool Empty = true;
        
        // 폴더 내의 파일 추가. 
        foreach (string file in Directory.GetFiles(Dir))
        {
            fils.Add(file);
            Empty = false;
        }

        //if (Empty)
        //{
        //    // 파일이 없고, 폴더도 없는 경우 자신의 폴더 추가. 
        //    if (Directory.GetDirectories(Dir).Length == 0)
        //        fils.Add(Dir + @"/");
        //}
        //// 폴더 내 폴더 목록. 
        //foreach (string dirs in Directory.GetDirectories(Dir))
        //{
        //    Debug.Log("1-4");
        //    // 해당 폴더로 다시 GenerateFileList 재귀 호출 
        //    foreach (object obj in GenerateFileList(dirs))
        //    {
        //        // 해당 폴더 내의 파일, 폴더 추가. 
        //        fils.Add(obj);
        //    }
        //}

        return fils;
    }
}
