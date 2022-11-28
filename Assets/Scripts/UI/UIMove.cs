using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMove : MonoBehaviour
{
    [SerializeField] private float speed = 1;
    private void Update()
    {
        float y = speed * Mathf.Sin(Time.time *2 );
        transform.localPosition = new Vector3(transform.localPosition.x, 2+y, transform.localPosition.z);
    }
}
