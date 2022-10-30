using System.Collections.Generic;

using UnityEngine;

using Broccoli.Base;
using Broccoli.Model;
using Broccoli.Pipe;
using Broccoli.Builder;
using Broccoli.Manager;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Sprout mesh generator component.
	/// </summary>
	public class SproutMeshGeneratorComponent : TreeFactoryComponent {
		#region Vars
		/// <summary>
		/// The sprout mesh builder.
		/// </summary>
		SproutMeshBuilder sproutMeshBuilder = null;
		/// <summary>
		/// Advanced sprout mesh builder.
		/// </summary>
		AdvancedSproutMeshBuilder advancedSproutMeshBuilder = null;
		/// <summary>
		/// The sprout mesh generator element.
		/// </summary>
		SproutMeshGeneratorElement sproutMeshGeneratorElement = null;
		/// <summary>
		/// The sprout meshes relationship between their group id and the assigned sprout mesh.
		/// </summary>
		Dictionary<int, SproutMesh> sproutMeshes = new Dictionary <int, SproutMesh> ();
		/// <summary>
		/// The sprout mappers.
		/// </summary>
		Dictionary<int, SproutMap> sproutMappers = new Dictionary <int, SproutMap> ();
		/// <summary>
		/// Flag to reduce the complexity of sprouts for LOD purposes.
		/// </summary>
		bool simplifySprouts = false;
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
			sproutMeshBuilder = SproutMeshBuilder.GetInstance ();
			advancedSproutMeshBuilder = AdvancedSproutMeshBuilder.GetInstance ();

			// Gather all SproutMap objects from elements downstream.
			PipelineElement pipelineElement = 
				sproutMeshGeneratorElement.GetDownstreamElement (PipelineElement.ClassType.SproutMapper);
			sproutMappers.Clear ();
			if (pipelineElement != null && pipelineElement.isActive) {
				SproutMapperElement sproutMapperElement = (SproutMapperElement)pipelineElement;
				for (int i = 0; i < sproutMapperElement.sproutMaps.Count; i++) {
					if (sproutMapperElement.sproutMaps[i].groupId > 0) {
						sproutMappers.Add (sproutMapperElement.sproutMaps[i].groupId, sproutMapperElement.sproutMaps[i]);
					}
				}
			}

			// Gather all SproutMesh objects from element.
			sproutMeshes.Clear ();
			for (int i = 0; i < sproutMeshGeneratorElement.sproutMeshes.Count; i++) {
				sproutMeshes.Add (sproutMeshGeneratorElement.sproutMeshes[i].groupId, sproutMeshGeneratorElement.sproutMeshes[i]);
			}

			sproutMeshBuilder.globalScale = treeFactory.treeFactoryPreferences.factoryScale;
			sproutMeshBuilder.SetGravity (GlobalSettings.gravityDirection);
			sproutMeshBuilder.mapST = true;

			advancedSproutMeshBuilder.globalScale = treeFactory.treeFactoryPreferences.factoryScale;
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.StructureGirth; // TODO
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
			if (sproutMeshBuilder != null)
				sproutMeshBuilder.Clear ();
			sproutMeshBuilder = null;
			if (advancedSproutMeshBuilder != null)
				advancedSproutMeshBuilder.Clear ();
			advancedSproutMeshBuilder = null;
			sproutMeshGeneratorElement = null;
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
		/// <param name="processControl">Process control.</param>
		public override bool Process (TreeFactory treeFactory, 
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null) {
			if (pipelineElement != null && tree != null) {
				sproutMeshGeneratorElement = pipelineElement as SproutMeshGeneratorElement;
				PrepareParams (treeFactory, useCache, useLocalCache, processControl);
				BuildMesh (treeFactory, processControl.lodIndex);
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
		}
		/// <summary>
		/// Builds the mesh or meshes for the sprouts.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="lodIndex">Index for the LOD definition.</param>
		private void BuildMesh (TreeFactory treeFactory, int lodIndex) {
			var sproutMeshesEnumerator = sproutMeshes.GetEnumerator ();
			sproutMeshBuilder.PrepareBuilder (sproutMeshes, sproutMappers);
			SproutMesh sproutMesh;
			sproutMeshGeneratorElement.PrepareSeed ();
			while (sproutMeshesEnumerator.MoveNext ()) {
				sproutMesh = sproutMeshesEnumerator.Current.Value;
				if (sproutMesh.meshingMode == SproutMesh.MeshingMode.Shape) {
					BuildShapeMesh (treeFactory, lodIndex, sproutMesh);
				} else if (sproutMesh.meshingMode == SproutMesh.MeshingMode.BranchCollection) {
					BuildBranchCollectionMesh (treeFactory, lodIndex, sproutMesh);
				}
			}
		}
		#endregion

		#region Process Shape Mesh
		/// <summary>
		/// Builds the mesh or meshes for the sprouts.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="lodIndex">Index for the LOD definition.</param>
		private void BuildShapeMesh (TreeFactory treeFactory, int lodIndex, SproutMesh sproutMesh) {
			int groupId = sproutMesh.groupId;
			bool isTwoSided = treeFactory.materialManager.IsSproutTwoSided ();
			if (pipelineElement.pipeline.sproutGroups.HasSproutGroup (groupId)) {
				if (sproutMappers.ContainsKey (groupId) && sproutMeshes[groupId].shapeMode != SproutMesh.ShapeMode.Mesh) {
					if (sproutMappers [groupId].IsTextured ()) {
						sproutMeshBuilder.AssignSproutSubgroups (tree, groupId, sproutMappers [groupId]);
						List<SproutMap.SproutMapArea> sproutAreas = sproutMappers [groupId].sproutAreas;
						for (int i = 0; i < sproutAreas.Count; i++) {
							if (sproutAreas[i].enabled) {
								Mesh groupMesh = sproutMeshBuilder.MeshSprouts (tree, 
														groupId, TranslateSproutMesh (sproutMeshes [groupId]), sproutAreas[i], i, isTwoSided);
								ApplyNormalMode (groupMesh, Vector3.zero);
								treeFactory.meshManager.DeregisterMesh (MeshManager.MeshData.Type.Sprout, groupId, i);
								treeFactory.meshManager.RegisterSproutMesh (groupMesh, groupId, i);
								List<SproutMeshBuilder.SproutMeshData> sproutMeshDatas = sproutMeshBuilder.sproutMeshData;
								for (int j = 0; j < sproutMeshDatas.Count; j++) {
									MeshManager.MeshPart meshPart = treeFactory.meshManager.AddMeshPart (sproutMeshDatas[j].startIndex, 
																		sproutMeshDatas[j].length,
																		sproutMeshDatas[j].position, 
																		0, 
																		sproutMeshDatas[j].origin,
																		MeshManager.MeshData.Type.Sprout,
																		groupId,
																		i);
									meshPart.sproutId = sproutMeshDatas[j].sproutId;
									meshPart.branchId = sproutMeshDatas[j].branchId;
								}
							} else {
								treeFactory.meshManager.DeregisterMesh (MeshManager.MeshData.Type.Sprout, groupId, i);
							}
						}
					} else {
						Mesh groupMesh = sproutMeshBuilder.MeshSprouts (tree, groupId, TranslateSproutMesh (sproutMeshes [groupId]));
						ApplyNormalMode (groupMesh, Vector3.zero);
						treeFactory.meshManager.DeregisterMesh (MeshManager.MeshData.Type.Sprout, groupId);
						treeFactory.meshManager.RegisterSproutMesh (groupMesh, groupId);
						List<SproutMeshBuilder.SproutMeshData> sproutMeshDatas = sproutMeshBuilder.sproutMeshData;
						for (int i = 0; i < sproutMeshDatas.Count; i++) {
							MeshManager.MeshPart meshPart = treeFactory.meshManager.AddMeshPart (sproutMeshDatas[i].startIndex, 
								sproutMeshDatas[i].length,
								sproutMeshDatas[i].position,
								0, 
								sproutMeshDatas[i].origin,
								MeshManager.MeshData.Type.Sprout,
								groupId);
							meshPart.sproutId = sproutMeshDatas[i].sproutId;
							meshPart.branchId = sproutMeshDatas[i].branchId;
						}
					}
				} else {
					// Process without sprout areas.
					Mesh groupMesh = sproutMeshBuilder.MeshSprouts (tree, 
						groupId, sproutMeshes [groupId]);
					ApplyNormalMode (groupMesh, Vector3.zero);
					treeFactory.meshManager.DeregisterMesh (MeshManager.MeshData.Type.Sprout, groupId);
					treeFactory.meshManager.RegisterSproutMesh (groupMesh, groupId);
					List<SproutMeshBuilder.SproutMeshData> sproutMeshDatas = sproutMeshBuilder.sproutMeshData;
					for (int i = 0; i < sproutMeshDatas.Count; i++) {
						MeshManager.MeshPart meshPart = treeFactory.meshManager.AddMeshPart (sproutMeshDatas[i].startIndex,
							sproutMeshDatas[i].length,
							sproutMeshDatas[i].position,
							0,
							sproutMeshDatas[i].origin,
							MeshManager.MeshData.Type.Sprout,
							groupId);
						meshPart.branchId = sproutMeshDatas[i].branchId;
						meshPart.sproutId = sproutMeshDatas[i].sproutId;
					}
				}
			}
		}
		/// <summary>
		/// Reprocess normals for the sprout mesh.
		/// </summary>
		/// <param name="targetMesh">Target sprout mesh.</param>
		/// <param name="offset">Vector3 offset from the normal reference point (depending on the normal mode applied).</param>
		void ApplyNormalMode (Mesh targetMesh, Vector3 offset) {
			if (sproutMeshGeneratorElement.normalMode == SproutMeshGeneratorElement.NormalMode.PerSprout) return;
			Vector3 referenceCenter = targetMesh.bounds.center;
			if (sproutMeshGeneratorElement.normalMode == SproutMeshGeneratorElement.NormalMode.TreeOrigin) {
				referenceCenter.y = 0;
			} else if (sproutMeshGeneratorElement.normalMode == SproutMeshGeneratorElement.NormalMode.SproutsBase) {
				referenceCenter.y -= targetMesh.bounds.size.y / 2f;
			}
			List<Vector3> normals = new List<Vector3> ();
			List<Vector3> vertices = new List<Vector3> ();
			targetMesh.GetNormals (normals);
			targetMesh.GetVertices (vertices);
			for (int i = 0; i < normals.Count; i++) {
				normals [i] = Vector3.Lerp (normals[i], (vertices[i] - referenceCenter + offset).normalized, sproutMeshGeneratorElement.normalModeStrength);
			}
			targetMesh.SetNormals (normals);
		}
		/// <summary>
		/// Simplifies sprout mesh parameters for LOD purposes.
		/// </summary>
		/// <param name="sproutMesh">SproutMesh to evaluate.</param>
		/// <returns>Translated SproutMesh.</returns>
		SproutMesh TranslateSproutMesh (SproutMesh sproutMesh) {
			if (simplifySprouts) {
				if (sproutMesh.shapeMode == SproutMesh.ShapeMode.GridPlane) {
					SproutMesh simplyfiedSproutMesh = sproutMesh.Clone ();
					if (sproutMesh.resolutionHeight > sproutMesh.resolutionWidth) {
						simplyfiedSproutMesh.resolutionWidth = 1;
						simplyfiedSproutMesh.resolutionHeight = 
						(int) Mathf.Clamp ( (float) simplyfiedSproutMesh.resolutionHeight / 2f,
							2.0f, 
							(float) simplyfiedSproutMesh.resolutionHeight);
					} else if (sproutMesh.resolutionWidth > sproutMesh.resolutionHeight) {
						simplyfiedSproutMesh.resolutionHeight = 1;
						simplyfiedSproutMesh.resolutionWidth = 
						(int) Mathf.Clamp ( (float) simplyfiedSproutMesh.resolutionWidth / 2f,
							2.0f, 
							(float) simplyfiedSproutMesh.resolutionWidth);
					} else {
						simplyfiedSproutMesh.resolutionHeight = 
						(int) Mathf.Clamp ( (float) simplyfiedSproutMesh.resolutionHeight / 2f,
							2.0f, 
							(float) simplyfiedSproutMesh.resolutionHeight);
						simplyfiedSproutMesh.resolutionWidth = 
						(int) Mathf.Clamp ( (float) simplyfiedSproutMesh.resolutionWidth / 2f,
							2.0f, 
							(float) simplyfiedSproutMesh.resolutionWidth);
					}
					return simplyfiedSproutMesh;
				} else if (sproutMesh.shapeMode == SproutMesh.ShapeMode.PlaneX) {
					
				}
			}
			return sproutMesh;
		}
		#endregion

		#region Process Branch Collection Mesh
		/// <summary>
		/// Builds the mesh or meshes for the sprouts.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="lodIndex">Index for the LOD definition.</param>
		private void BuildBranchCollectionMesh (TreeFactory treeFactory, int lodIndex, SproutMesh sproutMesh) {
			int groupId = sproutMesh.groupId;
			bool isTwoSided = treeFactory.materialManager.IsSproutTwoSided ();
			if (pipelineElement.pipeline.sproutGroups.HasSproutGroup (groupId) && sproutMesh.branchCollection != null) {
				// Get the branch collection.
				BranchDescriptorCollection branchCollection = ((BranchDescriptorCollectionSO)sproutMesh.branchCollection).branchDescriptorCollection;

				// Assign the sprout subgroups.
				sproutMeshBuilder.AssignSproutSubgroups (tree, groupId, branchCollection, sproutMesh);

				// Register the branch collection.
				RegisterBranchCollection (treeFactory, lodIndex, sproutMesh, branchCollection);

				// Generate a mesh for each snapshot.
				treeFactory.meshManager.DeregisterSproutGroupMeshes (groupId);
				if (sproutMesh.subgroups.Length == 0) {
					Mesh groupMesh = advancedSproutMeshBuilder.MeshSprouts (tree, sproutMesh, groupId, -1);
					ApplyNormalMode (groupMesh, Vector3.zero);
					treeFactory.meshManager.RegisterSproutMesh (groupMesh, groupId);
				} else {
					Mesh groupMesh = new Mesh ();
					CombineInstance[] combine = new CombineInstance [sproutMesh.subgroups.Length];
					for (int i = 0; i < sproutMesh.subgroups.Length; i++) {
						combine [i].mesh = advancedSproutMeshBuilder.MeshSprouts (tree, sproutMesh, groupId, sproutMesh.subgroups [i]);
						combine [i].transform = Matrix4x4.identity;
						combine [i].subMeshIndex = 0;
						ApplyNormalMode (combine [i].mesh, Vector3.zero);
					}
					groupMesh.CombineMeshes (combine, true, false);
					treeFactory.meshManager.RegisterSproutMesh (groupMesh, groupId);
				}
			}
		}
		private void RegisterBranchCollection (
			TreeFactory treeFactory, 
			int lodIndex, 
			SproutMesh sproutMesh, 
			BranchDescriptorCollection branchDescriptorCollection)
		{
			SproutCompositeManager sproutCompositeManager = new SproutCompositeManager ();
			// Reconstruct the branch collection.
			BranchDescriptor branchDescriptor;
			for (int i = 0; i < branchDescriptorCollection.branchDescriptors.Count; i++) {
				branchDescriptor = branchDescriptorCollection.branchDescriptors [i];
				for (int j = 0; j < branchDescriptor.polygonAreas.Count; j++) {
					PolygonAreaBuilder.SetPolygonAreaMesh (branchDescriptor.polygonAreas [j]);
					sproutCompositeManager.ManagePolygonArea (branchDescriptor.polygonAreas [j], branchDescriptor);
				}
				Mesh meshToRegister = sproutCompositeManager.GetMesh (branchDescriptor.id, lodIndex);
				NormalizeBranchCollectionTransform (meshToRegister, 5f, Quaternion.Euler (0f, 90f, 90f));
				advancedSproutMeshBuilder.RegisterMesh (meshToRegister, sproutMesh.groupId, i);
			}
		}
		/// <summary>
		/// Applies scale and rotation to meshes coming from SproutLab's branch descriptor collection.
		/// </summary>
		/// <param name="mesh">Mesh to appy the transformation.</param>
		/// <param name="scale">Scale transformation.</param>
		/// <param name="rotation">Rotation transformation.</param>
		private void NormalizeBranchCollectionTransform (Mesh mesh, float scale, Quaternion rotation) {
			Vector3[] _vertices = mesh.vertices;
			Vector3[] _normals = mesh.normals;
			Vector4[] _tangents = mesh.tangents;
			for (int i = 0; i < _vertices.Length; i++) {
				_vertices [i] = rotation * _vertices [i] * scale;
				_normals [i] = rotation * _normals [i];
				_tangents [i] = rotation * _tangents [i];
			}
			mesh.vertices = _vertices;
			mesh.normals = _normals;
			mesh.tangents = _tangents;
			mesh.RecalculateBounds ();
		}
		#endregion
	}
}