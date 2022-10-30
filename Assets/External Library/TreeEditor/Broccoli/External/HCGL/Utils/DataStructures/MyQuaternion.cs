using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Broccoli.HCGL
{
    //This will use Unity's Quaternion (which uses Vector3), but input and output will be MyVector3
    //This so we don't have to write our custom Quaternion class
    public struct MyQuaternion
    {
        private Quaternion unityQuaternion;
    
        public MyQuaternion(MyVector3 forward)
        {
            this.unityQuaternion = Quaternion.LookRotation(MyVector3.ToVector3(forward));
        }

        public MyQuaternion(MyVector3 forward, MyVector3 up)
        {
            this.unityQuaternion = Quaternion.LookRotation(MyVector3.ToVector3(forward), MyVector3.ToVector3(up));
        }

        public MyQuaternion(Quaternion quaternion)
        {
            this.unityQuaternion = quaternion;
        }



        //
        // Quaternion operations
        //

        //Rotate a quaternion some degrees around some axis
        public static MyQuaternion RotateQuaternion(MyQuaternion oldQuaternion, float angleInDegrees, MyVector3 rotationAxis)
        {        
            Quaternion rotationQuaternion = Quaternion.AngleAxis(angleInDegrees, MyVector3.ToVector3(rotationAxis));

            //To rotate a quaternion you just multiply it with the rotation quaternion
            //Important that rotationQuaternion is first!
            Quaternion newQuaternion = rotationQuaternion * oldQuaternion.unityQuaternion;

            MyQuaternion myNewQuaternion = new MyQuaternion(newQuaternion);

            return myNewQuaternion;
        }



        //Rotate a vector by using a quaternion
        public static MyVector3 RotateVector(MyQuaternion quat, MyVector3 vec)
        {
            Vector3 rotatedVec = quat.unityQuaternion * MyVector3.ToVector3(vec);

            return MyVector3.ToMyVector3(rotatedVec);
        }

        public MyVector3 RotateVector(MyVector3 vec)
        {
            Vector3 rotatedVec = unityQuaternion * MyVector3.ToVector3(vec);

            return MyVector3.ToMyVector3(rotatedVec);
        }



        //
        // Get directions from orientation
        //

        //If you multiply orientation with direction vector you will rotate the direction
        public MyVector3 Forward => MyVector3.ToMyVector3 (unityQuaternion * Vector3.forward);
        public MyVector3 Right   => MyVector3.ToMyVector3 (unityQuaternion * Vector3.right);
        public MyVector3 Up      => MyVector3.ToMyVector3 (unityQuaternion * Vector3.up);
    }
}
