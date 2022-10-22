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
	/// Positioner node editor.
	/// </summary>
	[CustomEditor(typeof(PositionerNode))]
	public class PositionerNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The positioner node.
		/// </summary>
		public PositionerNode positionerNode;
		/// <summary>
		/// The positions list.
		/// </summary>
		ReorderableList positionsList;
		SerializedProperty propUseCustomPositions;
		SerializedProperty propPositions;
		SerializedProperty propAddCollisionObjectAtTrunk;
		#endregion

		#region Messages
		private static string MSG_USE_CUSTOM_POSITIONS = "Use a list of custom positions for the trees.";
		private static string MSG_POSITIONS_LIST = "List of custom positions.";
		private static string MSG_POSITION_ENABLED = "Enables this position to be a possible point of origin for a tree.";
		private static string MSG_POSITION_POSITION = "Actual position.";
		private static string MSG_POSITION_OVERRIDE_DIRECTION = "If true then the tree spawning from this position takes a custom direction.";
		private static string MSG_POSITION_DIRECTION = "Custom direction for the tree spawning from this position.";
		private static string MSG_USE_COLLISION_OBJECTS = "Create collision objects at tree trunk level.";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			positionerNode = target as PositionerNode;

			SetPipelineElementProperty ("positionerElement");

			propUseCustomPositions = GetSerializedProperty ("useCustomPositions");
			propPositions = GetSerializedProperty ("positions");

			positionsList = new ReorderableList (serializedObject, propPositions, false, true, true, true);
			positionsList.draggable = false;
			positionsList.drawHeaderCallback += DrawListItemHeader;
			positionsList.drawElementCallback += DrawListItemElement;
			positionsList.onSelectCallback += OnSelectListItem;
			positionsList.onAddCallback += OnAddListItem;
			positionsList.onRemoveCallback += OnRemoveListItem;

			propAddCollisionObjectAtTrunk = GetSerializedProperty ("addCollisionObjectAtTrunk");
		}
		/// <summary>
		/// Raises the scene GUI event.
		/// </summary>
		/// <param name="sceneView">Scene view.</param>
		protected override void OnSceneGUI (SceneView sceneView) {
			if (positionerNode.positionerElement.useCustomPositions) {
				if (positionerNode.positionerElement.selectedPositionIndex >= 0) {
					// Handle
					EditorGUI.BeginChangeCheck ();
					Vector3 newTargetPosition = Handles.PositionHandle (
						positionerNode.positionerElement.positions [positionerNode.positionerElement.selectedPositionIndex].rootPosition +
                       TreeFactoryEditorWindow.editorWindow.treeFactory.transform.position,
                       Quaternion.identity);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RecordObject (positionerNode.positionerElement, "Tree position moved.");
						positionerNode.positionerElement.positions [positionerNode.positionerElement.selectedPositionIndex].rootPosition = 
						newTargetPosition - TreeFactoryEditorWindow.editorWindow.treeFactory.transform.position;
						SceneView.RepaintAll ();
						Repaint ();
					}
				}
				for (int i = 0; i < positionerNode.positionerElement.positions.Count; i++) {
					if (positionerNode.positionerElement.positions[i].enabled) {
						Handles.color = Color.yellow;
					} else {
						Handles.color = Color.gray;
					}
					Vector3 handlePosition = positionerNode.positionerElement.positions[i].rootPosition + 
						TreeFactoryEditorWindow.editorWindow.treeFactory.transform.position;
					Handles.DrawSolidDisc (handlePosition,
						Camera.current.transform.forward, HandleUtility.GetHandleSize (handlePosition) * 0.1f);
					
					#if UNITY_5_5_OR_NEWER || UNITY_5_5
					Handles.ArrowHandleCap (0, positionerNode.positionerElement.positions[i].rootPosition + 
						TreeFactoryEditorWindow.editorWindow.treeFactory.transform.position
						, Quaternion.LookRotation (positionerNode.positionerElement.positions[i].rootDirection), 
						HandleUtility.GetHandleSize (handlePosition) * 0.7f, EventType.Repaint);
					#else
					Handles.ArrowCap (0, position.rootPosition + TreeFactoryEditorWindow.editorWindow.treeFactory.transform.position
					, Quaternion.LookRotation (position.rootDirection), HandleUtility.GetHandleSize (handlePosition) * 0.7f);
					#endif
				}
				Handles.color = Color.yellow;
				Handles.DrawWireDisc ( TreeFactoryEditorWindow.editorWindow.treeFactory.transform.position,
					Camera.current.transform.forward, 
					HandleUtility.GetHandleSize (TreeFactoryEditorWindow.editorWindow.treeFactory.transform.position) * 
					GlobalSettings.treeFactoryPositionGizmoSize);
			}
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			UpdateSerialized ();

			// Log box.
			DrawLogBox ();

			EditorGUI.BeginChangeCheck ();

			// Positions.
			EditorGUILayout.PropertyField (propUseCustomPositions);
			ShowHelpBox (MSG_USE_CUSTOM_POSITIONS);
			if (propUseCustomPositions.boolValue) {
				if (positionerNode.positionerElement.selectedPositionIndex != positionsList.index &&
					positionerNode.positionerElement.selectedPositionIndex < positionsList.count) {
					positionsList.index = positionerNode.positionerElement.selectedPositionIndex;
				}
				ShowHelpBox (MSG_POSITIONS_LIST);
				positionsList.DoLayoutList ();
			}

			if (EditorGUI.EndChangeCheck ()) {
				ApplySerialized ();
				positionerNode.positionerElement.Validate ();
			}

			EditorGUI.BeginChangeCheck ();

			// Collision objects.
			EditorGUILayout.PropertyField (propAddCollisionObjectAtTrunk);
			ShowHelpBox (MSG_USE_COLLISION_OBJECTS);
			EditorGUILayout.Space ();

			// Seed options.
			DrawSeedOptions ();

			if (EditorGUI.EndChangeCheck ()) {
				UpdatePipeline (GlobalSettings.processingDelayVeryLow);
				ApplySerialized ();
				positionerNode.positionerElement.Validate ();
			}
			EditorGUILayout.Space ();

			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		#endregion

		#region Map Ordereable List
		/// <summary>
		/// Draws the positions list header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawListItemHeader(Rect rect)
		{
			GUI.Label(rect, "Positions");
		}
		/// <summary>
		/// Draws a position item element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			var positionProp = positionsList.serializedProperty.GetArrayElementAtIndex (index);
			bool positionEnabled = positionProp.FindPropertyRelative ("enabled").boolValue;
			EditorGUI.LabelField (new Rect (rect.x, rect.y, 
				150, EditorGUIUtility.singleLineHeight), "Position " + index + (positionEnabled?"":" <disabled>"));

			if (isActive) {
				if (index != positionerNode.positionerElement.selectedPositionIndex) {
					positionerNode.positionerElement.selectedPositionIndex = index;
					SceneView.RepaintAll ();
				}
				EditorGUILayout.Space ();

				// Enabled
				EditorGUILayout.PropertyField (positionProp.FindPropertyRelative ("enabled"));
				ShowHelpBox (MSG_POSITION_ENABLED);
				if (positionEnabled) {
					// Root position
					EditorGUILayout.PropertyField (positionProp.FindPropertyRelative ("rootPosition"));
					ShowHelpBox (MSG_POSITION_POSITION);
					// Override root direction
					EditorGUILayout.PropertyField (positionProp.FindPropertyRelative ("overrideRootDirection"));
					ShowHelpBox (MSG_POSITION_OVERRIDE_DIRECTION);
					if (positionProp.FindPropertyRelative ("overrideRootDirection").boolValue) {
						// Root direction
						EditorGUILayout.PropertyField (positionProp.FindPropertyRelative ("rootDirection"));
						ShowHelpBox (MSG_POSITION_DIRECTION);
					}
				}
			}
		}
		/// <summary>
		/// Adds a position to the list.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnAddListItem (ReorderableList list)
		{
			Position position = new Position ();
			Undo.RecordObject (positionerNode.positionerElement, "Tree position added");
			positionerNode.positionerElement.positions.Add (position);
		}
		/// <summary>
		/// Selects a position on the positions list.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnSelectListItem (ReorderableList list)
		{
			Undo.RecordObject (positionerNode.positionerElement, "Tree position selected");
			positionerNode.positionerElement.selectedPositionIndex = list.index;
		}
		/// <summary>
		/// Removes a position for the list.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnRemoveListItem (ReorderableList list)
		{
			int undoGroup = Undo.GetCurrentGroup ();
			Undo.SetCurrentGroupName ("Tree position removed");
			Undo.RecordObject (positionerNode.positionerElement, "Tree position removed");
			positionerNode.positionerElement.positions.RemoveAt (list.index);
			Undo.RecordObject (positionerNode.positionerElement, "Tree position selected");
			positionerNode.positionerElement.selectedPositionIndex = -1;
			Undo.CollapseUndoOperations (undoGroup);
		}
		#endregion

	}
}