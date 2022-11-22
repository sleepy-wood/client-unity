using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UpdateCustom : MonoBehaviour
{
    [SerializeField] private GameObject Update_Canvas;
    [SerializeField] private RectTransform refresh_BTN;
    [SerializeField] private GameObject uI_LandCustom;
    List<ResultGet<MarketData>> marketsData = new List<ResultGet<MarketData>>();
    public async void OnClickUpdate()
    {
        StartCoroutine(Move_RefreshBTN());

        for (int i = 0; i < 8; i++)
        {
            ResultGet<MarketData> marketData = await DataModule.WebRequestBuffer<ResultGet<MarketData>>("/api/v1/orders?category=" + (Category)i, DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
            marketsData.Add(marketData);
        }

        for (int h = 0; h < marketsData.Count; h++)
        {
            if (h != (int)Category.emoticon && h != (int)Category.collection)
            {
                if (marketsData[h].result)
                {

#if UNITY_STANDALONE
                    string path = Application.dataPath + "/MarketImg/" + (Category)h;
#elif UNITY_IOS || UNITY_ANDROID
            string path = Application.persistentDataPath + "/MarketImg/" + (Category)h;
#endif
                    if (marketsData[h].data.Count == 0)
                    {
                        continue;
                    }
                    
                    int l = 0;
                    for (int i = 0; i < marketsData[h].data.Count; i++)
                    {
                        for (int j = 0; j < marketsData[h].data[i].orderDetails.Count; j++)
                        {
                            List<ProductImages> productImages = new List<ProductImages>();
                            productImages = marketsData[h].data[i].orderDetails[j].product.productImages;
                            for (int k = 0; k < productImages.Count; k++)
                            {
                                if (productImages[k].mimeType.Split('/')[0] == "image")
                                {
                                    l++;  
                                }
                            }
                        }
                    }

                    string[] fileEntries = Directory.GetFiles(path, "*.png");
                    if(l > fileEntries.Length)
                    {
                        //업데이트 내용이 있음
                        Update_Canvas.transform.GetChild(0).gameObject.SetActive(true);
                        Update_Canvas.transform.GetChild(1).gameObject.SetActive(false);
                        break;
                    }
                    else
                    {
                        //업데이트 내용이 없음
                        Update_Canvas.transform.GetChild(0).gameObject.SetActive(false);
                        Update_Canvas.transform.GetChild(1).gameObject.SetActive(true);
                    }
                }
            }
        }
    }
    public async void OnClickUpdateStart()
    {
        for (int h = 0; h < marketsData.Count; h++)
        {
            if (h != (int)Category.emoticon && h != (int)Category.collection)
            {

                if (marketsData[h].result)
                {
#if UNITY_STANDALONE
                    string path = Application.dataPath + "/MarketImg/" + (Category)h;
#elif UNITY_IOS || UNITY_ANDROID
            string path = Application.persistentDataPath + "/MarketImg/" + (Category)h;
#endif
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    if (marketsData[h].data.Count == 0)
                    {
                        continue;
                    }

                    for (int i = 0; i < marketsData[h].data.Count; i++)
                    {
                        for (int j = 0; j < marketsData[h].data[i].orderDetails.Count; j++)
                        {
                            List<ProductImages> productImages = new List<ProductImages>();
                            productImages = marketsData[h].data[i].orderDetails[j].product.productImages;
                            for (int k = 0; k < productImages.Count; k++)
                            {
                                if (productImages[k].mimeType.Split('/')[0] == "image")
                                {
                                    Texture2D texture = await DataModule.WebrequestTextureGet(productImages[k].path, DataModule.NetworkType.GET);
                                    byte[] bytes = texture.EncodeToPNG();
                                    File.WriteAllBytes(path + "/Market_" + productImages[k].originalName, bytes);
                                }
                                else
                                {
                                    await DataModule.WebRequestAssetBundle(productImages[k].path, DataModule.NetworkType.GET, DataModule.DataType.ASSETBUNDLE, (Category)h + "/" + productImages[k].originalName);
                                }
                            }
                        }
                    }
                }
            }
        }
        Update_Canvas.transform.GetChild(0).gameObject.SetActive(false);
        Update_Canvas.transform.GetChild(1).gameObject.SetActive(true);
        
        StartCoroutine(Custom_Update());
    }
    private IEnumerator Custom_Update()
    {
        uI_LandCustom.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        uI_LandCustom.SetActive(true);
    }
    private IEnumerator Move_RefreshBTN()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            refresh_BTN.eulerAngles = Vector3.Lerp(refresh_BTN.eulerAngles, new Vector3(0, 0, 360), t);
            yield return null;
        }
        refresh_BTN.eulerAngles = Vector3.zero;
    }
    public void OnClickCancel()
    {
        Update_Canvas.transform.GetChild(0).gameObject.SetActive(false);
        Update_Canvas.transform.GetChild(1).gameObject.SetActive(false);
    }
}
