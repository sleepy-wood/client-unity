using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Broccoli.Base;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Length transform node editor.
	/// </summary>
	[CustomEditor(typeof(LengthTransformNode))]
	public class LengthTransformNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The length transform node.
		/// </summary>
		public LengthTransformNode lengthTransformNode;

		SerializedProperty propLevelCurve;
		SerializedProperty propPositionCurve;
		SerializedProperty propMinFactor;
		SerializedProperty propMaxFactor;

		/// <summary>
		/// The level curve range.
		/// </summary>
		private static Rect levelCurveRange = new Rect (0f, 0f, 1f, 1f);
		/// <summary>
		/// The position curve range.
		/// </summary>
		private static Rect positionCurveRange = new Rect (0f, 0f, 1f, 1f);
		#endregion

		#region Messages
		private static string MSG_MAX_FACTOR = "Maximum range value for length factor.";
		private static string MSG_MIN_FACTOR = "Minium range value for length factor.";
		private static string MSG_LEVEL_CURVE = "Distribution curve of values based on the hierarchical " +
			"position of the branch (from trunk to tip branches).";
		private static string MSG_POSITION_CURVE = "Distribution curve of values based on the position of " +
			"a branch along its parent branch (at base, at top).";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			lengthTransformNode = target as LengthTransformNode;

			SetPipelineElementProperty ("lengthTransformElement");
			propLevelCurve = GetSerializedProperty ("levelCurve");
			propPositionCurve = GetSerializedProperty ("positionCurve");
			propMinFactor = GetSerializedProperty ("minFactor");
			propMaxFactor = GetSerializedProperty ("maxFactor");
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			CheckUndoRequest ();

			UpdateSerialized ();

			// Log box.
			DrawLogBox ();

			EditorGUI.BeginChangeCheck ();

			float minFactor = propMinFactor.floatValue;
			float maxFactor = propMaxFactor.floatValue;
			
			// Factor
			EditorGUILayout.Slider (propMaxFactor, 0.1f, 5f);
			maxFactor = propMaxFactor.floatValue;
			ShowHelpBox (MSG_MAX_FACTOR);
			EditorGUILayout.Slider (propMinFactor, 0.1f, 5f);
			minFactor = propMinFactor.floatValue;
			ShowHelpBox (MSG_MIN_FACTOR);
				
			EditorGUILayout.Space ();

			// Level curve
			EditorGUILayout.CurveField (propLevelCurve, Color.green, levelCurveRange);
			ShowHelpBox (MSG_LEVEL_CURVE);
			EditorGUILayout.Space ();

			// Position curve
			EditorGUILayout.CurveField (propPositionCurve, Color.green, positionCurveRange);
			ShowHelpBox (MSG_POSITION_CURVE);
			EditorGUILayout.Space ();

			// Seed options.
			DrawSeedOptions ();

			if (EditorGUI.EndChangeCheck () &&
				minFactor <= maxFactor) {
				ApplySerialized ();
				UpdatePipeline (GlobalSettings.processingDelayHigh);
				lengthTransformNode.lengthTransformElement.Validate ();
				SetUndoControlCounter ();
			}

			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		#endregion
	}
}