using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FlipCard : MonoBehaviour
{
    private GameObject cardBack;
    private GameObject cardFront;
    private bool cardBackIsActive;
    private int timer;
    private void Awake()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<Button>().onClick.AddListener(
                () => OnButtonClickFlipCard());
        }
    }
    private void Start()
    {
        cardBack = transform.GetChild(1).gameObject;
        cardFront = transform.GetChild(0).gameObject;
    }
    public void StartFlip()
    {
        StopAllCoroutines();
        StartCoroutine(CalcFlip());
    }


    public void OnButtonClickFlipCard()
    {
        StartFlip();
    }
    public void Flip()
    {
        if (cardBackIsActive)
        {
            cardBackIsActive = false;
            cardBack.SetActive(false);
            cardFront.SetActive(true);
        }
        else
        {
            cardBackIsActive = true;
            cardBack.SetActive(true);
            cardFront.SetActive(false);
        }
    }
    public IEnumerator CalcFlip()
    {
        for(int i = 0; i < 180; i++)
        {
            yield return new WaitForSeconds(0.000001f);
            transform.Rotate(new Vector3(0,3,0));
            timer++;

            if (timer == 90 || timer == -90)
                Flip();
        }
        timer = 0;
    }
}
