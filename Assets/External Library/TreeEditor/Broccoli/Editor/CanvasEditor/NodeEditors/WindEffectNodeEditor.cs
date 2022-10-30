using UnityEditor;

using Broccoli.Base;
using Broccoli.Controller;
using Broccoli.Manager;
using Broccoli.Factory;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Wind effect node editor.
	/// </summary>
	[CustomEditor(typeof(WindEffectNode))]
	public class WindEffectNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The wind effect node.
		/// </summary>
		public WindEffectNode windEffectNode;
		SerializedProperty propWindSpread;
		SerializedProperty propWindAmplitude;
		SerializedProperty propSproutTurbulence;
		SerializedProperty propSproutSway;
		SerializedProperty propBranchSway;
		SerializedProperty propPreviewWindAlways;
		SerializedProperty propUseMultiPhaseOnTrunk;
		SerializedProperty propWindQuality;
		bool shouldUpdateController = false;
		#endregion

		#region Messages
		private static string MSG_WIND_SPREAD = "Spread of the wind effect along the branches.";
		private static string MSG_WIND_AMPLITUDE = "How much the branches swing with the wind.";
		private static string MSG_SPROUT_TURBULENCE = "Turbulence effect on the sprouts.";
		private static string MSG_SPROUT_SWAY = "Swinging from side to side on the sprouts following the wind direction.";
		private static string MSG_BRANCH_SWAY = "Swinging from side to side on the branches following the wind direction.";
		private static string MSG_PREVIEW_WIND_ALWAYS = "Keeps the wind animation going even if this node is not selected.";
		private static string MSG_ENABLE_ANIMATED = "In order to preview the wind effect please make sure the scene has a WindZone object " +
			"and \"Animated Materials\" on the Scene View panel is enabled. This implementation has support for directional wind zones only.";
		private static string MSG_MULTI_PHASE = "The sway effector is also applied to the maisn branch derived from the tree trunk. Applies only to ST8 shader.";
		private static string MSG_WIND_QUALITY = "Wind quality to set on the shader.";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			windEffectNode = target as WindEffectNode;

			if (TreeFactory.GetActiveInstance() != null && 
				TreeFactory.GetActiveInstance ().previewTree != null) {
				BroccoTreeController treeController = 
					TreeFactory.GetActiveInstance ().previewTree.obj.GetComponent<BroccoTreeController> ();
				if (treeController != null) {
					treeController.shaderType = (BroccoTreeController.ShaderType)MaterialManager.leavesShaderType;
					treeController.windQuality = (BroccoTreeController.WindQuality)windEffectNode.windEffectElement.windQuality;
					treeController.sproutTurbulance = windEffectNode.windEffectElement.sproutTurbulence;
					treeController.sproutSway = windEffectNode.windEffectElement.sproutSway;
					treeController.editorWindAlways = windEffectNode.windEffectElement.previewWindAlways;
					treeController.EnableEditorWind (true);
				}
			}

			SetPipelineElementProperty ("windEffectElement");
			propWindSpread = GetSerializedProperty ("windSpread");
			propWindAmplitude = GetSerializedProperty ("windAmplitude");
			propSproutTurbulence = GetSerializedProperty ("sproutTurbulence");
			propSproutSway = GetSerializedProperty ("sproutSway");
			propBranchSway = GetSerializedProperty ("branchSway");
			propPreviewWindAlways = GetSerializedProperty ("previewWindAlways");
			//propWindFactorCurve = GetSerializedProperty ("windFactorCurve");
			propUseMultiPhaseOnTrunk = GetSerializedProperty ("useMultiPhaseOnTrunk");
			propWindQuality = GetSerializedProperty ("windQuality");
		}
		/// <summary>
		/// Raises the disable specific event.
		/// </summary>
		protected override void OnDisableSpecific () {
			if (TreeFactory.GetActiveInstance () != null &&
			    TreeFactory.GetActiveInstance ().previewTree != null) {
				BroccoTreeController treeController = 
					TreeFactory.GetActiveInstance ().previewTree.obj.GetComponent<BroccoTreeController> ();
				if (treeController != null) {
					treeController.shaderType = (BroccoTreeController.ShaderType)MaterialManager.leavesShaderType;
					treeController.windQuality = (BroccoTreeController.WindQuality)windEffectNode.windEffectElement.windQuality;
					treeController.sproutTurbulance = windEffectNode.windEffectElement.sproutTurbulence;
					treeController.sproutSway = windEffectNode.windEffectElement.sproutSway;
					treeController.EnableEditorWind (false);
				}
			}
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			CheckUndoRequest ();

			UpdateSerialized ();

			shouldUpdateController = false;

			EditorGUILayout.HelpBox (MSG_ENABLE_ANIMATED, MessageType.None);
			EditorGUILayout.Space ();

			EditorGUI.BeginChangeCheck ();

			EditorGUILayout.Slider (propWindSpread, 0f, 2f, "Wind Spread");
			ShowHelpBox (MSG_WIND_SPREAD);

			EditorGUILayout.Slider (propWindAmplitude, 0f, 3f, "Wind Amplitude");
			ShowHelpBox (MSG_WIND_AMPLITUDE);
			EditorGUILayout.Space ();
			
			EditorGUILayout.Slider (propSproutTurbulence, 0f, 2f, "Sprout Turbulence");
			ShowHelpBox (MSG_SPROUT_TURBULENCE);

			EditorGUILayout.Slider (propSproutSway, 0f, 2f, "Sprout Sway");
			ShowHelpBox (MSG_SPROUT_SWAY);
			EditorGUILayout.Space ();

			EditorGUILayout.Slider (propBranchSway, 0f, 4f, "Branch Sway");
			ShowHelpBox (MSG_BRANCH_SWAY);
			EditorGUILayout.Space ();

			EditorGUILayout.PropertyField (propWindQuality);
			ShowHelpBox (MSG_WIND_QUALITY);
			EditorGUILayout.Space ();

			EditorGUILayout.PropertyField (propUseMultiPhaseOnTrunk);
			ShowHelpBox (MSG_MULTI_PHASE);
			EditorGUILayout.Space ();

			if (EditorGUI.EndChangeCheck ()) {
				ApplySerialized ();
				shouldUpdateController = true;
				UpdatePipeline (GlobalSettings.processingDelayMedium, true);
				//UpdateComponent (0,	GlobalSettings.processingDelayLow); //TODO
				SetUndoControlCounter ();
			}

			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (propPreviewWindAlways);
			if (EditorGUI.EndChangeCheck ()) {
				ApplySerialized ();
				shouldUpdateController = true;
			}
			ShowHelpBox (MSG_PREVIEW_WIND_ALWAYS);
			EditorGUILayout.Space ();

			if (shouldUpdateController) {
				TreeFactory treeFactory = TreeFactory.GetActiveInstance ();
				BroccoTreeController treeController = 
					treeFactory.previewTree.obj.GetComponent<BroccoTreeController> ();
				if (treeController != null) {
					treeController.editorWindAlways = propPreviewWindAlways.boolValue;
					treeController.shaderType = (BroccoTreeController.ShaderType)MaterialManager.leavesShaderType;
					treeController.windQuality = (BroccoTreeController.WindQuality)windEffectNode.windEffectElement.windQuality;
					treeController.sproutTurbulance = windEffectNode.windEffectElement.sproutTurbulence;
					treeController.sproutSway = windEffectNode.windEffectElement.sproutSway;
					treeController.localWindAmplitude = windEffectNode.windEffectElement.windAmplitude;
					treeController.EnableEditorWind (true);
				}
			}

			DrawFieldHelpOptions ();
		}
		#endregion
	}
}