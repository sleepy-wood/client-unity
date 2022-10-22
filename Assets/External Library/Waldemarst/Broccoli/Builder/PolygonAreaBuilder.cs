using System.Collections.Generic;

using UnityEngine;

using Broccoli.Pipe;
using Broccoli.Model;
using Broccoli.Utils;

namespace Broccoli.Builder
{
    public class PolygonAreaBuilder {
        #region Fragment Class
        public class Fragment {
            public List<System.Guid> includes = new List<System.Guid> ();
            public List<System.Guid> excludes = new List<System.Guid> ();
            public int baseBranchId = -1;
            public List<int> includeIds = new List<int> ();
            public List<int> excludeIds = new List<int> ();
            public List<Vector3> anchorPoints = new List<Vector3> ();
            public Vector3 offset = Vector3.zero;
            public int minLevel = 0;
            public bool hasIncludesOrExcludes {
                get {
                    return (includes.Count > 0 || excludes.Count > 0);
                }
            }
            public string IncludesExcludesToString (int branchDescriptorId) {
                string hashable = branchDescriptorId + ":" + 
                    baseBranchId + "-i:";
                includes.Sort ();
                for (int i = 0; i < includes.Count; i++) {
                    hashable += includes [i].ToString ();
                }
                hashable += "e:";
                for (int i = 0; i < excludes.Count; i++) {
                    hashable += excludes [i].ToString ();
                }
                return hashable;
            }
        }
        #endregion

        #region Vars
        public BroccoTree tree = null;
        public Mesh treeMesh = null;
        public float factoryScale = 1f;
        public enum FragmentBias {
            None = 0,
            PlaneAlignment = 1
        }
        public FragmentBias fragmentBias = FragmentBias.None;
        public bool simplifyHullEnabled = true;
        private float _biasMinPlaneAlign = 0f;
        private float _biasMaxPlaneAlign = 0f;
        #endregion

        #region Singleton
        /// <summary>
        /// Singleton for this class.
        /// </summary>
        static PolygonAreaBuilder _polygonAreaBuilder = null;
        /// <summary>
        /// Gets the singleton instance for this class.
        /// </summary>
        /// <returns>The instance.</returns>
        public static PolygonAreaBuilder GetInstance() {
            if (_polygonAreaBuilder == null) {
                _polygonAreaBuilder = new PolygonAreaBuilder ();
            }
            return _polygonAreaBuilder;
        }
        #endregion

        #region Data Ops
        /// <summary>
        /// Clear local variables.
        /// </summary>
        void Clear () {
            tree = null;
        }
        #endregion

        #region Usage
        /// <summary>
        /// Begins usage of this builder.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="scale"></param>
        public void BeginUsage (BroccoTree tree, Mesh treeMesh, float scale) {
            this.tree = tree;
            this.treeMesh = treeMesh;
            this.factoryScale = scale;
        }
        /// <summary>
        /// Ends usage of this builder.
        /// </summary>
        public void EndUsage () {
            this.tree = null;
            this.treeMesh = null;
        }
        #endregion

        #region Fragment Processing
        /// <summary>
        /// Sets the fragmentation method to directional bias, which takes the 
        /// angle of the children branches to create the fragments.
        /// </summary>
        /// <param name="minPlaneAlign">Minimum plane alignment.</param>
        /// <param name="maxPlaneAlign">Maximum plane alignment.</param>
        public void SetFragmentsDirectionalBias (
            float minPlaneAlign, 
            float maxPlaneAlign)
        {
            _biasMinPlaneAlign = minPlaneAlign;
            _biasMaxPlaneAlign = maxPlaneAlign;
            fragmentBias = FragmentBias.PlaneAlignment;
        }
        public void SetNoFragmentBias () {
            fragmentBias = FragmentBias.None;
        }
        /// <summary>
        /// Generates the fragments for a branch descriptor at a specific LOD.
        /// </summary>
        /// <param name="lodLevel">Level of detail.</param>
        /// <param name="branchDescriptor">Branch descriptor.</param>
        /// <returns>List of fragments for the LOD.</returns>
        public List<Fragment> GenerateSnapshotFragments (
            int lodLevel,
            BranchDescriptor branchDescriptor)
        {
            List<Fragment> fragments;
            switch (fragmentBias) {
                case FragmentBias.PlaneAlignment:
                    fragments = GeneratePlaneAlignmentFragments (lodLevel, branchDescriptor);
                    break;
                default:
                    fragments = GenerateNonBiasFragments (lodLevel, branchDescriptor);
                    break;
            }
            return fragments;
        }
        public List<Fragment> GenerateNonBiasFragments (int lodLevel, BranchDescriptor branchDescriptor) {
            List<Fragment> fragments = new List<Fragment> ();
            for (int i = 0; i < tree.branches.Count; i++) {
                Fragment baseFragment = new Fragment ();
                baseFragment.baseBranchId = tree.branches [i].id;
                baseFragment.offset = tree.branches [i].positionFromRoot;
                baseFragment.minLevel = 0;
                fragments.Add (baseFragment);
            }
            return fragments;
        }
        public List<Fragment> GeneratePlaneAlignmentFragments (int lodLevel, BranchDescriptor branchDescriptor) {
            List<Fragment> fragments = new List<Fragment> ();
            List<BroccoTree.Branch> outBranches = new List<BroccoTree.Branch> ();
            float threshold = 1f;
            if (lodLevel == 0) {
                threshold = 0.17f;
            } else if (lodLevel == 1) {
                threshold = 0.5f;
            } else {
                threshold = 0.7f;
            }
            // Get the threshold range.
            float planeRange = (_biasMaxPlaneAlign - _biasMinPlaneAlign);
            float midPlane = planeRange / 2f + _biasMinPlaneAlign;
            float minThreshold = midPlane - planeRange * 0.5f * threshold;
            float maxThreshold = midPlane + planeRange * 0.5f * threshold;
            // List of branches candidates for base.
            List<BroccoTree.Branch> planeGroup = new List<BroccoTree.Branch> ();
            
            // Process each root branch.
            float branchThresholdPos;
            float followingBranchThresholdPos;
            for (int rootI = 0; rootI < tree.branches.Count; rootI++) {
                // Set the base branch.
                BroccoTree.Branch baseBranch = tree.branches [rootI];
                // Sort children branches by directional angle.
                baseBranch.branches.Sort ((a, b) => a.direction.x.CompareTo (b.direction.x));
                // Iterate each child branch to get a ranged planeGroupCandidate.
                for (int i = 0; i < baseBranch.branches.Count; i++) {
                    // Create the plane group candidate.
                    List<BroccoTree.Branch> planeGroupCandidate = new List<BroccoTree.Branch> ();
                    // Add the initial branch.
                    planeGroupCandidate.Add (baseBranch.branches [i]);
                    branchThresholdPos = Mathf.InverseLerp (minThreshold, maxThreshold, baseBranch.branches [i].direction.x);
                    // Iterate the branches that follow and add them if the fall into the threshold.
                    if (i < baseBranch.branches.Count - 1) {
                         for (int j = i + 1; j < baseBranch.branches.Count; j++) {
                            followingBranchThresholdPos = Mathf.InverseLerp (minThreshold, maxThreshold, baseBranch.branches [j].direction.x);
                            if (followingBranchThresholdPos - branchThresholdPos < threshold) {
                                planeGroupCandidate.Add (baseBranch.branches [j]);
                            } else {
                                break;
                            }
                         }
                    }
                    if (planeGroupCandidate.Count > planeGroup.Count) {
                        planeGroup = planeGroupCandidate;
                    }
                }
                // Get the branches not belonging to the base plane.
                for (int i = 0; i < baseBranch.branches.Count; i++) {
                    if (!planeGroup.Contains (baseBranch.branches [i])) {
                        outBranches.Add (baseBranch.branches [i]);
                    }
                }
                // Create the base fragment.
                Fragment baseFragment = new Fragment ();
                baseFragment.baseBranchId = tree.branches [rootI].id;
                baseFragment.offset = tree.branches [rootI].positionFromRoot;
                baseFragment.minLevel = 0;
                // If the base fragment has no branches, remove one from the outbranches.
                if (outBranches.Count == baseBranch.branches.Count) {
                    outBranches.RemoveAt (0);
                }
                for (int j = 0; j < outBranches.Count; j++){
                    baseFragment.excludes.Add (outBranches [j].guid);
                    baseFragment.excludeIds.Add (outBranches [j].id);
                }
                fragments.Add (baseFragment);
                // Create the child fragments, one per each branch out of range.
                for (int j = 0; j < outBranches.Count; j++) {
                    Fragment childFragment = new Fragment ();
                    childFragment.offset = outBranches [j].GetPointAtPosition (0f);
                    childFragment.minLevel = 1;
                    childFragment.includes.Add (outBranches [j].guid);
                    childFragment.includeIds.Add (outBranches [j].id);
                    fragments.Add (childFragment);
                }
            }
            return fragments;
        }
        #endregion

        #region Polygon Processing
        /// <summary>
        /// Gets the outline points of a fragment and creates de bounds for a polygon area.
        /// 1. Get the topology points to create a hull.
        /// 2. Create a convex hull.
        /// 3. Simplify the convex hull.
        /// 4. Create the AABB.
        /// 5. Create the OBB and set OBB rotation.
        /// </summary>
        /// <param name="polygonArea">Polygon area to process.</param>
        /// <param name="fragment">Fragment to process.</param>
        public void ProcessPolygonAreaBounds (PolygonArea polygonArea, Fragment fragment) {
            // Create hull.
            if (polygonArea.lod <= 1 && fragment.hasIncludesOrExcludes) {
                CreatePolygonNonConvexHull (polygonArea, fragment);
            } else {
                CreatePolygonConvexHull (polygonArea, fragment);
            }

            
            if (polygonArea.points.Count > 0) {
                // AABB box.
                Bounds _aabb = GeometryUtility.CalculateBounds (polygonArea.points.ToArray (), Matrix4x4.identity);
                polygonArea.aabb = _aabb;

                // OBB box.
                float _obbAngle = 0f;
                GeometryAnalyzer ga = GeometryAnalyzer.Current ();
                Bounds _obb = ga.GetOBBFromPolygon (polygonArea.points, out _obbAngle);
                polygonArea.obb = _obb;
                polygonArea.obbAngle = _obbAngle;
            }

            // Set scale.
            float meshWidth = treeMesh.bounds.max.z - treeMesh.bounds.min.z;
            float meshHeight = treeMesh.bounds.max.y - treeMesh.bounds.min.y;
            if (meshWidth > meshHeight) {
                polygonArea.scale = (polygonArea.aabb.max.z - polygonArea.aabb.min.z) / meshWidth;
            } else {
                polygonArea.scale = (polygonArea.aabb.max.y - polygonArea.aabb.min.y) / meshHeight;
            }
        }
        /// <summary>
        /// Adds additional points to the geometry of a polygon area beforeits triangulation process.
        /// </summary>
        /// <param name="polygonArea">Polygon area to process.</param>
        /// <param name="fragment">Fragment to process.</param>
        public void ProcessPolygonDetailPoints (PolygonArea polygonArea, Fragment fragment) {
            for (int i = 0; i < fragment.anchorPoints.Count; i++) {
                polygonArea.points.Add (fragment.anchorPoints [i] * factoryScale);
            }
            if (polygonArea.lod == 0) {
                GeometryAnalyzer ga = GeometryAnalyzer.Current ();
                ga.GetInnerPoints (tree, fragment.includes, fragment.excludes, false);
                for (int i = 0; i < ga.branchPoints.Count; i++) {
                    polygonArea.points.Add (ga.branchPoints [i] * factoryScale);   
                }
            }
            if (polygonArea.lod == 1) {
                Vector3 lastPos = tree.branches [0].GetPointAtPosition (1f) * factoryScale;
                polygonArea.points.Add (lastPos);
            }
        }
        private void CreatePolygonNonConvexHull (PolygonArea polygonArea, Fragment fragment) {
            // Get the geometry analizer and set the snapshot points.
            GeometryAnalyzer ga = GeometryAnalyzer.Current ();
            List<Vector3> snapshotPoints = new List<Vector3> ();
            List<Vector3> snapshotHull = new List<Vector3> ();
            List<List<Vector3>> polygons = new List<List<Vector3>> ();

            // Get the filtered branches..
            List<BroccoTree.Branch> _filteredBranches = new List<BroccoTree.Branch> ();
            _filteredBranches = ga.GetFilteredBranches (tree, fragment.includes, fragment.excludes);
            
            // For each filtered branch and their sprouts, get a polygon.
            int branchLevel;
            List<Vector3> commonPoints = new List<Vector3> ();
            for (int i = 0; i < _filteredBranches.Count; i++) {
                branchLevel = _filteredBranches [i].GetLevel ();
                if (branchLevel <= 1) {
                    ga.Clear ();
                    snapshotPoints.Clear ();
                    snapshotHull.Clear ();
                    if (branchLevel == 0) {
                        if (_filteredBranches.Count == 1) return;
                        snapshotPoints.Add (_filteredBranches [i].GetPointAtPosition (0f));
                        snapshotPoints.Add (_filteredBranches [i].GetPointAtPosition (1f));
                        ga.GetSproutPositions (_filteredBranches [i], 1.3f, false);
                        snapshotPoints.AddRange (ga.sproutPoints);
                        commonPoints.AddRange (snapshotPoints);
                    } else if (branchLevel == 1) {
                        List<BroccoTree.Branch> subBranches = ga.GetOutlinePoints (_filteredBranches [i], false);
                        ga.GetSproutPositions (subBranches, 1.3f, false);
                        snapshotPoints.Add (_filteredBranches [i].GetPointAtPosition (0f));
                        snapshotPoints.AddRange (commonPoints);
                        snapshotPoints.AddRange (ga.branchPoints);
                        snapshotPoints.AddRange (ga.sproutPoints);
                        snapshotHull = ga.QuickHullYZ (new List<Vector3>(snapshotPoints));
                        for (int j = 0; j < snapshotHull.Count; j++) {
                            snapshotHull [j] = snapshotHull [j] * factoryScale;
                        }
                        polygons.Add (new List<Vector3> (new List<Vector3> (snapshotHull)));
                        #if BROCCOLI_DEVEL
                        polygonArea.topoPoints.AddRange (snapshotHull);
                        #endif
                    }
                }
            }

            // Combine
            polygonArea.points.Clear ();
            
            if (polygons.Count > 1) {
                polygonArea.points = ga.CombineConvexHullsYZ (polygons);
                if (simplifyHullEnabled)
                    polygonArea.points = ga.SimplifyConvexHullYZ (polygonArea.points, 25f);
            } else {
                polygonArea.points = polygons [0];
                if (simplifyHullEnabled)
                    polygonArea.points = ga.SimplifyConvexHullYZ (polygonArea.points, 25f);
            }
            //polygonArea.points = polygons [0];
            polygonArea.lastConvexPointIndex = polygonArea.points.Count - 1;
            polygonArea.isNonConvexHull = true;
        }
        private void CreatePolygonConvexHull (PolygonArea polygonArea, Fragment fragment) {
            // Analyze the tree points.
            GeometryAnalyzer ga = GeometryAnalyzer.Current ();
            List<Vector3> snapshotPoints = new List<Vector3> ();

            // Get branch points.
            List<BroccoTree.Branch> _filteredBranches = 
                ga.GetOutlinePoints (tree, fragment.includes, fragment.excludes, false);
            snapshotPoints.AddRange (ga.branchPoints);
            // Get Sprout points.
            ga.GetSproutPositions (_filteredBranches, 1f, false);
            snapshotPoints.AddRange (ga.sproutPoints);
            // Clear the geometry analizer points.
            ga.Clear ();

            // Scale points.
            for (int i = 0; i < snapshotPoints.Count; i++) {
                snapshotPoints [i] = snapshotPoints [i] * factoryScale;
                #if BROCCOLI_DEVEL
                polygonArea.topoPoints.Add (snapshotPoints [i]);
                #endif
            }

            // ConvexHull points.
            List<Vector3> _convexPoints = ga.QuickHullYZ (snapshotPoints, false);
            _convexPoints = ga.ShiftConvexHullPoint (_convexPoints);
            if (_convexPoints.Count > 0) {
                _convexPoints.Add (_convexPoints [0]);
            }

            // Simplify convex hull points.
            if (simplifyHullEnabled) {
                float simplifyAngle = 35f;
                if (polygonArea.lod == 0) {
                    simplifyAngle = 20f;
                } else if (polygonArea.lod == 1) {
                    simplifyAngle = 28f;
                }
                if (simplifyHullEnabled)
                    _convexPoints = ga.SimplifyConvexHullYZ (_convexPoints, simplifyAngle);
            }
            _convexPoints.RemoveAt (_convexPoints.Count - 1);

            // Set the polygon area points.
            polygonArea.lastConvexPointIndex = _convexPoints.Count - 1;
            polygonArea.points.Clear ();
            polygonArea.points.AddRange (_convexPoints);
        }
        /// <summary>
        /// Calculates the mesh related values for the polygon area definition and
        /// creates its mesh.
        /// </summary>
        /// <param name="polygonArea">Polygon area to create the mesh from.</param>
        public void ProcessPolygonAreaMesh (PolygonArea polygonArea) {
            // Triangulation
            GeometryAnalyzer ga = GeometryAnalyzer.Current ();
            List<int> _triangles = new List<int> ();
            if (polygonArea.isNonConvexHull) {
                _triangles = ga.DelaunayConstrainedTriangulationYZ (polygonArea.points, polygonArea.lastConvexPointIndex);
            } else {
                _triangles = ga.DelaunayTriangulationYZ (polygonArea.points);
            }
            polygonArea.triangles.Clear ();
            polygonArea.triangles.AddRange (_triangles);


			Mesh mesh = new Mesh ();
			// Set vertices.
			mesh.SetVertices (polygonArea.points);
			// Set triangles.
			mesh.SetTriangles (polygonArea.triangles, 0);
			mesh.RecalculateBounds ();
			// Set normals.
			mesh.RecalculateNormals ();
			polygonArea.normals.Clear ();
			polygonArea.normals.AddRange (mesh.normals);
			// Set tangents.
			Vector4[] _tangents = new Vector4[polygonArea.points.Count];
			for (int i = 0; i < _tangents.Length; i++) {
				_tangents [i] = Vector3.forward;
				_tangents [i].w = 1f;
			}
			mesh.tangents = _tangents;
			polygonArea.tangents.Clear ();
			polygonArea.tangents.AddRange (mesh.tangents);
			// Set UVs.
			float z, y;
			List<Vector4> uvs = new List<Vector4> ();
			for (int i = 0; i < polygonArea.points.Count; i++) {
				z = Mathf.InverseLerp (polygonArea.aabb.min.z, polygonArea.aabb.max.z, polygonArea.points [i].z);
				y = Mathf.InverseLerp (polygonArea.aabb.min.y, polygonArea.aabb.max.y, polygonArea.points [i].y);
				uvs.Add (new Vector4 (z, y, z, y));
			}
			mesh.SetUVs (0, uvs);
			polygonArea.uvs.Clear ();
			polygonArea.uvs.AddRange (uvs);

			// Set the mesh.
			polygonArea.mesh = mesh;
		}
        /// <summary>
        /// Creates a mesh from a polygon area definition.
        /// </summary>
        /// <param name="polygonArea">Polygon area to create the mesh from.</param>
        public static void SetPolygonAreaMesh (PolygonArea polygonArea) {
            Mesh mesh = new Mesh ();
			// Set vertices.
			mesh.SetVertices (polygonArea.points);
			// Set triangles.
			mesh.SetTriangles (polygonArea.triangles, 0);
			// Set normals.
			mesh.SetNormals (polygonArea.normals);
			// Set tangents.
			mesh.SetTangents (polygonArea.tangents);
			// Set UVs.
			mesh.SetUVs (0, polygonArea.uvs);
            mesh.SetUVs (1, polygonArea.uvs);
            // Recalculate bound.
            mesh.RecalculateBounds ();
			// Set the mesh.
			polygonArea.mesh = mesh;
        }
        #endregion

        #region Utils
        /// <summary>
        /// Gets the Axis Aligned Bounding Box area.
        /// </summary>
        /// <param name="bounds">Bounds</param>
        /// <returns>Area of the AABB.</returns>
        private float GetAABBAreaYZ (Bounds bounds) {
            float area = (bounds.max.y - bounds.min.y) * (bounds.max.z - bounds.min.z);
            return area;
        }
        #endregion
    }
}