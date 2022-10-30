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
	[CustomEditor(typeof(SproutFactory))]
	public class SproutFactoryEditor : Editor {
		#region Vars
		public SproutFactory sproutFactory;
		#endregion

		#region Events
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		void OnEnable () {
			sproutFactory = target as SproutFactory;
			if (Broccoli.BroccoEditor.SproutFactoryEditorWindow.IsOpen ()) {
				Broccoli.BroccoEditor.SproutFactoryEditorWindow.OpenSproutFactoryWindow (sproutFactory);
			}

			Tools.hidden = true;
		}
		/// <summary>
		/// Raises the disable event.
		/// </summary>
		void OnDisable () {
			sproutFactory = null;

			Tools.hidden = false;
		}
		/// <summary>
		/// Raises the scene GU event.
		/// </summary>
		protected void OnSceneGUI () {
			/*
			Handles.color = Color.yellow;
			if (positionerEnabled && positionerElement.useCustomPositions && positionerElement.HasValidPosition ()) {
				for (int i = 0; i < positionerElement.positions.Count; i++) {
					if (positionerElement.positions[i].enabled) {
						Handles.color = Color.yellow;
					} else {
						Handles.color = Color.gray;
					}
					Handles.DrawSolidDisc (positionerElement.positions[i].rootPosition + sproutFactory.transform.position,
						Camera.current.transform.forward, HandleUtility.GetHandleSize (positionerElement.positions[i].rootPosition + sproutFactory.transform.position) * 0.1f);
					#if UNITY_5_5_OR_NEWER || UNITY_5_5
					Handles.ArrowHandleCap (0, positionerElement.positions[i].rootPosition + sproutFactory.transform.position, 
						Quaternion.LookRotation (positionerElement.positions[i].rootDirection), 
						HandleUtility.GetHandleSize (positionerElement.positions[i].rootPosition + sproutFactory.transform.position) * 0.7f, EventType.Repaint);
					#else
					Handles.ArrowCap (0, position.rootPosition + treeFactory.transform.position, 
						Quaternion.LookRotation (position.rootDirection), 
						HandleUtility.GetHandleSize (position.rootPosition + treeFactory.transform.position) * 0.7f);
					#endif
				}
			} else if (structureGeneratorElement != null) {
				if (structureGeneratorElement.rootStructureLevel.radius > 0) {
					Handles.DrawWireArc (sproutFactory.transform.position,
						GlobalSettings.againstGravityDirection,
						Vector3.right,
						360,
						structureGeneratorElement.rootStructureLevel.radius);
				}
			}
			Handles.color = Color.yellow;
			Handles.DrawWireDisc (sproutFactory.transform.position,
				Camera.current.transform.forward, 
				HandleUtility.GetHandleSize (sproutFactory.transform.position) * GlobalSettings.treeFactoryPositionGizmoSize);
				*/
		}
		/// <summary>
		/// Raises the inspector GU event.
		/// </summary>
		public override void OnInspectorGUI() {
			/*
            if (GUILayout.Button ("Open Sprout Lab Window", GUILayout.Width (255))) {
				Debug.Log ("Opening SproutLab");
			}
			*/
			EditorGUI.BeginDisabledGroup (Broccoli.BroccoEditor.SproutFactoryEditorWindow.IsOpen ());
			if (GUILayout.Button ("Open Sprout Lab Window", GUILayout.Width (255))) {
				Broccoli.BroccoEditor.SproutFactoryEditorWindow.OpenSproutFactoryWindow (sproutFactory);
			}
			EditorGUI.EndDisabledGroup ();
		}
		#endregion
	}
}
