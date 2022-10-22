using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

using Broccoli.Model;
using Broccoli.Manager;

namespace Broccoli.Builder
{
	/// <summary>
	/// Wind meta builder.
	/// Analyzes trees to provide wind weight based on UV2 and colors channels.
	/// </summary>
	/// Leaves UV ch0
	/// z: Up/down branch swing
	/// Leaves UV4 ch.3 
	/// x: leaf sway factor with gradient from leaves origin.
	public class STWindMetaBuilder {
		#region Wind Jobs
		struct BranchWindJob : IJobParallelFor {
			/// <summary>
			/// Vertices for the mesh.
			/// </summary>
			public NativeArray<Vector3> vertices;
			/// <summary>
			/// UV information of the mesh.
			/// x: mapping U component.
			/// y: mapping V component.
			/// z: Adds wind data: gradient from trunk to branch tip.
			/// w: Adds wind data: branch swing phase. (0-15).
			/// </summary>
			public NativeArray<Vector4> uvs;
			/// <summary>
			/// UV2 information of the mesh.
			/// x: Unalloc: normalized U on leaves.
			/// y: Unalloc: normalized V on leaves.
			/// z: Adds wind data: x phased values (0-5).
			/// w: Adds wind data: y phased values (0-14).
			/// </summary>
			public NativeArray<Vector4> uv2s;
			/// <summary>
			/// UV5 information of the mesh.
			/// x: radial position.
			/// y: global length position.
			/// z: girth.
			/// w: unallocated.
			/// </summary>
			public NativeArray<Vector4> uv5s;
			/// <summary>
			/// UV6 information of the mesh.
			/// x: id of the branch.
			/// y: id of the branch skin.
			/// z: id of the struct.
			/// w: tuned.
			/// </summary>
			public NativeArray<Vector4> uv6s;
			/// <summary>
			/// UV7 information of the mesh.
			/// x, y, z: center.
			/// w: unallocated.
			/// </summary>
			public NativeArray<Vector4> uv7s;
			/// <summary>
			/// Saves the branch phase for each vertex.
			/// x: phase value.
			/// y: phase min length.
			/// z: phase max length.
			/// </summary>
			public NativeArray<Vector3> branchPhases;
			public bool isST7;
			public float windAmplitude;
			public float branchSway;
			public void Execute(int i) {
				// Get channel values.
				Vector3 vertex = vertices[i];
				Vector4 uv = uvs[i];
				Vector4 uv2 = uv2s[i];
				Vector4 uv5 = uv5s[i];
				Vector4 uv6 = uv6s[i];
				Vector4 uv7 = uv7s[i];
				Vector3 branchPhase = branchPhases [i];

				// Set UV
				if (branchPhase.x > 0) {
					uv.z = (uv5.y - branchPhase.y) / (branchPhase.z - branchPhase.y) * branchSway; // Length from tree origin (0-1)
					uv.w = branchPhase.x;
				} else {
					uv.z = 0f;
					uv.w = 0.5f;
				}
				//uv.z = 0f;
				

				if (isST7) {
					// Set UV2, holds vertex position.
					uv2 = vertex;
					uv2.y *= windAmplitude;
					uv2.w = 0;
				} else {
					// Set UV2.
					uv2.z = uv7.x;
					uv2.w = uv7.y;
				}
				uvs [i] = uv;
				uv2s [i] = uv2;
			}
		}
		#endregion

		#region Vars
		/// <summary>
		/// The branches on the analyzed tree.
		/// </summary>
		/// <typeparam name="int">Id of the branch.</typeparam>
		/// <typeparam name="BroccoTree.Branch">Branch.</typeparam>
		/// <returns>Branch given its id.</returns>
		public Dictionary<int, BroccoTree.Branch> branches = new Dictionary<int, BroccoTree.Branch> ();
		/// <summary>
		/// The wind factor used to multiply the UV2 value.
		/// </summary>
		public float windSpread = 1f;
		/// <summary>
		/// The wind amplitude.
		/// </summary>
		float _windAmplitude = 0f;
		/// <summary>
		/// Gets or sets the wind resistance.
		/// </summary>
		/// <value>The wind resistance.</value>
		public float windAmplitude {
			get { return _windAmplitude; }
			set {
				weightCurve = AnimationCurve.EaseInOut (value, 0f, 1f, 1f);
				_windAmplitude = value;
			}
		}
		public float sproutTurbulence = 1f;
		public float sproutSway = 1f;
		/// <summary>
		/// The weight curve used to get the UV2 values for wind.
		/// </summary>
		public AnimationCurve weightCurve;
		public AnimationCurve weightSensibilityCurve = null;
		public AnimationCurve weightAngleCurve = null;
		public bool useMultiPhaseOnTrunk = true;
		public bool isST7 = false;
		public float branchSway = 1f;
		/// <summary>
		/// True to apply wind mapping to roots.
		/// </summary>
		public bool applyToRoots = false;
		/// <summary>
		/// Relationship between branches given their id and the branch skin they belong to.
		/// </summary>
		/// <typeparam name="int">Branch id.</typeparam>
		/// <typeparam name="BranchMeshBuilder.BranchSkin">Branch skin instance.</typeparam>
		private Dictionary<int, BranchMeshBuilder.BranchSkin> _branchIdToBranchSkin = new Dictionary<int, BranchMeshBuilder.BranchSkin> ();
		public int maxHierarchyLevels = 0;
		public int hierarchyLevelToApplyWindPhase = 0;
		public Dictionary<int, float> branchToPhase = new Dictionary<int, float> ();
		public Dictionary<int, float> branchToMinPhaseLength = new Dictionary<int, float> ();
		public Dictionary<int, float> branchToMaxPhaseLength = new Dictionary<int, float> ();
		public Dictionary<int, float> branchToLengthOffset = new Dictionary<int, float> ();
		#endregion

		#region Channel Vars
		/// <summary>
		/// The UVs (channel 0) on each meshId.
		/// </summary>
		Dictionary<int, List<Vector4>> meshIdToUV = new Dictionary<int, List<Vector4>> ();
		/// <summary>
		/// The UV2s (channel 1) on each meshId.
		/// </summary>
		Dictionary<int, List<Vector4>> meshIdToUV2 = new Dictionary<int, List<Vector4>> ();
		/// <summary>
		/// The UV3s (channel 2) on each meshId.
		/// </summary>
		Dictionary<int, List<Vector4>> meshIdToUV3 = new Dictionary<int, List<Vector4>> ();
		/// <summary>
		/// The UV4s (channel 3) on each meshId.
		/// </summary>
		Dictionary<int, List<Vector4>> meshIdToUV4 = new Dictionary<int, List<Vector4>> ();
		/// <summary>
		/// The Color channel on each meshId.
		/// </summary>
		Dictionary<int, List<Color>> meshIdToColor = new Dictionary<int, List<Color>> ();
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Builder.STWindMetaBuilder"/> class.
		/// </summary>
		public STWindMetaBuilder () {
			weightSensibilityCurve = new AnimationCurve ();
			weightSensibilityCurve.AddKey (new Keyframe ());
			weightSensibilityCurve.AddKey (new Keyframe (1, 1, 2, 2));
		}
		#endregion

		#region Analyze
		/// <summary>
		/// Analyze the tree and its branches to apply wind on this data.
		/// </summary>
		/// <param name="tree">Broccoli tree instance to analyze.</param>
		/// <param name="branchSkins">List of branch skin instances from this tree.</param>
		public void AnalyzeTree (BroccoTree tree, List<BranchMeshBuilder.BranchSkin> branchSkins) {
			// Prepare for analysis.
			Clear ();

			// Build index for branchSkins.
			BuildBranchIdToBranchSkin (branchSkins);

			// Select the level at which to start applying branch wind phases.
			maxHierarchyLevels = tree.GetOffspringLevel ();
			hierarchyLevelToApplyWindPhase = maxHierarchyLevels - 1;

			// Analyze branches.
			for (int i = 0; i < tree.branches.Count; i++) {
				AnalyzeBranch (tree.branches[i]);
			}
		}
		void AnalyzeBranch (BroccoTree.Branch branch, int hierarchyLevel = 0, float lengthOffset = 0, float phase = -1f, float minPhaseLength = -1f, float maxPhaseLength = -1f) {
			// Index branch.
			if (!branches.ContainsKey (branch.id)) {
				branches.Add (branch.id, branch);
			}
			// Check if the branch should have a wind phase.
			if (hierarchyLevel >= hierarchyLevelToApplyWindPhase) {
				if (phase < 0f) {
					phase = Random.Range (0f, 2f);
					minPhaseLength = lengthOffset;
					maxPhaseLength = lengthOffset + GetMaxPhaseLength (branch);
				}
				branchToPhase.Add (branch.id, phase);
				branchToMinPhaseLength.Add (branch.id, minPhaseLength);
				branchToMaxPhaseLength.Add (branch.id, maxPhaseLength);
				branchToLengthOffset.Add (branch.id, lengthOffset);
			} else {
				branchToPhase.Add (branch.id, -1f);
				branchToMinPhaseLength.Add (branch.id, 0f);
				branchToMaxPhaseLength.Add (branch.id, 0f);
				branchToLengthOffset.Add (branch.id, lengthOffset);
			}
			// Run analysis on children branches.
			for (int i = 0; i < branch.branches.Count; i++) {
				AnalyzeBranch (branch.branches [i], hierarchyLevel + 1, lengthOffset + (branch.length * branch.branches [i].position), phase);
			}
		}
		float GetMaxPhaseLength (BroccoTree.Branch branch, float lengthOffset = 0f) {
			float maxPhaseLength = branch.length;
			for (int i = 0; i < branch.branches.Count; i++) {
				float childMaxPhaseLength = GetMaxPhaseLength (branch.branches[i], lengthOffset + (branch.length * branch.branches[i].position));
				if (childMaxPhaseLength > maxPhaseLength) maxPhaseLength = childMaxPhaseLength;
			}
			return maxPhaseLength;
		}
		/// <summary>
		/// Builds a relationship structure between branch ids and the branch skin instance they belong to.
		/// </summary>
		/// <param name="branchSkins">List of BranchSkin instances.</param>
		void BuildBranchIdToBranchSkin (List<BranchMeshBuilder.BranchSkin> branchSkins) {
			_branchIdToBranchSkin.Clear ();
			for (int i = 0; i < branchSkins.Count; i++) {
				for (int j = 0; j < branchSkins [i].ids.Count; j++) {
					if (!_branchIdToBranchSkin.ContainsKey (branchSkins [i].ids [j])) {
						_branchIdToBranchSkin.Add (branchSkins [i].ids [j], branchSkins [i]);
					}
				}
			}
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			branches.Clear ();
			branchToPhase.Clear ();
			branchToLengthOffset.Clear ();
			branchToMinPhaseLength.Clear ();
			branchToMaxPhaseLength.Clear ();
			_branchIdToBranchSkin.Clear ();
		}
		#endregion

		#region Branch Mesh
		public void SetBranchesWindDataJobs (Mesh mesh) {
			// Mark mesh as dynamic.
			mesh.MarkDynamic ();

			// Create job and set variables.
			BranchWindJob branchWindJob = new BranchWindJob ();
			branchWindJob.windAmplitude = windAmplitude;
			branchWindJob.isST7 = isST7;
			branchWindJob.branchSway = branchSway;

			// Set job vertices.
			branchWindJob.vertices = new NativeArray<Vector3> (mesh.vertices, Allocator.TempJob);
			// Set UVs
			List<Vector4> uvs = new List<Vector4> ();
			mesh.GetUVs (0, uvs);
			branchWindJob.uvs = new NativeArray<Vector4> (uvs.ToArray (), Allocator.TempJob);
			// Set UV5s, UV6s, UV7s.
			List<Vector4> uv5s = new List<Vector4> ();
			List<Vector4> uv6s = new List<Vector4> ();
			List<Vector4> uv7s = new List<Vector4> ();
			mesh.GetUVs (4, uv5s);
			mesh.GetUVs (5, uv6s);
			mesh.GetUVs (6, uv7s);
			branchWindJob.uv5s = new NativeArray<Vector4> (uv5s.ToArray (), Allocator.TempJob);
			branchWindJob.uv6s = new NativeArray<Vector4> (uv6s.ToArray (), Allocator.TempJob);
			branchWindJob.uv7s = new NativeArray<Vector4> (uv7s.ToArray (), Allocator.TempJob);
			// Set the branch phases.
			Vector3[] branchPhases = new Vector3 [uv6s.Count];
			int branchId;
			for (int i = 0; i < uv6s.Count; i++) {
				branchId = (int)uv6s[i].x;
				if (branchToPhase.ContainsKey (branchId)) {
					branchPhases [i] = new Vector3 (branchToPhase [branchId], branchToMinPhaseLength [branchId], branchToMaxPhaseLength [branchId]);
				} else {
					branchPhases [i] = Vector3.zero;
				}
			}
			branchWindJob.branchPhases = new NativeArray<Vector3> (branchPhases, Allocator.TempJob);
			// Set wind writeable channels.
			int totalVertices = uvs.Count;
			branchWindJob.uv2s = new NativeArray<Vector4> (totalVertices, Allocator.TempJob);

			// Execute job.
			JobHandle branchWindJobHandle = branchWindJob.Schedule (totalVertices, 64);

			// Complete job.
			branchWindJobHandle.Complete ();

			// Set UVs
			Vector4[] _uvs = new Vector4 [totalVertices];
			Vector4[] _uv2s = new Vector4 [totalVertices];

			branchWindJob.uvs.CopyTo (_uvs);
			branchWindJob.uv2s.CopyTo (_uv2s);

			mesh.SetUVs (0, new List<Vector4>(_uvs));
			mesh.SetUVs (1, new List<Vector4>(_uv2s));

			// Dispose.
			branchWindJob.vertices.Dispose ();
			branchWindJob.uvs.Dispose ();
			branchWindJob.uv2s.Dispose ();
			branchWindJob.uv5s.Dispose ();
			branchWindJob.uv6s.Dispose ();
			branchWindJob.uv7s.Dispose ();
			branchWindJob.branchPhases.Dispose ();
		}
		#endregion

		#region Sprout Mesh
		/// <summary>
		/// Bakes wind data on the sprout mesh UV channels.
		/// It takes values from the base mesh UV channels as parameters.
		/// </summary>
		/// <param name="sproutMeshId">Id of the sprout mesh.</param>
		/// <param name="sproutMesh">Sprout mesh.</param>
		public void SetSproutsWindData (
			int sproutMeshId,
			Mesh sproutMesh)
		{
			/*
			Each sprout mesh should have the following baked values:	
			UVs for the output mesh.
				x: U mapping value.
				y: V mapping value.
				z: sprout anchor gradient.
				w: sprout random value.
			UV2s for the output mesh.
				xyz: sprout anchor point.
				w: sprout relative position on the branch
			UV3s for the output mesh.
				xyz: vertex value.
				w: branch id.
			*/

			// List to get/save UVs.
			if (!meshIdToUV.ContainsKey (sproutMeshId)) { // TODO: Only if ST8
				meshIdToUV.Add (sproutMeshId, new List<Vector4>());
			}
			// List to get/save UV2s.
			if (!meshIdToUV2.ContainsKey (sproutMeshId)) {
				meshIdToUV2.Add (sproutMeshId, new List<Vector4>());
			}
			// List to get/save UV3s.
			if (!meshIdToUV3.ContainsKey (sproutMeshId)) {
				meshIdToUV3.Add (sproutMeshId, new List<Vector4>());
			}
			// List to get/save UV4s.
			if (!meshIdToUV4.ContainsKey (sproutMeshId)) {
				meshIdToUV4.Add (sproutMeshId, new List<Vector4>());
			}

			// Get base UVs.
			sproutMesh.GetUVs (0, meshIdToUV [sproutMeshId]);
			// Get base UV2s.
			sproutMesh.GetUVs (1, meshIdToUV2 [sproutMeshId]);
			// Get base UV3s, if not set, create them.
			sproutMesh.GetUVs (2, meshIdToUV3 [sproutMeshId]);
			if (meshIdToUV3 [sproutMeshId].Count == 0) {
				meshIdToUV3 [sproutMeshId] = new List<Vector4> (new Vector4[sproutMesh.vertices.Length]);
			}
			// Get base UV4s, if not set, create them.
			sproutMesh.GetUVs (3, meshIdToUV4 [sproutMeshId]);
			if (meshIdToUV4 [sproutMeshId].Count == 0) {
				meshIdToUV4 [sproutMeshId] = new List<Vector4> (new Vector4[sproutMesh.vertices.Length]);
			}
			// Get base vertices.
			List<Vector3> _vertices = new List<Vector3> ();
			sproutMesh.GetVertices (_vertices);

			// Get list as vars to be allowed to be passed as references.
			List<Vector4> _uvs = meshIdToUV [sproutMeshId];
			List<Vector4> _uv2s = meshIdToUV2 [sproutMeshId];
			List<Vector4> _uv3s = meshIdToUV3 [sproutMeshId];
			List<Vector4> _uv4s = meshIdToUV4 [sproutMeshId];

			// Bake wind data on the sprout mesh UV channels.
			SetSproutWindData (ref _vertices, ref _uvs, ref _uv2s, ref _uv3s, ref _uv4s);

			// Set the baked UVs back to the sprout mesh.
			sproutMesh.SetUVs (0, meshIdToUV[sproutMeshId]);
			sproutMesh.SetUVs (1, meshIdToUV2[sproutMeshId]);
			sproutMesh.SetUVs (2, meshIdToUV3[sproutMeshId]);
			sproutMesh.SetUVs (3, meshIdToUV4[sproutMeshId]);
		}
		void SetSproutWindData (
			ref List<Vector3> localVertices,
			ref List<Vector4> localUVs, 
			ref List<Vector4> localUV2s, 
			ref List<Vector4> localUV3s, 
			ref List<Vector4> localUV4s) 
		{
			/*
			Each sprout mesh should have the following baked values:	
			UVs for the output mesh.
				x: U mapping value.
				y: V mapping value.
				z: sprout anchor gradient.
				w: sprout random value (0-1).
			UV2s for the output mesh.
				xyz: sprout anchor point.
				w: sprout relative position on the branch
			UV3s for the output mesh.
				xyz: vertex value.
				w: branch id.
			*/

			Vector3 originalVertex = Vector3.zero;
			Vector4 originalUV = Vector4.zero;
			Vector4 originalUV3 = Vector4.zero;
			Vector2 windUV = Vector2.zero;
			Vector4 windUV2 = Vector4.zero;
			Vector4 windUV3 = Vector4.zero;
			Vector4 windUV4 = Vector4.zero;
			float branchId = -1;
			BroccoTree.Branch branch = null;
			float sproutRandomValue;
			float sproutRandomValue2;
			float sproutRandomValue3;
			Vector3 anchor;

			// Run per vertex.
			for (int i = 0; i < localVertices.Count; i++) {
				if (localUV3s [i].w != branchId) {
					branchId = localUV3s [i].w;
					branch = null;
					if (branches.ContainsKey ((int)branchId)) {
						branch = branches [(int)branchId];
					}
				}
				if (branch != null) {
					originalVertex = localVertices [i];
					windUV = GetUV (branch, localUV2s [i].w);
					originalUV = localUVs [i];
					anchor = localUV2s [i];
					sproutRandomValue = localUVs [i].w * 16f;
					sproutRandomValue2 = 0.5f + localUVs [i].w * 1.5f;
					sproutRandomValue3 = localUVs [i].w * 0.3f;	
					localUVs [i] = new Vector4 (originalUV.x, originalUV.y, windUV.x, windUV.y);
					if (isST7) {
						localUV2s [i] = GetUV2ST7 (anchor, originalUV.w * anchor.x);
						localUV3s [i] = GetUV3ST7 (anchor, sproutRandomValue, originalUV.w);
						localUV4s [i] = new Vector4 (originalVertex.y - anchor.y, originalUV.w * anchor.z, 0f, 1f);
					} else {
						localUV2s [i] = GetUV2ST8 (anchor, originalUV.z, originalUV.w);
						localUV3s [i] = GetUV3ST8 (localVertices [i], anchor.z);
						localUV4s [i] = GetUV4ST8 (localUVs [i].z + sproutRandomValue3, sproutRandomValue, (originalVertex.y - anchor.y) + sproutRandomValue2);
					}
				}
			}
		}
		public void SetSproutsWindData (
			BroccoTree tree,
			int sproutMeshId,
			Mesh sproutMesh,
			List<MeshManager.MeshPart> meshParts)
		{
			if (!meshIdToUV.ContainsKey (sproutMeshId)) { // TODO: Only if ST8
				meshIdToUV.Add (sproutMeshId, new List<Vector4>());
			}
			if (!meshIdToUV2.ContainsKey (sproutMeshId)) {
				meshIdToUV2.Add (sproutMeshId, new List<Vector4>());
			}
			if (!meshIdToUV3.ContainsKey (sproutMeshId)) {
				meshIdToUV3.Add (sproutMeshId, new List<Vector4>());
			}
			if (!meshIdToUV4.ContainsKey (sproutMeshId)) {
				meshIdToUV4.Add (sproutMeshId, new List<Vector4>());
			}
			if (!meshIdToColor.ContainsKey (sproutMeshId)) {
				meshIdToColor.Add (sproutMeshId, new List<Color>());
			}
			sproutMesh.GetUVs (0, meshIdToUV [sproutMeshId]);
			sproutMesh.GetUVs (1, meshIdToUV2 [sproutMeshId]);
			sproutMesh.GetUVs (2, meshIdToUV3 [sproutMeshId]);
			if (meshIdToUV3 [sproutMeshId].Count == 0) {
				meshIdToUV3 [sproutMeshId] = new List<Vector4> (new Vector4[sproutMesh.vertices.Length]);
			}
			sproutMesh.GetUVs (3, meshIdToUV4 [sproutMeshId]);
			if (meshIdToUV4 [sproutMeshId].Count == 0) {
				meshIdToUV4 [sproutMeshId] = new List<Vector4> (new Vector4[sproutMesh.vertices.Length]);
			}
			sproutMesh.GetColors (meshIdToColor [sproutMeshId]);

			List<Vector3> _vertices = new List<Vector3> ();
				sproutMesh.GetVertices (_vertices);
				List<Vector4> _uvs = meshIdToUV [sproutMeshId];
				List<Vector4> _uv2s = meshIdToUV2 [sproutMeshId];
				List<Vector4> _uv3s = meshIdToUV3 [sproutMeshId];
				List<Vector4> _uv4s = meshIdToUV4 [sproutMeshId];
				List<Color> _colors = meshIdToColor [sproutMeshId];

			for (int i = 0; i < meshParts.Count; i++) {
				SetSproutWindData (ref _vertices, ref _uvs, ref _uv2s, ref _uv3s, ref _uv4s, ref _colors, meshParts[i]);
			}

			sproutMesh.SetUVs (0, meshIdToUV[sproutMeshId]);
			sproutMesh.SetUVs (1, meshIdToUV2[sproutMeshId]);
			sproutMesh.SetUVs (2, meshIdToUV3[sproutMeshId]);
			sproutMesh.SetUVs (3, meshIdToUV4[sproutMeshId]);
		}
		void SetSproutWindData (
			ref List<Vector3> localVertices,
			ref List<Vector4> localUVs, 
			ref List<Vector4> localUV2s, 
			ref List<Vector4> localUV3s, 
			ref List<Vector4> localUV4s, 
			ref List<Color> localColors,
			MeshManager.MeshPart meshPart) 
		{
			int index = 0;
			Vector3 originalVertex = Vector3.zero;
			Vector4 originalUV = Vector4.zero;
			Vector4 originalUV2 = Vector4.zero;
			Vector2 windUV = Vector2.zero;
			Vector4 windUV2 = Vector4.zero;
			Vector4 windUV3 = Vector4.zero;
			Vector4 windUV4 = Vector4.zero;

			// Called once per sprout, to get the UV values at a branch length.
			if (!branches.ContainsKey (meshPart.branchId)) {
				return; // TODO: fix non-exitent branchId.
			}
			windUV = GetUV (branches [meshPart.branchId], meshPart.position);
			float sproutRandomValue = Random.Range (0f, 16f);
			float sproutRandomValue2 = Random.Range (0.5f, 2f);
			float sproutRandomValue3 = Random.Range (0f, 0.3f);
			for (int j = 0; j < meshPart.length; j++) {
				index = j + meshPart.startIndex;
				originalVertex = localVertices [index];
				originalUV = localUVs [index];
				originalUV2 = localUV2s [index];
				localUVs [index] = new Vector4 (originalUV.x, originalUV.y, windUV.x, windUV.y);
				if (isST7) {
					localUV2s [index] = GetUV2ST7 (meshPart.origin, originalUV.w * meshPart.origin.x);
					localUV3s [index] = GetUV3ST7 (meshPart.origin, sproutRandomValue, originalUV.w);
					localUV4s [index] = new Vector4 (originalVertex.y - meshPart.origin.y, originalUV.w * meshPart.origin.z, 0f, 1f);
				} else {
					localUV2s [index] = GetUV2ST8 (meshPart.origin, originalUV.z, originalUV.w);
					localUV3s [index] = GetUV3ST8 (originalVertex, meshPart.origin.z);
					localUV4s [index] = GetUV4ST8 (originalUV2.z + sproutRandomValue3, sproutRandomValue, (originalVertex.y - meshPart.origin.y) + sproutRandomValue2);
				}
				localColors [index] = Color.white;
			}
		}
		#endregion

		#region Channels
        /// <summary>
		/// Gets the UV wind weight at a given branch position on the tree.
		/// </summary>
		/// <returns>The UV value.</returns>
		/// <param name="branch">Branch.</param>
		/// <param name="position">Position on the branch.</param>
		public Vector2 GetUV (BroccoTree.Branch branch, float position) {
			if (branchToPhase.ContainsKey (branch.id)) {
				float phaseLength = 0f;
				float branchPhase = branchToPhase [branch.id];
				if (branchPhase > 0) {
					phaseLength = branch.length * position;
					phaseLength += branchToLengthOffset [branch.id];
					phaseLength = (phaseLength - branchToMinPhaseLength[branch.id]) / (branchToMaxPhaseLength[branch.id] - branchToMinPhaseLength[branch.id]);
					phaseLength *= branchSway;
				} else {
					branchPhase = 0.5f;
				}
				return new Vector2 (phaseLength, branchPhase);
			}
			return Vector2.zero;
		}
		public Vector4 GetUV2ST7 (Vector3 vertexPosition, float zPosition) {
			return new Vector4 (vertexPosition.x, vertexPosition.y, vertexPosition.z, zPosition);
		}
		public Vector4 GetUV2ST8 (Vector3 point, float u, float v) {
			return new Vector4 (u, v, point.x, point.y);
		}
		public Vector4 GetUV3ST7 (Vector3 point, float sproutValue, float v) {
			return new Vector4 (v * 0.5f * sproutTurbulence, sproutValue * sproutSway, Random.Range(2f, 15f) * 0f, 0f);
		}
		public Vector4 GetUV3ST8 (Vector3 vertexPosition, float zPosition) {
			return new Vector4 (vertexPosition.x, vertexPosition.y, vertexPosition.z, zPosition);
		}
		public Vector4 GetUV4ST8 (float xPosition, float yValue, float zValue) {
			return new Vector4 (xPosition * sproutSway * 2f, yValue, zValue, 2);
		}
		#endregion
	}
}
