
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

    public int Leaf_Shape = 0;
    private ParticleSystem particle;


    void Start()
    {
        particle = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// 나무 Sprout Group과 Sprout Areas Enabled로 떨어지는 잎 설정
    /// </summary>
    public void SetFallingLeafParticle(GetTreeData treeData)
    {
        // Sprout Group
        Leaf_Shape = treeData.sproutGroupId - 1;

        // Sprout Color
        if (treeData.sproutColor1 == 1)
        {
            particle.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[0]);
        }
        if (treeData.sproutColor2 == 1)
        {
            particle.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[1]);
        }
        if (treeData.sproutColor3 == 1)
        {
            particle.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[2]);
        }
        if (treeData.sproutColor4 == 1)
        {
            particle.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[3]);
        }
        if (treeData.sproutColor5 == 1)
        {
            particle.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[4]);
        }

        // Rotten Sprout
        int count = treeData.treeGrowths.Count;
        if (count > 0)
        {
            if (treeData.treeGrowths[count - 1].treePipeline.rottenRate > 0)
            {
                particle.textureSheetAnimation.AddSprite(Leaf_Rotten);
            }
        }
        
    }

    #region DR
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.P))
    //    {

    //        particle.Play();

    //    }
    //    if (Input.GetKeyDown(KeyCode.O))
    //    {

    //        particle.Stop();

    //    }
    //    ///////////////////////////////////
    //    // 잎의 모양 = Group Id
    //    if (Input.GetKeyDown(KeyCode.A))
    //    {
    //        Leaf_Shape = 0;
    //    }
    //    if (Input.GetKeyDown(KeyCode.B))
    //    {
    //        Leaf_Shape = 1;
    //    }
    //    if (Input.GetKeyDown(KeyCode.C))
    //    {
    //        Leaf_Shape = 2;
    //    }
    //    if (Input.GetKeyDown(KeyCode.D))
    //    {
    //        Leaf_Shape = 3;
    //    }

    //    /////////////////////////////////
    //    // 잎의 색깔 = Sprout Color
    //    if (Input.GetKeyDown("1"))
    //    {

    //        particle.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[0]);
    //    }
    //    if (Input.GetKeyDown("2"))
    //    {
    //        particle.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[1]);

    //    }
    //    if (Input.GetKeyDown("3"))
    //    {
    //        particle.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[2]);

    //    }
    //    if (Input.GetKeyDown("4"))
    //    {
    //        particle.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[3]);

    //    }
    //    if (Input.GetKeyDown("5"))
    //    {
    //        particle.textureSheetAnimation.AddSprite(Leaf[Leaf_Shape].Color[4]);

    //    }
    //    if (Input.GetKeyDown("6"))
    //    {
    //        particle.textureSheetAnimation.AddSprite(Leaf_Rotten);


    //    }
    //    if (Input.GetKeyDown("7"))
    //    {
    //        particle.textureSheetAnimation.RemoveSprite(5);
    //    }
        #endregion
}
