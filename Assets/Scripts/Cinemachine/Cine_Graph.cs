using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cine_Graph : MonoBehaviour
{
    [SerializeField] private float max;
    [SerializeField] private float min;

    public bool isStartGraph = false;

    float[] rand = new float[5];
    private void Start()
    {
        for (int i = 0; i < rand.Length; i++)
        {
            rand[i] = Random.Range(min, max) / 10;
        }
    
    }
    private void Update()
    {
        if (isStartGraph)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Vector3 value = transform.GetChild(i).localPosition;
                value.y += rand[i] / 2;
                Vector3 value_scale = transform.GetChild(i).localScale;
                value_scale.y += rand[i];
                StartCoroutine(GraphMove(value_scale, value, i));
            }
        }
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Vector3 value = transform.GetChild(i).localPosition;
                value.y = 0;
                transform.GetChild(i).localPosition = value;
                transform.GetChild(i).localScale = Vector3.one;
            }
        }
    }

    IEnumerator GraphMove(Vector3 value_scale, Vector3 value, int idx)
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.1f;
            transform.GetChild(idx).localPosition = Vector3.Lerp(transform.GetChild(idx).localPosition, value, t);
            transform.GetChild(idx).localScale = Vector3.Lerp(transform.GetChild(idx).localScale, value_scale, t);
            yield return null;
        }
    }
}
