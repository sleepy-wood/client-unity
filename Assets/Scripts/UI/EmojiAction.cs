using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmojiAction : MonoBehaviour
{
    private float activeTime = 3;
    private float curTime = 0;
    private void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > activeTime)
        {
            Color color = GetComponent<Image>().color;
            Color color1 = transform.GetChild(0).GetComponent<Image>().color;
            color.a = Mathf.Lerp(color.a, 0, Time.deltaTime * 3);
            color1.a = Mathf.Lerp(color1.a, 0, Time.deltaTime * 3);
            GetComponent<Image>().color = color;
            transform.GetChild(0).GetComponent<Image>().color = color1;

            if (GetComponent<Image>().color.a < 0.01f)
                Destroy(gameObject);
        }
    }
}
