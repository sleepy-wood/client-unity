using System.Collections.Generic;

using UnityEngine;

using Broccoli.Pipe;
using Broccoli.Builder;
using Broccoli.Manager;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Sprout mapper component.
	/// Set materials and UV mapping for sprout elements.
	/// </summary>
	public class SproutMapperComponent : TreeFactoryComponent {
		#region Vars
		/// <summary>
		/// The sprout mapper element.
		/// </summary>
		SproutMapperElement sproutMapperElement = null;
		/// <summary>
		/// The sprout meshes.
		/// </summary>
		Dictionary<int, SproutMesh> sproutMeshes = new Dictionary <int, SproutMesh> ();
		/// <summary>
		/// The sprout mappers.
		/// </summary>
		Dictionary<int, SproutMap> sproutMappers = new Dictionary <int, SproutMap> ();
		/// <summary>
		/// Component command.
		/// </summary>
		public enum ComponentCommand
		{
			BuildMaterials,
			SetUVs
		}
		#endregion

		#region Configuration
		/// <summary>
		/// Prepares the parameters to process with this component.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="processControl">Process control.</param>
		protected override void PrepareParams (TreeFactory treeFactory,
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null) 
		{
			sproutMapperElement.PrepareSeed ();
			// Gather all SproutGroup objects from elements upstream.
			List<PipelineElement> sproutMeshGenerators = 
				sproutMapperElement.GetUpstreamElements(PipelineElement.ClassType.SproutMeshGenerator);
			sproutMeshes.Clear ();

			for (int i = 0; i < sproutMeshGenerators.Count; i++) {
				SproutMeshGeneratorElement sproutMeshGeneratorElement = (SproutMeshGeneratorElement)sproutMeshGenerators[i];
				for (int j = 0; j < sproutMeshGeneratorElement.sproutMeshes.Count; j++) {
					if (sproutMeshGeneratorElement.sproutMeshes[j].groupId > 0) {
						sproutMeshes.Add (sproutMeshGeneratorElement.sproutMeshes[j].groupId, 
							sproutMeshGeneratorElement.sproutMeshes[j]);
					}
				}
			}
			sproutMeshGenerators.Clear ();

			sproutMappers.Clear ();
			for (int i = 0; i < sproutMapperElement.sproutMaps.Count; i++) {
				if (!sproutMappers.ContainsKey (sproutMapperElement.sproutMaps[i].groupId)) {
					sproutMappers.Add (sproutMapperElement.sproutMaps[i].groupId, 
						sproutMapperElement.sproutMaps[i]);
				}
			}
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.Material;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
			sproutMapperElement = null;
			sproutMeshes.Clear ();
			sproutMappers.Clear ();
		}
		#endregion

		#region Processing
		/// <summary>
		/// Process the tree according to the pipeline element.
		/// </summary>
		/// <param name="treeFactory">Parent tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="ProcessControl">Process control.</param>
		public override bool Process (TreeFactory treeFactory, 
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl ProcessControl = null) {
			if (pipelineElement != null && tree != null) {
				sproutMapperElement = pipelineElement as SproutMapperElement;
				PrepareParams (treeFactory, useCache, useLocalCache);
				BuildMaterials (treeFactory);
				AssignUVs (treeFactory);
				for (int i = 0; i < sproutMapperElement.sproutMaps.Count; i++) {
					if (sproutMapperElement.sproutMaps [i].colorVarianceMode == SproutMap.ColorVarianceMode.Shades) {
						AssignShadeVariance (treeFactory, sproutMapperElement.sproutMaps [i]);
					}	
				}
				return true;
			}
			return false;
		}
		/// <summary>
		/// Removes the product of this component on the factory processing.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		public override void Unprocess (TreeFactory treeFactory) {
			treeFactory.meshManager.DeregisterMeshByType (MeshManager.MeshData.Type.Sprout);
			treeFactory.materialManager.DeregisterMaterialByType (MeshManager.MeshData.Type.Sprout);
		}
		/// <summary>
		/// Process a special command or subprocess on this component.
		/// </summary>
		/// <param name="cmd">Cmd.</param>
		/// <param name="treeFactory">Tree factory.</param>
		public override void ProcessComponentOnly (int cmd, TreeFactory treeFactory) {
			if (pipelineElement != null && tree != null) {
				if (cmd == (int)ComponentCommand.BuildMaterials) {
					BuildMaterials (treeFactory, true);
				} else {
					AssignUVs (treeFactory, true);
					for (int i = 0; i < sproutMapperElement.sproutMaps.Count; i++) {
						if (sproutMapperElement.sproutMaps [i].colorVarianceMode == SproutMap.ColorVarianceMode.Shades) {
							AssignShadeVariance (treeFactory, sproutMapperElement.sproutMaps [i]);
						}	
					}
				}
			}
		}
		#endregion

		#region Prefab Processing
		/// <summary>
		/// Processes called only on the prefab creation.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		public override void OnProcessPrefab (TreeFactory treeFactory) {
			#if UNITY_EDITOR
			var sproutMeshesEnumerator = sproutMeshes.GetEnumerator ();
			int groupId;
			int meshId;
			while (sproutMeshesEnumerator.MoveNext ()) {
				groupId = sproutMeshesEnumerator.Current.Key;
				SproutGroups.SproutGroup sproutGroup = treeFactory.localPipeline.sproutGroups.GetSproutGroup (groupId);				
				if (sproutMappers.ContainsKey (groupId)) {
					// IF material comes from the Branch Collection.
					if (sproutGroup.branchCollection != null) {
						Material material;
						meshId = MeshManager.MeshData.GetMeshDataId (MeshManager.MeshData.Type.Sprout, groupId);
						if (treeFactory.meshManager.HasMeshAndNotEmpty (meshId)) {
							// Use a clone of the original material if specified by the preferences.
							material = treeFactory.materialManager.GetMaterial (MeshManager.MeshData.Type.Sprout, true, groupId);
							material.name = "Sprout Material " + groupId;
							if (material != null) {
								treeFactory.assetManager.AddMaterialToPrefab (
									material,
									treeFactory.meshManager.GetMergedMeshIndex (MeshManager.MeshData.Type.Sprout, groupId), 
									groupId);
							}
						}
					}
					// If is a generted or custom material.
					else {
						// If texture mode.
						if (sproutMappers[groupId].IsTextured ()) {
							// For each area.
							SproutMap.SproutMapArea sproutArea;
							for (int i = 0; i < sproutMappers[groupId].sproutAreas.Count; i++) {
								sproutArea = sproutMappers[groupId].sproutAreas[i];
								meshId = MeshManager.MeshData.GetMeshDataId (MeshManager.MeshData.Type.Sprout, groupId, i);
								if (treeFactory.meshManager.HasMeshAndNotEmpty (meshId)) {
									// Register as a native material CLONING
									if (treeFactory.treeFactoryPreferences.prefabCloneCustomMaterialEnabled) {
										Material material = treeFactory.materialManager.GetMaterial (MeshManager.MeshData.Type.Sprout, true, groupId, i);
										treeFactory.assetManager.AddMaterialParams (
											new AssetManager.MaterialParams (AssetManager.MaterialParams.ShaderType.Native,
												treeFactory.treeFactoryPreferences.prefabCreateAtlas, true),
											treeFactory.meshManager.GetMergedMeshIndex (MeshManager.MeshData.Type.Sprout, groupId, i));
										// Add the material to the asset manager.
										if (material != null) {
											material.name = "Optimized Sprout Material " + groupId + "." + i;
											treeFactory.assetManager.AddMaterialToPrefab (
												material,
												treeFactory.meshManager.GetMergedMeshIndex (MeshManager.MeshData.Type.Sprout, groupId, i), 
												groupId,
												sproutArea);
										}
									} else {
										Material material = treeFactory.materialManager.GetMaterial (meshId, false);
										treeFactory.assetManager.AddMaterialParams (
											new AssetManager.MaterialParams (AssetManager.MaterialParams.ShaderType.Native,
												treeFactory.treeFactoryPreferences.prefabCreateAtlas, true),
											treeFactory.meshManager.GetMergedMeshIndex (MeshManager.MeshData.Type.Sprout, groupId, i));
										if (material != null) {
											treeFactory.assetManager.AddMaterialToPrefab (
												material,
												treeFactory.meshManager.GetMergedMeshIndex (MeshManager.MeshData.Type.Sprout, groupId, i), 
												groupId,
												sproutArea);
										}
									}
								}
							}
						} else {
							// If custom material mode.
							Material material;
							meshId = MeshManager.MeshData.GetMeshDataId (MeshManager.MeshData.Type.Sprout, groupId);
							if (treeFactory.meshManager.HasMeshAndNotEmpty (meshId)) {
								if (treeFactory.treeFactoryPreferences.overrideMaterialShaderEnabled) {
									// Create material based on the custom one.
									material = treeFactory.materialManager.GetOverridedMaterial (meshId, true);
									// Register as a native material (using the tree creator shader).
									treeFactory.assetManager.AddMaterialParams (
										new AssetManager.MaterialParams (AssetManager.MaterialParams.ShaderType.Native,
											treeFactory.treeFactoryPreferences.prefabCreateAtlas, false),
										treeFactory.meshManager.GetMergedMeshIndex (MeshManager.MeshData.Type.Sprout, groupId));
									material.name = "Optimized Sprout Material " + groupId;
								} else {
									// Use a clone of the original material if specified by the preferences.
									if (treeFactory.treeFactoryPreferences.prefabCloneCustomMaterialEnabled) {
										material = treeFactory.materialManager.GetMaterial (MeshManager.MeshData.Type.Sprout, true, groupId);
										material.name = "Sprout Material " + groupId;
									} else {
										material = treeFactory.materialManager.GetMaterial (MeshManager.MeshData.Type.Sprout, false, groupId);
									}
								}
								if (material != null) {
									treeFactory.assetManager.AddMaterialToPrefab (
										material,
										treeFactory.meshManager.GetMergedMeshIndex (MeshManager.MeshData.Type.Sprout, groupId), 
										groupId);
								}
							}
						}
					}
				}
			}
			#endif
		}
		/// <summary>
		/// Gets the process prefab weight.
		/// </summary>
		/// <returns>The process prefab weight.</returns>
		/// <param name="treeFactory">Tree factory.</param>
		public override int GetProcessPrefabWeight (TreeFactory treeFactory) {
			// TODO: weith for atlas should go on the asset manager.
			int totalWeight = 0;
			if (treeFactory.treeFactoryPreferences.prefabCreateAtlas) {
				var sproutMeshesEnumerator = sproutMeshes.GetEnumerator ();
				int groupId;
				while (sproutMeshesEnumerator.MoveNext ()) {
					groupId = sproutMeshesEnumerator.Current.Key;
					switch (sproutMeshes [groupId].shapeMode) {
					//case SproutMesh.Mode.Billboard:
					case SproutMesh.ShapeMode.Plane:
					case SproutMesh.ShapeMode.Cross:
					case SproutMesh.ShapeMode.Tricross:
					case SproutMesh.ShapeMode.GridPlane:
						if (sproutMappers.ContainsKey (groupId)) {
							if (sproutMappers [groupId].IsTextured ()) {
								for (int i = 0; i < sproutMappers[groupId].sproutAreas.Count; i++) {
									if (sproutMappers[groupId].sproutAreas[i].texture != null && 
										sproutMappers[groupId].sproutAreas[i].enabled) {
										totalWeight += 10;
									}
								}
							} else {
								totalWeight += 10;
							}
						}
						break;
					case SproutMesh.ShapeMode.Mesh:
						totalWeight += 30;
						break;
					}
				}
				if (totalWeight > 0) {
					// Weight for creating atlas.
					totalWeight += 40;
				}
			}
			return totalWeight;
		}
		#endregion

		#region Materials
		/// <summary>
		/// Builds the materials for the sprouts.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		private void BuildMaterials (TreeFactory treeFactory, bool updatePreviewTree = false) {
			var sproutMeshesEnumerator = sproutMeshes.GetEnumerator ();
			int groupId;
			SproutMap sproutMap;
			while (sproutMeshesEnumerator.MoveNext ()) {
				groupId = sproutMeshesEnumerator.Current.Key;
				SproutGroups.SproutGroup sproutGroup = pipelineElement.pipeline.sproutGroups.GetSproutGroup (groupId);				
				if (sproutMappers.ContainsKey (groupId)) {
					sproutMap = sproutMappers [groupId];
					// IF material comes from the Branch Collection.
					if (sproutGroup.branchCollection != null) {
						BranchDescriptorCollection branchDescriptorCollection = 
							((BranchDescriptorCollectionSO)sproutGroup.branchCollection).branchDescriptorCollection;
						Material m = SproutCompositeManager.GenerateMaterial (sproutMap.color, sproutMap.alphaCutoff,
							sproutMap.glossiness, sproutMap.metallic, sproutMap.subsurfaceValue, sproutMap.subsurfaceColor,
							branchDescriptorCollection.atlasAlbedoTexture, branchDescriptorCollection.atlasNormalsTexture,
							branchDescriptorCollection.atlasExtrasTexture, branchDescriptorCollection.atlasSubsurfaceTexture);
						treeFactory.materialManager.RegisterCustomMaterial (MeshManager.MeshData.Type.Sprout,
									m, groupId, 0);
					}
					// ELSE generate the materials.
					else {
						if (sproutMap.mode == SproutMap.Mode.Texture) {
							SproutMap.SproutMapArea sproutArea;
							for (int i = 0; i < sproutMappers[groupId].sproutAreas.Count; i++) {
								sproutArea = sproutMap.sproutAreas [i];
								int meshId = MeshManager.MeshData.GetMeshDataId (MeshManager.MeshData.Type.Sprout, groupId, i);

								/// Get an existing material from the material manager or create a new one.
								Material material;
								//TODO: why the else? shouldn't the manager take care of returning a new material if none has been created?
								if (treeFactory.materialManager.HasMaterial (meshId) && 
									!treeFactory.materialManager.IsCustomMaterial (meshId) &&
									treeFactory.materialManager.GetMaterial (meshId) != null) {
									material = treeFactory.materialManager.GetMaterial (meshId);
								} else {
									material = treeFactory.materialManager.GetOwnedMaterial (meshId, treeFactory.materialManager.GetLeavesShader ().name);
								}
								treeFactory.materialManager.SetLeavesMaterialProperties (material, sproutMap, sproutArea);
								material.name = "Sprout " + meshId;
							}
						} else if (sproutMap.IsMaterialMode() &&
							sproutMap.customMaterial != null) {
							if (sproutMap.mode == SproutMap.Mode.MaterialOverride) {
								// Material Override Mode Cloning the Material
								if (treeFactory.treeFactoryPreferences.prefabCloneCustomMaterialEnabled) {
									SproutMap.SproutMapArea sproutArea;
									for (int i = 0; i < sproutMappers[groupId].sproutAreas.Count; i++) {
										sproutArea = sproutMap.sproutAreas [i];
										int meshId = MeshManager.MeshData.GetMeshDataId (MeshManager.MeshData.Type.Sprout, groupId, i);

										/// Get a cloned the material.
										Material material;
										if (treeFactory.treeFactoryPreferences.overrideMaterialShaderEnabled) {
											material = treeFactory.materialManager.GetOwnedMaterial (meshId, treeFactory.materialManager.GetLeavesShader ().name);
										} else {
											material = treeFactory.materialManager.GetOwnedMaterial (meshId, 
												sproutMap.customMaterial);
										}
										MaterialManager.OverrideLeavesMaterialProperties (material, sproutMap, sproutArea);
										material.name = "Sprout " + meshId;
									}
								} else {
									// Material Override NOT Cloning the Material
									SproutMap.SproutMapArea sproutArea;
									for (int i = 0; i < sproutMappers[groupId].sproutAreas.Count; i++) {
										sproutArea = sproutMap.sproutAreas [i];
										int meshId = MeshManager.MeshData.GetMeshDataId (MeshManager.MeshData.Type.Sprout, groupId, i);
										treeFactory.materialManager.RegisterCustomMaterial (meshId, sproutMap.customMaterial);
									}
								}
							} else {
								// Material Mode.
								Material customMaterial = sproutMap.customMaterial;
								treeFactory.materialManager.RegisterCustomMaterial (MeshManager.MeshData.Type.Sprout,
									customMaterial, groupId, 0);
							}
						} else {
							treeFactory.materialManager.DeregisterMaterial (MeshManager.MeshData.Type.Sprout, groupId);
						}
					}



				}
				if (updatePreviewTree) {
					MeshRenderer renderer = tree.obj.GetComponent<MeshRenderer> ();
					Material[] materials = renderer.sharedMaterials;
					for (int j = 0; j < treeFactory.meshManager.GetMeshesCount (); j++) {
						int meshId = treeFactory.meshManager.GetMergedMeshId (j);
						if (treeFactory.materialManager.HasMaterial (meshId)) {
							if (treeFactory.materialManager.IsCustomMaterial (meshId) &&
							    treeFactory.treeFactoryPreferences.overrideMaterialShaderEnabled) {
								bool isSprout = treeFactory.meshManager.IsSproutMesh (meshId);
								materials [j] = treeFactory.materialManager.GetOverridedMaterial (meshId, isSprout);
							} else {
								materials [j] = treeFactory.materialManager.GetMaterial (meshId);
							}
						} else if (materials [j] != null) {
							materials [j] = null;
						}
					}
					renderer.sharedMaterials = materials;
				}
			}
		}
		#endregion

		#region UVs and Colors
		/// <summary>
		/// Assigns the UVs to the sprout meshes.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		private void AssignUVs (TreeFactory treeFactory, bool updatePreviewTree = false) {
			var sproutMeshesEnumerator = sproutMeshes.GetEnumerator ();
			int groupId;
			while (sproutMeshesEnumerator.MoveNext ()) {
				groupId = sproutMeshesEnumerator.Current.Key;
				if (sproutMappers.ContainsKey (groupId) && sproutMeshesEnumerator.Current.Value.branchCollection == null) {
					if (sproutMappers [groupId].IsTextured ()) {
						List<Vector4> originalUVs = new List<Vector4> ();
						if (updatePreviewTree) {
							MeshFilter meshFilter = tree.obj.GetComponent<MeshFilter> ();
							meshFilter.sharedMesh.GetUVs (0, originalUVs);
						}
						SproutMap.SproutMapArea sproutArea;
						for (int i = 0; i < sproutMappers[groupId].sproutAreas.Count; i++) {
							sproutArea = sproutMappers [groupId].sproutAreas [i];
							int meshId = MeshManager.MeshData.GetMeshDataId (MeshManager.MeshData.Type.Sprout, groupId, i);
							List<Vector4> uvs = new List<Vector4> ();
							Mesh mesh = treeFactory.meshManager.GetMesh (meshId);
							mesh = treeFactory.meshManager.GetMesh (meshId);
							if (mesh != null) {
								mesh.GetUVs (0, uvs);
								SproutMeshMetaBuilder.GetInstance ().GetCropUVs (ref uvs, 
									sproutArea.x, 
									sproutArea.y, 
									sproutArea.width, 
									sproutArea.height, 
									sproutArea.normalizedStep);
								mesh.SetUVs (0, uvs);
							}
							if (updatePreviewTree) {
								int vertexOffset = treeFactory.meshManager.GetMergedMeshVertexOffset (meshId);
								for (int j = 0; j < uvs.Count; j++) {
									originalUVs [vertexOffset + j] = uvs [j];
								}
							}
						}
						if (updatePreviewTree) {
							MeshFilter meshFilter = tree.obj.GetComponent<MeshFilter> ();
							meshFilter.sharedMesh.SetUVs (0, originalUVs);
						}
					}
				}
			}
		}
		private void AssignShadeVariance (TreeFactory treeFactory, SproutMap sproutMap) {
			Dictionary<int, MeshManager.MeshData> meshDatas = 
				treeFactory.meshManager.GetMeshesDataOfType (MeshManager.MeshData.Type.Sprout, sproutMap.groupId);
			var meshDatasEnumerator = meshDatas.GetEnumerator ();
			int sproutMeshId;
			int index;
			Mesh mesh;
			Color meshColor = Color.green;
			while (meshDatasEnumerator.MoveNext ()) {
				sproutMeshId = meshDatasEnumerator.Current.Key;
				mesh = treeFactory.meshManager.GetMesh (sproutMeshId);
				if (treeFactory.meshManager.GetMesh (sproutMeshId) != null && treeFactory.meshManager.HasMeshParts (sproutMeshId)) {
					List<Color> localColors = new List<Color> ();
					mesh.GetColors (localColors);
					List<MeshManager.MeshPart> meshParts = treeFactory.meshManager.GetMeshParts (sproutMeshId);
					for (int i = 0; i < meshParts.Count; i++) {
						meshColor = Random.ColorHSV (sproutMap.minColorShade, sproutMap.maxColorShade,
							1f, 1f, 1f, 1f);
						// Color tint value is applied based on the color blue channel.
						// Green channel value is 0 on sprout vertex.
						if (sproutMap.colorTintEnabled) {
							meshColor.b = Random.Range (sproutMap.minColorTint, sproutMap.maxColorTint);
							meshColor.g = 0;
						}
						MeshManager.MeshPart meshPart = meshParts [i];
						for (int j = 0; j < meshPart.length; j++) {
							index = j + meshPart.startIndex;
							localColors [index] = meshColor;
						}
					}
					mesh.SetColors (localColors);
				}
			}
		}
		#endregion
	}
}