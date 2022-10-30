using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Broccoli.HCGL
{
    //Unity loves to automatically cast beween Vector2 and Vector3
    //Because theres no way to stop it, its better to use a custom struct 
    [System.Serializable]
    public struct MyVector2
    {
        #region Vars
        public float x;
        public float y;
        public int index;
        public int polygon;
        #endregion


        #region Casting
        //Vector3 - MyVector2
        public static MyVector2 ToMyVector2(Vector3 v)
        {
            return new MyVector2(v.x, v.z);
        }

        public static MyVector2 ToMyVector2ZY(Vector3 v)
        {
            return new MyVector2(v.z, v.y);
        }

        //Vector2 - MyVector2
        public static MyVector2 ToMyVector2(Vector2 v)
        {
            return new MyVector2(v.x, v.y);
        }
        #endregion

        #region Constructor
        public MyVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.index = -1;
            this.polygon = -1;
        }
        #endregion

        #region Vector Ops
        //
        // To make vector operations easier
        //

        //Test if this vector is approximately the same as another vector
        public bool Equals(MyVector2 other)
        {
            //Using Mathf.Approximately() is not accurate enough
            //Using Mathf.Abs is slow because Abs involves a root

            float xDiff = this.x - other.x;
            float yDiff = this.y - other.y;

            float e = MathUtility.EPSILON;

            //If all of the differences are around 0
            if (
                xDiff < e && xDiff > -e && 
                yDiff < e && yDiff > -e)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        //Vector operations
        public static float Dot(MyVector2 a, MyVector2 b)
        {
            float dotProduct = (a.x * b.x) + (a.y * b.y);

            return dotProduct;
        }

        // Length of vector a: ||a||
        public static float Magnitude(MyVector2 a)
        {
            float magnitude = Mathf.Sqrt(SqrMagnitude(a));

            return magnitude;
        }

        public static float SqrMagnitude(MyVector2 a)
        {
            float sqrMagnitude = (a.x * a.x) + (a.y * a.y);

            return sqrMagnitude;
        }

        public static float Distance(MyVector2 a, MyVector2 b)
        {
            float distance = Magnitude(a - b);

            return distance;
        }

        public static float SqrDistance(MyVector2 a, MyVector2 b)
        {
            float distance = SqrMagnitude(a - b);

            return distance;
        }

        public static MyVector2 Normalize(MyVector2 v)
        {
            float v_magnitude = Magnitude(v);

            MyVector2 v_normalized = new MyVector2(v.x / v_magnitude, v.y / v_magnitude);

            return v_normalized;
        }
        #endregion

        #region Overloads Ops
        //Operator overloads
        public static MyVector2 operator +(MyVector2 a, MyVector2 b)
        {
            return new MyVector2(a.x + b.x, a.y + b.y);
        }

        public static MyVector2 operator -(MyVector2 a, MyVector2 b)
        {
            return new MyVector2(a.x - b.x, a.y - b.y);
        }

        public static MyVector2 operator *(MyVector2 a, float b)
        {
            return new MyVector2(a.x * b, a.y * b);
        }

        public static MyVector2 operator *(float b, MyVector2 a)
        {
            return new MyVector2(a.x * b, a.y * b);
        }

        public static MyVector2 operator -(MyVector2 a)
        {
            return a * -1f;
        }
        #endregion
    }
}
