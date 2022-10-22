using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Utils;
using Broccoli.Generator;

namespace Broccoli.Pipe {
	[System.Serializable]
	/// <summary>
	/// Structure generator element.
	/// </summary>
	public class StructureGeneratorElement : PipelineElement, ISproutGroupConsumer, IStructureGenerator, ISerializationCallbackReceiver {
		#region Vars
		/// <summary>
		/// Gets the type of the connection.
		/// </summary>
		/// <value>The type of the connection.</value>
		public override ConnectionType connectionType {
			get { return PipelineElement.ConnectionType.Source; }
		}
		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		/// <value>The type of the element.</value>
		public override ElementType elementType {
			get { return PipelineElement.ElementType.StructureGenerator; }
		}
		/// <summary>
		/// Gets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public override ClassType classType {
			get { return PipelineElement.ClassType.StructureGenerator; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.structureGeneratorWeight;	}
		}
		/// <summary>
		/// Gets a value indicating whether this <see cref="Broccoli.Pipe.StructureGeneratorElement"/> uses randomization.
		/// </summary>
		/// <value><c>true</c> if uses randomization; otherwise, <c>false</c>.</value>
		public override bool usesRandomization {
			get { return true; }
		}
		/// <summary>
		/// Keeps the offset of the canvas used to edit the levels.
		/// </summary>
		public Vector2 canvasOffset = Vector2.zero;
		/// <summary>
		/// Keeps the structure level tree on a simple list.
		/// </summary>
		public List<StructureGenerator.StructureLevel> flatStructureLevels = new List<StructureGenerator.StructureLevel> ();
		/// <summary>
		/// The structure levels.
		/// </summary>
		[System.NonSerialized]
		public List<StructureGenerator.StructureLevel> structureLevels = 
			new List<StructureGenerator.StructureLevel> ();
		/// <summary>
		/// Holds the generated structures for this tree.
		/// </summary>
		/// <typeparam name="StructureGenerator.Structure"></typeparam>
		/// <returns></returns>
		[SerializeField]
		public List<StructureGenerator.Structure> flatStructures = new List<StructureGenerator.Structure> ();
		/// <summary>
		/// The structures.
		/// </summary>
		[System.NonSerialized]
		public List<StructureGenerator.Structure> structures = 
			new List<StructureGenerator.Structure> ();
		/// <summary>
		/// Id to structure dictionary.
		/// </summary>
		/// <typeparam name="int"></typeparam>
		/// <typeparam name="StructureGenerator.Structure"></typeparam>
		/// <returns></returns>
		[System.NonSerialized] 	
		public Dictionary<int, StructureGenerator.Structure> idToStructure = new Dictionary<int, StructureGenerator.Structure> ();
		/// <summary>
		/// Guid to structure dictionary.
		/// </summary>
		/// <typeparam name="System.Guid"></typeparam>
		/// <typeparam name="StructureGenerator.Structure"></typeparam>
		/// <returns></returns>
		[System.NonSerialized] 	
		public Dictionary<System.Guid, StructureGenerator.Structure> guidToStructure = new Dictionary<System.Guid, StructureGenerator.Structure> ();
		/// <summary>
		/// The identifier used on the last added structure level.
		/// </summary>
		[System.NonSerialized]
		private int lastId = 0;
		/// <summary>
		/// The selected structure level. When 0 no structure level is selected (means root is selected).
		/// </summary>
		[System.NonSerialized]
		public StructureGenerator.StructureLevel selectedLevel = null;
		/// <summary>
		/// Root structure level with instructions to build the root branches of the tree.
		/// </summary>
		/// <returns>Root structure level.</returns>
		public StructureGenerator.StructureLevel rootStructureLevel = new StructureGenerator.StructureLevel ();
		/// <summary>
		/// Id to structure level dictionary.
		/// </summary>
		[System.NonSerialized]
		public Dictionary<int, StructureGenerator.StructureLevel> idToStructureLevels = 
			new Dictionary<int, StructureGenerator.StructureLevel> ();
		/// <summary>
		/// True if all the sprout structure levels are assigned to a sprout group (used on validation).
		/// </summary>
		bool allAssignedToGroup = true;
		/// <summary>
		/// The levels to delete.
		/// </summary>
		List<StructureGenerator.StructureLevel> levelsToDelete = new List<StructureGenerator.StructureLevel> ();
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.StructureGeneratorElement"/> class.
		/// </summary>
		public StructureGeneratorElement () {}
		#endregion

		#region Serialization
		/// <summary>
		/// Prepares the flatStructure variable to hold the serialized information.
		/// </summary>
		public void OnBeforeSerialize() {
			flatStructures.Clear ();
			SerializeStructures (structures);
		}
		/// <summary>
		/// Deserializes the flatStructure to the working variables.
		/// </summary>
		public void OnAfterDeserialize () {
			DeserializeStructures ();
		}
		/// <summary>
		/// Prepares the structures to be serialized.
		/// </summary>
		/// <param name="structuresToSerialize">List of structures to serialize.</param>
		private void SerializeStructures (List<StructureGenerator.Structure> structuresToSerialize) {
			for (int i = 0; i < structuresToSerialize.Count; i++) {
				flatStructures.Add (structuresToSerialize[i]);

				// Set parent structure id.
				if (structuresToSerialize[i].parentStructure != null) {
					structuresToSerialize[i].parentStructureId = structuresToSerialize[i].parentStructure.id;

					if (structuresToSerialize[i].branch != null) {

						// Set branch parent id.
						if (structuresToSerialize[i].branch.parent != null) {
							structuresToSerialize[i].branch.parentBranchId = structuresToSerialize[i].branch.parent.id;
						} else {
							structuresToSerialize[i].branch.parentBranchId = -1;
						}
					}
				} else {
					structuresToSerialize[i].parentStructureId = -1;
				}
				SerializeStructures (structuresToSerialize[i].childrenStructures);
			}
		}
		/// <summary>
		/// Deserializes flatStructures to structures tree data. 
		/// </summary>
		public void DeserializeStructures () {
			structures.Clear ();
			idToStructure.Clear ();
			guidToStructure.Clear ();
			for (int i = 0; i < flatStructures.Count; i++) {
				flatStructures[i].childrenStructures.Clear ();
				flatStructures[i].branch.branches.Clear ();
				idToStructure.Add (flatStructures[i].id, flatStructures[i]);
				guidToStructure.Add (flatStructures[i].guid, flatStructures[i]);
			}
			StructureGenerator.Structure parentStructure;
			StructureGenerator.Structure childStructure;
			for (int i = 0; i < flatStructures.Count; i++) {
				childStructure = flatStructures[i];
				if (idToStructure.ContainsKey (flatStructures[i].parentStructureId)) {
					parentStructure = idToStructure[flatStructures[i].parentStructureId];

					// Add to parent or this element.
					parentStructure.childrenStructures.Add (childStructure);
					childStructure.parentStructure = parentStructure;
					childStructure.parentStructureId = parentStructure.id;

					// Add branch to parent.
					parentStructure.branch.AddBranch (childStructure.branch);
					childStructure.branch.parentBranchId = parentStructure.branch.id;
				} else if (flatStructures[i].parentStructureId == -1) {
					structures.Add (childStructure);
					childStructure.parentStructure = null;
					childStructure.parentStructureId = -1;
				}
			}
			// Update branches.
			for (int i = 0; i < structures.Count; i++) {
				structures[i].branch.Update (true);
			}
			
		}
		public void SetStructureLevelRecursive (StructureGenerator.StructureLevel structureLevel, int level) {
			structureLevel.level = level;
			/*
			for (int i = 0; i < structureLevel.childrenS; i++) {
				
			}
			*/
		}
		#endregion
		

		#region Validation
		/// <summary>
		/// Validate this instance.
		/// </summary>
		public override bool Validate () {
			log.Clear ();
			allAssignedToGroup = true;
			for (int i = 0; i < structureLevels.Count; i++) {
				ValidateGroupAssigned (structureLevels[i]);
				if (!allAssignedToGroup)
					break;
			}
			if (!allAssignedToGroup) {
				log.Enqueue (LogItem.GetWarnItem ("There are sprout levels not assigned to a group."));
			}
			return true;
		}
		/// <summary>
		/// Validates if a sprout structure level is assigned to a sprout group.
		/// </summary>
		/// <param name="level">Structure level.</param>
		private void ValidateGroupAssigned (StructureGenerator.StructureLevel level) {
			// TODO: protect against looping.
			if (allAssignedToGroup && level.isSprout && level.sproutGroupId <= 0) {
				allAssignedToGroup = false;
			}
			if (allAssignedToGroup) {
				for (int i = 0; i < level.structureLevels.Count; i++) {
					ValidateGroupAssigned (level.structureLevels[i]);
					if (!allAssignedToGroup)
						break;
				}
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// Raises the add to pipeline event.
		/// </summary>
		public override void OnAddToPipeline () {
			BuildStructureLevelTree ();
		}
		#endregion

		#region StructureLevels operations
		/// <summary>
		/// Gets the structure level identifier.
		/// </summary>
		/// <returns>The structure level identifier.</returns>
		private int GetStructureLevelId () {
			int id = lastId + 1;
			bool found = false;
			while (!found) {
				found = true;
				for (int i = 0; i < flatStructureLevels.Count; i++) {
					if (flatStructureLevels[i].id == id) found = false;
				}
				if (!found) id++;
			}
			lastId = id;
			return id;
		}
		/// <summary>
		/// Gets the index of the structure level.
		/// </summary>
		/// <returns>The structure level index.</returns>
		/// <param name="structureLevel">Structure level.</param>
		public int GetStructureLevelIndex (StructureGenerator.StructureLevel structureLevel) {
			int index = -1;
			for (int i = 0; i < flatStructureLevels.Count; i++) {
				if (flatStructureLevels[i] == structureLevel) {
					index = i;
					break;
				}
			}
			return index;
		}
		/// <summary>
		/// Adds a new structure level.
		/// </summary>
		/// <returns>The structure level.</returns>
		/// <param name="parent">Parent structure level, null to have root as parent.</param>
		/// <param name="isSprout">If set to <c>true</c> the structure level is for sprouts.</param>
		public StructureGenerator.StructureLevel AddStructureLevel (StructureGenerator.StructureLevel parent = null,
			bool isSprout = false, bool isRoot = false)
		{
			StructureGenerator.StructureLevel newLevel = new StructureGenerator.StructureLevel ();
			newLevel.id = GetStructureLevelId ();
			newLevel.isSprout = isSprout;
			newLevel.isRoot = isRoot;
			if (parent != null) {
				newLevel.parentId = parent.id;
				newLevel.nodePosition = parent.nodePosition + new Vector2 (50, (isRoot?70:-70));
				// Root structure level has another root structure level as parent.
				if (isRoot) {
					SetRootStructureLevel (newLevel);
				}
			} else {
				newLevel.nodePosition = rootStructureLevel.nodePosition + new Vector2 (50, (isRoot?70:-70));
				// Root structure level at the main trunk.
				if (isRoot) {
					SetRootStructureLevel (newLevel, true);
				}
			}
			flatStructureLevels.Add (newLevel);
			idToStructureLevels.Add (newLevel.id, newLevel);
			BuildStructureLevelTree ();
			return newLevel;
		}
		/// <summary>
		/// Set default values for structure levels to generate roots.
		/// </summary>
		/// <param name="rootStructureLevel"></param>
		/// <param name="fromTrunk">True is the structure level has the trunk as parent.</param>
		public void SetRootStructureLevel (StructureGenerator.StructureLevel rootStructureLevel, bool fromTrunk = false) {
			if (fromTrunk) {
				rootStructureLevel.actionRangeEnabled = true;
				rootStructureLevel.minRange = 0.05f;
				rootStructureLevel.maxRange = 0.05f;
				rootStructureLevel.distribution = StructureGenerator.StructureLevel.Distribution.Whorled;
				rootStructureLevel.childrenPerNode = 5;
				rootStructureLevel.minFrequency = 4;
				rootStructureLevel.maxFrequency = 5;
				rootStructureLevel.gravityAlignAtBase = -0.1f;
				rootStructureLevel.gravityAlignAtTop = -0.1f;
				rootStructureLevel.parallelAlignAtBase = -0.1f;
				rootStructureLevel.parallelAlignAtTop = -0.1f;
				rootStructureLevel.minGirthScale = 0.7f;
				rootStructureLevel.maxGirthScale = 0.85f;
				rootStructureLevel.distributionAngleVariance = 0.05f;
				rootStructureLevel.distributionSpacingVariance = 0.05f;
				rootStructureLevel.distributionOrigin = StructureGenerator.StructureLevel.DistributionOrigin.FromTip;
			} else {
				rootStructureLevel.gravityAlignAtBase = -0.1f;
				rootStructureLevel.gravityAlignAtTop = -0.1f;
				rootStructureLevel.randomTwirlOffsetEnabled = false;
				rootStructureLevel.minFrequency = 2;
				rootStructureLevel.maxFrequency = 3;
			}
		}
		/// <summary>
		/// Adds a new structure level sharing odds of occurence with a sibling node.
		/// </summary>
		/// <returns>The new structure level.</returns>
		/// <param name="siblingLevel">Level to share the occurrence with.</param>
		/// <param name="isSprout">If set to <c>true</c> the structure level is for sprouts.</param>
		public StructureGenerator.StructureLevel AddSharedStructureLevel (
			StructureGenerator.StructureLevel siblingLevel, 
			bool isSprout = false)
		{
			if (siblingLevel != null) {
				StructureGenerator.StructureLevel newLevel = new StructureGenerator.StructureLevel ();
				newLevel.id = GetStructureLevelId ();
				newLevel.isSprout = isSprout;
				newLevel.parentId = siblingLevel.parentId;
				flatStructureLevels.Add (newLevel);
				idToStructureLevels.Add (newLevel.id, newLevel);

				// Set sharing next.
				int stepsFromMain = 1;
				StructureGenerator.StructureLevel lastLevel = GetLastInSharedGroup (siblingLevel, out stepsFromMain);
				lastLevel.sharingNextId = newLevel.id;
				newLevel.nodePosition = lastLevel.nodePosition + new Vector2 (40f, 0);

				// Set sharing group.
				if (siblingLevel.sharingGroupId == 0) {
					newLevel.sharingGroupId = siblingLevel.id;
				} else {
					newLevel.sharingGroupId = siblingLevel.sharingGroupId;
				}
					
				BuildStructureLevelTree ();
				return newLevel;
			}
			return null;
		}
		/// <summary>
		/// Gets the last structure level in a shared group.
		/// </summary>
		/// <returns>The last structure level in a shared group.</returns>
		/// <param name="memberLevel">Member level of the shared group.</param>
		/// <param name="position">Position on the shared group, non-zero based.</param>
		StructureGenerator.StructureLevel GetLastInSharedGroup (StructureGenerator.StructureLevel memberLevel, out int position) {
			position = 1;
			if (memberLevel.sharingGroupId == 0 && memberLevel.sharingNextId == 0) {
				return memberLevel;
			} else {
				int mainLevelId = memberLevel.id;
				if (memberLevel.sharingGroupId != 0)
					mainLevelId = memberLevel.sharingGroupId;
				StructureGenerator.StructureLevel currentLevel = idToStructureLevels [mainLevelId];
				int maxLoop = 40;
				do {
					currentLevel = idToStructureLevels [currentLevel.sharingNextId];
					position++;
					maxLoop--;
				} while (currentLevel.sharingNextId != 0 && maxLoop > 0);
				if (maxLoop <= 0) {
					Debug.LogWarning ("Probable endless loop found on shared structure levels.");
				}
				return currentLevel;
			}
		}
		/// <summary>
		/// Builds the structure level tree.
		/// </summary>
		public void BuildStructureLevelTree () {
			structureLevels.Clear ();
			Dictionary<int, List<StructureGenerator.StructureLevel>> levelRel = 
				new Dictionary<int, List<StructureGenerator.StructureLevel>> ();
			for (int i = 0; i < flatStructureLevels.Count; i++) {
				if (flatStructureLevels[i].parentId == 0) {
					structureLevels.Add (flatStructureLevels[i]);
				} else {
					if (!levelRel.ContainsKey (flatStructureLevels[i].parentId)) {
						levelRel.Add (flatStructureLevels[i].parentId, new List<StructureGenerator.StructureLevel> ());
					}
					levelRel [flatStructureLevels[i].parentId].Add (flatStructureLevels[i]);
				}
				SproutGroups.SproutGroup sproutGroup = pipeline.sproutGroups.GetSproutGroup (flatStructureLevels[i].sproutGroupId);
				if (sproutGroup != null) {
					flatStructureLevels[i].sproutGroupColor = sproutGroup.GetColor ();
				}
			}
			idToStructureLevels.Clear ();
			for (int i = 0; i < flatStructureLevels.Count; i++) {
				if (levelRel.ContainsKey (flatStructureLevels[i].id)) {
					flatStructureLevels[i].structureLevels = levelRel [flatStructureLevels[i].id];
					for (int j = 0; j < flatStructureLevels[i].structureLevels.Count; j++) {
						flatStructureLevels[i].structureLevels[j].parentStructureLevel = flatStructureLevels[i];
					}
				}
				idToStructureLevels.Add (flatStructureLevels [i].id, flatStructureLevels [i]);
			}
			rootStructureLevel.structureLevels = structureLevels;
			UpdateDrawVisible ();
			levelRel.Clear ();
		}
		/// <summary>
		/// Removes a structure level.
		/// </summary>
		/// <param name="levelToDelete">Level to delete.</param>
		public void RemoveStructureLevel (StructureGenerator.StructureLevel levelToDelete) {
			MarkForDeletion (levelToDelete);

			// Remove from any sharing group if present in one.
			if (levelToDelete.IsShared ()) {
				RemoveFromSharingGroup (levelToDelete);
			}

			// Delete all the levels marked for deletion (levelToDelete and its hierarchy).
			levelsToDelete.Clear ();
			for (int i = 0; i < flatStructureLevels.Count; i++) {
				if (flatStructureLevels[i].isMarkedForDeletion) {
					levelsToDelete.Add (flatStructureLevels[i]);
				}
			}
			for (int i = 0; i < levelsToDelete.Count; i++) {
				flatStructureLevels.Remove (levelsToDelete [i]);
				idToStructureLevels.Remove (levelsToDelete [i].id);
			}
			levelsToDelete.Clear ();

			//Delete structure level from parent
			if (levelToDelete.parentStructureLevel != null) {
				int indexAt = -1;
				for (int i = 0; i < levelToDelete.parentStructureLevel.structureLevels.Count; i++) {
					if (levelToDelete == levelToDelete.parentStructureLevel.structureLevels [i]) {
						indexAt = i;
					}
				}
				if (indexAt >= 0) {
					levelToDelete.parentStructureLevel.structureLevels.RemoveAt (indexAt);
				}
			}
			RemoveStructureLevelRecursive (levelToDelete);

			// Build the structure tree again.
			BuildStructureLevelTree ();

			// Cleaning up.
			levelsToDelete.Clear ();
		}
		/// <summary>
		/// Marks a structure level for deletion.
		/// </summary>
		/// <param name="levelToMark">Level to mark.</param>
		private void MarkForDeletion (StructureGenerator.StructureLevel levelToMark) {
			levelToMark.isMarkedForDeletion = true;
			for (int i = 0; i < levelToMark.structureLevels.Count; i++) {
				MarkForDeletion (levelToMark.structureLevels[i]);
			}
		}
		/// <summary>
		/// Removes the structure levels recursively.
		/// </summary>
		/// <param name="level">Level.</param>
		private void RemoveStructureLevelRecursive (StructureGenerator.StructureLevel level) {
			for (int i = 0; i < level.structureLevels.Count; i++) {
				RemoveStructureLevelRecursive (level.structureLevels[i]);
			}
			level.structureLevels.Clear ();
		}
		/// <summary>
		/// Removes a structure level from a sharing group, updating id references.
		/// </summary>
		/// <param name="levelToDelete">Level to delete.</param>
		private void RemoveFromSharingGroup (StructureGenerator.StructureLevel levelToDelete) {
			if (levelToDelete.sharingGroupId == 0) {
				// levelToDelete is main, so we turn the next level to main
				StructureGenerator.StructureLevel nextLevel = idToStructureLevels [levelToDelete.sharingNextId];
				nextLevel.sharingGroupId = 0;

				// Update the levels on the sharing group with the new sharing group id.
				int sharingGroupId = nextLevel.id;
				int maxLoop = 40;
				do {
					if (idToStructureLevels.ContainsKey (nextLevel.sharingNextId)) {
						nextLevel = idToStructureLevels [nextLevel.sharingNextId];
						nextLevel.sharingGroupId = sharingGroupId;
					} else {
						nextLevel = null;
					}
					maxLoop --;
				} while (nextLevel != null && maxLoop > 0);
				if (maxLoop <= 0) {
					Debug.LogWarning ("Probable endless loop found on shared structure levels.");
				}
			} else {
				// Level is last or somewhere in the middle of the sharing group.
				// First we get the main level for the group.
				StructureGenerator.StructureLevel currentLevel = idToStructureLevels [levelToDelete.sharingGroupId];
				// Reference to the previous element.
				StructureGenerator.StructureLevel previousLevel = currentLevel;
				int maxLoop = 40;
				bool addPositionOffset = false;
				do {
					if (idToStructureLevels.ContainsKey (currentLevel.sharingNextId)) {
						currentLevel = idToStructureLevels [currentLevel.sharingNextId];
						if (currentLevel.id == levelToDelete.id) {
							previousLevel.sharingNextId = currentLevel.sharingNextId;
							addPositionOffset = true;
						}
						if (addPositionOffset)
							currentLevel.nodePosition -= new Vector2 (40, 0);
						previousLevel = currentLevel;
					} else {
						currentLevel = null;
					}
					maxLoop --;
				} while (currentLevel != null && maxLoop > 0);
				if (maxLoop <= 0) {
					Debug.LogWarning ("Probable endless loop found on shared structure levels.");
				}
			}
		}
		/// <summary>
		/// Updates the draw visible structure levels.
		/// </summary>
		public void UpdateDrawVisible () {
			for (int i = 0; i < flatStructureLevels.Count; i++) {
				flatStructureLevels[i].isDrawVisible = true;
			}
			for (int i = 0; i < structureLevels.Count; i++) {
				SetDrawVisible (structureLevels[i]);
			}
		}
		/// <summary>
		/// Sets the draw visible structure levels.
		/// </summary>
		/// <param name="levelToSet">Level to set.</param>
		/// <param name="overrideToFalse">If set to <c>true</c> override to false.</param>
		private void SetDrawVisible (StructureGenerator.StructureLevel levelToSet, bool overrideToFalse = false) {
			if (!levelToSet.enabled || overrideToFalse) {
				levelToSet.isDrawVisible = false;
				overrideToFalse = true;
			}
			for (int i = 0; i < levelToSet.structureLevels.Count; i++) {
				SetDrawVisible (levelToSet.structureLevels[i], overrideToFalse);
			}
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			StructureGeneratorElement clone = ScriptableObject.CreateInstance<StructureGeneratorElement> ();
			SetCloneProperties (clone);
			clone.rootStructureLevel = rootStructureLevel.Clone ();
			clone.canvasOffset = canvasOffset;
			for (int i = 0; i < flatStructureLevels.Count; i++) {
				clone.flatStructureLevels.Add (flatStructureLevels[i].Clone ());
			}
			for (int i = 0; i < flatStructures.Count; i++) {
				clone.flatStructures.Add (flatStructures[i].Clone ());
			}
			clone.DeserializeStructures ();
			return clone;
		}
		#endregion

		#region Sprout Group Consumer
		/// <summary>
		/// Look if certain sprout group is being used in this element.
		/// </summary>
		/// <returns><c>true</c>, if sprout group is being used, <c>false</c> otherwise.</returns>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public bool HasSproutGroupUsage (int sproutGroupId) {
			for (int i = 0; i < flatStructureLevels.Count; i++) {
				if (flatStructureLevels[i].isSprout && flatStructureLevels[i].sproutGroupId == sproutGroupId)
					return true;
			}
			return false;
		}
		/// <summary>
		/// Commands the element to stop using certain sprout group.
		/// </summary>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public void StopSproutGroupUsage (int sproutGroupId) {
			for (int i = 0; i < flatStructureLevels.Count; i++) {
				if (flatStructureLevels[i].isSprout && flatStructureLevels[i].sproutGroupId == sproutGroupId) {
					#if UNITY_EDITOR
					UnityEditor.Undo.RecordObject (this, "Sprout Group Removed from Level");
					#endif
					flatStructureLevels[i].sproutGroupId = 0;
					flatStructureLevels[i].sproutGroupColor = Color.clear;
				}
			}
		}
		#endregion
	}
}
