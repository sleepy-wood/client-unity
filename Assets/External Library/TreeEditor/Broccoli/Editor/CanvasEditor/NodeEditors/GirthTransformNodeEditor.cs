using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Broccoli.Base;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Girth transform node editor.
	/// </summary>
	[CustomEditor(typeof(GirthTransformNode))]
	public class GirthTransformNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The girth transform node.
		/// </summary>
		public GirthTransformNode girthTransformNode;
		/// <summary>
		/// The girth curve range.
		/// </summary>
		private static Rect girthCurveRange = new Rect (0f, 0f, 1f, 1f);

		SerializedProperty propMinGirthAtBase;
		SerializedProperty propMaxGirthAtBase;
		SerializedProperty propMinGirthAtTop;
		SerializedProperty propMaxGirthAtTop;
		SerializedProperty propGirthCurve;
		SerializedProperty propHierarchyScalingEnabled;
		SerializedProperty propMinHierarchyScaling;
		SerializedProperty propMaxHierarchyScaling;
		SerializedProperty propGirthAtRootBase;
		SerializedProperty propGirthAtRootBottom;
		SerializedProperty propGirthRootCurve;
		#endregion

		#region Messages
		private static string MSG_GIRTH_AT_BASE = "Girth to be used at the base of the tree trunk.";
		private static string MSG_GIRTH_AT_TOP = "Girth to be used at the tip of a terminal branch.";
		private static string MSG_CURVE = "Curve of girth values from tree trunk (base) " +
			"to the tip of a terminal branch (top).";
		private static string MSG_HIERARCHY_SCALING_ENABLED = "Adds girth scaling to terminal branches that come directly from the tree trunk.";
		//private static string MSG_MIN_HIERARCHY_SCALING = "";
		private static string MSG_MAX_HIERARCHY_SCALING = "Scaling for the girth on terminal branches coming out of the tree trunk.";
		private static string MSG_GIRTH_AT_ROOT_BASE = "Girth to be used at the base of the tree trunk.";
		private static string MSG_GIRTH_AT_ROOT_BOTTOM = "Girth to be used at the tip of a terminal branch.";
		private static string MSG_ROOT_CURVE = "Curve of girth values from tree trunk (base) " +
			"to the tip of a terminal branch (top).";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			girthTransformNode = target as GirthTransformNode;

			SetPipelineElementProperty ("GirthTransformElement");
			propMinGirthAtBase = GetSerializedProperty ("minGirthAtBase");
			propMaxGirthAtBase = GetSerializedProperty ("maxGirthAtBase");
			propMinGirthAtTop = GetSerializedProperty ("minGirthAtTop");
			propMaxGirthAtTop = GetSerializedProperty ("maxGirthAtTop");
			propGirthCurve = GetSerializedProperty ("curve");
			propHierarchyScalingEnabled = GetSerializedProperty ("hierarchyScalingEnabled");
			propMinHierarchyScaling = GetSerializedProperty ("minHierarchyScaling");
			propMaxHierarchyScaling = GetSerializedProperty ("maxHierarchyScaling");
			propGirthAtRootBase = GetSerializedProperty ("girthAtRootBase");
			propGirthAtRootBottom = GetSerializedProperty ("girthAtRootBottom");
			propGirthRootCurve = GetSerializedProperty ("rootCurve");
		}
		/// <summary>
		/// Raises the inspector GU event.
		/// </summary>
		public override void OnInspectorGUI() {
			CheckUndoRequest ();

			UpdateSerialized ();
			EditorGUILayout.LabelField ("Branches", EditorStyles.boldLabel);

			bool girthChanged = false;
			EditorGUI.BeginChangeCheck ();
			FloatRangePropertyField (propMinGirthAtBase, propMaxGirthAtBase, 
				0.01f, 3.5f, "Girth at Base");
			ShowHelpBox (MSG_GIRTH_AT_BASE);
			FloatRangePropertyField (propMinGirthAtTop, propMaxGirthAtTop, 
				0.01f, 3.5f, "Girth at Top");
			ShowHelpBox (MSG_GIRTH_AT_TOP);
			if (EditorGUI.EndChangeCheck ()) {
				girthChanged = true;
			}

			bool curveChanged = false;
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.CurveField (propGirthCurve, Color.green, girthCurveRange);
			ShowHelpBox (MSG_CURVE);
			if (EditorGUI.EndChangeCheck ()) {
				curveChanged = true;
			}

			bool hierarchyScaleChanged = false;
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (propHierarchyScalingEnabled);
			ShowHelpBox (MSG_HIERARCHY_SCALING_ENABLED);

			if (propHierarchyScalingEnabled.boolValue) {
				EditorGUILayout.Slider (propMaxHierarchyScaling, 0.01f, 1f, "Hierarchy Scaling");
				ShowHelpBox (MSG_MAX_HIERARCHY_SCALING);
				EditorGUILayout.Space ();
				/*
				EditorGUILayout.Slider (propMinHierarchyScaling, 0.01f, 1f, "Min Hierarchy Scaling");
				ShowHelpBox (MSG_MIN_HIERARCHY_SCALING);
				EditorGUILayout.Space ();
				*/
			}

			if (EditorGUI.EndChangeCheck () && 
				propMaxHierarchyScaling.floatValue >= propMinHierarchyScaling.floatValue) {
				hierarchyScaleChanged = true;
			}
			EditorGUILayout.Space ();

			EditorGUILayout.LabelField ("Roots", EditorStyles.boldLabel);
			float girthAtRootBase = propGirthAtRootBase.floatValue;
			EditorGUILayout.Slider (propGirthAtRootBase, 0.01f, 3.5f, "Girth at Root Base");
			ShowHelpBox (MSG_GIRTH_AT_ROOT_BASE);

			float girthAtRootBottom = propGirthAtRootBottom.floatValue;
			EditorGUILayout.Slider (propGirthAtRootBottom, 0.01f, 3.5f, "Girth at Root Bottom");
			ShowHelpBox (MSG_GIRTH_AT_ROOT_BOTTOM);

			bool rootCurveChanged = false;
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.CurveField (propGirthRootCurve, Color.green, girthCurveRange);
			ShowHelpBox (MSG_ROOT_CURVE);
			if (EditorGUI.EndChangeCheck ()) {
				curveChanged = true;
			}

			ApplySerialized ();

			if (girthChanged ||
				curveChanged ||
				girthAtRootBase != propGirthAtRootBase.floatValue ||
				girthAtRootBottom != propGirthAtRootBottom.floatValue ||
				rootCurveChanged || hierarchyScaleChanged) {
				UpdatePipeline (GlobalSettings.processingDelayHigh);
				girthTransformNode.GirthTransformElement.Validate ();
				SetUndoControlCounter ();

			}
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();

			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		#endregion
	}
}