using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Sprout mapper element.
	/// </summary>
	[System.Serializable]
	public class SproutMapperElement : PipelineElement, ISproutGroupConsumer {
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
			get { return PipelineElement.ElementType.MeshTransform; }
		}
		/// <summary>
		/// Gets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public override ClassType classType {
			get { return PipelineElement.ClassType.SproutMapper; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.mapperWeight + 10; }
		}
		/// <summary>
		/// Gets a value indicating whether this <see cref="Broccoli.Pipe.SproutMapperElement"/> uses randomization.
		/// </summary>
		/// <value><c>true</c> if uses randomization; otherwise, <c>false</c>.</value>
		public override bool usesRandomization {
			get { return true; }
		}
		/// <summary>
		/// The sprout maps.
		/// </summary>
		public List<SproutMap> sproutMaps = new List<SproutMap> ();
		/// <summary>
		/// The index of the selected map.
		/// </summary>
		public int selectedMapIndex = -1;
		/// <summary>
		/// The assigned sprout groups.
		/// </summary>
		private List<int> assignedSproutGroups = new List<int> ();
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.SproutMapperElement"/> class.
		/// </summary>
		public SproutMapperElement () {}
		#endregion

		#region Validation
		/// <summary>
		/// Validate this instance.
		/// </summary>
		public override bool Validate () {
			log.Clear ();
			if (sproutMaps.Count == 0) {
				log.Enqueue (LogItem.GetWarnItem ("There sprout maps are empty. Maps link sprouts to textures and materials."));
			} else {
				bool allAssigned = true;
				bool allHaveTextures = false;
				bool allMaterialsAssigned = true;
				int textureModeMaps = 0;
				for (int i = 0; i < sproutMaps.Count; i++) {
					if (sproutMaps[i].groupId <= 0) {
						allAssigned = false;
					}
					if (sproutMaps[i].mode == SproutMap.Mode.Texture) {
						textureModeMaps++;
						for (int j = 0; j < sproutMaps[i].sproutAreas.Count; j++) {
							if (sproutMaps[i].sproutAreas[j].texture != null && sproutMaps[i].sproutAreas[j].enabled) {
								allHaveTextures = true;
								break;
							}
						}
					} else if (sproutMaps[i].mode == SproutMap.Mode.Material && sproutMaps[i].customMaterial == null) {
						allMaterialsAssigned = false;
						break;
					}
				}
				if (!allAssigned) {
					log.Enqueue (LogItem.GetWarnItem ("Not all sprout map entries are assigned to a sprout group."));
				} else if (textureModeMaps > 0 && !allHaveTextures) {
					log.Enqueue (LogItem.GetWarnItem ("Not all sprout maps have textures assigned or are enabled."));
				} else if (!allMaterialsAssigned) {
					log.Enqueue (LogItem.GetWarnItem ("Not all sprout maps have an assigned material."));
				}
			}
			return true;
		}
		#endregion

		#region Sprout Maps
		/// <summary>
		/// Determines whether this instance can add a sprout map.
		/// </summary>
		/// <returns><c>true</c> if this instance can add sprout map; otherwise, <c>false</c>.</returns>
		public bool CanAddSproutMap () {
			return true; // TODO
		}
		/// <summary>
		/// Adds the sprout map.
		/// </summary>
		/// <param name="sproutMap">Sprout map.</param>
		public void AddSproutMap (SproutMap sproutMap) {
			if (pipeline != null) {
				sproutMaps.Add (sproutMap);
			}
		}
		/// <summary>
		/// Removes a sprout map.
		/// </summary>
		/// <param name="listIndex">List index.</param>
		public void RemoveSproutMap (int listIndex) {
			if (pipeline != null) {
				sproutMaps.RemoveAt (listIndex);
			}
		}
		/// <summary>
		/// Gets an array of sprout group ids assigned to the element.
		/// </summary>
		/// <returns>The sprout groups assigned.</returns>
		public List<int> GetSproutGroupsAssigned () {
			assignedSproutGroups.Clear ();
			for (int i = 0; i < sproutMaps.Count; i++) {
				if (sproutMaps[i].groupId >= 0) {
					assignedSproutGroups.Add (sproutMaps[i].groupId);
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
			for (int i = 0; i < sproutMaps.Count; i++) {
				if (sproutMaps[i].groupId == sproutGroupId)
					return true;
			}
			return false;
		}
		/// <summary>
		/// Get the SproutMap assigned to a group.
		/// </summary>
		/// <param name="sproutGroupId">Group Id</param>
		/// <returns>SproutMap assigned.</returns>
		public SproutMap GetSproutMap (int sproutGroupId) {
			if (sproutGroupId > -1) {
				for (int i = 0; i < sproutMaps.Count; i++) {
					if (sproutMaps[i].groupId == sproutGroupId)
						return sproutMaps[i];
				}	
			}
			return null;
		}
		/// <summary>
		/// Commands the element to stop using certain sprout group.
		/// </summary>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public void StopSproutGroupUsage (int sproutGroupId) {
			for (int i = 0; i < sproutMaps.Count; i++) {
				if (sproutMaps[i].groupId == sproutGroupId) {
					#if UNITY_EDITOR
					UnityEditor.Undo.RecordObject (this, "Sprout Group Removed from Mapper");
					#endif
					sproutMaps[i].groupId = 0;
				}
			}
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			SproutMapperElement clone = ScriptableObject.CreateInstance<SproutMapperElement> ();
			SetCloneProperties (clone);
			clone.sproutMaps.Clear ();
			for (int i = 0; i < sproutMaps.Count; i++) {
				clone.sproutMaps.Add (sproutMaps[i].Clone ());
			}
			clone.selectedMapIndex = selectedMapIndex;
			return clone;
		}
		#endregion
	}
}