using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Broccoli.Base;
using Broccoli.Pipe;

namespace Broccoli.Manager
{
	/// <summary>
	/// Manages all the meshes created 
	/// </summary>
	public class MeshManager {
		#region MeshData Class
		/// <summary>
		/// Represents a mesh and information about its nature.
		/// </summary>
		public class MeshData {
			public static int MESH_TYPE_FACTOR = 1000000;
			public static int MESH_GROUP_FACTOR = 100;
			/// <summary>
			/// The identifier for the mesh.
			/// </summary>
			public int id = 0;
			/// <summary>
			/// Types of mesh.
			/// </summary>
			public enum Type {
				Custom,
				Branch,
				Sprout
			}
			/// <summary>
			/// The type of mesh.
			/// </summary>
			public Type type = Type.Custom;
			/// <summary>
			/// The actual mesh.
			/// </summary>
			public Mesh mesh;
			/// <summary>
			/// The mesh parts.
			/// </summary>
			public List<MeshPart> meshParts = new List<MeshPart> ();
			/// <summary>
			/// Id of the sprout group if the mesh belongs to one or the branch skin id if the mesh is a branch type.
			/// </summary>
			public int groupId = 0;
			/// <summary>
			/// Id of the sprout area (mapping) in case the mesh belongs to.
			/// </summary>
			public int areaId = -1;
			/// <summary>
			/// Static constructor to get a mesh data of a specified type.
			/// </summary>
			/// <returns>The mesh data.</returns>
			/// <param name="mesh">Mesh object.</param>
			/// <param name="type">Type of mesh.</param>
			/// <param name="groupId">Sprout group identifier.</param>
			public static MeshData GetMeshData (Mesh mesh, Type type, int groupId = 0, int areaId = 0) {
				MeshData meshData = new MeshData ();
				meshData.mesh = mesh;
				meshData.type = type;
				meshData.groupId = groupId;
				meshData.areaId = areaId;
				meshData.id = GetMeshDataId (type, groupId, areaId);
				return meshData;
			}
			/// <summary>
			/// Gets the mesh data identifier.
			/// </summary>
			/// <returns>The mesh data identifier.</returns>
			/// <param name="type">Type.</param>
			/// <param name="groupId">Group identifier.</param>
			/// <param name="areaId">Area identifier.</param>
			public static int GetMeshDataId (Type type, int groupId = 0, int areaId = 0) {
				return (int)type * MESH_TYPE_FACTOR + groupId * MESH_GROUP_FACTOR + areaId;
			}
			/// <summary>
			/// Static constructor to get a mesh data of custom type.
			/// </summary>
			/// <returns>The custom mesh data.</returns>
			/// <param name="mesh">Mesh object.</param>
			/// <param name="groupId">Sprout group identifier.</param>
			public static MeshData GetCustom (Mesh mesh, int groupId = 0) {
				MeshData meshData = GetMeshData (mesh, Type.Custom, groupId);
				return meshData;
			}
			/// <summary>
			/// Static constructor to get a mesh data of branch type.
			/// </summary>
			/// <returns>The bark mesh data.</returns>
			/// <param name="mesh">Mesh object.</param>
			/// <param name="groupId">Sprout group identifier.</param>
			public static MeshData GetBranch (Mesh mesh, int groupId = 0) {
				MeshData meshData = GetMeshData (mesh, Type.Branch, groupId);
				return meshData;
			}
			/// <summary>
			/// Static constructor to geta mesh data of sprout type.
			/// </summary>
			/// <returns>The sprout.</returns>
			/// <param name="mesh">Mesh.</param>
			/// <param name="groupId">Group identifier.</param>
			public static MeshData GetSprout (Mesh mesh, int groupId = 0, int areaId = 0) {
				MeshData meshData = GetMeshData (mesh, Type.Sprout, groupId, areaId);
				return meshData;
			}
		}
		#endregion

		#region MeshPart Class
		/// <summary>
		/// Mesh part class, representing number of indexes 
		/// originated at a given length, girth and point of origin of a parent branch.
		/// </summary>
		public class MeshPart {
			/// <summary>
			/// The start index.
			/// </summary>
			public int startIndex = 0;
			/// <summary>
			/// The length at the parent branch.
			/// </summary>
			public int length = 0;
			/// <summary>
			/// The girth at the parent branch.
			/// </summary>
			public float girth = 0f;
			/// <summary>
			/// The point of origin.
			/// </summary>
			public Vector3 origin = Vector3.zero;
			/// <summary>
			/// Position in the branch.
			/// </summary>
			public float position = 0;
			/// <summary>
			/// The id of the branch.
			/// </summary>
			public int branchId = -1;
			/// <summary>
			/// The sprout identifier.
			/// </summary>
			public int sproutId = -1;
			/// <summary>
			/// The helper mesh identifier.
			/// </summary>
			public int helperMeshId = -1;
			/// <summary>
			/// Gets a mesh part instance.
			/// </summary>
			/// <returns>The mesh part.</returns>
			/// <param name="startIndex">Start index.</param>
			/// <param name="length">Length.</param>
			/// <param name="position">Position.</param>
			/// <param name="girth">Girth.</param>
			public static MeshPart GetMeshPart (int startIndex, int length, float position, float girth = 0f) {
				MeshPart meshPart = new MeshPart ();
				meshPart.startIndex = startIndex;
				meshPart.length = length;
				meshPart.position = position;
				meshPart.girth = girth;
				return meshPart;
			}
			/// <summary>
			/// Gets a mesh part.
			/// </summary>
			/// <returns>The mesh part.</returns>
			/// <param name="startIndex">Start index.</param>
			/// <param name="length">Length.</param>
			/// <param name="position">Position.</param>
			/// <param name="girth">Girth.</param>
			/// <param name="origin">Origin.</param>
			public static MeshPart GetMeshPart (int startIndex, int length, float position, float girth, Vector3 origin) {
				MeshPart meshPart = GetMeshPart (startIndex, length, position, girth);
				meshPart.origin = origin;
				return meshPart;
			}
		}
		#endregion

		#region Vars
		/// <summary>
		/// The meshes data.
		/// </summary>
		Dictionary <int, MeshData> meshesData = new Dictionary <int, MeshData> ();
		/// <summary>
		/// The merged mesh indexes.
		/// </summary>
		Dictionary <int, int> mergedMeshIndex = new Dictionary <int, int> ();
		/// <summary>
		/// The merger mesh vertex offset.
		/// </summary>
		Dictionary <int, int> mergedMeshVertexOffset = new Dictionary <int, int> ();
		/// <summary>
		/// The keep alive meshes.
		/// </summary>
		List<int> keepAliveMeshes = new List<int> ();
		/// <summary>
		/// The total vertices.
		/// </summary>
		int totalVertices = 0;
		/// <summary>
		/// The total triangles.
		/// </summary>
		int totalTriangles = 0;
		/// <summary>
		/// The merged mesh.
		/// </summary>
		Mesh mergedMesh = null;
		/// <summary>
		/// To delete mesh ids.
		/// </summary>
		List<int> toDeleteMeshes = new List<int> ();
		public bool enableAO = false;
		public int samplesAO = 5;
		public float strengthAO = 0.5f;
		#endregion

		#region Events
		/// <summary>
		/// Raises the remove sprout group event.
		/// </summary>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public void OnRemoveSproutGroup (int sproutGroupId) {
			DeregisterSproutGroupMeshes (sproutGroupId);
		}
		#endregion

		#region Usage
		/// <summary>
		/// Begins the usage of this manager.
		/// </summary>
		public void BeginUsage () {
			keepAliveMeshes.Clear ();
		}
		/// <summary>
		/// Ends the usage of this manager.
		/// </summary>
		public void EndUsage () {
			toDeleteMeshes.Clear ();
			var enumerator = meshesData.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				if (!keepAliveMeshes.Contains (enumerator.Current.Key)) {
					toDeleteMeshes.Add (enumerator.Current.Key);
				}
			}
			for (int i = 0; i < toDeleteMeshes.Count; i++) {
				totalVertices -= meshesData [toDeleteMeshes[i]].mesh.vertices.Length;
				if (totalVertices < 0)
					totalVertices = 0;
				totalTriangles -= meshesData [toDeleteMeshes[i]].mesh.triangles.Length / 3;
				if (totalTriangles < 0)
					totalTriangles = 0;
				if (meshesData [toDeleteMeshes[i]].mesh != null) {
					UnityEngine.Object.DestroyImmediate (meshesData [toDeleteMeshes[i]].mesh, true);
				}


				meshesData.Remove (toDeleteMeshes[i]);
			}
			toDeleteMeshes.Clear ();
		}
		#endregion

		#region Management
		/// <summary>
		/// Gets the mesh group identifier.
		/// </summary>
		/// <returns>The mesh group identifier.</returns>
		/// <param name="meshId">Mesh identifier.</param>
		public int GetMeshGroupId (int meshId) {
			if (meshesData.ContainsKey (meshId)) {
				return meshesData [meshId].groupId;
			}
			return 0;
		}
		/// <summary>
		/// Gets the meshes count.
		/// </summary>
		/// <returns>The meshes count.</returns>
		public int GetMeshesCount () {
			return meshesData.Count;
		}
		/// <summary>
		/// Gets the vertices count.
		/// </summary>
		/// <returns>The vertices count.</returns>
		public int GetVerticesCount () {
			return totalVertices;
		}
		/// <summary>
		/// Gets the triangles count.
		/// </summary>
		/// <returns>The triangles count.</returns>
		public int GetTrianglesCount () {
			return totalTriangles;
		}
		#endregion

		#region Mesh operations
		/// <summary>
		/// Registers a mesh.
		/// </summary>
		/// <returns>The mesh.</returns>
		/// <param name="mesh">Mesh.</param>
		/// <param name="type">Type.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public int RegisterMesh (Mesh mesh, MeshData.Type type, int groupId = 0, int areaId = 0) {
			int meshId = MeshData.GetMeshDataId (type, groupId, areaId);
			if (mesh != null && mesh.vertexCount > 0) {
				if (meshesData.ContainsKey (meshId)) {
					totalVertices -= meshesData [meshId].mesh.vertices.Length;
					totalTriangles -= meshesData [meshId].mesh.triangles.Length / 3;
					Object.DestroyImmediate (meshesData [meshId].mesh);
					meshesData.Remove (meshId);
				}
				meshesData.Add (meshId, MeshData.GetMeshData (mesh, type, groupId));
				totalVertices += mesh.vertices.Length;
				totalTriangles += mesh.triangles.Length / 3;
				if (!keepAliveMeshes.Contains (meshId)) {
					keepAliveMeshes.Add (meshId);
				}
			}
			return meshId;
		}
		/// <summary>
		/// Registers a branch mesh.
		/// </summary>
		/// <returns>The bark mesh.</returns>
		/// <param name="mesh">Mesh.</param>
		/// <param name="groupId">Group identifier.</param>
		public int RegisterBranchMesh (Mesh mesh, int groupId = 0) {
			return RegisterMesh (mesh, MeshData.Type.Branch, groupId);
		}
		/// <summary>
		/// Registers a sprout mesh.
		/// </summary>
		/// <returns>The sprout mesh.</returns>
		/// <param name="mesh">Mesh.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public int RegisterSproutMesh (Mesh mesh, int groupId = 0, int areaId = 0) {
			return RegisterMesh (mesh, MeshData.Type.Sprout, groupId, areaId);
		}
		/// <summary>
		/// Registers a custom mesh.
		/// </summary>
		/// <returns>The custom mesh.</returns>
		/// <param name="mesh">Mesh.</param>
		/// <param name="groupId">Group identifier.</param>
		public int RegisterCustomMesh (Mesh mesh, int groupId = 0) {
			return RegisterMesh (mesh, MeshData.Type.Custom, groupId);
		}
		/// <summary>
		/// Deregisters a mesh or meshes based on type, groupId and areaId.
		/// </summary>
		/// <returns><c>true</c>, if mesh was deregistered, <c>false</c> otherwise.</returns>
		/// <param name="type">Type.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public bool DeregisterMesh (MeshData.Type type, int groupId = 0, int areaId = 0) {
			int meshId = MeshData.GetMeshDataId (type, groupId, areaId);
			return DeregisterMesh (meshId);
		}
		/// <summary>
		/// Deregisters a mesh based on its id.
		/// </summary>
		/// <returns><c>true</c>, if a mesh was deregistered, <c>false</c> otherwise.</returns>
		/// <param name="meshId">Mesh identifier.</param>
		public bool DeregisterMesh (int meshId) {
			bool result = false;
			if (meshesData.ContainsKey (meshId)) {
				totalVertices -= meshesData [meshId].mesh.vertices.Length;
				totalTriangles -= meshesData [meshId].mesh.triangles.Length / 3;
				UnityEngine.Object.DestroyImmediate (meshesData [meshId].mesh, true);
				meshesData.Remove (meshId);
				result = true;
			}
			if (keepAliveMeshes.Contains (meshId)) {
				keepAliveMeshes.Remove (meshId);
			}
			return result;
		}
		/// <summary>
		/// Deregisters a mesh or meshes by its type.
		/// </summary>
		/// <returns><c>true</c>, if mesh by type was deregistered, <c>false</c> otherwise.</returns>
		/// <param name="type">Type.</param>
		public bool DeregisterMeshByType (MeshData.Type type) {
			toDeleteMeshes.Clear ();
			int typeFactor = (int)type * MeshData.MESH_TYPE_FACTOR;
			var enumerator = meshesData.GetEnumerator ();
			int meshId;
			while (enumerator.MoveNext ()) {
				meshId = enumerator.Current.Key;
				if (meshId >= typeFactor && meshId < typeFactor + MeshData.MESH_TYPE_FACTOR) {
					toDeleteMeshes.Add (meshId);
				}
			}
			if (toDeleteMeshes.Count > 0) {
				for (int i = 0; i < toDeleteMeshes.Count; i++) {
					DeregisterMesh (toDeleteMeshes[i]);
				}
				toDeleteMeshes.Clear ();
				return true;
			}
			toDeleteMeshes.Clear ();
			return false;
		}
		/// <summary>
		/// Deregisters all meshes assigned to a group.
		/// </summary>
		/// <returns><c>true</c>, if sprout group meshes were deregistered, <c>false</c> otherwise.</returns>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public bool DeregisterSproutGroupMeshes (int sproutGroupId) {
			toDeleteMeshes.Clear ();
			var enumerator = meshesData.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				var meshData = enumerator.Current.Value;
				if (meshData.groupId == sproutGroupId) {
					toDeleteMeshes.Add (enumerator.Current.Key);
				}
			}
			if (toDeleteMeshes.Count > 0) {
				for (int i = 0; i < toDeleteMeshes.Count; i++) {
					DeregisterMesh (toDeleteMeshes[i]);
				}
				toDeleteMeshes.Clear ();
				return true;
			}
			toDeleteMeshes.Clear ();
			return false;
		}
		/// <summary>
		/// Determines whether this instance has a mesh for the specified meshId.
		/// </summary>
		/// <returns><c>true</c> if this instance has a mesh for the specified meshId; otherwise, <c>false</c>.</returns>
		/// <param name="meshId">Mesh identifier.</param>
		public bool HasMesh (int meshId) {
			if (meshesData.ContainsKey (meshId)) {
				return true;
			}
			return false;
		}
		/// <summary>
		/// Determines whether this instance has a mesh for the specified meshId and it has vertices.
		/// </summary>
		/// <returns><c>true</c> if this instance has a mesh for the specified meshId; otherwise, <c>false</c>.</returns>
		/// <param name="meshId">Mesh identifier.</param>
		public bool HasMeshAndNotEmpty (int meshId) {
			if (meshesData.ContainsKey (meshId) && meshesData [meshId].mesh.vertexCount > 0) {
				return true;
			}
			return false;
		}
		/// <summary>
		/// Determines whether this instance has mesh the specified type, groupId  and areaId.
		/// </summary>
		/// <returns><c>true</c> if this instance has a mesh specified by type, groupId and areaId; otherwise, <c>false</c>.</returns>
		/// <param name="type">Type.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public bool HasMesh (MeshData.Type type, int groupId = 0, int areaId = 0) {
			return GetMesh (type, groupId, areaId) != null;
		}
		/// <summary>
		/// Gets a mesh for its meshId.
		/// </summary>
		/// <returns>The mesh.</returns>
		/// <param name="meshId">Mesh identifier.</param>
		public Mesh GetMesh (int meshId) {
			if (meshesData.ContainsKey (meshId)) {
				if (!keepAliveMeshes.Contains (meshId)) {
					keepAliveMeshes.Add (meshId);
				}
				return meshesData [meshId].mesh;
			}
			return null;
		}
		/// <summary>
		/// Gets a mesh for a type, groupId and areaId.
		/// </summary>
		/// <returns>The mesh.</returns>
		/// <param name="type">Type of mesh.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public Mesh GetMesh (MeshData.Type type, int groupId = 0, int areaId = 0) {
			return GetMesh (MeshData.GetMeshDataId (type, groupId, areaId));
		}
		/// <summary>
		/// Merges all meshes.
		/// </summary>
		/// <returns>Merged mesh.</returns>
		/// <param name="transform">Transform.</param>
		public Mesh MergeAll (Transform transform) {
			mergedMeshIndex.Clear ();
			mergedMeshVertexOffset.Clear ();
			Mesh mergingMesh = new Mesh ();

			mergingMesh.subMeshCount = meshesData.Count;
			CombineInstance[] combine = new CombineInstance[meshesData.Count];
			int i = 0;
			int mergedMeshOffset = 0;
			
			int branchVerticesLength = 0;
			int branchTrisLength = 0;
			bool isBranch = true;
			var enumerator = meshesData.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				var meshDataPair = enumerator.Current;
				combine [i].mesh = meshDataPair.Value.mesh;
				combine [i].transform = transform.localToWorldMatrix;
				combine [i].subMeshIndex = 0;
				if (isBranch) {
					branchVerticesLength = meshDataPair.Value.mesh.vertexCount;
					branchTrisLength = meshDataPair.Value.mesh.triangles.Length;
					isBranch = false;
				}
				mergedMeshIndex.Add (meshDataPair.Key, i);
				mergedMeshVertexOffset.Add (meshDataPair.Key, mergedMeshOffset);
				mergedMeshOffset += meshDataPair.Value.mesh.vertices.Length;
				i++;
			}
			mergingMesh.CombineMeshes (combine, false, false);
			mergingMesh.name = "Mesh";
			this.mergedMesh = (Mesh)UnityEngine.Object.Instantiate (mergingMesh);
			this.mergedMesh.name = "Mesh";
			if (GlobalSettings.experimentalAO && enableAO) {
				Broccoli.Factory.TreeFactory.GetActiveInstance ().BeginColliderUsage ();
				Color[] colors = this.mergedMesh.colors;
				List<int> triangles = new List<int> (this.mergedMesh.triangles);
				Broccoli.Utils.AmbientOcclusionBaker.BakeAO (
					Broccoli.Factory.TreeFactory.GetActiveInstance ().GetMeshCollider (),
					ref colors,
					this.mergedMesh.vertices,
					this.mergedMesh.normals,
					(branchTrisLength == triangles.Count?triangles.ToArray ():triangles.GetRange (0, branchTrisLength).ToArray ()),
					(branchTrisLength == triangles.Count?new int[0]:triangles.GetRange (branchTrisLength, triangles.Count - branchTrisLength).ToArray ()),
					Broccoli.Factory.TreeFactory.GetActiveInstance ().gameObject,
					samplesAO,
					0.5f,
					strengthAO
				);
				mergingMesh.colors = colors;
				Broccoli.Factory.TreeFactory.GetActiveInstance ().EndColliderUsage ();
			}
			mergingMesh.RecalculateBounds ();
			return mergingMesh;
		}
		/// <summary>
		/// Gets the submeshes.
		/// </summary>
		/// <returns>The submeshes.</returns>
		public Mesh[] GetSubmeshes () {
			Mesh[] submeshes = new Mesh[meshesData.Count];
			var enumerator = meshesData.GetEnumerator ();
			int i = 0;
			while (enumerator.MoveNext ()) {
				submeshes[i] = enumerator.Current.Value.mesh;
				i++;
			}
			return submeshes;
		}
		/// <summary>
		/// Gets the merged mesh.
		/// </summary>
		/// <returns>The merged mesh.</returns>
		public Mesh GetMergedMesh () {
			return mergedMesh;
		}
		/// <summary>
		/// Gets the index of the merged mesh.
		/// </summary>
		/// <returns>The merged mesh index.</returns>
		/// <param name="meshId">Mesh identifier.</param>
		public int GetMergedMeshIndex (int meshId) {
			if (mergedMeshIndex.ContainsKey (meshId)) {
				return mergedMeshIndex [meshId];
			}
			return -1;
		}
		/// <summary>
		/// Gets the index of the merged mesh.
		/// </summary>
		/// <returns>The merged mesh index.</returns>
		/// <param name="type">Type of mesh.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public int GetMergedMeshIndex (MeshData.Type type, int groupId = 0, int areaId = 0) {
			return GetMergedMeshIndex (MeshData.GetMeshDataId (type, groupId, areaId));
		}
		/// <summary>
		/// Gets the merged mesh identifier.
		/// </summary>
		/// <returns>The merged mesh identifier.</returns>
		/// <param name="index">Index.</param>
		public int GetMergedMeshId (int index) {
			if (index >= 0 && index < mergedMeshIndex.Count && mergedMeshIndex.ContainsValue (index)) {
				var enumerator = mergedMeshIndex.GetEnumerator ();
				while (enumerator.MoveNext ()) {
					var mergedMeshPair = enumerator.Current;
					if (mergedMeshPair.Value == index) {
						return mergedMeshPair.Key;
					}
				}
			}
			return -1;
		}
		/// <summary>
		/// Gets the vertex offset on the merged mesh.
		/// </summary>
		/// <returns>The merged mesh vertex offset.</returns>
		/// <param name="meshId">Mesh identifier.</param>
		public int GetMergedMeshVertexOffset (int meshId) {
			if (mergedMeshVertexOffset.ContainsKey (meshId)) {
				return mergedMeshVertexOffset [meshId];
			}
			return -1;
		}
		/// <summary>
		/// Gets the vertex offset on the merged mesh.
		/// </summary>
		/// <returns>The merged mesh vertex offset.</returns>
		/// <param name="type">Type of mesh.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public int GetMergedMeshVertexOffset (MeshData.Type type, int groupId = 0, int areaId = 0) {
			return GetMergedMeshVertexOffset (MeshData.GetMeshDataId (type, groupId, areaId));
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			var enumerator = meshesData.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				if (enumerator.Current.Value.mesh != null) enumerator.Current.Value.mesh.Clear ();
				UnityEngine.Object.DestroyImmediate (enumerator.Current.Value.mesh, true);
			}
			totalVertices  = 0;
			totalTriangles = 0;
			meshesData.Clear ();
			mergedMeshIndex.Clear ();
			mergedMeshVertexOffset.Clear ();
			keepAliveMeshes.Clear ();
			if (mergedMesh == null) {
				UnityEngine.Object.DestroyImmediate (mergedMesh, true);
			}
			mergedMesh = null;
			toDeleteMeshes.Clear ();
		}
		#endregion

		#region MeshData operations
		public bool IsSproutMesh (int meshId) {
			if (meshesData.ContainsKey (meshId)) {
				if (meshesData [meshId].type == MeshData.Type.Sprout) {
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Gets a mesh data given a mesh id.
		/// </summary>
		/// <returns>The mesh data, null if the data is not found.</returns>
		/// <param name="meshId">Mesh identifier.</param>
		public MeshData GetMeshData (int meshId) {
			if (meshesData.ContainsKey (meshId)) {
				return meshesData [meshId];
			}
			return null;
		}
		/// <summary>
		/// Gets a mesh data given its type and properties.
		/// </summary>
		/// <returns>The mesh data.</returns>
		/// <param name="type">Type of the mesh.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public MeshData GetMeshData (MeshData.Type type, int groupId = 0, int areaId = 0) {
			return GetMeshData (MeshData.GetMeshDataId (type, groupId, areaId));
		}
		/// <summary>
		/// Gets the meshes data.
		/// </summary>
		/// <returns>The meshes data.</returns>
		public Dictionary<int, MeshData> GetMeshesData() {
			return meshesData;
		}
		/// <summary>
		/// Gets meshes data of a specified type.
		/// </summary>
		/// <returns>Type of mesh data.</returns>
		/// <param name="type">Type.</param>
		public Dictionary<int, MeshData> GetMeshesDataOfType (MeshData.Type type) {
			Dictionary<int, MeshData> resultMesh = new Dictionary<int, MeshData> ();
			int typeFactor = (int)type * MeshData.MESH_TYPE_FACTOR;
			var enumerator = meshesData.GetEnumerator ();
			int meshId;
			while (enumerator.MoveNext ()) {
				meshId = enumerator.Current.Key;
				if (meshId >= typeFactor && meshId < typeFactor + MeshData.MESH_TYPE_FACTOR) {
					resultMesh.Add (meshId, meshesData [meshId]);
				}
			}
			return resultMesh;
		}
		/// <summary>
		/// Gets meshes data of a specified type assigned to a group.
		/// </summary>
		/// <returns>Type of mesh data.</returns>
		/// <param name="type">Type.</param>
		public Dictionary<int, MeshData> GetMeshesDataOfType (MeshData.Type type, int group = 0) {
			Dictionary<int, MeshData> resultMesh = new Dictionary<int, MeshData> ();
			int typeFactor = (int)type * MeshData.MESH_TYPE_FACTOR;
			int groupFactor = group * MeshData.MESH_GROUP_FACTOR;
			var enumerator = meshesData.GetEnumerator ();
			int meshId;
			while (enumerator.MoveNext ()) {
				meshId = enumerator.Current.Key;
				if (meshId >= typeFactor + groupFactor && meshId < typeFactor + groupFactor + MeshData.MESH_GROUP_FACTOR) {
					resultMesh.Add (meshId, meshesData [meshId]);
				}
			}
			return resultMesh;
		}
		/// <summary>
		/// Adds information as part of a mesh.
		/// </summary>
		/// <param name="startIndex">Starting vertex index.</param>
		/// <param name="length">Length.</param>
		/// <param name="origin">Point of origin.</param>
		/// <param name="type">Type of mesh data.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public MeshPart AddMeshPart (int startIndex, int length, Vector3 origin, MeshData.Type type, int groupId = 0, int areaId = 0) {
			return AddMeshPart (MeshPart.GetMeshPart(startIndex, length, 0f, 0f, origin), type, groupId, areaId);
		}
		/// <summary>
		/// Adds information as part of a mesh.
		/// </summary>
		/// <param name="startIndex">Starting vertex index.</param>
		/// <param name="length">Length.</param>
		/// <param name="girth">Girth.</param>
		/// <param name="origin">Point of origin.</param>
		/// <param name="type">Type of mesh data.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public MeshPart AddMeshPart (int startIndex, int length, float position, float girth, Vector3 origin, MeshData.Type type, int groupId = 0, int areaId = 0) {
			return AddMeshPart (MeshPart.GetMeshPart(startIndex, length, position, girth, origin), type, groupId, areaId);
		}
		/// <summary>
		/// Adds information as part of a mesh.
		/// </summary>
		/// <param name="meshPart">Mesh part.</param>
		/// <param name="type">Type of mesh data.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public MeshPart AddMeshPart (MeshPart meshPart, MeshData.Type type, int groupId = 0, int areaId = 0) {
			int meshId = MeshData.GetMeshDataId (type, groupId, areaId);
			if (meshesData.ContainsKey (meshId)) {
				if (!keepAliveMeshes.Contains (meshId)) {
					keepAliveMeshes.Add (meshId);
				}
				meshesData [meshId].meshParts.Add (meshPart);
			}
			return meshPart;
		}
		/// <summary>
		/// Determines whether this instance has mesh parts the specified meshId.
		/// </summary>
		/// <returns><c>true</c> if this instance has mesh parts the specified meshId; otherwise, <c>false</c>.</returns>
		/// <param name="meshId">Mesh identifier.</param>
		public bool HasMeshParts (int meshId) {
			if (meshesData.ContainsKey (meshId)) {
				return meshesData [meshId].meshParts.Count > 0;
			}
			return false;
		}
		/// <summary>
		/// Determines whether this instance has mesh parts the specified type groupId and areaId.
		/// </summary>
		/// <returns><c>true</c> if this instance has mesh parts the specified type groupId areaId; otherwise, <c>false</c>.</returns>
		/// <param name="type">Type of mesh data.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public bool HasMeshParts (MeshData.Type type, int groupId = 0, int areaId = 0) {
			int meshId = MeshData.GetMeshDataId (type, groupId, areaId);
			return HasMeshParts (meshId);
		}
		/// <summary>
		/// Gets the mesh parts.
		/// </summary>
		/// <returns>The mesh parts.</returns>
		/// <param name="meshId">Mesh identifier.</param>
		public List<MeshPart> GetMeshParts (int meshId) {
			if (meshesData.ContainsKey (meshId)) {
				if (!keepAliveMeshes.Contains (meshId)) {
					keepAliveMeshes.Add (meshId);
				}
				return meshesData [meshId].meshParts;
			}
			return null;
		}
		/// <summary>
		/// Gets the mesh parts.
		/// </summary>
		/// <returns>The mesh parts.</returns>
		/// <param name="type">Type.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public List<MeshPart> GetMeshParts (MeshData.Type type, int groupId = 0, int areaId = 0) {
			int meshId = MeshData.GetMeshDataId (type, groupId, areaId);
			return GetMeshParts (meshId);
		}
		#endregion
	}
}