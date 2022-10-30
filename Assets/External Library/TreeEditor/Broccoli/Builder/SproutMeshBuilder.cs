using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Base;
using Broccoli.Model;
using Broccoli.Utils;
using Broccoli.Pipe;

namespace Broccoli.Builder
{
	/// <summary>
	/// Mesh building for sprouts.
	/// </summary>
	public class SproutMeshBuilder {
		#region SproutMeshData class
		/// <summary>
		/// Data container for data from the process of mesh generation.
		/// Every instance of this class represents one sprout on the mesh.
		/// </summary>
		public class SproutMeshData {
			/// <summary>
			/// Start index on the mesh vertices for this sprouts.
			/// </summary>
			public int startIndex = 0;
			/// <summary>
			/// Number of vertices for this sprout.
			/// </summary>
			public int length = 0;
			/// <summary>
			/// Position in the branch.
			/// </summary>
			public float position;
			/// <summary>
			/// The origin of the sprout on the local mesh coordinate system..
			/// </summary>
			public Vector3 origin = Vector3.zero;
			/// <summary>
			/// The index of the branch.
			/// </summary>
			public int branchId = -1;
			/// <summary>
			/// The sprout identifier.
			/// </summary>
			public int sproutId = -1;
			/// <summary>
			/// True for double sided mesh.
			/// </summary>
			public bool isTwoSided = true;
			/// <summary>
			/// Initializes a new instance of the <see cref="Broccoli.Builder.SproutMeshBuilder+SproutMeshData"/> class
			/// with information about a single sprout.
			/// </summary>
			/// <param name="startIndex">Start index on the mesh.</param>
			/// <param name="length">Number of vertices taking the sprout.</param>
			/// <param name="position">Position on the branch.</param>
			/// <param name="origin">Point of origin on the mesh space.</param>
			/// <param name="branchId">Branch identifier.</param>
			/// <param name="sproutId">Sprout identifier.</param>
			/// <param name="isTwoSided">The mesh is double sided.</param>
			public SproutMeshData (int startIndex, int length, float position, Vector3 origin, int branchId, int sproutId, bool isTwoSided) {
				this.startIndex = startIndex;
				this.length = length;
				this.position = position;
				this.origin = origin;
				this.branchId = branchId;
				this.sproutId = sproutId;
				this.isTwoSided = isTwoSided;
			}
		}
		#endregion

		#region PlaneDef class
		/// <summary>
		/// Description for sprouts with a plane topology.
		/// </summary>
		class PlaneDef {
			#region Vars
			/// <summary>
			/// The minimum width for the plane.
			/// </summary>
			float _minWidth = 1f;
			/// <summary>
			/// The maximum width for the plane.
			/// </summary>
			float _maxWidth = 1f;
			/// <summary>
			/// The minimum height for the plane.
			/// </summary>
			float _minHeight = 1f;
			/// <summary>
			/// The maximum height for the plane.
			/// </summary>
			float _maxHeight = 1f;
			/// <summary>
			/// The pivot position on the width side,
			/// relative from 0 to 1.
			/// </summary>
			float _pivotPosW = 0.5f;
			/// <summary>
			/// The pivot position on the height side,
			/// relative from 0 to 1.
			/// </summary>
			float _pivotPosH = 0f;
			/// <summary>
			/// Number of planes (single, crossed, tri-crossed).
			/// </summary>
			int _planes = 1;
			/// <summary>
			/// Randomize width.
			/// </summary>
			bool _randWidth = false;
			/// <summary>
			/// Randomize height.
			/// </summary>
			bool _randHeight = false;
			/// <summary>
			/// UV mapping x offset.
			/// </summary>
			public float uvX = 0f;
			/// <summary>
			/// UV mapping y offset.
			/// </summary>
			public float uvY = 0f;
			/// <summary>
			/// UV mapping width offset.
			/// </summary>
			public float uvWidth = 1f;
			/// <summary>
			/// UV mapping height offset.
			/// </summary>
			public float uvHeight = 1f;
			/// <summary>
			/// UV mapping rotation.
			/// </summary>
			public int uvSteps = 0;
			#endregion

			#region Getters and Setters
			/// <summary>
			/// Gets or sets the minimum width for the plane.
			/// </summary>
			/// <value>The minimum width for the plane.</value>
			public float minWidth {
				get { return _minWidth; }
				set {
					_minWidth = value;
					if (_minWidth > _maxWidth)
						_maxWidth = _minWidth;
					if (_minWidth == _maxWidth)
						_randWidth = false;
					else
						_randWidth = true;
				}
			}
			/// <summary>
			/// Gets or sets the maximum width for the plane.
			/// </summary>
			/// <value>The maximum width for the plane.</value>
			public float maxWidth {
				get { return _maxWidth; }
				set {
					_maxWidth = value;
					if (_maxWidth < _minWidth)
						_minWidth = _maxWidth;
					if (_minWidth == _maxWidth)
						_randWidth = false;
					else
						_randWidth = true;
				}
			}
			/// <summary>
			/// Gets or sets the width for the plane.
			/// </summary>
			/// <value>The width for the plane.</value>
			public float width {
				get { 
					if (_randWidth)
						return Random.Range (_minWidth, _maxWidth);
					else
						return _minWidth;
				}
				set {
					_minWidth = value;
					_maxWidth = value;
					_randWidth = false;
				}
			}
			/// <summary>
			/// Gets or sets the minimum height for the plane.
			/// </summary>
			/// <value>The minimum height for the plane.</value>
			public float minHeight {
				get { return _minHeight; }
				set {
					_minHeight = value;
					if (_minHeight > _maxHeight)
						_maxHeight = _minHeight;
					if (_minHeight == _maxHeight)
						_randHeight = false;
					else
						_randHeight = true;
				}
			}
			/// <summary>
			/// Gets or sets the maximum height for the plane.
			/// </summary>
			/// <value>The maximum height for the plane.</value>
			public float maxHeight {
				get { return _maxHeight; }
				set {
					_maxHeight = value;
					if (_maxHeight < _minHeight)
						_minHeight = _maxHeight;
					if (_minHeight == _maxHeight)
						_randHeight = false;
					else
						_randHeight = true;
				}
			}
			/// <summary>
			/// Gets or sets the height for the plane.
			/// </summary>
			/// <value>The height for the plane.</value>
			public float height {
				get { 
					if (_randHeight)
						return Random.Range (_minHeight, _maxHeight);
					else
						return _minHeight;
				}
				set {
					_minHeight = value;
					_maxHeight = value;
					_randHeight = false;
				}
			}
			/// <summary>
			/// Gets or sets the pivot position on the width side.
			/// </summary>
			/// <value>The pivot position on the width side.</value>
			public float pivotPosW {
				get { return _pivotPosW; }
				set { _pivotPosW = value; }
			}
			/// <summary>
			/// Gets or sets the pivot position on the height side.
			/// </summary>
			/// <value>The pivot position on the height side.</value>
			public float pivotPosH {
				get { return _pivotPosH; }
				set { _pivotPosH = value; }
			}
			/// <summary>
			/// Gets or sets the number of planes.
			/// </summary>
			/// <value>The number of planes (1 to 3).</value>
			public int planes {
				get { return _planes; }
				set {
					if (_planes < 1)
						_planes = 1;
					else if (_planes > 3)
						_planes = 3;
					else
						_planes = value;
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
		/// Mapping flag for SpeedTree shaders.
		/// </summary>
		public bool mapST = false;
		/// <summary>
		/// The plane definition used when building sprout meshes.
		/// </summary>
		PlaneDef planeDef;
		/// <summary>
		/// The sprout mesh used when building sprout meshes.
		/// </summary>
		SproutMesh sproutMesh;
		/// <summary>
		/// Count for the number of sprouts assigned to a sprout group.
		/// </summary>
		int sproutsInGroup = 0;
		/// <summary>
		/// The vertices on the mesh.
		/// </summary>
		List<Vector3> vertices = new List<Vector3> ();
		/// <summary>
		/// The normals on the mesh.
		/// </summary>
		List<Vector3> normals = new List<Vector3> ();
		/// <summary>
		/// The tangents on the mesh.
		/// </summary>
		List<Vector4> tangents = new List<Vector4> ();
		/// <summary>
		/// The triangles on the mesh.
		/// </summary>
		List<int> triangles = new List<int> ();
		/// <summary>
		/// The UVs on the mesh.
		/// </summary>
		List<Vector4> uvs = new List<Vector4> ();
		/// <summary>
		/// The UV2s on the mesh.
		/// </summary>
		List<Vector4> uv2s = new List<Vector4> ();
		/// <summary>
		/// The UV6s on the mesh.
		/// </summary>
		List<Vector4> uv6s = new List<Vector4> ();
		/// <summary>
		/// Sprout mesh data.
		/// </summary>
		List<SproutMeshData> _sproutMeshData = new List<SproutMeshData> ();
		/// <summary>
		/// Gets the sprout mesh data.
		/// </summary>
		/// <value>The sprout mesh data.</value>
		public List<SproutMeshData> sproutMeshData { get { return _sproutMeshData; } }
		/// <summary>
		/// The enabled areas temp var.
		/// </summary>
		List<int> enabledSubgroups = new List<int> ();
		/// <summary>
		/// gravity forward vector used on rotation operations.
		/// </summary>
		Vector3 gravityForward = Vector3.down;
		/// <summary>
		/// Gravity up vector used on rotation operations.
		/// </summary>
		Vector3 gravityUp = Vector3.forward;
		/// <summary>
		/// Hold the vertices created for a sprout on every iteration.
		/// </summary>
		Vector3[] verticesBuffer;
		/// <summary>
		/// Hold the normals created for a sprout on every iteration.
		/// </summary>
		Vector3[] normalsBuffer;
		/// <summary>
		/// Hold the tangents created for a sprout on every iteration.
		/// </summary>
		Vector4[] tangentsBuffer;
		/// <summary>
		/// Hold the UVs created for a sprout on every iteration.
		/// </summary>
		Vector4[] uvsBuffer;
		/// <summary>
		/// Hold the UV2s created for a sprout on every iteration.
		/// </summary>
		Vector4[] uv2sBuffer;
		/// <summary>
		/// Saves scaling relative to a map area on an atlas texture.
		/// </summary>
		/// <typeparam name="int">Sprout group multiplied by 1000 plus the index of the map area.</typeparam>
		/// <typeparam name="float">Scaling.</typeparam>
		Dictionary<int, float> _mapAreaToScale = new Dictionary<int, float> ();
		#endregion

		#region Singleton
		/// <summary>
		/// Singleton for this class.
		/// </summary>
		static SproutMeshBuilder _sproutMeshBuilder = null;
		/// <summary>
		/// Gets the singleton instance for this class.
		/// </summary>
		/// <returns>The instance.</returns>
		public static SproutMeshBuilder GetInstance() {
			if (_sproutMeshBuilder == null) {
				_sproutMeshBuilder = new SproutMeshBuilder ();
			}
			return _sproutMeshBuilder;
		}
		#endregion

		#region Data Ops
		/// <summary>
		/// Prepares the builder for sprout mesh creation process.
		/// </summary>
		/// <param name="tree">Tree.</param>
		/// <param name="sproutGroupId">Sprout group id.</param>
		/// <param name="sproutMesh">Sprout mesh.</param>
		/// <param name="sproutArea">Sprout area.</param>
		/// <param name="sproutAreaIndex">Sprout area index.</param>
		void PrepareData (BroccoTree tree, int sproutGroupId, SproutMesh sproutMesh, SproutMap.SproutMapArea sproutArea = null, int sproutAreaIndex = -1) {
			Clear ();
			tree.SetHelperSproutIds ();
			//Count sprouts assigned to group.
			sproutsInGroup = 0;
			List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
			for (int i = 0; i < branches.Count; i++) {
				for (int j = 0; j < branches[i].sprouts.Count; j++) {
					if (branches[i].sprouts[j].groupId > 0 && 
						branches[i].sprouts[j].groupId == sproutGroupId) {
						sproutsInGroup++;
					}
				}
			}
			// Init planeDef.
			planeDef = new PlaneDef ();
			if (sproutMesh.shapeMode == SproutMesh.ShapeMode.Plane)
				planeDef.planes = 1;
			else if (sproutMesh.shapeMode == SproutMesh.ShapeMode.Cross)
				planeDef.planes = 2;
			else if (sproutMesh.shapeMode == SproutMesh.ShapeMode.Tricross)
				planeDef.planes = 3;
			planeDef.width = sproutMesh.width;
			planeDef.height = sproutMesh.height;
			if (sproutArea != null && 
				sproutArea.enabled &&
				sproutArea.width > 0 && 
				sproutArea.texture != null) {
				if (sproutMesh.overrideHeightWithTexture)
					planeDef.height = sproutMesh.width * sproutArea.normalizedHeightPx / (float)sproutArea.normalizedWidthPx;
				if (sproutMesh.includeScaleFromAtlas && sproutAreaIndex >= 0) {
					planeDef.width *= _mapAreaToScale [sproutGroupId * 1000 + sproutAreaIndex];
					planeDef.height *= _mapAreaToScale [sproutGroupId * 1000 + sproutAreaIndex];
				}
			}
			if (sproutArea != null) {
				planeDef.pivotPosW = sproutArea.normalizedPivotX;
				planeDef.pivotPosH = sproutArea.normalizedPivotY;
				planeDef.uvX = sproutArea.x;
				planeDef.uvY = sproutArea.y;
				planeDef.uvWidth = sproutArea.width;
				planeDef.uvHeight = sproutArea.height;
				planeDef.uvSteps = sproutArea.normalizedStep;
			} else {
				planeDef.pivotPosW = sproutMesh.pivotX;
				planeDef.pivotPosH = sproutMesh.pivotY;
			}

			// Assign sprout mesh.
			this.sproutMesh = sproutMesh;

			// Get processed mesh (to use on sprout mesh copies).
			if (sproutMesh.shapeMode == SproutMesh.ShapeMode.Mesh) {
				if (sproutMesh.processedMesh != null) {
					Object.DestroyImmediate (sproutMesh.processedMesh);
				}
				if (sproutMesh.meshGameObject != null) {
					MeshFilter[] meshFilters = sproutMesh.meshGameObject.GetComponentsInChildren<MeshFilter> ();
					if (meshFilters.Length > 0 && meshFilters [0] != null) {
						sproutMesh.processedMesh =  Object.Instantiate (meshFilters [0].sharedMesh);
						ScaleRotateMesh (sproutMesh.processedMesh, sproutMesh.meshScale, sproutMesh.meshRotation);
					}
				}
			}

			// Prepare mesh sprout.
			switch (sproutMesh.shapeMode) {
				case SproutMesh.ShapeMode.GridPlane:
					GetGridPlane (ref verticesBuffer, 
						ref normalsBuffer, 
						ref uvsBuffer,
						ref uv2sBuffer);
				break;
				case SproutMesh.ShapeMode.PlaneX:
					GetPlaneX (ref verticesBuffer,
						ref normalsBuffer,
						ref uvsBuffer,
						ref uv2sBuffer);
				break;
				/*
				case SproutMesh.Mode.Billboard:
					GetBillboard (ref verticesBuffer,
						ref normalsBuffer,
						ref uvsBuffer,
						ref uv2sBuffer);
				break;
				*/
				case SproutMesh.ShapeMode.Mesh:
					GetMesh (ref verticesBuffer,
						ref normalsBuffer,
						ref uvsBuffer,
						ref uv2sBuffer);
				break;
				default:
					GetPlane (ref verticesBuffer,
						ref normalsBuffer,
						ref tangentsBuffer,
						ref uvsBuffer,
						ref uv2sBuffer);
				break;
			}
		}
		/// <summary>
		/// Set the gravity vector to use on the vertices processing.
		/// </summary>
		/// <param name="gravity">Gravity vector</param>
		public void SetGravity (Vector3 gravity) {
			Quaternion gravityQuaternion = Quaternion.FromToRotation (Vector3.down, gravity);
			gravityForward = gravityQuaternion * Vector3.forward;
			gravityUp = gravityQuaternion * Vector3.up;
		}
		/// <summary>
		/// Cleans data after finishing the sprout mesh process.
		/// </summary>
		/// <param name="sproutMesh">Sprout mesh.</param>
		void ClearData (SproutMesh sproutMesh) {
			if (sproutMesh.processedMesh != null) {
				Object.DestroyImmediate (sproutMesh.processedMesh);
			}
			this.sproutMesh = null;
			this.planeDef = null;
		}
		/// <summary>
		/// Clear local variables.
		/// </summary>
		public void Clear () {
			vertices.Clear ();
			normals.Clear ();
			tangents.Clear ();
			triangles.Clear ();
			uvs.Clear ();
			uv2s.Clear ();
			uv6s.Clear ();
			_sproutMeshData.Clear ();
		}
		#endregion

		#region Processing
		public void PrepareBuilder (Dictionary<int, SproutMesh> sproutMeshes, Dictionary<int, SproutMap> sproutMappers) {
			// Clean sprout mesh to atlas dictionary
			// Iterate through all areas.
			// Save the scalings.
			var sproutMappersEnumerator = sproutMappers.GetEnumerator ();
			int groupId;
			SproutMap sproutMap;
			SproutMap.SproutMapArea sproutArea;
			List<float> areaDiagonals = new List<float> ();
			float maxDiagonal = -1f;
			float diagonal;
			_mapAreaToScale.Clear ();
			while (sproutMappersEnumerator.MoveNext ()) {
				groupId = sproutMappersEnumerator.Current.Key;
				sproutMap = sproutMappersEnumerator.Current.Value;
				areaDiagonals.Clear ();
				maxDiagonal = -1f;
				for (int i = 0; i < sproutMap.sproutAreas.Count; i++) {
					sproutArea = sproutMap.sproutAreas [i];
					if (sproutArea.enabled && sproutArea.texture != null) {
						diagonal = sproutArea.diagonal;
						if (diagonal > maxDiagonal) {
							maxDiagonal = diagonal;
						}
					} else {
						diagonal = 0f;
					}
					areaDiagonals.Add (diagonal);
				}
				for (int i = 0; i < areaDiagonals.Count; i++) {
					_mapAreaToScale.Add (groupId * 1000 + i, areaDiagonals [i] / maxDiagonal);
				}
			}
		}
		/// <summary>
		/// Creates the mesh coming from a sprout group on a tree instance.
		/// </summary>
		/// <returns>The mesh object for the sprouts.</returns>
		/// <param name="tree">Tree object.</param>
		/// <param name="sproutGroupId">Sprout group id.</param>
		/// <param name="sproutMesh">Sprout mesh instance.</param>
		public Mesh MeshSprouts (BroccoTree tree, int sproutGroupId, SproutMesh sproutMesh) {
			PrepareData (tree, sproutGroupId, sproutMesh);
			List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
			for (int i = 0; i < branches.Count; i++) {
				for (int j = 0; j < branches[i].sprouts.Count; j++) {
					if (branches[i].sprouts[j].groupId == sproutGroupId) {
						branches[i].sprouts[j].horizontalAlign = 
							Mathf.Lerp (sproutMesh.horizontalAlignAtBase, 
								sproutMesh.horizontalAlignAtTop, branches[i].sprouts[j].position);
						branches[i].sprouts[j].CalculateVectors ();
						MeshSprout (branches[i].sprouts[j], branches[i], sproutMesh);
					}
				}
			}
			Mesh groupMesh = new Mesh ();
			groupMesh.vertices = vertices.ToArray ();
			groupMesh.triangles = triangles.ToArray ();
			groupMesh.colors = 
				SproutMeshMetaBuilder.GetInstance ().GetColor (mapST?Color.white:Color.gray, vertices.Count);
			groupMesh.normals = normals.ToArray ();
			groupMesh.tangents = tangents.ToArray ();
			groupMesh.SetUVs (0, uvs);
			groupMesh.SetUVs (1, uv2s);
			groupMesh.SetUVs (2, vertices);
			groupMesh.SetUVs (5, uv6s);
			if (GlobalSettings.useAutoCalculateTangents) {
				//if (sproutMesh.mode != SproutMesh.Mode.Billboard || tangents.Count == 0) {
				if (tangents.Count == 0) {
					#if UNITY_5_6_OR_NEWER
					groupMesh.RecalculateTangents ();
					#else
					SproutMeshMetaBuilder.GetInstance ().RecalculateTangents (groupMesh);
					#endif
				} else {
					SproutMeshMetaBuilder.GetInstance ().TangentsToZero (groupMesh);
				}
			}
			
			groupMesh.RecalculateBounds ();
			ClearData (sproutMesh);
			return groupMesh;
		}
		/// <summary>
		/// Creates the mesh for a sprout group on a tree instance taking a sproutArea for mapping.
		/// </summary>
		/// <returns>The mesh object for the sprouts.</returns>
		/// <param name="tree">Tree object.</param>
		/// <param name="sproutGroupId">Sprout group id.</param>
		/// <param name="sproutMesh">Sprout mesh instance.</param>
		/// <param name="sproutArea">Sprout area instance.</param>
		/// <param name="sproutAreaIndex">Sprout area index.</param>
		/// <param name="isTwoSided">True if the sprout mesh has a front and a back plane.</param>
		public Mesh MeshSprouts (BroccoTree tree, int sproutGroupId, SproutMesh sproutMesh, SproutMap.SproutMapArea sproutArea, int sproutAreaIndex, bool isTwoSided) {
			sproutMesh.isTwoSided = isTwoSided;
			PrepareData (tree, sproutGroupId, sproutMesh, sproutArea, sproutAreaIndex);
			List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
			for (int i = 0; i < branches.Count; i++) {
				for (int j = 0; j < branches[i].sprouts.Count; j++) {
					if (branches[i].sprouts[j].groupId == sproutGroupId && 
						branches[i].sprouts[j].subgroupId == sproutAreaIndex) {
						branches[i].sprouts[j].horizontalAlign = 
							Mathf.Lerp (sproutMesh.horizontalAlignAtBase, 
								sproutMesh.horizontalAlignAtTop, 
								branches[i].sprouts[j].position);
						branches[i].sprouts[j].CalculateVectors ();
						MeshSprout (branches[i].sprouts[j], branches[i], sproutMesh);
					}
				}
			}
			Mesh groupMesh = new Mesh ();
			groupMesh.vertices = vertices.ToArray ();
			groupMesh.triangles = triangles.ToArray ();
			groupMesh.colors = 
				SproutMeshMetaBuilder.GetInstance ().GetColor (mapST?Color.white:Color.gray, vertices.Count);
			groupMesh.normals = normals.ToArray ();
			groupMesh.tangents = tangents.ToArray ();
			groupMesh.SetUVs (0, uvs);
			groupMesh.SetUVs (1, uv2s);
			groupMesh.SetUVs (2, vertices);
			groupMesh.SetUVs (5, uv6s);			
			if (GlobalSettings.useAutoCalculateTangents) {
				//if (sproutMesh.mode != SproutMesh.Mode.Billboard || tangents.Count == 0) {
				if (tangents.Count == 0) {
					#if UNITY_5_6_OR_NEWER
					groupMesh.RecalculateTangents ();
					#else
					SproutMeshMetaBuilder.GetInstance ().RecalculateTangents (groupMesh);
					#endif
				} else {
					SproutMeshMetaBuilder.GetInstance ().TangentsToZero (groupMesh);
				}
			}
			
			groupMesh.RecalculateBounds ();
			ClearData (sproutMesh);
			return groupMesh;
		}
		/// <summary>
		/// Matches sprout groups with sprout maps.
		/// </summary>
		/// <param name="tree">Tree object.</param>
		/// <param name="sproutGroupId">Sprout group id.</param>
		/// <param name="sproutMap">Sprout map instance.</param>
		public void AssignSproutSubgroups (BroccoTree tree, int sproutGroupId, SproutMap sproutMap) {
			sproutMap.NormalizeAreas ();
			enabledSubgroups.Clear ();
			for (int i = 0; i < sproutMap.sproutAreas.Count; i++) {
				if (sproutMap.sproutAreas[i].enabled && 
					sproutMap.sproutAreas[i].texture != null) {
					enabledSubgroups.Add (i);
				}
			}
			int maxAreaIndex = enabledSubgroups.Count;
			List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
			for (int i = 0; i < branches.Count; i++) {
				for (int j = 0; j < branches[i].sprouts.Count; j++) {
					if (branches[i].sprouts[j].groupId == sproutGroupId) {
						if (maxAreaIndex > 0) {
							branches[i].sprouts[j].subgroupId = 
								enabledSubgroups [Random.Range (0, maxAreaIndex)];
						} else {
							branches[i].sprouts[j].subgroupId = -1;
						}
					}
				}
			}
			enabledSubgroups.Clear ();
		}
		/// <summary>
		/// Matches sprout groups with branch collection snapshot.
		/// </summary>
		/// <param name="tree">Tree object.</param>
		/// <param name="sproutGroupId">Sprout group id.</param>
		/// <param name="branchCollection">Sprout map instance.</param>
		public void AssignSproutSubgroups (BroccoTree tree, int sproutGroupId, BranchDescriptorCollection branchCollection, SproutMesh sproutMesh) {
			enabledSubgroups.Clear ();
			for (int i = 0; i < branchCollection.branchDescriptors.Count; i++) {
				enabledSubgroups.Add (i);
			}
			sproutMesh.subgroups = enabledSubgroups.ToArray ();
			
			int maxSnapshotIndex = enabledSubgroups.Count;
			List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
			for (int i = 0; i < branches.Count; i++) {
				for (int j = 0; j < branches[i].sprouts.Count; j++) {
					if (branches[i].sprouts[j].groupId == sproutGroupId) {
						if (maxSnapshotIndex > 0) {
							branches[i].sprouts[j].subgroupId = enabledSubgroups [Random.Range (0, maxSnapshotIndex)];
						} else {
							branches[i].sprouts[j].subgroupId = -1;
						}
					}
				}
			}
			enabledSubgroups.Clear ();
		}
		/// <summary>
		/// Adds information on the mesh creation for a single sprout.
		/// </summary>
		/// <param name="sprout">Sprout instance.</param>
		/// <param name="branch">Parent branch.</param>
		/// <param name="sproutMesh">Sprout mesh instance.</param>
		void MeshSprout (BroccoTree.Sprout sprout, BroccoTree.Branch branch, SproutMesh sproutMesh) {
			// Get scale.
			float scale = Mathf.Lerp (sproutMesh.scaleAtBase, sproutMesh.scaleAtTop, 
				Mathf.Clamp (sproutMesh.scaleCurve.Evaluate(sprout.preferedPosition), 0f, 1f));
			sprout.meshHeight = scale * planeDef.height * (1f - planeDef.pivotPosH);

			switch (sproutMesh.shapeMode) {
			case SproutMesh.ShapeMode.Plane:
			case SproutMesh.ShapeMode.Cross:
			case SproutMesh.ShapeMode.Tricross:
				AddPlaneMesh (sprout.inGirthPosition * globalScale, sprout.position,
					sprout.sproutDirection, sprout.sproutNormal, 
					branch, sprout, sproutMesh.isTwoSided, scale * globalScale);
				break;
			/*
			case SproutMesh.Mode.Billboard:
				float rotationZ = Mathf.Lerp(sproutMesh.billboardRotationAtBase, sproutMesh.billboardRotationAtTop, 
					Mathf.Clamp (sproutMesh.billboardRotationCurve.Evaluate(sprout.preferedPosition), 0f, 1f));
				AddBillboardMesh (sproutMesh.billboardAtOrigin, rotationZ, 
					sprout.inGirthPosition * globalScale, sprout.position, sprout.sproutDirection,
					sprout.sproutNormal, branch, sprout, sproutMesh.isTwoSided, scale * globalScale);
				break;
			*/
			case SproutMesh.ShapeMode.PlaneX:
				AddPlaneXMesh (sproutMesh.depth, sprout.inGirthPosition * globalScale, sprout.position,
					sprout.sproutDirection, sprout.sproutNormal, 
					branch, sprout, sproutMesh.isTwoSided, scale * globalScale);
				break;
			case SproutMesh.ShapeMode.Mesh:
				if (sproutMesh.processedMesh != null) {
					AddCustomMesh (sproutMesh.processedMesh, sprout.inGirthPosition * globalScale,
						sprout.position, sprout.sproutDirection, sprout.sproutNormal, 
						sproutMesh.meshOffset, branch, sprout, sproutMesh.isTwoSided, scale * globalScale);
				}
				break;
			case SproutMesh.ShapeMode.GridPlane:
				AddGridPlaneMesh (sprout.inGirthPosition * globalScale, sprout.position,
					sprout.sproutDirection, sprout.sproutNormal, 
					branch, sprout, sproutMesh.isTwoSided, sproutMesh.resolutionWidth, sproutMesh.resolutionHeight, 
					scale * globalScale);
				break;
			}
		}
		/// <summary>
		/// Adds a single plane, crossed planes or tri-cross planes to the mesh creation data.
		/// </summary>
		/// <param name="position">Origin position on the global mesh.</param>
		/// <param name="forward">Forward vector.</param>
		/// <param name="upward">Upward vector.</param>
		/// <param name="isTwoSided">True for a two sided mesh.</param>
		/// <param name="scale">Scale.</param>
		void AddPlaneMesh (Vector3 position,
			float branchPosition,
			Vector3 forward, 
			Vector3 upward, 
			BroccoTree.Branch branch, 
			BroccoTree.Sprout sprout,
			bool isTwoSided,
			float scale = 1f) 
		{
			Vector3[] planeVertices = (Vector3[])verticesBuffer.Clone ();
			Vector3[] planeNormals = (Vector3[])normalsBuffer.Clone ();
			Vector4[] planeTangents = (Vector4[])tangentsBuffer.Clone ();
			
			RotatePoints (ref planeVertices, forward, upward);
			RotatePoints (ref planeNormals, forward, upward);
			RotatePoints (ref planeTangents, forward, upward);

			_sproutMeshData.Add (new SproutMeshData (vertices.Count, planeVertices.Length, branchPosition,
				position, branch.id, sprout.helperSproutId, isTwoSided));

			for (int i = 0; i < planeVertices.Length; i++) {
				vertices.Add ((planeVertices[i] * scale) + position);
			}
			normals.AddRange (planeNormals);
			tangents.AddRange (planeTangents);
			uvs.AddRange (uvsBuffer);
			uv2s.AddRange (uv2sBuffer);
			for (int i = 0; i < uv2sBuffer.Length; i++) {
				uv6s.Add (new Vector4 (branch.id, 0, 0, 0));
			}

			if (isTwoSided) {
				for (int i = 0; i < planeDef.planes; i++) {
					// Polygon A.
					triangles.Add (vertices.Count - 6 - (i * 8));
					triangles.Add (vertices.Count - 7 - (i * 8));
					triangles.Add (vertices.Count - 8 - (i * 8));
					// Polygon B.
					triangles.Add (vertices.Count - 5 - (i * 8));
					triangles.Add (vertices.Count - 6 - (i * 8));
					triangles.Add (vertices.Count - 8 - (i * 8));
					// Polygon C.
					triangles.Add (vertices.Count - 3 - (i * 8));
					triangles.Add (vertices.Count - 2 - (i * 8));
					triangles.Add (vertices.Count - 4 - (i * 8));
					// Polygon D.
					triangles.Add (vertices.Count - 2 - (i * 8));
					triangles.Add (vertices.Count - 1 - (i * 8));
					triangles.Add (vertices.Count - 4 - (i * 8));
				}
			} else {
				for (int i = 0; i < planeDef.planes; i++) {
					// Polygon A.
					triangles.Add (vertices.Count - 2 - (i * 4));
					triangles.Add (vertices.Count - 3 - (i * 4));
					triangles.Add (vertices.Count - 4 - (i * 4));
					// Polygon B.
					triangles.Add (vertices.Count - 1 - (i * 4));
					triangles.Add (vertices.Count - 2 - (i * 4));
					triangles.Add (vertices.Count - 4 - (i * 4));
				}
			}
		}
		/*
		/// <summary>
		/// Adds a billboard to the mesh creation data.
		/// </summary>
		/// <param name="atOrigin">Billboard is placed at the sprout point.</param>
		/// <param name="position">Origin position on the global mesh.</param>
		/// <param name="forward">Forward vector.</param>
		/// <param name="upward">Upward vector.</param>
		/// <param name="isTwoSided">True for a two sided mesh.</param>
		/// <param name="scale">Scale.</param>
		void AddBillboardMesh (bool atOrigin, 
			float rotationZ, 
			Vector3 position,
			float branchPosition, 
			Vector3 forward, 
			Vector3 upward, 
			BroccoTree.Branch branch, 
			BroccoTree.Sprout sprout,
			bool isTwoSided,
			float scale = 1f) 
		{
			Vector3[] billboardVertices = (Vector3[])verticesBuffer.Clone ();
			Vector3[] billboardNormals = (Vector3[])normalsBuffer.Clone ();

			// Rotate Vertices.
			RotatePoints (ref billboardVertices, forward, upward);

			/// Rotate Normals.
			rotationZ *= Mathf.PI;
			rotationZ += Mathf.PI / 2f;
			RotatePoints (billboardNormals, 
				Vector3.forward, new Vector3 (Mathf.Cos (rotationZ), Mathf.Sin (rotationZ)));

			_sproutMeshData.Add (new SproutMeshData (vertices.Count, billboardVertices.Length, 
				branchPosition, position, branch.id, sprout.helperSproutId, isTwoSided));
			for (int i = 0; i < billboardVertices.Length; i++) {
				vertices.Add ((billboardVertices[i] * scale) + position);
				normals.Add (billboardNormals[i] * scale);
			}
			
			uvs.AddRange (uvsBuffer);
			uv2s.AddRange (uv2sBuffer);

			// Polygon A.
			triangles.Add (vertices.Count - 4);
			triangles.Add (vertices.Count - 2);
			triangles.Add (vertices.Count - 3);
			// Polygon B.
			triangles.Add (vertices.Count - 4);
			triangles.Add (vertices.Count - 1);
			triangles.Add (vertices.Count - 2);
		}
		*/
		/// <summary>
		/// Adds a plane x to the mesh creation data.
		/// </summary>
		/// <param name="depth">Plane depth.</param>
		/// <param name="position">Origin position on the global mesh.</param>
		/// <param name="forward">Forward vector.</param>
		/// <param name="upward">Upward vector.</param>
		/// <param name="isTwoSided">True for a two sided mesh.</param>
		/// <param name="scale">Scale.</param>
		void AddPlaneXMesh (float depth, 
			Vector3 position,
			float branchPosition,
			Vector3 forward, 
			Vector3 upward, 
			BroccoTree.Branch branch, 
			BroccoTree.Sprout sprout,
			bool isTwoSided, 
			float scale = 1f) 
		{
			Vector3[] planeXVertices = (Vector3[])verticesBuffer.Clone ();
			Vector3[] planeXNormals = (Vector3[])normalsBuffer.Clone ();

			RotatePoints (ref planeXVertices, forward, upward);
			RotatePoints (ref planeXNormals, forward, upward);

			_sproutMeshData.Add (new SproutMeshData (vertices.Count, planeXVertices.Length, 
				branchPosition, position, branch.id, sprout.helperSproutId, isTwoSided));

			for (int i = 0; i < planeXVertices.Length; i++) {
				vertices.Add ((planeXVertices [i] * scale) + position);
			}
			normals.AddRange (planeXNormals);
			uvs.AddRange (uvsBuffer);
			uv2s.AddRange (uv2sBuffer);
			for (int i = 0; i < uv2sBuffer.Length; i++) {
				uv6s.Add (new Vector4 (branch.id, 0, 0, 0));
			}

			if (isTwoSided) {
				// Polygon A.
				triangles.Add (vertices.Count - 6); // 4
				triangles.Add (vertices.Count - 9); // 1
				triangles.Add (vertices.Count - 10); // 0
				// Polygon B.
				triangles.Add (vertices.Count - 7); // 3
				triangles.Add (vertices.Count - 6); // 4
				triangles.Add (vertices.Count - 10); // 0
				// Polygon C.
				triangles.Add (vertices.Count - 9); // 1
				triangles.Add (vertices.Count - 6); // 4
				triangles.Add (vertices.Count - 8); // 2
				// Polygon D.
				triangles.Add (vertices.Count - 6); // 4
				triangles.Add (vertices.Count - 7); // 3
				triangles.Add (vertices.Count - 8); // 2

				// Polygon A2.
				triangles.Add (vertices.Count - 4); // 6
				triangles.Add (vertices.Count - 1); // 9
				triangles.Add (vertices.Count - 5); // 5
				// Polygon B2.
				triangles.Add (vertices.Count - 1); // 9
				triangles.Add (vertices.Count - 2); // 8
				triangles.Add (vertices.Count - 5); // 5
				// Polygon C2.
				triangles.Add (vertices.Count - 1); // 9
				triangles.Add (vertices.Count - 4); // 6
				triangles.Add (vertices.Count - 3); // 7
				// Polygon D2.
				triangles.Add (vertices.Count - 2); // 8
				triangles.Add (vertices.Count - 1); // 9
				triangles.Add (vertices.Count - 3); // 7
			} else {
				// Polygon A.
				triangles.Add (vertices.Count - 1); // 4
				triangles.Add (vertices.Count - 4); // 1
				triangles.Add (vertices.Count - 5); // 0
				// Polygon B.
				triangles.Add (vertices.Count - 2); // 3
				triangles.Add (vertices.Count - 1); // 4
				triangles.Add (vertices.Count - 5); // 0
				// Polygon C.
				triangles.Add (vertices.Count - 4); // 1
				triangles.Add (vertices.Count - 1); // 4
				triangles.Add (vertices.Count - 3); // 2
				// Polygon D.
				triangles.Add (vertices.Count - 1); // 4
				triangles.Add (vertices.Count - 2); // 3
				triangles.Add (vertices.Count - 3); // 2
			}
		}
		/// <summary>
		/// Adds data to the mesh creation from a custom mesh.
		/// </summary>
		/// <param name="mesh">Custom mesh.</param>
		/// <param name="position">Origin position on the global mesh.</param>
		/// <param name="forward">Forward vector.</param>
		/// <param name="upward">Upward vector.</param>
		/// <param name="isTwoSided">True for a two sided mesh.</param>
		/// <param name="scale">Scale.</param>
		void AddCustomMesh (Mesh mesh, 
			Vector3 position,
			float branchPosition, 
			Vector3 forward, 
			Vector3 upward, 
			Vector3 offset, 
			BroccoTree.Branch branch, 
			BroccoTree.Sprout sprout,
			bool isTwoSided,
			float scale = 1f) 
		{
			if (mesh == null)
				return;
			Vector3[] meshVertices = (Vector3[])verticesBuffer.Clone ();
			Vector3[] meshNormals = (Vector3[])normalsBuffer.Clone ();

			RotatePoints (ref meshVertices, forward, upward);
			RotatePoints (ref meshNormals, forward, upward);

			_sproutMeshData.Add (new SproutMeshData (vertices.Count, meshVertices.Length, 
				branchPosition, position, branch.id, sprout.helperSproutId, isTwoSided));

			for (int i = 0; i < mesh.triangles.Length; i++) {
				triangles.Add (vertices.Count + mesh.triangles [i]);
			}
			for (int i = 0; i < meshVertices.Length; i++) {
				vertices.Add ((meshVertices [i] * scale) + position);
			}

			normals.AddRange (meshNormals);
			uvs.AddRange (uvsBuffer);
			uv2s.AddRange (uv2sBuffer);
			for (int i = 0; i < uv2sBuffer.Length; i++) {
				uv6s.Add (new Vector4 (branch.id, 0, 0, 0));
			}
		}
		/// <summary>
		/// Scales and rotates a mesh taking its zero coordinates as pivot point.
		/// </summary>
		/// <param name="mesh">Mesh.</param>
		/// <param name="scale">Scale.</param>
		/// <param name="rotation">Rotation.</param>
		void ScaleRotateMesh (Mesh mesh, Vector3 scale, Vector3 rotation) {
			if (mesh != null) {
				Vector3[] originalVertices = mesh.vertices;
				Vector3[] originalNormals = mesh.normals;
				for (int i = 0; i < originalVertices.Length; i++) {
					originalVertices [i].x *= scale.x;
					originalVertices [i].y *= scale.y;
					originalVertices [i].z *= scale.z;
					originalVertices [i] = Quaternion.Euler (rotation) * originalVertices [i];
					originalNormals [i] = Quaternion.Euler (rotation) * originalNormals [i];
				}
				mesh.vertices = originalVertices;
				mesh.normals = originalNormals;
			}
		}
		/// <summary>
		/// Adds a single plane, crossed planes or tri-cross planes to the mesh creation data.
		/// </summary>
		/// <param name="position">Origin position on the global mesh.</param>
		/// <param name="forward">Forward vector.</param>
		/// <param name="upward">Upward vector.</param>
		/// <param name="isTwoSided">True for a two sided mesh.</param>
		/// <param name="scale">Scale.</param>
		void AddGridPlaneMesh (Vector3 position,
			float branchPosition,
			Vector3 forward, 
			Vector3 upward, 
			BroccoTree.Branch branch, 
			BroccoTree.Sprout sprout,
			bool isTwoSided, 
			int resolutionWidth = 1,
			int resolutionHeight = 1,
			float scale = 1f) 
		{
			float gravityStrength = Mathf.Lerp (sproutMesh.gravityBendingAtBase, 
				sproutMesh.gravityBendingAtTop, sprout.position); //TODO: use hierarchy position.
			//Quaternion gravityOffset = Quaternion.FromToRotation (gravityForward, (upward * -1)); TODO: use gravityOffset

			Vector3[] gridPlaneVertices = (Vector3[])verticesBuffer.Clone ();
			Vector3[] gridPlaneNormals = (Vector3[])normalsBuffer.Clone ();

			GravityBend (
				ref gridPlaneVertices, 
				ref gridPlaneNormals,
				gravityForward,
				gravityUp, 
				planeDef,
				gravityStrength,
				sproutMesh.gravityBendingMultiplierAtMiddle,
				sproutMesh.sideGravityBendingAtBase);

			RotatePoints (ref gridPlaneVertices, forward, upward);
			RotatePoints (ref gridPlaneNormals, forward, upward);

			_sproutMeshData.Add (new SproutMeshData (vertices.Count, gridPlaneVertices.Length, 
				branchPosition, position, branch.id, sprout.helperSproutId, isTwoSided));

			int verticesBeforeAddition = vertices.Count;
			for (int i = 0; i < gridPlaneVertices.Length; i++) {
				vertices.Add ((gridPlaneVertices[i] * scale) + position);
			}
			normals.AddRange (gridPlaneNormals);
			uvs.AddRange (uvsBuffer);
			uv2s.AddRange (uv2sBuffer);
			for (int i = 0; i < uv2sBuffer.Length; i++) {
				uv6s.Add (new Vector4 (branch.id, 0, 0, 0));
			}

			int pointCount = 0;
			// Add triangles
			for (int j = 0; j < resolutionHeight; j++) {
				for (int i = 0; i < resolutionWidth; i++) {
					pointCount = verticesBeforeAddition + i + (j * (resolutionWidth + 1));
					triangles.Add (pointCount);
					triangles.Add (pointCount + resolutionWidth + 1);
					triangles.Add (pointCount + resolutionWidth + 2);
					triangles.Add (pointCount);
					triangles.Add (pointCount + resolutionWidth + 2);
					triangles.Add (pointCount + 1);
				}
			}
			if (sproutMesh.isTwoSided) {
				verticesBeforeAddition += (resolutionHeight + 1) * (resolutionWidth + 1);
				for (int j = 0; j < resolutionHeight; j++) {
					for (int i = 0; i < resolutionWidth; i++) {
						pointCount = verticesBeforeAddition + i + (j * (resolutionWidth + 1));
						triangles.Add (pointCount);
						triangles.Add (pointCount + resolutionWidth + 2);
						triangles.Add (pointCount + resolutionWidth + 1);
						triangles.Add (pointCount);
						triangles.Add (pointCount + 1);
						triangles.Add (pointCount + resolutionWidth + 2);
					}
				}	
			}
		}
		#endregion

		#region Building Blocks
		/// <summary>
		/// Populates the components of a plane, cross or tricross.
		/// </summary>
		/// <param name="planeVertices">Array of vertices to fill.</param>
		/// <param name="planeTangents">Array of tangents to fill.</param>
		/// <param name="planeNormals">Array of normals to fill.</param>
		/// <param name="planeUVs">Array of UVs to fill.</param>
		/// <param name="planeUV2s">Array of UV2s to fill.</param>
		void GetPlane (
			ref Vector3[] planeVertices,
			ref Vector3[] planeNormals,
			ref Vector4[] planeTangents,
			ref Vector4[] planeUVs,
			ref Vector4[] planeUV2s) 
		{
			List<Vector3> _points = new List<Vector3> ();
			List<Vector3> _normals = new List<Vector3> ();
			List<Vector4> _tangents = new List<Vector4> ();
			List<Vector4> _uv2s = new List<Vector4> ();

			if (planeDef.planes == 1) {
				_points.AddRange (GetBasePlane (planeDef.width, planeDef.height, 
					planeDef.pivotPosW, planeDef.pivotPosH, sproutMesh.isTwoSided));
				_normals.AddRange (GetBasePlaneNormals (sproutMesh.isTwoSided));
				_tangents.AddRange (GetBasePlaneTangents (sproutMesh.isTwoSided));
				_uv2s.AddRange (SproutMeshMetaBuilder.GetInstance ().GetPlaneUV2s (
					planeDef.pivotPosW, planeDef.pivotPosH, 
					planeDef.width, planeDef.height, sproutMesh.isTwoSided));
			} else if (planeDef.planes == 2) {
				_points.AddRange (GetBasePlane (planeDef.width, planeDef.height, 
					planeDef.pivotPosW, planeDef.pivotPosH, sproutMesh.isTwoSided));
				_points.AddRange ( 
					RotatePoints (GetBasePlane (planeDef.width, planeDef.height, 
						planeDef.pivotPosW, planeDef.pivotPosH, sproutMesh.isTwoSided), 
						Vector3.forward,
						Vector3.right));
				//_normals.AddRange (GetCenterNormals (_points.ToArray ()));
				/*
				_normals.AddRange (GetBasePlaneNormals (sproutMesh.isTwoSided));
				if (GlobalSettings.useCrossMeshPerpendicularNormasl) {
					_normals.AddRange (GetBasePlaneNormals2 (sproutMesh.isTwoSided));
				} else {
					_normals.AddRange (RotatePoints (GetBasePlaneNormalsFlat (sproutMesh.isTwoSided, true),
							Vector3.forward,
							Vector3.right));
				}
				*/
				_normals.AddRange (GetBasePlaneNormalsFlat (sproutMesh.isTwoSided));
				_normals.AddRange (RotatePoints (GetBasePlaneNormalsFlat (sproutMesh.isTwoSided),
						Vector3.forward,
						Vector3.right));
				_tangents.AddRange (GetBasePlaneTangents (sproutMesh.isTwoSided));
				_tangents.AddRange (GetBasePlaneTangentsA (sproutMesh.isTwoSided));
				_uv2s.AddRange (SproutMeshMetaBuilder.GetInstance ().GetPlaneUV2s (
					planeDef.pivotPosW, planeDef.pivotPosH, 
					planeDef.width, planeDef.height, sproutMesh.isTwoSided));
				_uv2s.AddRange (SproutMeshMetaBuilder.GetInstance ().GetPlaneUV2s (
					planeDef.pivotPosW, planeDef.pivotPosH, 
					planeDef.width, planeDef.height, sproutMesh.isTwoSided));
			} else {
				_points.AddRange (GetBasePlane (planeDef.width, planeDef.width, 
					planeDef.pivotPosW, planeDef.pivotPosH, sproutMesh.isTwoSided));
				_points.AddRange ( 
					RotatePoints (GetBasePlane (planeDef.width, planeDef.width, 
						planeDef.pivotPosW, planeDef.pivotPosH, sproutMesh.isTwoSided), 
						Vector3.forward, 
						Vector3.right));
				_points.AddRange (
					RotatePoints (GetBasePlane (planeDef.width, planeDef.width, 
						planeDef.pivotPosW, planeDef.pivotPosH, sproutMesh.isTwoSided), 
						Vector3.up, Vector3.back));
				_normals.AddRange (GetCenterNormals (_points.ToArray ()));
				_uv2s.AddRange (SproutMeshMetaBuilder.GetInstance ().GetPlaneUV2s (
					planeDef.pivotPosW, planeDef.pivotPosH, 
					planeDef.width, planeDef.height, sproutMesh.isTwoSided));
				_uv2s.AddRange (SproutMeshMetaBuilder.GetInstance ().GetPlaneUV2s (
					planeDef.pivotPosW, planeDef.pivotPosH, 
					planeDef.width, planeDef.height, sproutMesh.isTwoSided));
				_uv2s.AddRange (SproutMeshMetaBuilder.GetInstance ().GetPlaneUV2s (
					planeDef.pivotPosW, planeDef.pivotPosH, 
					planeDef.width, planeDef.height, sproutMesh.isTwoSided));
			}
			planeVertices = _points.ToArray ();
			planeNormals = _normals.ToArray ();
			planeTangents = _tangents.ToArray ();

			uvsBuffer = SproutMeshMetaBuilder.GetInstance ().GetCropPlaneUVs (
				planeDef.planes, planeDef.uvX, planeDef.uvY, 
				planeDef.uvWidth, planeDef.uvHeight, sproutMesh.isTwoSided, planeDef.uvSteps);

			uv2sBuffer = _uv2s.ToArray ();
		}
		/// <summary>
		/// Get the vertices for a plane.
		/// </summary>
		/// <param name="width">Width of the plane.</param>
		/// <param name="height">Height of the plane.</param>
		/// <param name="pivotPosW">Pivot position on width (from 0 to 1).</param>
		/// <param name="pivotPosH">Pivot position on height (from 0 to 1).</param>
		/// <param name="isTwoSided">True if the plane has two sides.</param>
		/// <returns></returns>
		Vector3[] GetBasePlane (
			float width, 
			float height, 
			float pivotPosW, 
			float pivotPosH,
			bool isTwoSided) 
		{
			Vector3[] points = new Vector3[isTwoSided?8:4];
			float wA = -pivotPosW * width;
			float wB = (1 - pivotPosW) * width;
			float hA = -pivotPosH * height;
			float hB = (1 - pivotPosH) * height;

			float postInterplaneSpace = 1.788139E-07f;
			float negInterplaneSpace = -5.960464E-08f;

			points[0] = new Vector3 (wA, postInterplaneSpace, hB); // 0
			points[1] = new Vector3 (wA, negInterplaneSpace, hA); 	// 1
			points[2] = new Vector3 (wB, negInterplaneSpace, hA); 	// 2
			points[3] = new Vector3 (wB, postInterplaneSpace, hB); // 3
			if (isTwoSided) {
				points[4] = new Vector3 (wA, postInterplaneSpace, hB); // 4
				points[5] = new Vector3 (wA, negInterplaneSpace, hA); 	// 5
				points[6] = new Vector3 (wB, negInterplaneSpace, hA); 	// 6
				points[7] = new Vector3 (wB, postInterplaneSpace, hB); // 7
			}

			return points;
		}
		/// <summary>
		/// Gets normals for a plane.
		/// </summary>
		/// <returns>The normals for a plane.</returns>
		Vector3[] GetBasePlaneNormals (bool isTwoSided, bool flat = false) {
			Vector3[] normalsPerPlane = new Vector3[isTwoSided?8:4];
			normalsPerPlane[0] = Quaternion.Euler (0, 45, flat?90:35) * Vector3.up;
			normalsPerPlane[1] = Quaternion.Euler (0, 315, flat?90:35) * Vector3.up;
			normalsPerPlane[2] = Quaternion.Euler (0, 225, flat?90:35) * Vector3.up;
			normalsPerPlane[3] = Quaternion.Euler (0, 135, flat?90:35) * Vector3.up;
			if (isTwoSided) {
				normalsPerPlane[4] = Quaternion.Euler (0, 45, flat?90:145) * Vector3.up;
				normalsPerPlane[5] = Quaternion.Euler (0, 315, flat?90:145) * Vector3.up;
				normalsPerPlane[6] = Quaternion.Euler (0, 225, flat?90:145) * Vector3.up;
				normalsPerPlane[7] = Quaternion.Euler (0, 135, flat?90:145) * Vector3.up;
			}
			return normalsPerPlane;
		}
		Vector3[] GetBasePlaneNormals2 (bool isTwoSided, bool flat = false) {
			Vector3[] normalsPerPlane = new Vector3[isTwoSided?8:4];
			/*
			normalsPerPlane[0] = Vector3.left;
			normalsPerPlane[1] = Vector3.left;
			normalsPerPlane[2] = Vector3.right;
			normalsPerPlane[3] = Vector3.right;
			*/
			normalsPerPlane[0] = Vector3.left;
			normalsPerPlane[1] = Vector3.left;
			normalsPerPlane[2] = Vector3.right;
			normalsPerPlane[3] = Vector3.right;
			if (isTwoSided) {
				normalsPerPlane[4] = Vector3.up;
				normalsPerPlane[5] = Vector3.up;
				normalsPerPlane[6] = Vector3.up;
				normalsPerPlane[7] = Vector3.up;
			}
			return normalsPerPlane;
		}
		Vector3[] GetBasePlaneNormalsFlat (bool isTwoSided, bool flat = false) {
			Vector3[] normalsPerPlane = new Vector3[isTwoSided?8:4];
			normalsPerPlane[0] = Vector3.up;
			normalsPerPlane[1] = Vector3.up;
			normalsPerPlane[2] = Vector3.up;
			normalsPerPlane[3] = Vector3.up;
			if (isTwoSided) {
				normalsPerPlane[4] = Vector3.up;
				normalsPerPlane[5] = Vector3.up;
				normalsPerPlane[6] = Vector3.up;
				normalsPerPlane[7] = Vector3.up;
			}
			return normalsPerPlane;
		}
		/// <summary>
		/// Gets normals for a plane.
		/// </summary>
		/// <returns>The normals for a plane.</returns>
		Vector4[] GetBasePlaneTangents (bool isTwoSided) {
			/*
			Vector4[] tangentsPerPlane = new Vector4[isTwoSided?8:4];
			tangentsPerPlane[0] = Vector3.right;
			tangentsPerPlane[1] = Vector3.right;
			tangentsPerPlane[2] = Vector3.right;
			tangentsPerPlane[3] = Vector3.right;
			if (isTwoSided) {
				tangentsPerPlane[4] = Vector3.right;
				tangentsPerPlane[5] = Vector3.right;
				tangentsPerPlane[6] = Vector3.right;
				tangentsPerPlane[7] = Vector3.right;
			}
			return tangentsPerPlane;
			*/
			Vector4[] tangentsPerPlane = new Vector4[isTwoSided?8:4];
			tangentsPerPlane[0] = Vector3.left;
			tangentsPerPlane[1] = Vector3.left;
			tangentsPerPlane[2] = Vector3.left;
			tangentsPerPlane[3] = Vector3.left;
			if (isTwoSided) {
				tangentsPerPlane[4] = Vector3.left;
				tangentsPerPlane[5] = Vector3.left;
				tangentsPerPlane[6] = Vector3.left;
				tangentsPerPlane[7] = Vector3.left;
			}
			return tangentsPerPlane;
		}
		Vector4[] GetBasePlaneTangentsA (bool isTwoSided) {
			/*
			Vector4[] tangentsPerPlane = new Vector4[isTwoSided?8:4];
			tangentsPerPlane[0] = Vector3.right;
			tangentsPerPlane[1] = Vector3.right;
			tangentsPerPlane[2] = Vector3.right;
			tangentsPerPlane[3] = Vector3.right;
			if (isTwoSided) {
				tangentsPerPlane[4] = Vector3.right;
				tangentsPerPlane[5] = Vector3.right;
				tangentsPerPlane[6] = Vector3.right;
				tangentsPerPlane[7] = Vector3.right;
			}
			return tangentsPerPlane;
			*/
			Vector4[] tangentsPerPlane = new Vector4[isTwoSided?8:4];
			tangentsPerPlane[0] = Vector3.up;
			tangentsPerPlane[1] = Vector3.up;
			tangentsPerPlane[2] = Vector3.up;
			tangentsPerPlane[3] = Vector3.up;
			if (isTwoSided) {
				tangentsPerPlane[4] = Vector3.up;
				tangentsPerPlane[5] = Vector3.up;
				tangentsPerPlane[6] = Vector3.up;
				tangentsPerPlane[7] = Vector3.up;
			}
			return tangentsPerPlane;
		}
		Vector3[] GetCenterNormals (Vector3[] refVertices) {
			Vector3[] centerNormals = new Vector3[refVertices.Length];
			for (int i = 0; i < refVertices.Length; i++) {
				centerNormals [i] = refVertices[i].normalized;
			}
			return centerNormals;
		}
		/// <summary>
		/// Populates the components of a planeX.
		/// </summary>
		/// <param name="planeVertices">Array of vertices to fill.</param>
		/// <param name="planeNormals">Array of normals to fill.</param>
		/// <param name="planeUVs">Array of UVs to fill.</param>
		/// <param name="planeUV2s">Array of UV2s to fill.</param>
		void GetPlaneX (ref Vector3[] planeVertices,
			ref Vector3[] planeNormals,
			ref Vector4[] planeUVs,
			ref Vector4[] planeUV2s)
		{
			planeVertices = new Vector3[sproutMesh.isTwoSided?10:5];
			planeNormals = new Vector3 [planeVertices.Length];
			planeUVs = new Vector4 [planeVertices.Length];
			planeUV2s = new Vector4 [planeVertices.Length];

			float wA = -planeDef.pivotPosW * planeDef.width;
			float wB = (1 - planeDef.pivotPosW) * planeDef.width;
			float hA = -planeDef.pivotPosH * planeDef.height;
			float hB = (1 - planeDef.pivotPosH) * planeDef.height;

			float postInterplaneSpace = 1.788139E-07f;
			float negInterplaneSpace = -5.960464E-08f;

			float angle = Mathf.Atan2 (sproutMesh.depth * (sproutMesh.depth<0?-1:1), planeDef.height / 2f);
			float slopeLength = (planeDef.pivotPosH * planeDef.height) / Mathf.Cos (angle);
			float deltaY = Mathf.Sin (angle) * slopeLength * (sproutMesh.depth < 0?1:-1);

			// Vertices.
			planeVertices[0] = new Vector3 (wA, postInterplaneSpace - deltaY, hB); // 5
			planeVertices[1] = new Vector3 (wA, negInterplaneSpace - deltaY, hA); 	// 6
			planeVertices[2] = new Vector3 (wB, negInterplaneSpace - deltaY, hA); 	// 7
			planeVertices[3] = new Vector3 (wB, postInterplaneSpace - deltaY, hB); // 8
			planeVertices[4] = new Vector3 ((wA + wB) / 2f, -sproutMesh.depth - deltaY, ((hA + hB) / 2f)); // Center top
			if (sproutMesh.isTwoSided) {
				planeVertices[5] = new Vector3 (wA, postInterplaneSpace - deltaY, hB); // 0
				planeVertices[6] = new Vector3 (wA, negInterplaneSpace - deltaY, hA); 	// 1
				planeVertices[7] = new Vector3 (wB, negInterplaneSpace - deltaY, hA); 	// 2
				planeVertices[8] = new Vector3 (wB, postInterplaneSpace - deltaY, hB); // 3
				planeVertices[9] = new Vector3 ((wA + wB) / 2f, -sproutMesh.depth - deltaY, ((hA + hB) / 2f)); // Center bottom
			}

			// Normals.
			planeNormals[0] = new Vector3 (-0.4850712f, 0.727607f, 0.4850713f);
			planeNormals[1] = new Vector3 (-0.5547002f, 0.8320503f, 0);
			planeNormals[2] = new Vector3 (0.5547002f, 0.8320503f, 0);
			planeNormals[3] = new Vector3 (0.4850712f, 0.727607f, 0.4850713f);
			planeNormals[4] = Vector3.up;
			if (sproutMesh.isTwoSided) {
				planeNormals[5] = new Vector3 (-0.4850712f, -0.727607f, 0.4850712f);
				planeNormals[6] = new Vector3 (-0.5547002f, -0.8320503f, 0);
				planeNormals[7] = new Vector3 (0.5547002f, -0.8320503f, 0);
				planeNormals[8] = new Vector3 (0.4850712f, -0.727607f, 0.4850712f);
				planeNormals[9] = Vector3.down;
			}

			// UVs.
			uvsBuffer = SproutMeshMetaBuilder.GetInstance ().GetCropPlaneXUVs (planeDef.uvX, planeDef.uvY, 
				planeDef.uvWidth, planeDef.uvHeight, sproutMesh.isTwoSided, planeDef.uvSteps);

			// UV2s.		
			uv2sBuffer = SproutMeshMetaBuilder.GetInstance ().GetPlaneXUV2s (
				planeDef.pivotPosW, planeDef.pivotPosH, planeDef.width, planeDef.height, sproutMesh.isTwoSided);
		}
		/// <summary>
		/// Populates the components of a grid plane.
		/// </summary>
		/// <param name="planeVertices">Array to save the vertices.</param>
		/// <param name="planeNormals">Array to save the normals.</param>
		/// <param name="planeUVs">Array to save the UV1.</param>
		/// <param name="planeUV2s">Array to save the UV2.</param>
		void GetGridPlane (
			ref Vector3[] planeVertices,
			ref Vector3[] planeNormals,
			ref Vector4[] planeUVs,
			ref Vector4[] planeUV2s)
		{
			planeVertices = 
				new Vector3 [(sproutMesh.resolutionWidth + 1) * (sproutMesh.resolutionHeight + 1) * (sproutMesh.isTwoSided?2:1)];
			planeNormals = new Vector3 [planeVertices.Length];
			planeUVs = new Vector4 [planeVertices.Length];
			planeUV2s = new Vector4 [planeVertices.Length];

			// Starting position for the grid on the width.
			float posW = -planeDef.pivotPosW * planeDef.width;
			// Starting position for the grid on the height.
			float posH = -planeDef.pivotPosH * planeDef.height;
			// Max diagonal length on the plane.
			float maxLength = 1f;
			float widthFromPivot, heightFromPivot;
			if ( planeDef.pivotPosW * planeDef.width > planeDef.width - (planeDef.pivotPosW * planeDef.width)) {
				widthFromPivot = planeDef.pivotPosW * planeDef.width;
			} else {
				widthFromPivot = planeDef.width - (planeDef.pivotPosW * planeDef.width);
			}
			if ( planeDef.pivotPosH * planeDef.height > planeDef.height - (planeDef.pivotPosH * planeDef.height)) {
				heightFromPivot = planeDef.pivotPosH * planeDef.height;
			} else {
				heightFromPivot = planeDef.height - (planeDef.pivotPosH * planeDef.height);
			}
			maxLength = Mathf.Sqrt (Mathf.Pow (widthFromPivot, 2) + Mathf.Pow (heightFromPivot, 2));
			// Value for each segment on the width.
			float segmentW = planeDef.width / sproutMesh.resolutionWidth;
			// Value for each segment on the height.
			float segmentH = planeDef.height / sproutMesh.resolutionHeight;
			// Positions in space for the plane.
			float posX = posW;
			float posY = 0;
			float posZ = posH;
			float posU = 0f;
			float posV = 0f;
			// Points count.
			int pointCount = 0;

			for (int k = 0; k < (sproutMesh.isTwoSided?2:1); k++) {
				for (int j = 0; j <= sproutMesh.resolutionHeight; j++) {
					for (int i = 0; i <= sproutMesh.resolutionWidth; i++) {
						planeVertices [pointCount] = new Vector3 (posX, posY, posZ);
						posU = (posX - posW) / planeDef.width;
						posV = (posZ - posH) / planeDef.height;
						planeNormals [pointCount] = GetGridPlaneNormal (posU, posV, sproutMesh.isTwoSided, k<1);
						if (planeDef.uvSteps == 1) {
							planeUVs [pointCount] = new Vector4 (1f - posV, posU, 1f - posV, posU);
						} else if (planeDef.uvSteps == 2) {
							planeUVs [pointCount] = new Vector4 (1f - posU, 1f - posV, 1f - posU, 1f - posV);
						} else if (planeDef.uvSteps == 3) {
							planeUVs [pointCount] = new Vector4 (posV, 1f - posU, posV, 1f - posU);
						} else {
							planeUVs [pointCount] = new Vector4 (posU, posV, posU, posV);
						}
						planeUV2s [pointCount] = new Vector4 (0f, 0f, 
							Mathf.Sqrt (Mathf.Pow (posX, 2) + Mathf.Pow (posZ, 2)) / maxLength,
							Mathf.Abs(posX) / widthFromPivot / 2f);
						posX += segmentW;
						pointCount++;
					}
					posX = posW;
					posZ += segmentH;
				}
				posX = posW;
				posY = 0;
				posZ = posH;
			}
		}
		/// <summary>
		/// Gets a plane normal from the interpolation of x,y coordinates on the plane.
		/// </summary>
		/// <param name="x">X coordinate (0 to 1).</param>
		/// <param name="y">Y coordinate (0 to 1).</param>
		/// <param name="reverse">True to use the reverse normals of the plane</param>
		/// <returns></returns>
		Vector3 GetGridPlaneNormal (
			float x, 
			float y,
			bool isTwoSided,
			bool reverse = false) {
			Vector3[] baseNormals = GetBasePlaneNormals (isTwoSided);
			Vector3 planeNormal = Vector3.one;
			Vector3 wA, wB;
			if (reverse) {
				wA = Vector3.Slerp (baseNormals[1], baseNormals[0], y);
				wB = Vector3.Slerp (baseNormals[2], baseNormals[3], y);
			} else {
				wA = Vector3.Slerp (baseNormals[5], baseNormals[4], y);
				wB = Vector3.Slerp (baseNormals[6], baseNormals[7], y);
			}
			planeNormal = Vector3.Slerp (wA, wB, x);
			return planeNormal;
		}
		/*
		/// <summary>
		/// Populates the components of a billboard.
		/// </summary>
		/// <param name="planeVertices">Array of vertices to fill.</param>
		/// <param name="planeNormals">Array of normals to fill.</param>
		/// <param name="planeUVs">Array of UVs to fill.</param>
		/// <param name="planeUV2s">Array of UV2s to fill.</param>
		void GetBillboard (
			ref Vector3[] planeVertices,
			ref Vector3[] planeNormals,
			ref Vector4[] planeUVs,
			ref Vector4[] planeUV2s) 
		{
			planeVertices = new Vector3[4];
			float wA = -planeDef.pivotPosW * planeDef.width;
			float wB = (1 - planeDef.pivotPosW) * planeDef.width;
			float hA = -planeDef.pivotPosH * planeDef.height;
			float hB = (1 - planeDef.pivotPosH) * planeDef.height;

			if (!sproutMesh.billboardAtOrigin) {
				planeVertices[0] = new Vector3 ((wA + wB) / 2f, 0, (hA + hB) / 2f); // 0
				planeVertices[1] = new Vector3 ((wA + wB) / 2f, 0, (hA + hB) / 2f); // 1
				planeVertices[2] = new Vector3 ((wA + wB) / 2f, 0, (hA + hB) / 2f); // 2
				planeVertices[3] = new Vector3 ((wA + wB) / 2f, 0, (hA + hB) / 2f); // 3
			} else {
				planeVertices[0] = Vector3.zero;
				planeVertices[1] = Vector3.zero;
				planeVertices[2] = Vector3.zero;
				planeVertices[3] = Vector3.zero;
			}

			planeNormals = new Vector3[4];
			planeNormals[0] = new Vector3 (wA, hB, 1f); // 0
			planeNormals[1] = new Vector3 (wA, hA, 1f); // 1
			planeNormals[2] = new Vector3 (wB, hA, 1f); // 2
			planeNormals[3] = new Vector3 (wB, hB, 1f); // 3

			uvsBuffer = SproutMeshMetaBuilder.GetInstance ().GetCropBillboardUVs (planeDef.uvX, planeDef.uvY, 
						planeDef.uvWidth, planeDef.uvHeight, planeDef.uvSteps);
			uv2sBuffer = SproutMeshMetaBuilder.GetInstance ().GetBillboardUV2s (
						planeDef.pivotPosW, planeDef.pivotPosH, planeDef.width, planeDef.height);
		}
		*/
		/// <summary>
		/// Populates the components of a mesh.
		/// </summary>
		/// <param name="meshVertices">Arrays of vertices to fill.</param>
		/// <param name="meshNormals">Array of normals to fill.</param>
		/// <param name="meshUVs">Array of UVs to fill.</param>
		/// <param name="meshUV2s">Array of UV2s to fill.</param>
		void GetMesh (
			ref Vector3[] meshVertices,
			ref Vector3[] meshNormals,
			ref Vector4[] meshUVs,
			ref Vector4[] meshUV2s) 
		{
			if (sproutMesh.meshGameObject != null && sproutMesh.processedMesh != null) {
				meshVertices = (Vector3[]) sproutMesh.processedMesh.vertices.Clone ();
				meshNormals = (Vector3[]) sproutMesh.processedMesh.normals.Clone ();
				List<Vector4> listMeshUVs = new List<Vector4> ();
				sproutMesh.processedMesh.GetUVs (0, listMeshUVs);
				if (listMeshUVs.Count == 0) {
					listMeshUVs = new List<Vector4> (new Vector4[meshVertices.Length]);
				}
				uvsBuffer = new Vector4 [listMeshUVs.Count];
				for (int i = 0; i < listMeshUVs.Count; i++) {
					uvsBuffer [i] = new Vector4 (listMeshUVs [i].x, listMeshUVs [i].y, listMeshUVs [i].x, listMeshUVs [i].y);
				}
				sproutMesh.processedMesh.GetUVs (1, listMeshUVs);
				if (listMeshUVs.Count == 0) {
					listMeshUVs = new List<Vector4> (new Vector4[meshVertices.Length]);
				}
				uv2sBuffer = new Vector4 [listMeshUVs.Count];
				for (int i = 0; i < listMeshUVs.Count; i++) {
					uv2sBuffer [i] = new Vector4 (listMeshUVs [i].x, listMeshUVs [i].y, listMeshUVs [i].x, listMeshUVs [i].y);
				}
			}
		}
		#endregion

		#region Processing
		/// <summary>
		/// Rotates an array of points.
		/// </summary>
		/// <returns>The points rotated.</returns>
		/// <param name="pointsToRotate">Points to rotate.</param>
		/// <param name="forward">New forward vector.</param>
		/// <param name="up">New up vector.</param>
		Vector3[] RotatePoints (Vector3[] pointsToRotate, Vector3 forward, Vector3 up) {
			for (int i = pointsToRotate.Length - 1; i >= 0; i--) {
				pointsToRotate [i] = Quaternion.LookRotation (forward, up) * pointsToRotate [i];
			}
			return pointsToRotate;
		}
		/// <summary>
		/// Rotates an array of points.
		/// </summary>
		/// <returns>The points rotated.</returns>
		/// <param name="pointsToRotate">Points to rotate.</param>
		/// <param name="forward">New forward vector.</param>
		/// <param name="up">New up vector.</param>
		Vector4[] RotatePoints (Vector4[] pointsToRotate, Vector3 forward, Vector3 up) {
			for (int i = pointsToRotate.Length - 1; i >= 0; i--) {
				pointsToRotate [i] = Quaternion.LookRotation (forward, up) * pointsToRotate [i];
			}
			return pointsToRotate;
		}
		/// <summary>
		/// Rotates an array of points.
		/// </summary>
		/// <param name="pointsToRotate">Points to rotate.</param>
		/// <param name="forward">New forward vector.</param>
		/// <param name="up">New up vector.</param>
		void RotatePoints (ref Vector3[] pointsToRotate, Vector3 forward, Vector3 up) {
			for (int i = pointsToRotate.Length - 1; i >= 0; i--) {
				pointsToRotate [i] = Quaternion.LookRotation (forward, up) * pointsToRotate [i];
			}
		}
		/// <summary>
		/// Rotates an array of points.
		/// </summary>
		/// <param name="pointsToRotate">Points to rotate.</param>
		/// <param name="forward">New forward vector.</param>
		/// <param name="up">New up vector.</param>
		void RotatePoints (ref Vector4[] pointsToRotate, Vector3 forward, Vector3 up) {
			for (int i = pointsToRotate.Length - 1; i >= 0; i--) {
				pointsToRotate [i] = Quaternion.LookRotation (forward, up) * pointsToRotate [i];
				pointsToRotate [i].w = 1f;
			}
		}
		/// <summary>
		/// Rotates a list of points.
		/// </summary>
		/// <param name="pointsToRotate">Points to rotate.</param>
		/// <param name="forward">Forward.</param>
		/// <param name="up">Up.</param>
		void RotatePoints (List<Vector3> pointsToRotate, Vector3 forward, Vector3 up) {
			for (int i = pointsToRotate.Count - 1; i >= 0; i--) {
				pointsToRotate [i] = Quaternion.LookRotation (forward, up) * pointsToRotate [i];
			}
		}
		/// <summary>
		/// Applies bending to an array of vertices. Pivot is Vector3.zero.
		/// </summary>
		/// <param name="pointsToBend">Array of points to bend.</param>
		/// <param name="gravityForward">Vector pointing forward relative to gravity.</param>
		/// <param name="gravityUp">Vector pointing against the gravity.</param>
		/// <param name="radius">Length for the bending.</param>
		/// <param name="gravityStrength">Strength of the bending.</param>
		void GravityBend (
			ref Vector3[] pointsToBend, 
			ref Vector3[] normalsToBend, 
			Vector3 gravityForward, 
			Vector3 gravityUp, 
			PlaneDef planeDef, 
			float gravityStrength, 
			float middleMultiplier, 
			float sideGravityStrength)
		{
			if (gravityStrength < 0) {
				gravityForward *= -1;
				gravityUp *= -1;
				gravityStrength *= -1;
			}
			float radiusStrenght = 0;
			bool inversePlane = false;
			float radius = planeDef.height;
			Quaternion gravityQuaternion = Quaternion.LookRotation (gravityUp * -1, gravityForward);
			Quaternion antigravityQuaternion = Quaternion.LookRotation (gravityUp, gravityForward * -1);
			Quaternion bendQuaternion;
			// Annotation 2702211205
			float widthCenter = planeDef.width * planeDef.pivotPosW;
			float widthRange = (planeDef.pivotPosW>=0.5f?widthCenter:planeDef.width - widthCenter);
			float widthAxisStrength;
			for (int i = pointsToBend.Length - 1; i >= 0; i--) {
				// Trhe closer to the middle line value tends to 1
				widthAxisStrength = 1f - Mathf.InverseLerp (0f, widthRange, Mathf.Abs (pointsToBend[i].x));
				/// Add side gravity.
				pointsToBend[i].y = (planeDef.width * sideGravityStrength) * (1f - widthAxisStrength);
				// Calculate middle bending factor.
 				radiusStrenght = Vector3.Distance (Vector3.zero, pointsToBend [i]) / radius;
				inversePlane = pointsToBend [i].z > 0;
				widthAxisStrength = 1f + (widthAxisStrength * 0.5f * middleMultiplier);
				bendQuaternion = Quaternion.Slerp (Quaternion.identity, 
					(inversePlane?gravityQuaternion:antigravityQuaternion), 
					gravityStrength * radiusStrenght * widthAxisStrength);
				pointsToBend [i] = bendQuaternion * pointsToBend [i];
				normalsToBend [i] = bendQuaternion * normalsToBend [i];
			}
		}
		#endregion

		#region Preview
		/// <summary>
		/// Preview tree for meshing.
		/// </summary>
		private static BroccoTree _previewTree = null;
		/// <summary>
		/// Build a tree to create the preview mesh.
		/// </summary>
		/// <param name="sproutMesh">Definition for the sprouts.</param>
		/// <param name="anew">Should the tree be created from anew.</param>
		/// <returns>A BroccoTree.</returns>
		private static BroccoTree GetPreviewTree (SproutMesh sproutMesh, bool anew = false) {
			if (_previewTree == null || anew) {
				_previewTree = new BroccoTree ();
				BroccoTree.Branch _branch = new BroccoTree.Branch ();
				BroccoTree.Sprout _sprout = new BroccoTree.Sprout ();
				_sprout.groupId = sproutMesh.groupId;
				_sprout.position = 0.5f;
				_branch.AddSprout (_sprout);
				_previewTree.AddBranch (_branch);
			}
			return _previewTree;
		}
		/// <summary>
		/// Destroys the tree used to build a sprout mesh preview.
		/// </summary>
		private static void DestroyPreviewTree () {
			if (_previewTree != null) {
				_previewTree.Clear ();
			}
			Object.DestroyImmediate (_previewTree.obj);
		}
		/// <summary>
		/// Creates a preview mesh for a given SproutMesh definition.
		/// </summary>
		/// <param name="sproutMesh">SproutMesh definition.</param>
		/// <param name="sproutArea">SproutArea definition.</param>
		/// <returns>A preview mesh.</returns>
		public static Mesh GetPreview (SproutMesh sproutMesh, bool isTwoSided, SproutMap.SproutMapArea sproutArea = null) {
			Mesh mesh = GetInstance ().MeshSprouts (GetPreviewTree (sproutMesh, true), sproutMesh.groupId, sproutMesh, sproutArea, -1, isTwoSided);
			List<Vector4> uvs = new List<Vector4> ();
			if (sproutArea != null) {
				mesh.GetUVs (0, uvs);
				SproutMeshMetaBuilder.GetInstance ().GetCropUVs (ref uvs,
					sproutArea.x, sproutArea.y, 
					sproutArea.width, sproutArea.height, sproutArea.normalizedStep);
				mesh.SetUVs (0, uvs);
			}
			DestroyPreviewTree ();
			return mesh;
		}
		#endregion
	}
}