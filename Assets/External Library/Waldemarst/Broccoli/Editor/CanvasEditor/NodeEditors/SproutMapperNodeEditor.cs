using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.Component;
using Broccoli.Manager;
using Broccoli.Factory;
using Broccoli.BroccoEditor;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Sprout mapper node editor.
	/// </summary>
	[CustomEditor(typeof(SproutMapperNode))]
	public class SproutMapperNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The sprout mapper node.
		/// </summary>
		public SproutMapperNode sproutMapperNode;
		/// <summary>
		/// The maps list.
		/// </summary>
		ReorderableList mapsList;
		/// <summary>
		/// The areas list.
		/// </summary>
		ReorderableList areasList;
		/// <summary>
		/// The property sprout maps.
		/// </summary>
		SerializedProperty propSproutMaps;
		/// <summary>
		/// The property sprout areas.
		/// </summary>
		SerializedProperty propSproutAreas;
		/*
		/// <summary>
		/// The color variations to use on sprout meshes.
		/// </summary>
		SerializedProperty propCustomColors;
		*/
		/// <summary>
		/// Current variance mode to apply to sprout meshes.
		/// </summary>
		SerializedProperty propColorVarianceMode;
		/// <summary>
		/// The changes are to be applied on the pipeline.
		/// </summary>
		private bool changesForPipeline = false;
		/// <summary>
		/// The changes are for only materials.
		/// </summary>
		private bool changesForMaterials = false;
		/// <summary>
		/// The changes are for the UV indexes.
		/// </summary>
		private bool changesForUVs = false;
		/// <summary>
		/// The changes are for meshes.
		/// </summary>
		private bool changesForMeshes = false;
		/// <summary>
		/// The area canvas.
		/// </summary>
		private SproutAreaCanvasEditor areaCanvas = new SproutAreaCanvasEditor ();
		/// <summary>
		/// The index of the current map.
		/// </summary>
		int currentMapIndex = -1;
		/// <summary>
		/// The current sprout map.
		/// </summary>
		SproutMap currentSproutMap = null;
		#endregion

		#region Messages
		/*
		private static string MSG_RM_TEX_TITLE = "Remove Texture";
		private static string MSG_RM_TEX_MSG = "Are you sure you want to remove this texture from the sprout mapping?";
		private static string MSG_RM_TEX_YES = "Remove";
		private static string MSG_RM_TEX_NO = "Do not remove";
		*/
		private static string MSG_OVERRIDE_IS_ON = "The tree factory overrides the custom material's shader with Unity's tree creator one. You " +
			"can turn this off on 'Advanced Preferences'.";
		private static string MSG_MATERIAL_OVERRIDE = "\"Material Overrive\" clones the assigned material to override its properties (like textures and normal maps). If your assigned material does not support double side rendering, please select \"TreeCreator\" as your preferred shader. To reflect changes made to the overrided material " +
			" please use the \"Update from Base Material\" button.";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			sproutMapperNode = target as SproutMapperNode;

			SetPipelineElementProperty ("sproutMapperElement");
			propSproutMaps = GetSerializedProperty ("sproutMaps");
			mapsList = new ReorderableList (serializedObject, propSproutMaps, false, true, true, true);
			mapsList.draggable = false;
			mapsList.drawHeaderCallback += DrawSproutMapHeader;
			mapsList.drawElementCallback += DrawSproutMapElement;
			mapsList.onSelectCallback += OnSelectMapItem;
			mapsList.onAddCallback += OnAddSproutMapItem;
			mapsList.onRemoveCallback += OnRemoveSproutMapItem;
		}
		/// <summary>
		/// Raises the disable specific event.
		/// </summary>
		protected override void OnDisableSpecific () {
			currentMapIndex = -1;
			currentSproutMap = null;
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			CheckUndoRequest ();

			UpdateSerialized ();

			// Log box.
			DrawLogBox ();

			EditorGUILayout.Space ();
			
			changesForPipeline = false;
			changesForMaterials = false;
			changesForUVs = false;
			changesForMeshes = false;

			// maps
			if (sproutMapperNode.sproutMapperElement.selectedMapIndex != mapsList.index &&
			    sproutMapperNode.sproutMapperElement.selectedMapIndex < mapsList.count) {
				mapsList.index = sproutMapperNode.sproutMapperElement.selectedMapIndex;
			}
			mapsList.DoLayoutList ();

			if (changesForPipeline) {
				ApplySerialized ();
				UpdatePipeline (GlobalSettings.processingDelayVeryHigh, true);
				sproutMapperNode.sproutMapperElement.Validate ();
				SetUndoControlCounter ();
			} else if (changesForMaterials) {
				ApplySerialized ();
				UpdateComponent ((int)SproutMapperComponent.ComponentCommand.BuildMaterials, 
					GlobalSettings.processingDelayVeryLow);
				sproutMapperNode.sproutMapperElement.Validate ();
				SetUndoControlCounter ();
			} else if (changesForUVs) {
				ApplySerialized ();
				UpdateComponent ((int)SproutMapperComponent.ComponentCommand.SetUVs, 
					GlobalSettings.processingDelayVeryLow);
				sproutMapperNode.sproutMapperElement.Validate ();
				SetUndoControlCounter ();
			} else if (changesForMeshes) {
				ApplySerialized ();
				UpdatePipelineUpstream (PipelineElement.ClassType.SproutMeshGenerator, 
					GlobalSettings.processingDelayVeryHigh);
				sproutMapperNode.sproutMapperElement.Validate ();
				SetUndoControlCounter ();
			}

			// SEED OPTIONS
			DrawSeedOptions ();
			// HELP OPTIONS
			DrawFieldHelpOptions ();
			// KEYNAME OPTIONS
			DrawKeyNameOptions ();
		}
		#endregion

		#region Sprout Maps
		/// <summary>
		/// Draws the list item header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawSproutMapHeader(Rect rect)
		{
			GUI.Label(rect, "Texture Maps");
		}
		/// <summary>
		/// Draws the sprout map element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawSproutMapElement (Rect rect, int index, bool isActive, bool isFocused) {
			var sproutMapProp = mapsList.serializedProperty.GetArrayElementAtIndex (index);

			int sproutGroupId = sproutMapProp.FindPropertyRelative ("groupId").intValue;
			if (sproutGroupId > 0) {
				rect.y += 2;
				SproutGroups sproutGroups = sproutMapperNode.sproutMapperElement.pipeline.sproutGroups;
				EditorGUI.DrawRect (new Rect (rect.x, rect.y, 
					EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), 
					sproutGroups.GetSproutGroupColor(sproutGroupId));
				rect.x += 22;
				EditorGUI.LabelField (new Rect (rect.x, rect.y, 
					150, EditorGUIUtility.singleLineHeight), "Assigned to group " + sproutGroupId);
			} else {
				rect.y += 2;
				EditorGUI.DrawRect (new Rect (rect.x, rect.y, 
					EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), 
					Color.black);
				rect.x += 22;
				EditorGUI.LabelField (new Rect (rect.x, rect.y, 
					150, EditorGUIUtility.singleLineHeight), "Unassigned map");
			}

			if (isActive) {
				SproutGroups sproutGroups = sproutMapperNode.sproutMapperElement.pipeline.sproutGroups;
				SproutGroups.SproutGroup sproutGroup = sproutGroups.GetSproutGroup (sproutGroupId);
				if (index != sproutMapperNode.sproutMapperElement.selectedMapIndex) {
					sproutMapperNode.sproutMapperElement.selectedMapIndex = index;
				}
				EditorGUILayout.Space ();

				// Sprout group.
				EditorGUI.BeginChangeCheck ();
				int sproutGroupIndex = EditorGUILayout.Popup ("Sprout Group",
					sproutMapperNode.pipelineElement.pipeline.sproutGroups.GetSproutGroupIndex (sproutGroupId, true),
					sproutMapperNode.pipelineElement.pipeline.sproutGroups.GetPopupOptions (true));
				int selectedSproutGroupId = 
					sproutMapperNode.pipelineElement.pipeline.sproutGroups.GetSproutGroupId (sproutGroupIndex);
				if (EditorGUI.EndChangeCheck() && sproutGroupId != selectedSproutGroupId) {
					if (sproutMapperNode.sproutMapperElement.GetSproutGroupsAssigned ().Contains (selectedSproutGroupId)) {
						Debug.LogWarning ("The sprout group has already been assigned to a material.");
					} else {
						sproutMapProp.FindPropertyRelative ("groupId").intValue = selectedSproutGroupId;
						changesForPipeline = true;
					}
				}
				if (sproutGroup != null && sproutGroup.branchCollection != null) {
					EditorGUILayout.HelpBox ("This Sprout Group uses a Branch Collection Scriptable Object to define its meshes. Textures will be taken from the collection as well.", MessageType.Info);
				} else {
					EditorGUI.BeginChangeCheck ();
					// Mode.
					SproutMap.Mode sproutMapMode = (SproutMap.Mode)sproutMapProp.FindPropertyRelative ("mode").enumValueIndex;
					EditorGUILayout.PropertyField(sproutMapProp.FindPropertyRelative ("mode"));
					if ((SproutMap.Mode)sproutMapProp.FindPropertyRelative ("mode").enumValueIndex == SproutMap.Mode.MaterialOverride) {
						// Changes for mode.
						if (EditorGUI.EndChangeCheck () ||
							sproutMapMode != (SproutMap.Mode)sproutMapProp.FindPropertyRelative ("mode").enumValueIndex) {
							changesForPipeline = true;
						} else {
							DrawSproutMapElementMaterialOverrideMode (sproutMapProp, index);
						}
					} else if ((SproutMap.Mode)sproutMapProp.FindPropertyRelative ("mode").enumValueIndex == SproutMap.Mode.Material) {
						// Changes for mode.
						if (EditorGUI.EndChangeCheck () ||
							sproutMapMode != (SproutMap.Mode)sproutMapProp.FindPropertyRelative ("mode").enumValueIndex) {
							changesForPipeline = true;
						} else {
							DrawSproutMapElementMaterialMode (sproutMapProp);
						}
					} else if ((SproutMap.Mode)sproutMapProp.FindPropertyRelative ("mode").enumValueIndex == SproutMap.Mode.Texture) {
						// Changes for mode.
						if (EditorGUI.EndChangeCheck () ||
							sproutMapMode != (SproutMap.Mode)sproutMapProp.FindPropertyRelative ("mode").enumValueIndex) {
							changesForPipeline = true;
						} else {
							DrawSproutMapElementTextureMode (sproutMapProp, index);
						}
					}
				}
				EditorGUILayout.Space ();
			}
		}
		/// <summary>
		/// Adds a list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnAddSproutMapItem(ReorderableList list)
		{
			if (sproutMapperNode.sproutMapperElement.CanAddSproutMap ()) {
				SproutMap sproutMap = new SproutMap ();
				Undo.RecordObject (sproutMapperNode.sproutMapperElement, "Sprout Map added");
				sproutMapperNode.sproutMapperElement.AddSproutMap (sproutMap);
			}
		}
		/// <summary>
		/// Event called when a map is selected from the list.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnSelectMapItem (ReorderableList list)
		{
			Undo.RecordObject (sproutMapperNode.sproutMapperElement, "Sprout Map selected");
			sproutMapperNode.sproutMapperElement.selectedMapIndex = list.index;
		}
		/// <summary>
		/// Removes a list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnRemoveSproutMapItem(ReorderableList list)
		{
			int undoGroup = Undo.GetCurrentGroup ();
			Undo.SetCurrentGroupName ("Sprout Map removed");
			Undo.RecordObject (sproutMapperNode.sproutMapperElement, "Sprout Map removed");
			sproutMapperNode.sproutMapperElement.sproutMaps.RemoveAt (list.index);
			Undo.RecordObject (sproutMapperNode.sproutMapperElement, "Sprout Map removed");
			sproutMapperNode.sproutMapperElement.selectedMapIndex = -1;
			Undo.CollapseUndoOperations (undoGroup);
			changesForMeshes = true;
		}
		/// <summary>
		/// Draws the sprout map element texture mode.
		/// </summary>
		/// <param name="sproutMapProp">Sprout map property.</param>
		private void DrawSproutMapElementTextureMode (SerializedProperty sproutMapProp, int index) {
			EditorGUI.BeginChangeCheck ();
			// Color
			EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("color"));
			// Color Variance.
			EditorGUI.BeginChangeCheck ();
			propColorVarianceMode = sproutMapProp.FindPropertyRelative ("colorVarianceMode");
			EditorGUILayout.PropertyField (propColorVarianceMode, new GUIContent ("Color Variance"));
			if (propColorVarianceMode.enumValueIndex == (int)SproutMap.ColorVarianceMode.Shades) {
				FloatRangePropertyField (sproutMapProp.FindPropertyRelative ("minColorShade"), 
					sproutMapProp.FindPropertyRelative ("maxColorShade"), 0.65f, 1f, "Shade Range");
			}
			if (EditorGUI.EndChangeCheck ()) {
				changesForPipeline = true;
			}
			// Alpha cutoff
			EditorGUILayout.Slider (sproutMapProp.FindPropertyRelative ("alphaCutoff"), 0f, 1f);
			/* TODO: remove tree creator.
			if (MaterialManager.leavesShaderType == MaterialManager.LeavesShaderType.TreeCreatorOrSimilar) {
				// Transparency
				EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("transparency"));
				// Translucency view dependency
				EditorGUILayout.Slider (sproutMapProp.FindPropertyRelative ("translucencyViewDependency"), 0f, 1f);
				// Shadow strength
				EditorGUILayout.Slider (sproutMapProp.FindPropertyRelative ("shadowStrength"), 0f, 1f);
				// Shadow offset scale
				EditorGUILayout.Slider (sproutMapProp.FindPropertyRelative ("shadowOffsetScale"), 0f, 10f);
			}
			*/
			if (MaterialManager.leavesShaderType == MaterialManager.LeavesShaderType.SpeedTree8OrSimilar) {
				EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("subsurfaceColor"));
				EditorGUILayout.Slider (sproutMapProp.FindPropertyRelative ("subsurfaceValue"), 0f, 1f);
				EditorGUILayout.Slider (sproutMapProp.FindPropertyRelative ("glossiness"), 0f, 1f);
				EditorGUILayout.Slider (sproutMapProp.FindPropertyRelative ("metallic"), 0f, 1f);
			}
			if (ExtensionManager.isHDRP) {
				SerializedProperty propDiffusionProfileSettings = sproutMapProp.FindPropertyRelative ("diffusionProfileSettings");
				ScriptableObject former = (ScriptableObject)propDiffusionProfileSettings.objectReferenceValue;
				former = 
					(ScriptableObject)EditorGUILayout.ObjectField (
						"Diffusion Profile", 
						former, 
						System.Type.GetType ("UnityEngine.Rendering.HighDefinition.DiffusionProfileSettings, Unity.RenderPipelines.HighDefinition.Runtime"), 
						false);
				if (former != (ScriptableObject)propDiffusionProfileSettings.objectReferenceValue) {
					propDiffusionProfileSettings.objectReferenceValue = former;
					changesForMaterials = true;
				}
			}
			// Texture Mode.
			/*
			SerializedProperty textureModeProp = sproutMapProp.FindPropertyRelative ("textureMode");
			EditorGUILayout.PropertyField (textureModeProp);
			if (textureModeProp.enumValueIndex == (int)SproutMap.TextureMode.PerGroup) {
				EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("texture"), new GUIContent ("Albedo Texture"));
				EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("normalMap"), new GUIContent ("Normal Texture"));
				EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("extraMap"), new GUIContent ("Extras Texture"));
				EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("subsurfaceMap"), new GUIContent ("Subsurface Texture"));
			}
			*/

			if (EditorGUI.EndChangeCheck ()) {
				changesForMaterials = true;
			}
			/*
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("vector4UV0Enabled"), new GUIContent ("Use Vector4 on UV0 channel"));
			if (EditorGUI.EndChangeCheck ()) {
				changesForMeshes = true;
			}
			*/

			SproutMap sproutMap = sproutMapperNode.sproutMapperElement.sproutMaps [index];
			EditorGUILayout.Space ();

			//Draw Areas
			DrawAreas (index, sproutMap, sproutMapProp);
		}
		/// <summary>
		/// Draws the sprout map element material mode.
		/// </summary>
		/// <param name="sproutMapProp">Sprout map property.</param>
		private void DrawSproutMapElementMaterialMode (SerializedProperty sproutMapProp) {
			// Changes for custom material.
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("customMaterial"));
			if (TreeFactory.GetActiveInstance ().treeFactoryPreferences.overrideMaterialShaderEnabled) {
				EditorGUILayout.HelpBox (MSG_OVERRIDE_IS_ON, MessageType.Info);
			}
			if (EditorGUI.EndChangeCheck ()) {
				changesForMaterials = true;
			}
		}
		/// <summary>
		/// Draws the sprout map element material override mode.
		/// </summary>
		/// <param name="sproutMapProp">Sprout map property.</param>
		private void DrawSproutMapElementMaterialOverrideMode (SerializedProperty sproutMapProp, int index) {
			// Changes for custom material.
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("customMaterial"));
			if (TreeFactory.GetActiveInstance ().treeFactoryPreferences.overrideMaterialShaderEnabled) {
				EditorGUILayout.HelpBox (MSG_OVERRIDE_IS_ON, MessageType.Info);
			}
			EditorGUILayout.HelpBox (MSG_MATERIAL_OVERRIDE, MessageType.Info);
			if (GUILayout.Button ("Update from Base Material")) {
				changesForMaterials = true;
			}
			EditorGUILayout.Space ();
			// Color
			EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("color"));
			// Alpha cutoff
			EditorGUILayout.Slider (sproutMapProp.FindPropertyRelative ("alphaCutoff"), 0f, 1f);

			if (EditorGUI.EndChangeCheck ()) {
				changesForMaterials = true;
			}
			/*
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (sproutMapProp.FindPropertyRelative ("vector4UV0Enabled"), new GUIContent ("Use Vector4 on UV0 channel"));
			if (EditorGUI.EndChangeCheck ()) {
				changesForMeshes = true;
			}
			*/

			SproutMap sproutMap = sproutMapperNode.sproutMapperElement.sproutMaps [index];
			EditorGUILayout.Space ();

			//Draw Areas
			DrawAreas (index, sproutMap, sproutMapProp);
		}
		#endregion

		#region Sprout Areas
		/// <summary>
		/// Draws the areas for a sprout map.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="sproutMap">Sprout map.</param>
		/// <param name="sproutMapProp">Sprout map property.</param>
		void DrawAreas (int index, SproutMap sproutMap, SerializedProperty sproutMapProp) {
			if (index != currentMapIndex) {
				currentMapIndex = index;
				currentSproutMap = sproutMap;
				propSproutAreas = sproutMapProp.FindPropertyRelative ("sproutAreas");
				areasList = new ReorderableList (serializedObject, propSproutAreas, false, true, true, true);
				areasList.draggable = false;
				areasList.drawHeaderCallback += DrawSproutAreaHeader;
				areasList.drawElementCallback += DrawSproutAreaElement;
				areasList.onSelectCallback += OnSelectSproutAreaItem;
				areasList.onAddCallback += OnAddSproutAreaItem;
				areasList.onRemoveCallback += OnRemoveSproutAreaItem;
			}
			if (areasList != null) {
				if (currentSproutMap.selectedAreaIndex != areasList.index &&
					currentSproutMap.selectedAreaIndex < mapsList.count) {
					areasList.index = currentSproutMap.selectedAreaIndex;
				}
				areasList.DoLayoutList ();
			}
		}
		/// <summary>
		/// Draws the sprout area header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawSproutAreaHeader(Rect rect)
		{
			GUI.Label(rect, "Textures");
		}
		/// <summary>
		/// Draws the sprout area element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawSproutAreaElement (Rect rect, int index, bool isActive, bool isFocused) {
			var sproutAreaProp = areasList.serializedProperty.GetArrayElementAtIndex (index);
			EditorGUI.LabelField (new Rect (rect.x, rect.y, 
					150, EditorGUIUtility.singleLineHeight), "Texture Area " + index);
			if (isActive) {
				if (index != currentSproutMap.selectedAreaIndex) {
					currentSproutMap.selectedAreaIndex = index;
				}
				EditorGUILayout.Space ();

				// Enabled
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (sproutAreaProp.FindPropertyRelative ("enabled"));
				if (EditorGUI.EndChangeCheck ()) {
					changesForMeshes = true;
				}

				Texture2D texture = null;
				SproutMap.SproutMapArea sproutArea = currentSproutMap.sproutAreas [index];

				if (currentSproutMap.textureMode == SproutMap.TextureMode.PerArea) {
					// Texture.
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (sproutAreaProp.FindPropertyRelative ("texture"), 
						new GUIContent ("Texture"));
					if (EditorGUI.EndChangeCheck ()) {
						changesForMaterials = true;
					}
					texture = sproutArea.texture;

					// Normal map.
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (sproutAreaProp.FindPropertyRelative ("normalMap"), 
						new GUIContent ("Normap Map"));
					if (EditorGUI.EndChangeCheck ()) {
						changesForMaterials = true;
					}

					if (MaterialManager.leavesShaderType == MaterialManager.LeavesShaderType.SpeedTree8OrSimilar) {
						// Extra map.
						EditorGUI.BeginChangeCheck ();
						EditorGUILayout.PropertyField (sproutAreaProp.FindPropertyRelative ("extraMap"), 
							new GUIContent ("Extra Map"));
						if (EditorGUI.EndChangeCheck ()) {
							changesForMaterials = true;
						}

						// Subsurface map.
						EditorGUI.BeginChangeCheck ();
						EditorGUILayout.PropertyField (sproutAreaProp.FindPropertyRelative ("subsurfaceMap"), 
							new GUIContent ("Subsurface Map"));
						if (EditorGUI.EndChangeCheck ()) {
							changesForMaterials = true;
						}
					}
				} else {
					// Texture Mode is Per Group
					texture = currentSproutMap.texture;
				}

				if (texture != null) {
					// x, y, width, height, pivot x, pivot y
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.Slider (sproutAreaProp.FindPropertyRelative ("x"), 0f, 1f, "Area X");
					EditorGUILayout.Slider (sproutAreaProp.FindPropertyRelative ("y"), 0f, 1f, "Area Y");
					EditorGUILayout.Slider (sproutAreaProp.FindPropertyRelative ("width"), 0f, 1f, "Area Width");
					EditorGUILayout.Slider (sproutAreaProp.FindPropertyRelative ("height"), 0f, 1f, "Area Height");
					EditorGUILayout.Slider (sproutAreaProp.FindPropertyRelative ("pivotX"), 0f, 1f, "Pivot X");
					EditorGUILayout.Slider (sproutAreaProp.FindPropertyRelative ("pivotY"), 0f, 1f, "Pivot Y");
					if (EditorGUI.EndChangeCheck ()) {
						ApplySerialized ();
						changesForMeshes = true;
						currentSproutMap.sproutAreas [index].Validate ();
					}
					GUILayout.Box ("", GUIStyle.none, 
						GUILayout.Width (Screen.width * 0.8f), 
						GUILayout.Height (Screen.width * 0.8f));
					Rect texRect = GUILayoutUtility.GetLastRect ();
					//Rect canvas = DrawTextureCanvas (texRect, texture);
					areaCanvas.DrawCanvas (texRect, texture, sproutArea);
					if (areaCanvas.HasChanged ()) {
						Undo.RecordObject (sproutMapperNode.sproutMapperElement, "Sprout area coordinates changed");
						areaCanvas.ApplyChanges (sproutArea);
						changesForMeshes = true;
					}
				}
			}
		}
		/// <summary>
		/// Raises the select sprout area item event.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnSelectSproutAreaItem (ReorderableList list)
		{
			Undo.RecordObject (sproutMapperNode.sproutMapperElement, "Sprout Map Area selected");
			currentSproutMap.selectedAreaIndex = list.index;
		}
		/// <summary>
		/// Adds a list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnAddSproutAreaItem(ReorderableList list)
		{
			Undo.RecordObject (sproutMapperNode.sproutMapperElement, "Sprout Map added");
			currentSproutMap.sproutAreas.Add (new SproutMap.SproutMapArea ());
		}
		/// <summary>
		/// Removes a list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnRemoveSproutAreaItem(ReorderableList list)
		{
			int undoGroup = Undo.GetCurrentGroup ();
			Undo.SetCurrentGroupName ("Sprout Map Area removed");
			Undo.RecordObject (sproutMapperNode.sproutMapperElement, "Sprout Map Area removed");
			currentSproutMap.sproutAreas.RemoveAt (list.index);
			Undo.RecordObject (sproutMapperNode.sproutMapperElement, "Sprout Map Area removed");
			currentSproutMap.selectedAreaIndex = -1;
			Undo.CollapseUndoOperations (undoGroup);
			changesForPipeline = true;
			
			//if (EditorUtility.DisplayDialog (MSG_RM_TEX_TITLE, MSG_RM_TEX_MSG, MSG_RM_TEX_YES, MSG_RM_TEX_NO)) {
			/*
				currentSproutMap.sproutAreas.RemoveAt (list.index);
				changesForPipeline = true;
				EditorUtility.SetDirty (target);
				*/
			//}
		}
		#endregion
	}
}