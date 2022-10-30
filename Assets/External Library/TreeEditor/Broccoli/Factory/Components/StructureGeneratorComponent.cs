using UnityEngine;

using Broccoli.Pipe;
using Broccoli.Generator;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Structure generator component.
	/// </summary>
	public class StructureGeneratorComponent : TreeFactoryComponent, IStructureGeneratorComponent {
		#region Vars
		/// <summary>
		/// The structure generator element.
		/// </summary>
		StructureGeneratorElement structureGeneratorElement;
		/// <summary>
		/// The structure generator.
		/// </summary>
		StructureGenerator generator = new StructureGenerator();
		/// <summary>
		/// The available root positions for the root branches.
		/// </summary>
		int availableRootPositions = 0;
		/// <summary>
		/// The unique root position if all the root branches have this common origin.
		/// </summary>
		Vector3 uniqueRootPosition = Vector3.zero;
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
			structureGeneratorElement.PrepareSeed ();
			structureGeneratorElement.rootStructureLevel.structureLevels = structureGeneratorElement.structureLevels;
			generator.useParentStructureRandomState = useLocalCache;
			//generator.useParentStructureRandomState = true;
			availableRootPositions = 0;
			uniqueRootPosition = Vector3.zero;
			generator.globalScale = treeFactory.treeFactoryPreferences.factoryScale;
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.Structure;
		}
		/// <summary>
		/// Clears the cache.
		/// </summary>
		public override void ClearCache () {
			// TODO
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
			structureGeneratorElement = null;
			generator.Clear ();
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
			structureGeneratorElement = pipelineElement as StructureGeneratorElement;
			if (processControl != null) {
				if (pipelineElement != null && tree != null && !useLocalCache) {
					PrepareParams (treeFactory, useCache, useLocalCache, processControl);
					StructureTree (treeFactory, useCache);
					randomState = Random.state;
				} else {
					Random.state = randomState;
				}
				if (useLocalCache) {
					processControl.lockedAspects |= (int)TreeFactoryProcessControl.ChangedAspect.Structure;
				}
				BuildTree (treeFactory, useCache);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Structures the tree.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		private void StructureTree(TreeFactory treeFactory, bool useCache = false) {
			// Check if there is a positioner element to position the trees.
			PositionerElement positionerElement = 
				(PositionerElement) structureGeneratorElement.GetDownstreamElement (PipelineElement.ClassType.Positioner);
			generator.Clear ();
			if (positionerElement != null && positionerElement.useCustomPositions) {
				generator.positions.Clear ();
				for (int i = 0; i < positionerElement.positions.Count; i++) {
					if (positionerElement.positions[i].enabled) {
						generator.positions.Add (positionerElement.positions[i].Clone ());
					}
				}
				generator.referencePosition = treeFactory.transform.position;
				availableRootPositions = generator.positions.Count;
				if (availableRootPositions == 1) {
					uniqueRootPosition = generator.positions [0].rootPosition;
				}
			} else {
				generator.positions.Clear ();
			}
			structureGeneratorElement.structures = 
					generator.GenerateStructures (structureGeneratorElement.structures, structureGeneratorElement.rootStructureLevel);
			structureGeneratorElement.OnBeforeSerialize ();
			structureGeneratorElement.DeserializeStructures ();
		}
		public static int version = 10;
		private void BuildTree (TreeFactory treeFactory, bool useCache = false) {
			generator.BuildTree (tree,
				structureGeneratorElement.structures);
			tree.UpdateFollowUps ();
		}
		/// <summary>
		/// Commit changes made on a branch's curve.
		/// </summary>
		/// <param name="branchGuid"></param>
		/// <param name="curve"></param>
		/// <param name="nodeIndex"></param>
		public void CommitBranchCurve (System.Guid branchGuid, int nodeIndex, Vector3 offset, bool commitToFollowup = false) {
			for (int i = 0; i < structureGeneratorElement.flatStructures.Count; i++) {
				if (structureGeneratorElement.flatStructures[i].guid == branchGuid) {
					StructureGenerator.Structure structure = structureGeneratorElement.flatStructures[i];
					// Update branch curve.
					//structure.branch.curve.ProcessAfterCurveCshanged ();
					// See if the branch is a followup, then the parent branch needs to be updated.
					if (nodeIndex == 0 && structure.branch.IsFollowUp ()) {
						// Update parent handle.
						structure.branch.parent.curve.Last ().handleStyle = structure.branch.curve.nodes[0].handleStyle;
						structure.branch.parent.curve.Last ().handle1 = structure.branch.curve.nodes[0].handle1;
						structure.branch.parent.curve.Last ().handle2 = structure.branch.curve.nodes[0].handle2;
						structure.branch.parent.curve.ProcessAfterCurveChanged ();
					} else if (nodeIndex == structure.branch.curve.nodes.Count - 1 && structure.branch.followUp != null && commitToFollowup) {
						// See if the branch needs to update its followup branch
						structure.branch.followUp.curve.First ().handleStyle = structure.branch.curve.Last ().handleStyle;
						structure.branch.followUp.curve.First ().handle1 = structure.branch.curve.Last ().handle1;
						structure.branch.followUp.curve.First ().handle2 = structure.branch.curve.Last ().handle2;
						structure.branch.followUp.curve.ProcessAfterCurveChanged ();
						SetStructureTuned (structure.branch.followUp.id);
					}
					structure.branch.curve.ProcessAfterCurveChanged ();
					CommitBranchCurveToRoot (structure);
					break;
				}
			}
		}
		public void CommitStructure (StructureGenerator.Structure structure) {
			structure.isTuned = true;
			structure.branch.isTuned = true;
			if (structure.parentStructure != null) {
				CommitStructure (structure.parentStructure);
			}
		}
		public void UnlockStructure (System.Guid branchGuid) {
			for (int i = 0; i < structureGeneratorElement.flatStructures.Count; i++) {
				if (structureGeneratorElement.flatStructures[i].guid == branchGuid) {
					UnlockStructureRecursive (structureGeneratorElement.flatStructures [i]);
					break;
				}
			}	
		}
		void UnlockStructureRecursive (StructureGenerator.Structure structure) {
			structure.isTuned = false;
			structure.branch.isTuned = false;
			for (int i = 0; i < structure.childrenStructures.Count; i++) {
				UnlockStructureRecursive (structure.childrenStructures[i]);
			}
		}
		/// <summary>
		/// Set to tuned structures up to the root of the tree.
		/// </summary>
		/// <param name="structure">Originating structure.</param>
		private void CommitBranchCurveToRoot (StructureGenerator.Structure structure) {
			structure.isTuned = true;
			structure.branch.isTuned = true;
			if (structureGeneratorElement.idToStructure.ContainsKey(structure.parentStructureId)) {
				CommitBranchCurveToRoot (structureGeneratorElement.idToStructure[structure.parentStructureId]);
			}
		}
		/// <summary>
		/// Set structure to tuned.
		/// </summary>
		/// <param name="structureId"></param>
		private void SetStructureTuned (int structureId) {
			for (int i = 0; i < structureGeneratorElement.flatStructures.Count; i++) {
				if (structureGeneratorElement.flatStructures[i].id == structureId) {
					structureGeneratorElement.flatStructures[i].isTuned = true;
					structureGeneratorElement.flatStructures[i].branch.isTuned = true;
					break;
				}
			}
		}
		#endregion

		#region IStructureGeneratorComponent
		/// <summary>
		/// The number of available root positions.
		/// </summary>
		public int GetAvailableRootPositions () {
			return availableRootPositions;
		}
		/// <summary>
		/// The unique root position offset when all the root branches have this origin.
		/// </summary>
		public Vector3 GetUniqueRootPosition () {
			return uniqueRootPosition;
		}
		#endregion
	}
}