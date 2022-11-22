using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class UpdateEmoticon : MonoBehaviour
{
    ResultGet<MarketData> marketData = new ResultGet<MarketData>();
    private void Update()
    {
        
    }
    public async void onClickUpdate()
    {
        //마켓에서 산 것이 있으면 다운로드
        marketData = await DataModule.WebRequestBuffer<ResultGet<MarketData>>("/api/v1/orders?category=" + Category.emoticon, DataModule.NetworkType.GET, DataModule.DataType.BUFFER);
        if (marketData.result)
        {
            //Debug.Log(marketData.result);
            ArrayMarketData arrayMarket = new ArrayMarketData();
            arrayMarket.marketData = marketData.data;
            DataTemporary.arrayMarketData = arrayMarket;
            //이모지 다운로드
            int l = 0;
            for (int i = 0; i < marketData.data.Count; i++)
            {
                for (int j = 0; j < marketData.data[i].orderDetails.Count; j++)
                {
                    List<ProductImages> productImages = new List<ProductImages>();
                    productImages = marketData.data[i].orderDetails[j].product.productImages;
                    for (int k = 0; k < productImages.Count - 1; k++)
                    {
                        l++;
                    }
                }
            }
            if (l > DataTemporary.emoji_Url.Count)
            {
                //업데이트 내용이 있음
            }
            else
            {
                //업데이트 내용이 없음
            }
        }
    }
    public async void OnClickUpdateStart()
    {

#if UNITY_STANDALONE
        string path = Application.dataPath + "/TextureImg";
#elif UNITY_IOS || UNITY_ANDROID
            string path = Application.persistentDataPath + "/TextureImg";
#endif
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        List<string> emoji_urls = new List<string>();
        int l = 0;
        for (int i = 0; i < marketData.data.Count; i++)
        {
            for (int j = 0; j < marketData.data[i].orderDetails.Count; j++)
            {
                List<ProductImages> productImages = new List<ProductImages>();
                productImages = marketData.data[i].orderDetails[j].product.productImages;
                for (int k = 0; k < productImages.Count - 1; k++)
                {
                    emoji_urls.Add(productImages[k].path);
                    Texture2D texture = await DataModule.WebrequestTextureGet(productImages[k].path, DataModule.NetworkType.GET);
                    byte[] bytes = texture.EncodeToPNG();
                    File.WriteAllBytes(path + "/Market_Emoji_" + l + ".png", bytes);
                    l++;
                }
            }
        }
        DataTemporary.emoji_Url = emoji_urls;
    }
}
