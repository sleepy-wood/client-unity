using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cine_Dolly : MonoBehaviour
{
    public bool isInit;

    private void Update()
    {
        if (isInit)
        {
            GetComponent<CinemachineDollyCart>().m_Position = 0;
            isInit = false;
        }
    }
}
