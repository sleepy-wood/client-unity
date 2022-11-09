using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class XRText : Text
{
    //크기가 변경되었을 대 호출되는 함수를 가지느 변수
    public Action onChangedSize;
    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        if (onChangedSize != null)
        {
            onChangedSize();
        }
    }
    public override void CalculateLayoutInputVertical()
    {
        base.CalculateLayoutInputVertical();
        if (onChangedSize != null)
        {
            onChangedSize();
        }
    }
}
