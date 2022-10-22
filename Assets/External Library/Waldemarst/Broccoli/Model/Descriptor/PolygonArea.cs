using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Model
{
    /// <summary>
    /// Descriptor for a polygon area.
    /// </summary>
    [System.Serializable]
    public class PolygonArea {
        #region Vars
        /// <summary>
        /// Id of the instance.
        /// </summary>
        public int id = 0;
        /// <summary>
        /// Hash for the polygon, based on the branches included and excluded.
        /// </summary>
        [System.NonSerialized]
        public Hash128 hash;
        /// <summary>
        /// Global scale to apply to the texture size.
        /// </summary>
        [System.NonSerialized]
        public float scale = 1f;
        /// <summary>
        /// The id of the branch descriptor the polygon area belongs to.
        /// </summary>
        public int branchDescriptorId = 0;
        /// <summary>
        /// The level of detail this polygon area belongs to.
        /// </summary>
        public int lod = 0;
        /// <summary>
        /// The fragment this polygon area belongs to.
        /// </summary>
        public int fragment = 0;
        /// <summary>
        /// Offset when the polygon is a fragment of a snapshot.
        /// </summary>
        public Vector3 fragmentOffset;
        /// <summary>
        /// Points for the polygon enclosed area.
        /// </summary>
        /// <typeparam name="Vector3">Point.</typeparam>
        /// <returns>List of points.</returns>
        public List<Vector3> points = new List<Vector3> ();
        /// <summary>
        /// Saves the index to the last point of the convex polygon.
        /// </summary>
        public int lastConvexPointIndex = 0;
        /// <summary>
        /// Marks this polygon to be non convex.
        /// </summary>
        public bool isNonConvexHull = false;
        /// <summary>
        /// Normals for this polygon mesh.
        /// </summary>
        public List<Vector3> normals = new List<Vector3> ();
        /// <summary>
        /// Tangents for this polygon mesh.
        /// </summary>
        public List<Vector4> tangents = new List<Vector4> ();
        /// <summary>
        /// UV mapping.
        /// </summary>
        public List<Vector4> uvs = new List<Vector4> ();
        /// <summary>
        /// Triangles for this poygon mesh.
        /// </summary>
        public List<int> triangles = new List<int> ();
        /// <summary>
        /// AABB bounds.
        /// </summary>
        public Bounds aabb;
        /// <summary>
        /// OBB bounds.
        /// </summary>
        public Bounds obb;
        /// <summary>
        /// OBB angle.
        /// </summary>
        public float obbAngle;
        /// <summary>
        /// The mesh for the polygon area.
        /// </summary>
        [System.NonSerialized]
        public Mesh mesh;
        /// <summary>
        /// Guids of the branches included in the polygon area.
        /// </summary>
        /// <typeparam name="System.Guid">Branch guid.</typeparam>
        /// <returns>List of branch guids.</returns>
        [System.NonSerialized]
        public List<System.Guid> includes = new List<System.Guid> ();
        /// <summary>
        /// Guids of the branches excluded in the polygon area.
        /// </summary>
        /// <typeparam name="System.Guid">Branch guid.</typeparam>
        /// <returns>List of branch guids.</returns>
        [System.NonSerialized]
        public List<System.Guid> excludes = new List<System.Guid> ();
        /// <summary>
        /// Ids of the branches included in the polygon area.
        /// </summary>
        /// <typeparam name="int">Branch id.</typeparam>
        /// <returns>List of branch ids.</returns>
        [System.NonSerialized]
        public List<int> includedBranchIds = new List<int> ();
        /// <summary>
        /// Ids of the branches excluded in the polygon area.
        /// </summary>
        /// <typeparam name="int">Branch id.</typeparam>
        /// <returns>List of branch ids.</returns>
        [System.NonSerialized]
        public List<int> excludedBranchIds = new List<int> ();
        #if BROCCOLI_DEVEL
        /// <summary>
        /// Points from the topology of the branches used to create the polygons.
        /// </summary>
        [System.NonSerialized]
        public List<Vector3> topoPoints = new List<Vector3> ();
        #endif
        #endregion

        #region Construction
        /// <summary>
        /// Private class constructor.
        /// </summary>
        private PolygonArea () {}
        /// <summary>
        /// Class contructor.
        /// </summary>
        public PolygonArea (int branchDescriptorId, int fragmentIndex, int lod = 0) {
            this.branchDescriptorId = branchDescriptorId;
            this.fragment = fragmentIndex;
            this.lod = lod;
            id = GetCompundId (branchDescriptorId, fragmentIndex, lod);
        }
        #endregion

        #region Ops
        /// <summary>
        /// Get an instance id.
        /// </summary>
        /// <param name="branchDescriptorId">Id of the branch descriptor.</param>
        /// <param name="fragment">Fragment for the polygon (from 0 to 9,999).</param>
        /// <param name="lod">LOD for the polygon (from 0 to 9).</param>
        /// <returns>Id for a polygon area instance.</returns>
        public static int GetCompundId (int branchDescriptorId, int fragment, int lod = 0) {
            return branchDescriptorId * 100000 + lod * 10000 + fragment;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Clone this instance.
        /// </summary>
        public PolygonArea Clone () {
            PolygonArea clone = new PolygonArea ();
            clone.id = id;
            clone.hash = hash;
            clone.scale = scale;
            clone.branchDescriptorId = branchDescriptorId;
            clone.fragment = fragment;
            clone.lod = lod;
            clone.fragmentOffset = fragmentOffset;
            clone.lastConvexPointIndex = lastConvexPointIndex;
            clone.isNonConvexHull = isNonConvexHull;
            for (int i = 0; i < points.Count; i++) {
                clone.points.Add (points [i]);
            }
            for (int i = 0; i < normals.Count; i++) {
                clone.normals.Add (normals [i]);
            }
            for (int i = 0; i < uvs.Count; i++) {
                clone.uvs.Add (uvs [i]);
            }
            for (int i = 0; i < tangents.Count; i++) {
                clone.tangents.Add (tangents [i]);
            }
            for (int i = 0; i < triangles.Count; i++) {
                clone.triangles.Add (triangles [i]);
            }
            clone.aabb = aabb;
            clone.obb = obb;
            clone.obbAngle = obbAngle;
            #if BROCCOLI_DEVEL
            for (int i = 0; i < topoPoints.Count; i++) {
                clone.topoPoints.Add (topoPoints [i]);
            }
            #endif
            for (int i = 0; i < includes.Count; i++) {
                clone.includes.Add (includes [i]);
            }
            for (int i = 0; i < excludes.Count; i++) {
                clone.excludes.Add (excludes [i]);
            }
            for (int i = 0; i < includedBranchIds.Count; i++) {
                clone.includedBranchIds.Add (includedBranchIds [i]);
            }
            for (int i = 0; i < excludedBranchIds.Count; i++) {
                clone.excludedBranchIds.Add (excludedBranchIds [i]);
            }
            return clone;
        }
        #endregion
    }
}