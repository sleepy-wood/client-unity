using System.Collections.Generic;

using UnityEngine;

using Broccoli.Model;
using Broccoli.Pipe;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Baker component.
	/// Does nothing, knows nothing... just like Jon.
	/// </summary>
	public class BakerComponent : TreeFactoryComponent {
		#region Vars
		/// <summary>
		/// The positioner element.
		/// </summary>
		BakerElement bakerElement = null;
		#endregion

		#region Configuration
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.None;
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
			TreeFactoryProcessControl ProcessControl = null) 
		{
			if (pipelineElement != null && tree != null) {
				bakerElement = pipelineElement as BakerElement;
				if (bakerElement.enableAO) {
					bool enableAO = (ProcessControl.isPreviewProcess && bakerElement.enableAOInPreview) || ProcessControl.isRuntimeProcess || ProcessControl.isPrefabProcess;
					if (enableAO) {
						treeFactory.meshManager.enableAO = true;
						treeFactory.meshManager.samplesAO = bakerElement.samplesAO;
						treeFactory.meshManager.strengthAO = bakerElement.strengthAO;
					} else {
						treeFactory.meshManager.enableAO = false;
					}
					return true;
				}
			}
			treeFactory.meshManager.enableAO = false;
			return false;
		}
		/// <summary>
		/// Processes called only on the prefab creation.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		public override void OnProcessPrefab (TreeFactory treeFactory) {
			treeFactory.meshManager.enableAO = false;
			if (bakerElement.enableAO) {
				treeFactory.assetManager.enableAO = true;
				treeFactory.assetManager.samplesAO = bakerElement.samplesAO;
				treeFactory.assetManager.strengthAO = bakerElement.strengthAO;
			}
			if (bakerElement.lodFade == BakerElement.LODFade.Crossfade) {
				treeFactory.assetManager.lodFadeMode = LODFadeMode.CrossFade;
			} else if (bakerElement.lodFade == BakerElement.LODFade.SpeedTree) {
				treeFactory.assetManager.lodFadeMode = LODFadeMode.SpeedTree;
			} else {
				treeFactory.assetManager.lodFadeMode = LODFadeMode.None;
			}
			treeFactory.assetManager.lodFadeAnimate = bakerElement.lodFadeAnimate;
			treeFactory.assetManager.lodTransitionWidth = bakerElement.lodTransitionWidth;
		}
		#endregion
	}
}