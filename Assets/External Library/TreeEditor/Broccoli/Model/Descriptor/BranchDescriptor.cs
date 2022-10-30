using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Model;

namespace Broccoli.Pipe {
    /// <summary>
    /// Sprout group container class.
    /// </summary>
    [System.Serializable]
    public class BranchDescriptor {
        #region Branch Level Descriptor
        [System.Serializable]
        public class BranchLevelDescriptor {
            #region Vars
            public bool isEnabled = true;
            public int minFrequency = 1;
            public int maxFrequency = 1;
            public float radius = 1f;
            public float minLengthAtBase = 3f;
            public float maxLengthAtBase = 4f;
            public float minLengthAtTop = 3f;
            public float maxLengthAtTop = 4f;
            public float minParallelAlignAtTop = 0f;
            public float maxParallelAlignAtTop = 0f;
            public float minParallelAlignAtBase = 0f;
            public float maxParallelAlignAtBase = 0f;
            public float minGravityAlignAtTop = 0f;
            public float maxGravityAlignAtTop = 0f;
            public float minGravityAlignAtBase = 0f;
            public float maxGravityAlignAtBase = 0f;
            public float minPlaneAlignAtTop = 0f;
            public float maxPlaneAlignAtTop = 0f;
            public float minPlaneAlignAtBase = 0f;
            public float maxPlaneAlignAtBase = 0f;
            #endregion

            #region Clone
            public BranchLevelDescriptor Clone () {
                BranchLevelDescriptor clone = new BranchLevelDescriptor ();
                clone.isEnabled = isEnabled;
                clone.minFrequency = minFrequency;
                clone.maxFrequency = maxFrequency;
                clone.radius = radius;
                clone.minLengthAtBase = minLengthAtBase;
                clone.maxLengthAtBase = maxLengthAtBase;
                clone.minLengthAtTop = minLengthAtTop;
                clone.maxLengthAtTop = maxLengthAtTop;
                clone.minParallelAlignAtTop = minParallelAlignAtTop;
                clone.maxParallelAlignAtTop = maxParallelAlignAtTop;
                clone.minParallelAlignAtBase = minParallelAlignAtBase;
                clone.maxParallelAlignAtBase = maxParallelAlignAtBase;
                clone.minGravityAlignAtTop = minGravityAlignAtTop;
                clone.maxGravityAlignAtTop = maxGravityAlignAtTop;
                clone.minGravityAlignAtBase = minGravityAlignAtBase;
                clone.maxGravityAlignAtBase = maxGravityAlignAtBase;
                clone.minPlaneAlignAtTop = minPlaneAlignAtTop;
                clone.maxPlaneAlignAtTop = maxPlaneAlignAtTop;
                clone.minPlaneAlignAtBase = minPlaneAlignAtBase;
                clone.maxPlaneAlignAtBase = maxPlaneAlignAtBase;
                return clone;
            }
            #endregion
        }
        #endregion

        #region Sprout Level Descriptor
        [System.Serializable]
        public class SproutLevelDescriptor {
            #region Vars
            public bool isEnabled = true;
            public int minFrequency = 5;
            public int maxFrequency = 9;
            public float minParallelAlignAtTop = 0f;
            public float maxParallelAlignAtTop = 0f;
            public float minParallelAlignAtBase = 0f;
            public float maxParallelAlignAtBase = 0f;
            public float minGravityAlignAtTop = 0f;
            public float maxGravityAlignAtTop = 0f;
            public float minGravityAlignAtBase = 0f;
            public float maxGravityAlignAtBase = 0f;
            public float minRange = 0f;
            public float maxRange = 1f;
            #endregion

            #region Clone
            public SproutLevelDescriptor Clone () {
                SproutLevelDescriptor clone = new SproutLevelDescriptor ();
                clone.isEnabled = isEnabled;
                clone.minFrequency = minFrequency;
                clone.maxFrequency = maxFrequency;
                clone.minParallelAlignAtTop = minParallelAlignAtTop;
                clone.maxParallelAlignAtTop = maxParallelAlignAtTop;
                clone.minParallelAlignAtBase = minParallelAlignAtBase;
                clone.maxParallelAlignAtBase = maxParallelAlignAtBase;
                clone.minGravityAlignAtTop = minGravityAlignAtTop;
                clone.maxGravityAlignAtTop = maxGravityAlignAtTop;
                clone.minGravityAlignAtBase = minGravityAlignAtBase;
                clone.maxGravityAlignAtBase = maxGravityAlignAtBase;
                clone.minRange = minRange;
                clone.maxRange = maxRange;
                return clone;
            }
            #endregion
        }
        #endregion

        #region Structure Vars
        public int id = 0;
        public int processorId = 0;
        public int seed = 0;
        /// <summary>
        /// Selects how many branch structural levels are enabled in the branch hierarchy.
        /// Level 0 has only the main branch enabled.
        /// </summary>
        public int activeLevels = 1;
        public float girthAtBase = 0.2f;
        public float girthAtTop = 0.01f;
        public float noiseAtBase = 0.5f;
        public float noiseAtTop = 0.5f;
        public float noiseScaleAtBase = 0.75f;
        public float noiseScaleAtTop = 0.75f;
        public List<BranchLevelDescriptor> branchLevelDescriptors = new List<BranchLevelDescriptor> ();
        public float sproutASize = 1f;
        public float sproutAScaleAtBase = 1f;
        public float sproutAScaleAtTop = 1f;
        public float sproutAFlipAlign = 0.8f;
        public List<SproutLevelDescriptor> sproutALevelDescriptors = new List<SproutLevelDescriptor> ();
        public float sproutBSize = 1f;
        public float sproutBScaleAtBase = 1f;
        public float sproutBScaleAtTop = 1f;
        public float sproutBFlipAlign = 0.8f;
        public List<SproutLevelDescriptor> sproutBLevelDescriptors = new List<SproutLevelDescriptor> ();
        public List<PolygonArea> polygonAreas = new List<PolygonArea> ();
        public int lodCount = 3;
        #endregion

        #region Constructor
        public BranchDescriptor () {
            if (branchLevelDescriptors.Count == 0) {
                for (int i = 0; i < 4; i++) {
                    branchLevelDescriptors.Add (new BranchLevelDescriptor ());
                }
            }
            if (sproutALevelDescriptors.Count == 0) {
                for (int i = 0; i < 4; i++) {
                    sproutALevelDescriptors.Add (new SproutLevelDescriptor ());
                }
            }
            if (sproutBLevelDescriptors.Count == 0) {
                for (int i = 0; i < 4; i++) {
                    sproutBLevelDescriptors.Add (new SproutLevelDescriptor ());
                }
            }
        }
        #endregion

        #region Clone
        /// <summary>
        /// Clone this instance.
        /// </summary>
        public BranchDescriptor Clone () {
            BranchDescriptor clone = new BranchDescriptor ();
            clone.id = id;
            clone.processorId = processorId;
            clone.seed = seed;
            clone.activeLevels = activeLevels;
            clone.girthAtBase = girthAtBase;
            clone.girthAtTop = girthAtTop;
            clone.noiseAtBase = noiseAtBase;
            clone.noiseAtTop = noiseAtTop;
            clone.noiseScaleAtBase = noiseScaleAtBase;
            clone.noiseScaleAtTop = noiseScaleAtTop;
            clone.branchLevelDescriptors.Clear ();
            for (int i = 0; i < branchLevelDescriptors.Count; i++) {
                clone.branchLevelDescriptors.Add (branchLevelDescriptors [i].Clone ());
            }
            clone.sproutASize = sproutASize;
            clone.sproutAScaleAtBase = sproutAScaleAtBase;
            clone.sproutAScaleAtTop = sproutAScaleAtTop;
            clone.sproutAFlipAlign = sproutAFlipAlign;
            clone.sproutALevelDescriptors.Clear ();
            for (int i = 0; i < sproutALevelDescriptors.Count; i++) {
                clone.sproutALevelDescriptors.Add (sproutALevelDescriptors [i].Clone ());
            }
            clone.sproutBSize = sproutBSize;
            clone.sproutBScaleAtBase = sproutBScaleAtBase;
            clone.sproutBScaleAtTop = sproutBScaleAtTop;
            clone.sproutBFlipAlign = sproutBFlipAlign;
            clone.sproutBLevelDescriptors.Clear ();
            for (int i = 0; i < sproutBLevelDescriptors.Count; i++) {
                clone.sproutBLevelDescriptors.Add (sproutBLevelDescriptors [i].Clone ());
            }
            for (int i = 0; i < polygonAreas.Count; i++) {
                clone.polygonAreas.Add (polygonAreas [i].Clone ());
            }
            clone.lodCount = lodCount;
            return clone;
        }
        #endregion
    }
}