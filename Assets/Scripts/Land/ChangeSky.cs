using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSky : MonoBehaviour
{
        public Material daySky;
        public Material nightSky;
        
        public void Day()
        {
                RenderSettings.skybox = daySky;
        }

        public void Night()
        {
                RenderSettings.skybox = nightSky;
        }
}
