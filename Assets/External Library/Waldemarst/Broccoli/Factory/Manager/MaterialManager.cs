using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.Factory;

namespace Broccoli.Manager
{
	/// <summary>
	/// Material manager.
	/// </summary>
	public class MaterialManager {
		#region Vars
		public enum RenderPipelineType {
			Regular,
			LWRP,
			HDRP,
			URP
		}
		public static RenderPipelineType renderPipelineType;
		public static RenderPipelineType leavesShaderPipelineType;
		public static RenderPipelineType barkShaderPipelineType;
		public enum LeavesShaderType {
			NotSet = 0,
			//TreeCreatorOrSimilar = 1, // Deprecated, no longer supported by Unity, no URP support.
			SpeedTree7OrSimilar = 2,
			SpeedTree8OrSimilar = 3
		}
		public static LeavesShaderType leavesShaderType = LeavesShaderType.NotSet;
		public enum BarkShaderType {
			NotSet = 0,
			//TreeCreatorOrSimilar = 1, // Deprecated, no longer supported by Unity, no URP support.
			SpeedTree7OrSimilar = 2,
			SpeedTree8OrSimilar = 3
		}
		public static BarkShaderType barkShaderType = BarkShaderType.NotSet;
		public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }
		public static LeavesShaderType billboardShaderType = LeavesShaderType.NotSet;
		public static Shader defaultShader;
		public static Shader leavesShader;
		public static Shader barkShader;
		public static Shader billboardShader;
		/// <summary>
		/// The list of materials.
		/// </summary>
		Dictionary <int, Material> materials = new Dictionary <int, Material> ();
		/// <summary>
		/// Reference for custom materials.
		/// </summary>
		List<int> customMaterials  = new List<int> ();
		/// <summary>
		/// Reference to owned materials (created by request on this instance and
		/// destroyed on Clear method).
		/// </summary>
		List<int> ownedMaterials = new List<int> ();
		/// <summary>
		/// The colored materials.
		/// </summary>
		Dictionary <int, Material> coloredMaterials = new Dictionary <int, Material> ();
		/// <summary>
		/// Materials owned by this manager with processed origin.
		/// </summary>
		Dictionary<int, Material> processedOwnedMaterials = new Dictionary <int, Material> ();
		/// <summary>
		/// Keeps track of the materials relevant to the pipeline.
		/// </summary>
		List<int> keepAliveMaterials = new List<int> ();
		/// <summary>
		/// Ids of materials to delete.
		/// </summary>
		List<int> toDeleteMaterialIds = new List<int> ();
		/// <summary>
		/// Material used on the branches mesh when selecting branches.
		/// </summary>
		Material branchSelectionMaterial;
		#endregion

		#region Usage
		/// <summary>
		/// Begins the usage of this manager.
		/// </summary>
		public void BeginUsage (TreeFactoryPreferences treeFactoryPreferences) {
			keepAliveMaterials.Clear ();
			DetectRenderPipeline ();
			SetDefaultShader ();
			SetBranchShader (treeFactoryPreferences.preferredShader, treeFactoryPreferences.customBranchShader);
			SetLeavesShader (treeFactoryPreferences.preferredShader, treeFactoryPreferences.customSproutShader);
			SetBillboardShader (treeFactoryPreferences.preferredShader);
		}
		/// <summary>
		/// Ends the usage of this manager, deleting the materials not
		/// relevant to the pipeline.
		/// </summary>
		public void EndUsage () {
			toDeleteMaterialIds.Clear ();
			var materialsEnumerator = materials.GetEnumerator ();
			while (materialsEnumerator.MoveNext ()) {
				if (!keepAliveMaterials.Contains (materialsEnumerator.Current.Key)) {
					toDeleteMaterialIds.Add (materialsEnumerator.Current.Key);
				}
			}
			int materialId;
			for (int i = 0; i < toDeleteMaterialIds.Count; i++) {
				materialId = toDeleteMaterialIds[i];
				materials.Remove (materialId);
				if (customMaterials.Contains (materialId)) {
					customMaterials.Remove (materialId);
				} else if (materials.ContainsKey(materialId) && materials[materialId] != null) {
					UnityEngine.Object.DestroyImmediate (materials [materialId], true);
				}
			}
			
			toDeleteMaterialIds.Clear ();
		}
		public void DetectRenderPipeline () {
			// LightweightPipelineAsset
			// HDRenderPipelineAsset
			// UniversalRenderPipelineAsset
			var currentRenderPipeline = GraphicsSettings.renderPipelineAsset;
			renderPipelineType = RenderPipelineType.Regular;
			leavesShaderPipelineType = RenderPipelineType.Regular;
			barkShaderPipelineType = RenderPipelineType.Regular;
			if (currentRenderPipeline != null) {
				if (GraphicsSettings.renderPipelineAsset.GetType().Name.Contains ("UniversalRenderPipelineAsset")) {
					renderPipelineType = RenderPipelineType.URP;
					leavesShaderPipelineType = RenderPipelineType.URP;
					barkShaderPipelineType = RenderPipelineType.URP;
				} else if (GraphicsSettings.renderPipelineAsset.GetType().Name.Contains ("LightweightPipelineAsset")) {
					renderPipelineType = RenderPipelineType.LWRP;
					leavesShaderPipelineType = RenderPipelineType.LWRP;
					barkShaderPipelineType = RenderPipelineType.LWRP;
				} else if (GraphicsSettings.renderPipelineAsset.GetType().Name.Contains ("HDRenderPipelineAsset")) {
					renderPipelineType = RenderPipelineType.HDRP;
					leavesShaderPipelineType = RenderPipelineType.HDRP;
					barkShaderPipelineType = RenderPipelineType.HDRP;
				}
			}
		}
		public void SetDefaultShader () {
			#if UNITY_2017_2_OR_NEWER
			var currentRenderPipeline = GraphicsSettings.renderPipelineAsset;
			if (currentRenderPipeline != null) {
				#if UNITY_2019_1_OR_NEWER
				defaultShader = currentRenderPipeline.defaultShader;
				#else
				defaultShader = currentRenderPipeline.GetDefaultShader ();
				#endif
			} else {
				defaultShader = Shader.Find ("Hidden/Broccoli/Colored Sprout Preview Mode");
			}
			#else
			defaultShader = Shader.Find ("Hidden/Broccoli/Colored Sprout Preview Mode");
			#endif
		}
		public void SetLeavesShader (TreeFactoryPreferences.PreferredShader preferredShader, Shader customShader = null) {
			// https://forum.unity.com/threads/still-no-speedtree-support.894025/#post-5946692
			// https://issuetracker.unity3d.com/issues/lwrp-shadows-applied-to-speed-trees-are-too-dark-and-change-their-appearance-depending-on-the-distance
			leavesShader = null;
			bool fallbackToST7 = false;
			//bool fallbackToTC = false;
			// Speed Tree 8
			if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8 || preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8Compatible) {
				#if UNITY_2018_4_OR_NEWER
				if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8Compatible && customShader != null) {
					// Use custom shader
					leavesShaderType = LeavesShaderType.SpeedTree8OrSimilar;
					leavesShader = customShader;
					return;
				} else {
					// Use Unity Shader
					#if UNITY_2019_1_OR_NEWER
					var currentRenderPipeline = GraphicsSettings.renderPipelineAsset;
					if (currentRenderPipeline != null) {
						leavesShader = currentRenderPipeline.defaultSpeedTree8Shader;
					}
					if (leavesShader != null) {
						leavesShaderType = LeavesShaderType.SpeedTree8OrSimilar;
						return;
					}
					#endif
					leavesShader = Shader.Find ("Nature/SpeedTree8");
					if (leavesShader != null) {
						leavesShaderType = LeavesShaderType.SpeedTree8OrSimilar;
						leavesShaderPipelineType = RenderPipelineType.Regular;
						return;
					}
				}
				#else
				if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8) preferredShader = TreeFactoryPreferences.PreferredShader.SpeedTree7;
				if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8Compatible) preferredShader = TreeFactoryPreferences.PreferredShader.SpeedTree7Compatible;
				#endif
				fallbackToST7 = true;
			}
			// Speed Tree 7
			if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree7 || preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree7Compatible || fallbackToST7) {
				if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree7Compatible && customShader != null) {
					// Use custom shader
					leavesShaderType = LeavesShaderType.SpeedTree7OrSimilar;
					leavesShader = customShader;
					return;
				} else {
					// Use Unity shader
					#if UNITY_2019_1_OR_NEWER
					var currentRenderPipeline = GraphicsSettings.renderPipelineAsset;
					if (currentRenderPipeline != null) {
						leavesShader = currentRenderPipeline.defaultSpeedTree7Shader;
					}
					#else
					leavesShader = null;
					#endif
					if (leavesShader != null) {
						leavesShaderType = LeavesShaderType.SpeedTree7OrSimilar;
						return;
					}
					leavesShader = Shader.Find ("Nature/SpeedTree7");
					if (leavesShader == null) {
						leavesShader = Shader.Find ("Nature/SpeedTree");
					}
					if (leavesShader != null) {
						leavesShaderType = LeavesShaderType.SpeedTree7OrSimilar;
						leavesShaderPipelineType = RenderPipelineType.Regular;
						return;
					}
				}
				//fallbackToTC = true;
			}
			// Tree Creator. Deprecated, no longer supported by Unity, no URP support.
			/*
			if (preferredShader == TreeFactoryPreferences.PreferredShader.TreeCreator || preferredShader == TreeFactoryPreferences.PreferredShader.TreeCreatorCompatible || fallbackToTC) {
				if (preferredShader == TreeFactoryPreferences.PreferredShader.TreeCreatorCompatible && customShader != null) {
					// Use custom shader
					leavesShaderType = LeavesShaderType.TreeCreatorOrSimilar;
					leavesShader = customShader;
					return;
				} else {
					// Use Unity shader
					leavesShader = Shader.Find ("Hidden/Nature/Tree Creator Leaves Optimized");
					leavesShaderType = LeavesShaderType.TreeCreatorOrSimilar;
					leavesShaderPipelineType = RenderPipelineType.Regular;
				}
			}
			*/
		}
		public void SetBillboardShader (TreeFactoryPreferences.PreferredShader preferredShader) {
			billboardShader = null;
			bool fallbackToST7 = false;
			if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8 || preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8Compatible) {
				// Use Unity Shader
				#if UNITY_2019_1_OR_NEWER
				var currentRenderPipeline = GraphicsSettings.renderPipelineAsset;
				if (currentRenderPipeline != null) {
					billboardShader = currentRenderPipeline.defaultSpeedTree8Shader;
				}
				if (billboardShader != null) {
					billboardShaderType = LeavesShaderType.SpeedTree8OrSimilar;
					return;
				}
				#endif
				if (billboardShader == null) {
					billboardShader = Shader.Find ("Nature/SpeedTree8");
					billboardShaderType = LeavesShaderType.SpeedTree8OrSimilar;
					return;
				}
				fallbackToST7 = true;
			}

			if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree7 || preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree7Compatible || fallbackToST7) {
				var currentRenderPipeline = GraphicsSettings.renderPipelineAsset;
				#if UNITY_2019_1_OR_NEWER
				if (currentRenderPipeline != null) {
					billboardShader = currentRenderPipeline.defaultSpeedTree7Shader;
				}
				if (billboardShader != null) {
					billboardShaderType = LeavesShaderType.SpeedTree7OrSimilar;
					billboardShader = billboardShader.GetDependency ("BillboardShader");
					return;
				}
				#endif
				billboardShader = Shader.Find ("Nature/SpeedTree7");
				if (billboardShader != null) {
					billboardShaderType = LeavesShaderType.SpeedTree7OrSimilar;
					#if UNITY_2019_1_OR_NEWER
					billboardShader = billboardShader.GetDependency ("BillboardShader");
					#endif
					return;
				}
				billboardShader = Shader.Find ("Nature/SpeedTree Billboard");
			}
		}
		public bool IsSproutTwoSided () {
			/*
			if (leavesShaderType == LeavesShaderType.TreeCreatorOrSimilar) {
				return true;
			}
			*/
			return false;
		}
		public void SetBranchShader (TreeFactoryPreferences.PreferredShader preferredShader, Shader customShader = null) {
			// https://forum.unity.com/threads/still-no-speedtree-support.894025/#post-5946692
			// https://issuetracker.unity3d.com/issues/lwrp-shadows-applied-to-speed-trees-are-too-dark-and-change-their-appearance-depending-on-the-distance
			// https://github.com/Unity-Technologies/Graphics/pull/720
			barkShader = null;
			bool fallbackToST7 = false;
			//bool fallbackToTC = false;
			// Speed Tree 8.
			if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8 || preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8Compatible) {
				#if UNITY_2018_4_OR_NEWER
				if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8Compatible && customShader != null) {
					// Use Custom Shader
					barkShaderType = BarkShaderType.SpeedTree8OrSimilar;
					barkShader = customShader;
					return;
				} else {
					// Use Unity Shader
					#if UNITY_2019_1_OR_NEWER
					var currentRenderPipeline = GraphicsSettings.renderPipelineAsset;
					if (currentRenderPipeline != null) {
						barkShader = currentRenderPipeline.defaultSpeedTree8Shader;
					}
					if (barkShader != null) {
						barkShaderType = BarkShaderType.SpeedTree8OrSimilar;
						return;
					}
					#endif
					barkShader = Shader.Find ("Nature/SpeedTree8");
					if (barkShader != null) {
						barkShaderType = BarkShaderType.SpeedTree8OrSimilar;
						barkShaderPipelineType = RenderPipelineType.Regular;
						return;
					}
				}
				#else
				if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8) preferredShader = TreeFactoryPreferences.PreferredShader.SpeedTree7;
				if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8Compatible) preferredShader = TreeFactoryPreferences.PreferredShader.SpeedTree7Compatible;
				#endif
				fallbackToST7 = true;
			}
			// Speed Tree 7.
			if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree7 || preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree7Compatible || fallbackToST7) {
				if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree7Compatible && customShader != null) {
					// Use Custom Shader
					barkShaderType = BarkShaderType.SpeedTree7OrSimilar;
					barkShader = customShader;
					return;
				} else {
					// Use Unity Shader
					#if UNITY_2019_1_OR_NEWER
					var currentRenderPipeline = GraphicsSettings.renderPipelineAsset;
					if (currentRenderPipeline != null) {
						barkShader = currentRenderPipeline.defaultSpeedTree7Shader;
					}
					#else
					barkShader = null;
					#endif
					if (barkShader != null) {
						barkShaderType = BarkShaderType.SpeedTree7OrSimilar;
						return;
					}
					barkShader = Shader.Find ("Nature/SpeedTree7");
					if (barkShader == null) {
						barkShader = Shader.Find ("Nature/SpeedTree");
					}
					if (barkShader != null) {
						barkShaderType = BarkShaderType.SpeedTree7OrSimilar;
						barkShaderPipelineType = RenderPipelineType.Regular;
						return;
					}
				}
				//fallbackToTC = true;
			}
			// Tree Creator. // Deprecated, no longer supported by Unity, no URP support.
			/*
			if (preferredShader == TreeFactoryPreferences.PreferredShader.TreeCreator || preferredShader == TreeFactoryPreferences.PreferredShader.TreeCreatorCompatible || fallbackToTC) {
				if (preferredShader == TreeFactoryPreferences.PreferredShader.TreeCreatorCompatible && customShader != null) {
					// Use Custom Shader
					barkShaderType = BarkShaderType.TreeCreatorOrSimilar;
					barkShader = customShader;
				} else {
					// Use Unity Shader
					barkShader = Shader.Find ("Hidden/Nature/Tree Creator Bark Optimized");
					barkShaderType = BarkShaderType.TreeCreatorOrSimilar;
					barkShaderPipelineType = RenderPipelineType.Regular;
				}
			}
			*/
		}
		public Shader GetDefaultShader () { return defaultShader; }
		public Shader GetLeavesShader () { return leavesShader; }
		public Shader GetBarkShader () { return barkShader; }
		public Shader GetBillboardShader () { return billboardShader; }
		#endregion

		#region Management
		/// <summary>
		/// Registers a material on the manager.
		/// </summary>
		/// <returns><c>true</c>, if material was registered, <c>false</c> otherwise.</returns>
		/// <param name="id">Identifier of the material.</param>
		/// <param name="material">Material.</param>
		public bool RegisterCustomMaterial (int id, Material material) {
			if (material != null) {
				if (materials.ContainsKey (id) && customMaterials.Contains (id)) {
					materials.Remove (id);
					customMaterials.Remove (id);
					ownedMaterials.Remove (id);
				}
				if (materials.ContainsKey (id)) {
					materials.Remove (id);
				}
				materials.Add (id, material);
				customMaterials.Add (id);
				if (!keepAliveMaterials.Contains (id)) {
					keepAliveMaterials.Add (id);
				}
				return true;
			}
			return false;
		}
		/// <summary>
		/// Registers a material on the manager.
		/// </summary>
		/// <returns><c>true</c>, if material was registered, <c>false</c> otherwise.</returns>
		/// <param name="type">Type of material.</param>
		/// <param name="material">Material.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public bool RegisterCustomMaterial (MeshManager.MeshData.Type type, 
			Material material, 
			int groupId = 0, 
			int areaId = 0) 
		{
			return RegisterCustomMaterial (MeshManager.MeshData.GetMeshDataId (type, groupId, areaId), material);
		}
		/// <summary>
		/// Deregisters a material on this manager based on its id.
		/// </summary>
		/// <returns><c>true</c>, if material was deregistered, <c>false</c> otherwise.</returns>
		/// <param name="id">Identifier.</param>
		public bool DeregisterMaterial (int id) {
			if (materials.ContainsKey (id)) {
				if (customMaterials.Contains (id)) {
					customMaterials.Remove (id);
				} else {
					Object.DestroyImmediate (materials [id]);
					ownedMaterials.Remove (id);
				}
				if (processedOwnedMaterials.ContainsKey (id)) {
					processedOwnedMaterials.Remove (id);
				}
				materials.Remove (id);
				if (keepAliveMaterials.Contains (id)) {
					keepAliveMaterials.Remove (id);
				}
				return true;
			}
			return false;
		}
		/// <summary>
		/// Deregisters a material on this manager based on its type and properties.
		/// </summary>
		/// <returns><c>true</c>, if material was deregistered, <c>false</c> otherwise.</returns>
		/// <param name="type">Type.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public bool DeregisterMaterial (MeshManager.MeshData.Type type, int groupId = 0, int areaId = 0) {
			return DeregisterMaterial (MeshManager.MeshData.GetMeshDataId (type, groupId, areaId));
		}
		/// <summary>
		/// Deregisters all materials of a type.
		/// </summary>
		/// <returns><c>true</c>, if material by type was deregistered, <c>false</c> otherwise.</returns>
		/// <param name="type">Type.</param>
		public bool DeregisterMaterialByType (MeshManager.MeshData.Type type) {
			toDeleteMaterialIds.Clear ();
			var materialsEnumerator = materials.GetEnumerator ();
			int materialId;
			while (materialsEnumerator.MoveNext ()) {
				materialId = materialsEnumerator.Current.Key;
				int typeFactor = (int)type * 10000;
				if (materialId >= typeFactor && materialId < typeFactor + 10000) {
					toDeleteMaterialIds.Add (materialId);
				}
			}
			if (toDeleteMaterialIds.Count > 0) {
				for (int i = 0; i < toDeleteMaterialIds.Count; i++) {
					materialId = toDeleteMaterialIds [i];
					DeregisterMaterial (materialId);
				}
				toDeleteMaterialIds.Clear ();
				return true;
			}
			toDeleteMaterialIds.Clear ();
			return false;
		}
		/// <summary>
		/// Determines whether this instance has a material by id.
		/// </summary>
		/// <returns><c>true</c> if this instance has material the specified id; otherwise, <c>false</c>.</returns>
		/// <param name="id">Identifier.</param>
		public bool HasMaterial (int id) {
			return materials.ContainsKey (id);
		}
		/// <summary>
		/// Determines whether this instance has a material by type, groupId and areaId.
		/// </summary>
		/// <returns><c>true</c> if this instance has a material; otherwise, <c>false</c>.</returns>
		/// <param name="type">Type.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public bool HasMaterial (MeshManager.MeshData.Type type, int groupId = 0, int areaId = 0) {
			return HasMaterial (MeshManager.MeshData.GetMeshDataId (type, groupId, areaId));
		}
		/// <summary>
		/// Determines whether a material id belongs to a custom material.
		/// </summary>
		/// <returns><c>true</c> if the id belongs to a custom material; otherwise, <c>false</c>.</returns>
		/// <param name="id">Identifier.</param>
		public bool IsCustomMaterial (int id) {
			return customMaterials.Contains (id);
		}
		/// <summary>
		/// Determines whether a material id belongs to a custom material.
		/// </summary>
		/// <returns><c>true</c> if the id belongs to a custom material; otherwise, <c>false</c>.</returns>
		/// <param name="type">Type.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public bool IsCustomMaterial (MeshManager.MeshData.Type type, int groupId = 0, int areaId = 0) {
			return IsCustomMaterial (MeshManager.MeshData.GetMeshDataId (type, groupId, areaId));
		}
		/// <summary>
		/// Determines whether a material is owned by this manager.
		/// </summary>
		/// <returns><c>true</c> if the material is owned by this manager; otherwise, <c>false</c>.</returns>
		/// <param name="id">Identifier.</param>
		public bool IsOwnedMaterial (int id) {
			return ownedMaterials.Contains (id);
		}
		/// <summary>
		/// Determines whether a material is owned by this manager.
		/// </summary>
		/// <returns><c>true</c> if the material is owned by this manager; otherwise, <c>false</c>.</returns>
		/// <param name="type">Type.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public bool IsOwnedMaterial (MeshManager.MeshData.Type type, int groupId = 0, int areaId = 0) {
			return IsOwnedMaterial (MeshManager.MeshData.GetMeshDataId (type, groupId, areaId));
		}
		/// <summary>
		/// Gets an owned material (created and destroy by this manager only).
		/// </summary>
		/// <returns>The owned material.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="shaderLocation">Shader location.</param>
		public Material GetOwnedMaterial (int id, string shaderLocation) {
			Material material = null;
			// Get existing material to return if equals the shader.
			if (ownedMaterials.Contains (id) && materials.ContainsKey (id)) {
				if (materials [id].shader.name.Equals (shaderLocation)) {
					material = materials [id];
				} else {
					Object.DestroyImmediate (materials [id]);
					materials.Remove (id);
					ownedMaterials.Remove (id);
				}
			}
			if (material == null) {
				material = new Material (Shader.Find (shaderLocation));
				if (materials.ContainsKey (id)) {
					materials.Remove (id);
				}
				materials.Add (id, material);
				ownedMaterials.Add (id);
			}
			if (!keepAliveMaterials.Contains (id)) {
				keepAliveMaterials.Add (id);
			}
			return material;
		}
		/// <summary>
		/// Gets an owned material (cloned from a base material by this manager).
		/// </summary>
		/// <returns>The owned material.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="materialToClone">Material to clone.</param>
		public Material GetOwnedMaterial (int id, Material materialToClone) {
			Material material = null;
			if (ownedMaterials.Contains (id) && materials.ContainsKey (id)) {
				Object.DestroyImmediate (materials [id]);
				materials.Remove (id);
				ownedMaterials.Remove (id);
			}

			if (materials.ContainsKey (id)) {
				materials.Remove (id);
			}
			material = Object.Instantiate<Material> (materialToClone);
			materials.Add (id, material);
			ownedMaterials.Add (id);
		
			if (!keepAliveMaterials.Contains (id)) {
				keepAliveMaterials.Add (id);
			}
			return material;
		}
		/// <summary>
		/// Gets a material by id.
		/// </summary>
		/// <returns>The material.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="makeClone">If set to <c>true</c> return a clone of the material.</param>
		public Material GetMaterial (int id, bool makeClone = false) {
			if (materials.ContainsKey (id)) {
				if (!keepAliveMaterials.Contains (id)) {
					keepAliveMaterials.Add (id);
				}
				if (makeClone) {
					return UnityEngine.Object.Instantiate<Material> (materials[id]);
				} else {
					return materials [id];
				}
			}
			return null;
		}
		/// <summary>
		/// Gets a material by type, groupId and areaId.
		/// </summary>
		/// <returns>The material.</returns>
		/// <param name="type">Type of the material.</param>
		/// <param name="makeClone">If set to <c>true</c> return a clone of the material.</param>
		/// <param name="groupId">Group identifier.</param>
		/// <param name="areaId">Area identifier.</param>
		public Material GetMaterial (MeshManager.MeshData.Type type, 
			bool makeClone = false,
			int groupId = 0, 
			int areaId = 0) 
		{
			return GetMaterial (MeshManager.MeshData.GetMeshDataId (type, groupId, areaId), makeClone);
		}
		/// <summary>
		/// Gets materials by type.
		/// </summary>
		/// <returns>The materials by type.</returns>
		/// <param name="type">Type.</param>
		/// <param name="makeClones">If set to <c>true</c> return clones.</param>
		public Dictionary<int, Material> GetMaterialsByType (MeshManager.MeshData.Type type, bool makeClones = false) {
			Dictionary<int, Material> mats = new Dictionary<int, Material> ();
			var materialsEnumerator = materials.GetEnumerator ();
			int materialId;
			while (materialsEnumerator.MoveNext ()) {
				materialId = materialsEnumerator.Current.Key;
				int typeFactor = (int)type * 10000;
				if (materialId >= typeFactor && materialId < typeFactor + 10000) {
					if (makeClones) {
						mats.Add (materialId, UnityEngine.Object.Instantiate<Material> (materials[materialId]));
					} else {
						mats.Add (materialId, materials [materialId]);
					}
				}
			}
			return mats;
		}
		/// <summary>
		/// Gets the materials.
		/// </summary>
		/// <returns>The materials.</returns>
		public Dictionary <int, Material> GetMaterials () {
			return materials;
		}
		/// <summary>
		/// Return the material used when selecting branches.
		/// </summary>
		/// <returns></returns>
		public Material GetBranchSelectionMaterial () {
			if (branchSelectionMaterial == null) {
				branchSelectionMaterial = new Material (Shader.Find ("Hidden/Broccoli/Tree Creator Branch Selection"));
			}
			return branchSelectionMaterial;
		}
		/// <summary>
		/// Gets the materials count.
		/// </summary>
		/// <returns>The materials count.</returns>
		public int GetMaterialsCount () {
			return materials.Count;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			var materialsEnumerator = materials.GetEnumerator ();
			while (materialsEnumerator.MoveNext ()) {
				if (!customMaterials.Contains (materialsEnumerator.Current.Key)) {
					UnityEngine.Object.DestroyImmediate (materialsEnumerator.Current.Value, true);
				}
			}
			materials.Clear ();
			customMaterials.Clear ();
			ownedMaterials.Clear ();

			// Clear colored materials.
			var coloredMaterialsEnumerator = coloredMaterials.GetEnumerator ();
			while (coloredMaterialsEnumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (coloredMaterialsEnumerator.Current.Value, true);
			}
			coloredMaterials.Clear ();

			// Clear processed owned materials.
			var processedMaterialsEnumerator = processedOwnedMaterials.GetEnumerator ();
			while (processedMaterialsEnumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (processedMaterialsEnumerator.Current.Value, true);
			}
			processedOwnedMaterials.Clear ();

			keepAliveMaterials.Clear ();
		}
		#endregion

		#region Util
		/// <summary>
		/// Gets the default unlit material.
		/// </summary>
		/// <returns>Default unlit material.</returns>
		public static Material[] GetUnlitMaterials (Material[] originalMaterials) {
			Shader unlitShader = GetUnlitShader ();
			Material[] unlitMaterials = new Material [originalMaterials.Length];
			for (int i = 0; i < unlitMaterials.Length; i++) {
				unlitMaterials [i] = new Material (unlitShader);
				if (originalMaterials [i].HasProperty ("_Color")) {
					unlitMaterials [i].SetColor ("_Color", originalMaterials [i].GetColor ("_Color"));
				}
				if (originalMaterials [i].HasProperty ("_MainTex")) {
					unlitMaterials [i].SetTexture ("_MainTex", originalMaterials [i].GetTexture ("_MainTex"));
				}
				unlitMaterials [i].SetFloat ("_Cutoff", 0.2f);
				if (ExtensionManager.isURP || ExtensionManager.isHDRP) {
					if (originalMaterials [i].HasProperty ("_Color")) {
						unlitMaterials [i].SetColor ("_BaseColor", originalMaterials [i].GetColor ("_Color"));
						unlitMaterials [i].SetColor ("_Color", originalMaterials [i].GetColor ("_Color"));
					}
					if (originalMaterials [i].HasProperty ("_MainTex")) {
						unlitMaterials [i].SetTexture ("_BaseMap", originalMaterials [i].GetTexture ("_MainTex"));
						unlitMaterials [i].SetTexture ("_MainTex", originalMaterials [i].GetTexture ("_MainTex"));
					}
					unlitMaterials [i].SetFloat ("_Surface", 0f);
					unlitMaterials [i].SetFloat ("_AlphaClip", 1f);
					unlitMaterials [i].SetFloat ("_Cull", 0f);
					unlitMaterials [i].SetFloat ("_Cutoff", 0.1f);
				}
			}
			return unlitMaterials;
		}
		/// <summary>
		/// Gets the default unlit shader.
		/// </summary>
		/// <returns>Default unlit shader.</returns>
		public static Shader GetUnlitShader () {
			Shader unlitShader = null;
			if (ExtensionManager.isURP) {
				unlitShader = Shader.Find ("Universal Render Pipeline/Unlit");
				if (unlitShader == null) {
					unlitShader = Shader.Find ("Lightweight Render Pipeline/Unlit");
				}
			} else if (ExtensionManager.isHDRP) {
				unlitShader = Shader.Find ("HDRP/Unlit");
			}
			if (unlitShader == null) {
				unlitShader = Shader.Find ("Hidden/Broccoli/Billboard Unlit");
			}
			unlitShader = Shader.Find ("Hidden/Broccoli/Billboard Unlit");
			return unlitShader;
		}
		/// <summary>
		/// Gets a default colored material.
		/// </summary>
		/// <returns>The colored material.</returns>
		public Material GetColoredMaterial (bool isBranch = false) {
			return GetColoredMaterial (Color.white, isBranch);
		}
		/// <summary>
		/// Gets a colored material.
		/// </summary>
		/// <returns>The colored material.</returns>
		/// <param name="color">Color.</param>
		/// <param name="makeClone">If set to <c>true</c> make clone.</param>
		public Material GetColoredMaterial (Color color, bool isBranch = false, bool isTranslucid = false) {
			if (isTranslucid) {
				color.a = 0.2f;
			} else {
				color.a = 1f;
			}
			int colorHashCode = color.GetHashCode ();
			if (coloredMaterials.ContainsKey (colorHashCode) && 
				coloredMaterials [colorHashCode] != null ) {
				return coloredMaterials [colorHashCode];
			} else {
				Material material;
				if (isBranch) {
					material = new Material (Shader.Find ("Hidden/Broccoli/Colored Branch Preview Mode"));
				} else {
					material = new Material (defaultShader);
					if (isTranslucid) {
						if (renderPipelineType == RenderPipelineType.Regular) {
							SetupMaterialWithBlendMode (material, BlendMode.Fade);
						} else {
							material.SetFloat ("_Surface", 1f);
							SetupMaterialWithBlendMode (material, BlendMode.Transparent);
						}
					}
				}
				material.SetColor ("_Color", color);
				material.SetColor ("_BaseColor", color);
				material.SetFloat ("_Cull", 0f);
				if (coloredMaterials.ContainsKey (colorHashCode)) {
					coloredMaterials.Remove (colorHashCode);
				}
				coloredMaterials.Add (colorHashCode, material);
				return material;
			}
		}
		public void CommitToColoredMaterials (string property, float value, bool isBranch = false) {
			List<Material> _materials = new List<Material> ();
			foreach (Material _material in coloredMaterials.Values) {
				_materials.Add(_material);
			}
			CommitToColoredMaterials (_materials.ToArray(), property, value, isBranch);
		}
		public void CommitToColoredMaterials (Material[] _coloredMaterials, string property, float value, bool isBranch = false) {
			for (int i = 0; i < _coloredMaterials.Length; i++) {
				if (isBranch)  {
					if (_coloredMaterials[i].shader.name.CompareTo ("Hidden/Broccoli/Colored Branch Preview Mode") == 0) {
						_coloredMaterials[i].SetFloat (property, value);
					}
				} else {
					if (_coloredMaterials[i].shader.name.CompareTo ("Hidden/Broccoli/Colored Sprout Preview Mode") == 0) {
						_coloredMaterials[i].SetFloat (property, value);
					}
				}
			}
		}
		public void CommitToColoredMaterials (string property, Color value, bool isBranch = false) {
			List<Material> _materials = new List<Material> ();
			foreach (Material _material in coloredMaterials.Values) {
				_materials.Add(_material);
			}
			CommitToColoredMaterials (_materials.ToArray(), property, value, isBranch);
		}
		public void CommitToColoredMaterials (Material[] _coloredMaterials, string property, Color value, bool isBranch = false) {
			for (int i = 0; i < _coloredMaterials.Length; i++) {
				if (isBranch)  {
					if (_coloredMaterials[i].shader.name.CompareTo ("Hidden/Broccoli/Colored Branch Preview Mode") == 0) {
						_coloredMaterials[i].SetColor (property, value);
					}
				} else {
					if (_coloredMaterials[i].shader.name.CompareTo ("Hidden/Broccoli/Colored Sprout Preview Mode") == 0) {
						_coloredMaterials[i].SetColor (property, value);
					}
				}
			}
		}
		public void SetMaterialsBlendMode (BlendMode blendMode, bool isBranch = false) {
			List<Material> _materials = new List<Material> ();
			foreach (Material _material in coloredMaterials.Values) {
				_materials.Add(_material);
			}
			SetMaterialsBlendMode (_materials.ToArray(), blendMode, isBranch);
		}
		public void SetMaterialsBlendMode (Material[] _coloredMaterials, BlendMode blendMode, bool isBranch = false) {
			for (int i = 0; i < _coloredMaterials.Length; i++) {
				if (isBranch)  {
					if (_coloredMaterials[i].shader.name.CompareTo ("Hidden/Broccoli/Colored Branch Preview Mode") == 0) {
						SetupMaterialWithBlendMode(_coloredMaterials[i], blendMode);
					}
				} else {
					if (_coloredMaterials[i].shader.name.CompareTo ("Hidden/Broccoli/Colored Sprout Preview Mode") == 0) {
						SetupMaterialWithBlendMode(_coloredMaterials[i], blendMode);
					}
				}
			}
		}
		public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }
		/// <summary>
		/// Gets the shadow base texture path.
		/// </summary>
		/// <returns>The shadow base texture path.</returns>
		public static string GetShadowTexPath () {
			return GetBaseTexPath ("Broccoli/Textures/Base/brcc_shadow.png");
		}
		/// <summary>
		/// Gets the normal specular base texture path.
		/// </summary>
		/// <returns>The normal specular base texture path.</returns>
		public static string GetNormalSpecularTexPath () {
			return GetBaseTexPath ("Broccoli/Textures/Base/brcc_normal_specular.png");
		}
		/// <summary>
		/// Gets the translucency base texture path.
		/// </summary>
		/// <returns>The translucency base texture path.</returns>
		public static string GetTranslucencyTexPath () {
			return GetBaseTexPath ("Broccoli/Textures/Base/brcc_translucency_gloss.png");
		}
		/// <summary>
		/// Gets the base texture path.
		/// </summary>
		/// <returns>The base texture path.</returns>
		/// <param name="relativePath">Relative path.</param>
		public static string GetBaseTexPath (string relativePath) {
			relativePath = ExtensionManager.resourcesPath + relativePath;
			return relativePath;
		}
		/// <summary>
		/// Gets the shadow base texture.
		/// </summary>
		/// <returns>The shadow texture.</returns>
		public static Texture2D GetShadowTex (bool makeCopy = false) {
			return GetBaseTex (GetShadowTexPath (), makeCopy);
		}
		/// <summary>
		/// Gets the normal specular base texture.
		/// </summary>
		/// <returns>The normal specular texture.</returns>
		public static Texture2D GetNormalSpecularTex (bool makeCopy = false) {
			return GetBaseTex (GetNormalSpecularTexPath (), makeCopy);
		}
		/// <summary>
		/// Gets the translucency gloss base texture.
		/// </summary>
		/// <returns>The translucency gloss texture.</returns>
		public static Texture2D GetTranslucencyTex (bool makeCopy = false) {
			return GetBaseTex (GetTranslucencyTexPath (), makeCopy);
		}
		/// <summary>
		/// Gets a extension base texture.
		/// </summary>
		/// <returns>The base tex.</returns>
		/// <param name="relativePath">Relative path.</param>
		/// <param name="makeCopy">If set to <c>true</c> make copy.</param>
		private static Texture2D GetBaseTex (string relativePath, bool makeCopy = false) {
			Texture2D destTexture = null;
			Texture2D baseTexture = null;
			#if UNITY_EDITOR
			baseTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D> (relativePath);
			#else
			string extension = System.IO.Path.GetExtension (relativePath);
			relativePath = relativePath.Substring(0, relativePath.Length - extension.Length);
			baseTexture = UnityEngine.Resources.Load<Texture2D> (relativePath);
			#endif
			if (baseTexture != null && makeCopy) {
				destTexture = TextureManager.GetInstance ().GetCopy (baseTexture);
				/*
				RenderTexture renderTexture= new RenderTexture (baseTexture.width, baseTexture.height, 1);
				RenderTexture.active = renderTexture;
				destTexture = new Texture2D (baseTexture.width, baseTexture.height);
				destTexture.ReadPixels(new Rect(0, 0, destTexture.width, render_texture.height), 0, 0);
				destination_texture.Apply();
				*/
			} else {
				destTexture = baseTexture;
			}
			return destTexture;
		}
		/// <summary>
		/// Gets a material using another material as base.
		/// The method tries to translate as many properties as possible from
		/// the original material to the tree creator material.
		/// </summary>
		/// <returns>A tree material.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="isSprout">If set to <c>true</c> is sprout.</param>
		public Material GetOverridedMaterial (int id, bool isSprout) {
			Material material = null;
			if (materials.ContainsKey (id)) {
				Material baseMaterial = materials [id];
				if (isSprout) {
					if (processedOwnedMaterials.ContainsKey (id)) {
						material = processedOwnedMaterials [id];
					}
					if (material == null) {
						if (renderPipelineType == RenderPipelineType.URP) {
							material = new Material (Shader.Find ("Nature/SpeedTree8"));
						} else {
							material = new Material (Shader.Find ("Hidden/Nature/Tree Creator Leaves Optimized"));
						}
						if (processedOwnedMaterials.ContainsKey (id)) {
							processedOwnedMaterials.Remove (id);
						}
						processedOwnedMaterials.Add (id, material);
					}
					if (baseMaterial.HasProperty ("_MainTex")) {
						material.SetTexture ("_MainTex", baseMaterial.GetTexture ("_MainTex"));
					}
					Texture2D normalTexture = TextureManager.GetInstance ().GetNormalTexture (baseMaterial);
					if (normalTexture == null) {
						//material.SetTexture ("_BumpSpecMap", MaterialManager.GetNormalSpecularTex ());
					} else {
						//material.SetTexture ("_BumpSpecMap", normalTexture);
					}
					if (baseMaterial.HasProperty ("_Color")) {
						material.SetColor ("_Color", baseMaterial.GetColor ("_Color"));
						material.SetColor ("_TranslucencyColor", baseMaterial.GetColor ("_Color"));
					}
					material.SetFloat ("_Cutoff", 0.75f);
					//material.SetFloat ("_TranslucencyViewDependency", sproutMappers [groupId].translucencyViewDependency);
					material.SetFloat ("_ShadowStrength", 0.5f);
					material.SetFloat ("_ShadowOffsetScale", 0.5f);
					material.SetTexture ("_ShadowTex", MaterialManager.GetShadowTex ());
					material.SetTexture ("_TranslucencyMap", MaterialManager.GetTranslucencyTex ());
					material.name = baseMaterial.name;
				} else {
					if (processedOwnedMaterials.ContainsKey (id)) {
						material = processedOwnedMaterials [id];
					} else {
						if (renderPipelineType == RenderPipelineType.URP) {
							material = new Material (Shader.Find ("Nature/SpeedTree8")); // TODO
						} else {
							material = new Material (Shader.Find ("Hidden/Nature/Tree Creator Bark Optimized"));
						}
						processedOwnedMaterials.Add (id, material);
					}
					if (baseMaterial.HasProperty ("_MainTex")) {
						material.SetTexture ("_MainTex", baseMaterial.GetTexture ("_MainTex"));
					}
					Texture2D normalTexture = TextureManager.GetInstance ().GetNormalTexture (baseMaterial);
					if (normalTexture == null) {
						material.SetTexture ("_BumpSpecMap", MaterialManager.GetNormalSpecularTex ());
					} else {
						material.SetTexture ("_BumpSpecMap", normalTexture);
					}
					if (baseMaterial.HasProperty ("_Color")) {
						material.SetColor ("_Color", baseMaterial.GetColor ("_Color"));
					}
					material.SetTexture ("_TranslucencyMap", MaterialManager.GetTranslucencyTex ());
					material.name = baseMaterial.name;
				}
			}
			return material;
		}
		/// <summary>
		/// Set properties for a leaves material using shader values.
		/// </summary>
		/// <param name="material">Material to set the properties to.</param>
		/// <param name="color">Tint color.</param>
		/// <param name="cutoff">Alpha cutoff value.</param>
		/// <param name="glossiness">Glossiness value.</param>
		/// <param name="metallic">Metallic value.</param>
		/// <param name="subsurface">Subsurface (light scattering) value.</param>
		/// <param name="subsurfaceColor">Subsurface color.</param>
		/// <param name="mainTex">Main texture.</param>
		/// <param name="normalsTex">Normals texture.</param>
		/// <param name="extrasTex">Extras textures.</param>
		/// <param name="subsurfaceTex">Subsurface textures.</param>
		/// <param name="diffusionProfile">Diffusion profile settings used on HDRP pipelines.</param>
		public static void SetLeavesMaterialProperties (
			Material material,
			Color color,
			float cutoff,
			float glossiness,
			float metallic,
			float subsurface,
			Color subsurfaceColor,
			Texture2D mainTex,
			Texture2D normalsTex,
			Texture2D extrasTex,
			Texture2D subsurfaceTex,
			ScriptableObject diffusionProfile = null
		) {
			material.SetTexture ("_MainTex", mainTex);
			material.SetColor ("_Color", color);
			if (leavesShaderType == LeavesShaderType.SpeedTree8OrSimilar) {
				// NORMAL
				if (normalsTex != null) {
					material.SetTexture ("_BumpMap", normalsTex);
					material.EnableKeyword ("EFFECT_BUMP");
					material.SetFloat ("_NormalMapKwToggle", 1f);
				}
				// EXTRAS
				if (extrasTex != null) {
					material.EnableKeyword ("EFFECT_EXTRA_TEX");
					material.SetFloat ("EFFECT_EXTRA_TEX", 1f);
					material.SetTexture ("_ExtraTex", extrasTex);
				} else {
					material.DisableKeyword ("EFFECT_EXTRA_TEX");
					material.SetFloat ("EFFECT_EXTRA_TEX", 0f);
				}
				// BACKSIDE NORMALS
				material.EnableKeyword ("EFFECT_BACKSIDE_NORMALS");
				// SUBSURFACE
				material.SetFloat ("_SubsurfaceKwToggle", 1f);
				material.SetColor ("_SubsurfaceColor", subsurfaceColor);
				material.SetFloat ("_SubsurfaceIndirect", subsurface);
				material.EnableKeyword ("EFFECT_SUBSURFACE");
				if (subsurfaceTex != null) {
					material.SetTexture ("_SubsurfaceTex", subsurfaceTex);
				} else {
					material.SetTexture ("_SubsurfaceTex", mainTex);
				}
				Color cutoffColor = color;
				cutoffColor.a = 1f - (cutoff / 2f);
				material.SetColor ("_Color", cutoffColor);
				material.SetColor ("_HueVariationColor", color);
				material.SetFloat ("_Glossiness", glossiness);
				material.SetFloat ("_Metallic", metallic);
				if (ExtensionManager.isHDRP) {
					float hash = 0;
					Vector4 guidVector = Vector4.zero;
					if (diffusionProfile != null) {
						hash = ExtensionManager.GetHashFromDiffusionProfile (diffusionProfile);
						guidVector = ExtensionManager.GetVector4FromScriptableObject (diffusionProfile);
					}
					material.SetFloat ("Diffusion_Profile", hash);
					material.SetVector ("Diffusion_Profile_Asset", guidVector);
					material.SetFloat ("_Surface", 0f);
					material.SetFloat ("_SurfaceType", 0f);
					material.SetFloat ("_AlphaCutoffEnable", 1f);
					material.SetFloat ("_AlphaClipThreshold", cutoff);
					material.SetFloat ("_OpaqueCullMode", 0f);
					material.SetFloat ("_OpaqueCullMode", 0f);
					material.SetFloat ("_CullMode", 0f);
					material.SetFloat ("_CullModeForward", 0f);
					material.EnableKeyword ("DEBUG_DISPLAY");
					material.EnableKeyword ("_DOUBLESIDED_ON");
					material.EnableKeyword ("_DISABLE_SSR_TRANSPARENT");
					material.EnableKeyword ("_ALPHATEST_ON");
				}
				material.SetFloat ("_TwoSided", 0f);
				material.SetFloat ("_WindQuality", 4f);
				material.EnableKeyword ("GEOM_TYPE_LEAF");
				material.doubleSidedGI = true;
				material.enableInstancing = true;
				//material.enableInstancing = false;
			} else if (leavesShaderType == LeavesShaderType.SpeedTree7OrSimilar) {
				/*
				_Color ("Main Color", Color) = (1,1,1,1)
				_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
				_HueVariation ("Hue Variation", Color) = (1.0,0.5,0.0,0.1)
				_DetailTex ("Detail", 2D) = "black" {}
				_BumpMap ("Normal Map", 2D) = "bump" {}
				_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.333
				[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Int) = 2
				[MaterialEnum(None,0,Fastest,1,Fast,2,Better,3,Best,4,Palm,5)] _WindQuality ("Wind Quality", Range(0,5)) = 0
				*/
				if (normalsTex != null) {
					material.SetTexture ("_BumpMap", normalsTex);
					material.EnableKeyword ("EFFECT_BUMP");
				}
				material.SetColor ("_HueVariation", color);
				material.SetFloat ("_Cutoff", cutoff);
				material.SetFloat ("_GeometryType", 2f);
				material.SetFloat ("_WindQuality", 4f);
				material.SetFloat ("_Cull", 0f);
				material.EnableKeyword ("GEOM_TYPE_LEAF");
				material.doubleSidedGI = true;
				material.enableInstancing = true;
			}
		}
		/// <summary>
		/// Set properties for a leaves material using a SpeedTree shader or similar from a SproutMap and a SproutArea instance.
		/// </summary>
		/// <param name="material">Material</param>
		/// <param name="sproutMap">SproutMap</param>
		/// <param name="sproutArea">SpoutArea</param>
		public void SetLeavesMaterialProperties (Material material, SproutMap sproutMap, SproutMap.SproutMapArea sproutArea) {
			SetLeavesMaterialProperties (material, sproutMap.color, sproutMap.alphaCutoff, 
				sproutMap.glossiness, sproutMap.metallic, sproutMap.subsurfaceValue, sproutMap.subsurfaceColor, 
				sproutArea.texture, sproutArea.normalMap, sproutArea.extraMap, sproutArea.subsurfaceMap, sproutMap.diffusionProfileSettings);
		}
		public static void OverrideLeavesMaterialProperties (Material material, SproutMap sproutMap, SproutMap.SproutMapArea sproutArea) {
			/* Available Shader Options
				_Color ("Main Color", Color) = (1,1,1,1)
				_Cutoff ("Alpha cutoff", Range(0,1)) = 0.3

				_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
				_BumpSpecMap ("Normalmap (GA) Spec (R) Shadow Offset (B)", 2D) = "bump" {}
			*/
			if (material.HasProperty ("_MainTex")) {
				material.SetTexture ("_MainTex", sproutArea.texture);
			}
			if (material.HasProperty ("_BumpMap")) {
				if (sproutArea.normalMap != null) {
					material.SetTexture ("_BumpMap", sproutArea.normalMap);
				} else {
					material.SetTexture ("_BumpMap", MaterialManager.GetNormalSpecularTex ());
				}
			}
			if (material.HasProperty ("_Color")) {
				material.SetColor ("_Color", sproutMap.color);
			}
			if (material.HasProperty ("_Cutoff")) {
				material.SetFloat ("_Cutoff", sproutMap.alphaCutoff);
			}
		}
		/// <summary>
		/// Gets a leaves material.
		/// </summary>
		/// <returns>Leaves material.</returns>
		public static Material GetLeavesMaterial () {
			Material m = new Material (leavesShader);
			return m;
		}
		/// <summary>
		/// Get a leaves material for previewing.
		/// </summary>
		/// <param name="sproutMap">SproutMap.</param>
		/// <param name="sproutArea">SproutArea.</param>
		/// <returns></returns>
		public static Material GetPreviewLeavesMaterial (SproutMap sproutMap, SproutMap.SproutMapArea sproutArea) {
			Material material = null;
			if (sproutMap.mode == SproutMap.Mode.Texture && sproutArea != null) {
				material = new Material (Shader.Find ("Hidden/Nature/Tree Creator Leaves Optimized"));
				TreeFactory.GetActiveInstance ().materialManager.SetLeavesMaterialProperties (material, sproutMap, sproutArea);
			} else if (sproutMap.customMaterial != null) {
				material = Object.Instantiate (sproutMap.customMaterial);
				if (sproutMap.mode == SproutMap.Mode.MaterialOverride && sproutArea != null) {
					MaterialManager.OverrideLeavesMaterialProperties (material, sproutMap, sproutArea);
				}
			}
			return material;
		}
		#endregion
	}
}