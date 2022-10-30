using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Broccoli.Base;
using Broccoli.Model;
using Broccoli.Factory;

namespace Broccoli.Builder {
	/// <summary>
	/// Gives methods to help create mesh segments using BranchSkin instances.
	/// </summary>
	public class WeldingMeshBuilder : IBranchMeshBuilder {
		#region Class BranchInfo
		/// <summary>
		/// Class containing the information to process the mesh of a branch.
		/// </summary>
		public class BranchInfo {
			/// <summary>
			/// Minimum distance from the base of the branch/root to being the welding (1 = branch base girth).
			/// </summary>
			public float weldingDistance = 1f;
			/// <summary>
			/// Minimum distance the welding spreads in the branch/root parent growth direction.
			/// </summary>
			public float weldingUpperSpread = 0.5f;
			/// <summary>
			/// Minimum distance the welding spreads against the branch/root parent growth direction.
			/// </summary>
			public float weldingLowerSpread = 1f;
			/// <summary>
			/// BranchSkin start of the welding.
			/// </summary>
			public float weldingBranchSkinStartPosition = 0f;
			/// <summary>
			/// BranchSkin end of the welding.
			/// </summary>
			public float weldingBranchSkinEndPosition = 0f;
		}
		#endregion

		#region Vars
		public bool useBranchWelding = true;
		/// <summary>
		/// Adds faces at the base of the welding segment.
		/// </summary>
		public bool useBranchWeldingMeshCap = false;
		/// <summary>
		/// Minimum value on how much on the tree hierarchy the welding should extend to in relative distance to the trunk origin (0 welding, 1 all branch distance).
		/// </summary>
		public float minBranchWeldingHierarchyRange = 0.3f;
		/// <summary>
		/// Maximum value on how much on the tree hierarchy the welding should extend to in relative distance to the trunk origin (0 welding, 1 all branch distance).
		/// </summary>
		public float maxBranchWeldingHierarchyRange = 0.4f;
		/// <summary>
		/// Processed top range value for the hierarchy range to apply welding to branches.
		/// </summary>
		public float branchWeldingHierarchyRange = 0f;
		/// <summary>
		/// Curve to dampen the welding effect (spread) depending on the branch distance in the hierarchy,
		/// </summary>
		public AnimationCurve branchWeldingHierarchyRangeCurve = AnimationCurve.Linear (0f, 1f, 1f, 0f);
		/// <summary>
		/// Curve to follow to shape the welding.
		/// </summary>
		public AnimationCurve branchWeldingCurve = AnimationCurve.Linear (0f, 1f, 1f, 0f);
		/// <summary>
		/// Minimum distance from the base of the branch to being the welding (1 = branch base girth).
		/// </summary>
		public float minBranchWeldingDistance = 1f;
		/// <summary>
		/// Maximum distance from the base of the branch to being the welding (1 = branch base girth).
		/// </summary>
		public float maxBranchWeldingDistance = 1f;
		/// <summary>
		/// Minimum additional segments to add to the welding segment for branches.
		/// </summary>
		public int minAdditionalBranchWeldingSegments = 0;
		/// <summary>
		/// Maximum additional segments to add to the welding segment for branches.
		/// </summary>
		public int maxAdditionalBranchWeldingSegments = 0;
		/// <summary>
		/// Minimum distance the welding spreads in the branch parent growth direction.
		/// </summary>
		public float minBranchWeldingUpperSpread = 0.5f;
		/// <summary>
		/// Maximum distance the welding spreads in the branch parent growth direction.
		/// </summary>
		public float maxBranchWeldingUpperSpread = 0.5f;
		/// <summary>
		/// Minimum distance the welding spreads against the branch parent growth direction.
		/// </summary>
		public float minBranchWeldingLowerSpread = 1f;
		/// <summary>
		/// Maximum distance the welding spreads against the branch parent growth direction.
		/// </summary>
		public float maxBranchWeldingLowerSpread = 1f;
		/// <summary>
		/// Flag to turn on root welding.
		/// </summary>
		public bool useRootWelding = true;
		/// <summary>
		/// Adds faces at the base of the welding segment for roots.
		/// </summary>
		public bool useRootWeldingMeshCap = false;
		/// <summary>
		/// Minimum value on how much on the tree hierarchy the welding should extend to in relative distance to the trunk origin (0 welding, 1 all root distance).
		/// </summary>
		public float minRootWeldingHierarchyRange = 0.3f;
		/// <summary>
		/// Maximum value on how much on the tree hierarchy the welding should extend to in relative distance to the trunk origin (0 welding, 1 all root distance).
		/// </summary>
		public float maxRootWeldingHierarchyRange = 0.4f;
		/// <summary>
		/// Processed top range value for the hierarchy range to apply welding to roots.
		/// </summary>
		public float rootWeldingHierarchyRange = 0f;
		/// <summary>
		/// Curve to dampen the welding effect (spread) depending on the root distance in the hierarchy,
		/// </summary>
		public AnimationCurve rootWeldingHierarchyRangeCurve = AnimationCurve.Linear (0f, 1f, 1f, 0f);
		/// <summary>
		/// Curve to follow to shape the welding.
		/// </summary>
		public AnimationCurve rootWeldingCurve = AnimationCurve.Linear (0f, 1f, 1f, 0f);
		/// <summary>
		/// Minimum distance from the base of the root to being the welding (1 = root base girth).
		/// </summary>
		public float minRootWeldingDistance = 1f;
		/// <summary>
		/// Maximum distance from the base of the root to being the welding (1 = root base girth).
		/// </summary>
		public float maxRootWeldingDistance = 1f;
		/// <summary>
		/// Minimum additional segments to add to the welding segment for branches.
		/// </summary>
		public int minAdditionalRootWeldingSegments = 0;
		/// <summary>
		/// Maximum additional segments to add to the welding segment for branches.
		/// </summary>
		public int maxAdditionalRootWeldingSegments = 0;
		/// <summary>
		/// Minimum distance the welding spreads in the root parent growth direction.
		/// </summary>
		public float minRootWeldingUpperSpread = 0.5f;
		/// <summary>
		/// Maximum distance the welding spreads in the root parent growth direction.
		/// </summary>
		public float maxRootWeldingUpperSpread = 0.5f;
		/// <summary>
		/// Minimum distance the welding spreads against the root parent growth direction.
		/// </summary>
		public float minRootWeldingLowerSpread = 1f;
		/// <summary>
		/// Maximum distance the welding spreads against the root parent growth direction.
		/// </summary>
		public float maxRootWeldingLowerSpread = 1f;
		/// <summary>
		/// Branch welding information per branch.
		/// </summary>
		/// <typeparam name="int"></typeparam>
		/// <typeparam name="BranchInfo"></typeparam>
		/// <returns></returns>
		protected Dictionary<int, BranchInfo> _branchInfos = new Dictionary<int, BranchInfo> ();
		public float angleTolerance = 200f;
		public float globalScale = 1f;
		#endregion

		#region Interface
		public virtual void SetAngleTolerance (float angleTolerance) {
			//this.angleTolerance = angleTolerance * 2.5f;
		}
		public virtual float GetAngleTolerance () {
			return angleTolerance;
		}
		public virtual void SetGlobalScale (float globalScale) { this.globalScale = globalScale; }
		public virtual float GetGlobalScale () { return this.globalScale; }
		/// <summary>
		/// Get the branch mesh builder type.
		/// </summary>
		/// <returns>Branch mesh builder type.</returns>
		public virtual BranchMeshBuilder.BuilderType GetBuilderType () {
			return BranchMeshBuilder.BuilderType.Welding;
		}
		/// <summary>
		/// Called right after a BranchSkin is created.
		/// </summary>
		/// <param name="rangeIndex">Index of the branch skin range to process.</param>
		/// <param name="branchSkin">BranchSkin instance to process.</param>
		/// <param name="firstBranch">The first branch instance on the BranchSkin instance.</param>
		/// <param name="parentBranchSkin">Parent BranchSkin instance to process.</param>
		/// <param name="parentBranch">The parent branch of the first branch of the BranchSkin instance.</param>
		/// <returns>True if any processing gets done.</returns>
		public virtual bool PreprocessBranchSkinRange (
			int rangeIndex, 
			BranchMeshBuilder.BranchSkin branchSkin, 
			BroccoTree.Branch firstBranch, 
			BranchMeshBuilder.BranchSkin parentBranchSkin = null, 
			BroccoTree.Branch parentBranch = null)
		{
			bool result = false;
			// If the selected range index is a Shape type.
			if (rangeIndex < branchSkin.ranges.Count && branchSkin.ranges[rangeIndex].builderType == BranchMeshBuilder.BuilderType.Welding) {
				// Add additional segments to the welding range. TODO: add pass.
				int addSegs = Random.Range (minAdditionalBranchWeldingSegments, maxAdditionalBranchWeldingSegments);
				if (addSegs > 0) {
					float addSegStep = branchSkin.ranges[rangeIndex].to / addSegs;
					for (int i = 1; i < addSegs; i++) {
						branchSkin.AddRelevantPosition (addSegStep * i, 0.01f, 1);
					}
				}

				// Add relevant position at the end of the welding.
				branchSkin.AddRelevantPosition (branchSkin.ranges[rangeIndex].to, 0.01f, 2);
				// Add the welding limit to the branchskin info.
				BranchInfo branchInfo = GetBranchInfo (firstBranch.id);
				if (branchInfo != null) {
					branchInfo.weldingBranchSkinEndPosition = branchSkin.ranges[rangeIndex].to;
				}
				result = true;
			}
			return result;
		}
		/// <summary>
		/// Called per branchskin after the main mesh has been processed. Modifies an additional mesh to merge it with the one processed.
		/// </summary>
		/// <param name="mesh">Mesh to process.</param>
		/// <param name="rangeIndex">Index of the branch skin range to process.</param>
		/// <param name="branchSkin">BranchSkin instance to process.</param>
		/// <param name="firstBranch">The first branch instance on the BranchSkin instance.</param>
		/// <param name="parentBranchSkin">Parent BranchSkin instance to process.</param>
		/// <param name="parentBranch">The parent branch of the first branch of the BranchSkin instance.</param>
		/// <returns>True if any processing gets done.</returns>
		public virtual Mesh PostprocessBranchSkinRange (Mesh mesh, int rangeIndex, BranchMeshBuilder.BranchSkin branchSkin, BroccoTree.Branch firstBranch, BranchMeshBuilder.BranchSkin parentBranchSkin = null, BroccoTree.Branch parentBranch = null) {
			int numSegments = branchSkin.maxPolygonSides;
			// Find branch info.
			BranchInfo branchInfo = GetBranchInfo (branchSkin.id);
			// If the branch info is found.
			if (branchInfo != null && parentBranch != null) {
				// Get angle between direction at parent branch and child normal vector.
				Plane directionPlane = firstBranch.GetParentPlane ();
				float weldingAngle = Vector3.SignedAngle (
					Vector3.ProjectOnPlane (firstBranch.GetNormalAtPosition (0f), directionPlane.normal),
					Vector3.ProjectOnPlane (parentBranch.GetDirectionAtPosition (firstBranch.position), directionPlane.normal),
					directionPlane.normal
				);
				
				// Create mesh.
				Mesh weldingMesh = new Mesh ();
				List<Vector3> vertices = new List<Vector3> ();
				List<int> triangles = new List<int> ();
				List<Vector3> normals = new List<Vector3> ();
				List<Vector4> uvs = new List<Vector4> ();
				List<Vector4> uv3s = new List<Vector4> ();
				List<Vector4> uv5s = new List<Vector4> ();
				List<Vector4> uv6s = new List<Vector4> ();
				List<Vector4> uv7s = new List<Vector4> ();
				// Add segments belonging to the welding builder to mesh.
				int segmentIndex = 0;
				float inBranchWeldingLerp;
				float hierarchyWeldingLerp;
				int firstSegmentIndex = -1;
				int lastSegmentIndex = 0;
				float weldingLengthAtBranchSkin = 0f;
				float weldingPosAtBranchSkin = 0f;
				Vector3 weldingNormal = Vector3.zero;
				while (segmentIndex < branchSkin.builders.Count && branchSkin.builders [segmentIndex] == BranchMeshBuilder.BuilderType.Welding) {
					// Welding position curve.
					inBranchWeldingLerp = Mathf.InverseLerp (branchInfo.weldingBranchSkinStartPosition, branchInfo.weldingBranchSkinEndPosition, branchSkin.positionsAtSkin [segmentIndex]);
					inBranchWeldingLerp = branchWeldingCurve.Evaluate (inBranchWeldingLerp);
					// Wlding hierarchy curve..
					hierarchyWeldingLerp = inBranchWeldingLerp * branchWeldingHierarchyRangeCurve.Evaluate (Mathf.InverseLerp (0f, branchWeldingHierarchyRange, branchSkin.hierarchyLevel));
					// Add segments.
					if (segmentIndex == 0) {
						weldingLengthAtBranchSkin = SetWeldingPolygon (
							numSegments,
							weldingAngle * Mathf.Deg2Rad,
							hierarchyWeldingLerp,
							branchSkin,
							firstBranch,
							parentBranchSkin,
							parentBranch,
							branchInfo,
							vertices,
							normals,
							uvs,
							uv3s,
							uv5s,
							uv6s,
							uv7s,
							out weldingNormal);
						// Set the branchskin position of the begining of the welding.
						branchInfo.weldingBranchSkinStartPosition = weldingLengthAtBranchSkin / branchSkin.length;
						// set next welding post at double the distance of the initial segment.
						weldingPosAtBranchSkin = branchInfo.weldingBranchSkinStartPosition * 1.2f;
					} else if (branchSkin.positionsAtSkin[segmentIndex] >= weldingPosAtBranchSkin) {
						SetPolygonAt (
							branchSkin.id,
							firstBranch.id,
							firstBranch.helperStructureLevelId,
							(branchSkin.segments [segmentIndex] == 0?numSegments:branchSkin.segments [segmentIndex]),
							branchSkin.segmentTypes [segmentIndex],
							branchSkin.centers [segmentIndex],
							branchSkin.positions [segmentIndex],
							branchSkin.directions [segmentIndex],
							branchSkin.positionsAtSkin [segmentIndex],
							branchSkin.normals [segmentIndex],
							branchSkin.girths [segmentIndex],
							branchSkin.lengthOffset + (branchSkin.length * branchSkin.positionsAtSkin [segmentIndex]),
							(int)branchSkin.builders [segmentIndex],
							vertices,
							normals,
							uvs,
							uv3s,
							uv5s,
							uv6s,
							uv7s,
							weldingAngle * Mathf.Deg2Rad,
							inBranchWeldingLerp,
							hierarchyWeldingLerp,
							weldingNormal,
							branchInfo);
						lastSegmentIndex = segmentIndex;
						if (firstSegmentIndex < 0) firstSegmentIndex = segmentIndex;
					}
					segmentIndex++;
				}
				segmentIndex = lastSegmentIndex + 1;
				if (segmentIndex == 1) return null; // No segments to add (branch within the parent branch girth).
				
				// Add triangles.
				int baseIndex = 0;
				int topIndex = 0;
				int prevSegmentSideCount;
				int currSegmentSideCount;
				// If there is a mesh cap at base, build the triangles.
				if ((firstBranch.isRoot && useRootWeldingMeshCap) || (!firstBranch.isRoot && useBranchWeldingMeshCap)) {
					topIndex = numSegments + 1;
					for (int j = 0; j < numSegments; j++) {
						triangles.Add (topIndex + j + 1);
						triangles.Add (topIndex + j);
						triangles.Add (baseIndex + j);
						triangles.Add (baseIndex + j + 1);
						triangles.Add (topIndex + j + 1);
						triangles.Add (baseIndex + j);
					}
					baseIndex = topIndex;
				}
				// Add the rest of the triangles.
				for (int i = firstSegmentIndex; i < segmentIndex; i++) {
					prevSegmentSideCount = numSegments;
					if (i == segmentIndex - 1) {
						currSegmentSideCount = branchSkin.segments [segmentIndex - 1];
					} else {
						currSegmentSideCount = numSegments;
					}
					topIndex += prevSegmentSideCount + 1;
					// Add triangles when current and previous segment have the same number of sides.
					if (prevSegmentSideCount == currSegmentSideCount) {
						for (int j = 0; j < prevSegmentSideCount; j++) {
							triangles.Add (topIndex + j + 1);
							triangles.Add (topIndex + j);
							triangles.Add (baseIndex + j);
							triangles.Add (baseIndex + j + 1);
							triangles.Add (topIndex + j + 1);
							triangles.Add (baseIndex + j);
						}
					} else {
						// Add triangles when one current and previous segment have a different number of sides.
						int aSegmentIndex; // A segment has the fewer number of sides.
						int bSegmentIndex; // B segment has the greater number of sides.
						bool inverse = false;
						if (prevSegmentSideCount > currSegmentSideCount) {
							aSegmentIndex = topIndex; // A segment has the fewer number of sides.
							bSegmentIndex = baseIndex; // B segment has the greater number of sides.
						} else {
							aSegmentIndex = baseIndex; // A segment has the fewer number of sides.
							bSegmentIndex = topIndex; // B segment has the greater number of sides.	
							inverse = true;
						}
						float aSegmentValue = uvs [aSegmentIndex].x; // A segment index radial value.
						float bSegmentValue = uvs [bSegmentIndex].x; // B segment index radial value.
						float halfA = (aSegmentValue + uvs [aSegmentIndex + 1].x) /2f; // half value between segment A index and segment A index + 1.
						while (bSegmentValue < 1) {
							if (bSegmentValue < halfA) {
								// Segment B
								triangles.Add (bSegmentIndex);
								if (inverse) {
									triangles.Add (aSegmentIndex);
									triangles.Add (bSegmentIndex + 1);
								} else {
									triangles.Add (bSegmentIndex + 1);
									triangles.Add (aSegmentIndex);
								}
								bSegmentIndex++;
								bSegmentValue = uvs [bSegmentIndex].x;
							} else {
								// Segment A
								triangles.Add (bSegmentIndex);
								if (inverse) {
									triangles.Add (aSegmentIndex + 1);
									triangles.Add (aSegmentIndex);
								} else {
									triangles.Add (aSegmentIndex + 1);
									triangles.Add (aSegmentIndex);
								}
								aSegmentIndex++;
								aSegmentValue = uvs [aSegmentIndex].x;
								if (aSegmentValue < 1f) {
									halfA = (aSegmentValue + uvs [aSegmentIndex + 1].x) /2f;
								} else {
									halfA = aSegmentValue + 0.2f;
								}
							}
						}
						if (aSegmentValue < 1f) {
							triangles.Add (bSegmentIndex);
							if (inverse) {
								triangles.Add (aSegmentIndex + 1);
								triangles.Add (aSegmentIndex);
							} else {
								triangles.Add (aSegmentIndex + 1);
								triangles.Add (aSegmentIndex);
							}
						}
					}
					baseIndex = topIndex;
				}
				// Add info to the mesh.
				weldingMesh.SetVertices (vertices);
				weldingMesh.SetTriangles(triangles, 0);
				weldingMesh.SetNormals (normals);
				weldingMesh.SetUVs (0, uvs);
				weldingMesh.SetUVs (2, uv3s);
				weldingMesh.SetUVs (4, uv5s);
				weldingMesh.SetUVs (5, uv6s);
				weldingMesh.SetUVs (6, uv7s);
				weldingMesh.RecalculateBounds ();
				weldingMesh.RecalculateTangents ();
				//return null;
				return weldingMesh;
			}
			return null;
		}
		public virtual Vector3 GetBranchSkinPositionOffset (float positionAtBranch, BroccoTree.Branch branch, float rollAngle, Vector3 forward, BranchMeshBuilder.BranchSkin branchSkin) {
			return Vector3.zero;
		}
		#endregion

		#region BranchInfo Methods
		public void ClearBranchInfos () {
			_branchInfos.Clear ();
		}
		public BranchInfo RegisterBranchInfo (BroccoTree.Branch branch) {
			BranchInfo branchInfo = new BranchInfo ();

			// Welding Distance.
			float weldingDistance = 0f;
			if (branch.parent != null) {
				weldingDistance = branch.parent.GetGirthAtPosition (branch.position) * GetWeldingDistanceFactor (branch) * 2f;
				if (weldingDistance > branch.length) weldingDistance = branch.length;
			}
			branchInfo.weldingDistance = weldingDistance;

			// Welding Upper Spread.
			float weldingUpperSpread = 0f;
			weldingUpperSpread = GetWeldingUpperSpreadFactor (branch);
			branchInfo.weldingUpperSpread = weldingUpperSpread;

			// Welding Lower Spread.
			float weldingLowerSpread = 0f;
			weldingLowerSpread = GetWeldingLowerSpreadFactor (branch);
			branchInfo.weldingLowerSpread = weldingLowerSpread;

			if (weldingDistance > 0f) {
				if (_branchInfos.ContainsKey (branch.id)) {
					_branchInfos.Remove (branch.id);
				}
				_branchInfos.Add (branch.id, branchInfo);
			}
			return branchInfo;
		}
		/// <summary>
		/// Return a branch info registered for a given branch.
		/// </summary>
		/// <param name="branch">Branch using the branch info object.</param>
		/// <returns>Branch info object or null if not found.</returns>
		public BranchInfo GetBranchInfo (int branchId) {
			BranchInfo branchInfo = null;
			if (_branchInfos.ContainsKey (branchId)) {
				return _branchInfos [branchId];
			}
			return branchInfo;
		}
		public float GetWeldingDistanceFactor (BroccoTree.Branch branch) {
			float weldingDistanceFactor = 1f;
			if (branch.isRoot) {
				weldingDistanceFactor = Random.Range (minRootWeldingDistance, maxRootWeldingDistance);
			} else {
				weldingDistanceFactor = Random.Range (minBranchWeldingDistance, maxBranchWeldingDistance);
			}
			return weldingDistanceFactor;
		}
		public float GetWeldingUpperSpreadFactor (BroccoTree.Branch branch) {
			float weldingUpperSpreadFactor = 1f;
			if (branch.isRoot) {
				weldingUpperSpreadFactor = Random.Range (minRootWeldingUpperSpread, maxRootWeldingUpperSpread);
			} else {
				weldingUpperSpreadFactor = Random.Range (minBranchWeldingUpperSpread, maxBranchWeldingUpperSpread);
			}
			return weldingUpperSpreadFactor;
		}
		public float GetWeldingLowerSpreadFactor (BroccoTree.Branch branch) {
			float weldingLowerSpreadFactor = 1f;
			if (branch.isRoot) {
				weldingLowerSpreadFactor = Random.Range (minRootWeldingLowerSpread, maxRootWeldingLowerSpread);
			} else {
				weldingLowerSpreadFactor = Random.Range (minBranchWeldingLowerSpread, maxBranchWeldingLowerSpread);
			}
			return weldingLowerSpreadFactor;
		}
		#endregion

		#region Vertices
		public virtual Vector3[] GetPolygonAt (
			BranchMeshBuilder.BranchSkin branchSkin,
			int segmentIndex,
			ref List<float> radialPositions,
			float scale,
			float radiusScale = 1f)
		{
			return null;
			/*
			return GetPolygonAt (branchSkin.id, branchSkin.centers [segmentIndex], branchSkin.directions [segmentIndex], branchSkin.normals [segmentIndex],
				branchSkin.girths [segmentIndex], branchSkin.segments [segmentIndex], ref radialPositions, scale, radiusScale);
			*/
		}
		float SetWeldingPolygon (
			int numSegments,
			float weldingAngle,
			float weldingLerp,
			BranchMeshBuilder.BranchSkin childBranchSkin,
			BroccoTree.Branch childBranch,
			BranchMeshBuilder.BranchSkin parentBranchSkin,
			BroccoTree.Branch parentBranch,
			BranchInfo branchInfo,
			List<Vector3> vertices,
			List<Vector3> normals,
			List<Vector4> uvs,
			List<Vector4> uv3s,
			List<Vector4> uv5s,
			List<Vector4> uv6s,
			List<Vector4> uv7s,
			out Vector3 weldingNormal)
		{
			//float positionAtParentBranchSkin = BranchMeshBuilder.BranchSkin.TranslateToPositionAtBranchSkin (childBranch.position, childBranch.parent.id, parentBranchSkin);
			//float positionAtParentBranchSkin = BranchMeshBuilder.BranchSkin.TranslateToPositionAtBranchSkin (childBranch.position, parentBranch, parentBranch, parentBranchSkin);
			float positionAtParentBranchSkin = BranchMeshBuilder.BranchSkin.TranslateToPositionAtBranchSkin (childBranch.position, childBranch.parent, parentBranch, parentBranchSkin);
			// Correct positionAtBranchSkin based on the child branch intersection with the mesh girth.
			MeshCollider meshCollider = GenerateBranchMeshCollider (positionAtParentBranchSkin, 0.1f, parentBranch, parentBranchSkin);
			// Get point at 0.
			Vector3 pointAtOrigin = childBranchSkin.GetPointAtPosition (0f, childBranch);
			// Get point at branchGirth.
			float girthAtParent = parentBranchSkin.GetGirthAtPosition (positionAtParentBranchSkin, parentBranch);
			Vector3 pointAtGirth = childBranchSkin.GetPointAtLength (girthAtParent, childBranch);
			// Adjust posiionAtBranchSkin measuring the distance between center and atGirth.
			Vector3 differenceDirection = parentBranchSkin.GetDirectionAtPosition (positionAtParentBranchSkin, parentBranch);
			float lengthDiff = Vector3.Dot (differenceDirection, pointAtOrigin - pointAtGirth);
			positionAtParentBranchSkin += -lengthDiff / parentBranchSkin.length;
			// Raycast direction.
			//Vector3 rayDirection = (pointAtGirth - pointAtOrigin).normalized;
			Vector3 parentDirection = parentBranchSkin.GetDirectionAtPosition (positionAtParentBranchSkin, parentBranch);
			Vector3 parentNormal = parentBranchSkin.GetNormalAtPosition (positionAtParentBranchSkin, parentBranch);
			Vector3 rayDirection = Quaternion.AngleAxis (childBranch.rollAngle * Mathf.Rad2Deg, parentDirection) * parentNormal;
			weldingNormal = rayDirection;

			float girthAtChild = childBranchSkin.GetGirthAtLength (girthAtParent, childBranch);

			int polygonSides = numSegments;
			Vector3 vertex;
			Vector3 vertexNormal;
			float localGirth = girthAtChild * globalScale;
			float radialPosition;
			RaycastHit hitInfo = new RaycastHit ();

			if (polygonSides >= 3) {
				//Vector3 center = childBranch.GetPointAtPosition (0f) * globalScale;
				Vector3 center = parentBranchSkin.GetPointAtPosition (positionAtParentBranchSkin, parentBranch) * globalScale;
				Vector3 direction = rayDirection;
				Vector3 normal = childBranch.GetNormalAtPosition (0f);
				Vector3 firstVertex = Vector3.zero;
				Vector3 firstNormal = Vector3.zero;

				float angle = (Mathf.PI * 2) / (float)polygonSides;
				float radialAngle = 1f / (float)polygonSides;
				float spread;
				// Add cap if enabled.
				if ((childBranch.isRoot && useRootWeldingMeshCap) || (!childBranch.isRoot && useBranchWeldingMeshCap)) {
					for (int i = 0; i <= polygonSides; i++) {
						radialPosition = i * radialAngle;
						vertex = center;
						vertexNormal = -rayDirection.normalized;
						// Assign point and normal values.
						vertices.Add (vertex);
						normals.Add (vertexNormal);
						uvs.Add (new Vector4 (radialPosition, childBranchSkin.lengthOffset, 0f, 0f));
						uv3s.Add (new Vector4 (vertex.x, vertex.y, vertex.z, 0f));
						uv5s.Add (new Vector4 (radialPosition, childBranchSkin.lengthOffset, girthAtChild, 0f));
						uv6s.Add (new Vector4 (childBranch.id, childBranchSkin.id, childBranch.helperStructureLevelId, 0f));
						uv7s.Add (center);
					}
				}
				// Add surface segment.
				for (int i = 0; i <= polygonSides; i++) {
					if (i != polygonSides) {
						if (angle * i < Mathf.PI) spread = branchInfo.weldingLowerSpread;
						else spread = branchInfo.weldingUpperSpread;
						float welding = Mathf.Lerp (1f, spread, weldingLerp);
						// Calculate vertex and normal.
						vertex = new Vector3 (
							Mathf.Cos ((angle * i) - weldingAngle),
							Mathf.Sin ((angle * i) - weldingAngle) * welding,
							0f);
						radialPosition = i * radialAngle;
						// Welding angle rotation.
						Vector2 circPoint = vertex;
						vertex.x = circPoint.x * Mathf.Cos (weldingAngle) - circPoint.y * Mathf.Sin (weldingAngle);
						vertex.y = circPoint.x * Mathf.Sin (weldingAngle) + circPoint.y * Mathf.Cos (weldingAngle);
						vertex.x *= localGirth;
						vertex.y *= localGirth;

						// Direction and normal rotation.
						Quaternion rotation = Quaternion.LookRotation (direction, normal);
						vertex = (rotation * vertex) + center;
						vertexNormal = (vertex - center).normalized;

						if (meshCollider.Raycast (new Ray (vertex, rayDirection), out hitInfo, girthAtParent * 12f)) {
							vertex = hitInfo.point;
							vertexNormal = hitInfo.normal;
						} else if (meshCollider.Raycast (new Ray (center, vertex - center), out hitInfo, girthAtParent * 12f)) {
							vertex = hitInfo.point;
							vertexNormal = hitInfo.normal;
						}
					} else {
						vertex = firstVertex;
						vertexNormal = firstNormal;
						radialPosition = i * radialAngle;
					}

					// Assign point and normal values.
					vertices.Add (vertex);
					normals.Add (-vertexNormal);
					if (i == 0) {
						firstVertex = vertex;
						firstNormal = vertexNormal;
					}
					uvs.Add (new Vector4 (radialPosition, childBranchSkin.lengthOffset, 0f, 0f));
					uv3s.Add (new Vector4 (vertex.x, vertex.y, vertex.z, 0f));
					uv5s.Add (new Vector4 (radialPosition, childBranchSkin.lengthOffset, girthAtChild, 0f));
					uv6s.Add (new Vector4 (childBranch.id, childBranchSkin.id, childBranch.helperStructureLevelId, 0f));
					uv7s.Add (center);
				}
			}
			return girthAtParent;
		}
		/// <summary>
		/// Get an array of vertices around a center point with some rotation.
		/// </summary>
		/// <returns>Vertices for the polygon <see cref="System.Collections.Generic.List`1[[UnityEngine.Vector3]]"/> points.</returns>
		/// <param name="center_pos">Center of the polygon.</param>
		/// <param name="direction_posAtSkin">Look at rotation.</param>
		/// <param name="girth">Radius of the polygon.</param>
		/// <param name="polygonSides">Number of sides for the polygon.</param>
		void SetPolygonAt (
			int branchSkinId,
			int branchId,
			int structId,
			int numSegments,
			int segType,
			Vector3 center,
			float posAtBranch,
			Vector3 direction,
			float posAtSkin,
			Vector3 normal,
			float girth,
			float lengthOffset,
			int segmentBuilder,
			List<Vector3> vertices,
			List<Vector3> normals,
			List<Vector4> uvs,
			List<Vector4> uv3s,
			List<Vector4> uv5s,
			List<Vector4> uv6s,
			List<Vector4> uv7s,
			float weldingAngle,
			float inBranchWeldingLerp,
			float hierarchyWeldingLerp,
			Vector3 weldingNormal,
			BranchInfo branchInfo)
		{
			center *= globalScale;
			float localGirth = girth * globalScale;

			Vector3 vertex;
			Vector3 vertexNormal = Vector3.zero;
			float radialPosition;
			int indexPos = 0;
			int polygonSides = numSegments;
			int segmentType = segType;
			if (polygonSides >= 3) {
				Vector3 firstVertex = Vector3.zero;
				Vector3 firstNormal = Vector3.zero;
				float angle = (Mathf.PI * 2) / (float)polygonSides;
				float radialAngle = 1f / (float)polygonSides;
				float spread;
				for (int i = 0; i <= polygonSides; i++) {
					if (i != polygonSides) {
						if (angle * i < Mathf.PI) spread = branchInfo.weldingLowerSpread;
						else spread = branchInfo.weldingUpperSpread;
						float welding = Mathf.Lerp (1f, spread, hierarchyWeldingLerp);
						vertex = new Vector3 (
							Mathf.Cos ((angle * i) - weldingAngle),
							Mathf.Sin ((angle * i) - weldingAngle) * welding,
							0f);
						radialPosition = i * radialAngle;
						// Welding angle rotation.
						// x' = x*cos(θ)-y*sin(θ)
						// y' = x*sin(θ)+y*cos(θ)
						Vector2 circPoint = vertex;
						vertex.x = circPoint.x * Mathf.Cos (weldingAngle) - circPoint.y * Mathf.Sin (weldingAngle);
						vertex.y = circPoint.x * Mathf.Sin (weldingAngle) + circPoint.y * Mathf.Cos (weldingAngle);
						vertex.x *= localGirth;
						vertex.y *= localGirth;
						Quaternion rotation = Quaternion.LookRotation (Vector3.Lerp (weldingNormal, direction, 1f - inBranchWeldingLerp), normal);
						vertex = (rotation * vertex) + center;
						vertexNormal = (vertex - center).normalized;
					} else {
						vertex = firstVertex;
						vertexNormal = firstNormal;
						radialPosition = i * radialAngle;
					}
					// Assign point and normal values.
					vertices.Add (vertex);
					normals.Add (vertexNormal);
					if (i == 0) {
						firstVertex = vertex;
						firstNormal = vertexNormal;
					}
					uvs.Add (new Vector4 (radialPosition, lengthOffset, 0f, 0f));
					uv3s.Add (new Vector4 (vertex.x, vertex.y, vertex.z, 0f));
					uv5s.Add (new Vector4 (radialPosition, lengthOffset, girth, 0f));
					uv6s.Add (new Vector4 (branchId, branchSkinId, structId, 0f));
					uv7s.Add (center);
					indexPos++;
				}
			}
		}
		/// <summary>
		/// Gets the number of segments (like polygon sides) as resolution for a branch position.
		/// </summary>
		/// <param name="branchSkin">BranchSkin instance.</param>
		/// <param name="branchSkinPosition">Position along the BranchSkin instance.</param>
		/// <param name="branchAvgGirth">Branch average girth.</param>
		/// <returns>The number polygon sides.</returns>
		public virtual int GetNumberOfSegments (BranchMeshBuilder.BranchSkin branchSkin, float branchSkinPosition, float branchAvgGirth) {
			// Returns 0 for every segment except for the last welding segment.
			// This way the default mesh builder starts building right after the welding range.
			// The welding branch builder builds the welding mesh right after all branch meshes has been build.
			BranchInfo branchInfo = GetBranchInfo (branchSkin.id);
			if (branchInfo != null && 
				Mathf.Approximately (branchSkinPosition, branchInfo.weldingBranchSkinEndPosition))
			{
				float girthPosition = (branchAvgGirth - branchSkin.minAvgGirth) / (branchSkin.maxAvgGirth - branchSkin.minAvgGirth);
				branchSkin.polygonSides = Mathf.Clamp (
					Mathf.RoundToInt (
						Mathf.Lerp (
							branchSkin.minPolygonSides,
							branchSkin.maxPolygonSides,
							girthPosition)), 
							branchSkin.minPolygonSides,
							branchSkin.maxPolygonSides);
				return branchSkin.polygonSides;
			}
			return 0;
		}
		int GetNumberOfSegmentsInWelding (BranchMeshBuilder.BranchSkin branchSkin, float branchSkinPosition, float branchAvgGirth) {
			float girthPosition = (branchAvgGirth - branchSkin.minAvgGirth) / (branchSkin.maxAvgGirth - branchSkin.minAvgGirth);
			branchSkin.polygonSides = Mathf.Clamp (
				Mathf.RoundToInt (
					Mathf.Lerp (
						branchSkin.minPolygonSides,
						branchSkin.maxPolygonSides,
						girthPosition)), 
						branchSkin.minPolygonSides,
						branchSkin.maxPolygonSides);
			return branchSkin.polygonSides;
		}
		#endregion

		#region Mesh Collider Methods
		/// <summary>
		/// Generates a mesh collider for a segment range on a BranchSkin instance
		/// </summary>
		/// <param name="positionAtSkin">Initial position at the BranchSkin instance.</param>
		/// <param name="range">Range up and down the position.</param>
		/// <param name="branchSkin">BranchSkin instance.</param>
		public MeshCollider GenerateBranchMeshCollider (float positionAtSkin, float range, BroccoTree.Branch firstBranch, BranchMeshBuilder.BranchSkin branchSkin) {
			// Get collider component.
			MeshCollider meshCollider = TreeFactory.GetActiveInstance ().GetMeshCollider ();
			Vector3 factoryOffset = TreeFactory.GetActiveInstance ().gameObject.transform.position;

			int defaultNumSeg = branchSkin.maxPolygonSides;

			// Get bottom and top segments.
			float bottomRange = positionAtSkin - range;
			float topRange = positionAtSkin + range;
			List<float> segmentPositions = new List<float> ();
			List<int> segments = new List<int> ();
			float posAtSkin;
			segmentPositions.Add (bottomRange);
			int lastI = 0;
			for (int i = 0; i < branchSkin.segments.Count; i++) {
				posAtSkin = branchSkin.positionsAtSkin [i];
				if (posAtSkin > bottomRange && posAtSkin < topRange) {
					if (segments.Count == 0) segments.Add (branchSkin.segments [i] == 0?defaultNumSeg:branchSkin.segments [i]);
					segments.Add (branchSkin.segments [i] == 0?defaultNumSeg:branchSkin.segments [i]);
					segmentPositions.Add (posAtSkin);
					lastI = i;
				}
			}
			segments.Add (branchSkin.segments [lastI]==0?defaultNumSeg:branchSkin.segments [lastI]);
			segmentPositions.Add (topRange);

			// Build mesh.
			List<Vector3> vertices = new List<Vector3> ();
			List<int> triangles = new List<int> ();
			List<float> radialPos = new List<float> ();
			int topIndex = 0;
			int baseIndex = 0;
			int prevSegmentSideCount;

			for (int i = 0; i < segments.Count; i++) {
				// Set the segment index.
				int currSegmentSideCount = segments[i];

				SetSimplePolygonAt (
					currSegmentSideCount,
					branchSkin.GetPointAtPosition (segmentPositions [i], firstBranch),
					branchSkin.GetGirthAtPosition (segmentPositions [i], firstBranch),
					branchSkin.GetDirectionAtPosition (segmentPositions [i], firstBranch),
					branchSkin.GetNormalAtPosition (segmentPositions [i], firstBranch),
					vertices,
					radialPos,
					factoryOffset
				);
			}
			for (int i = 1; i < segments.Count; i++) {
				int currSegmentSideCount = segments[i];
				// Add triangles.
				prevSegmentSideCount = segments[i-1];
				topIndex += prevSegmentSideCount + 1;
				// Add triangles when current and previous segment have the same number of sides.
				if (prevSegmentSideCount == currSegmentSideCount) {
					for (int j = 0; j < prevSegmentSideCount; j++) {
						triangles.Add (baseIndex + j);
						triangles.Add (topIndex + j);
						triangles.Add (topIndex + j + 1);
						triangles.Add (baseIndex + j);
						triangles.Add (topIndex + j + 1);
						triangles.Add (baseIndex + j + 1);
					}
				} else {
					// Add triangles when one current and previous segment have a different number of sides.
					int aSegmentIndex; // A segment has the fewer number of sides.
					int bSegmentIndex; // B segment has the greater number of sides.
					bool inverse = false;
					if (prevSegmentSideCount > currSegmentSideCount) {
						aSegmentIndex = topIndex; // A segment has the fewer number of sides.
						bSegmentIndex = baseIndex; // B segment has the greater number of sides.
					} else {
						aSegmentIndex = baseIndex; // A segment has the fewer number of sides.
						bSegmentIndex = topIndex; // B segment has the greater number of sides.	
						inverse = true;
					}
					float aSegmentValue = radialPos [aSegmentIndex]; // A segment index radial value.
					float bSegmentValue = radialPos [bSegmentIndex]; // B segment index radial value.
					float halfA = (aSegmentValue + radialPos [aSegmentIndex + 1]) /2f; // half value between segment A index and segment A index + 1.
					while (bSegmentValue < 1) {
						if (bSegmentValue < halfA) {
							// Segment B
							triangles.Add (bSegmentIndex);
							if (inverse) {
								triangles.Add (bSegmentIndex + 1);
								triangles.Add (aSegmentIndex);
							} else {
								triangles.Add (aSegmentIndex);
								triangles.Add (bSegmentIndex + 1);
							}
							bSegmentIndex++;
							bSegmentValue = radialPos [bSegmentIndex];
						} else {
							// Segment A
							triangles.Add (bSegmentIndex);
							if (inverse) {
								triangles.Add (aSegmentIndex);
								triangles.Add (aSegmentIndex + 1);
							} else {
								triangles.Add (aSegmentIndex);
								triangles.Add (aSegmentIndex + 1);
							}
							aSegmentIndex++;
							aSegmentValue = radialPos [aSegmentIndex];
							if (aSegmentValue < 1f) {
								halfA = (aSegmentValue + radialPos [aSegmentIndex + 1]) /2f;
							} else {
								halfA = aSegmentValue + 0.2f;
							}
						}
					}
					if (aSegmentValue < 1f) {
						triangles.Add (bSegmentIndex);
						if (inverse) {
							triangles.Add (aSegmentIndex);
							triangles.Add (aSegmentIndex + 1);
						} else {
							triangles.Add (aSegmentIndex);
							triangles.Add (aSegmentIndex + 1);
						}
					}
				}
				baseIndex = topIndex;
			}
			Mesh mesh = new Mesh ();
			mesh.vertices = vertices.ToArray ();
			mesh.triangles = triangles.ToArray ();
			meshCollider.sharedMesh = mesh;

			return meshCollider;
		}
		/// <summary>
		/// Get an array of vertices around a center point with some rotation.
		/// </summary>
		/// <returns>Vertices for the polygon <see cref="System.Collections.Generic.List`1[[UnityEngine.Vector3]]"/> points.</returns>
		/// <param name="center_pos">Center of the polygon.</param>
		/// <param name="direction_posAtSkin">Look at rotation.</param>
		/// <param name="girth">Radius of the polygon.</param>
		/// <param name="polygonSides">Number of sides for the polygon.</param>
		void SetSimplePolygonAt (
			int numSegments,
			Vector3 center,
			float girth,
			Vector3 direction,
			Vector3 normal,
			List<Vector3> vertices,
			List<float> radialPos,
			Vector3 factoryOffset)
		{
			center *= globalScale;
			float localGirth = girth * globalScale;

			Vector3 vertex;
			if (numSegments >= 3) {
				float radialStep = 1f / (float)numSegments;
				float angleStep = (Mathf.PI * 2) / (float)numSegments;
				for (int i = 0; i <= numSegments; i++) {
					// Calculate vertex.
					vertex = new Vector3 (
						Mathf.Cos (angleStep * i) * localGirth,
						Mathf.Sin (angleStep * i) * localGirth,
						0f);
					//Quaternion rotation = Quaternion.LookRotation (direction_posAtSkin, normal_girth);
					Quaternion rotation = Quaternion.LookRotation (direction, normal);
					vertex = (rotation * vertex) + center;
					// Assign point.
					vertices.Add (vertex - factoryOffset);
					radialPos.Add (i * radialStep);
				}
			}
		}
		#endregion
	}
}