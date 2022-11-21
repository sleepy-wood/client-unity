using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<CapsuleCollider>().enabled = false;
    }
}
