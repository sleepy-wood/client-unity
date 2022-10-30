using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Base;
using Broccoli.Generator;
using Broccoli.Pipe;
using Broccoli.NodeEditorFramework;
using Broccoli.Utils;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Sprout generator node editor.
	/// </summary>
	[CustomEditor(typeof(SproutGeneratorNode))]
	public class SproutGeneratorNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The sprout generator node.
		/// </summary>
		public SproutGeneratorNode sproutGeneratorNode;
		/// <summary>
		/// The sprout seed list.
		/// </summary>
		ReorderableList sproutSeedList;

		SerializedProperty propMaxFrequency;
		SerializedProperty propMinFrequency;
		SerializedProperty propDistribution;
		SerializedProperty propDistributionSpacingVariance;
		SerializedProperty propDistributionAngleVariance;
		SerializedProperty propWhorledStep;
		SerializedProperty propDistributionCurve;
		SerializedProperty propMinTwirl;
		SerializedProperty propMaxTwirl;

		SerializedProperty propMinParallelAlignAtTop;
		SerializedProperty propMaxParallelAlignAtTop;
		SerializedProperty propMinParallelAlignAtBase;
		SerializedProperty propMaxParallelAlignAtBase;
		SerializedProperty propParallelAlignCurve;
		SerializedProperty propMinGravityAlignAtTop;
		SerializedProperty propMaxGravityAlignAtTop;
		SerializedProperty propMinGravityAlignAtBase;
		SerializedProperty propMaxGravityAlignAtBase;
		SerializedProperty propGravityAlignCurve;
		SerializedProperty propFromBranchCenter;

		SerializedProperty propDistributionOrigin;
		SerializedProperty propSpreadEnabled;
		SerializedProperty propSpreadRange;
		SerializedProperty propSproutSeeds;
		#endregion

		#region Messages
		private static string MSG_FREQUENCY = "Number of sprouts to generate on a branch lineage.";
		private static string MSG_DISTRIBUTION = "Distribution of the sprouts along the branches.";
		private static string MSG_DISTRIBUTION_SPACING_VARIANCE = "Adds spacing variance between sprouts along the parent branch.";
		private static string MSG_DISTRIBUTION_ANGLE_VARIANCE = "Add angle variance between sprouts along the parent branch.";
		private static string MSG_WHORLED_STEP = "Number of sprouts per node whirl.";
		private static string MSG_DISTRIBUTION_CURVE = "Curve to spatially spawn the sprout nodes along the branch lineages.";
		private static string MSG_TWIRL = "Rotation angle on the spawned elements taking the parent branch direction as axis.";
		private static string MSG_DISTRIBUTION_ORIGIN = "Point of origin for the spawned sprout nodes.";
		private static string MSG_SPREAD_ENABLED = "If enabled then the sprouts spread beyond their origin branch.";
		private static string MSG_SPREAD_RANGE = "How far the sprouts spread from their origin branch, from 0 to 1.";
		private static string MSG_SPROUT_GROUP = "Group the seed belongs to.";
		private static string MSG_PARALLEL_ALIGN_AT_TOP = "Value of direction alignment for spawned element following " +
			"their parent branch direction at the top end of it.";
		private static string MSG_PARALLEL_ALIGN_AT_BASE = "Value of direction alignment for spawned element following " +
			"their parent branch direction at the base end of it.";
		private static string MSG_PARALLEL_ALIGN_CURVE = "Parallel alignment distribution curve from base to top of the parent branch.";
		private static string MSG_GRAVITY_ALIGN_AT_TOP = "Value of direction alignment for spawned element against " +
			"gravity at the top end of the parent branch.";
		private static string MSG_GRAVITY_ALIGN_AT_BASE = "Value of direction alignment for spawned element against " +
			"gravity at the base end of the parent branch.";
		private static string MSG_GRAVITY_ALIGN_CURVE = "Gravity alignment distribution curve from base to top of the parent branch.";
		private static string MSG_FROM_BRANCH_CENTER = "Enabled if the seed generates from the center of the branch.";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			sproutGeneratorNode = target as SproutGeneratorNode;

			SetPipelineElementProperty ("sproutGeneratorElement");
			propMaxFrequency = GetSerializedProperty ("maxFrequency");
			propMinFrequency = GetSerializedProperty ("minFrequency");
			propDistribution = GetSerializedProperty ("distribution");
			propDistributionSpacingVariance = GetSerializedProperty ("distributionSpacingVariance");
			propDistributionAngleVariance = GetSerializedProperty ("distributionAngleVariance");
			propWhorledStep = GetSerializedProperty ("whorledStep");
			propDistributionCurve = GetSerializedProperty ("distributionCurve");
			propMinTwirl = GetSerializedProperty ("minTwirl");
			propMaxTwirl = GetSerializedProperty ("maxTwirl");

			propMinParallelAlignAtTop = GetSerializedProperty ("minParallelAlignAtTop");
			propMaxParallelAlignAtTop = GetSerializedProperty ("maxParallelAlignAtTop");
			propMinParallelAlignAtBase = GetSerializedProperty ("minParallelAlignAtBase");
			propMaxParallelAlignAtBase = GetSerializedProperty ("maxParallelAlignAtBase");
			propParallelAlignCurve = GetSerializedProperty ("parallelAlignCurve");
			propMinGravityAlignAtTop = GetSerializedProperty ("minGravityAlignAtTop");
			propMaxGravityAlignAtTop = GetSerializedProperty ("maxGravityAlignAtTop");
			propMinGravityAlignAtBase = GetSerializedProperty ("minGravityAlignAtBase");
			propMaxGravityAlignAtBase = GetSerializedProperty ("maxGravityAlignAtBase");
			propGravityAlignCurve = GetSerializedProperty ("gravityAlignCurve");
			propFromBranchCenter = GetSerializedProperty ("fromBranchCenter");

			propDistributionOrigin = GetSerializedProperty ("distributionOrigin");
			propSpreadEnabled = GetSerializedProperty ("spreadEnabled");
			propSpreadRange = GetSerializedProperty ("spreadRange");

			propSproutSeeds = GetSerializedProperty ("sproutSeeds");
			sproutSeedList = new ReorderableList (serializedObject, propSproutSeeds, false, true, true, true);
			sproutSeedList.draggable = false;
			sproutSeedList.drawHeaderCallback += DrawListItemHeader;
			sproutSeedList.drawElementCallback += DrawListItemElement;
			sproutSeedList.onAddCallback += AddListItem;
			sproutSeedList.onRemoveCallback += RemoveListItem;
		}
		/// <summary>
		/// Raises the scene GUI event.
		/// </summary>
		/// <param name="sceneView">Scene view.</param>
		protected override void OnSceneGUI (SceneView sceneView) {
			Handles.color = Color.yellow;
			TreeEditorUtils.DrawTreeSproutsForStructureLevel (
				sproutGeneratorNode.pipelineElement.GetInstanceID (), 
				TreeFactoryEditorWindow.editorWindow.treeFactory.previewTree,
				TreeFactoryEditorWindow.editorWindow.treeFactory.GetPreviewTreeWorldOffset (),
				TreeFactoryEditorWindow.editorWindow.treeFactory.treeFactoryPreferences.factoryScale);
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

			EditorGUILayout.IntSlider (propMaxFrequency, 0, 100, "Max Frequency");
			EditorGUILayout.IntSlider (propMinFrequency, 0, 100, "Min Frequency");
			ShowHelpBox (MSG_FREQUENCY);
			EditorGUILayout.Space ();

			// Distribution
			int distribution = propDistribution.enumValueIndex;
			EditorGUILayout.PropertyField (propDistribution);
			ShowHelpBox (MSG_DISTRIBUTION);
			if ((int)SproutGenerator.Distribution.Whorled == distribution) {
				EditorGUILayout.PropertyField (propWhorledStep);
				ShowHelpBox (MSG_WHORLED_STEP);
			}
			EditorGUILayout.Slider (propDistributionSpacingVariance, 0f, 1f);
			ShowHelpBox (MSG_DISTRIBUTION_SPACING_VARIANCE);
			EditorGUILayout.Slider (propDistributionAngleVariance, 0f, 1f);
			ShowHelpBox (MSG_DISTRIBUTION_ANGLE_VARIANCE);
			EditorGUILayout.PropertyField (propDistributionCurve);
			ShowHelpBox (MSG_DISTRIBUTION_CURVE);
			//EditorGUILayout.Slider (propTwirl, -1f, 1f, "Twirl");
			FloatRangePropertyField (propMinTwirl, propMaxTwirl, -1f,1f, "Twirl");
			ShowHelpBox (MSG_TWIRL);
			EditorGUILayout.Space ();

			// Parallel alignment relative to branch.
			//EditorGUILayout.Slider (propParallelAlignAtTop, -1f, 1f);
			FloatRangePropertyField (propMinParallelAlignAtTop, propMaxParallelAlignAtTop, -1f, 1f, "Parallel Align at Top");
			ShowHelpBox (MSG_PARALLEL_ALIGN_AT_TOP);
			//EditorGUILayout.Slider (propParallelAlignAtBase, -1f, 1f);
			FloatRangePropertyField (propMinParallelAlignAtBase, propMaxParallelAlignAtBase, -1f, 1f, "Parallel Align at Base");
			ShowHelpBox (MSG_PARALLEL_ALIGN_AT_BASE);
			EditorGUILayout.PropertyField (propParallelAlignCurve);
			ShowHelpBox (MSG_PARALLEL_ALIGN_CURVE);

			// Gravity alignment.
			//EditorGUILayout.Slider (propGravityAlignAtTop, -1f, 1f);
			FloatRangePropertyField (propMinGravityAlignAtTop, propMaxGravityAlignAtTop, -1f, 1f, "Gravity Align at Top");
			ShowHelpBox (MSG_GRAVITY_ALIGN_AT_TOP);
			//EditorGUILayout.Slider (propGravityAlignAtBase, -1f, 1f);
			FloatRangePropertyField (propMinGravityAlignAtBase, propMaxGravityAlignAtBase, -1f, 1f, "Gravity Align at Base");
			ShowHelpBox (MSG_GRAVITY_ALIGN_AT_BASE);
			EditorGUILayout.PropertyField (propGravityAlignCurve);
			ShowHelpBox (MSG_GRAVITY_ALIGN_CURVE);
			EditorGUILayout.Space ();

			// Sprout origin.
			EditorGUILayout.PropertyField (propFromBranchCenter);
			ShowHelpBox (MSG_FROM_BRANCH_CENTER);
			EditorGUILayout.Space ();

			// Distribution origin
			EditorGUILayout.PropertyField (propDistributionOrigin);
			ShowHelpBox (MSG_DISTRIBUTION_ORIGIN);

			// Spread Enabled
			bool spreadEnabled = propSpreadEnabled.boolValue;
			EditorGUILayout.PropertyField (propSpreadEnabled);
			ShowHelpBox (MSG_SPREAD_ENABLED);
			if (spreadEnabled) {
				EditorGUILayout.Slider (propSpreadRange, 0f, 1f);
				ShowHelpBox (MSG_SPREAD_RANGE);
			}
			EditorGUILayout.Space ();

			//NodeEditorGUI.BeginUsingDefaultSkin ();
			sproutSeedList.DoLayoutList ();
			//NodeEditorGUI.EndUsingSkin ();
			EditorGUILayout.Space ();
			// Seed options.
			DrawSeedOptions ();

			if (EditorGUI.EndChangeCheck () && 
				propMaxFrequency.intValue >= propMinFrequency.intValue) 
			{
				ApplySerialized ();
				UpdatePipeline (GlobalSettings.processingDelayMedium);
				SetUndoControlCounter ();
				NodeEditorFramework.NodeEditor.RepaintClients ();
			}

			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		#endregion

		#region Sprout Seeds Ordereable List
		/// <summary>
		/// Draws the list item header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawListItemHeader(Rect rect)
		{
			GUI.Label(rect, "Sprout Seeds");
		}
		/// <summary>
		/// Draws the list item element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			var sproutSeed = sproutSeedList.serializedProperty.GetArrayElementAtIndex (index);
			int groupId = sproutSeed.FindPropertyRelative ("groupId").intValue;
			Color groupColor = sproutGeneratorNode.sproutGeneratorElement.GetSproutGroupColor (groupId);
			rect.y += 2;
			EditorGUI.DrawRect (new Rect (rect.x, rect.y, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), groupColor);
			rect.x += 22;
			if (groupId <= 0) {
				EditorGUI.LabelField (new Rect (rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight), "Seed unassigned to group");
			} else {
				EditorGUI.LabelField (new Rect (rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight), "Seed on group " + groupId);
			}

			if (isActive) {
				EditorGUILayout.Space ();

				// Sprout group.
				int sproutGroupIndex = EditorGUILayout.Popup ("Sprout Group",
					sproutGeneratorNode.pipelineElement.pipeline.sproutGroups.GetSproutGroupIndex (groupId),
					sproutGeneratorNode.pipelineElement.pipeline.sproutGroups.GetPopupOptions ());
				int selectedSproutGroupId = 
					sproutGeneratorNode.pipelineElement.pipeline.sproutGroups.GetSproutGroupId (sproutGroupIndex);
				if (groupId != selectedSproutGroupId) {
					sproutSeed.FindPropertyRelative ("groupId").intValue = selectedSproutGroupId;
				}
				ShowHelpBox (MSG_SPROUT_GROUP);
			}
		}
		/// <summary>
		/// Adds the list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void AddListItem(ReorderableList list)
		{
			SproutSeed sproutSeed = new SproutSeed ();
			sproutGeneratorNode.sproutGeneratorElement.AddSproutSeed (sproutSeed);
			EditorUtility.SetDirty (target);
		}
		/// <summary>
		/// Removes the list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void RemoveListItem(ReorderableList list)
		{
			sproutGeneratorNode.sproutGeneratorElement.RemoveSproutSeed (list.index);
			EditorUtility.SetDirty (target);
		}
		#endregion
	}
}