using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Broccoli.Model
{
    /// <summary>
    /// Level of Detail (LOD) definition for trees.
    /// </summary>
	[System.Serializable]
	public class LODDef {
		#region Vars
        /// <summary>
        /// Predefined quality settings on LOD definitions.
        /// </summary>
        public enum Preset {
            UltraLowPoly,
            LowPoly,
            RegularPoly,
            HighPoly,
            UltraHighPoly,
            Custom
        }
        /// <summary>
        /// Selected LOD preset for this definition.
        /// </summary>
        public Preset preset = Preset.RegularPoly;
        /// <summary>
        /// <c>True</c> to include this instance in the LOD group for a prefab.
        /// </summary>
        public bool includeInPrefab = true;
        /// <summary>
        /// Minimum number of polygons to use on branches.
        /// </summary>
        public int minPolygonSides = 6;
        /// <summary>
        /// Maximum number of polygons to use on branches.
        /// </summary>
        public int maxPolygonSides = 16;
        /// <summary>
        /// Max angle tolerance on branch curves.
        /// </summary>
        public float branchAngleTolerance = 25f;
        /// <summary>
        /// Max angle tolerance on sprout curves.
        /// </summary>
        public float sproutAngleTolerance = 25f;
        /// <summary>
        /// Capping mesh at the base of each branch.
        /// </summary>
        public bool useMeshCapAtBase = true;
        /// <summary>
        /// Allows applying branch welding to the tree.
        /// </summary>
        public bool allowBranchWelding = true;
        /// <summary>
        /// Allows appying root welding to the tree.
        /// </summary>
        public bool allowRootWelding = true;
        /// <summary>
        /// How much percentage this LOD definition takes on a LOD group.
        /// </summary>
        public float groupPercentage = 0.3f;
		#endregion

        #region Presets
        /// <summary>
        /// Get a preset LOD definition.
        /// </summary>
        /// <param name="preset">Preset for the LOD.</param>
        /// <param name="lodDef">Optional LODDef instance to set the preset values to.</param>
        /// <returns>LODDef instance.</returns>
        public static LODDef GetPreset (Preset preset, LODDef lodDef = null) {
            if (lodDef == null) {
                lodDef = new LODDef ();
            }
            lodDef.preset = preset;
            switch (lodDef.preset) {
                case Preset.UltraLowPoly:
                    lodDef.minPolygonSides = 3;
                    lodDef.maxPolygonSides = 5;
                    lodDef.branchAngleTolerance = 45f;
                    lodDef.sproutAngleTolerance = 45f;
                    lodDef.useMeshCapAtBase = false;
                    lodDef.allowBranchWelding = false;
                    lodDef.allowRootWelding = false;
                    lodDef.groupPercentage = 0.5f;
                    break;
                case Preset.LowPoly:
                    lodDef.minPolygonSides = 3;
                    lodDef.maxPolygonSides = 6;
                    lodDef.branchAngleTolerance = 36f;
                    lodDef.sproutAngleTolerance = 36f;
                    lodDef.useMeshCapAtBase = false;
                    lodDef.allowBranchWelding = false;
                    lodDef.allowRootWelding = false;
                    lodDef.groupPercentage = 0.4f;
                    break;
                case Preset.RegularPoly:
                case Preset.Custom:
                    lodDef.minPolygonSides = 3;
                    lodDef.maxPolygonSides = 8;
                    lodDef.branchAngleTolerance = 24f;
                    lodDef.sproutAngleTolerance = 24f;
                    lodDef.groupPercentage = 0.3f;
                    break;
                case Preset.HighPoly:
                    lodDef.minPolygonSides = 4;
                    lodDef.maxPolygonSides = 9;
                    lodDef.branchAngleTolerance = 17f;
                    lodDef.sproutAngleTolerance = 17f;
                    lodDef.groupPercentage = 0.2f;
                    break;
                case Preset.UltraHighPoly:
                    lodDef.minPolygonSides = 5;
                    lodDef.maxPolygonSides = 12;
                    lodDef.branchAngleTolerance = 8f;
                    lodDef.sproutAngleTolerance = 8f;
                    lodDef.groupPercentage = 0.1f;
                    break;
            }
            return lodDef;
        }
        #endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		public LODDef Clone() {
			LODDef clone = new LODDef ();
            clone.preset = preset;
            clone.includeInPrefab = true;
            clone.minPolygonSides = minPolygonSides;
            clone.maxPolygonSides = maxPolygonSides;
            clone.branchAngleTolerance = branchAngleTolerance;
            clone.sproutAngleTolerance = sproutAngleTolerance;
            clone.useMeshCapAtBase = useMeshCapAtBase;
            clone.groupPercentage = groupPercentage;
			return clone;
		}
		#endregion
	}
}