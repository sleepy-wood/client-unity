using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Falling_Leaf : MonoBehaviour
{

    public Sprite[] Leaf;
    private ParticleSystem Falling_leaf;
    // Start is called before the first frame update
    void Start()
    {
        Falling_leaf = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {              
          
                Falling_leaf.Play();        
          
        }
        if (Input.GetKeyDown(KeyCode.R))
        {

            Falling_leaf.Stop();

        }
        if (Input.GetKeyDown("1"))
        {
            Falling_leaf.textureSheetAnimation.SetSprite(0,Leaf[0]);

        }
        if (Input.GetKeyDown("2"))
        {
            Falling_leaf.textureSheetAnimation.SetSprite(1, Leaf[1]);

        }
        if (Input.GetKeyDown("3"))
        {
            Falling_leaf.textureSheetAnimation.SetSprite(2, Leaf[2]);

        }
        if (Input.GetKeyDown("4"))
        {
            Falling_leaf.textureSheetAnimation.SetSprite(3, Leaf[3]);

        }
        if (Input.GetKeyDown("5"))
        {
            Falling_leaf.textureSheetAnimation.SetSprite(4, Leaf[4]);

        }



    }
}
