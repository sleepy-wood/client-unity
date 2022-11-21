using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateCustom : MonoBehaviour
{
    public void OnClickUpdate()
    {
//        for (int h = 0; h < marketsData.Count; h++)
//        {
//            if (h != (int)Category.emoticon && h != (int)Category.collection)
//            {

//                if (marketsData[h].result)
//                {
//#if UNITY_STANDALONE
//                    string path = Application.dataPath + "/MarketImg/" + (Category)h;
//#elif UNITY_IOS || UNITY_ANDROID
//            string path = Application.persistentDataPath + "/MarketImg/" + (Category)h;
//#endif
//                    if (!Directory.Exists(path))
//                    {
//                        Directory.CreateDirectory(path);
//                    }

//                    if (marketsData[h].data.Count == 0)
//                    {
//                        continue;
//                    }

//                    for (int i = 0; i < marketsData[h].data.Count; i++)
//                    {
//                        for (int j = 0; j < marketsData[h].data[i].orderDetails.Count; j++)
//                        {
//                            List<ProductImages> productImages = new List<ProductImages>();
//                            productImages = marketsData[h].data[i].orderDetails[j].product.productImages;
//                            for (int k = 0; k < productImages.Count; k++)
//                            {
//                                if (productImages[k].mimeType.Split('/')[0] == "image")
//                                {
//                                    Texture2D texture = await DataModule.WebrequestTextureGet(productImages[k].path, DataModule.NetworkType.GET);
//                                    byte[] bytes = texture.EncodeToPNG();
//                                    File.WriteAllBytes(path + "/Market_" + productImages[k].originalName, bytes);
//                                }
//                                else
//                                {
//                                    await DataModule.WebRequestAssetBundle(productImages[k].path, DataModule.NetworkType.GET, DataModule.DataType.ASSETBUNDLE, (Category)h + "/" + productImages[k].originalName);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }
    }
}
