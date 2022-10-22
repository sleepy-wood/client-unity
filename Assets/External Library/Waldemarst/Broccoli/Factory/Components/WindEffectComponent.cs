using System.Collections.Generic;

using UnityEngine;

using Broccoli.Pipe;
using Broccoli.Model;
using Broccoli.Builder;
using Broccoli.Manager;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Wind effect component.
	/// </summary>
	public class WindEffectComponent : TreeFactoryComponent {
		#region Vars
		/// <summary>
		/// The wind effect element.
		/// </summary>
		WindEffectElement windEffectElement = null;
		/// <summary>
		/// SpeedTree8 and SpeedTree7 compatible wind meta builder.
		/// </summary>
		STWindMetaBuilder stWindMetaBuilder = new STWindMetaBuilder ();
		/// <summary>
		/// The UV2s used on the merged mesh, placeholder for preview updating operations.
		/// </summary>
		List<Vector4> mergedMeshUV2s = new List<Vector4> ();
		/// <summary>
		/// The colors used on the merged mesh, placeholder for preview updating operations.
		/// </summary>
		Color[] mergedMeshColors = new Color[0];
		/// <summary>
		/// UV2 per sprout.
		/// </summary>
		Dictionary<int, Vector4> sproutUV2 = new Dictionary<int, Vector4> ();
		/// <summary>
		/// Color per sprout.
		/// </summary>
		Dictionary<int, Color> sproutColor = new Dictionary<int, Color> ();
		#endregion

		#region Configuration
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.None;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
			windEffectElement = null;
			stWindMetaBuilder.Clear ();
			sproutUV2.Clear ();
			sproutColor.Clear ();
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
				windEffectElement = pipelineElement as WindEffectElement;

				BranchMeshGeneratorElement branchMeshGeneratorElement = 
				(BranchMeshGeneratorElement) windEffectElement.GetUpstreamElement (PipelineElement.ClassType.BranchMeshGenerator); 
			
				if (branchMeshGeneratorElement != null && branchMeshGeneratorElement.isActive) {
					// Prepare branch vertex information for traversing the tree.
					BranchMeshGeneratorComponent branchMeshGeneratorComponent = 
						(BranchMeshGeneratorComponent) treeFactory.componentManager.GetFactoryComponent (
							branchMeshGeneratorElement);

					// Configure Wind Meta Builder.
					stWindMetaBuilder.windSpread = windEffectElement.windSpread;
					stWindMetaBuilder.windAmplitude = windEffectElement.windAmplitude;
					stWindMetaBuilder.sproutTurbulence = windEffectElement.sproutTurbulence;
					stWindMetaBuilder.sproutSway = windEffectElement.sproutSway;
					stWindMetaBuilder.weightCurve = windEffectElement.windFactorCurve;
					stWindMetaBuilder.isST7 = MaterialManager.leavesShaderType == MaterialManager.LeavesShaderType.SpeedTree7OrSimilar;
					stWindMetaBuilder.useMultiPhaseOnTrunk = windEffectElement.useMultiPhaseOnTrunk;
					if (stWindMetaBuilder.isST7) stWindMetaBuilder.useMultiPhaseOnTrunk = true;
					stWindMetaBuilder.applyToRoots = windEffectElement.applyToRoots;
					stWindMetaBuilder.branchSway = windEffectElement.branchSway;
					stWindMetaBuilder.AnalyzeTree (tree, branchMeshGeneratorComponent.branchSkins);

					// Set Wind Data.
					SetWindData (treeFactory);

					// Cleaning.
					stWindMetaBuilder.Clear ();
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Process a special command or subprocess on this component.
		/// </summary>
		/// <param name="cmd">Cmd.</param>
		/// <param name="treeFactory">Tree factory.</param>
		public override void ProcessComponentOnly (int cmd, TreeFactory treeFactory) {
			if (pipelineElement != null && tree != null) {
				windEffectElement = pipelineElement as WindEffectElement;

				BranchMeshGeneratorElement branchMeshGeneratorElement = 
				(BranchMeshGeneratorElement) windEffectElement.GetUpstreamElement (PipelineElement.ClassType.BranchMeshGenerator); 
			
				if (branchMeshGeneratorElement != null && branchMeshGeneratorElement.isActive) {
					// Prepare branch vertex information for traversing the tree.
					BranchMeshGeneratorComponent branchMeshGeneratorComponent = 
						(BranchMeshGeneratorComponent) treeFactory.componentManager.GetFactoryComponent (
							branchMeshGeneratorElement);

					// Configure Wind Meta Builder.
					stWindMetaBuilder.windSpread = windEffectElement.windSpread;
					stWindMetaBuilder.windAmplitude = windEffectElement.windAmplitude;
					stWindMetaBuilder.weightCurve = windEffectElement.windFactorCurve;
					stWindMetaBuilder.isST7 = MaterialManager.leavesShaderType == MaterialManager.LeavesShaderType.SpeedTree7OrSimilar;
					stWindMetaBuilder.useMultiPhaseOnTrunk = windEffectElement.useMultiPhaseOnTrunk;
					if (stWindMetaBuilder.isST7) stWindMetaBuilder.useMultiPhaseOnTrunk = true;
					stWindMetaBuilder.branchSway = windEffectElement.branchSway;
					stWindMetaBuilder.AnalyzeTree (tree, branchMeshGeneratorComponent.branchSkins);

					// Set Wind Data.
					SetWindData (treeFactory, true);

					// Cleaning.
					stWindMetaBuilder.Clear ();
				}
			}
		}
		/// <summary>
		/// Sets the wind data.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="updatePreviewTree">If set to <c>true</c> update preview tree.</param>
		void SetWindData (TreeFactory treeFactory, bool updatePreviewTree = false) {
			// Clean merged mesh UV2s and colors if we are going to update the whole mesh.
			if (updatePreviewTree) {
				mergedMeshUV2s.Clear ();
				mergedMeshColors  = new Color[0];
			}

			// Set wind on branch mesh
			int branchMeshId = MeshManager.MeshData.GetMeshDataId (MeshManager.MeshData.Type.Branch);
			Mesh mesh = treeFactory.meshManager.GetMesh (branchMeshId);
			stWindMetaBuilder.SetBranchesWindDataJobs (mesh);

			// Set wind on each sprouts mesh
			Dictionary<int, MeshManager.MeshData> meshDatas = 
				treeFactory.meshManager.GetMeshesDataOfType (MeshManager.MeshData.Type.Sprout);
			var meshDatasEnumerator = meshDatas.GetEnumerator ();
			int sproutMeshId;
			MeshManager.MeshData sproutMeshData;
			while (meshDatasEnumerator.MoveNext ()) {
				sproutMeshId = meshDatasEnumerator.Current.Key;
				sproutMeshData = meshDatasEnumerator.Current.Value;
				mesh = treeFactory.meshManager.GetMesh (sproutMeshId);

				// If the mesh has mesh parts.
				if (mesh != null && treeFactory.meshManager.HasMeshParts (sproutMeshId)) {
					List<MeshManager.MeshPart> meshParts = treeFactory.meshManager.GetMeshParts (sproutMeshId);
					stWindMetaBuilder.SetSproutsWindData (treeFactory.previewTree, sproutMeshId, mesh, meshParts);
				}
				// If the mesh has baked UV data for wind. 
				else if (sproutMeshData.type == MeshManager.MeshData.Type.Sprout) {
					stWindMetaBuilder.SetSproutsWindData (sproutMeshId, mesh);	
				}
			}
			
			if (updatePreviewTree) {
				MeshFilter meshFilter = tree.obj.GetComponent<MeshFilter> ();
				meshFilter.sharedMesh.SetUVs (1, mergedMeshUV2s);
				meshFilter.sharedMesh.colors = mergedMeshColors;
			}

			sproutUV2.Clear ();
			sproutColor.Clear ();
		}
	}
	#endregion
}