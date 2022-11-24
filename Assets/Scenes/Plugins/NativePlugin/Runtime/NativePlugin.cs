using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NativePlugin
{
    public class HelloTest
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void iOS_runHello();
#endif

        public static void RunHello()
        {
#if UNITY_IOS
            iOS_runHello();
#else
            Debug.Log("RunHello: Unsupported Platform");
#endif
        }
    }
}
