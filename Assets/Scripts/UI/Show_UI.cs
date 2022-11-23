using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Show_UI : MonoBehaviour
{
    [SerializeField] private GameObject show_UICanvas;
    private int activeCnt = 0;
    private void Start()
    {
        show_UICanvas.SetActive(false);
    }
    public void OnClickShowButton()
    {
        activeCnt++;
        if(activeCnt >= 3)
        {
            show_UICanvas.SetActive(true);
        }
        else if( activeCnt >= 6)
        {
            show_UICanvas.SetActive(false);
            activeCnt = 0;
        }
    }
}
