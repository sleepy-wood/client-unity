using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GraphUI_Manage : MonoBehaviour
{
    [SerializeField] private GameObject init_Window;
    [SerializeField] private GameObject sleep_Record;
    public void OnClickActive_Graph()
    {
        init_Window.SetActive(true);
        sleep_Record.SetActive(false);
    }
    public void OnClickNotActive_Graph()
    {
        init_Window.SetActive(false);
        sleep_Record.SetActive(false);
    }

    public void OnClickActive_SleepRecord()
    {
        init_Window.SetActive(false);
        sleep_Record.SetActive(true);
    }
    public void OnClickNotActive_SleepRecord()
    {
        init_Window.SetActive(true);
        sleep_Record.SetActive(false);
    }

}
