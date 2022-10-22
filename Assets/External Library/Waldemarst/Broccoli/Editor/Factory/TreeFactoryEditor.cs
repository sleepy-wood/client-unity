using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Broccoli.Base;
using Broccoli.Pipe;

namespace Broccoli.Factory
{
	/// <summary>
	/// Tree factory editor.
	/// </summary>
	[CustomEditor(typeof(TreeFactory))]
	public class TreeFactoryEditor : Editor {
		#region Vars
		public TreeFactory treeFactory;
		StructureGeneratorElement structureGeneratorElement = null;
		PositionerElement positionerElement = null;
		bool positionerEnabled = false;
		#endregion

		#region Events
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		void OnEnable () {
			treeFactory = target as TreeFactory;

			if (Broccoli.TreeNodeEditor.TreeFactoryEditorWindow.IsOpen ()) {
				Broccoli.TreeNodeEditor.TreeFactoryEditorWindow.OpenTreeFactoryWindow (treeFactory);
			}

			// Handles init
			positionerEnabled = false;
			if (treeFactory.localPipeline != null && treeFactory.localPipeline.root != null) {
				if (treeFactory.localPipeline.root.classType == PipelineElement.ClassType.StructureGenerator) {
					structureGeneratorElement = (StructureGeneratorElement)treeFactory.localPipeline.root;
				}
				positionerElement = (PositionerElement)treeFactory.localPipeline.root.GetDownstreamElement (PipelineElement.ClassType.Positioner);
				if (positionerElement != null) {
					positionerEnabled = positionerElement.HasValidPosition ();
				}
			}
		}
		/// <summary>
		/// Raises the disable event.
		/// </summary>
		void OnDisable () {
			structureGeneratorElement = null;
			positionerElement = null;
			treeFactory = null;
		}
		/// <summary>
		/// Raises the scene GU event.
		/// </summary>
		protected void OnSceneGUI () {
			Handles.color = Color.yellow;
			if (positionerEnabled && positionerElement.useCustomPositions && positionerElement.HasValidPosition ()) {
				for (int i = 0; i < positionerElement.positions.Count; i++) {
					if (positionerElement.positions[i].enabled) {
						Handles.color = Color.yellow;
					} else {
						Handles.color = Color.gray;
					}
					Handles.DrawSolidDisc (positionerElement.positions[i].rootPosition + treeFactory.transform.position,
						Camera.current.transform.forward, HandleUtility.GetHandleSize (positionerElement.positions[i].rootPosition + treeFactory.transform.position) * 0.1f);
					#if UNITY_5_5_OR_NEWER || UNITY_5_5
					Handles.ArrowHandleCap (0, positionerElement.positions[i].rootPosition + treeFactory.transform.position, 
						Quaternion.LookRotation (positionerElement.positions[i].rootDirection), 
						HandleUtility.GetHandleSize (positionerElement.positions[i].rootPosition + treeFactory.transform.position) * 0.7f, EventType.Repaint);
					#else
					Handles.ArrowCap (0, position.rootPosition + treeFactory.transform.position, 
						Quaternion.LookRotation (position.rootDirection), 
						HandleUtility.GetHandleSize (position.rootPosition + treeFactory.transform.position) * 0.7f);
					#endif
				}
			} else if (structureGeneratorElement != null) {
				if (structureGeneratorElement.rootStructureLevel.radius > 0) {
					Handles.DrawWireArc (treeFactory.transform.position,
						GlobalSettings.againstGravityDirection,
						Vector3.right,
						360,
						structureGeneratorElement.rootStructureLevel.radius);
				}
			}
			Handles.color = Color.yellow;
			Handles.DrawWireDisc (treeFactory.transform.position,
				Camera.current.transform.forward, 
				HandleUtility.GetHandleSize (treeFactory.transform.position) * GlobalSettings.treeFactoryPositionGizmoSize);
		}
		/// <summary>
		/// Raises the inspector GU event.
		/// </summary>
		public override void OnInspectorGUI() {
			EditorGUI.BeginDisabledGroup (Broccoli.TreeNodeEditor.TreeFactoryEditorWindow.IsOpen ());
			if (GUILayout.Button ("Open Tree Editor Window", GUILayout.Width (255))) {
				Broccoli.TreeNodeEditor.TreeFactoryEditorWindow.OpenTreeFactoryWindow (treeFactory);
			}
			EditorGUI.EndDisabledGroup ();
		}
		#endregion
	}
}
