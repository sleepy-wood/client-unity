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
	/// Baker node editor.
	/// </summary>
	[CustomEditor(typeof(BakerNode))]
	public class BakerNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The positioner node.
		/// </summary>
		public BakerNode bakerNode;
		
		SerializedProperty propEnableAO;
		SerializedProperty propEnableAOInPreview;
		SerializedProperty propEnableAOAtRuntime;
		SerializedProperty propSamplesAO;
		SerializedProperty propStrengthAO;
		SerializedProperty propLodFade;
		SerializedProperty propLodFadeAnimate;
		SerializedProperty propLodTransitionWidth;
		GUIContent lodFadingGUIContent = new GUIContent ("LOD Fading Mode");
		GUIContent lodFadingAnimateGUIContent = new GUIContent ("LOD Fading Animation");
		#endregion

		#region Messages
		private static string MSG_ENABLE_AO = "Enables ambient occlusion baked on the final prefab mesh.";
		private static string MSG_ENABLE_AO_IN_PREVIEW = "Enable ambient occlusion when previewing the tree in the editor.";
		private static string MSG_ENABLE_AO_AT_RUNTIME = "Enable ambient occlusion when creating trees at runtime. Baking ambient occlusion to the mesh at runtime is processing intensive.";
		private static string MSG_SAMPLES_AO = "Enables this position to be a possible point of origin for a tree.";
		private static string MSG_STRENGTH_AO = "Amount of ambient occlusion to bake into the mesh.";
		private static string MSG_LOD_FADE = "LOD transition mode on the final prefab.";
		private static string MSG_LOD_FADE_ANIMATE = "LOD transition mode animation enabled or disabled.";
		private static string MSG_LOD_TRANSITION_WIDTH = "Transition value to cross-fade between elements within the LOD group.";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			bakerNode = target as BakerNode;

			SetPipelineElementProperty ("bakerElement");

			propEnableAO = GetSerializedProperty ("enableAO");
			propEnableAOInPreview = GetSerializedProperty ("enableAOInPreview");
			propEnableAOAtRuntime = GetSerializedProperty ("enableAOAtRuntime");
			propSamplesAO = GetSerializedProperty ("samplesAO");
			propStrengthAO = GetSerializedProperty ("strengthAO");
			propLodFade = GetSerializedProperty ("lodFade");
			propLodFadeAnimate = GetSerializedProperty ("lodFadeAnimate");
			propLodTransitionWidth = GetSerializedProperty ("lodTransitionWidth");
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			UpdateSerialized ();

			// Log box.
			DrawLogBox ();

			EditorGUI.BeginChangeCheck ();

			EditorGUILayout.LabelField ("Ambient Occlusion", EditorStyles.boldLabel);
			// Enables AO baking on the final prefab mesh.
			EditorGUILayout.PropertyField (propEnableAO);
			ShowHelpBox (MSG_ENABLE_AO);
			if (propEnableAO.boolValue) {
				// AO Samples.
				EditorGUILayout.IntSlider (propSamplesAO, 1, 8);
				ShowHelpBox (MSG_SAMPLES_AO);
				// AO Strength.
				EditorGUILayout.Slider (propStrengthAO, 0f, 1f);
				ShowHelpBox (MSG_STRENGTH_AO);
				// Enables AO in the preview tree of the editor.
				EditorGUILayout.PropertyField (propEnableAOInPreview);
				ShowHelpBox (MSG_ENABLE_AO_IN_PREVIEW);
				// Enables AO at runtime.
				EditorGUILayout.PropertyField (propEnableAOAtRuntime);
				ShowHelpBox (MSG_ENABLE_AO_AT_RUNTIME);
			}

			if (EditorGUI.EndChangeCheck ()) {
				UpdatePipeline (GlobalSettings.processingDelayLow);
				ApplySerialized ();
				bakerNode.bakerElement.Validate ();
			}
			EditorGUILayout.Space ();

			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.LabelField ("LOD Options", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField (propLodFade, lodFadingGUIContent);
			ShowHelpBox (MSG_LOD_FADE);
			EditorGUILayout.PropertyField (propLodFadeAnimate, lodFadingAnimateGUIContent);
			ShowHelpBox (MSG_LOD_FADE_ANIMATE);
			EditorGUILayout.Slider (propLodTransitionWidth, 0f, 1f, "Transition Width");
			ShowHelpBox (MSG_LOD_TRANSITION_WIDTH);
			if (EditorGUI.EndChangeCheck ()) {
				ApplySerialized ();
				bakerNode.bakerElement.Validate ();
			}
			EditorGUILayout.Space ();

			// Seed options.
			//DrawSeedOptions ();
			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		#endregion
	}
}