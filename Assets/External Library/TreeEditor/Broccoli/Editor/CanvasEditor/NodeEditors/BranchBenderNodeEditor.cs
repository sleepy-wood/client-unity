using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Broccoli.Base;
using Broccoli.Utils;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Branch bender node editor.
	/// </summary>
	[CustomEditor(typeof(BranchBenderNode))]
	public class BranchBenderNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The branch bender node.
		/// </summary>
		public BranchBenderNode branchBenderNode;
		SerializedProperty propApplyJointSmoothing;
		SerializedProperty propApplyDirectionalBending;
		SerializedProperty propApplyNoise;
		SerializedProperty propSmoothJointStrength;
		SerializedProperty propForceAtTips;
		SerializedProperty propForceAtTrunk;
		SerializedProperty propHierarchyDistributionCurve;
		SerializedProperty propHorizontalAlignAtBase;
		SerializedProperty propHorizontalAlignStrength;
		SerializedProperty propHorizontalAlignHierarchyDistributionCurve;
		SerializedProperty propVerticalAlignAtTop;
		SerializedProperty propVerticalAlignStrength;
		SerializedProperty propVerticalAlignHierarchyDistributionCurve;
		SerializedProperty propNoiseAtTop;
		SerializedProperty propNoiseAtBase;
		SerializedProperty propNoiseScaleAtTop;
		SerializedProperty propNoiseScaleAtBase;
		SerializedProperty propSmoothRootJointStrength;
		SerializedProperty propForceAtRootTips;
		SerializedProperty propForceAtRootTrunk;
		SerializedProperty propRootHierarchyDistributionCurve;
		SerializedProperty propNoiseAtRootBase;
		SerializedProperty propNoiseAtRootBottom;
		SerializedProperty propNoiseScaleAtRootBase;
		SerializedProperty propNoiseScaleAtRootBottom;
		private static Rect curveRange = new Rect (0f, 0f, 1f, 1f);
		int selectedToolbar = 0;
		#endregion

		#region Messages
		private static string MSG_APPLY_SMOOTHING = "Check to smooth the transition areas between parent and child branches or roots.";
		private static string MSG_APPLY_DIRECTIONAL_BENDING = "Check to bend branches and roots relative to gravity.";
		private static string MSG_APPLY_NOISE = "Check to apply Perlin noise to branches and roots.";
		private static string MSG_SMOOTHING_STRENGTH = "Strength applied to the curve between a parent and a followup branch.";
		private static string MSG_FORCE_AT_TIPS = "Amount of bending force applied to terminal branches.";
		private static string MSG_FORCE_AT_TRUNK = "Amount of bending force applied to branches near the trunk.";
		private static string MSG_HIERARCHY_DISTRIBUTION_CURVE = "Distribution of bending from branches closer to the trunk and those afar.";
		private static string MSG_HORIZONTAL_ALIGN_AT_BASE = "How much the base of the branches are align to the horizon plane.";
		private static string MSG_HORIZONTAL_ALIGN_STRENGTH = "Horizontal alignment strength used to lerp from the original direction to the horizon plane.";
		private static string MSG_HORIZONTAL_ALIGN_HIERARCHY_CURVE = "Distribution of the horizontal alignment along the tree hierarchy.";
		private static string MSG_VERTICAL_ALIGN_AT_TOP = "How much the top of the branches are align with the gravity direction.";
		private static string MSG_VERTICAL_ALIGN_STRENGTH = "Vertical alignment strength used to lerp from the original direction to the gravity direction.";
		private static string MSG_VERTICAL_ALIGN_HIERARCHY_CURVE = "Distribution of the vertical alignment along the tree hierarchy.";
		private static string MSG_NOISE = "Amount of offset noise to apply to branches.";
		private static string MSG_NOISE_SCALE = "Perlin noise scale. Lower values gives more resolution to the noise.";
		private static string MSG_ROOT_SMOOTHING_STRENGTH = "Strength applied to the curve between a parent and a followup root.";
		private static string MSG_FORCE_AT_ROOT_TIPS = "Amount of bending force applied to terminal roots.";
		private static string MSG_FORCE_AT_ROOT_TRUNK = "Amount of bending force applied to roots near the trunk.";
		private static string MSG_ROOT_HIERARCHY_DISTRIBUTION_CURVE = "Distribution of bending from roots closer to the trunk and those afar.";
		private static string MSG_ROOT_NOISE = "Amount of offset noise to apply to roots.";
		private static string MSG_ROOT_NOISE_SCALE = "Perlin noise scale. Lower values gives more resolution to the noise.";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			branchBenderNode = target as BranchBenderNode;

			SetPipelineElementProperty ("branchBenderElement");

			propApplyJointSmoothing = GetSerializedProperty ("applyJointSmoothing");
			propApplyDirectionalBending  = GetSerializedProperty ("applyDirectionalBending");
			propApplyNoise = GetSerializedProperty ("applyNoise");

			propSmoothJointStrength = GetSerializedProperty ("smoothJointStrength");
			propForceAtTips = GetSerializedProperty ("forceAtTips");
			propForceAtTrunk = GetSerializedProperty ("forceAtTrunk");
			propHierarchyDistributionCurve = GetSerializedProperty ("hierarchyDistributionCurve");
			propHorizontalAlignAtBase = GetSerializedProperty ("horizontalAlignAtBase");
			propHorizontalAlignStrength = GetSerializedProperty ("horizontalAlignStrength");
			propHorizontalAlignHierarchyDistributionCurve = GetSerializedProperty ("horizontalAlignHierarchyDistributionCurve");
			propVerticalAlignAtTop = GetSerializedProperty ("verticalAlignAtTop");
			propVerticalAlignStrength = GetSerializedProperty ("verticalAlignStrength");
			propVerticalAlignHierarchyDistributionCurve = GetSerializedProperty ("verticalAlignHierarchyDistributionCurve");

			propNoiseAtBase = GetSerializedProperty ("noiseAtBase");
			propNoiseAtTop = GetSerializedProperty ("noiseAtTop");
			propNoiseScaleAtBase = GetSerializedProperty ("noiseScaleAtBase");
			propNoiseScaleAtTop = GetSerializedProperty ("noiseScaleAtTop");

			propSmoothRootJointStrength = GetSerializedProperty ("smoothRootJointStrength");
			propForceAtRootTips = GetSerializedProperty ("forceAtRootTips");
			propForceAtRootTrunk = GetSerializedProperty ("forceAtRootTrunk");
			propRootHierarchyDistributionCurve = GetSerializedProperty ("rootHierarchyDistributionCurve");
			propNoiseAtRootBase = GetSerializedProperty ("noiseAtRootBase");
			propNoiseAtRootBottom = GetSerializedProperty ("noiseAtRootBottom");
			propNoiseScaleAtRootBase = GetSerializedProperty ("noiseScaleAtRootBase");
			propNoiseScaleAtRootBottom = GetSerializedProperty ("noiseScaleAtRootBottom");
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			CheckUndoRequest ();

			UpdateSerialized ();

			EditorGUI.BeginChangeCheck ();

			// Joint smoothing
			EditorGUILayout.PropertyField (propApplyJointSmoothing);
			ShowHelpBox (MSG_APPLY_SMOOTHING);
			// Directional bending
			EditorGUILayout.PropertyField (propApplyDirectionalBending);
			ShowHelpBox (MSG_APPLY_DIRECTIONAL_BENDING);
			EditorGUILayout.PropertyField (propApplyNoise);
			ShowHelpBox (MSG_APPLY_NOISE);
			EditorGUILayout.Space ();

			if (propApplyJointSmoothing.boolValue || propApplyDirectionalBending.boolValue || propApplyNoise.boolValue) {
				selectedToolbar = GUILayout.Toolbar (selectedToolbar, new string[] {"Branches", "Roots"});
				if (selectedToolbar == 0) {
					// BRANCH JOINT SMOOTHING
					if (propApplyJointSmoothing.boolValue) {
						EditorGUILayout.LabelField ("Branch Joint Smoothing", EditorStyles.boldLabel);
						EditorGUILayout.Slider (propSmoothJointStrength, 0f, 1f, "Smoothing Strength");
						ShowHelpBox (MSG_SMOOTHING_STRENGTH);
						EditorGUILayout.Space ();
					}
					// BRANCH DIRECTIONAL BENDING
					if (propApplyDirectionalBending.boolValue) {
						EditorGUILayout.LabelField ("Branch Directional Bending", EditorStyles.boldLabel);
						EditorGUILayout.Slider (propForceAtTips, -1f, 1f, "Force at Tips");
						ShowHelpBox (MSG_FORCE_AT_TIPS);
						EditorGUILayout.Slider (propForceAtTrunk, -1f, 1f, "Force at Trunk");
						ShowHelpBox (MSG_FORCE_AT_TRUNK);
						EditorGUILayout.CurveField (propHierarchyDistributionCurve, Color.green, curveRange);
						ShowHelpBox (MSG_HIERARCHY_DISTRIBUTION_CURVE);
						EditorGUILayout.Space ();
						EditorGUILayout.LabelField ("Branch Directional Alignment", EditorStyles.boldLabel);
						EditorGUILayout.Slider (propHorizontalAlignAtBase, -1f, 1f, "H Align at Base");
						ShowHelpBox (MSG_HORIZONTAL_ALIGN_AT_BASE);
						EditorGUILayout.Slider (propHorizontalAlignStrength, 0f, 1f, "H Align Strength");
						ShowHelpBox (MSG_HORIZONTAL_ALIGN_STRENGTH);
						EditorGUILayout.CurveField (propHorizontalAlignHierarchyDistributionCurve, Color.green, curveRange);
						ShowHelpBox (MSG_HORIZONTAL_ALIGN_HIERARCHY_CURVE);
						EditorGUILayout.Slider (propVerticalAlignAtTop, -1f, 1f, "V Align at Base");
						ShowHelpBox (MSG_VERTICAL_ALIGN_AT_TOP);
						EditorGUILayout.Slider (propVerticalAlignStrength, 0f, 1f, "V Align Strength");
						ShowHelpBox (MSG_VERTICAL_ALIGN_STRENGTH);
						EditorGUILayout.CurveField (propVerticalAlignHierarchyDistributionCurve, Color.green, curveRange);
						ShowHelpBox (MSG_VERTICAL_ALIGN_HIERARCHY_CURVE);
						EditorGUILayout.Space ();
					}
					// BRANCH NOISE
					if (propApplyNoise.boolValue) {
						EditorGUILayout.LabelField ("Branch Noise", EditorStyles.boldLabel);
						EditorGUILayout.Slider (propNoiseAtBase, 0f, 1f, "Noise at Base");
						EditorGUILayout.Slider (propNoiseAtTop, 0f, 1f, "Noise at Top");
						ShowHelpBox (MSG_NOISE);
						EditorGUILayout.Slider (propNoiseScaleAtBase, 0f, 1f, "Noise Scale at Base");
						EditorGUILayout.Slider (propNoiseScaleAtTop, 0f, 1f, "Noise Scale at Top");
						ShowHelpBox (MSG_NOISE_SCALE);
						EditorGUILayout.Space ();
					}
				} else {
					// ROOT JOINT SMOOTHING
					if (propApplyJointSmoothing.boolValue) {
						EditorGUILayout.LabelField ("Root Joint Smoothing", EditorStyles.boldLabel);
						EditorGUILayout.Slider (propSmoothRootJointStrength, 0f, 1f, "Smoothing Strength");
						ShowHelpBox (MSG_ROOT_SMOOTHING_STRENGTH);
						EditorGUILayout.Space ();
					}
					if (propApplyDirectionalBending.boolValue) {
						EditorGUILayout.LabelField ("Root Directional Bending", EditorStyles.boldLabel);
						EditorGUILayout.Slider (propForceAtRootTips, -1f, 1f, "Force at Tips");
						ShowHelpBox (MSG_FORCE_AT_ROOT_TIPS);
						EditorGUILayout.Slider (propForceAtRootTrunk, -1f, 1f, "Force at Trunk");
						ShowHelpBox (MSG_FORCE_AT_ROOT_TRUNK);
						EditorGUILayout.CurveField (propRootHierarchyDistributionCurve, Color.green, curveRange);
						ShowHelpBox (MSG_ROOT_HIERARCHY_DISTRIBUTION_CURVE);
						EditorGUILayout.Space ();
					}
					if (propApplyNoise.boolValue) {
						EditorGUILayout.LabelField ("Root Noise", EditorStyles.boldLabel);
						EditorGUILayout.Slider (propNoiseAtRootBase, 0f, 1f, "Noise at Base");
						EditorGUILayout.Slider (propNoiseAtRootBottom, 0f, 1f, "Noise at Bottom");
						ShowHelpBox (MSG_ROOT_NOISE);
						EditorGUILayout.Slider (propNoiseScaleAtRootBase, 0f, 1f, "Noise Scale at Base");
						EditorGUILayout.Slider (propNoiseScaleAtRootBottom, 0f, 1f, "Noise Scale at Bottom");
						ShowHelpBox (MSG_ROOT_NOISE_SCALE);
						EditorGUILayout.Space ();
					}
				}
			}

			// Seed options.
			DrawSeedOptions ();

			if (EditorGUI.EndChangeCheck ()) {
				ApplySerialized ();
				UpdatePipeline (GlobalSettings.processingDelayHigh);
				branchBenderNode.branchBenderElement.Validate ();
				SetUndoControlCounter ();
			}

			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		#endregion
	}
}