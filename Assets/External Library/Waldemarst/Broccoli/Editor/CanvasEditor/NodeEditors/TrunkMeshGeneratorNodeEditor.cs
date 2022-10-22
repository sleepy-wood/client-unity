using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.Builder;
using Broccoli.Model;
using Broccoli.Utils;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Trunk mesh generator node editor.
	/// </summary>
	[CustomEditor(typeof(TrunkMeshGeneratorNode))]
	public class TrunkMeshGeneratorNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The trunk mesh generator node.
		/// </summary>
		public TrunkMeshGeneratorNode trunkMeshGeneratorNode;
		SerializedProperty propRootMode;
		SerializedProperty propMinSpread;
		SerializedProperty propMaxSpread;
		SerializedProperty propMinDisplacementPoints;
		SerializedProperty propMaxDisplacementPoints;
		SerializedProperty propMinDisplacementAngleVariance;
		SerializedProperty propMaxDisplacementAngleVariance;
		SerializedProperty propMinDisplacementTwirl;
		SerializedProperty propMaxDisplacementTwirl;
		SerializedProperty propMinDisplacementScaleAtBase;
		SerializedProperty propMaxDisplacementScaleAtBase;
		SerializedProperty propScaleCurve;
		SerializedProperty propRadialResolutionFactor;
		SerializedProperty propLengthResolutionFactor;
		/// <summary>
		/// The scale curve range.
		/// </summary>
		private static Rect scaleCurveRange = new Rect (0f, 0f, 1f, 1f);
		#endregion

		#region Messages
		private static string MSG_ROOT_MODE = "Mode to mesh roots on the tree trunk.\n1. Pseudo: simulates root bumps on the trunk mesh." +
			"\n2. Integration Or Pseudo: integration is the preferred mode, if no root structures are found then pseudo mode is set." +
			"\n3. Integration: integrates existing roots from the tree structure into the tree trunk mesh.";
		private static string MSG_MIN_MAX_SPREAD = "Range along the trunk the mesh will take.";
		private static string MSG_MIN_MAX_POINTS = "Displacement points around the trunk, reminiscent of roots comming from the trunk.";
		private static string MSG_MIN_MAX_ANGLE_VARIANCE = "Angle variance between displacement points.";
		private static string MSG_MIN_MAX_TWIRL = "Twirl fo the displacement points around the trunk.";
		private static string MSG_MIN_MAX_SCALE_AT_BASE = "Scale applied at the girth of the base of the trunk.";
		private static string MSG_SCALE_CURVE = "Distribution of the girth scaling along the trunk.";
		private static string MSG_RADIAL_RESOLUTION_FACTOR = "Factor to multiply the number of radial segment at the trunk.";
		private static string MSG_LENGTH_RESOLUTION_FACTOR = "Factory to multiply the number of cross-sections along the trunk length.";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			trunkMeshGeneratorNode = target as TrunkMeshGeneratorNode;

			SetPipelineElementProperty ("trunkMeshGeneratorElement");

			propRootMode = GetSerializedProperty ("rootMode");
			propMinSpread = GetSerializedProperty ("minSpread");
			propMaxSpread = GetSerializedProperty ("maxSpread");
			propMinDisplacementPoints = GetSerializedProperty ("minDisplacementPoints");
			propMaxDisplacementPoints = GetSerializedProperty ("maxDisplacementPoints");
			propMinDisplacementAngleVariance = GetSerializedProperty ("minDisplacementAngleVariance");
			propMaxDisplacementAngleVariance = GetSerializedProperty ("maxDisplacementAngleVariance");
			propMinDisplacementTwirl = GetSerializedProperty ("minDisplacementTwirl");
			propMaxDisplacementTwirl = GetSerializedProperty ("maxDisplacementTwirl");
			propMinDisplacementScaleAtBase = GetSerializedProperty ("minDisplacementScaleAtBase");
			propMaxDisplacementScaleAtBase = GetSerializedProperty ("maxDisplacementScaleAtBase");
			propScaleCurve = GetSerializedProperty ("scaleCurve");
			propRadialResolutionFactor = GetSerializedProperty ("radialResolutionFactor");
			propLengthResolutionFactor = GetSerializedProperty ("lengthResolutionFactor");
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			CheckUndoRequest ();

			UpdateSerialized ();

			EditorGUI.BeginChangeCheck ();

			EditorGUILayout.PropertyField (propRootMode);
			ShowHelpBox (MSG_ROOT_MODE);
			EditorGUILayout.Space ();

			if (propRootMode.enumValueIndex == (int)TrunkMeshGeneratorElement.RootMode.Pseudo ||
				propRootMode.enumValueIndex == (int)TrunkMeshGeneratorElement.RootMode.IntegrationOrPseudo) {
				EditorGUILayout.LabelField ("Trunk Shape", EditorStyles.boldLabel);
				// Spread
				FloatRangePropertyField (propMinSpread, propMaxSpread, 0f, 1f, "Spread");
				ShowHelpBox (MSG_MIN_MAX_SPREAD);

				// Displacement points
				IntRangePropertyField (propMinDisplacementPoints, propMaxDisplacementPoints, 3, 10, "Points");
				ShowHelpBox (MSG_MIN_MAX_POINTS);

				// Displacement point angle variance
				FloatRangePropertyField (propMinDisplacementAngleVariance, propMaxDisplacementAngleVariance, 0f, 0.5f, "Angle Variance");
				ShowHelpBox (MSG_MIN_MAX_ANGLE_VARIANCE);

				// Twirl
				FloatRangePropertyField (propMinDisplacementTwirl, propMaxDisplacementTwirl, -2.5f, 2.5f, "Twirl");
				ShowHelpBox (MSG_MIN_MAX_TWIRL);

				// Scale at Base
				FloatRangePropertyField (propMinDisplacementScaleAtBase, propMaxDisplacementScaleAtBase, 1f, 3f, "Scale at Base");
				ShowHelpBox (MSG_MIN_MAX_SCALE_AT_BASE);

				// Scale Curve
				EditorGUILayout.CurveField (propScaleCurve, Color.green, scaleCurveRange);
				ShowHelpBox (MSG_SCALE_CURVE);
				EditorGUILayout.Space ();

				EditorGUILayout.LabelField ("Mesh Resolution", EditorStyles.boldLabel);
				// Min Max Polygon Sides.
				EditorGUILayout.Slider (propRadialResolutionFactor, 1f, 3f, "Radial Resolution Factor");
				ShowHelpBox (MSG_RADIAL_RESOLUTION_FACTOR);

				EditorGUILayout.Slider (propLengthResolutionFactor, 1f, 4f, "Length Resolution Factor");
				ShowHelpBox (MSG_LENGTH_RESOLUTION_FACTOR);
			}

			if (EditorGUI.EndChangeCheck () &&
				propMinSpread.floatValue <= propMaxSpread.floatValue) {
				ApplySerialized ();
				UpdatePipeline (GlobalSettings.processingDelayHigh);
				NodeEditorFramework.NodeEditor.RepaintClients ();
				SetUndoControlCounter ();
			}
			EditorGUILayout.Space ();

			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		/// <summary>
		/// Raises the scene GUI event.
		/// </summary>
		/// <param name="sceneView">Scene view.</param>
		protected override void OnSceneGUI (SceneView sceneView) {
			/*
			BranchMeshBuilder branchMeshBuilder = BranchMeshBuilder.GetInstance ();
			TrunkMeshBuilder trunkMeshBuilder = (TrunkMeshBuilder) branchMeshBuilder.GetBranchMeshBuilder (BranchMeshBuilder.BuilderType.Trunk);
			BezierCurve bezierCurve = null;
			var enumerator = trunkMeshBuilder.baseCurves.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				bezierCurve = enumerator.Current.Value;
				break;
			}
			BezierCurveDraw.DrawCurve (bezierCurve, Vector3.zero, 3, Color.white, 2);
			//BezierCurveDraw.DrawCurvePoints (bezierCurve, Vector3.zero, 3, Color.white);
			BezierCurveDraw.DrawCurvePoints (bezierCurve, Vector3.zero, 3, Color.white);
			//BezierCurveDraw.DrawCurveFinePoints (bezierCurve, Vector3.zero, 3, Color.white);
			*/
		}
		#endregion
	}
}