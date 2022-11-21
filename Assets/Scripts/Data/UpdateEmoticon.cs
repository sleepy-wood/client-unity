using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateEmoticon : MonoBehaviour
{
    public void onClickUpdate()
    {
//        for (int h = 0; h < marketsData.Count; h++)
//        {
//            if (h == (int)Category.emoticon)
//            {
//                if (marketsData[h].result)
//                {

//#if UNITY_STANDALONE
//                    string path = Application.dataPath + "/TextureImg";
//#elif UNITY_IOS || UNITY_ANDROID
//            string path = Application.persistentDataPath + "/TextureImg";
//#endif
//                    if (!Directory.Exists(path))
//                    {
//                        Directory.CreateDirectory(path);
//                    }

//                    //Debug.Log(marketData.result);
//                    ArrayMarketData arrayMarket = new ArrayMarketData();
//                    arrayMarket.marketData = marketsData[h].data;
//                    DataTemporary.arrayMarketData = arrayMarket;
//                    //이모지 다운로드
//                    int l = 0;
//                    for (int i = 0; i < marketsData[h].data.Count; i++)
//                    {
//                        for (int j = 0; j < marketsData[h].data[i].orderDetails.Count; j++)
//                        {
//                            List<string> emoji_urls = new List<string>();
//                            List<ProductImages> productImages = new List<ProductImages>();
//                            productImages = marketsData[h].data[i].orderDetails[j].product.productImages;
//                            for (int k = 0; k < productImages.Count - 1; k++)
//                            {
//                                emoji_urls.Add(productImages[k].path);
//                                DataTemporary.emoji_Url.Add(productImages[k].path);
//                                Texture2D texture = await DataModule.WebrequestTextureGet(productImages[k].path, DataModule.NetworkType.GET);
//                                byte[] bytes = texture.EncodeToPNG();
//                                File.WriteAllBytes(path + "/Market_Emoji_" + l + ".png", bytes);
//                                l++;
//                            }
//                            //DataTemporary.market_url.Add(emoji_urls);
//                        }
//                    }
//                }
//            }
        //}
    }
}
