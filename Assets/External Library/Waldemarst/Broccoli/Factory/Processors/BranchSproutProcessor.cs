using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Factory
{
    /// <summary>
    /// Default processor for branch collections.
    /// </summary>
    [SproutProcessor (0)]
    public class BranchSproutProcessor : ISproutProcessor {
        /// <summary>
        /// Gets the fragmentation parameters according to the
        /// tree max hierarchy level and the LOD.
        /// </summary>
        /// <param name="maxLevel">Tree max hierarchy level.</param>
        /// <param name="lod">Level of detail. From 0 to 2.</param>
        /// <param name="fragLevels">How many fragmentation levels to support. From 1 to n.</param>
        /// <param name="minFragLevel">Where the fragmentation level begins. From 0 to n.</param>
        /// <returns>Fragmentation bias type to generate the fragments.</returns>
        public SproutProcessor.FragmentationBias GetFragmentation (
            int maxLevel, 
            int lod, 
            out int fragLevels,
            out int minFragLevel,
            out int maxFragLevel)
        {
            // For LOD 2
            fragLevels = 1;
            minFragLevel = 0;
            maxFragLevel = 0;
            // For LOD 0, LOD 1
            if (lod == 0 || lod == 1) {
                if (maxLevel > 1) {
                    minFragLevel = 1;
                    maxFragLevel = 1;
                }
            }
            if (lod == 2) {
                return SproutProcessor.FragmentationBias.None;
            }
            return SproutProcessor.FragmentationBias.PlaneAlignment;
        }
        /// <summary>
        /// Gets the type of hull (convex or non-convex) for a fragment mesh.
        /// </summary>
        /// <param name="maxLevel">Tree max hierarchy level.</param>
        /// <param name="lod">Level of detail.</param>
        /// <param name="fragLevel">Frag level to request.</param>
        /// <param name="hullAngle">Parameter to simplify the hull.</param>
        /// <returns>Hull type.</returns>
        public SproutProcessor.HullType GetHullType (
            int maxLevel,
            int lod,
            int fragLevel,
            out float hullAngle) 
        {
            // Set hull angle.
            hullAngle = 30f;
            if (lod == 0) hullAngle = 24f;
            else if (lod == 1) hullAngle = 27f;

            // Return Hull Type.
            if (maxLevel >= 2 && lod == 0) {
                if (fragLevel == 0) {
                    return SproutProcessor.HullType.NonConvex;
                }
            }
            return SproutProcessor.HullType.Convex;
        }
    }
}