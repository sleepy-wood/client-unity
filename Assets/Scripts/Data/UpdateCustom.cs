using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UpdateCustom : MonoBehaviour
{
    List<ResultGet<MarketData>> marketsData = new List<ResultGet<MarketData>>();
    public async void OnClickUpdate()
    {
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
                        break;
                    }
                    else
                    {
                        //업데이트 내용이 없음
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
    }
}
