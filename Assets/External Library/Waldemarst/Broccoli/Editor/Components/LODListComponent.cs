using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Model;
using Broccoli.Utils;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Component used to display a list of LODs for a structure.
	/// Events:
	/// onSelectLOD			Callback to call when a LOD is selected on the list.
    /// onBeforeAddLOD		Callback to call before a LOD instance gets added to the list.
	/// onAddLOD			Callback to call after a LOD has been added to the list.
	/// onBeforeEditLOD		Callback to call before a LOD get edited.
	/// onEditLOD			Callback to call after a LOD get edited.
    /// onBeforeRemoveLOD	Callback to call before a LOD get deleted from the list.
    /// onRemoveLOD 		Callback to call after a LOD has been removed from the list.
	/// onReorderLODs		Callback to call after the list get reodered.
	public class LODListComponent {
		#region Vars
		/// <summary>
		/// List of LODs.
		/// </summary>
		List<LODDef> lods = new List<LODDef> ();
		/// <summary>
		/// Preview LOD index.
		/// </summary>
		int previewLODIndex = -1;
		/// <summary>
		/// Flag to show the billboard option.
		/// </summary>
		public bool showBillboardOption = true;
		/// <summary>
		/// Flag to enable billboard LOD.
		/// </summary>
		bool hasBillboard = false;
		/// <summary>
		/// Percentage of the billboardLOD.
		/// </summary>
		float billboardPercentage = 0.3f;
		/// <summary>
		/// Preview LOD definition.
		/// </summary>
		LODDef previewLOD = null;
		/// <summary>
		/// The lods reorderable list.
		/// </summary>
		ReorderableList lodList;
		/// <summary>
		/// <c>True</c> if this list has been initialized.
		/// </summary>
		private bool isInit = false;
		/// <summary>
		/// Flag to show help text at each field.
		/// </summary>
		public bool showFieldHelp = false;
		/// <summary>
		/// Keeps track of all the active LODs in the list.
		/// </summary>
		/// <typeparam name="int"></typeparam>
		/// <returns></returns>
		private List<int> activeLODs = new List<int> ();
		private static Color lodBarColor = new Color (0.16f, 0.42f, 0.52f, 1);
		private static Color billboardBarColor = new Color (0.5f, 0.35f, 0.35f, 1);
		private static Color culledBarColor = new Color (0.3f, 0, 0, 1);
		#endregion

		#region Accessors
        /// <summary>
        /// Selected index of the lod from the list.
        /// </summary>
        /// <value>Value of the selected index in the LOD list.</value>
		public int selectedLODIndex {
			get { return lodList.index; }
			set {
				if (lods != null && value < lods.Count) {
					if (lodList.index != value) {
						lodList.index = value;
						onSelectLOD?.Invoke (lods [value], value);
					}
				}
			}
		}
        /// <summary>
        /// Selected lod instance from the list.
        /// </summary>
        /// <value>LOD instane selected on the list. If none is selected, null.</value>
        public LODDef selectedLOD {
            get { 
                if (lodList.index < 0) return null;
                return lods [lodList.index];
            }
        }
		#endregion

		#region Delegates
        /// <summary>
        /// Delegate to call with a LOD definition instance.
        /// </summary>
        /// <param name="lod">LOD definition instance.</param>
        public delegate void OnLODEventCallback (LODDef lod);
        /// <summary>
        /// Delegate to call with a LOD definition instance and its index in the list.
        /// </summary>
        /// <param name="lod">LOD definition instance.</param>
        /// <param name="index">Index on the list.</param>
		public delegate void OnLODIndexEventCallback (LODDef lod, int index);
		/// <summary>
		/// Delegate to call when there is a change in the billboard settings.
		/// </summary>
		/// <param name="hasBillboard"><c>True</c> to apply a billboard on the final prefab.</param>
		/// <param name="billboardPercentage">Percentage for the billboard on the prefab LOD group.</param>
		public delegate void OnBillboardEventCallback (bool hasBillboard, float billboardPercentage);
		/// <summary>
		/// Delegate to call on custom events from this list.
		/// </summary>
		public delegate void OnEventCallback ();
        /// <summary>
        /// Callback to call when a LOD definition is selected on the list.
        /// </summary>
		public OnLODIndexEventCallback onSelectLOD;
        /// <summary>
        /// Callback to call before a LOD definition instance gets added to the list.
        /// </summary>
		public OnLODEventCallback onBeforeAddLOD;
        /// <summary>
        /// Call back to call after a LOD definition has been added to the list.
        /// </summary>
		public OnLODIndexEventCallback onAddLOD;
		/// <summary>
        /// Callback to call before a LOD is edited.
        /// </summary>
		public OnLODEventCallback onBeforeEditLOD;
		/// <summary>
        /// Callback to call after a LOD is edited.
        /// </summary>
		public OnLODEventCallback onEditLOD;
        /// <summary>
        /// Callback to call before a LOD definition get deleted from the list.
        /// </summary>
		public OnLODIndexEventCallback onBeforeRemoveLOD;
        /// <summary>
        /// Callback to call after a LOD definition has been removed from the list.
        /// </summary>
		public OnLODEventCallback onRemoveLOD;
		/// <summary>
		/// Callback to call after the list is reordered.
		/// </summary>
		public OnEventCallback onReorderLODs;
		/// <summary>
		/// Callback to call when a preview LOD gets assigned.
		/// </summary>
		public OnLODEventCallback onPreviewLODSet;
		/// <summary>
		/// Called when changes on the preview LOD requires the structure to be rebuild.
		/// </summary>
		public OnEventCallback onRequiresRebuild;
		/// <summary>
		/// Called when there are changes in the billboard settings.
		/// </summary>
		public OnBillboardEventCallback onEditBillboard;
		#endregion

		#region Messages
		static string MSG_REMOVE_LOD_TITLE = "Remove LOD Definition";
		static string MSG_REMOVE_LOD_MESSAGE = "Do you really want to remove this LOD definition?";
		static string MSG_REMOVE_LOD_OK = "Yes, remove this LOD definition";
		static string MSG_REMOVE_LOD_CANCEL = "Cancel";
		static string MSG_LOD_LIST = "This list contains the LOD definitions. Selecting a LOD definition lets you edit its properties." +
			"The order of the definitions will be used to include them in the final Prefab LOD group; if only one definitions is checked " +
			"or there are no definitions on the list, then the final prefab will have just one mesh resolution and no LOD groups.";
		static string MSG_INCLUDE_IN_PREFAB = "If checked this LOD will be included in the final Prefab LOD Group.";
		static string MSG_MIN_POLYGON_SIDES = "Minimum number of sides on the polygon used to create the mesh.";
		static string MSG_MAX_POLYGON_SIDES = "Maximum number of sides on the polygon used to create the mesh.";
		static string MSG_BRANCH_CURVE_RESOLUTION = "Resolution used to process branch curves to create segments. The higher the angle, the lesser the resolution.";
		static string MSG_USE_MESH_CAP_AT_BASE = "Add triangles to the base of each branch to conceal inner (non-rendereable) planes.";
		static string MSG_ALLOW_BRANCH_WELDING = "If unchecked then branch welding is disabled on the tree.";
		static string MSG_ALLOW_ROOT_WELDING = "If unchecked then root welding is disabled on the tree.";
		static string MSG_SET_AS_PREVIEW = "Sets the selected LOD definition as default to use as preview in the editor and at runtime.";
		static string MSG_LOD_OVERFLOW = "The sum of the LOD groups are more than 100%. Please correct the LOD percentage values.";
		#endregion

		#region LODs
		/// <summary>
		/// Initializes the list.
		/// </summary>
		/// <param name="lodsToLoad">Structure LODs to initialize this list.</param>
		public void LoadLODs (List<LODDef> lodsToLoad, int previewLODIndex, bool hasBillboard, float billboardPercentage = 0.3f) {
			if (lodsToLoad == null)
				return;
			isInit = true;
			lods = lodsToLoad;
			lodList = new ReorderableList (lods, typeof (LOD), false, true, true, true);
			lodList.draggable = true;
			lodList.drawHeaderCallback += DrawListHeader;
			lodList.drawElementCallback += DrawItem;
			lodList.onSelectCallback += SelectItem;
			lodList.onAddCallback += AddItem;
			lodList.onRemoveCallback += RemoveItem;
			lodList.onAddDropdownCallback += AddDropdownMenu;
			lodList.onReorderCallback += ReorderList;
			previewLOD = null;
			if (previewLODIndex >= 0 && previewLODIndex < lods.Count) {
				previewLOD = lods [previewLODIndex];
				this.previewLODIndex = previewLODIndex;
			} else {
				previewLOD = null;
				this.previewLODIndex = -1;
			}
			this.hasBillboard = hasBillboard;
			this.billboardPercentage = billboardPercentage;
			CheckActiveLODs ();
		}
		/// <summary>
		/// Clears this instance.
		/// </summary>
		public void Clear () {
			lods = new List<LODDef> ();
			previewLOD = null;
			isInit = false;
		}
		/// <summary>
		/// Shows the help box.
		/// </summary>
		/// <param name="msg">Message.</param>
		protected void ShowHelpBox (string msg) {
			if (showFieldHelp)
				EditorGUILayout.HelpBox (msg, MessageType.None);
		}
        #endregion

        #region Reorderable List
		/// <summary>
		/// Renders this list content.
		/// </summary>
		public void DoLayout () {
			if (!isInit) {
				EditorGUILayout.LabelField ("No LODs to show.");
			} else {
				ShowHelpBox (MSG_LOD_LIST);
				lodList.DoLayoutList ();
				if (showBillboardOption) {
					EditorGUILayout.Space ();
					// HAS BILLBOARD.
					bool _hasBillboard = EditorGUILayout.Toggle ("Billboard on Prefab", hasBillboard);
					if (_hasBillboard != hasBillboard) {
						hasBillboard = _hasBillboard;
						onEditBillboard?.Invoke (hasBillboard, billboardPercentage);
					}
					if (hasBillboard) {
						// BILLBOARD PERCENTAGE.
						float _billboardPct = EditorGUILayout.Slider ("Billboard %", billboardPercentage * 100f, 5f, 100f);
						if (_billboardPct != billboardPercentage * 100f) {
							billboardPercentage = _billboardPct / 100f;
							onEditBillboard?.Invoke (hasBillboard, billboardPercentage);
						}
					}
				}
			}
		}
		/// <summary>
		/// Draws the lod list header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		void DrawListHeader (Rect rect) {
			GUI.Label(rect, "LODs", BroccoEditorGUI.labelBoldCentered);
		}
		/// <summary>
		/// Draws each lod list item.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		void DrawItem (Rect rect, int index, bool isActive, bool isFocused) {
			LODDef lod = null;
			if (index >= 0) lod = lods [index];
			if (lod != null) {
				bool isEnabled = EditorGUI.Toggle (new Rect (rect.x, rect.y, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight),
					lod.includeInPrefab);
				if (isEnabled != lod.includeInPrefab) {
					onBeforeEditLOD?.Invoke (lod);
					lod.includeInPrefab = isEnabled;
					onEditLOD?.Invoke (lod);
					CheckActiveLODs ();
				}
				rect.x += EditorGUIUtility.singleLineHeight + 2;
				GUI.Label (new Rect (rect.x, rect.y, 180, EditorGUIUtility.singleLineHeight + 5), 
					"LOD " + index + " (" + lod.preset + ")" + (index == previewLODIndex?" (Preview)":""));
				if (isActive) {
					EditorGUILayout.LabelField ("LOD " + index + " (" + lod.preset + ")", 
						BroccoEditorGUI.labelBoldCentered);
					// INCLUDE IN PREFAB.
					bool includeInPrefab = EditorGUILayout.Toggle ("Include in Prefab", lod.includeInPrefab);
					ShowHelpBox (MSG_INCLUDE_IN_PREFAB);
					if (includeInPrefab != lod.includeInPrefab) {
						onBeforeEditLOD?.Invoke (lod);
						lod.includeInPrefab = includeInPrefab;
						onEditLOD?.Invoke (lod);
						CheckActiveLODs ();
					}
					// MIN POLYGON SIDES.
					int minPolygonSides = EditorGUILayout.IntSlider ("Min Polygon Sides", lod.minPolygonSides, 3, lod.maxPolygonSides);
					ShowHelpBox (MSG_MIN_POLYGON_SIDES);
					if (minPolygonSides != lod.minPolygonSides) {
						onBeforeEditLOD?.Invoke (lod);
						lod.minPolygonSides = minPolygonSides;
						lod.preset = LODDef.Preset.Custom;
						onEditLOD?.Invoke (lod);
						if (lod == previewLOD)
							onRequiresRebuild?.Invoke ();
					}
					// MAX POLYGON SIDES.
					int maxPolygonSides = EditorGUILayout.IntSlider ("Max Polygon Sides", lod.maxPolygonSides, lod.minPolygonSides, 20);
					ShowHelpBox (MSG_MAX_POLYGON_SIDES);
					if (maxPolygonSides != lod.maxPolygonSides) {
						onBeforeEditLOD?.Invoke (lod);
						lod.maxPolygonSides = maxPolygonSides;
						lod.preset = LODDef.Preset.Custom;
						onEditLOD?.Invoke (lod);
						if (lod == previewLOD)
							onRequiresRebuild?.Invoke ();
					}
					// BRANCH ANGLE TOLERANCE.
					float branchAngleTolerance = EditorGUILayout.Slider ("Branch Angle Tolerance", lod.branchAngleTolerance, 5, 50);
					ShowHelpBox (MSG_BRANCH_CURVE_RESOLUTION);
					if (branchAngleTolerance != lod.branchAngleTolerance) {
						onBeforeEditLOD?.Invoke (lod);
						lod.branchAngleTolerance = branchAngleTolerance;
						lod.preset = LODDef.Preset.Custom;
						onEditLOD?.Invoke (lod);
						if (lod == previewLOD)
							onRequiresRebuild?.Invoke ();
					}
					// USE MESH CAP AT BASE.
					bool useMeshCapAtBase = EditorGUILayout.Toggle ("Use Mesh Cap at Base", lod.useMeshCapAtBase);
					ShowHelpBox (MSG_USE_MESH_CAP_AT_BASE);
					if (useMeshCapAtBase != lod.useMeshCapAtBase) {
						onBeforeEditLOD?.Invoke (lod);
						lod.useMeshCapAtBase = useMeshCapAtBase;
						lod.preset = LODDef.Preset.Custom;
						onEditLOD?.Invoke (lod);
						if (lod == previewLOD)
							onRequiresRebuild?.Invoke ();
					}
					// ALLOW BRANCH WELDING.
					bool allowBranchWelding = EditorGUILayout.Toggle ("Allow Branch Welding", lod.allowBranchWelding);
					ShowHelpBox (MSG_ALLOW_BRANCH_WELDING);
					if (allowBranchWelding != lod.allowBranchWelding) {
						onBeforeEditLOD?.Invoke (lod);
						lod.allowBranchWelding = allowBranchWelding;
						lod.preset = LODDef.Preset.Custom;
						onEditLOD?.Invoke (lod);
						if (lod == previewLOD)
							onRequiresRebuild?.Invoke ();
					}
					// ALLOW ROOT WELDING.
					bool allowRootWelding = EditorGUILayout.Toggle ("Allow Root Welding", lod.allowRootWelding);
					ShowHelpBox (MSG_ALLOW_ROOT_WELDING);
					if (allowRootWelding != lod.allowRootWelding) {
						onBeforeEditLOD?.Invoke (lod);
						lod.allowRootWelding = allowRootWelding;
						lod.preset = LODDef.Preset.Custom;
						onEditLOD?.Invoke (lod);
						if (lod == previewLOD)
							onRequiresRebuild?.Invoke ();
					}
					// LOD GROUP PERCENTAGE.
					float groupPercentage = EditorGUILayout.Slider ("LOD Group %", lod.groupPercentage * 100f, 5f, 100f);
					if (groupPercentage != lod.groupPercentage * 100f) {
						onBeforeEditLOD?.Invoke (lod);
						lod.groupPercentage = groupPercentage / 100f;
						onEditLOD?.Invoke (lod);
					}
					// SELECT AS PREVIEW.
					if (previewLODIndex != index) {
						if (GUILayout.Button ("Set As Preview LOD")) {
							previewLOD = lod;
							previewLODIndex = index;
							onPreviewLODSet?.Invoke (lod);
						}
						ShowHelpBox (MSG_SET_AS_PREVIEW);
					}
				}
			}
		}
		/// <summary>
		/// Called when an item in the list is selected.
		/// </summary>
		/// <param name="list"></param>
		void SelectItem (ReorderableList list) {
			if (0 <= list.index && list.index < list.count) {
				LODDef lod = selectedLOD;
				onSelectLOD?.Invoke (lod, list.index);
			}
		}
		/// <summary>
		/// Adds a lod to the list.
		/// </summary>
		/// <param name="list">List.</param>
		void AddItem (ReorderableList list) {
			LODDef lod = new LODDef ();
			onBeforeAddLOD?.Invoke (lod);
			lods.Add (lod);
			onAddLOD?.Invoke (lod, lods.Count - 1);
			list.index = lods.Count - 1;
			onSelectLOD?.Invoke (lod, lods.Count - 1);
			CheckActiveLODs ();
		}
		/// <summary>
		/// Removes a lod item.
		/// </summary>
		/// <param name="list">List.</param>
		void RemoveItem (ReorderableList list) {
			LODDef lod = selectedLOD;
			if (lod != null) {
				if (EditorUtility.DisplayDialog (MSG_REMOVE_LOD_TITLE, 
					MSG_REMOVE_LOD_MESSAGE, 
					MSG_REMOVE_LOD_OK, 
					MSG_REMOVE_LOD_CANCEL)) {
						onBeforeRemoveLOD?.Invoke (lod, list.index);
						int previousPreviewIndex = previewLODIndex;
						lods.Remove (lods [list.index]);
						onRemoveLOD?.Invoke (lod);
						// See if the preview LOD index changed.
						if (list.index == previewLODIndex) {
							previewLODIndex = -1;
						} else if (previewLODIndex > list.index) {
							previewLODIndex--;
						}
						if (previousPreviewIndex != previewLODIndex) {
							previewLOD = null;
							if (previewLODIndex >= 0) previewLOD = lods [previewLODIndex];
							onPreviewLODSet?.Invoke (previewLOD);
						}
						list.index = -1;
						onSelectLOD?.Invoke (null, -1);
						CheckActiveLODs ();
						GUIUtility.ExitGUI ();
				}
			}
		}
		/// <summary>
		/// Called when a list gets reordered.
		/// </summary>
		/// <param name="list">List.</param>
		void ReorderList (ReorderableList list) {
			if (previewLOD != null) {
				previewLODIndex = lods.IndexOf (previewLOD);
				onPreviewLODSet?.Invoke (previewLOD);
			}
			onReorderLODs?.Invoke ();
			CheckActiveLODs ();
		}
		/// <summary>
		/// Adds the dropdown menu.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="list">List.</param>
		private void AddDropdownMenu (Rect rect, ReorderableList list) {
			var menu = new GenericMenu ();
			menu.AddItem (new GUIContent ("Ultra Low Poly LOD"), 
				true, clickHandler, LODDef.Preset.UltraLowPoly);
			menu.AddItem (new GUIContent ("Low Poly LOD"), 
				true, clickHandler, LODDef.Preset.LowPoly);
			menu.AddItem (new GUIContent ("Regular Poly LOD"), 
				true, clickHandler, LODDef.Preset.RegularPoly);
			menu.AddItem (new GUIContent ("High Poly LOD"), 
				true, clickHandler, LODDef.Preset.HighPoly);
			menu.AddItem (new GUIContent ("Ultra High Poly LOD"), 
				true, clickHandler, LODDef.Preset.UltraHighPoly);
			menu.ShowAsContext();
		}
		/// <summary>
		/// Handler for the add menu.
		/// </summary>
		/// <param name="reference">Reference object.</param>
		private void clickHandler (object reference) {
			LODDef.Preset lodPreset = (LODDef.Preset) reference;
			LODDef lod = LODDef.GetPreset (lodPreset);
			onBeforeAddLOD?.Invoke (lod);
			lods.Add (lod);
			onAddLOD?.Invoke (lod, lods.Count - 1);
			lodList.index = lods.Count - 1;
			onSelectLOD?.Invoke (lod, lods.Count - 1);
			CheckActiveLODs ();
		}
		/// <summary>
		/// Keeps tracked of the index of the LOD definitions to be included in the LOD Group.
		/// </summary>
		private void CheckActiveLODs () {
			activeLODs.Clear ();
			for (int i = 0; i < lods.Count; i++) {
				if (lods[i].includeInPrefab) {
					activeLODs.Add (i);
				}
			}
		}
		#endregion

		#region LODs Bar
		// Custom GUILayout progress bar.
		public void DrawLODsBar (string label) {
			// Check if the bar should be drawn.
			if (hasBillboard || activeLODs.Count > 1) {
				// Backup GUI Colors.
				Color backupColor = GUI.color;
				Color backupContentColor = GUI.contentColor;
				GUI.contentColor = Color.white;
				GUI.color = Color.white;

				Rect barRect = GUILayoutUtility.GetRect (18, 18, "TextField");
				barRect.height = barRect.height * 2f - 4f;
				Rect lodRect = new Rect (barRect);
				float lodAccum = 0f;
				// DRAW LOD BARS.
				if (activeLODs.Count == 0) {
					// Draw LOD 0.
					lodRect.width = barRect.width * 0.7f;
					EditorGUI.DrawRect (lodRect, lodBarColor);
					EditorGUI.LabelField (lodRect, "LOD Base\n100%");
					lodAccum += 0.7f;
				} else {
					Color colorVar = new Color (0f, 0f, 0f, 0f);
					for (int i = 0; i < activeLODs.Count; i++) {
						if (lodAccum < 1f) {
							if (i != 0) lodRect.x += lodRect.width;
							lodRect.width = barRect.width * lods [activeLODs [i]].groupPercentage;		
							EditorGUI.DrawRect (lodRect, lodBarColor - colorVar);
							EditorGUI.LabelField (lodRect, "LOD" + activeLODs[i] +"\n" + Mathf.Floor((1f - lodAccum) * 100) + "%");
							colorVar.g -= 0.06f;
							lodAccum += lods [activeLODs [i]].groupPercentage;
						}
					}
				}
				// DRAW BILLBOARD BAR.
				if (hasBillboard && lodAccum < 1f) {
					lodRect.x += lodRect.width;
					lodRect.width = barRect.width * billboardPercentage;		
					EditorGUI.DrawRect (lodRect, billboardBarColor);
					EditorGUI.LabelField (lodRect, "Billboard\n" + Mathf.Floor((1f - lodAccum) * 100) + "%");
					lodAccum += billboardPercentage;
				}
				// DRAW CULLED BAR.
				if (lodAccum < 1f) {
					lodRect.x += lodRect.width;
					lodRect.width = barRect.width * (1f - lodAccum);
					EditorGUI.DrawRect (lodRect, culledBarColor);
					EditorGUI.LabelField (lodRect, "Culled\n" + Mathf.Floor((1f - lodAccum) * 100) + "%");
				}
				// Restore GUI colors.
				GUI.color = backupColor;
				GUI.contentColor = backupContentColor;
				GUILayoutUtility.GetRect (18, 18, "TextField");

				if (lodAccum > 1f) {
					EditorGUILayout.HelpBox (MSG_LOD_OVERFLOW, MessageType.Warning);
				}
			}

			// Get a rect for the progress bar using the same margins as a textfield:
			/*
			Rect rect = GUILayoutUtility.GetRect (18, 18, "TextField");
			EditorGUI.DrawRect (rect, Color.green);
			//EditorGUI.ProgressBar (rect, value, label);
			EditorGUILayout.Space ();
			*/
		}
		#endregion
	}
}
