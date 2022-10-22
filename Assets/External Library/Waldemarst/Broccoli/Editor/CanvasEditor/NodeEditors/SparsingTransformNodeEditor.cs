using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Base;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Sparsing transform node editor.
	/// </summary>
	[CustomEditor(typeof(SparsingTransformNode))]
	public class SparsingTransformNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The sparsing transform node.
		/// </summary>
		public SparsingTransformNode sparsingTransformNode;

		SerializedProperty propSparseLevels;

		ReorderableList sparseLevelsList;
		#endregion

		#region Messages
		private static string MSG_REORDER_MODE = "Mode for reordering branches along the root branch.";
		private static string MSG_LENGTH_MODE = "Mode for modifying the length of the branches.";
		private static string MSG_LENGTH_VALUE = "Value used to modify the length of the branches.";
		private static string MSG_TWIRL_MODE = "Mode for modifying the length of the branches.";
		private static string MSG_TWIRL_VALUE = "Value used to modify the twirl of the branches.";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			sparsingTransformNode = target as SparsingTransformNode;

			SetPipelineElementProperty ("sparsingTransformElement");

			propSparseLevels = GetSerializedProperty ("sparseLevels");
			sparseLevelsList = new ReorderableList (serializedObject, propSparseLevels, false, true, true, true);

			sparseLevelsList.draggable = false;
			sparseLevelsList.drawHeaderCallback += DrawSparseLevelItemHeader;
			sparseLevelsList.drawElementCallback += DrawSparseLevelItemElement;
			sparseLevelsList.onSelectCallback += OnSelectSparseLevelItem;
			sparseLevelsList.onAddCallback += OnAddSparseLevelItem;
			sparseLevelsList.onRemoveCallback += OnRemoveSparseLevelItem;
			sparseLevelsList.onAddDropdownCallback += AddDropdownMenu;
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			CheckUndoRequest ();

			UpdateSerialized ();

			EditorGUI.BeginChangeCheck ();
			if (sparsingTransformNode.sparsingTransformElement.selectedSparseLevelIndex != sparseLevelsList.index &&
				sparsingTransformNode.sparsingTransformElement.selectedSparseLevelIndex < sparseLevelsList.count) {
				sparseLevelsList.index = sparsingTransformNode.sparsingTransformElement.selectedSparseLevelIndex;
			}
			sparseLevelsList.DoLayoutList ();

			// Seed options.
			DrawSeedOptions ();

			if (EditorGUI.EndChangeCheck ()) {
				ApplySerialized ();
				UpdatePipeline (GlobalSettings.processingDelayHigh);
				SetUndoControlCounter ();
			}

			// Help options.
			DrawFieldHelpOptions ();
		}
		#endregion

		#region Sparse Levels Ordereable List
		/// <summary>
		/// Draws the list item header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawSparseLevelItemHeader(Rect rect)
		{
			GUI.Label(rect, "Levels");
		}
		/// <summary>
		/// Draws the list item element.
		/// </summary>
		/// <param name="rect">Rect to draw to.</param>
		/// <param name="index">Index of the item.</param>
		/// <param name="isActive">If set to <c>true</c> the item is active.</param>
		/// <param name="isFocused">If set to <c>true</c> the item is focused.</param>
		private void DrawSparseLevelItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			var sparseLevel = sparseLevelsList.serializedProperty.GetArrayElementAtIndex (index);
			int level = sparseLevel.FindPropertyRelative ("level").intValue;
			EditorGUI.LabelField (new Rect (rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight), 
				"Sparse for Level " + level);

			if (isActive) {
				if (index != sparsingTransformNode.sparsingTransformElement.selectedSparseLevelIndex) {
					sparsingTransformNode.sparsingTransformElement.selectedSparseLevelIndex = index;
				}
				EditorGUILayout.Space ();

				// Ordering mode.
				EditorGUILayout.PropertyField (sparseLevel.FindPropertyRelative ("reorderMode"));
				ShowHelpBox (MSG_REORDER_MODE);
				EditorGUILayout.Space ();

				// Length sparsing.
				EditorGUILayout.PropertyField (sparseLevel.FindPropertyRelative ("lengthSparsingMode"));
				ShowHelpBox (MSG_LENGTH_MODE);
				if (sparseLevel.FindPropertyRelative ("lengthSparsingMode").enumValueIndex != 
					(int)SparsingTransformElement.LengthSparsingMode.None) 
				{
					EditorGUILayout.Slider (sparseLevel.FindPropertyRelative ("lengthSparsingValue"), 0, 1, "Value");
					ShowHelpBox (MSG_LENGTH_VALUE);
				}
				EditorGUILayout.Space ();

				// Twirl sparsing.
				EditorGUILayout.PropertyField (sparseLevel.FindPropertyRelative ("twirlSparsingMode"));
				if (showFieldHelp)
					ShowHelpBox (MSG_TWIRL_MODE);
				if (sparseLevel.FindPropertyRelative ("twirlSparsingMode").enumValueIndex != 
					(int)SparsingTransformElement.TwirlSparsingMode.None) 
				{
					EditorGUILayout.Slider (sparseLevel.FindPropertyRelative ("twirlSparsingValue"), 0, 6.28f, "Value");
					ShowHelpBox (MSG_TWIRL_VALUE);
				}
				EditorGUILayout.Space ();
			}
		}
		/// <summary>
		/// Raises the select sparse level item event.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnSelectSparseLevelItem (ReorderableList list)
		{
			Undo.RecordObject (sparsingTransformNode.sparsingTransformElement, "Sparse Level selected");
			sparsingTransformNode.sparsingTransformElement.selectedSparseLevelIndex = list.index;
		}
		/// <summary>
		/// Adds a list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnAddSparseLevelItem(ReorderableList list)
		{
		}
		/// <summary>
		/// Removes a list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnRemoveSparseLevelItem(ReorderableList list)
		{
			int undoGroup = Undo.GetCurrentGroup ();
			Undo.SetCurrentGroupName ("Sparse Level removed");
			Undo.RecordObject (sparsingTransformNode.sparsingTransformElement, "Sparse Level removed");
			sparsingTransformNode.sparsingTransformElement.sparseLevels.RemoveAt (list.index);
			Undo.RecordObject (sparsingTransformNode.sparsingTransformElement, "Sparse Level removed");
			sparsingTransformNode.sparsingTransformElement.selectedSparseLevelIndex = -1;
			Undo.CollapseUndoOperations (undoGroup);
		}
		/// <summary>
		/// Adds the dropdown menu.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="list">List.</param>
		private void AddDropdownMenu (Rect rect, ReorderableList list) {
			var menu = new GenericMenu ();
			for (int i = 0; i < 6; i++) {
				bool exists = false;
				for (int j = 0; j < sparsingTransformNode.sparsingTransformElement.sparseLevels.Count; j++) {
					if (sparsingTransformNode.sparsingTransformElement.sparseLevels[j].level == i) {
						exists = true;
					}
				}
				if (!exists) {
					menu.AddItem (new GUIContent ("Level " + i),
						true, clickHandler, i);
				}
			}
			menu.ShowAsContext();
		}
		/// <summary>
		/// Handler for the add menu.
		/// </summary>
		/// <param name="reference">Reference object.</param>
		private void clickHandler (object reference) {
			int levelToAdd = (int)reference;
			SparsingTransformElement.SparseLevel sparseLevel = 
				new SparsingTransformElement.SparseLevel();
			sparseLevel.level = levelToAdd;
			Undo.RecordObject (sparsingTransformNode.sparsingTransformElement, "Sparse Level added");
			sparsingTransformNode.sparsingTransformElement.sparseLevels.Add (
				sparseLevel);
		}
		#endregion
	}
}