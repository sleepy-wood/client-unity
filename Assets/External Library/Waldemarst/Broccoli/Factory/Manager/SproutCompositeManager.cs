using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Broccoli.Model;
using Broccoli.Pipe;
using Broccoli.Factory;
using Broccoli.Utils;

namespace Broccoli.Manager
{
	/// <summary>
	/// Manager for sprout descriptor.
	/// Manages and merges polygon fragments into compound meshes.
	/// Manages textures for polygon fragments.
	/// Manages materials for polygon fragments.
	/// </summary>
	public class SproutCompositeManager {
		#region Vars
		/// <summary>
		/// Size in pixels for the side size of the texture generated.
		/// </summary>
		public int textureSize = 512;
		/// <summary>
		/// Global scale to apply to the texture dimensions.
		/// </summary>
		public float textureGlobalScale = 1f;
		/// <summary>
		/// Maximum level of LOD managed.
		/// </summary>
		private int maxLod = 0;
		/// <summary>
		/// BranchDescriptor id to BranchDescriptor instance dictionary.
		/// </summary>
		/// <typeparam name="int">Id of the branch descriptor.</typeparam>
		/// <typeparam name="BranchDescriptor">BranchDescriptor instance.</typeparam>
		/// <returns>Relationship between a branch descriptor and its id.</returns>
		Dictionary<int, BranchDescriptor> _idToBranchDescriptor = new Dictionary<int, BranchDescriptor> ();
		/// <summary>
		/// Polygon area id to PolygonArea instance dictionary.
		/// </summary>
		/// <typeparam name="int">Id of the PolygonArea instance.</typeparam>
		/// <typeparam name="PolygonArea">PolygonArea instance.</typeparam>
		/// <returns>Relationship between a polygon area and its id.</returns>
		Dictionary<int, PolygonArea> _idToPolygonArea = new Dictionary<int, PolygonArea> ();
		/// <summary>
		/// PolygonArea id to BranchDescriptor instance dictionary.
		/// </summary>
		/// <typeparam name="int">PolygonArea id.</typeparam>
		/// <typeparam name="BranchDescriptor">BranchDescriptor instance.</typeparam>
		/// <returns>Relationship between a polygon area id and its branch descriptor.</returns>
		Dictionary<int, BranchDescriptor> _polygonIdToBranchDescriptor = new Dictionary<int, BranchDescriptor> ();
		/// <summary>
		/// BranchDescriptor id/LOD to merged mesh dictionary.
		/// </summary>
		/// <typeparam name="int">BranchDescriptor id x 100000 + LOD.</typeparam>
		/// <typeparam name="Mesh">Merged mesh.</typeparam>
		/// <returns>Relationship between a branch descriptor and its mesh.</returns>
		Dictionary<int, Mesh> _branchDescriptorIdToMesh = new Dictionary<int, Mesh> ();
		/// <summary>
		/// Polygon id to albedo Texture2D instance dictionary.
		/// </summary>
		/// <typeparam name="Hash128">Texture hash for the polygon texture.</typeparam>
		/// <typeparam name="Texture2D">Albedo Texture2D instance.</typeparam>
		/// <returns>Relationship between a polygon area id and its albedo texture.</returns>
		Dictionary<Hash128, Texture2D> _polygonHashToAlbedoTexture = new Dictionary<Hash128, Texture2D> ();
		/// <summary>
		/// Polygon id to normals Texture2D instance dictionary.
		/// </summary>
		/// <typeparam name="Hash128">Texture hash for the polygon texture.</typeparam>
		/// <typeparam name="Texture2D">Normals Texture2D instance.</typeparam>
		/// <returns>Relationship between a polygon area id and its normals texture.</returns>
		Dictionary<Hash128, Texture2D> _polygonHashToNormalsTexture = new Dictionary<Hash128, Texture2D> ();
		/// <summary>
		/// Polygon id to extras Texture2D instance dictionary.
		/// </summary>
		/// <typeparam name="Hash128">Texture hash for the polygon texture.</typeparam>
		/// <typeparam name="Texture2D">Extras Texture2D instance.</typeparam>
		/// <returns>Relationship between a polygon area id and its extras texture.</returns>
		Dictionary<Hash128, Texture2D> _polygonHashToExtrasTexture = new Dictionary<Hash128, Texture2D> ();
		/// <summary>
		/// Polygon id to subsurface Texture2D instance dictionary.
		/// </summary>
		/// <typeparam name="Hash128">Texture hash for the polygon texture.</typeparam>
		/// <typeparam name="Texture2D">Subsurface Texture2D instance.</typeparam>
		/// <returns>Relationship between a polygon area id and its subsurface texture.</returns>
		Dictionary<Hash128, Texture2D> _polygonHashToSubsurfaceTexture = new Dictionary<Hash128, Texture2D> ();
		/// <summary>
		/// Polygon id to Material instance dictionary.
		/// </summary>
		/// <typeparam name="Hash128">Texture hash for the polygon texture.</typeparam>
		/// <typeparam name="Material">Materials.</typeparam>
		/// <returns>Relationship between a polygon area id and its materials.</returns>
		Dictionary<Hash128, Material> _polygonHashToMaterials = new Dictionary<Hash128, Material> ();
		/// <summary>
		/// Polygon hash to rect in atlas dictionary.
		/// </summary>
		/// <typeparam name="Hash128">Polygon hash for the branches selection.</typeparam>
		/// <typeparam name="Rect">Rect in atlas.</typeparam>
		/// <returns>Relationship between hash and rect.</returns>
		Dictionary<Hash128, Rect> _polygonHashToRect = new Dictionary<Hash128, Rect> ();
		#endregion

		#region Cache Vars
		private int _cachedBranchDescriptorId = 0;
		private int _cachedLOD = 0;
		List<PolygonArea> _cachedPolygonAreas = null;
		/// <summary>
		/// Tree for the current snapshot.
		/// </summary>
		public BroccoTree _snapshotTree = null;
		/// <summary>
        /// Mesh for the current snapshot.
        /// </summary>
        //public Mesh _snapshotMesh = null;
        /// <summary>
        /// Materials of the current snapshot.
        /// </summary>
        public Material[] _snapshotMaterials = new Material [0];
		#endregion

		#region Singleton
		/// <summary>
		/// Singleton for this class.
		/// </summary>
		private static SproutCompositeManager _current = null;
		/// <summary>
		/// Gets the singleton instance of this class.
		/// </summary>
		/// <returns>Singleton for this class.</returns>
		public static SproutCompositeManager Current () {
			if (_current == null ) _current = new SproutCompositeManager ();
			return _current;
		}
		#endregion

		#region Usage
		/// <summary>
		/// Begins usage of this manager.
		/// </summary>
		/// <param name="tree">Broccoli tree to manage.</param>
		/// <param name="factoryScale">Factory scale.</param>
		public void BeginUsage (BroccoTree tree, float factoryScale) {
			// Get mesh and materials.
			//MeshFilter meshFilter = tree.obj.GetComponent<MeshFilter>();
			//meshFilter.sharedMesh.RecalculateNormals ();
			//_snapshotMesh = Object.Instantiate (meshFilter.sharedMesh);
			MeshRenderer meshRenderer = tree.obj.GetComponent<MeshRenderer>();
			_snapshotMaterials = meshRenderer.sharedMaterials;
			_snapshotTree = tree;
		}
		/// <summary>
		/// Ends usage of this manager.
		/// </summary>
		public void EndUsage () {
			
			_snapshotTree = null;
			_snapshotMaterials = new Material [0];
		}
		#endregion

		#region Polygon Management
		/// <summary>
		/// Checks if the manager already has polygons for a snapshot.
		/// </summary>
		/// <param name="id">Id of the snapshot or branch descriptor.</param>
		/// <returns><c>True</c> if the snapshot exists.</returns>
		public bool HasSnapshot (int id) {
			return _idToBranchDescriptor.ContainsKey (id);
		}
		/// <summary>
		/// Adds a polygon area to be managed for this instance.
		/// </summary>
		/// <param name="polygonArea">PolygonArea instance.</param>
		/// <param name="branchDescriptor">BranchDescriptor instance the polygon area belongs to.</param>
		/// <returns><c>True</c> if the polygon area gets managed.</returns>
		public bool ManagePolygonArea (PolygonArea polygonArea, BranchDescriptor branchDescriptor) {
			if (!_idToPolygonArea.ContainsKey (polygonArea.id)) {
				if (polygonArea.lod > maxLod) maxLod = polygonArea.lod;
				_idToPolygonArea.Add (polygonArea.id, polygonArea);
				if (!_idToBranchDescriptor.ContainsKey (branchDescriptor.id)) {
					_idToBranchDescriptor.Add (branchDescriptor.id, branchDescriptor);
				}
				if (!_polygonIdToBranchDescriptor.ContainsKey (polygonArea.id)) {
					_polygonIdToBranchDescriptor.Add (polygonArea.id, branchDescriptor);
				}
				return true;
			}
			return false;
		}
		/// <summary>
		/// Removes all polygon areas belonging to a BranchDescriptor from management from this instance.
		/// </summary>
		/// <param name="branchDescriptorId">Id of the branch descriptor the polygon areas belong to.</param>
		/// <returns><c>True</c> if the polygon areas get removed from management.</returns>
		public bool UnmanagePolygonAreas (int branchDescriptorId) {
			if (!_idToBranchDescriptor.ContainsKey (branchDescriptorId)) {
				BranchDescriptor branchDescriptor = _idToBranchDescriptor [branchDescriptorId];
				if (branchDescriptor != null) {
					List<int> polygonIds = new List<int> ();
					var enumerator = _polygonIdToBranchDescriptor.GetEnumerator ();
					while (enumerator.MoveNext ()) {
						if (enumerator.Current.Value == branchDescriptor) {
							polygonIds.Add (enumerator.Current.Key);
						}
					}
					for (int i = 0; i < polygonIds.Count; i++) {
						_idToPolygonArea.Remove (polygonIds [i]);
						_polygonIdToBranchDescriptor.Remove (polygonIds [i]);
					}
					_idToBranchDescriptor.Remove (branchDescriptorId);
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Gets the mesh for a given branch descriptor id and LOD.
		/// </summary>
		/// <param name="branchDescriptorId"></param>
		/// <param name="lod"></param>
		/// <param name="useCache"></param>
		/// <returns>Mesh for the branch descriptor / LOD.</returns>
		public Mesh GetMesh (int branchDescriptorId, int lod, bool useCache = true) {
			if (lod > maxLod) lod = maxLod;
			int branchLOD = branchDescriptorId * 100000 + lod;
			if (_branchDescriptorIdToMesh.ContainsKey (branchLOD) && useCache) {
				return _branchDescriptorIdToMesh [branchLOD];
			} else {
				// Create the merged mesh for the branchDescriptor/LOD.
				List<PolygonArea> polygons = GetPolygonAreas (branchDescriptorId, lod, false);
				CombineInstance[] combine = new CombineInstance[polygons.Count];
				for (int i = 0; i < polygons.Count; i++) {
					combine[i].mesh = polygons [i].mesh;
					combine[i].transform = Matrix4x4.identity;
				}
				Mesh mesh = new Mesh();
                                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                                mesh.CombineMeshes (combine, false);
				if (_branchDescriptorIdToMesh.ContainsKey (branchLOD)) {
					UnityEngine.Object.DestroyImmediate (_branchDescriptorIdToMesh [branchLOD]);
					_branchDescriptorIdToMesh.Remove (branchLOD);
				}
				_branchDescriptorIdToMesh.Add (branchLOD, mesh);
				return mesh;
			}
		}
		/// <summary>
		/// Clears this instance.
		/// </summary>
		public void Clear () {
			_idToBranchDescriptor.Clear ();
			_idToPolygonArea.Clear ();
			_polygonIdToBranchDescriptor.Clear ();

			// Clear meshes.
			var meshEnumerator = _branchDescriptorIdToMesh.GetEnumerator ();
			while (meshEnumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (meshEnumerator.Current.Value);
			}
			_branchDescriptorIdToMesh.Clear ();

			// Clear albedo textures.
			var albedoTextureEnumerator = _polygonHashToAlbedoTexture.GetEnumerator ();
			while (albedoTextureEnumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (albedoTextureEnumerator.Current.Value);
			}
			_polygonHashToAlbedoTexture.Clear ();

			// Clear normals textures.
			var normalsTextureEnumerator = _polygonHashToNormalsTexture.GetEnumerator ();
			while (normalsTextureEnumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (normalsTextureEnumerator.Current.Value);
			}
			_polygonHashToNormalsTexture.Clear ();

			// Clear extras textures.
			var extrasTextureEnumerator = _polygonHashToExtrasTexture.GetEnumerator ();
			while (extrasTextureEnumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (extrasTextureEnumerator.Current.Value);
			}
			_polygonHashToExtrasTexture.Clear ();

			// Clear subsurface textures.
			var subsurfaceTextureEnumerator = _polygonHashToSubsurfaceTexture.GetEnumerator ();
			while (subsurfaceTextureEnumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (subsurfaceTextureEnumerator.Current.Value);
			}
			_polygonHashToSubsurfaceTexture.Clear ();

			// Clear materials.
			var materialEnumerator = _polygonHashToMaterials.GetEnumerator ();
			while (materialEnumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (materialEnumerator.Current.Value);
			}
			_polygonHashToMaterials.Clear ();
			_polygonHashToRect.Clear ();
		}
		#endregion

		#region Polygon Querying
		/// <summary>
		/// Return the polygon areas registeed on this manager.
		/// </summary>
		/// <value></value>
		public Dictionary<int, PolygonArea> polygonAreas {
			get { return _idToPolygonArea; }
		}
		/// <summary>
		/// Get a polygon area.
		/// </summary>
		/// <param name="branchDescriptorId">BranchDescriptor id.</param>
		/// <param name="lod">Level of detail.</param>
		/// <param name="fragment">Fragment.</param>
		/// <returns>Polygon area instance.</returns>
		public PolygonArea GetPolygonArea (int branchDescriptorId, int lod, int fragment = 0) {
			int id = PolygonArea.GetCompundId (branchDescriptorId, fragment, lod);
			if (_idToPolygonArea.ContainsKey (id)) {
				return _idToPolygonArea [id];
			}
			return null;
		}
		/// <summary>
		/// Get all the polygon area instances that belong to a branch descritpor and
		/// have an especific LOD.
		/// </summary>
		/// <param name="branchDescriptorId">BranchDescriptor id.</param>
		/// <param name="lod">Level of detail.</param>
		/// <param name="useCache"><c>True</c> to used a cached list when calling recurrently with the same parameters.</param>
		/// <returns>List of polygon areas.</returns>
		public List<PolygonArea> GetPolygonAreas (int branchDescriptorId, int lod, bool useCache) {
			if (useCache && _cachedPolygonAreas != null && 
				branchDescriptorId == _cachedBranchDescriptorId && lod == _cachedLOD)
			{
				return _cachedPolygonAreas;
			}
			if (_cachedPolygonAreas == null) {
				_cachedPolygonAreas = new List<PolygonArea> ();	
			} else {
				_cachedPolygonAreas.Clear ();
			}
			_cachedLOD = lod;
			_cachedBranchDescriptorId = branchDescriptorId;
			var polyEnumerator = _idToPolygonArea.GetEnumerator ();
			while (polyEnumerator.MoveNext ()) {
				if (polyEnumerator.Current.Value.branchDescriptorId == branchDescriptorId &&
					polyEnumerator.Current.Value.lod == lod)
				{
					_cachedPolygonAreas.Add (polyEnumerator.Current.Value);
				}
			}
			return _cachedPolygonAreas;
		}
		#endregion

		#region Texture Manager
		/// <summary>
		/// Adds a polygon area to be managed for this instance.
		/// </summary>
		/// <param name="polygonArea">PolygonArea instance.</param>
		/// <param name="branchDescriptor">BranchDescriptor instance the polygon area belongs to.</param>
		/// <returns><c>True</c> if the polygon area gets managed.</returns>
		public bool GenerateTextures (PolygonArea polygonArea, BranchDescriptor branchDescriptor, SproutSubfactory sproutFactory) {
			if (_snapshotTree != null/*_snapshotMesh != null*/) {
				// Show and hide polygons according to branch includes and excludes.
				ReflectIncludeAndExcludesToMesh (polygonArea, branchDescriptor);

				MeshFilter meshFilter = _snapshotTree.obj.GetComponent<MeshFilter> ();
				Mesh _snapshotMesh = meshFilter.sharedMesh;

				//Generate textures.
				Texture2D albedoTex = null;
				Texture2D normalsTex = null;
				Texture2D extrasTex = null;
				Texture2D subsurfaceTex = null;
				int texSize = (int)(textureSize * polygonArea.scale * textureGlobalScale);
				sproutFactory.GeneratePolygonTexture (_snapshotMesh, polygonArea.aabb, _snapshotMaterials, 
                	SproutSubfactory.MaterialMode.Albedo, texSize, texSize, out albedoTex);
				sproutFactory.GeneratePolygonTexture (_snapshotMesh, polygonArea.aabb, _snapshotMaterials, 
					SproutSubfactory.MaterialMode.Normals, texSize, texSize, out normalsTex);
				sproutFactory.GeneratePolygonTexture (_snapshotMesh, polygonArea.aabb, _snapshotMaterials, 
					SproutSubfactory.MaterialMode.Extras, texSize, texSize, out extrasTex);
				sproutFactory.GeneratePolygonTexture (_snapshotMesh, polygonArea.aabb, _snapshotMaterials, 
					SproutSubfactory.MaterialMode.Subsurface, texSize, texSize, out subsurfaceTex);
				if (_polygonHashToAlbedoTexture.ContainsKey (polygonArea.hash)) {
					UnityEngine.Object.DestroyImmediate (_polygonHashToAlbedoTexture [polygonArea.hash]);
					_polygonHashToAlbedoTexture.Remove (polygonArea.hash);
				}
				_polygonHashToAlbedoTexture.Add (polygonArea.hash, albedoTex);
				if (_polygonHashToNormalsTexture.ContainsKey (polygonArea.hash)) {
					UnityEngine.Object.DestroyImmediate (_polygonHashToNormalsTexture [polygonArea.hash]);
					_polygonHashToNormalsTexture.Remove (polygonArea.hash);
				}
				_polygonHashToNormalsTexture.Add (polygonArea.hash, normalsTex);
				if (_polygonHashToExtrasTexture.ContainsKey (polygonArea.hash)) {
					UnityEngine.Object.DestroyImmediate (_polygonHashToExtrasTexture [polygonArea.hash]);
					_polygonHashToExtrasTexture.Remove (polygonArea.hash);
				}
				_polygonHashToExtrasTexture.Add (polygonArea.hash, extrasTex);
				if (_polygonHashToSubsurfaceTexture.ContainsKey (polygonArea.hash)) {
					UnityEngine.Object.DestroyImmediate (_polygonHashToSubsurfaceTexture [polygonArea.hash]);
					_polygonHashToSubsurfaceTexture.Remove (polygonArea.hash);
				}
				_polygonHashToSubsurfaceTexture.Add (polygonArea.hash, subsurfaceTex);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Show and hide branches on the tree mesh according to their branch id and the includes and excludes list.
		/// </summary>
		/// <param name="polygonArea">Polygon area.</param>
		/// <param name="branchDescriptor">Branch descriptor.</param>
		private void ReflectIncludeAndExcludesToMesh (PolygonArea polygonArea, BranchDescriptor branchDescriptor) {
			// Get the list of branches to keep on display.
			GeometryAnalyzer ga = GeometryAnalyzer.Current ();
			List<BroccoTree.Branch> shownBranches = 
				ga.GetFilteredBranches (_snapshotTree, polygonArea.includes, polygonArea.excludes);

			MeshFilter meshFilter = _snapshotTree.obj.GetComponent<MeshFilter> ();
			Mesh _snapshotMesh = meshFilter.sharedMesh;

			// Create index of ids of branches to include.
			List<int> shownBranchIds = new List<int> ();
			for (int i = 0; i < shownBranches.Count; i++) {
				shownBranchIds.Add (shownBranches [i].id);
			}

			// Hide and show the selected branches.
			List<Vector4> uv6 = new List<Vector4> ();
			_snapshotMesh.GetUVs (5, uv6);
			Color[] colors = _snapshotMesh.colors;
			if (colors.Length == 0) {
				colors = new Color[uv6.Count];
				for (int i = 0; i < uv6.Count; i++) {
					colors [i] = Color.white;
				}
			}
			for (int i = 0; i < uv6.Count; i++) {
				if (shownBranchIds.Contains ((int)uv6 [i].x)) {
					colors [i].a = 1f;
				} else {
					colors [i].a = 0f;
				}
			}
			_snapshotMesh.colors = colors;
		}
		/// <summary>
		/// Resets all hidden branches on the tree mesh, showing them again.
		/// </summary>
		public void ShowAllBranchesInMesh () {
			MeshFilter meshFilter = _snapshotTree.obj.GetComponent<MeshFilter> ();
			Mesh _snapshotMesh = meshFilter.sharedMesh;
			List<Vector4> uv6 = new List<Vector4> ();
			_snapshotMesh.GetUVs (5, uv6);
			Color[] colors = _snapshotMesh.colors;
			for (int i = 0; i < uv6.Count; i++) {
					colors [i].a = 1f;
			}
			_snapshotMesh.colors = colors;
		}
		/// <summary>
		/// Set the rect areas from the atlas belonging to each registered polygon texture.
		/// </summary>
		/// <param name="rects">Rects array.</param>
		public void SetAtlasRects (Rect[] rects) {
			_polygonHashToRect.Clear ();
			if (_polygonHashToAlbedoTexture.Count == rects.Length) {
				var enumTex = _polygonHashToSubsurfaceTexture.GetEnumerator ();
				int i = 0;
				while (enumTex.MoveNext ()) {
					_polygonHashToRect.Add (enumTex.Current.Key, rects [i]);
					i++;
				}
			} else {
				Debug.LogWarning ("Atlas rects count is different to the number of textures.");
			}
		}
		/// <summary>
		/// Apply the rect value from the atlas to the polygons uvs.
		/// </summary>
		public void ApplyAtlasUVs () {
			if (_polygonHashToRect.Count > 0) {
				PolygonArea polygonArea;
				var enumPolys = _idToPolygonArea.GetEnumerator ();
				while (enumPolys.MoveNext ()) {
					polygonArea = enumPolys.Current.Value;
					Vector4 uv;
					if (_polygonHashToRect.ContainsKey (polygonArea.hash)) {
						Rect rect = _polygonHashToRect [polygonArea.hash];
						for (int i = 0; i < polygonArea.uvs.Count; i++) {
							uv = polygonArea.uvs [i];
							uv.x = rect.x + rect.width * uv.z;
							uv.y = rect.y + rect.height * uv.w;
							polygonArea.uvs [i] = uv;
						}
					}
				}
			}
		}
		#endregion

		#region Texture Querying
		/// <summary>
		/// Get the list of albedo textures.
		/// </summary>
		/// <returns>List of albedo textures.</returns>
		public List<Texture2D> GetAlbedoTextures () {
			List<Texture2D> texs = new List<Texture2D> ();
			var enumTex = _polygonHashToAlbedoTexture.GetEnumerator ();
			while (enumTex.MoveNext ()) {
				texs.Add (enumTex.Current.Value);
			}
			return texs;
		}
		/// <summary>
		/// Get the list of normals textures.
		/// </summary>
		/// <returns>List of normals textures.</returns>
		public List<Texture2D> GetNormalsTextures () {
			List<Texture2D> texs = new List<Texture2D> ();
			var enumTex = _polygonHashToNormalsTexture.GetEnumerator ();
			while (enumTex.MoveNext ()) {
				texs.Add (enumTex.Current.Value);
			}
			return texs;
		}
		/// <summary>
		/// Get the list of extras textures.
		/// </summary>
		/// <returns>List of extras textures.</returns>
		public List<Texture2D> GetExtrasTextures () {
			List<Texture2D> texs = new List<Texture2D> ();
			var enumTex = _polygonHashToExtrasTexture.GetEnumerator ();
			while (enumTex.MoveNext ()) {
				texs.Add (enumTex.Current.Value);
			}
			return texs;
		}
		/// <summary>
		/// Get the list of subsurface textures.
		/// </summary>
		/// <returns>List of subsurface textures.</returns>
		public List<Texture2D> GetSubsurfaceTextures () {
			List<Texture2D> texs = new List<Texture2D> ();
			var enumTex = _polygonHashToSubsurfaceTexture.GetEnumerator ();
			while (enumTex.MoveNext ()) {
				texs.Add (enumTex.Current.Value);
			}
			return texs;
		}
		#endregion

		#region Material Manager
		/// <summary>
		/// Generates a leaves material.
		/// </summary>
		/// <returns>Leaves material.</returns>
		public static Material GenerateMaterial (Color color, float cutoff, float glossiness, float metallic, float subsurface,
			Color subsurfaceColor, Texture2D albedoTex, Texture2D normalsTex, Texture2D extrasTex, Texture2D subsurfaceTex)
		{
			Material m = MaterialManager.GetLeavesMaterial ();
			MaterialManager.SetLeavesMaterialProperties (
				m, Color.white, 0.6f, 0.1f, 0.1f, 0.5f, Color.white, 
				albedoTex, normalsTex, extrasTex, subsurfaceTex, null);
			return m;
		}
		/// <summary>
		/// Adds a polygon area to be managed for this instance.
		/// </summary>
		/// <param name="polygonArea">PolygonArea instance.</param>
		/// <param name="branchDescriptor">BranchDescriptor instance the polygon area belongs to.</param>
		/// <returns><c>True</c> if the polygon area gets managed.</returns>
		public bool GenerateMaterials (PolygonArea polygonArea, BranchDescriptor branchDescriptor) {
			if (_snapshotTree != null /*_snapshotMesh != null*/) {
				Material m = MaterialManager.GetLeavesMaterial ();
				MaterialManager.SetLeavesMaterialProperties (
                	m, Color.white, 0.6f, 0.1f, 0.1f, 0.5f, Color.white, 
					_polygonHashToAlbedoTexture [polygonArea.hash], _polygonHashToNormalsTexture [polygonArea.hash],
					_polygonHashToExtrasTexture [polygonArea.hash], _polygonHashToSubsurfaceTexture [polygonArea.hash], null);
				if (_polygonHashToMaterials.ContainsKey (polygonArea.hash)) {
					UnityEngine.Object.DestroyImmediate (_polygonHashToMaterials [polygonArea.hash]);
					_polygonHashToMaterials.Remove (polygonArea.hash);
				}
				_polygonHashToMaterials.Add (polygonArea.hash, m);
			}
			return false;
		}
		#endregion

		#region Material Querying
		public Material[] GetMaterials (int branchDescriptorId, int lod) {
			List<Material> mats = new List<Material> ();
			List<PolygonArea> polys = new List<PolygonArea> ();
			var enumPoly = _idToPolygonArea.GetEnumerator ();
			while (enumPoly.MoveNext ()) {
				if (enumPoly.Current.Value.branchDescriptorId == branchDescriptorId &&
					enumPoly.Current.Value.lod == lod)
				{
					polys.Add (enumPoly.Current.Value);
				}
			}
			polys.Sort ((p1,p2) => p1.id.CompareTo(p2.id));
			for (int i = 0; i < polys.Count; i++) {
				if (_polygonHashToMaterials.ContainsKey (polys [i].hash)) {
					mats.Add (_polygonHashToMaterials [polys [i].hash]);
				} else {
					mats.Add (null);
				}
			}
			return mats.ToArray ();
		}
		#endregion
	}
}