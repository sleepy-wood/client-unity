using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Sprout groups manager.
	/// </summary>
	[System.Serializable]
	public class SproutGroups : ISerializationCallbackReceiver {
		#region class SproutGroup
		/// <summary>
		/// Sprout group container class.
		/// </summary>
		[System.Serializable]
		public class SproutGroup {
			/// <summary>
			/// The identifier.
			/// </summary>
			public int id = 0;
			/// <summary>
			/// Branch Collection asset, used to load sprout meshes to populate the tree.
			/// </summary>
			public ScriptableObject branchCollection = null;
			/// <summary>
			/// The index of the color.
			/// </summary>
			public int colorIndex = -1;
			/// <summary>
			/// The index.
			/// </summary>
			[System.NonSerialized]
			public int index = -1;
			/// <summary>
			/// Gets the color assigned to this group.
			/// </summary>
			/// <returns>The color.</returns>
			public Color GetColor () {
				return SproutGroups.GetColor (colorIndex);
			}
			/// <summary>
			/// Clone this instance.
			/// </summary>
			public SproutGroup Clone () {
				SproutGroup clone = new SproutGroup ();
				clone.id = id;
				clone.branchCollection = branchCollection;
				clone.colorIndex = colorIndex;
				return clone;
			}
		}
		#endregion

		#region Vars
		/// <summary>
		/// Number of sprout groups allowed.
		/// </summary>
		int limit = 11;
		/// <summary>
		/// The sprout groups.
		/// </summary>
		[SerializeField]
		List<SproutGroup> _sproutGroups = new List<SproutGroup> ();
		/// <summary>
		/// The popup group options.
		/// </summary>
		string[] popupOptions;
		/// <summary>
		/// The popup group options including unassigned.
		/// </summary>
		string[] popupOptionsWithUnassigned;
		/// <summary>
		/// Maps the sprout group id to its index on the sprout groups list.
		/// </summary>
		Dictionary<int, int> idToIndex = new Dictionary<int, int> ();
		#endregion

		#region Colors
		public static int colorCount = 11;
		static Color blueColor = new Color (0.51f, 0.71f, 0.89f);
		static Color emeraldColor = new Color (0.45f, 0.82f, 0.84f);
		static Color greenColor = new Color (0.76f, 0.87f, 0.44f);
		static Color pinkColor = new Color (1f, 0.75f, 0.79f);
		static Color yellowColor = new Color (1f, 0.96f, 0.60f);
		static Color redColor = new Color (0.93f, 0.38f, 0.38f);
		static Color violetColor = new Color (0.89f, 0.66f, 0.98f);
		static Color whiteColor = new Color (1f, 1f, 1f);
		static Color creamColor = new Color (0.78f, 0.76f, 0.66f);
		static Color purpleColor = new Color (0.52f, 0.39f, 0.67f);
		static Color orangeColor = new Color (0.98f, 0.45f, 0.40f);
		#endregion

		#region Serialization
		/// <summary>
		/// Raises the after deserialize event.
		/// </summary>
		public void OnAfterDeserialize () {
			BuildIndexes ();
			BuildPopupOptions ();
		}
		/// <summary>
		/// Raises the before serialize event.
		/// </summary>
		public void OnBeforeSerialize () {}
		#endregion

		#region CRUD
		/// <summary>
		/// Gets the sprout groups.
		/// </summary>
		/// <returns>The sprout groups.</returns>
		public List<SproutGroup> GetSproutGroups () {
			return _sproutGroups;
		}
		/// <summary>
		/// Gets the sprout group at the specified index.
		/// </summary>
		/// <returns>The sprout group at index.</returns>
		/// <param name="index">Index.</param>
		public SproutGroup GetSproutGroupAtIndex (int index) {
			if (index >= 0 && index < _sproutGroups.Count) {
				return _sproutGroups [index];
			}
			return null;
		}
		/// <summary>
		/// Gets the index of a sprout group given its id.
		/// </summary>
		/// <returns>The sprout group index.</returns>
		/// <param name="id">Group identifier.</param>
		/// <param name="includeUnassigned">If set to <c>true</c> includes the group count as the unassigned index.</param>
		public int GetSproutGroupIndex (int id, bool includeUnassigned = false) {
			if (idToIndex.ContainsKey (id)) {
				return idToIndex [id];
			}
			if (includeUnassigned)
				return _sproutGroups.Count ();
			return -1;
		}
		/// <summary>
		/// Gets the sprout group identifier.
		/// </summary>
		/// <returns>The sprout group identifier.</returns>
		/// <param name="index">Index.</param>
		public int GetSproutGroupId (int index) {
			SproutGroup sproutGroup = GetSproutGroupAtIndex (index);
			if (sproutGroup != null) {
				return sproutGroup.id;
			}
			return 0;
		}
		/// <summary>
		/// Determines whether this instance can create a sprout group.
		/// </summary>
		/// <returns><c>true</c> if this instance can create a sprout group; otherwise, <c>false</c>.</returns>
		public bool CanCreateSproutGroup () {
			return (_sproutGroups.Count < limit);
		}
		/// <summary>
		/// Clear the sprout groups.
		/// </summary>
		public void Clear () {
			_sproutGroups.Clear ();
			idToIndex.Clear ();
		}
		/// <summary>
		/// Determines whether this instance has a sprout group with a specified id.
		/// </summary>
		/// <returns><c>true</c> if this instance has a sprout group with the specified id; otherwise, <c>false</c>.</returns>
		/// <param name="id">Identifier.</param>
		public bool HasSproutGroup (int id) {
			SproutGroup group = GetSproutGroup (id);
			return (group != null);
		}
		/// <summary>
		/// Number of sprout groups on this instance.
		/// </summary>
		public int Count () {
			return _sproutGroups.Count;
		}
		/// <summary>
		/// Creates a sprout group.
		/// </summary>
		/// <returns>The sprout group.</returns>
		public SproutGroup CreateSproutGroup () {
			SproutGroup sproutGroup = null;
			if (CanCreateSproutGroup ()) {
				sproutGroup = new SproutGroup ();
				sproutGroup.id = GetId ();
				sproutGroup.colorIndex = GetColorIndex ();
				sproutGroup.index = _sproutGroups.Count ();
				idToIndex.Add (sproutGroup.id, sproutGroup.index);
				_sproutGroups.Add (sproutGroup);
				BuildPopupOptions ();
			}
			return sproutGroup;
		}
		/// <summary>
		/// Adds the sprout group to this container.
		/// </summary>
		/// <param name="sproutGroup">Sprout group.</param>
		public void AddSproutGroup (SproutGroup sproutGroup) {
			_sproutGroups.Add (sproutGroup);
		}
		/// <summary>
		/// Gets a sprout group given and id.
		/// </summary>
		/// <returns>The sprout group.</returns>
		/// <param name="id">Identifier.</param>
		public SproutGroup GetSproutGroup (int id) {
			return _sproutGroups.Find (sproutGroup => sproutGroup.id == id);
		}
		/// <summary>
		/// Deletes a sprout group given an id.
		/// </summary>
		/// <returns><c>true</c>, if sprout group was deleted, <c>false</c> otherwise.</returns>
		/// <param name="id">Identifier.</param>
		public bool DeleteSproutGroup (int id) {
			SproutGroup toDelete = GetSproutGroup (id);
			if (toDelete != null) {
				_sproutGroups.Remove (toDelete);
				BuildIndexes ();
				BuildPopupOptions ();
			}
			return false;
		}
		/// <summary>
		/// Deletes a sprout group at a given index.
		/// </summary>
		/// <returns><c>true</c>, if sprout group at index was deleted, <c>false</c> otherwise.</returns>
		/// <param name="index">Index.</param>
		public bool DeleteSproutGroupAtIndex (int index) {
			if (index < _sproutGroups.Count) {
				_sproutGroups.RemoveAt (index);
				BuildIndexes ();
				BuildPopupOptions ();
				return true;
			}
			return false;
		}
		/// <summary>
		/// Get an available id for a sprout group.
		/// </summary>
		/// <returns>Identifier.</returns>
		private int GetId () {
			int id = 1;
			while (HasSproutGroup (id)) {
				id++;
			}
			return id;
		}
		/// <summary>
		/// Gets the next available color index for a sprout group.
		/// </summary>
		/// <returns>The color index.</returns>
		private int GetColorIndex () {
			for (int i = 0; i < colorCount; i++) {
				bool colorUsed = false;
				for (int j = 0; j < _sproutGroups.Count; j++) {
					if (_sproutGroups [j].colorIndex == i) {
						colorUsed = true;
						break;
					}
				}
				if (!colorUsed) {
					return i;
				}
			}
			return 0;
		}
		#endregion

		#region Helpers
		/// <summary>
		/// Gets the array of sprout groups to use on a popup field.
		/// </summary>
		/// <returns>Array of sprout group names.</returns>
		public string[] GetPopupOptions (bool includeUnassigned = false) {
			if (includeUnassigned)
				return popupOptionsWithUnassigned;
			return popupOptions;
		}
		/// <summary>
		/// Builds the array of sprout groups to use on a popup field.
		/// </summary>
		public void BuildPopupOptions () {
			this.popupOptions = new string [_sproutGroups.Count];
			this.popupOptionsWithUnassigned = new string[_sproutGroups.Count + 1];
			for (int i = 0; i < _sproutGroups.Count; i++) {
				popupOptions [i] = "Group " + _sproutGroups [i].id;
				popupOptionsWithUnassigned [i] = "Group " + _sproutGroups [i].id;
			}
			popupOptionsWithUnassigned [_sproutGroups.Count] = "Unassigned";
		}
		/// <summary>
		/// Builds the indexes.
		/// </summary>
		public void BuildIndexes () {
			idToIndex.Clear ();
			for (int i = 0; i < _sproutGroups.Count; i++) {
				_sproutGroups [i].index = i;
				idToIndex.Add (_sproutGroups [i].id, i);
			}
		}
		#endregion

		#region Colors
		/// <summary>
		/// Color to index relationship.
		/// </summary>
		/// <returns>The color value.</returns>
		/// <param name="colorIndex">Color index.</param>
		public static Color GetColor (int colorIndex) {
			switch (colorIndex) {
			case 0:
				return greenColor;
			case 1:
				return pinkColor;
			case 2:
				return blueColor;
			case 3:
				return yellowColor;
			case 4:
				return violetColor;
			case 5:
				return redColor;
			case 6:
				return emeraldColor;
			case 7:
				return creamColor;
			case 8:
				return purpleColor;
			case 9:
				return orangeColor;
			case 10:
				return whiteColor;
			}
			return Color.clear;
		}
		/// <summary>
		/// Gets the color of the sprout group.
		/// </summary>
		/// <returns>The sprout group color.</returns>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public Color GetSproutGroupColor (int sproutGroupId) {
			if (idToIndex.ContainsKey (sproutGroupId)) {
				return _sproutGroups [idToIndex [sproutGroupId]].GetColor ();
			}
			return Color.black;
		}
		#endregion
	}
}