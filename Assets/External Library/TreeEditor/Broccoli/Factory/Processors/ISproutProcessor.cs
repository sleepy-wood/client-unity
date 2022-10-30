namespace Broccoli.Factory
{
    public class SproutProcessor {
        /// <summary>
        /// How the mesh fragments of the tree should be generated.
        /// </summary>
        public enum FragmentationBias {
            None = 0,
            PlaneAlignment = 1
        }
        /// <summary>
        /// Hull types.
        /// </summary>
        public enum HullType {
            Convex = 0,
            NonConvex = 1
        }
    }
    /// <summary>
    /// Interface for sprout processors on the factories.
    /// </summary>
    public interface ISproutProcessor {
        /// <summary>
        /// Gets the fragmentation parameters according to the
        /// tree max hierarchy level and the LOD.
        /// </summary>
        /// <param name="maxLevel">Tree max hierarchy level.</param>
        /// <param name="lod">Level of detail.</param>
        /// <param name="fragLevels">How many fragmentation levels to support.</param>
        /// <param name="minFragLevel">Where the fragmentation level begins.</param>
        /// <param name="maxFragLevel">Where the fragmentation level ends.</param>
        /// <returns>Fragmentation bias type to generate the fragments.</returns>
        SproutProcessor.FragmentationBias GetFragmentation (
            int maxLevel, 
            int lod, 
            out int fragLevels,
            out int minFragLevel,
            out int maxFragLevel
        );
        /// <summary>
        /// Gets the type of hull (convex or non-convex) for a fragment mesh.
        /// </summary>
        /// <param name="maxLevel">Tree max hierarchy level.</param>
        /// <param name="lod">Level of detail.</param>
        /// <param name="fragLevel">Frag level to request.</param>
        /// <param name="hullAngle">Parameter to simplify the hull.</param>
        /// <returns>Hull type.</returns>
        SproutProcessor.HullType GetHullType (
            int maxLevel,
            int lod,
            int fragLevel,
            out float hullAngle
        );
    }
}