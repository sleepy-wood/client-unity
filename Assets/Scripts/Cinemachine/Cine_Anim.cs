using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cine_Anim : MonoBehaviour
{
    Animator anim;
    public bool isNext;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        if (isNext)
            anim.SetTrigger("Next");
    }
}
