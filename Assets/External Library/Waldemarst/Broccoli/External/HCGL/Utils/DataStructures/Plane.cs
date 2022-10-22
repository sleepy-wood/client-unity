using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Broccoli.HCGL
{
    //3D
    public class Plane3
    {
        public MyVector3 pos;

        public MyVector3 normal;


        public Plane3(MyVector3 pos, MyVector3 normal)
        {
            this.pos = pos;

            this.normal = normal;
        }


        //p1-p2-p3 should be ordered clock-wise
        public Plane3(MyVector3 p1, MyVector3 p2, MyVector3 p3)
        {
            this.pos = p1;

            MyVector3 normal = _Geometry.CalculateTriangleNormal(p1, p2, p3);

            this.normal = normal;
        }
    }



    //Oriented plane which is needed if we want to transform between coordinate systems
    public class OrientedPlane3
    {
        public Transform planeTrans;

        public OrientedPlane3(Transform planeTrans)
        {
            this.planeTrans = planeTrans;
        }

        public Plane3 Plane3 => new Plane3(Position, Normal);

        public MyVector3 Position => MyVector3.ToMyVector3(planeTrans.position);

        public MyVector3 Normal => MyVector3.ToMyVector3(planeTrans.up);
    }



    //2D
    public class Plane2
    {
        public MyVector2 pos;

        public MyVector2 normal;


        public Plane2(MyVector2 pos, MyVector2 normal)
        {
            this.pos = pos;

            this.normal = normal;
        }
    }
}
