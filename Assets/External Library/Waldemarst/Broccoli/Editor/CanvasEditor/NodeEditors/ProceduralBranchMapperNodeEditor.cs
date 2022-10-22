using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Broccoli.Base;
using Broccoli.Factory;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Branch mapper node editor.
	/// </summary>
	[CustomEditor(typeof(ProceduralBranchMapperNode))]
	public class ProceduralBranchMapperNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The branch mapper node.
		/// </summary>
		public ProceduralBranchMapperNode proceduralBranchMapperNode;
		SerializedProperty propMaterialMode;
		SerializedProperty propCustomMaterial;
		//SerializedProperty propMainTexture;
		//SerializedProperty propNormalTexture;
		SerializedProperty propMappingXDisplacement;
		SerializedProperty propMappingYDisplacement;
		SerializedProperty propIsGirthSensitive;
		SerializedProperty propApplyMappingOffsetFromParent;
		#endregion

		#region Serialized property
		SerializedProperty propMappingMode;
		SerializedProperty propGridSize;
		SerializedProperty propGridTextureMode;
		SerializedProperty propGridTextureFile;
		#endregion

		#region Messages
		//private static string MSG_MAPPING_MODE = "";
		private static string MSG_GRID_SIZE = "";
		private static string MSG_GRID_TEXTURE_MODE = "";
		private static string MSG_GRID_TEXTURE_FILE = "";
		/*
		private static string MSG_MATERIAL_MODE = "Material mode to apply.";
		private static string MSG_CUSTOM_MATERIAL = "Material applied to the branches.";
		private static string MSG_MAIN_TEXTURE = "Main texture for the generated material.";
		private static string MSG_NORMAL_TEXTURE = "Normal map texture for the generated material.";
		private static string MSG_MAPPING_X_DISP = "Girth to be used at the base of the tree trunk.";
		private static string MSG_MAPPING_Y_DISP = "Girth to be used at the tip of a terminal branch.";
		private static string MSG_GIRTH_SENSITIVE = "UV mapping is smaller at lower values of girth on the branches.";
		private static string MSG_APPLY_PARENT_OFFSET = "Children branches get an UV mapping offset from their parent branch position.";
		*/
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			proceduralBranchMapperNode = target as ProceduralBranchMapperNode;

			SetPipelineElementProperty ("proceduralBranchMapperElement");

			propMappingMode = GetSerializedProperty ("mappingMode");
			propGridSize = GetSerializedProperty ("gridSize");
			propGridTextureMode = GetSerializedProperty ("gridTextureMode");
			propGridTextureFile = GetSerializedProperty ("gridTextureFile");

			/*
			propMaterialMode = GetSerializedProperty ("materialMode");
			propCustomMaterial = GetSerializedProperty ("customMaterial");
			*/
			
			/*
			propMainTexture = GetSerializedProperty ("mainTexture");
			propNormalTexture = GetSerializedProperty ("normalTexture");
			*/

			/*
			propMappingXDisplacement = GetSerializedProperty ("mappingXDisplacement");
			propMappingYDisplacement = GetSerializedProperty ("mappingYDisplacement");
			propIsGirthSensitive = GetSerializedProperty ("isGirthSensitive");
			propApplyMappingOffsetFromParent = GetSerializedProperty ("applyMappingOffsetFromParent");
			*/
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			CheckUndoRequest ();

			UpdateSerialized ();

			// Log box.
			DrawLogBox ();

			bool elementChanged = false;

			/*
			int mappingMode = propMappingMode.enumValueIndex;
			EditorGUILayout.PropertyField (propMappingMode);
			ShowHelpBox (MSG_MAPPING_MODE);
			EditorGUILayout.Space ();
			*/			

			// GRID MODE
			if (propMappingMode.enumValueIndex == (int)ProceduralBranchMapperElement.MappingMode.Grid) {
				int gridSize = propGridSize.enumValueIndex;
				EditorGUILayout.PropertyField (propGridSize);
				ShowHelpBox (MSG_GRID_SIZE);
				EditorGUILayout.Space ();

				int gridTextureMode = propGridTextureMode.enumValueIndex;
				EditorGUILayout.PropertyField (propGridTextureMode);
				ShowHelpBox (MSG_GRID_TEXTURE_MODE);
				EditorGUILayout.Space ();

				if (propGridTextureMode.enumValueIndex == (int) ProceduralBranchMapperElement.GridTextureMode.File) {
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (propGridTextureFile);
					if (EditorGUI.EndChangeCheck ()) {
						elementChanged = true;
					}
					ShowHelpBox (MSG_GRID_TEXTURE_FILE);
				}

				if (gridSize != (int)propGridSize.enumValueIndex ||
					gridTextureMode != (int)propGridTextureMode.enumValueIndex ||
					elementChanged) 
				{
					ApplySerialized ();
					/*
					UpdateComponent ((int)ProceduralBranchMapperComponent.ComponentCommand.SetUVs, 
						GlobalSettings.processingDelayLow);
						*/
					SetUndoControlCounter ();
				}
				EditorGUILayout.Space ();

			}

			/*
			int materialModeIndex = propMaterialMode.enumValueIndex;
			EditorGUILayout.PropertyField (propMaterialMode);
			ShowHelpBox (MSG_MATERIAL_MODE);
			EditorGUILayout.Space ();

			if (materialModeIndex == (int)ProceduralBranchMapperElement.MaterialMode.Custom) {
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (propCustomMaterial);
				ShowHelpBox (MSG_CUSTOM_MATERIAL);
				if (EditorGUI.EndChangeCheck () ||
				    materialModeIndex != propMaterialMode.enumValueIndex) {
					ApplySerialized ();
					UpdateComponent ((int)ProceduralBranchMapperComponent.ComponentCommand.BuildMaterials, 
						GlobalSettings.processingDelayMedium);
					// TODO: update with pink material when no material is set.
					SetUndoControlCounter ();
				}
			} else if (materialModeIndex == (int)ProceduralBranchMapperElement.MaterialMode.Texture) {
				if (materialModeIndex != propMaterialMode.enumValueIndex) {
					ApplySerialized ();
					UpdateComponent ((int)ProceduralBranchMapperComponent.ComponentCommand.BuildMaterials, 
						GlobalSettings.processingDelayLow);
					SetUndoControlCounter ();
				}
			}
			*/
			/*
			float textureXDisplacement = propMappingXDisplacement.floatValue;
			EditorGUILayout.Slider (propMappingXDisplacement, -5f, 5f, "Mapping X Displacement");
			ShowHelpBox (MSG_MAPPING_X_DISP);


			float textureYDisplacement = propMappingYDisplacement.floatValue;
			EditorGUILayout.Slider (propMappingYDisplacement, -5f, 5f, "Mapping Y Displacement");
			ShowHelpBox (MSG_MAPPING_Y_DISP);

			bool isGirthSensitive = propIsGirthSensitive.boolValue;
			EditorGUILayout.PropertyField (propIsGirthSensitive);
			ShowHelpBox (MSG_GIRTH_SENSITIVE);

			bool applyMappingOffsetFromParent = propApplyMappingOffsetFromParent.boolValue;
			EditorGUILayout.PropertyField (propApplyMappingOffsetFromParent);
			ShowHelpBox (MSG_APPLY_PARENT_OFFSET);
			*/

			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		#endregion
	}
}