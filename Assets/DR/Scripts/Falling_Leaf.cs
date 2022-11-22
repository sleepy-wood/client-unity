
using System; /* Serializable */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Falling_Leaf : MonoBehaviour
{


    [Serializable]
    public class Array_2
    {
        public Sprite[] Color = new Sprite[5];
    }

    public Array_2[] Leaf = new Array_2[4];
    public Sprite Leaf_Rotten;

    //public int[,] array = new int[3, 3];

    public int Leaf_Shape=0;
    
    private ParticleSystem Falling_leaf;
    // Start is called before the first frame update
    void Start()
    {
        Falling_leaf = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {              
          
            Falling_leaf.Play();        
          
        }
        if (Input.GetKeyDown(KeyCode.O))
        {

            Falling_leaf.Stop();

        }
        ///////////////////////////////////
        if (Input.GetKeyDown(KeyCode.A))
        {
            Leaf_Shape = 0;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Leaf_Shape = 1;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Leaf_Shape = 2;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Leaf_Shape = 3;
        }
        
        /////////////////////////////////
        if (Input.GetKeyDown("1"))
        {
            
            Falling_leaf.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[0]);
        }
        if (Input.GetKeyDown("2"))
        {
            Falling_leaf.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[1]);

        }
        if (Input.GetKeyDown("3"))
        {
            Falling_leaf.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[2]);

        }
        if (Input.GetKeyDown("4"))
        {
            Falling_leaf.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[3]);

        }
        if (Input.GetKeyDown("5"))
        {
            Falling_leaf.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[4]);

        }
        if(Input.GetKeyDown("6"))
        {
            Falling_leaf.textureSheetAnimation.AddSprite(Leaf_Rotten);
            
          
        }
        if (Input.GetKeyDown("7"))
        {
            Falling_leaf.textureSheetAnimation.RemoveSprite(5);

        }
        

    }
}
