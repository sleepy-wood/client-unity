using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Sprout mesh generator element.
	/// </summary>
	[System.Serializable]
	public class SproutMeshGeneratorElement : PipelineElement, ISproutGroupConsumer {
		#region Vars
		/// <summary>
		/// Gets the type of the connection.
		/// </summary>
		/// <value>The type of the connection.</value>
		public override ConnectionType connectionType {
			get { return PipelineElement.ConnectionType.Transform; }
		}
		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		/// <value>The type of the element.</value>
		public override ElementType elementType {
			get { return PipelineElement.ElementType.MeshGenerator; }
		}
		/// <summary>
		/// Gets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public override ClassType classType {
			get { return PipelineElement.ClassType.SproutMeshGenerator; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get {
				return PipelineElement.meshGeneratorWeight + 20;
			}
		}
		/// <summary>
		/// The sprout meshes.
		/// </summary>
		public List<SproutMesh> sproutMeshes = new List<SproutMesh> ();
		/// <summary>
		/// Modes to process mesh normals.
		/// </summary>
		public enum NormalMode {
			/// <summary>
			/// Normals calculated from the center of every sprout entity.
			/// </summary>
			PerSprout,
			/// <summary>
			/// Normals calculated from the base of the tree.
			/// </summary>
			TreeOrigin,
			/// <summary>
			/// Normals from the center of the sprouts mesh.
			/// </summary>
			SproutsCenter,
			/// <summary>
			/// Normals calculated from the base of the lowest sprout found.
			/// </summary>
			SproutsBase
		}
		/// <summary>
		/// Normal mode to apply to the sprout mesh.
		/// </summary>
		public NormalMode normalMode = NormalMode.SproutsBase;
		/// <summary>
		/// Lerp value on how much the sprout normal mode is applied to mesh normals.
		/// </summary>
		public float normalModeStrength = 0.5f;
		/// <summary>
		/// The index of the selected mesh on the list.
		/// </summary>
		public int selectedMeshIndex = -1;
		/// <summary>
		/// Level of LOD information to show.
		/// </summary>
		[System.NonSerialized]
		public int showLODInfoLevel = -1;
		/// <summary>
		/// The vertices count for the first pass LOD.
		/// </summary>
		[System.NonSerialized]
		public int verticesCountFirstPass = 0;
		/// <summary>
		/// The triangles count for the first pass LOD.
		/// </summary>
		[System.NonSerialized]
		public int trianglesCountFirstPass = 0;
		/// <summary>
		/// The vertices count for the second pass LOD.
		/// </summary>
		[System.NonSerialized]
		public int verticesCountSecondPass = 0;
		/// <summary>
		/// The triangles count for the second pass LOD.
		/// </summary>
		[System.NonSerialized]
		public int trianglesCountSecondPass = 0;
		/// <summary>
		/// The assigned sprout groups.
		/// </summary>
		private List<int> assignedSproutGroups = new List<int> ();
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.SproutMeshGeneratorElement"/> class.
		/// </summary>
		public SproutMeshGeneratorElement () {}
		#endregion

		#region Validation
		/// <summary>
		/// Validate this instance.
		/// </summary>
		public override bool Validate () {
			log.Clear ();
			if (sproutMeshes.Count == 0) {
				log.Enqueue (LogItem.GetWarnItem ("There sprout mesh list is empty. Add an entry and assign it to a sprout group to mesh sprouts."));
			} else {
				bool allAssigned = true;
				bool allMeshAssigned = true;
				bool allBranchCollectionAssigned = true;
				for (int i = 0; i < sproutMeshes.Count; i++) {
					if (sproutMeshes[i].groupId <= 0) {
						allAssigned = false;
						break;
					}
					if (sproutMeshes[i].meshingMode == SproutMesh.MeshingMode.Shape && sproutMeshes[i].shapeMode == SproutMesh.ShapeMode.Mesh && sproutMeshes[i].meshGameObject == null) {
						allMeshAssigned = false;
						break;
					}
					if (sproutMeshes[i].meshingMode == SproutMesh.MeshingMode.BranchCollection && sproutMeshes[i].branchCollection == null) {
						allBranchCollectionAssigned = false;
						break;
					}
				}
				if (!allAssigned) {
					log.Enqueue (LogItem.GetWarnItem ("Not all sprout mesh entries are assigned to a sprout group."));
				}
				if (!allMeshAssigned) {
					log.Enqueue (LogItem.GetWarnItem ("Mesh missing on sprout group."));
				}
				if (!allBranchCollectionAssigned) {
					log.Enqueue (LogItem.GetWarnItem ("Branch Collection missing on sprout group."));
				}
			}
			return true;
		}
		#endregion

		#region Sprout Mesh Ops
		/// <summary>
		/// Determines whether this instance can add a sprout mesh.
		/// </summary>
		/// <returns><c>true</c> if this instance can add sprout mesh; otherwise, <c>false</c>.</returns>
		public bool CanAddSproutMesh () {
			return true; // TODO
		}
		/// <summary>
		/// Adds the sprout mesh.
		/// </summary>
		/// <param name="sproutMesh">Sprout mesh.</param>
		public void AddSproutMesh (SproutMesh sproutMesh) {
			if (pipeline != null) {
				sproutMeshes.Add (sproutMesh);
			}
		}
		/// <summary>
		/// Removes a sprout mesh.
		/// </summary>
		/// <param name="listIndex">List index.</param>
		public void RemoveSproutMesh (int listIndex) {
			if (pipeline != null) {
				SproutMesh sproutMesh = sproutMeshes [listIndex];
				sproutMeshes.RemoveAt (listIndex);
				pipeline.UpdateElementsOfType (ClassType.SproutMapper);
			}
		}
		/// <summary>
		/// Gets an array of sprout group ids assigned to the element.
		/// </summary>
		/// <returns>The sprout groups assigned.</returns>
		public List<int> GetSproutGroupsAssigned () {
			assignedSproutGroups.Clear ();
			for (int i = 0; i < sproutMeshes.Count; i++) {
				if (sproutMeshes[i].groupId >= 0) {
					assignedSproutGroups.Add (sproutMeshes[i].groupId);
				}
			}
			return assignedSproutGroups;
		}
		#endregion

		#region Sprout Group Consumer
		/// <summary>
		/// Look if certain sprout group is being used in this element.
		/// </summary>
		/// <returns><c>true</c>, if sprout group is being used, <c>false</c> otherwise.</returns>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public bool HasSproutGroupUsage (int sproutGroupId) {
			for (int i = 0; i < sproutMeshes.Count; i++) {
				if (sproutMeshes[i].groupId == sproutGroupId)
					return true;
			}
			return false;
		}
		/// <summary>
		/// Commands the element to stop using certain sprout group.
		/// </summary>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public void StopSproutGroupUsage (int sproutGroupId) {
			for (int i = 0; i < sproutMeshes.Count; i++) {
				if (sproutMeshes[i].groupId == sproutGroupId) {
					#if UNITY_EDITOR
					UnityEditor.Undo.RecordObject (this, "Sprout Group Removed from Mesh");
					#endif
					sproutMeshes[i].groupId = 0;
				}
			}
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			SproutMeshGeneratorElement clone = ScriptableObject.CreateInstance<SproutMeshGeneratorElement> ();
			SetCloneProperties (clone);
			clone.sproutMeshes.Clear ();
			for (int i = 0; i < sproutMeshes.Count; i++) {
				clone.sproutMeshes.Add (sproutMeshes[i].Clone());
			}
			clone.selectedMeshIndex = selectedMeshIndex;
			clone.normalMode = normalMode;
			clone.normalModeStrength = normalModeStrength;
			return clone;
		}
		#endregion
	}
}