using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cine_TreeMove : MonoBehaviour
{
    [SerializeField] private float speed = 1;

    void Update()
    {
        float y = speed * Mathf.Sin(Time.time);

        transform.position = new Vector3(transform.position.x, transform.position.y +  y, transform.position.z);
        if(y<=0.001f && y >= -0.001f)
        {
            speed = Random.Range(0.001f, 0.005f);
        }
    }
}
