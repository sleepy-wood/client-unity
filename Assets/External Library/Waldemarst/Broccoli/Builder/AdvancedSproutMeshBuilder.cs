using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

using Broccoli.Base;
using Broccoli.Model;
using Broccoli.Utils;
using Broccoli.Pipe;

namespace Broccoli.Builder
{
	/// <summary>
	/// Mesh building for sprouts.
	/// </summary>
	public class AdvancedSproutMeshBuilder {
		#region Jobs
		/// <summary>
		/// Job structure to process branch skins.
		/// </summary>
		struct SproutJob : IJobParallelFor {
			#region Input
			/// <summary>
			/// Contains the POSITION (x, y, z) and SCALE (w) of the sprout.
			/// </summary>
			public NativeArray<Vector4> sproutPositionScale;
			/// <summary>
			/// Contains the DIRECTION of the sprout.
			/// </summary>
			public NativeArray<Vector3> sproutDirection;
			/// <summary>
			/// Contains the NORMAL of the sprout.
			/// </summary>
			public NativeArray<Vector3> sproutNormal;
			/// <summary>
			/// Contains the BRANCH_ID, BRANCH_POS, MAX_LENGTH_FROM_ANCHOR and SPROUT_RANDOM of the sprout.
			/// </summary>
			public NativeArray<Vector4> branchIdBranchPosMaxLengthRand;
			/// <summary>
			/// Contains the SPROUT_ANCHOR and SPROUT_BENDING.
			/// </summary>
			public NativeArray<Vector4> branchAnchorSproutBending;
			#endregion

			#region Mesh Input
			/// <summary>
			/// Vertices for the input mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector3> inputVertices;
			/// <summary>
			/// Normals for the input mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector3> inputNormals;
			/// <summary>
			/// Tangents for the input mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> inputTangents;
			/// <summary>
			/// Triangles for the input mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<int> inputTris;
			/// <summary>
			/// UVs for the input mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> inputUVs;
			/*
			/// <summary>
			/// UV2s for the input mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> inputUV2s;
			*/
			#endregion

			#region Mesh Output
			/// <summary>
			/// Vertices for the output mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector3> outputVertices;
			/// <summary>
			/// Normals for the output mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector3> outputNormals;
			/// <summary>
			/// Tangents for the output mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> outputTangents;
			/// <summary>
			/// The triangles array for the output mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<int> outputTris;
			/// <summary>
			/// UVs for the output mesh.
			/// x: U mapping value.
			/// y: V mapping value.
			/// z: sprout anchor gradient.
			/// w: sprout random value.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> outputUVs;
			/// <summary>
			/// UV2s for the output mesh.
			/// xyz: sprout anchor point.
			/// w: sprout relative position on the branch
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> outputUV2s;
			/// <summary>
			/// UV3s for the output mesh.
			/// xyz: vertex value.
			/// w: branch id.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> outputUV3s;
			/// <summary>
			/// UV6s for the output mesh.
			/// x: branch id.
			/// y: unassigned.
			/// z: unassigned.
			/// w: unassigned.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> outputUV6s;
			#endregion

			#region Job Methods
			/// <summary>
			/// Executes one per sprout.
			/// </summary>
			/// <param name="i"></param>
			public void Execute (int i) {
				Vector3 spOffset = (Vector3)sproutPositionScale [i];
				Vector3 spDirection = sproutDirection [i];
				float spScale = sproutPositionScale [i].w;
				Vector3 spNormal = sproutNormal[i];
				int brId = (int)branchIdBranchPosMaxLengthRand [i].x;
				float brPos = branchIdBranchPosMaxLengthRand [i].y;
				int vertexStart = i * inputVertices.Length;
				int vertexEnd = (i * inputVertices.Length) + inputVertices.Length;
				int trisStart = i * inputTris.Length;
				int trisEnd = (i * inputTris.Length) + inputTris.Length;
				float bMaxLength = branchIdBranchPosMaxLengthRand [i].z <= 0 ? 0.1f : branchIdBranchPosMaxLengthRand [i].z;
				float rand = branchIdBranchPosMaxLengthRand [i].w;
				Vector3 anchor = branchAnchorSproutBending [i];
				float sproutBending = branchAnchorSproutBending [i].w;

				// Clone the mesh vertices, normals and tangents (to scale, rotate, bend and translate).
				Vector3 [] _vertices = new Vector3[inputVertices.Length];
				inputVertices.CopyTo (_vertices);
				Vector3 [] _normals = inputNormals.ToArray ();
				Vector4 [] _tangents = inputTangents.ToArray ();
				Vector4 [] _uvs = inputUVs.ToArray ();
				Vector4 [] _uv2s = new Vector4 [inputUVs.Length];
				Vector4 [] _uv3s = new Vector4 [inputUVs.Length];
				Vector4 [] _uv6s = GetUV6s (inputVertices.Length, brId);
				int[] _tris = inputTris.ToArray ();

				// Apply the transformations.
				SetUVs (ref _uvs, ref _vertices, bMaxLength, rand);
				SetUV2s (ref _uv2s, anchor, brPos);
				BendSprout (ref _vertices, ref _normals, ref _tangents, sproutBending, bMaxLength);
				ScaleSprout (ref _vertices, spScale);
				RotateSprout (ref _vertices, ref _normals, ref _tangents, spDirection, spNormal);
				TranslateSprout (ref _vertices, spOffset);
				SetUV3s (ref _uv3s, ref _vertices, brId);

				// Add the values to the output mesh vars.
				int k = 0;
				for (int j = vertexStart; j < vertexEnd; j++) {
					outputVertices [j] = _vertices [k];
					outputNormals [j] = _normals [k];
					outputTangents [j] = _tangents [k];
					outputUVs [j] = _uvs [k];
					outputUV2s [j] = _uv2s [k];
					outputUV3s [j] = _uv3s [k];
					outputUV6s [j] = _uv6s [k];
					k++;
				}
				k = 0;
				for (int j = trisStart; j < trisEnd; j++) {
					outputTris [j] = _tris [k] + vertexStart;
					k++;
				}
			}
			/// <summary>
			/// Set UVs to a sprout.
			/// </summary>
			/// <param name="uvs">Base UVs.</param>
			/// <param name="vertices">Vertices.</param>
			/// <param name="bMaxLength">Max length from the sprout anchor point.</param>
			/// <param name="rand">Random value for the sprout.</param>
			public void SetUVs (ref Vector4[] uvs, ref Vector3[] vertices, float bMaxLength, float rand) {
				for (int i = 0; i < uvs.Length; i++) {
					uvs[i].z = (vertices [i].magnitude) / bMaxLength;
					uvs[i].w = rand;
				}
			}
			/// <summary>
			/// Set UV2s to a sprout.
			/// </summary>
			/// <param name="uv2s">Base UV2s.</param>
			/// <param name="anchor">Anchor point.</param>
			/// <param name="branchPosition">sprout relative position in the branch.</param>
			public void SetUV2s (ref Vector4[] uv2s, Vector3 anchor, float branchPosition) {
				for (int i = 0; i < uv2s.Length; i++) {
					uv2s[i] = anchor;
					uv2s[i].w = branchPosition;
				}
			}
			/// <summary>
			/// Set UV3s to a sprout.
			/// </summary>
			/// <param name="uv3s">Base UV3s.</param>
			/// <param name="vertices">Vertices.</param>
			/// <param name="branchId">Branch id.</param>
			public void SetUV3s (ref Vector4[] uv3s, ref Vector3[] vertices, int branchId) {
				for (int i = 0; i < uv3s.Length; i++) {
					uv3s [i] = vertices [i];
					uv3s [i].w = branchId;
				}
			}
			/// <summary>
			/// Get the UV6 values for the sprouts.
			/// </summary>
			/// <param name="size">Size of the expected UV6 array.</param>
			/// <param name="branchId">Id of the branch.</param>
			/// <returns></returns>
			public Vector4[] GetUV6s (int size, int branchId) {
				Vector4 uv6 = new Vector4 (branchId, 0, 0, 0);
				Vector4[] uv6s = new Vector4 [size];
				for (int i = 0; i < size; i++) {
					uv6s [i] = uv6;
				}
				return uv6s;
			}
			/// <summary>
			/// Applies rotation to vertices, normals and tangents.
			/// </summary>
			/// <param name="vertices">Sprout vertices array.</param>
			/// <param name="normals">Sprout normals array.</param>
			/// <par-am name="tangents">Sprout tangents array.</param>
			/// <param name="direction">Direction (forward) to apply the rotation.</param>
			/// <param name="normal">Normal (up) to apply the rotation.</param>
			public void BendSprout (
				ref Vector3[] vertices, 
				ref Vector3[] normals, 
				ref Vector4[] tangents, 
				float strength,
				float maxLength)
			{
				Vector3 gravityForward = Vector3.forward;
				Vector3 gravityUp = Vector3.up;
				if (strength < 0) {
					gravityForward *= -1;
					gravityUp *= -1;
					strength *= -1;
				}
				Quaternion gravityQuaternion = Quaternion.LookRotation (gravityUp * -1, gravityForward);
				Quaternion bendQuaternion;
				float radialStrength;
				for (int i = 0; i < vertices.Length; i++) {
					radialStrength = strength * vertices[i].magnitude /maxLength;
					bendQuaternion = Quaternion.Slerp (Quaternion.identity, gravityQuaternion, radialStrength);
					vertices [i] = bendQuaternion * vertices [i];
					normals [i] = bendQuaternion * normals [i];
					tangents [i] = bendQuaternion * tangents [i];
				}
			}
			/// <summary>
			/// Scales the sprout points.
			/// </summary>
			/// <param name="vertices">Sprout vertices array.</param>
			/// <param name="scale">Scale.</param>
			public void ScaleSprout (ref Vector3[] vertices, float scale) {
				for (int i = 0; i < vertices.Length; i++) {
					vertices [i] = vertices [i] * scale;
				}
			}
			/// <summary>
			/// Applies rotation to vertices, normals and tangents.
			/// </summary>
			/// <param name="vertices">Sprout vertices array.</param>
			/// <param name="normals">Sprout normals array.</param>
			/// <par-am name="tangents">Sprout tangents array.</param>
			/// <param name="direction">Direction (forward) to apply the rotation.</param>
			/// <param name="normal">Normal (up) to apply the rotation.</param>
			public void RotateSprout (
				ref Vector3[] vertices, 
				ref Vector3[] normals, 
				ref Vector4[] tangents, 
				Vector3 direction,
				Vector3 normal)
			{
				Quaternion rotation = Quaternion.LookRotation (direction, normal);
				for (int i = 0; i < vertices.Length; i++) {
					vertices [i] = rotation * vertices [i];
					normals [i] = rotation * normals [i];
					tangents [i] = rotation * tangents [i];
				}
			}
			/// <summary>
			/// Applies an offset to all the vertices.
			/// </summary>
			/// <param name="vertices">Sprout vertices array.</param>
			/// <param name="offset">Offset to apply.</param>
			public void TranslateSprout (ref Vector3[] vertices, Vector3 offset) {
				for (int i = 0; i < vertices.Length; i++) {
					vertices [i] = vertices [i] + offset;
				}
			}
			#endregion
		}
		#endregion

		#region Vars
		/// <summary>
		/// Scale for all the meshes generated.
		/// </summary>
		public float globalScale = 1.0f;
		/// <summary>
		/// Relationship between registered meshes and their ids.
		/// </summary>
		/// <typeparam name="int">Id of the mesh, compound for groupId * 10000 + subgroupId.</typeparam>
		/// <typeparam name="Mesh">Mesh.</typeparam>
		/// <returns>Relationship between meshes and their ids.</returns>
		Dictionary<int, Mesh> _idToMesh = new Dictionary<int, Mesh> ();
		/// <summary>
		/// Contains the POSITION (x, y, z) and SCALE (w) of the sprout.
		/// </summary>
		private List<Vector4> _sproutPositionScale = new List<Vector4> ();
		/// <summary>
		/// Contains the DIRECTION of the sprout.
		/// </summary>
		private List<Vector3> _sproutDirection = new List<Vector3> ();
		/// <summary>
		/// Contains the NORMAL of the sprout.
		/// </summary>
		private List<Vector3> _sproutNormal = new List<Vector3> ();
		/// <summary>
		/// Contains the BRANCH_ID, BRANCH_POS, MAX_LENGTH_TO_ANCHOR and SPROUT_RANDOM of the sprout.
		/// </summary>
		private List<Vector4> _branchIdBranchPosMaxLengthRand = new List<Vector4> ();
		/// <summary>
		/// Contains the SPROUT_ANCHOR and SPROUT_BENDING.
		/// </summary>
		private List<Vector4> _branchAnchorSproutBending = new List<Vector4> ();
		#endregion

		#region Singleton
		/// <summary>
		/// Singleton for this class.
		/// </summary>
		static AdvancedSproutMeshBuilder _sproutMeshBuilder = null;
		/// <summary>
		/// Gets the singleton instance for this class.
		/// </summary>
		/// <returns>The instance.</returns>
		public static AdvancedSproutMeshBuilder GetInstance() {
			if (_sproutMeshBuilder == null) {
				_sproutMeshBuilder = new AdvancedSproutMeshBuilder ();
			}
			return _sproutMeshBuilder;
		}
		#endregion

		#region Meshes Management
		/// <summary>
		/// Bounds a mesh to  group id and subgroup id.
		/// </summary>
		/// <param name="mesh">Mesh to register.</param>
		/// <param name="groupId">Group id.</param>
		/// <param name="subgroupId">Subgroup id.</param>
		public void RegisterMesh (Mesh mesh, int groupId, int subgroupId = -1) {
			int meshId = GetGroupSubgroupId (groupId, subgroupId);
			if (_idToMesh.ContainsKey (meshId)) {
				UnityEngine.Object.DestroyImmediate (_idToMesh [meshId]);
				_idToMesh.Remove (meshId);
			}
			_idToMesh.Add (meshId, UnityEngine.Object.Instantiate (mesh));
		}
		/// <summary>
		/// Remove all registered meshes.
		/// </summary>
		public void RemoveRegisteredMeshes () {
			var meshEnum = _idToMesh.GetEnumerator ();
			while (meshEnum.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (meshEnum.Current.Value);
			}
			_idToMesh.Clear ();
		}
		/// <summary>
		/// Builds the compound id between groupId/subgroupId.
		/// </summary>
		/// <param name="mesh">Mesh to register.</param>
		/// <param name="groupId">Group id.</param>
		/// <returns>Compound id.</returns>
		private int GetGroupSubgroupId (int groupId, int subgroupId) {
			if (subgroupId < 0) subgroupId = -1;
			int meshId =  groupId * 10000 + (subgroupId + 1);
			return meshId;
		}
		#endregion

		#region Initialization
		/// <summary>
		/// Clear local variables.
		/// </summary>
		public void Clear () {
			RemoveRegisteredMeshes ();
			ClearSproutParams ();
		}
		/// <summary>
		/// Clear the sprout params lists.
		/// </summary>
		private void ClearSproutParams () {
			_sproutPositionScale.Clear ();
			_sproutDirection.Clear ();
			_sproutNormal.Clear ();
			_branchIdBranchPosMaxLengthRand.Clear ();
			_branchAnchorSproutBending.Clear ();
		}
		#endregion

		#region Processing
		/// <summary>
		/// Creates the mesh coming from a sprout group on a tree instance.
		/// </summary>
		/// <returns>The mesh object for the sprouts.</returns>
		/// <param name="tree">Tree object.</param>
		/// <param name="subgroupId">Sprout mesh instance.</param>
		/// <param name="groupId">Sprout group id.</param>
		public Mesh MeshSprouts (BroccoTree tree, SproutMesh sproutMesh, int groupId, int subgroupId = -1) {
			// Mesh to build.
			Mesh groupMesh = new Mesh ();

			// Generate mesh if a registered mesh is available to the groupId/subgroupId.
			int meshId = GetGroupSubgroupId (groupId, subgroupId);
			if (_idToMesh.ContainsKey (meshId)) {
				// Get the mesh bound to this groupId/subgroupId.
				Mesh baseMesh = _idToMesh [meshId];

				// Clear the list to feed the job.
				ClearSproutParams ();

				// Get all branches/roots in the tree to populate sprouts with groupId/subgroupId duple.
				List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
				for (int i = 0; i < branches.Count; i++) {
					for (int j = 0; j < branches[i].sprouts.Count; j++) {
						if (branches[i].sprouts[j].groupId == groupId && 
							(subgroupId>=0?branches[i].sprouts[j].subgroupId == subgroupId:true))
						{
							// Set the sprout horizontal alignment.
							branches[i].sprouts[j].horizontalAlign = 
								Mathf.Lerp (sproutMesh.horizontalAlignAtBase, 
									sproutMesh.horizontalAlignAtTop, 
									branches[i].sprouts[j].position);
							// Recalculate sprouts direction and normal.
							branches[i].sprouts[j].CalculateVectors ();
							// Add sprout bound to the mesh to the job system.
							MeshSprout (branches[i].sprouts[j], branches[i], sproutMesh, baseMesh);
						}
					}
				}

				// Create Job.
				List<Vector4> baseMeshUVs = new List<Vector4> ();
				List<Vector4> baseMeshUV2s = new List<Vector4> ();
				baseMesh.GetUVs (0, baseMeshUVs);
				baseMesh.GetUVs (1, baseMeshUV2s);
				int totalVertices = baseMesh.vertexCount * _sproutPositionScale.Count;
				int totalTriangles = baseMesh.triangles.Length * _sproutPositionScale.Count;
				SproutJob _sproutJob = new SproutJob () {
					// Input.
					sproutPositionScale = new NativeArray<Vector4> (_sproutPositionScale.ToArray (), Allocator.TempJob),
					sproutDirection = new NativeArray<Vector3> (_sproutDirection.ToArray (), Allocator.TempJob),
					sproutNormal = new NativeArray<Vector3> (_sproutNormal.ToArray (), Allocator.TempJob),
					branchIdBranchPosMaxLengthRand = new NativeArray<Vector4> (_branchIdBranchPosMaxLengthRand.ToArray (), Allocator.TempJob),
					branchAnchorSproutBending = new NativeArray<Vector4> (_branchAnchorSproutBending.ToArray (), Allocator.TempJob),

					// Mesh Input.
					inputVertices = new NativeArray<Vector3> (baseMesh.vertices, Allocator.TempJob),
					inputNormals = new NativeArray<Vector3> (baseMesh.normals, Allocator.TempJob),
					inputTangents = new NativeArray<Vector4> (baseMesh.tangents, Allocator.TempJob),
					inputTris = new NativeArray<int> (baseMesh.triangles, Allocator.TempJob),
					inputUVs = new NativeArray<Vector4> (baseMeshUVs.ToArray (), Allocator.TempJob),
					//inputUV2s = new NativeArray<Vector4> (baseMeshUV2s.ToArray (), Allocator.TempJob),

					// Mesh Output.
					outputVertices = new NativeArray<Vector3> (totalVertices, Allocator.TempJob),
					outputNormals = new NativeArray<Vector3> (totalVertices, Allocator.TempJob),
					outputTangents = new NativeArray<Vector4> (totalVertices, Allocator.TempJob),
					outputTris = new NativeArray<int> (totalTriangles, Allocator.TempJob),
					outputUVs = new NativeArray<Vector4> (totalVertices, Allocator.TempJob),
					outputUV2s = new NativeArray<Vector4> (totalVertices, Allocator.TempJob),
					outputUV3s = new NativeArray<Vector4> (totalVertices, Allocator.TempJob),
					outputUV6s = new NativeArray<Vector4> (totalVertices, Allocator.TempJob)
				};

				// Execute the branch jobs.
				JobHandle _sproutJobHandle = _sproutJob.Schedule (_sproutPositionScale.Count, 8);

				// Complete the job.
				_sproutJobHandle.Complete();

				// Copy to mesh.
				Vector3[] _vertices = new Vector3 [totalVertices];
				Vector3[] _normals = new Vector3 [totalVertices];
				Vector4[] _tangents = new Vector4 [totalVertices];
				Vector4[] _uvs = new Vector4 [totalVertices];
				Vector4[] _uv2s = new Vector4 [totalVertices];
				Vector4[] _uv3s = new Vector4 [totalVertices];
				Vector4[] _uv6s = new Vector4 [totalVertices];
				int[] _triangles = new int [totalTriangles];

				_sproutJob.outputVertices.CopyTo (_vertices);
				_sproutJob.outputNormals.CopyTo (_normals);
				_sproutJob.outputTangents.CopyTo (_tangents);
				_sproutJob.outputTris.CopyTo (_triangles);
				_sproutJob.outputUVs.CopyTo (_uvs);
				_sproutJob.outputUV2s.CopyTo (_uv2s);
				_sproutJob.outputUV3s.CopyTo (_uv3s);
				_sproutJob.outputUV6s.CopyTo (_uv6s);

				groupMesh.vertices = _vertices;
				groupMesh.normals = _normals;
				groupMesh.triangles = _triangles;
				groupMesh.tangents = _tangents;
				groupMesh.SetUVs (0, new List<Vector4>(_uvs));
				groupMesh.SetUVs (1, new List<Vector4>(_uv2s));
				groupMesh.SetUVs (2, new List<Vector4>(_uv3s));
				groupMesh.SetUVs (5, new List<Vector4>(_uv6s));

				// Dispose allocated memory.
				_sproutJob.sproutPositionScale.Dispose();
				_sproutJob.sproutDirection.Dispose();
				_sproutJob.sproutNormal.Dispose();
				_sproutJob.branchIdBranchPosMaxLengthRand.Dispose();
				_sproutJob.branchAnchorSproutBending.Dispose();
				_sproutJob.inputVertices.Dispose();
				_sproutJob.inputNormals.Dispose();
				_sproutJob.inputTangents.Dispose();
				_sproutJob.inputTris.Dispose();
				_sproutJob.inputUVs.Dispose();
				//_sproutJob.inputUV2s.Dispose();
				_sproutJob.outputVertices.Dispose();
				_sproutJob.outputNormals.Dispose();
				_sproutJob.outputTangents.Dispose();
				_sproutJob.outputTris.Dispose();
				_sproutJob.outputUVs.Dispose();
				_sproutJob.outputUV2s.Dispose();
				_sproutJob.outputUV3s.Dispose();
				_sproutJob.outputUV6s.Dispose();
			}

			// Return the mesh.
			return groupMesh;
		}
		/// <summary>
		/// Adds information on the mesh creation for a single sprout.
		/// </summary>
		/// <param name="sprout">Sprout instance.</param>
		/// <param name="branch">Parent branch.</param>
		/// <param name="sproutMesh">Sprout mesh instance.</param>
		void MeshSprout (BroccoTree.Sprout sprout, BroccoTree.Branch branch, SproutMesh sproutMesh, Mesh baseMesh) {
			float scale = Mathf.Lerp (sproutMesh.scaleAtBase, sproutMesh.scaleAtTop, 
				Mathf.Clamp (sproutMesh.scaleCurve.Evaluate(sprout.preferedPosition), 0f, 1f));
			Vector3 pos = sprout.inGirthPosition * globalScale;
			_sproutPositionScale.Add (new Vector4 (pos.x, pos.y, pos.z, scale * globalScale));

			_sproutDirection.Add (sprout.sproutDirection);
			_sproutNormal.Add (sprout.sproutNormal);
			float maxLength = baseMesh.bounds.max.magnitude * scale * globalScale;
			float randSprout = Random.Range (0f, 1f);
			_branchIdBranchPosMaxLengthRand.Add (new Vector4 (branch.id, sprout.position, maxLength, randSprout));
			Vector4 sproutAnchorSproutBending = branch.GetPointAtPosition (sprout.position) * globalScale;
			float sproutBending = Mathf.Lerp (sproutMesh.gravityBendingAtBase, 
				sproutMesh.gravityBendingAtTop, sprout.hierarchyPosition);
			sproutBending = sproutMesh.gravityBendingCurve.Evaluate (sproutBending);
			sproutAnchorSproutBending.w = sproutBending;
			_branchAnchorSproutBending.Add (sproutAnchorSproutBending);

		}
		#endregion
	}
}