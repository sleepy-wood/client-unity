using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Pipe;
using Broccoli.Model;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Length transform component.
	/// </summary>
	public class LengthTransformComponent : TreeFactoryComponent {
		#region Vars
		/// <summary>
		/// The length transform element.
		/// </summary>
		LengthTransformElement lengthTransformElement = null;
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
			base.PrepareParams (treeFactory, useCache, useLocalCache, processControl);
			lengthTransformElement.PrepareSeed ();
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.StructureLength;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
			lengthTransformElement = null;
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
				lengthTransformElement = pipelineElement as LengthTransformElement;
				PrepareParams (treeFactory, useCache, useLocalCache);
				SetLength();
				return true;
			}
			return false;
		}
				/// <summary>
		/// Removes the product of this component on the factory processing.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		public override void Unprocess (TreeFactory treeFactory) {
			List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
			for (int i = 0; i < branches.Count; i++) {
				if (!branches[i].isTuned) {
					branches[i].lengthFactor = 1f;
				}
			}
		}
		/// <summary>
		/// Sets the length for the branches.
		/// </summary>
		void SetLength () {
			List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
			float levels = tree.GetOffspringLevel ();
			for (int i = 0; i < branches.Count; i++) {
				if (!branches[i].isTuned && !branches[i].isRoot) {
					float relativeHierarchyLevel = Mathf.InverseLerp (0, levels - 1, branches[i].GetHierarchyLevel ());
					relativeHierarchyLevel = lengthTransformElement.levelCurve.Evaluate (relativeHierarchyLevel);
					float relativePosition = lengthTransformElement.positionCurve.Evaluate (branches[i].position);
					branches[i].lengthFactor = Mathf.Lerp (lengthTransformElement.minFactor, 
						lengthTransformElement.maxFactor, 
						relativeHierarchyLevel * relativePosition);
				}
			}
		}
		#endregion
	}
}