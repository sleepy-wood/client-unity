using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.Rendering;

using Broccoli.Base;
using Broccoli.Model;
using Broccoli.Pipe;
using Broccoli.Utils;
using Broccoli.Factory;
using Broccoli.Catalog;
using Broccoli.Manager;

namespace Broccoli.BroccoEditor
{
	/// <summary>
	/// SproutLab instance.
	/// </summary>
	public class SproutLabEditor {
		#region Canvas Settings
		/// <summary>
		/// Settings for the mesh preview canvas.
		/// </summary>
		public class CanvasSettings {
			public int id = 0;
			public bool freeViewEnabled = true;
			public bool resetZoom = false;
			public float defaultZoomFactor = 2.5f;
			public float minZoomFactor = 0.5f;
			public float maxZoomFactor = 2.5f;
			public bool resetView = true;
			public Vector3 viewOffset = new Vector3 (-0.04f, 0.6f, -5.5f);
			public Vector2 viewDirection = new Vector2 (90f, 0f);
			public Quaternion viewTargetRotation = Quaternion.identity;
			public bool showPlane = false;
			public float planeSize = 1f;
		}
		#endregion

		#region Structure Settings
		/// <summary>
		/// Settings for the structure implementation.
		/// </summary>
		public class StructureSettings {
			public int id;
			public string branchEntityName = "Branch";
			public string branchEntitiesName = "Branches";
			public bool variantsEnabled = false;
			public bool displayFreqAtBaseLevel = false;
			public bool displayRadiusControl = false;
			public bool displayExportDescriptor = true;
			public bool displayExportPrefab = false;
			public bool displayExportTextures = true;
		}
		#endregion

		#region Target Vars
		public BranchDescriptorCollection branchDescriptorCollection = null;
		private SproutSubfactory sproutSubfactory = null;
		#endregion
		
		#region Var
		/// <summary>
		/// Mesh preview utility.
		/// </summary>
		MeshPreview meshPreview;
		SproutCatalog catalog;
		Color defaultPreviewBackgroundColor = new Color (0.35f, 0.35f, 0.35f, 1f);
		Color graySkyPreviewBackgroundColor = new Color (0.28f, 0.38f, 0.47f);
		//Color normalPreviewBackgroundColor = new Color(0.5f, 0.5f, 1.0f, 1.0f);
		Color normalPreviewBackgroundColor = new Color(0.5f, 0.5f, 1.0f, 1.0f);
		Color extrasPreviewBackgroundColor = new Color(0f, 0f, 0f, 1.0f);
		Color subsurfacePreviewBackgroundColor = new Color(0f, 0f, 0f, 1.0f);
		Material[] currentPreviewMaterials = null;
		Material[] compositeMaterials = null;
		/// <summary>
		/// The area canvas.
		/// </summary>
		private SproutAreaCanvasEditor areaCanvas = new SproutAreaCanvasEditor ();
		/// <summary>
		/// Editor persistence utility.
		/// </summary>
		private EditorPersistence<BranchDescriptorCollectionSO> editorPersistence = null;
		/// <summary>
		/// Holds the canvas settings to display.
		/// </summary>
		private CanvasSettings currentCanvasSettings = null;
		/// <summary>
		/// Holds the structure configuration for the loaded structure implementation.
		/// </summary>
		private StructureSettings currentStructureSettings = null;
		/// <summary>
		/// Default canvas settings when none has been provided by an editor implementation.
		/// </summary>
		private CanvasSettings defaultCanvasSettings = new CanvasSettings ();
		/// <summary>
		/// Default structure settings when none has been provided by an editor implementation.
		/// </summary>
		private StructureSettings defaultStructureSettings = new StructureSettings ();
		public SproutMap.SproutMapArea selectedSproutMap = null; 
		int selectedSproutMapGroup = 0;
		int selectedSproutMapIndex = 0;
		public BranchDescriptorCollection.SproutMapDescriptor selectedSproutMapDescriptor = null;
		bool showLightControls = true;
		bool showLODOptions = false;
		private static Dictionary<int, ISproutLabEditorImpl> _implIdToImplementation = new Dictionary<int, ISproutLabEditorImpl>();
		private static Dictionary<Type, ISproutLabEditorImpl> _implementations = new Dictionary<Type, ISproutLabEditorImpl>();
		private ISproutLabEditorImpl currentImplementation = null;
		#endregion

		#region Debug Vars
		private bool debugEnabled = false;
		private int debugPolyIndex = 0;
		private bool debugSkipSimplifyHull = false;
		private bool debugShowTopoPoints = false;
		private bool debugShowConvexHullPoints = false;
		private bool debugShowConvexHullPointsOrder = false;
		private bool debugShowConvexHull = false;
		private bool debugShowAABB = false;
		private bool debugShowOBB = false;
		private bool debugShowTris = false;
		private bool debugShowMeshWireframe = false;
		private bool debugShowMeshNormals = false;
		private bool debugShowMeshTangents = false;
		private int debugClearTargetId = 0;
		private Texture2D debugAtlas = null;
		#endregion

		#region Delegates and Events
		public delegate void BranchDescriptorCollectionChange (BranchDescriptorCollection branchDescriptorCollection);
		public delegate void ShowNotification (string notification);
		public BranchDescriptorCollectionChange onBeforeBranchDescriptorChange;
		public BranchDescriptorCollectionChange onBranchDescriptorChange;
		public BranchDescriptorCollectionChange onBeforeVariationDescriptorChange;
		public BranchDescriptorCollectionChange onVariationDescriptorChange;
		public ShowNotification onShowNotification;
		#endregion

		#region GUI Vars
		Rect currentRect;
		private EditorGUISplitView verticalSplitView;
		public enum ViewMode {
			SelectMode,
			Structure,
			Templates
		}
		public ViewMode viewMode = ViewMode.Structure;
		public enum CanvasStructureView {
			Snapshot,
			Variation
		}
		public CanvasStructureView canvasStructureView = CanvasStructureView.Snapshot;
		/// <summary>
		/// Selected LOD view.
		/// </summary>
		private int selectedLODView = 0;
		/// <summary>
		/// Reorderable list to use on assigning sprout A textures.
		/// </summary>
		ReorderableList sproutAMapList;
		/// <summary>
		/// Reorderable list to use on assigning sprout B textures.
		/// </summary>
		ReorderableList sproutBMapList;
		/// <summary>
		/// Width for the left column on secondary panels.
		/// </summary>
		private int secondaryPanelColumnWidth = 120;
		/// <summary>
		/// Panel section selected.
		/// </summary>
		int currentPanelSection = 0;
		/// <summary>
		/// Structure view selected.
		/// </summary>
		int currentStructureView = 0;
		/// <summary>
		/// Texture view selected.
		/// </summary>
		int currenTextureView = 0;
		/// <summary>
		/// Map view selected.
		/// </summary>
		int currentMapView = 0;
		/// <summary>
		/// Export view selected.
		/// </summary>
		int currentExportView = 0;
		/// <summary>
		/// Debug view selected.
		/// </summary>
		int currentDebugView = 0;
		/// <summary>
		/// Saves the vertical scroll position for the structure view.
		/// </summary>
		private Vector2 structurePanelScroll;
		/// <summary>
		/// Saves the vertical scroll position for the texture view.
		/// </summary>
		private Vector2 texturePanelScroll;
		private Vector2 mappingPanelScroll;
		private Vector2 exportPanelScroll;
		private Vector2 debugPanelScroll;
		string[] levelOptions = new string[] {"Main Branch", "One Level", "Two Levels", "Three Levels"};
		bool branchGirthFoldout = false;
		bool branchNoiseFoldout = false;
		bool[] branchFoldouts = new bool[4];
		bool[] sproutAFoldouts = new bool[4];
		bool[] sproutBFoldouts = new bool[4];
		bool branchMapFoldout = true;
		bool sproutAMapFoldout = true;
		bool sproutBMapFoldout = true;
		BranchDescriptor selectedBranchDescriptor = null;
		VariationDescriptor selectedVariationDescriptor = null;
		BranchDescriptor.BranchLevelDescriptor selectedBranchLevelDescriptor;
		BranchDescriptor.SproutLevelDescriptor selectedSproutALevelDescriptor;
		BranchDescriptor.SproutLevelDescriptor selectedSproutBLevelDescriptor;
		BranchDescriptor.BranchLevelDescriptor proxyBranchLevelDescriptor = new BranchDescriptor.BranchLevelDescriptor ();
		BranchDescriptor.SproutLevelDescriptor proxySproutALevelDescriptor = new BranchDescriptor.SproutLevelDescriptor ();
		BranchDescriptor.SproutLevelDescriptor proxySproutBLevelDescriptor = new BranchDescriptor.SproutLevelDescriptor ();
		SproutMap.SproutMapArea proxySproutMap = new SproutMap.SproutMapArea ();
		BranchDescriptorCollection.SproutMapDescriptor proxySproutMapDescriptor = new BranchDescriptorCollection.SproutMapDescriptor ();
		bool sproutMapChanged = false;
		public static int catalogItemSize = 100;
		Texture2D tmpTexture = null;
		int lightAngleStep = 2;
		float lightAngleStepValue = 45f;
		string lightAngleDisplayStr = "front";
		float lightAngleToAddTime = 0.75f;
		float lightAngleToAddTimeTmp = -1f;
		Vector3 lightAngleEulerFrom = new Vector3 (0,-90,0);
		Vector3 lightAngleEulerTo = new Vector3 (0,-90,0);
		bool viewTransitionEnabled = false;
		bool zoomTransitionEnabled = false;
		Vector2 cameraTransitionDirection = new Vector2 (90, 0);
		Vector3 cameraTransitionOffset = Vector3.zero;
		Quaternion cameraTransitionTargetRotation = Quaternion.identity;
		float cameraTransitionZoom = 2.5f;
		Vector2 cameraTransitionDirectionTmp;
		Vector3 cameraTransitionOffsetTmp;
		Quaternion cameraTransitionTargetRotationTmp;
		float cameraTransitionZoomTmp;
		float cameraTransitionTime = 0.333f;
		float cameraTransitionTimeTmp = 0f;
		static string[] exportTextureOptions = new string[] {"Albedo", "Normals", "Extras", "Subsurface", "Composite"};
		bool showProgressBar = false;
		float progressBarProgress = 0f;
		string progressBarTitle = "";
		Rect meshPreviewRect = Rect.zero;
		#endregion

		#region GUI Content & Labels
		/// <summary>
		/// Tab titles for panel sections.
		/// </summary>
		private static GUIContent[] panelSectionOption = new GUIContent[4];
		/// <summary>
		/// Structure views: branch or leaves.
		/// </summary>
		private static GUIContent[] structureViewOptions = new GUIContent[3];
		/// <summary>
		/// Preview options GUIContent array.
		/// </summary>
		private static GUIContent[] mapViewOptions = new GUIContent[5];
		/// <summary>
		/// Debug options GUIContent array.
		/// </summary>
		private static GUIContent[] debugViewOptions = new GUIContent[4];
		/// <summary>
		/// Debug polygon options GUIContent array.
		/// </summary>
		private static GUIContent[] debugPolygonOptions = new GUIContent[6];
		/// <summary>
		/// Displays the snapshots as a list of options.
		/// </summary>
		private static GUIContent[] snapshotViewOptions;
		/// <summary>
		/// Displays the variations as a list of options.
		/// </summary>
		private static GUIContent[] variationViewOptions;
		/// <summary>
		/// Displays the LOD views as a list of options.
		/// </summary>
		private static GUIContent[] lodViewOptions;
		private static GUIContent exportViewOptionDescriptorGUI = 
			new GUIContent ("Export Descriptor", "Displays the panel with options to export the descriptor, meshes and texture atlas for all the branches to be used on a BroccoTree.");
		private static GUIContent exportViewOptionTexturesGUI = 
			new GUIContent ("Export Textures", "Displays the panel with options to export textures only or create an atlas from the branches.");
		private static GUIContent exportViewOptionPrefabGUI = 
			new GUIContent ("Export Prefab", "Displays the panel with options to export the collection to a Prefab Asset or multiple Prefab Assets.");
		private static GUIContent exportDescriptorAndAtlasGUI = 
			new GUIContent ("Export Descriptor with Atlas Texture", "Saves the Structure Collection to an editable ScriptableObject and creates textures atlases to map its meshes.");
		private static GUIContent exportPrefabGUI = 
			new GUIContent ("Export Prefab", "Exports the collection to a Prefab Asset or multiple Prefab Assets.");
		private static GUIContent selectPathGUI = 
			new GUIContent ("...", "Select the path to save the textures to.");
		private static GUIContent exportTexturesGUI = 
			new GUIContent ("Export Textures", "Exports textures only or create an atlas from the branches.");
		private static GUIContent backToCreateProjectGUI = 
			new GUIContent ("Back to Create Project", "Navigates back to the Project Creation options.");
		private static GUIContent backToStructureViewGUI = 
			new GUIContent ("Back to Structure View", "Navigate back to the structure view to edit the working structure collection.");
		private static GUIContent generateNewStructureGUI = 
			new GUIContent ("Generate New Structure", "Generates a new structure using a new randomization seed.");
		private static GUIContent regenerateCurrentGUI = 
			new GUIContent ("Regenerate Current", "Regenerates the current structure using its spawning random seed.");
		private static GUIContent loadFromTemplateGUI = 
			new GUIContent ("Load From Template", "Show the template catalog view to select a structure template to beging working with.");
		private static GUIContent addSnapshotGUI = 
			new GUIContent ("Add Snapshot", "Adds a Snapshot Structure to this Collection.");
		private static GUIContent removeSnapshotGUI = 
			new GUIContent ("Remove", "Removes the selected Snapshot Structure in this Collection.");
		private static GUIContent addVariationGUI = 
			new GUIContent ("Add Variation", "Adds a Variation Structure to this Collection.");
		private static GUIContent removeVariationGUI = 
			new GUIContent ("Remove", "Removes the selected Variation Structure in this Collection.");
		private static string labelCreateProject = "Create a Project";
		private static string labelStructures = "Structures";
		private static string labelBranches = "Branches";
		private static string labelSaturation = "Saturation";
		private static string labelTintColor = "Tint Color";
		private static string labelShadeRange = "Shade Range";
		private static string labelTintRange = "Tint Range";
		private static string labelMetallic = "Metallic";
		private static string labelGlossiness = "Glossiness";
		private static string labelTransluscencyFactor = "Transluscency Factor";
		private static string labelSproutA = "Sprout A";
		private static string labelSproutB = "Sprout B";
		private static string labelActiveLevels = "Active Levels";
		private static string labelGirth = "Girth";
		private static string labelGirthAtBase = "Girth at Base";
		private static string labelGirthAtTop = "Girth at Top";
		private static string labelNoise = "Noise";
		private static string labelNoiseAtBase = "Noise at Base";
		private static string labelNoiseAtTop = "Noise at Top";
		private static string labelNoiseScaleAtBase = "Noise Scale at Base";
		private static string labelNoiseScaleAtTop = "Noise Scale at Top";
		private static string labelSproutASettings = "Sprout A Settings";
		private static string labelSproutBSettings = "Sprout B Settings";
		private static string labelSize = "Size";
		private static string labelScaleAtBase = "Scale at Base";
		private static string labelScaleAtTop = "Scale at Top";
		private static string labelPlaneAlignment = "Plane Alignment";
		private static string labelFrequency = "Frequency";
		private static string labelRadius = "Spawn Radius";
		private static string labelLengthAtBase = "Length at Base";
		private static string labelLengthAtTop = "Length at Top";
		private static string labelParallelAlignAtBase = "Parallel Align at Base";
		private static string labelParallelAlignAtTop = "Parallel Align at Top";
		private static string labelGravityAlignAtBase = "Gravity Align at Base";
		private static string labelGravityAlignAtTop = "Gravity Align at Top";
		private static string labelBranchRange = "Branch Range";
		private static string labelBranchTextures = "Branch Textures";
		private static string labelSproutATextures = "Sprout A Textures";
		private static string labelSproutBTextures = "Sprout B Textures";
		private static string labelYDisplacement = "Y Displacement";
		private static string labelMappingView = "Mapping View";
		private static string labelCompositeMapSettings = "Composite Map Settings";
		private static string labelAlbedoMapSettings = "Albedo Map Settings";
		private static string labelNormalMapSettings = "Normal Map Settings";
		private static string labelExtraMapSettings = "Metallic, Smoothness and Ambient Occlusion Map Settings";
		private static string labelSubsurfaceMapSettings = "Subsurface Map Settings";
		private static string labelExportOptions = "Export Options";
		private static string labelImportOptions = "Import Options";
		private static string labelBranchDescExportSettings = "Branch Descriptor Export Settings";
		private static string labelAtlasTextureSettings = "Atlas Texture Settings";
		private static string labelAtlasSize = "Atlas Size";
		private static string labelPadding = "Padding";
		private static string labelTake = "Take";
		private static string labelPrefix = "Prefix";
		private static string labelPath = "Path:";
		private static string labelTexturesFolder = "Textures Folder";
		private static string labelTextures = "Textures";
		private static string labelPrefabSettings = "Prefab Settings";
		private static string labelPrefabFileSettings = "Prefab File Settings";
		private static string labelPrefabTextureSettings = "Prefab Texture Settings";
		private static string labelTextureExportSettings = "Texture Export Settings";
		private static string labelOutputFile = "Output File";
		private static string labelExportMode = "Export Mode";
		private static string labelEnabled = "Enabled";
		#endregion

		#region Constants
		public const int STRUCTURE_SNAPSHOT = 0;
		public const int STRUCTURE_VARIATION = 1;
		public const int PANEL_STRUCTURE = 0;
		public const int PANEL_TEXTURE = 1;
		public const int PANEL_MAPPING = 2;
		public const int PANEL_EXPORT = 3;
		public const int PANEL_DEBUG = 4;
		private const int VIEW_COMPOSITE = 0;
		private const int VIEW_ALBEDO = 1;
		private const int VIEW_NORMALS = 2;
		private const int VIEW_EXTRAS = 3;
		private const int VIEW_SUBSURFACE = 4;
		private const int STRUCTURE_BRANCH = 0;
		private const int STRUCTURE_SPROUT_A = 1;
		private const int STRUCTURE_SPROUT_B = 2;
		private const int TEXTURE_VIEW_TEXTURE = 0;
		private const int TEXTURE_VIEW_STRUCTURE = 1;
		private const int EXPORT_DESCRIPTOR = 0;
		private const int EXPORT_PREFAB = 1;
		private const int EXPORT_TEXTURES = 2;
		private const int DEBUG_GEOMETRY = 0;
		private const int DEBUG_CANVAS = 1;
		private const int DEBUG_MESHING = 2;
		private const int DEBUG_TEXTURES = 3;
		#endregion

		#region Messages
		private static string MSG_MAPPING_COMPOSITE = "How the branch and leaves will look like applying the albedo, normals and extra textures to the shader.";
		private static string MSG_MAPPING_ALBEDO = "Unlit texture to apply color values per leaf or group of leaves. The final map receives per leaf tint variations if enabled.";
		private static string MSG_MAPPING_NORMALS = "Normal mapping is adjusted per leaf texture to the tangent space according to the leaf mesh rotation.";
		private static string MSG_MAPPING_EXTRA = "Metallic value on the red channel, smoothness (glossiness) value on the green channel and ambient occlusion on the blue channel.";
		private static string MSG_MAPPING_SUBSURFACE = "Mapping for subsurface values, basically how the light should pass trough the material.";
		private static string MSG_EXPORT_DESCRIPTOR = "Exports a ScriptableObject with the Branch Collection and their atlas texture. The Descriptor File can be used on a Broccoli Tree Factory " +
			"to define branches (their meshes and textures).";
		private static string MSG_EXPORT_PREFAB = "Exports the current collection to a Prefab Asset.";
		private static string MSG_EXPORT_TEXTURE = "Exports texture files from a single Branch Snapshot or by creating a Texture Atlas for the Collection.";
		private static string MSG_DELETE_SPROUT_MAP_TITLE = "Remove Sprout Map";
		private static string MSG_DELETE_SPROUT_MAP_MESSAGE = "Do you really want to remove this sprout mapping?";
		private static string MSG_DELETE_SPROUT_MAP_OK = "Yes";
		private static string MSG_DELETE_SPROUT_MAP_CANCEL = "No";
		private static string MSG_DELETE_BRANCH_DESC_TITLE = "Remove Branch Descriptor";
		private static string MSG_DELETE_BRANCH_DESC_MESSAGE = "Do you really want to remove this branch descriptor snapshot?";
		private static string MSG_DELETE_BRANCH_DESC_OK = "Yes";
		private static string MSG_DELETE_BRANCH_DESC_CANCEL = "No";
		private static string MSG_DELETE_VARIATION_DESC_TITLE = "Remove Variation Descriptor";
		private static string MSG_DELETE_VARIATION_DESC_MESSAGE = "Do you really want to remove this variation descriptor snapshot?";
		private static string MSG_DELETE_VARIATION_DESC_OK = "Yes";
		private static string MSG_DELETE_VARIATION_DESC_CANCEL = "No";
		private static string MSG_LOAD_CATALOG_ITEM_TITLE = "Load Sprout Template";
		private static string MSG_LOAD_CATALOG_ITEM_MESSAGE = "Do you really want to load this sprout template? (Unsaved settings will be lost).";
		private static string MSG_LOAD_CATALOG_ITEM_OK = "Yes";
		private static string MSG_LOAD_CATALOG_ITEM_CANCEL = "No";
		private static string MSG_EMPTY_SNAPSHOTS = "No Snapshots found; you need to have at least one Variation to export them to prefabs.";
		private static string MSG_EMPTY_VARIATIONS = "No Variations found; you need to have at least one Variation to export them to prefabs.";
		#endregion

		#region Constructor and Initialization
		/// <summary>
		/// Static constructor. Registers this editor's implementations.
		/// </summary>
		static SproutLabEditor() {
			_implIdToImplementation.Clear ();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (Type type in assembly.GetTypes()) {
					if (type.GetCustomAttribute<SproutLabEditorImplAttribute>() != null) {
						ISproutLabEditorImpl instance = (ISproutLabEditorImpl)Activator.CreateInstance (type);
						_implementations.Add (type, instance);
						int[] implIds = instance.implIds;
						for (int i = 0; i < implIds.Length; i++) {
							if (!_implIdToImplementation.ContainsKey (implIds [i])) {
								_implIdToImplementation.Add (implIds [i], instance);
							} else {
								Debug.LogWarning ("Registering duplicated SproutLabImplementation with implId: " + implIds [i]);
							}
						}
					}
				}
			}
		}
		/// <summary>
		/// Creates a new SproutLabEditor instance.
		/// </summary>
		public SproutLabEditor () {
			var enumImpl = _implIdToImplementation.GetEnumerator ();
			while (enumImpl.MoveNext ()) {
				enumImpl.Current.Value.Initialize (this);
			}
			if (debugEnabled) {
				panelSectionOption = new GUIContent[5];
			}
			panelSectionOption [0] = 
				new GUIContent ("Structure", "Settings for tunning the structure of branches and leafs.");
			panelSectionOption [1] = 
				new GUIContent ("Textures", "Select the textures to apply to the branch and leaves.");
			panelSectionOption [2] = 
				new GUIContent ("Mapping", "Settings for textures and materials.");
			panelSectionOption [3] = 
				new GUIContent ("Export / Import", "Save or load a branch collection from file or export texture files.");
			if (debugEnabled) {
				panelSectionOption [4] = 
					new GUIContent ("Debug", "Debug tools.");
			}
			structureViewOptions [0] = 
				new GUIContent ("Branches", "Settings for branches.");
			structureViewOptions [1] = 
				new GUIContent ("Sprouts A", "Settings for A sprouts.");
			structureViewOptions [2] = 
				new GUIContent ("Sprouts B", "Settings for B sprouts.");
			mapViewOptions [0] = 
				new GUIContent ("Composite", "Composite branch preview.");
			mapViewOptions [1] = 
				new GUIContent ("Albedo", "Unlit albedo texture.");
			mapViewOptions [2] = 
				new GUIContent ("Normals", "Normal (bump) texture.");
			mapViewOptions [3] = 
				new GUIContent ("Extras", "Metallic (R), Glossiness (G), AO (B) texture.");
			mapViewOptions [4] = 
				new GUIContent ("Subsurface", "Subsurface texture.");
			debugViewOptions [0] = 
				new GUIContent ("Geometry", "Geometry debugging options.");
			debugViewOptions [1] = 
				new GUIContent ("Canvas", "Canvas debugging options.");
			debugViewOptions [2] = 
				new GUIContent ("Meshing", "Mesh debugging options.");
			debugViewOptions [3] = 
				new GUIContent ("Textures", "Texture creation debugging options.");
			debugPolygonOptions [0] = 
				new GUIContent ("All", "Display debug data for all options.");
			debugPolygonOptions [1] = 
				new GUIContent ("Base", "Display debug data for the base polygon.");
			debugPolygonOptions [2] = 
				new GUIContent ("Poly 1", "Display debug data for polygon 1.");
			debugPolygonOptions [3] = 
				new GUIContent ("Poly 2", "Display debug data for polygon 2.");
			debugPolygonOptions [4] = 
				new GUIContent ("Poly 3", "Display debug data for polygon 3.");
			debugPolygonOptions [5] = 
				new GUIContent ("Poly 4", "Display debug data for polygon 4.");
			OnEnable ();
		}
		public void OnEnable () {
			// Add update method.
			EditorApplication.update -= OnEditorUpdate;
			EditorApplication.update += OnEditorUpdate;
			// Init mesh preview
			if (meshPreview == null) {
				meshPreview = new MeshPreview ();
				meshPreview.showDebugInfo = false;
				meshPreview.showPivot = false;
				meshPreview.onDrawHandles += OnPreviewMeshDrawHandles;
				meshPreview.onDrawGUI += OnPreviewMeshDrawGUI;
				meshPreview.onRequiresRepaint += OnMeshPreviewRequiresRepaint;
				meshPreview.SetOffset (defaultCanvasSettings.viewOffset);
				meshPreview.SetDirection (defaultCanvasSettings.viewDirection);
				meshPreview.SetZoom (defaultCanvasSettings.defaultZoomFactor);
				meshPreview.minZoomFactor = defaultCanvasSettings.minZoomFactor;
				meshPreview.maxZoomFactor = defaultCanvasSettings.maxZoomFactor;
				Light light = meshPreview.GetLightA ();
				light.lightShadowCasterMode = LightShadowCasterMode.Everything;
				light.spotAngle = 1f;
				light.color = Color.white;
				light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;
				light.shadowStrength = 0.6f;
				light.shadowBias = 0;
				light.shadowNormalBias = 0f;
				light.shadows = LightShadows.Hard;
				if (PlayerSettings.colorSpace == ColorSpace.Linear) {
					meshPreview.SetLightA (1.2f, Quaternion.Euler (30, 0, 0));
				} else {
					meshPreview.SetLightA (0.75f, Quaternion.Euler (30, 0, 0));
				}
				lightAngleDisplayStr = "Left";

				if (currentImplementation != null) {
					SetCanvasSettings (currentImplementation.GetCanvasSettings (PANEL_STRUCTURE, 0));
				} else {
					SetCanvasSettings (null);
				}
			} else {
				meshPreview.Clear ();
			}
			// Init Editor Persistence.
			if (editorPersistence == null) {
				editorPersistence = new EditorPersistence<BranchDescriptorCollectionSO>();
				editorPersistence.elementName = "Branch Collection";
				editorPersistence.saveFileDefaultName = "SproutLabBranchCollection";
				editorPersistence.btnSaveAsNewElement = "Export to File";
				editorPersistence.btnLoadElement = "Import from File";
				editorPersistence.InitMessages ();
				editorPersistence.onCreateNew += OnCreateNewBranchDescriptorCollectionSO;
				editorPersistence.onLoad += OnLoadBranchDescriptorCollectionSO;
				editorPersistence.onGetElementToSave += OnGetBranchDescriptorCollectionSOToSave;
				editorPersistence.onGetElementToSaveFilePath += OnGetBranchDescriptorCollectionSOToSaveFilePath;
				editorPersistence.onSaveElement += OnSaveBranchDescriptorCollectionSO;
				editorPersistence.savePath = ExtensionManager.fullExtensionPath + GlobalSettings.pipelineSavePath;
				editorPersistence.showCreateNewEnabled = false;
				editorPersistence.showSaveCurrentEnabled = false;
				if (GlobalSettings.experimentalAdvancedSproutLab) {
					editorPersistence.showSaveEnabled = false;
				}
			}

			if (verticalSplitView == null) {
				verticalSplitView = new EditorGUISplitView (EditorGUISplitView.Direction.Vertical, SproutFactoryEditorWindow.focusedWindow);
				verticalSplitView.AddFixedSplit (90);
				verticalSplitView.AddDynamicSplit (0.6f);
				verticalSplitView.AddDynamicSplit ();
			}
			ShowPreviewMesh ();
		}
		public void OnDisable () {
			// Remove update method.
			meshPreview.Clear ();
			meshPreview = null;
			EditorApplication.update -= OnEditorUpdate;
			currentImplementation = null;
			currentCanvasSettings = null;
			currentStructureSettings = null;
			Clear ();
		}
		void Clear () {
			if (meshPreview != null) {
				meshPreview.Clear ();
			}
			selectedBranchDescriptor = null;
			selectedSproutALevelDescriptor = null;
			selectedSproutBLevelDescriptor = null;
			selectedSproutMap = null;
			selectedSproutMapDescriptor = null;
			branchDescriptorCollection = null;
		}
		/// <summary>
		/// Event called when destroying this editor.
		/// </summary>
		private void OnDestroy() {
			meshPreview.Clear ();
			verticalSplitView.Clear ();
			if (meshPreview.onDrawHandles != null) {
				meshPreview.onDrawHandles -= OnPreviewMeshDrawHandles;
				meshPreview.onDrawGUI -= OnPreviewMeshDrawGUI;
			}
		}
		#endregion

		#region Implementations
		/// <summary>
		/// Retuns an editor implementation according to a registered id.
		/// </summary>
		/// <param name="implId">Implementation id.</param>
		/// <returns>SproutLab implementation or null if none has been registered.</returns>
		private ISproutLabEditorImpl GetImplementation (int implId) {
			if (_implIdToImplementation.ContainsKey (implId)) {
				return _implIdToImplementation [implId];
			}
			return null;
		}
		#endregion

		#region Branch Descriptor Processing
		/// <summary>
		/// Load a BranchDescriptorCollection instance to this Editor.
		/// </summary>
		/// <param name="branchDescriptorCollection">Collection descriptor instance.</param>
		/// <param name="sproutSubfactory">Sprout Factory of the collection.</param>
		public void LoadBranchDescriptorCollection (BranchDescriptorCollection branchDescriptorCollection, SproutSubfactory sproutSubfactory) {
			GUITextureManager.Init ();
			// Assign the current branch descriptor.
			this.branchDescriptorCollection = branchDescriptorCollection;
			// Creates the sprout factory to handle the branches processing.
			this.sproutSubfactory = sproutSubfactory;
			// If the descriptor collection is new.
			if (branchDescriptorCollection.descriptorImplId < 0) {
				viewMode = ViewMode.SelectMode;
			} else {
				// Set editor implementation according to the collection instance.
				currentImplementation = GetImplementation (branchDescriptorCollection.descriptorImplId);
				if (currentImplementation != null) {
					meshPreview.showPreviewTitle = true;
					meshPreview.previewTitle = currentImplementation.GetPreviewTitle (branchDescriptorCollection.descriptorImplId);
					SetCanvasSettings (currentImplementation.GetCanvasSettings (currentPanelSection, 0));
					SetStructureSettings (currentImplementation.GetStructureSettings (branchDescriptorCollection.descriptorImplId));
				}

				// Set the editor view mode.
				viewMode = ViewMode.Structure;
				if (this.branchDescriptorCollection.branchDescriptors.Count == 0) {
					branchDescriptorCollection.AddBranchDescriptor (new BranchDescriptor ());
				}
				sproutSubfactory.onReportProgress -= OnReportProgress;
				sproutSubfactory.onReportProgress += OnReportProgress;
				sproutSubfactory.onFinishProgress -= OnFinishProgress;
				sproutSubfactory.onFinishProgress += OnFinishProgress;

				// Set the editor canvas view mode and select the first index of them (snapshot or variation).
				InitVariationViewOptions ();
				if (branchDescriptorCollection.variationDescriptorIndex < 0 && branchDescriptorCollection.variationDescriptors.Count > 0)
					branchDescriptorCollection.variationDescriptorIndex = 0;
				InitSnapshotViewOptions ();
				if (branchDescriptorCollection.branchDescriptorIndex < 0 && branchDescriptorCollection.branchDescriptors.Count > 0)
					branchDescriptorCollection.branchDescriptorIndex = 0;
				if (currentStructureSettings.variantsEnabled) {
					SelectVariation (branchDescriptorCollection.variationDescriptorIndex);
				} else {
					SelectSnapshot (branchDescriptorCollection.branchDescriptorIndex);
					InitSproutMapLists ();
				}
				
				// Prepare the internal tree factory to process the branch descriptor.
				LoadBranchDescriptorCollectionTreeFactory ();
			}
		}
		public void UnloadBranchDescriptorCollection () {
			selectedBranchDescriptor = null;
			if (sproutSubfactory != null)
				sproutSubfactory.UnloadPipeline ();
		}
		private void LoadBranchDescriptorCollectionTreeFactory () {
			// Load Sprout Lab base pipeline.
			string pathToAsset = ExtensionManager.fullExtensionPath + GlobalSettings.templateSproutLabPipelinePath;
			pathToAsset = pathToAsset.Replace(Application.dataPath, "Assets");
			Broccoli.Pipe.Pipeline loadedPipeline =
				AssetDatabase.LoadAssetAtPath<Broccoli.Pipe.Pipeline> (pathToAsset);

			if (loadedPipeline == null) {
				throw new UnityException ("Cannot Load Pipeline: The file at the specified path '" + 
					pathToAsset + "' is no valid save file as it does not contain a Pipeline.");
			}
			sproutSubfactory.LoadPipeline (loadedPipeline, branchDescriptorCollection, pathToAsset);
			Resources.UnloadAsset (loadedPipeline);
			sproutSubfactory.BranchDescriptorCollectionToPipeline ();
			RegeneratePreview ();
			selectedSproutMap = null;
			selectedSproutMapDescriptor = null;
		}
		public void ReflectChangesToPipeline () {
			sproutSubfactory.BranchDescriptorCollectionToPipeline ();
		}
		/// <summary>
		/// Regenerates the mesh according to the selected snapshot.
		/// </summary>
		public void RegeneratePreview (int viewMode = VIEW_COMPOSITE) {
			sproutSubfactory.ProcessSnapshot ();
			compositeMaterials = null;
			ShowPreviewMesh (viewMode);
		}
		/// <summary>
		/// Process a polygon area.
		/// </summary>
		public void ProcessPolygonArea (PolygonArea polygonArea, BroccoTree tree) {
			/*
			// Set the vertices.
			GeometryAnalyzer ga = GeometryAnalyzer.Current ();
			// Add base of the branch.
			ga.GetBaseBranchPositions (sproutSubfactory.treeFactory.previewTree, 0f, false);
			// Add points at the end of terminal branches.
			ga.GetTerminalBranchPositions (sproutSubfactory.treeFactory.previewTree, 1f, true);
			// Add terminal point of all leaves.
			ga.GetSproutPositions (sproutSubfactory.treeFactory.previewTree, -1, false);
			// Save all points.
			_snapshotPoints.Clear ();
			_snapshotPoints.AddRange (ga.branchPoints);
			_snapshotPoints.AddRange (ga.sproutPoints);
			
			// Scale points.
			for (int i = 0; i < _snapshotPoints.Count; i++) {
				_snapshotPoints [i] = _snapshotPoints [i] * sproutSubfactory.factoryScale;
			}

			// ConvexHull points.
			_convexPoints = ga.QuickHullYZ (_snapshotPoints, false);
			_convexPoints = ga.ShiftConvexHullPoint (_convexPoints);
			if (_convexPoints.Count > 0) {
				_convexPoints.Add (_convexPoints [0]);
			}

			// Simplify convex hull points.
			if (debugSimplifyHull) {
				_convexPoints = ga.SimplifyConvexHullYZ (_convexPoints, 35f);
			}
			_convexPoints.RemoveAt (_convexPoints.Count - 1);

			// Set the polygon area points.
			polygonArea.points.Clear ();
			polygonArea.points.AddRange (_convexPoints);
			polygonArea.lastConvexPointIndex = _convexPoints.Count - 1;

			// AABB box.
			_aabb = GeometryUtility.CalculateBounds (_convexPoints.ToArray (), Matrix4x4.identity);
			polygonArea.aabb = _aabb;

			// OBB box.
			_obb = ga.GetOBBFromPolygon (_convexPoints, out _obbAngle);
			polygonArea.obb = _obb;
			polygonArea.obbAngle = _obbAngle;

			// Triangulation
			_triangles = ga.DelaunayTriangulationYZ (_convexPoints);
			polygonArea.triangles.Clear ();
			polygonArea.triangles.AddRange (_triangles);

			// Build the mesh.
			ProcessPolygonAreaMesh (polygonArea);

			// Clear.
			_convexPoints.Clear ();
			_triangles.Clear ();
			_convexPoints.Clear ();
			*/
		}
		private void ProcessPolygonAreaMesh (PolygonArea polygonArea) {
			Mesh mesh = new Mesh ();
			// Set vertices.
			mesh.SetVertices (polygonArea.points);
			// Set triangles.
			mesh.SetTriangles (polygonArea.triangles, 0);
			mesh.RecalculateBounds ();
			// Set normals.
			mesh.RecalculateNormals ();
			polygonArea.normals.Clear ();
			polygonArea.normals.AddRange (mesh.normals);
			// Set tangents.
			Vector4[] _tangents = new Vector4[polygonArea.points.Count];
			for (int i = 0; i < _tangents.Length; i++) {
				_tangents [i] = Vector3.forward;
				_tangents [i].w = 1f;
			}
			mesh.tangents = _tangents;
			polygonArea.tangents.Clear ();
			polygonArea.tangents.AddRange (mesh.tangents);
			// Set UVs.
			float z, y;
			List<Vector4> uvs = new List<Vector4> ();
			for (int i = 0; i < polygonArea.points.Count; i++) {
				z = Mathf.InverseLerp (polygonArea.aabb.min.z, polygonArea.aabb.max.z, polygonArea.points [i].z);
				y = Mathf.InverseLerp (polygonArea.aabb.min.y, polygonArea.aabb.max.y, polygonArea.points [i].y);
				uvs.Add (new Vector4 (z, y, z, y));
			}
			mesh.SetUVs (0, uvs);
			polygonArea.uvs.Clear ();
			polygonArea.uvs.AddRange (uvs);
			// Set the mesh.
			polygonArea.mesh = mesh;
		}
		/// <summary>
		/// Sets the descriptor type loaded on this editor.
		/// </summary>
		/// <param name="descriptorImpl">Descriptor type to set.</param>
		public void SetBranchDescriptorCollectionImpl (int descriptorImpl) {
			this.branchDescriptorCollection.descriptorImplId = descriptorImpl;
			viewMode = ViewMode.Structure;
			LoadBranchDescriptorCollection (this.branchDescriptorCollection, this.sproutSubfactory);
		}
		#endregion

		#region Draw Methods
        public void Draw (Rect windowRect) {
			currentRect = windowRect;
			if (viewMode == ViewMode.SelectMode) {
				DrawHeader (windowRect);
				DrawSelectModeView (windowRect);
			} else if (viewMode == ViewMode.Structure) {
				if (compositeMaterials == null) {
					SetMapView (VIEW_COMPOSITE, true);
				}
				verticalSplitView.BeginSplitView ();
				DrawHeader (windowRect);
				DrawStructureViewHeader (windowRect);
				verticalSplitView.Split ();
				DrawStructureViewCanvas (windowRect);
				verticalSplitView.Split ();
				DrawStructureViewControlPanel ();
				verticalSplitView.EndSplitView ();
			} else {				
				DrawHeader (windowRect);
				DrawTemplateView (windowRect);
			}
        }
		public void DrawHeader (Rect windowRect) {
			EditorGUILayout.Space ();
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Box ("", GUIStyle.none, 
				GUILayout.Width (150), 
				GUILayout.Height (60));
			GUI.DrawTexture (new Rect (5, 8, 140, 48), GUITextureManager.GetLogo (), ScaleMode.ScaleToFit);
			string headerMsg = string.Empty;
			var enumImpl = _implementations.GetEnumerator ();
			while (enumImpl.MoveNext ()) {
				headerMsg += enumImpl.Current.Value.GetHeaderMsg () + "\n";
			}
			if (ExtensionManager.isHDRP || ExtensionManager.isURP) {
				headerMsg += "Editor SRP is " + (ExtensionManager.isHDRP?"HDRP":"URP");
			} else {
				headerMsg += "Editor SRP is Standard";
			}
			EditorGUILayout.HelpBox (headerMsg, MessageType.None);
			EditorGUILayout.EndHorizontal ();
		}
		public void SetMapView (int mapView, bool force = false) {
			if (mapView != currentMapView || force) {
				currentMapView = mapView;
				if (compositeMaterials == null) {
					compositeMaterials = sproutSubfactory.treeFactory.previewTree.obj.GetComponent<MeshRenderer>().sharedMaterials;
				}
				if (compositeMaterials.Length > 0 && compositeMaterials[0] != null) {
					if (currentMapView == VIEW_COMPOSITE) { // Composite
						currentPreviewMaterials = sproutSubfactory.GetCompositeMaterials (compositeMaterials,
							branchDescriptorCollection.sproutStyleA.colorTint,
							branchDescriptorCollection.sproutStyleB.colorTint,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						meshPreview.backgroundColor = graySkyPreviewBackgroundColor;
						meshPreview.hasSecondPass = true;
						meshPreview.secondPassMaterials = sproutSubfactory.GetAlbedoMaterials (compositeMaterials,
							branchDescriptorCollection.sproutStyleA.colorTint,
							branchDescriptorCollection.sproutStyleB.colorTint,
							branchDescriptorCollection.sproutStyleA.colorSaturation,
							branchDescriptorCollection.sproutStyleB.colorSaturation,
							branchDescriptorCollection.branchColorShade,
							branchDescriptorCollection.branchColorSaturation,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection),
							false);
						/*
						currentPreviewMaterials = sproutSubfactory.GetAlbedoMaterials (compositeMaterials,
							branchDescriptorCollection.colorTintA,
							branchDescriptorCollection.colorTintB,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						meshPreview.hasSecondPass = true;
						meshPreview.secondPassMaterials = sproutSubfactory.GetCompositeMaterials (compositeMaterials,
							branchDescriptorCollection.colorTintA,
							branchDescriptorCollection.colorTintB,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						*/
						showLightControls = true;
					} else if (currentMapView == VIEW_ALBEDO) { // Albedo
						currentPreviewMaterials = sproutSubfactory.GetAlbedoMaterials (compositeMaterials,
							branchDescriptorCollection.sproutStyleA.colorTint,
							branchDescriptorCollection.sproutStyleB.colorTint,
							branchDescriptorCollection.sproutStyleA.colorSaturation,
							branchDescriptorCollection.sproutStyleB.colorSaturation,
							branchDescriptorCollection.branchColorShade,
							branchDescriptorCollection.branchColorSaturation,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						meshPreview.backgroundColor = defaultPreviewBackgroundColor;
						meshPreview.hasSecondPass = false;
						showLightControls = false;
					} else if (currentMapView == VIEW_NORMALS) { // Normals
						currentPreviewMaterials = sproutSubfactory.GetNormalMaterials (compositeMaterials);
						meshPreview.backgroundColor = normalPreviewBackgroundColor;
						meshPreview.hasSecondPass = false;
						showLightControls = false;
					} else if (currentMapView == VIEW_EXTRAS) { // Extra
						currentPreviewMaterials = sproutSubfactory.GetExtraMaterials (compositeMaterials,
							branchDescriptorCollection.sproutStyleA.metallic,
							branchDescriptorCollection.sproutStyleA.glossiness,
							branchDescriptorCollection.sproutStyleB.metallic,
							branchDescriptorCollection.sproutStyleB.glossiness,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						meshPreview.backgroundColor = extrasPreviewBackgroundColor;
						meshPreview.hasSecondPass = false;
						showLightControls = false;
					} else if (currentMapView == VIEW_SUBSURFACE) { // Subsurface
						currentPreviewMaterials = sproutSubfactory.GetSubsurfaceMaterials (compositeMaterials,
							branchDescriptorCollection.sproutStyleA.colorTint,
							branchDescriptorCollection.sproutStyleB.colorTint,
							branchDescriptorCollection.branchColorSaturation,
							branchDescriptorCollection.sproutStyleA.colorSaturation,
							branchDescriptorCollection.sproutStyleB.colorSaturation,
							branchDescriptorCollection.sproutStyleA.subsurfaceMul,
							branchDescriptorCollection.sproutStyleB.subsurfaceMul,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						meshPreview.backgroundColor = subsurfacePreviewBackgroundColor;
						meshPreview.hasSecondPass = false;
						showLightControls = false;
					}
				} else {
					currentPreviewMaterials = sproutSubfactory.GetAlbedoMaterials (compositeMaterials,
						branchDescriptorCollection.sproutStyleA.colorTint,
						branchDescriptorCollection.sproutStyleB.colorTint,
						branchDescriptorCollection.sproutStyleA.colorSaturation,
						branchDescriptorCollection.sproutStyleB.colorSaturation,
						branchDescriptorCollection.branchColorShade,
						branchDescriptorCollection.branchColorSaturation,
						SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
						SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection),
						false);
					meshPreview.backgroundColor = defaultPreviewBackgroundColor;
					meshPreview.hasSecondPass = true;
					meshPreview.secondPassMaterials = compositeMaterials; 
					/*
					currentPreviewMaterials = compositeMaterials;
					meshPreview.backgroundColor = defaultPreviewBackgroundColor;
					meshPreview.hasSecondPass = true;
					meshPreview.secondPassMaterials = sproutSubfactory.GetAlbedoMaterials (compositeMaterials,
						branchDescriptorCollection.colorTintA,
						branchDescriptorCollection.colorTintB,
						SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
						SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						*/
				}
			}
		}
		/// <summary>
		/// Editor View Mode for when the Sprout Factory is empty.
		/// </summary>
		/// <param name="windowRect">Window rect.</param>
		public void DrawSelectModeView (Rect windowRect) {
			GUILayout.BeginArea(new Rect(windowRect.width * 0.15f, 0, windowRect.width * 0.7f, windowRect.height));
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical ();
			GUILayout.FlexibleSpace();

			EditorGUILayout.LabelField (labelCreateProject, BroccoEditorGUI.labelBoldCentered);
			EditorGUILayout.Space ();
			var enumImpl = _implementations.GetEnumerator ();
			while (enumImpl.MoveNext ()) {
				enumImpl.Current.Value.DrawSelectModeViewBeforeOptions ();
			}
			enumImpl = _implementations.GetEnumerator ();
			while (enumImpl.MoveNext ()) {
				enumImpl.Current.Value.DrawSelectModeViewAfterOptions ();
			}
			EditorGUILayout.Space (200);

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndArea();
		}
		/// <summary>
		/// Draws the header for the structure canvas view.
		/// </summary>
		/// <param name="windowRect">Window rect.</param>
		public void DrawStructureViewHeader (Rect windowRect) {
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (generateNewStructureGUI)) {
				onBeforeBranchDescriptorChange (branchDescriptorCollection);
				sproutSubfactory.ProcessSnapshot (true, SproutSubfactory.MaterialMode.Composite, true);
				onBranchDescriptorChange (branchDescriptorCollection);
				ShowPreviewMesh ();
			}
			if (GUILayout.Button (regenerateCurrentGUI)) {
				RegeneratePreview ();
			}
			if (GUILayout.Button (loadFromTemplateGUI)) {
				catalog = SproutCatalog.GetInstance ();
				viewMode = ViewMode.Templates;
			}
			GUILayout.EndHorizontal ();
		}
		/// <summary>
		/// Editor View Mode for when the Sprout Factory is displaying a structure.
		/// </summary>
		/// <param name="windowRect">Window rect.</param>
		public void DrawStructureViewCanvas (Rect windowRect) {
			int splitHeigth = verticalSplitView.GetCurrentSplitSize ();
			GUILayout.Box ("", GUIStyle.none, 
				GUILayout.Width (windowRect.width), 
				GUILayout.Height (splitHeigth > 0 ? splitHeigth - 2 : 0));
			Rect viewRect = GUILayoutUtility.GetLastRect ();
			// STRUCTURE IS SNAPSHOT.
			if (canvasStructureView == CanvasStructureView.Snapshot) {
				// DRAW NOT SELECTED SNAPSHOT.
				if (selectedBranchDescriptor == null) {
					DrawEmptyCanvas (viewRect);
				}
				// DRAW TEXTURE CANVAS.
				else if (currentPanelSection == PANEL_TEXTURE && (currentStructureView == STRUCTURE_SPROUT_A || currentStructureView == STRUCTURE_SPROUT_B)  
					&& selectedSproutMap != null && selectedSproutMap.texture != null)
				{
					currenTextureView = TEXTURE_VIEW_TEXTURE;
					tmpTexture = sproutSubfactory.GetSproutTexture (selectedSproutMapGroup, selectedSproutMapIndex);
					
					if (tmpTexture != null) {
						areaCanvas.DrawCanvas (viewRect, tmpTexture, selectedSproutMap);
					}

					if (areaCanvas.HasChanged ()) {
						branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
						onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
						areaCanvas.ApplyChanges (selectedSproutMap);
						onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
						meshPreview.backgroundColor = defaultPreviewBackgroundColor;
					}
				}
				// DRAW STRUCTURE MESH.
				else {
					if (currenTextureView == TEXTURE_VIEW_TEXTURE) {
						RegeneratePreview ();
						currenTextureView = TEXTURE_VIEW_STRUCTURE;
					}
					if (viewRect.height > 0) {
						meshPreviewRect = viewRect;
						meshPreview.RenderViewport (viewRect, GUIStyle.none, currentPreviewMaterials);
					}
				}
			}
			// STRUCTURE IS VARIATION.
			else {
				// DRAW NOT SELECTED VARIATION.
				if (selectedVariationDescriptor == null) {
					DrawEmptyCanvas (viewRect);
				}
			}
		}
		/// <summary>
		/// Displays an empty canvas when no structure (snapshot or variation) has been selected.
		/// </summary>
		/// <param name="rect">Recto to display the empty canvas.</param>
		void DrawEmptyCanvas (Rect rect) {
			rect.x = rect.width / 2f - 80;
			rect.y = rect.height / 2f;
			rect.height = EditorGUIUtility.singleLineHeight * 2;
			rect.width = 220;
			EditorGUI.HelpBox (rect, "No Snapshot or Variation selected.\nSelect one to display it here.", MessageType.Info);
		}
		/// <summary>
		/// Editor View Mode for when the Sprout Factory is displaying the template catalog.
		/// </summary>
		/// <param name="windowRect">Window rect.</param>
		public void DrawTemplateView (Rect windowRect) {
			Rect toolboxRect = new Rect (windowRect);
			toolboxRect.height = EditorGUIUtility.singleLineHeight;
			GUILayout.BeginHorizontal ();
			if (branchDescriptorCollection.descriptorImplId < 0) {
				if (GUILayout.Button (backToCreateProjectGUI)) {
					viewMode = ViewMode.SelectMode;
				}
			} else {
				if (GUILayout.Button (backToStructureViewGUI)) {
					viewMode = ViewMode.Structure;
				}
			}
			GUILayout.EndHorizontal ();
			if (catalog == null) {
				catalog = SproutCatalog.GetInstance ();
			}
			// Draw Templates.
			if (catalog.GetGUIContents ().Count > 0) {
				string categoryKey = "";
				var enumerator = catalog.contents.GetEnumerator ();
				while (enumerator.MoveNext ()) {
					var contentPair = enumerator.Current;
					categoryKey = contentPair.Key;
					EditorGUILayout.LabelField (categoryKey, BroccoEditorGUI.label);
					int columns = Mathf.CeilToInt ((windowRect.width - 8) / catalogItemSize);
					int height = Mathf.CeilToInt (catalog.GetGUIContents ()[categoryKey].Count / (float)columns) * catalogItemSize;
					int selectedIndex = 
						GUILayout.SelectionGrid (-1, catalog.GetGUIContents ()[categoryKey].ToArray (), 
							columns, Broccoli.TreeNodeEditor.TreeCanvasGUI.catalogItemStyle, GUILayout.Height (height), GUILayout.Width (windowRect.width - 8));
					if (selectedIndex >= 0 &&
					   EditorUtility.DisplayDialog (MSG_LOAD_CATALOG_ITEM_TITLE, 
						   MSG_LOAD_CATALOG_ITEM_MESSAGE, 
						   MSG_LOAD_CATALOG_ITEM_OK, 
						   MSG_LOAD_CATALOG_ITEM_CANCEL)) {
						// Load the Snapshot Collection SO
						string pathToCollection = ExtensionManager.fullExtensionPath + catalog.GetItemAtIndex (categoryKey, selectedIndex).path;
						BranchDescriptorCollectionSO branchDescriptorCollectionSO = editorPersistence.LoadElementFromFile (pathToCollection);
						if (branchDescriptorCollectionSO != null) {
							selectedIndex = -1;
							OnLoadBranchDescriptorCollectionSO (branchDescriptorCollectionSO, pathToCollection);
							viewMode = ViewMode.Structure;
						} else {
							Debug.LogWarning ("Could not find BranchDescriptorCollectionSO at: " + pathToCollection);
						}
						GUIUtility.ExitGUI ();
					}
				}
			}
		}
		public void DrawStructureViewControlPanel () {
			DrawSnapshotsPanel ();
			if (currentStructureSettings.variantsEnabled) {
				DrawVariationsPanel ();
			}
			int _currentPanelSection = GUILayout.Toolbar (currentPanelSection, panelSectionOption, GUI.skin.button);
			if (_currentPanelSection != currentPanelSection) {
				currentPanelSection = _currentPanelSection;
				ShowPreviewMesh ();
				if (currentImplementation != null) {
					SetCanvasSettings (currentImplementation.GetCanvasSettings (currentPanelSection, 0));
				}
			}
			switch (currentPanelSection) {
				case PANEL_STRUCTURE:
					DrawStructurePanel ();
					break;
				case PANEL_TEXTURE:
					DrawTexturePanel ();
					break;
				case PANEL_MAPPING:
					DrawMappingPanel ();
					break;
				case PANEL_EXPORT:
					DrawExportPanel ();
					break;
				case PANEL_DEBUG:
					DrawDebugPanel ();
					break;
			}
		}
		#endregion

		#region Structure Panel
		/// <summary>
		/// Draw the structure panel window view.
		/// </summary>
		public void DrawStructurePanel () {
			if (selectedBranchDescriptor == null) return;

			bool changed = false;
			float girthAtBase = selectedBranchDescriptor.girthAtBase;
			float girthAtTop = selectedBranchDescriptor.girthAtTop;
			float noiseAtBase = selectedBranchDescriptor.noiseAtBase;
			float noiseAtTop = selectedBranchDescriptor.noiseAtTop;
			float noiseScaleAtBase = selectedBranchDescriptor.noiseScaleAtBase;
			float noiseScaleAtTop = selectedBranchDescriptor.noiseScaleAtTop;
			float sproutASize = selectedBranchDescriptor.sproutASize;
			float sproutAScaleAtBase = selectedBranchDescriptor.sproutAScaleAtBase;
			float sproutAScaleAtTop = selectedBranchDescriptor.sproutAScaleAtTop;
			float sproutAFlipAlign = selectedBranchDescriptor.sproutAFlipAlign;
			float sproutBSize = selectedBranchDescriptor.sproutBSize;
			float sproutBScaleAtBase = selectedBranchDescriptor.sproutBScaleAtBase;
			float sproutBScaleAtTop = selectedBranchDescriptor.sproutBScaleAtTop;
			float sproutBFlipAlign = selectedBranchDescriptor.sproutBFlipAlign;
			int activeLevels = 2;
			if (GlobalSettings.experimentalAdvancedSproutLab) {
				showLODOptions = true;
			}

			EditorGUILayout.BeginHorizontal ();
			// View Mode Selection.
			EditorGUILayout.BeginVertical ();
			EditorGUILayout.LabelField (labelStructures, BroccoEditorGUI.labelBoldCentered);
			currentStructureView = GUILayout.SelectionGrid (currentStructureView, structureViewOptions, 1, GUILayout.Width (secondaryPanelColumnWidth));
			EditorGUILayout.EndVertical ();
			// Mapping Settings.
			structurePanelScroll = EditorGUILayout.BeginScrollView (structurePanelScroll, GUILayout.ExpandWidth (true));
			switch (currentStructureView) {
				case STRUCTURE_BRANCH: // BRANCHES.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (currentStructureSettings.branchEntityName + " Global Settings", BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					// Active levels
					EditorGUI.BeginChangeCheck ();
					activeLevels = EditorGUILayout.Popup (labelActiveLevels, selectedBranchDescriptor.activeLevels, levelOptions); 
					changed = EditorGUI.EndChangeCheck ();
					
					// GIRTH.
					branchGirthFoldout = EditorGUILayout.Foldout (branchGirthFoldout, labelGirth, BroccoEditorGUI.foldoutBold);
					if (branchGirthFoldout) {
						EditorGUI.indentLevel++;
						// Branch structure settings
						girthAtBase = EditorGUILayout.Slider (labelGirthAtBase, selectedBranchDescriptor.girthAtBase, 0.005f, 0.4f);
						if (girthAtBase != selectedBranchDescriptor.girthAtBase) {
							changed = true;
						}
						girthAtTop = EditorGUILayout.Slider (labelGirthAtTop, selectedBranchDescriptor.girthAtTop, 0.005f, 0.4f);
						if (girthAtTop != selectedBranchDescriptor.girthAtTop) {
							changed = true;
						}
						EditorGUI.indentLevel--;
						EditorGUILayout.Space ();
					}

					// NOISE.
					branchNoiseFoldout = EditorGUILayout.Foldout (branchNoiseFoldout, labelNoise, BroccoEditorGUI.foldoutBold);
					if (branchNoiseFoldout) {
						EditorGUI.indentLevel++;
						noiseAtBase = EditorGUILayout.Slider (labelNoiseAtBase, selectedBranchDescriptor.noiseAtBase, 0f, 1f);
						if (noiseAtBase != selectedBranchDescriptor.noiseAtBase) {
							changed = true;
						}
						noiseAtTop = EditorGUILayout.Slider (labelNoiseAtTop, selectedBranchDescriptor.noiseAtTop, 0f, 1f);
						if (noiseAtTop != selectedBranchDescriptor.noiseAtTop) {
							changed = true;
						}
						noiseScaleAtBase = EditorGUILayout.Slider (labelNoiseScaleAtBase, selectedBranchDescriptor.noiseScaleAtBase, 0f, 1f);
						if (noiseScaleAtBase != selectedBranchDescriptor.noiseScaleAtBase) {
							changed = true;
						}
						noiseScaleAtTop = EditorGUILayout.Slider (labelNoiseScaleAtTop, selectedBranchDescriptor.noiseScaleAtTop, 0f, 1f);
						if (noiseScaleAtTop != selectedBranchDescriptor.noiseScaleAtTop) {
							changed = true;
						}
						EditorGUI.indentLevel--;
					}
					EditorGUILayout.Space ();
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (currentStructureSettings.branchEntityName + " Levels Settings", BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					// Draw Branch Structure Panel
					changed |= DrawBranchStructurePanel ();
					break;
				case STRUCTURE_SPROUT_A: // LEAVES.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelSproutASettings, BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					// Active levels
					EditorGUI.BeginChangeCheck ();
					activeLevels = EditorGUILayout.Popup (labelActiveLevels, selectedBranchDescriptor.activeLevels, levelOptions); 
					changed = EditorGUI.EndChangeCheck ();
					EditorGUILayout.Space ();
					// Sprout structure settings
					EditorGUI.BeginChangeCheck ();
					sproutASize = EditorGUILayout.Slider (labelSize, selectedBranchDescriptor.sproutASize, 0.1f, 5f);
					sproutAScaleAtBase = EditorGUILayout.Slider (labelScaleAtBase, selectedBranchDescriptor.sproutAScaleAtBase, 0.1f, 5f);
					sproutAScaleAtTop = EditorGUILayout.Slider (labelScaleAtTop, selectedBranchDescriptor.sproutAScaleAtTop, 0.1f, 5f);
					sproutAFlipAlign = EditorGUILayout.Slider (labelPlaneAlignment, selectedBranchDescriptor.sproutAFlipAlign, 0.5f, 1f);
					changed |= EditorGUI.EndChangeCheck ();
					EditorGUILayout.Space ();
					// Draw Sprout A Hierarchy Structure Panel
					changed |= DrawSproutAStructurePanel ();
					break;
				case STRUCTURE_SPROUT_B: // LEAVES.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelSproutBSettings, BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					// Active levels
					EditorGUI.BeginChangeCheck ();
					activeLevels = EditorGUILayout.Popup (labelActiveLevels, selectedBranchDescriptor.activeLevels, levelOptions); 
					changed = EditorGUI.EndChangeCheck ();
					EditorGUILayout.Space ();
					// Sprout structure settings
					EditorGUI.BeginChangeCheck ();
					sproutBSize = EditorGUILayout.Slider (labelSize, selectedBranchDescriptor.sproutBSize, 0.1f, 5f);
					sproutBScaleAtBase = EditorGUILayout.Slider (labelScaleAtBase, selectedBranchDescriptor.sproutBScaleAtBase, 0.1f, 5f);
					sproutBScaleAtTop = EditorGUILayout.Slider (labelScaleAtTop, selectedBranchDescriptor.sproutBScaleAtTop, 0.1f, 5f);
					sproutBFlipAlign = EditorGUILayout.Slider (labelPlaneAlignment, selectedBranchDescriptor.sproutBFlipAlign, 0.5f, 1f);
					changed |= EditorGUI.EndChangeCheck ();
					EditorGUILayout.Space ();
					// Draw Sprout A Hierarchy Structure Panel
					changed |= DrawSproutBStructurePanel ();
					break;
			}
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();
			if (changed) {
				branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				CopyFromProxyBranchLevelDescriptor ();
				CopyFromProxySproutALevelDescriptor ();
				CopyFromProxySproutBLevelDescriptor ();
				selectedBranchDescriptor.activeLevels = activeLevels;
				selectedBranchDescriptor.girthAtBase = girthAtBase;
				selectedBranchDescriptor.girthAtTop = girthAtTop;
				selectedBranchDescriptor.noiseAtBase = noiseAtBase;
				selectedBranchDescriptor.noiseAtTop = noiseAtTop;
				selectedBranchDescriptor.noiseScaleAtBase = noiseScaleAtBase;
				selectedBranchDescriptor.noiseScaleAtTop = noiseScaleAtTop;
				selectedBranchDescriptor.sproutASize = sproutASize;
				selectedBranchDescriptor.sproutAScaleAtBase = sproutAScaleAtBase;
				selectedBranchDescriptor.sproutAScaleAtTop = sproutAScaleAtTop;
				selectedBranchDescriptor.sproutAFlipAlign = sproutAFlipAlign;
				selectedBranchDescriptor.sproutBSize = sproutBSize;
				selectedBranchDescriptor.sproutBScaleAtBase = sproutBScaleAtBase;
				selectedBranchDescriptor.sproutBScaleAtTop = sproutBScaleAtTop;
				selectedBranchDescriptor.sproutBFlipAlign = sproutBFlipAlign;
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				ReflectChangesToPipeline ();
				RegeneratePreview ();
			}
		}
		bool DrawBranchStructurePanel () {
			bool changed = false;
			// Foldouts per hierarchy branch level.
			GUIStyle st = BroccoEditorGUI.foldoutBold;
			GUIStyle stB = BroccoEditorGUI.labelBold;
			for (int i = 0; i <= selectedBranchDescriptor.activeLevels; i++) {
				branchFoldouts [i] = EditorGUILayout.Foldout (branchFoldouts [i], currentStructureSettings.branchEntityName + " Level " + i, BroccoEditorGUI.foldoutBold);
				if (branchFoldouts [i]) {
					EditorGUI.indentLevel++;
					selectedBranchLevelDescriptor = selectedBranchDescriptor.branchLevelDescriptors [i];
					CopyToProxyBranchLevelDescriptor ();
					// Properties for non-root levels.
					if (i == 0) {
						// FREQUENCY
						if (currentStructureSettings.displayFreqAtBaseLevel) {
							changed |= BroccoEditorGUI.IntRangePropertyField (
								ref proxyBranchLevelDescriptor.minFrequency,
								ref proxyBranchLevelDescriptor.maxFrequency,
								1, 12, labelFrequency);
						}
						// LENGTH
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minLengthAtBase,
							ref proxyBranchLevelDescriptor.maxLengthAtBase,
							3, 15, currentStructureSettings.branchEntitiesName + " Length");
						// RADIUS
						if (currentStructureSettings.displayRadiusControl) {
							float newRadius = EditorGUILayout.Slider (labelRadius, proxyBranchLevelDescriptor.radius, 0f, 0.5f);
							if (newRadius != proxyBranchLevelDescriptor.radius) {
								proxyBranchLevelDescriptor.radius = newRadius;
								changed = true;
							}
						}
					} else {
						// FREQUENCY
						changed |= BroccoEditorGUI.IntRangePropertyField (
							ref proxyBranchLevelDescriptor.minFrequency,
							ref proxyBranchLevelDescriptor.maxFrequency,
							1, 12, labelFrequency);
						EditorGUILayout.Space ();
						// LENGTH
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minLengthAtBase,
							ref proxyBranchLevelDescriptor.maxLengthAtBase,
							1, 12, labelLengthAtBase);
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minLengthAtTop,
							ref proxyBranchLevelDescriptor.maxLengthAtTop,
							1, 12, labelLengthAtTop);
						EditorGUILayout.Space ();
						// ALIGNMENT
						// Min Branch Branch Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minParallelAlignAtBase,
							ref proxyBranchLevelDescriptor.maxParallelAlignAtBase,
							-1f, 1f, labelParallelAlignAtBase);
						// Max Branch Branch Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minParallelAlignAtTop,
							ref proxyBranchLevelDescriptor.maxParallelAlignAtTop,
							-1f, 1f, labelParallelAlignAtTop);
						// Min Branch Gravity Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minGravityAlignAtBase,
							ref proxyBranchLevelDescriptor.maxGravityAlignAtBase,
							-1f, 1f, labelGravityAlignAtBase);
						// Max Branch Gravity Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minGravityAlignAtTop,
							ref proxyBranchLevelDescriptor.maxGravityAlignAtTop,
							-1f, 1f, labelGravityAlignAtTop);
						// Min Branch Plane Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minPlaneAlignAtBase,
							ref proxyBranchLevelDescriptor.maxPlaneAlignAtBase,
							-2f, 2f, labelPlaneAlignment);
					}
					EditorGUI.indentLevel--;
					EditorGUILayout.Space ();
				}
				if (changed) break;
			}
			return changed;
		}
		bool DrawSproutAStructurePanel () {
			bool changed = false;
			// Foldouts per hierarchy branch level.
			for (int i = 0; i <= selectedBranchDescriptor.activeLevels; i++) {
				sproutAFoldouts [i] = EditorGUILayout.Foldout (sproutAFoldouts [i], "Sprout Level " + i, BroccoEditorGUI.foldoutBold);
				if (sproutAFoldouts [i]) {
					selectedSproutALevelDescriptor = selectedBranchDescriptor.sproutALevelDescriptors [i];
					CopyToProxySproutALevelDescriptor ();
					// ENABLED
					bool isEnabled = EditorGUILayout.Toggle ("Enabled", proxySproutALevelDescriptor.isEnabled);
					if (isEnabled != proxySproutALevelDescriptor.isEnabled) {
						changed = true;
						proxySproutALevelDescriptor.isEnabled = isEnabled;
					}
					EditorGUILayout.Space ();
					if (isEnabled) {
						// FREQUENCY
						changed |= BroccoEditorGUI.IntRangePropertyField (
							ref proxySproutALevelDescriptor.minFrequency,
							ref proxySproutALevelDescriptor.maxFrequency,
							1, 25, labelFrequency);
						EditorGUILayout.Space ();
						// ALIGNMENT
						// Min Branch Branch Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutALevelDescriptor.minParallelAlignAtBase,
							ref proxySproutALevelDescriptor.maxParallelAlignAtBase,
							-1f, 1f, labelParallelAlignAtBase);
						// Max Branch Branch Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutALevelDescriptor.minParallelAlignAtTop,
							ref proxySproutALevelDescriptor.maxParallelAlignAtTop,
							-1f, 1f, labelParallelAlignAtTop);
						// Min Branch Gravity Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutALevelDescriptor.minGravityAlignAtBase,
							ref proxySproutALevelDescriptor.maxGravityAlignAtBase,
							-1f, 1f, labelGravityAlignAtBase);
						// Max Branch Gravity Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutALevelDescriptor.minGravityAlignAtTop,
							ref proxySproutALevelDescriptor.maxGravityAlignAtTop,
							-1f, 1f, labelGravityAlignAtTop);
						// Min and max range for the sprouts to be spawn.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutALevelDescriptor.minRange,
							ref proxySproutALevelDescriptor.maxRange,
							-1f, 1f, labelBranchRange);
						EditorGUILayout.Space ();
					}
				}
				if (changed) break;
			}
			return changed;
		}
		bool DrawSproutBStructurePanel () {
			bool changed = false;
			// Foldouts per hierarchy branch level.
			for (int i = 0; i <= selectedBranchDescriptor.activeLevels; i++) {
				sproutBFoldouts [i] = EditorGUILayout.Foldout (sproutBFoldouts [i], "Sprout Level " + i, BroccoEditorGUI.foldoutBold);
				if (sproutBFoldouts [i]) {
					selectedSproutBLevelDescriptor = selectedBranchDescriptor.sproutBLevelDescriptors [i];
					CopyToProxySproutBLevelDescriptor ();
					// ENABLED
					bool isEnabled = EditorGUILayout.Toggle (labelEnabled, proxySproutBLevelDescriptor.isEnabled);
					if (isEnabled != proxySproutBLevelDescriptor.isEnabled) {
						changed = true;
						proxySproutBLevelDescriptor.isEnabled = isEnabled;
					}
					EditorGUILayout.Space ();
					if (isEnabled) {
						// FREQUENCY
						changed |= BroccoEditorGUI.IntRangePropertyField (
							ref proxySproutBLevelDescriptor.minFrequency,
							ref proxySproutBLevelDescriptor.maxFrequency,
							1, 25, labelFrequency);
						EditorGUILayout.Space ();
						// ALIGNMENT
						// Min Branch Branch Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutBLevelDescriptor.minParallelAlignAtBase,
							ref proxySproutBLevelDescriptor.maxParallelAlignAtBase,
							-1f, 1f, labelParallelAlignAtBase);
						// Max Branch Branch Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutBLevelDescriptor.minParallelAlignAtTop,
							ref proxySproutBLevelDescriptor.maxParallelAlignAtTop,
							-1f, 1f, labelParallelAlignAtTop);
						// Min Branch Gravity Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutBLevelDescriptor.minGravityAlignAtBase,
							ref proxySproutBLevelDescriptor.maxGravityAlignAtBase,
							-1f, 1f, labelGravityAlignAtBase);
						// Max Branch Gravity Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutBLevelDescriptor.minGravityAlignAtTop,
							ref proxySproutBLevelDescriptor.maxGravityAlignAtTop,
							-1f, 1f, labelGravityAlignAtTop);
						EditorGUILayout.Space ();
					}
				}
				if (changed) break;
			}
			return changed;
		}
		#endregion

		#region Texture Panel
		/// <summary>
		/// Draws the texture panel window view.
		/// </summary>
		public void DrawTexturePanel () {
			bool changed = false;
			showLODOptions = false;
			Texture2D branchAlbedoTexture = branchDescriptorCollection.branchAlbedoTexture;
			Texture2D branchNormalTexture = branchDescriptorCollection.branchNormalTexture;
			float branchTextureYDisplacement = branchDescriptorCollection.branchTextureYDisplacement;
			EditorGUILayout.BeginHorizontal ();
			// View Mode Selection.
			EditorGUILayout.BeginVertical ();
			EditorGUILayout.LabelField (labelStructures, BroccoEditorGUI.labelBoldCentered);
			currentStructureView = GUILayout.SelectionGrid (currentStructureView, structureViewOptions, 1, GUILayout.Width (secondaryPanelColumnWidth));
			EditorGUILayout.EndVertical ();
			// Mapping Settings.
			texturePanelScroll = EditorGUILayout.BeginScrollView (texturePanelScroll, GUILayout.ExpandWidth (true));
			switch (currentStructureView) {
				case STRUCTURE_BRANCH: // BRANCHES.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelBranchTextures, BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginVertical (GUILayout.Width (200));
					branchAlbedoTexture = (Texture2D) EditorGUILayout.ObjectField ("Main Texture", branchDescriptorCollection.branchAlbedoTexture, typeof (Texture2D), false);
					if (branchAlbedoTexture != branchDescriptorCollection.branchAlbedoTexture) {
						changed = true;
					}
					branchNormalTexture = (Texture2D) EditorGUILayout.ObjectField ("Normal Texture", branchDescriptorCollection.branchNormalTexture, typeof (Texture2D), false);
					if (branchNormalTexture != branchDescriptorCollection.branchNormalTexture) {
						changed = true;
					}
					EditorGUILayout.EndVertical ();
					branchTextureYDisplacement = EditorGUILayout.Slider (labelYDisplacement, branchDescriptorCollection.branchTextureYDisplacement, -3f, 4f);
					if (branchTextureYDisplacement != branchDescriptorCollection.branchTextureYDisplacement) {
						changed = true;
					}
					break;
				case STRUCTURE_SPROUT_A: // SPROUT A.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelSproutATextures, BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					sproutMapChanged = false;
					sproutAMapList.DoLayoutList ();
					changed |= sproutMapChanged;
					break;
				case STRUCTURE_SPROUT_B: // SPROUT B.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelSproutBTextures, BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					sproutMapChanged = false;
					sproutBMapList.DoLayoutList ();
					changed |= sproutMapChanged;
					break;
			}
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();
			if (changed) {
				branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				CopyFromProxyBranchLevelDescriptor ();
				branchDescriptorCollection.branchAlbedoTexture = branchAlbedoTexture;
				branchDescriptorCollection.branchNormalTexture = branchNormalTexture;
				branchDescriptorCollection.branchTextureYDisplacement = branchTextureYDisplacement;
				if (sproutMapChanged && selectedSproutMap != null) {
					CopyFromProxySproutMap ();
				}
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				ReflectChangesToPipeline ();
				RegeneratePreview ();
			}
		}
		#endregion

		#region Mapping Panel
		/// <summary>
		/// Draws the mapping panel window view.
		/// </summary>
		public void DrawMappingPanel () {
			showLODOptions = false;
			EditorGUILayout.BeginHorizontal ();
			// View Mode Selection.
			EditorGUILayout.BeginVertical ();
			EditorGUILayout.LabelField (labelMappingView, BroccoEditorGUI.labelBoldCentered);
			int _currentMapView = GUILayout.SelectionGrid (currentMapView, mapViewOptions, 1, GUILayout.Width (secondaryPanelColumnWidth));
			if (_currentMapView != currentMapView) {
				SetMapView (_currentMapView);
			}
			EditorGUILayout.EndVertical ();
			// Mapping Settings.
			mappingPanelScroll = EditorGUILayout.BeginScrollView (mappingPanelScroll, GUILayout.ExpandWidth (true));
			switch (currentMapView) {
				case VIEW_COMPOSITE: // Composite.
					EditorGUILayout.LabelField (labelCompositeMapSettings, BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_MAPPING_COMPOSITE, MessageType.None);
					break;
				case VIEW_ALBEDO: // Albedo.
					EditorGUILayout.LabelField (labelAlbedoMapSettings, BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_MAPPING_ALBEDO, MessageType.None);
					break;
				case VIEW_NORMALS: // Normals.
					EditorGUILayout.LabelField (labelNormalMapSettings, BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_MAPPING_NORMALS, MessageType.None);
					break;
				case VIEW_EXTRAS: // Extras.
					EditorGUILayout.LabelField (labelExtraMapSettings, BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_MAPPING_EXTRA, MessageType.None);
					break;
				case VIEW_SUBSURFACE: // Subsurface
					EditorGUILayout.LabelField (labelSubsurfaceMapSettings, BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_MAPPING_SUBSURFACE, MessageType.None);
					break;
			}
			DrawCompositeMappingSettings ();
			EditorGUILayout.Space ();
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();
		}
		public void DrawCompositeMappingSettings () {
			float branchColorSaturation = branchDescriptorCollection.branchColorSaturation;
			float branchColorShade = branchDescriptorCollection.branchColorShade;

			float minColorShadeA = branchDescriptorCollection.sproutStyleA.minColorShade;
			float maxColorShadeA = branchDescriptorCollection.sproutStyleA.maxColorShade;
			Color tintColorA = branchDescriptorCollection.sproutStyleA.colorTint;
			float minColorTintA = branchDescriptorCollection.sproutStyleA.minColorTint;
			float maxColorTintA = branchDescriptorCollection.sproutStyleA.maxColorTint;
			float metallicA = branchDescriptorCollection.sproutStyleA.metallic;
			float glossinessA = branchDescriptorCollection.sproutStyleA.glossiness;
			float subsurfaceMulA = branchDescriptorCollection.sproutStyleA.subsurfaceMul;
			float colorSaturationA = branchDescriptorCollection.sproutStyleA.colorSaturation;
			
			float minColorShadeB = branchDescriptorCollection.sproutStyleB.minColorShade;
			float maxColorShadeB = branchDescriptorCollection.sproutStyleB.maxColorShade;
			Color tintColorB = branchDescriptorCollection.sproutStyleB.colorTint;
			float minColorTintB = branchDescriptorCollection.sproutStyleB.minColorTint;
			float maxColorTintB = branchDescriptorCollection.sproutStyleB.maxColorTint;
			float metallicB = branchDescriptorCollection.sproutStyleB.metallic;
			float glossinessB = branchDescriptorCollection.sproutStyleB.glossiness;
			float subsurfaceMulB = branchDescriptorCollection.sproutStyleB.subsurfaceMul;
			float colorSaturationB = branchDescriptorCollection.sproutStyleB.colorSaturation;

			EditorGUI.BeginChangeCheck ();

			// Branches
			branchMapFoldout = EditorGUILayout.Foldout (branchMapFoldout, labelBranches);
			if (branchMapFoldout) {
				EditorGUI.indentLevel++;
				//branchColorShade = EditorGUILayout.Slider ("Shade", branchColorShade, 0.6f, 1f);
				branchColorSaturation = EditorGUILayout.Slider (labelSaturation, branchColorSaturation, 0f, 1.5f);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.Space ();

			// Sprout A
			sproutAMapFoldout = EditorGUILayout.Foldout (sproutAMapFoldout, labelSproutA);
			if (sproutAMapFoldout) {
				EditorGUI.indentLevel++;
				// Saturation A
				colorSaturationA = EditorGUILayout.Slider (labelSaturation, colorSaturationA, 0f, 1.5f);

				// Shade A
				BroccoEditorGUI.FloatRangePropertyField (
							ref minColorShadeA,
							ref maxColorShadeA,
							0.65f, 1f, labelShadeRange);
				// Tint A
				tintColorA = EditorGUILayout.ColorField (labelTintColor, tintColorA);
				BroccoEditorGUI.FloatRangePropertyField (
							ref minColorTintA,
							ref maxColorTintA,
							0f, 1f, labelTintRange);
				// Metallic, glossiness, surface A
				metallicA = EditorGUILayout.Slider (labelMetallic, metallicA, 0f, 1f);
				glossinessA = EditorGUILayout.Slider (labelGlossiness, glossinessA, 0f, 1f);

				// Subsurface.
				subsurfaceMulA = EditorGUILayout.Slider (labelTransluscencyFactor, subsurfaceMulA, 0.5f, 1.5f);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.Space ();

			// Sprout B
			sproutBMapFoldout = EditorGUILayout.Foldout (sproutBMapFoldout, labelSproutB);
			if (sproutBMapFoldout) {
				EditorGUI.indentLevel++;
				// Saturation B
				colorSaturationB = EditorGUILayout.Slider (labelSaturation, colorSaturationB, 0f, 1.5f);

				// Shade B
				BroccoEditorGUI.FloatRangePropertyField (
							ref minColorShadeB,
							ref maxColorShadeB,
							0.65f, 1f, labelShadeRange);
				// Tint B
				tintColorB = EditorGUILayout.ColorField (labelTintColor, tintColorB);
				BroccoEditorGUI.FloatRangePropertyField (
							ref minColorTintB,
							ref maxColorTintB,
							0f, 1f, labelTintRange);
				// Metallic, glossiness, surface B
				metallicB = EditorGUILayout.Slider (labelMetallic, metallicB, 0f, 1f);
				glossinessB = EditorGUILayout.Slider (labelGlossiness, glossinessB, 0f, 1f);

				// Subsurface.
				subsurfaceMulB = EditorGUILayout.Slider (labelTransluscencyFactor, subsurfaceMulB, 0.5f, 1.5f);
				EditorGUI.indentLevel--;
			}

			if (EditorGUI.EndChangeCheck ()) {
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				branchDescriptorCollection.branchColorShade = branchColorShade;
				branchDescriptorCollection.branchColorSaturation = branchColorSaturation;
				branchDescriptorCollection.sproutStyleA.minColorShade = minColorShadeA;
				branchDescriptorCollection.sproutStyleA.maxColorShade = maxColorShadeA;
				branchDescriptorCollection.sproutStyleA.colorTint = tintColorA;
				branchDescriptorCollection.sproutStyleA.minColorTint = minColorTintA;
				branchDescriptorCollection.sproutStyleA.maxColorTint = maxColorTintA;
				branchDescriptorCollection.sproutStyleA.metallic = metallicA;
				branchDescriptorCollection.sproutStyleA.glossiness = glossinessA;
				branchDescriptorCollection.sproutStyleA.subsurfaceMul = subsurfaceMulA;
				branchDescriptorCollection.sproutStyleA.colorSaturation = colorSaturationA;
				branchDescriptorCollection.sproutStyleB.minColorShade = minColorShadeB;
				branchDescriptorCollection.sproutStyleB.maxColorShade = maxColorShadeB;
				branchDescriptorCollection.sproutStyleB.colorTint = tintColorB;
				branchDescriptorCollection.sproutStyleB.minColorTint = minColorTintB;
				branchDescriptorCollection.sproutStyleB.maxColorTint = maxColorTintB;
				branchDescriptorCollection.sproutStyleB.metallic = metallicB;
				branchDescriptorCollection.sproutStyleB.glossiness = glossinessB;
				branchDescriptorCollection.sproutStyleB.subsurfaceMul = subsurfaceMulB;
				branchDescriptorCollection.sproutStyleB.colorSaturation = colorSaturationB;
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				ReflectChangesToPipeline ();
				RegeneratePreview (currentMapView);
			}
		}
		#endregion

		#region Export Panel
		/// <summary>
		/// Draws the export panel window view.
		/// </summary>
		public void DrawExportPanel () {
			bool changed = false;
			showLODOptions = false;
			EditorGUILayout.BeginHorizontal ();
			
			// View Mode Selection.
			EditorGUILayout.BeginVertical ();
			if (currentStructureSettings.displayExportDescriptor && 
				!currentStructureSettings.displayExportPrefab &&
				!currentStructureSettings.displayExportTextures)
			{
				currentExportView = EXPORT_DESCRIPTOR;
			} else if (!currentStructureSettings.displayExportDescriptor && 
				currentStructureSettings.displayExportPrefab &&
				!currentStructureSettings.displayExportTextures)
			{
				currentExportView = EXPORT_PREFAB;
			} else if (!currentStructureSettings.displayExportDescriptor && 
				!currentStructureSettings.displayExportPrefab &&
				currentStructureSettings.displayExportTextures)
			{
				currentExportView = EXPORT_TEXTURES;
			} else {
				EditorGUILayout.LabelField (labelExportOptions, BroccoEditorGUI.labelBoldCentered);
				if (GlobalSettings.experimentalAdvancedSproutLab) {
					if (currentStructureSettings.displayExportDescriptor) {
						if (GUILayout.Button (exportViewOptionDescriptorGUI)) currentExportView = EXPORT_DESCRIPTOR;
					}
				} else {
					currentExportView = EXPORT_TEXTURES;
				}
				if (currentStructureSettings.displayExportPrefab) {
					if (GUILayout.Button (exportViewOptionPrefabGUI)) currentExportView = EXPORT_PREFAB;
				}
				if (currentStructureSettings.displayExportTextures) {
					if (GUILayout.Button (exportViewOptionTexturesGUI)) currentExportView = EXPORT_TEXTURES;
				}
				EditorGUILayout.Space ();
			}

			EditorGUILayout.LabelField (labelImportOptions, BroccoEditorGUI.labelBoldCentered);
			editorPersistence.DrawOptions ();
			EditorGUILayout.EndVertical ();
			// Export Settings.
			exportPanelScroll = EditorGUILayout.BeginScrollView (exportPanelScroll, GUILayout.ExpandWidth (true));
			BranchDescriptorCollection.TextureSize exportTextureSize;
			int exportTake;
			string exportPrefix;
			int paddingSize;
			bool isValid;
			bool isAtlas;
			bool isValidTemp;
			string albedoPath;
			string normalsPath;
			string extrasPath;
			string subsurfacePath;
			int exportFlags;
			BranchDescriptorCollection.ExportMode exportMode;
			string subfolder;
			switch (currentExportView) {
				case EXPORT_DESCRIPTOR: // Branch Descriotor.
					EditorGUILayout.LabelField (labelBranchDescExportSettings, BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_EXPORT_DESCRIPTOR, MessageType.None);
					// SNAPSHOT PROCESSING.
					if (GUILayout.Button (exportDescriptorAndAtlasGUI)) {
						ExportDescriptor (true);
					}
					EditorGUILayout.Space ();
					// ATLAS TEXTURE.
					EditorGUILayout.LabelField (labelAtlasTextureSettings, BroccoEditorGUI.labelBold);
					// Atlas size.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelAtlasSize, BroccoEditorGUI.label);
					exportTextureSize = 
						(BranchDescriptorCollection.TextureSize)EditorGUILayout.EnumPopup (branchDescriptorCollection.exportTextureSize, GUILayout.Width (120));
					changed |= exportTextureSize != branchDescriptorCollection.exportTextureSize;
					EditorGUILayout.EndHorizontal ();
					// Atlas padding.
					paddingSize = EditorGUILayout.IntField (labelPadding, branchDescriptorCollection.exportAtlasPadding);
					if (paddingSize < 0 || paddingSize > 25) {
						paddingSize = branchDescriptorCollection.exportAtlasPadding;			
					}
					changed |= paddingSize != branchDescriptorCollection.exportAtlasPadding;
					// Export take.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelTake, BroccoEditorGUI.label);
					exportTake = EditorGUILayout.IntField (branchDescriptorCollection.exportTake);
					changed |= exportTake != branchDescriptorCollection.exportTake;
					EditorGUILayout.EndHorizontal ();
					// Export prefix.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelPrefix, BroccoEditorGUI.label);
					exportPrefix = EditorGUILayout.TextField (branchDescriptorCollection.exportPrefix);
					changed |= !exportPrefix.Equals (branchDescriptorCollection.exportPrefix);
					EditorGUILayout.EndHorizontal ();
					// Export path.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelPath, BroccoEditorGUI.label);
					EditorGUILayout.LabelField ("/Assets" + branchDescriptorCollection.exportPath);
					if (GUILayout.Button (selectPathGUI, GUILayout.Width (30))) {
						string currentPath = Application.dataPath + branchDescriptorCollection.exportPath;
						string selectedPath = EditorUtility.OpenFolderPanel (labelTexturesFolder, currentPath, "");
						if (!string.IsNullOrEmpty (selectedPath)) {
							selectedPath = selectedPath.Substring (Application.dataPath.Length);
							if (selectedPath.CompareTo (branchDescriptorCollection.exportPath) != 0) {
								branchDescriptorCollection.exportPath = selectedPath;
								changed = true;
							}
						}
						GUIUtility.ExitGUI();
					}
					EditorGUILayout.EndHorizontal ();

					// List of paths
					isValid = false; isValidTemp = false;
					isAtlas = branchDescriptorCollection.exportMode == BranchDescriptorCollection.ExportMode.Atlas;
					subfolder = branchDescriptorCollection.exportPrefix + FileUtils.GetFileTakeSuffix (branchDescriptorCollection.exportTake);
					albedoPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Albedo, isAtlas);
					normalsPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Normals, isAtlas);
					extrasPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Extras, isAtlas);
					subsurfacePath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Subsurface, isAtlas);
					EditorGUILayout.HelpBox (albedoPath + "\n" + normalsPath + "\n" + extrasPath + "\n" + subsurfacePath, MessageType.None);

					// Export textures flags
					exportFlags = EditorGUILayout.MaskField(labelTextures, branchDescriptorCollection.exportTexturesFlags, exportTextureOptions);
					changed |= exportFlags != branchDescriptorCollection.exportTexturesFlags;
					if (changed) {
						branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
						onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
						branchDescriptorCollection.exportTextureSize = exportTextureSize;
						branchDescriptorCollection.exportAtlasPadding = paddingSize;
						branchDescriptorCollection.exportTake = exportTake;
						branchDescriptorCollection.exportPrefix = exportPrefix;
						branchDescriptorCollection.exportTexturesFlags = exportFlags;
						onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
					}
					break;
				case EXPORT_PREFAB: // Texture.
					EditorGUILayout.LabelField (labelPrefabSettings, BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_EXPORT_PREFAB, MessageType.None);
					if (GUILayout.Button (exportPrefabGUI)) {
						ExportPrefab ();
					}
					EditorGUILayout.Space ();

					// OUTPUT FILE
					EditorGUILayout.LabelField (labelPrefabFileSettings, BroccoEditorGUI.labelBold);
					// Export path.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelPath, BroccoEditorGUI.label);
					EditorGUILayout.LabelField ("Assets" + branchDescriptorCollection.exportPath);
					if (GUILayout.Button (selectPathGUI, GUILayout.Width (30))) {
						string currentPath = Application.dataPath + branchDescriptorCollection.exportPath;
						string selectedPath = EditorUtility.OpenFolderPanel (labelTexturesFolder, currentPath, "");
						if (!string.IsNullOrEmpty (selectedPath)) {
							selectedPath = selectedPath.Substring (Application.dataPath.Length);
							if (selectedPath.CompareTo (branchDescriptorCollection.exportPath) != 0) {
								branchDescriptorCollection.exportPath = selectedPath;
								changed = true;
							}
						}
						GUIUtility.ExitGUI();
					}
					EditorGUILayout.EndHorizontal ();
					// Export prefix.
					exportPrefix = EditorGUILayout.TextField (labelPrefix, branchDescriptorCollection.exportPrefix);
					changed |= !exportPrefix.Equals (branchDescriptorCollection.exportPrefix);
					// Export take.
					exportTake = EditorGUILayout.IntField (labelTake, branchDescriptorCollection.exportTake);
					changed |= exportTake != branchDescriptorCollection.exportTake;
					string prefabPath = FileUtils.GetFilePath ("Assets" + branchDescriptorCollection.exportPath, branchDescriptorCollection.exportPrefix,
						"prefab", branchDescriptorCollection.exportTake);
					EditorGUILayout.HelpBox (prefabPath, MessageType.None);
					EditorGUILayout.Space ();

					// TEXTURES
					EditorGUILayout.LabelField (labelPrefabTextureSettings, BroccoEditorGUI.labelBold);
					// Atlas size.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelAtlasSize, BroccoEditorGUI.label);
					exportTextureSize = 
						(BranchDescriptorCollection.TextureSize)EditorGUILayout.EnumPopup (branchDescriptorCollection.exportTextureSize, GUILayout.Width (120));
					changed |= exportTextureSize != branchDescriptorCollection.exportTextureSize;
					EditorGUILayout.EndHorizontal ();
					// Atlas padding.
					paddingSize = EditorGUILayout.IntField (labelPadding, branchDescriptorCollection.exportAtlasPadding);
					if (paddingSize < 0 || paddingSize > 25) {
						paddingSize = branchDescriptorCollection.exportAtlasPadding;
					}
					changed |= paddingSize != branchDescriptorCollection.exportAtlasPadding;
					EditorGUILayout.Space ();

					// List of paths
					isValid = false; isValidTemp = false;
					isAtlas = branchDescriptorCollection.exportMode == BranchDescriptorCollection.ExportMode.Atlas;
					subfolder = branchDescriptorCollection.exportPrefix + FileUtils.GetFileTakeSuffix (branchDescriptorCollection.exportTake);
					albedoPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Albedo, isAtlas);
					normalsPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Normals, isAtlas);
					extrasPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Extras, isAtlas);
					subsurfacePath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Subsurface, isAtlas);
					EditorGUILayout.HelpBox (albedoPath + "\n" + normalsPath + "\n" + extrasPath + "\n" + subsurfacePath, MessageType.None);
					
					// Export textures flags
					exportFlags = EditorGUILayout.MaskField(labelTextures, branchDescriptorCollection.exportTexturesFlags, exportTextureOptions);
					changed |= exportFlags != branchDescriptorCollection.exportTexturesFlags;
					if (changed) {
						branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
						onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
						branchDescriptorCollection.exportPrefix = exportPrefix;
						branchDescriptorCollection.exportTextureSize = exportTextureSize;
						branchDescriptorCollection.exportAtlasPadding = paddingSize;
						branchDescriptorCollection.exportTake = exportTake;
						branchDescriptorCollection.exportTexturesFlags = exportFlags;
						onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
					}
					
					break;
				case EXPORT_TEXTURES: // Texture.
					EditorGUILayout.LabelField (labelTextureExportSettings, BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_EXPORT_TEXTURE, MessageType.None);
					if (GUILayout.Button (exportTexturesGUI)) {
						ExportTextures ();
					}
					EditorGUILayout.Space ();
					// Atlas size.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelAtlasSize, BroccoEditorGUI.label);
					exportTextureSize = 
						(BranchDescriptorCollection.TextureSize)EditorGUILayout.EnumPopup (branchDescriptorCollection.exportTextureSize, GUILayout.Width (120));
					changed |= exportTextureSize != branchDescriptorCollection.exportTextureSize;
					EditorGUILayout.EndHorizontal ();
					// Atlas padding.
					paddingSize = EditorGUILayout.IntField (labelPadding, branchDescriptorCollection.exportAtlasPadding);
					if (paddingSize < 0 || paddingSize > 25) {
						paddingSize = branchDescriptorCollection.exportAtlasPadding;			
					}
					changed |= paddingSize != branchDescriptorCollection.exportAtlasPadding;
					EditorGUILayout.Space ();
					// OUTPUT FILE
					EditorGUILayout.LabelField (labelOutputFile, BroccoEditorGUI.labelBold);
					// Export mode.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelExportMode, BroccoEditorGUI.label);
					exportMode = 
						(BranchDescriptorCollection.ExportMode)EditorGUILayout.EnumPopup (branchDescriptorCollection.exportMode, GUILayout.Width (120));
					changed |= exportMode != branchDescriptorCollection.exportMode;
					EditorGUILayout.EndHorizontal ();
					// Export take.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelTake, BroccoEditorGUI.label);
					exportTake = EditorGUILayout.IntField (branchDescriptorCollection.exportTake);
					changed |= exportTake != branchDescriptorCollection.exportTake;
					EditorGUILayout.EndHorizontal ();
					// Export path.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (labelPath, BroccoEditorGUI.label);
					EditorGUILayout.LabelField ("/Assets" + branchDescriptorCollection.exportPath);
					if (GUILayout.Button (selectPathGUI, GUILayout.Width (30))) {
						string currentPath = Application.dataPath + branchDescriptorCollection.exportPath;
						string selectedPath = EditorUtility.OpenFolderPanel (labelTexturesFolder, currentPath, "");
						if (!string.IsNullOrEmpty (selectedPath)) {
							selectedPath = selectedPath.Substring (Application.dataPath.Length);
							if (selectedPath.CompareTo (branchDescriptorCollection.exportPath) != 0) {
								branchDescriptorCollection.exportPath = selectedPath;
								changed = true;
							}
						}
						GUIUtility.ExitGUI();
					}
					EditorGUILayout.EndHorizontal ();
					// List of paths
					isValid = false; isValidTemp = false;
					isAtlas = branchDescriptorCollection.exportMode == BranchDescriptorCollection.ExportMode.Atlas;
					subfolder = branchDescriptorCollection.exportPrefix + FileUtils.GetFileTakeSuffix (branchDescriptorCollection.exportTake);
					albedoPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Albedo, isAtlas);
					normalsPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Normals, isAtlas);
					extrasPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Extras, isAtlas);
					subsurfacePath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Subsurface, isAtlas);
					EditorGUILayout.HelpBox (albedoPath + "\n" + normalsPath + "\n" + extrasPath + "\n" + subsurfacePath, MessageType.None);
					// Export textures flags
					exportFlags = EditorGUILayout.MaskField(labelTextures, branchDescriptorCollection.exportTexturesFlags, exportTextureOptions);
					changed |= exportFlags != branchDescriptorCollection.exportTexturesFlags;
					if (changed) {
						branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
						onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
						branchDescriptorCollection.exportTextureSize = exportTextureSize;
						branchDescriptorCollection.exportAtlasPadding = paddingSize;
						branchDescriptorCollection.exportMode = exportMode;
						branchDescriptorCollection.exportTake = exportTake;
						branchDescriptorCollection.exportTexturesFlags = exportFlags;
						onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
					}
					break;
			}
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();
		}
		#endregion

		#region Debug Panel
		/// <summary>
		/// Draw the debug panel window mode.
		/// </summary>
		public void DrawDebugPanel () {
			showLODOptions = true;
			EditorGUILayout.BeginHorizontal ();
			// View Mode Selection.
			EditorGUILayout.BeginVertical ();
			EditorGUILayout.LabelField ("Debug", BroccoEditorGUI.labelBoldCentered);
			int _currentDebugView = GUILayout.SelectionGrid (currentDebugView, debugViewOptions, 1, GUILayout.Width (secondaryPanelColumnWidth));
			if (_currentDebugView != currentDebugView) {
				currentDebugView = _currentDebugView; 
				/*
				if (currentDebugView == DEBUG_MESHING) {
					meshPreview.hasSecondPass = false;
					ShowPreviewMesh (_polygonArea.mesh);
				} else {
					ShowPreviewMesh ();
					SetMapView (VIEW_COMPOSITE, true);
				}
				*/
			}
			EditorGUILayout.EndVertical ();
			// Debugging Settings.
			debugPanelScroll = EditorGUILayout.BeginScrollView (debugPanelScroll, GUILayout.ExpandWidth (true));
			switch (currentDebugView) {
				case DEBUG_GEOMETRY: // GEOMETRY.
					EditorGUILayout.LabelField ("Factory scale: " + sproutSubfactory.factoryScale);
					debugPolyIndex = EditorGUILayout.Popup (debugPolyIndex, debugPolygonOptions, GUILayout.Width (secondaryPanelColumnWidth));
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Geometry Debug", BroccoEditorGUI.labelBoldCentered);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					if (selectedBranchDescriptor != null) {
						string snapshotInfo = $"Snapshot [id: {selectedBranchDescriptor.id}]";
						EditorGUILayout.HelpBox (snapshotInfo, MessageType.None);
					}
					debugShowTopoPoints = EditorGUILayout.Toggle ("Show Topo Points", debugShowTopoPoints);
					debugShowConvexHullPoints = EditorGUILayout.Toggle ("Show Convex Hull Points", debugShowConvexHullPoints);
					debugShowConvexHullPointsOrder = EditorGUILayout.Toggle ("Show Convex Hull Points Order", debugShowConvexHullPointsOrder);
					debugShowConvexHull = EditorGUILayout.Toggle ("Show Convex Hull", debugShowConvexHull);
					debugShowAABB = EditorGUILayout.Toggle ("Show AABB", debugShowAABB);
					debugShowOBB = EditorGUILayout.Toggle ("Show OBB", debugShowOBB);
					debugShowTris = EditorGUILayout.Toggle ("Show Tris", debugShowTris);
					//EditorGUILayout.LabelField ($"OBB angle: {_obbAngle}");
					bool _debugSkipSimplifyHull = EditorGUILayout.Toggle ("Skip Simplify Hull", debugSkipSimplifyHull);
					if (_debugSkipSimplifyHull != debugSkipSimplifyHull) {
						debugSkipSimplifyHull = _debugSkipSimplifyHull;
						RegeneratePreview ();
					}
					int _lods = EditorGUILayout.IntSlider ("LODs", selectedBranchDescriptor.lodCount, 1, 3);
					if (_lods != selectedBranchDescriptor.lodCount) {
						selectedBranchDescriptor.lodCount = _lods;
					}
					break;
				case DEBUG_CANVAS: // CANVAS.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Canvas Debug", BroccoEditorGUI.labelBoldCentered);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.HelpBox (meshPreview.GetDebugInfo (), MessageType.None);
					//MeshPreview Target Rotation Offset.
					Vector3 rotationOffset = EditorGUILayout.Vector3Field ("Rotation Offset", meshPreview.targetRotationOffset.eulerAngles);
					if (rotationOffset != meshPreview.targetRotationOffset.eulerAngles) {
						meshPreview.targetRotationOffset.eulerAngles = rotationOffset;
					}
					//MeshPreview Target Position Offset.
					Vector3 positionOffset = EditorGUILayout.Vector3Field ("Position Offset", meshPreview.targetPositionOffset);
					if (positionOffset != meshPreview.targetPositionOffset) {
						meshPreview.targetPositionOffset = positionOffset;
					}
					// MeshPreview Light A.
					Light lightA = meshPreview.GetLightA ();
					float lightAIntensity = EditorGUILayout.FloatField ("Light A Intensity", lightA.intensity);
					if (lightA.intensity != lightAIntensity) {
						lightA.intensity = lightAIntensity;
					}
					Vector3 lightARotation = EditorGUILayout.Vector3Field ("Light A Rot", lightA.transform.rotation.eulerAngles);
					if (lightARotation != lightA.transform.rotation.eulerAngles) {
						lightA.transform.root.eulerAngles = lightARotation;
					}
					break;
				case DEBUG_MESHING: // MESHING.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Meshing Debug", BroccoEditorGUI.labelBoldCentered);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					bool _debugShowMeshWireframe = EditorGUILayout.Toggle ("Show Wireframe", debugShowMeshWireframe);
					if (_debugShowMeshWireframe != debugShowMeshWireframe) {
						debugShowMeshWireframe = _debugShowMeshWireframe;
						meshPreview.showWireframe = debugShowMeshWireframe;
					}
					debugShowMeshNormals = EditorGUILayout.Toggle ("Show Normals", debugShowMeshNormals);
					debugShowMeshTangents = EditorGUILayout.Toggle ("Show Tangents", debugShowMeshTangents);
					EditorGUILayout.BeginHorizontal ();
					debugClearTargetId = EditorGUILayout.IntField ("Target Branch", debugClearTargetId);
					if (GUILayout.Button ("Set Alpha color to 0")) {
						Mesh m = sproutSubfactory.snapshotTreeMesh;
						Color[] colors = m.colors;
						List<Vector4> uv6 = new List<Vector4> ();
						m.GetUVs (5, uv6);
						for (int i = 0; i < colors.Length; i++) {
							if (uv6 [i].x == debugClearTargetId) {
								colors[i].a = 0;
							}
						}
						m.colors = colors;
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					debugAtlas = (Texture2D)EditorGUILayout.ObjectField ("Atlas", debugAtlas, typeof(Texture2D), true);
					if (GUILayout.Button ("Show Atlased Mesh")) {
						// Regenerate the meshes for the polygons.
						var polys = sproutSubfactory.sproutCompositeManager.polygonAreas;
						var enumPolys = polys.GetEnumerator ();
						while (enumPolys.MoveNext ()) {
							Broccoli.Builder.PolygonAreaBuilder.SetPolygonAreaMesh (enumPolys.Current.Value);
						} 
						meshPreview.hasSecondPass = false;
						Mesh lodMesh = sproutSubfactory.sproutCompositeManager.GetMesh (selectedBranchDescriptor.id, selectedLODView - 1, false);
						Material[] mats = sproutSubfactory.sproutCompositeManager.GetMaterials (selectedBranchDescriptor.id, selectedLODView - 1);
						Material uniqueMat = SproutCompositeManager.GenerateMaterial (Color.white, 0.3f, 0.1f, 0.1f, 0.5f, Color.white,
							debugAtlas, null, null, null);
						for (int i = 0; i < mats.Length; i++) {
							mats [i] = uniqueMat;
						}
						ShowPreviewMesh (lodMesh, mats);
					}
					EditorGUILayout.EndHorizontal ();
					break;
				case DEBUG_TEXTURES: // TEXTURES.
					if (GUILayout.Button ("Generate Snapshot")) {
						//ExportDebugTexturesSingleSnapshot ();
					}
					break;
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndScrollView ();
		}
		#endregion

		#region Undo
		void CopyToProxyBranchLevelDescriptor () {
			proxyBranchLevelDescriptor.minFrequency = selectedBranchLevelDescriptor.minFrequency;
			proxyBranchLevelDescriptor.maxFrequency = selectedBranchLevelDescriptor.maxFrequency;
			proxyBranchLevelDescriptor.radius = selectedBranchLevelDescriptor.radius;
			proxyBranchLevelDescriptor.minLengthAtBase = selectedBranchLevelDescriptor.minLengthAtBase;
			proxyBranchLevelDescriptor.maxLengthAtBase = selectedBranchLevelDescriptor.maxLengthAtBase;
			proxyBranchLevelDescriptor.minLengthAtTop = selectedBranchLevelDescriptor.minLengthAtTop;
			proxyBranchLevelDescriptor.maxLengthAtTop = selectedBranchLevelDescriptor.maxLengthAtTop;
			proxyBranchLevelDescriptor.minParallelAlignAtTop = selectedBranchLevelDescriptor.minParallelAlignAtTop;
			proxyBranchLevelDescriptor.maxParallelAlignAtTop = selectedBranchLevelDescriptor.maxParallelAlignAtTop;
			proxyBranchLevelDescriptor.minParallelAlignAtBase = selectedBranchLevelDescriptor.minParallelAlignAtBase;
			proxyBranchLevelDescriptor.maxParallelAlignAtBase = selectedBranchLevelDescriptor.maxParallelAlignAtBase;
			proxyBranchLevelDescriptor.minGravityAlignAtTop = selectedBranchLevelDescriptor.minGravityAlignAtTop;
			proxyBranchLevelDescriptor.maxGravityAlignAtTop = selectedBranchLevelDescriptor.maxGravityAlignAtTop;
			proxyBranchLevelDescriptor.minGravityAlignAtBase = selectedBranchLevelDescriptor.minGravityAlignAtBase;
			proxyBranchLevelDescriptor.maxGravityAlignAtBase = selectedBranchLevelDescriptor.maxGravityAlignAtBase;
			proxyBranchLevelDescriptor.minPlaneAlignAtTop = selectedBranchLevelDescriptor.minPlaneAlignAtTop;
			proxyBranchLevelDescriptor.maxPlaneAlignAtTop = selectedBranchLevelDescriptor.maxPlaneAlignAtTop;
			proxyBranchLevelDescriptor.minPlaneAlignAtBase = selectedBranchLevelDescriptor.minPlaneAlignAtBase;
			proxyBranchLevelDescriptor.maxPlaneAlignAtBase = selectedBranchLevelDescriptor.maxPlaneAlignAtBase;
		}
		void CopyFromProxyBranchLevelDescriptor () {
			if (selectedBranchLevelDescriptor != null) {
				selectedBranchLevelDescriptor.minFrequency = proxyBranchLevelDescriptor.minFrequency;
				selectedBranchLevelDescriptor.maxFrequency = proxyBranchLevelDescriptor.maxFrequency;
				selectedBranchLevelDescriptor.radius = proxyBranchLevelDescriptor.radius;
				selectedBranchLevelDescriptor.minLengthAtBase = proxyBranchLevelDescriptor.minLengthAtBase;
				selectedBranchLevelDescriptor.maxLengthAtBase = proxyBranchLevelDescriptor.maxLengthAtBase;
				selectedBranchLevelDescriptor.minLengthAtTop = proxyBranchLevelDescriptor.minLengthAtTop;
				selectedBranchLevelDescriptor.maxLengthAtTop = proxyBranchLevelDescriptor.maxLengthAtTop;
				selectedBranchLevelDescriptor.minParallelAlignAtTop = proxyBranchLevelDescriptor.minParallelAlignAtTop;
				selectedBranchLevelDescriptor.maxParallelAlignAtTop = proxyBranchLevelDescriptor.maxParallelAlignAtTop;
				selectedBranchLevelDescriptor.minParallelAlignAtBase = proxyBranchLevelDescriptor.minParallelAlignAtBase;
				selectedBranchLevelDescriptor.maxParallelAlignAtBase = proxyBranchLevelDescriptor.maxParallelAlignAtBase;
				selectedBranchLevelDescriptor.minGravityAlignAtTop = proxyBranchLevelDescriptor.minGravityAlignAtTop;
				selectedBranchLevelDescriptor.maxGravityAlignAtTop = proxyBranchLevelDescriptor.maxGravityAlignAtTop;
				selectedBranchLevelDescriptor.minGravityAlignAtBase = proxyBranchLevelDescriptor.minGravityAlignAtBase;
				selectedBranchLevelDescriptor.maxGravityAlignAtBase = proxyBranchLevelDescriptor.maxGravityAlignAtBase;
				selectedBranchLevelDescriptor.minPlaneAlignAtTop = proxyBranchLevelDescriptor.minPlaneAlignAtTop;
				selectedBranchLevelDescriptor.maxPlaneAlignAtTop = proxyBranchLevelDescriptor.maxPlaneAlignAtTop;
				selectedBranchLevelDescriptor.minPlaneAlignAtBase = proxyBranchLevelDescriptor.minPlaneAlignAtBase;
				selectedBranchLevelDescriptor.maxPlaneAlignAtBase = proxyBranchLevelDescriptor.maxPlaneAlignAtBase;
			}
		}
		void CopyToProxySproutALevelDescriptor () {
			proxySproutALevelDescriptor.isEnabled = selectedSproutALevelDescriptor.isEnabled;
			proxySproutALevelDescriptor.minFrequency = selectedSproutALevelDescriptor.minFrequency;
			proxySproutALevelDescriptor.maxFrequency = selectedSproutALevelDescriptor.maxFrequency;
			proxySproutALevelDescriptor.minParallelAlignAtTop = selectedSproutALevelDescriptor.minParallelAlignAtTop;
			proxySproutALevelDescriptor.maxParallelAlignAtTop = selectedSproutALevelDescriptor.maxParallelAlignAtTop;
			proxySproutALevelDescriptor.minParallelAlignAtBase = selectedSproutALevelDescriptor.minParallelAlignAtBase;
			proxySproutALevelDescriptor.maxParallelAlignAtBase = selectedSproutALevelDescriptor.maxParallelAlignAtBase;
			proxySproutALevelDescriptor.minGravityAlignAtTop = selectedSproutALevelDescriptor.minGravityAlignAtTop;
			proxySproutALevelDescriptor.maxGravityAlignAtTop = selectedSproutALevelDescriptor.maxGravityAlignAtTop;
			proxySproutALevelDescriptor.minGravityAlignAtBase = selectedSproutALevelDescriptor.minGravityAlignAtBase;
			proxySproutALevelDescriptor.maxGravityAlignAtBase = selectedSproutALevelDescriptor.maxGravityAlignAtBase;
			proxySproutALevelDescriptor.minRange = selectedSproutALevelDescriptor.minRange;
			proxySproutALevelDescriptor.maxRange = selectedSproutALevelDescriptor.maxRange;
		}
		void CopyFromProxySproutALevelDescriptor () {
			if (selectedSproutALevelDescriptor != null) {
				selectedSproutALevelDescriptor.isEnabled = proxySproutALevelDescriptor.isEnabled;
				selectedSproutALevelDescriptor.minFrequency = proxySproutALevelDescriptor.minFrequency;
				selectedSproutALevelDescriptor.maxFrequency = proxySproutALevelDescriptor.maxFrequency;
				selectedSproutALevelDescriptor.minParallelAlignAtTop = proxySproutALevelDescriptor.minParallelAlignAtTop;
				selectedSproutALevelDescriptor.maxParallelAlignAtTop = proxySproutALevelDescriptor.maxParallelAlignAtTop;
				selectedSproutALevelDescriptor.minParallelAlignAtBase = proxySproutALevelDescriptor.minParallelAlignAtBase;
				selectedSproutALevelDescriptor.maxParallelAlignAtBase = proxySproutALevelDescriptor.maxParallelAlignAtBase;
				selectedSproutALevelDescriptor.minGravityAlignAtTop = proxySproutALevelDescriptor.minGravityAlignAtTop;
				selectedSproutALevelDescriptor.maxGravityAlignAtTop = proxySproutALevelDescriptor.maxGravityAlignAtTop;
				selectedSproutALevelDescriptor.minGravityAlignAtBase = proxySproutALevelDescriptor.minGravityAlignAtBase;
				selectedSproutALevelDescriptor.maxGravityAlignAtBase = proxySproutALevelDescriptor.maxGravityAlignAtBase;
				selectedSproutALevelDescriptor.minRange = proxySproutALevelDescriptor.minRange;
				selectedSproutALevelDescriptor.maxRange = proxySproutALevelDescriptor.maxRange;
			}
		}
		void CopyToProxySproutBLevelDescriptor () {
			proxySproutBLevelDescriptor.isEnabled = selectedSproutBLevelDescriptor.isEnabled;
			proxySproutBLevelDescriptor.minFrequency = selectedSproutBLevelDescriptor.minFrequency;
			proxySproutBLevelDescriptor.maxFrequency = selectedSproutBLevelDescriptor.maxFrequency;
			proxySproutBLevelDescriptor.minParallelAlignAtTop = selectedSproutBLevelDescriptor.minParallelAlignAtTop;
			proxySproutBLevelDescriptor.maxParallelAlignAtTop = selectedSproutBLevelDescriptor.maxParallelAlignAtTop;
			proxySproutBLevelDescriptor.minParallelAlignAtBase = selectedSproutBLevelDescriptor.minParallelAlignAtBase;
			proxySproutBLevelDescriptor.maxParallelAlignAtBase = selectedSproutBLevelDescriptor.maxParallelAlignAtBase;
			proxySproutBLevelDescriptor.minGravityAlignAtTop = selectedSproutBLevelDescriptor.minGravityAlignAtTop;
			proxySproutBLevelDescriptor.maxGravityAlignAtTop = selectedSproutBLevelDescriptor.maxGravityAlignAtTop;
			proxySproutBLevelDescriptor.minGravityAlignAtBase = selectedSproutBLevelDescriptor.minGravityAlignAtBase;
			proxySproutBLevelDescriptor.maxGravityAlignAtBase = selectedSproutBLevelDescriptor.maxGravityAlignAtBase;
			proxySproutBLevelDescriptor.minRange = selectedSproutBLevelDescriptor.minRange;
			proxySproutBLevelDescriptor.maxRange = selectedSproutBLevelDescriptor.maxRange;
		}
		void CopyFromProxySproutBLevelDescriptor () {
			if (selectedSproutBLevelDescriptor != null) {
				selectedSproutBLevelDescriptor.isEnabled = proxySproutBLevelDescriptor.isEnabled;
				selectedSproutBLevelDescriptor.minFrequency = proxySproutBLevelDescriptor.minFrequency;
				selectedSproutBLevelDescriptor.maxFrequency = proxySproutBLevelDescriptor.maxFrequency;
				selectedSproutBLevelDescriptor.minParallelAlignAtTop = proxySproutBLevelDescriptor.minParallelAlignAtTop;
				selectedSproutBLevelDescriptor.maxParallelAlignAtTop = proxySproutBLevelDescriptor.maxParallelAlignAtTop;
				selectedSproutBLevelDescriptor.minParallelAlignAtBase = proxySproutBLevelDescriptor.minParallelAlignAtBase;
				selectedSproutBLevelDescriptor.maxParallelAlignAtBase = proxySproutBLevelDescriptor.maxParallelAlignAtBase;
				selectedSproutBLevelDescriptor.minGravityAlignAtTop = proxySproutBLevelDescriptor.minGravityAlignAtTop;
				selectedSproutBLevelDescriptor.maxGravityAlignAtTop = proxySproutBLevelDescriptor.maxGravityAlignAtTop;
				selectedSproutBLevelDescriptor.minGravityAlignAtBase = proxySproutBLevelDescriptor.minGravityAlignAtBase;
				selectedSproutBLevelDescriptor.maxGravityAlignAtBase = proxySproutBLevelDescriptor.maxGravityAlignAtBase;
				selectedSproutBLevelDescriptor.minRange = proxySproutBLevelDescriptor.minRange;
				selectedSproutBLevelDescriptor.maxRange = proxySproutBLevelDescriptor.maxRange;
			}
		}
		void CopyToProxySproutMap () {
			if (selectedSproutMap != null) {
				proxySproutMap.texture = selectedSproutMap.texture;
				proxySproutMap.normalMap = selectedSproutMap.normalMap;
				proxySproutMap.extraMap = selectedSproutMap.extraMap;
				proxySproutMap.subsurfaceMap = selectedSproutMap.subsurfaceMap;
				if (proxySproutMapDescriptor != null && selectedSproutMapDescriptor != null) {
					proxySproutMapDescriptor.alphaFactor = selectedSproutMapDescriptor.alphaFactor;
				}
			}
		}
		void CopyFromProxySproutMap () {
			selectedSproutMap.texture = proxySproutMap.texture;
			selectedSproutMap.normalMap = proxySproutMap.normalMap;
			selectedSproutMap.extraMap = proxySproutMap.extraMap;
			selectedSproutMap.subsurfaceMap = proxySproutMap.subsurfaceMap;
			if (selectedSproutMapDescriptor != null && selectedSproutMapDescriptor != null) {
				selectedSproutMapDescriptor.alphaFactor = proxySproutMapDescriptor.alphaFactor;
			}
		}
		#endregion

		#region Snapshots
		/// <summary>
		/// Initializes the view options for the snapshots present in the loaded collection.
		/// </summary>
		void InitSnapshotViewOptions () {
			// Build GUIContents per branch.
			snapshotViewOptions = new GUIContent[branchDescriptorCollection.branchDescriptors.Count];
			for (int i = 0; i < branchDescriptorCollection.branchDescriptors.Count; i++) {
				snapshotViewOptions [i] = new GUIContent ("S" + i);
			}
		}
		/// <summary>
		/// Draws the list of snapshots on this collection.
		/// </summary>
		void DrawSnapshotsPanel () {
			EditorGUILayout.BeginHorizontal ();
			EditorGUI.BeginChangeCheck ();
			if (branchDescriptorCollection.branchDescriptors.Count > 0) {
				branchDescriptorCollection.branchDescriptorIndex = GUILayout.Toolbar (branchDescriptorCollection.branchDescriptorIndex, snapshotViewOptions);
			} else {
				EditorGUILayout.HelpBox (MSG_EMPTY_SNAPSHOTS, MessageType.Warning, true);
			}
			if (EditorGUI.EndChangeCheck ()) {
				SelectSnapshot (branchDescriptorCollection.branchDescriptorIndex);
				ReflectChangesToPipeline ();
				RegeneratePreview ();
			}
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button (addSnapshotGUI)) {
				AddSnapshot ();
			}
			EditorGUI.BeginDisabledGroup (branchDescriptorCollection.branchDescriptorIndex < 0);
			if (GUILayout.Button (removeSnapshotGUI)) {
				RemoveSnapshot ();
			}
			EditorGUI.EndDisabledGroup ();
			EditorGUILayout.EndHorizontal ();
		}
		/// <summary>
		/// Adds a snapshot to the loaded collection.
		/// </summary>
		void AddSnapshot () {
			if (branchDescriptorCollection.branchDescriptors.Count < 10) {
				BranchDescriptor newBranchDescriptor = selectedBranchDescriptor.Clone ();
				branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				branchDescriptorCollection.AddBranchDescriptor (newBranchDescriptor);
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				InitSnapshotViewOptions ();
				SelectSnapshot (branchDescriptorCollection.branchDescriptors.Count - 1);
				ReflectChangesToPipeline ();
				RegeneratePreview ();
			}
		}
		/// <summary>
		/// Removes the selected snapshot in the loaded collection.
		/// </summary>
		void RemoveSnapshot () {
			if (EditorUtility.DisplayDialog (MSG_DELETE_BRANCH_DESC_TITLE, 
				MSG_DELETE_BRANCH_DESC_MESSAGE, 
				MSG_DELETE_BRANCH_DESC_OK, 
				MSG_DELETE_BRANCH_DESC_CANCEL)) {
				branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				branchDescriptorCollection.branchDescriptors.RemoveAt (branchDescriptorCollection.branchDescriptorIndex);
				SelectSnapshot (0);
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				GUIUtility.ExitGUI ();
			}
		}
		/// <summary>
		/// Selects a snapshot in the loaded collection.
		/// </summary>
		/// <param name="index">Index of the snapshot to select.</param>
		public void SelectSnapshot (int index) {
			canvasStructureView = CanvasStructureView.Snapshot;
			InitSnapshotViewOptions ();
			if (branchDescriptorCollection.branchDescriptors.Count > 0 && index >= 0 && index < branchDescriptorCollection.branchDescriptors.Count) {
				branchDescriptorCollection.branchDescriptorIndex = index;
				this.selectedBranchDescriptor = branchDescriptorCollection.branchDescriptors [index];
				this.sproutSubfactory.branchDescriptorIndex = index;
				meshPreview.previewTitle = "Snapshot " + index + " (" + currentImplementation.GetPreviewTitle (branchDescriptorCollection.descriptorImplId) + ")";
			} else {
				branchDescriptorCollection.branchDescriptorIndex = -1;
				this.selectedBranchDescriptor = null;
				this.sproutSubfactory.branchDescriptorIndex = -1;
			}
			InitLODViewOptions ();
		}
		#endregion

		#region Variants
		/// <summary>
		/// Initializes the view options for the variations present in the loaded collection.
		/// </summary>
		void InitVariationViewOptions () {
			// Build GUIContents per variation.
			variationViewOptions = new GUIContent[branchDescriptorCollection.variationDescriptors.Count];
			for (int i = 0; i < branchDescriptorCollection.variationDescriptors.Count; i++) {
				variationViewOptions [i] = new GUIContent ("Var" + i);
			}
		}
		/// <summary>
		/// Draws the list of variations on this collection.
		/// </summary>
		void DrawVariationsPanel () {
			EditorGUILayout.BeginHorizontal ();
			EditorGUI.BeginChangeCheck ();
			if (branchDescriptorCollection.variationDescriptors.Count > 0) {
				branchDescriptorCollection.variationDescriptorIndex = GUILayout.Toolbar (branchDescriptorCollection.variationDescriptorIndex, variationViewOptions);
			} else {
				EditorGUILayout.HelpBox (MSG_EMPTY_VARIATIONS, MessageType.Warning, true);
			}
			if (EditorGUI.EndChangeCheck ()) {
				SelectVariation (branchDescriptorCollection.variationDescriptorIndex);
				RegeneratePreview ();
			}
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button (addVariationGUI)) {
				AddVariation ();
			}
			EditorGUI.BeginDisabledGroup (branchDescriptorCollection.variationDescriptorIndex < 0);
			if (GUILayout.Button (removeVariationGUI)) {
				RemoveVariation ();
			}
			EditorGUI.EndDisabledGroup ();
			EditorGUILayout.EndHorizontal ();
		}
		/// <summary>
		/// Adds a variation to the loaded collection.
		/// </summary>
		void AddVariation () {
			if (branchDescriptorCollection.variationDescriptors.Count < 15) {
				VariationDescriptor newVariationDescriptor;
				if (selectedVariationDescriptor != null) {
					newVariationDescriptor = selectedVariationDescriptor.Clone ();
				} else {
					newVariationDescriptor = new VariationDescriptor ();
				}
				branchDescriptorCollection.lastVariationDescriptorIndex = branchDescriptorCollection.variationDescriptorIndex;
				onBeforeVariationDescriptorChange?.Invoke (branchDescriptorCollection);
				branchDescriptorCollection.AddVariationDescriptor (newVariationDescriptor);
				onVariationDescriptorChange?.Invoke (branchDescriptorCollection);
				InitVariationViewOptions ();
				SelectVariation (branchDescriptorCollection.variationDescriptors.Count - 1);
				RegeneratePreview ();
			}
		}
		/// <summary>
		/// Removes the selected variation in the loaded collection.
		/// </summary>
		void RemoveVariation () {
			if (EditorUtility.DisplayDialog (MSG_DELETE_VARIATION_DESC_TITLE, 
				MSG_DELETE_VARIATION_DESC_MESSAGE, 
				MSG_DELETE_VARIATION_DESC_OK, 
				MSG_DELETE_VARIATION_DESC_CANCEL)) {
				branchDescriptorCollection.lastVariationDescriptorIndex = branchDescriptorCollection.variationDescriptorIndex;
				onBeforeVariationDescriptorChange?.Invoke (branchDescriptorCollection);
				branchDescriptorCollection.variationDescriptors.RemoveAt (branchDescriptorCollection.variationDescriptorIndex);
				SelectVariation (0);
				onVariationDescriptorChange?.Invoke (branchDescriptorCollection);
				GUIUtility.ExitGUI ();
			}
		}
		/// <summary>
		/// Selects a variation in the loaded collection.
		/// </summary>
		/// <param name="index">Index of the variation to select.</param>
		public void SelectVariation (int index) {
			canvasStructureView = CanvasStructureView.Variation;
			InitVariationViewOptions ();
			if (branchDescriptorCollection.variationDescriptors.Count > 0 && index >= 0 && index < branchDescriptorCollection.variationDescriptors.Count) {
				branchDescriptorCollection.variationDescriptorIndex = index;
				this.selectedVariationDescriptor = branchDescriptorCollection.variationDescriptors [index];
				this.sproutSubfactory.variationDescriptorIndex = index;
				meshPreview.previewTitle = "Variation " + index + " (" + currentImplementation.GetPreviewTitle (branchDescriptorCollection.descriptorImplId) + ")";
			} else {
				branchDescriptorCollection.variationDescriptorIndex = -1;
				this.selectedVariationDescriptor = null;
				this.sproutSubfactory.variationDescriptorIndex = -1;
			}
			//InitLODViewOptions ();
		}
		#endregion

		#region LODs
		/// <summary>
		/// Initializes the view options for the LODs present in the loaded collection.
		/// </summary>
		void InitLODViewOptions () {
			if (selectedBranchDescriptor != null) {
				int lodCount = selectedBranchDescriptor.lodCount + 1;
				lodViewOptions = new GUIContent[lodCount];
				lodViewOptions [0] = new GUIContent ("#");
				for (int i = 1; i < lodCount; i++) {
					lodViewOptions [i] = new GUIContent ("LOD" + (i - 1));
				}
				selectedLODView = 0;
			}
		}
		#endregion

		#region Mesh Preview
		/// <summary>
		/// Get a preview mesh for a SproutMesh.
		/// </summary>
		/// <returns>Mesh for previewing.</returns>
		public Mesh GetPreviewMesh (SproutMesh sproutMesh, SproutMap.SproutMapArea sproutMapArea) {
			// TODO: optimize.
			return sproutSubfactory.treeFactory.previewTree.obj.GetComponent<MeshFilter> ().sharedMesh;
		}
		/// <summary>
		/// Shows a mesh preview according to the selected snapshot.
		/// </summary>
		public void ShowPreviewMesh (int viewMode = VIEW_COMPOSITE) {
			if (sproutSubfactory == null) return;
			SetMapView (viewMode, true);
			// Gets the shared mesh on the MeshFilter component on the sprout subfactory.
			Mesh mesh = GetPreviewMesh (null, null);
			Material material = new Material(Shader.Find ("Standard"));
			meshPreview.Clear ();
			meshPreview.CreateViewport ();
			mesh.RecalculateBounds();
			if (material != null) {
				meshPreview.AddMesh (0, mesh, material, true);
			} else {
				meshPreview.AddMesh (0, mesh, true);
			}
			selectedLODView = 0;
		}
		/// <summary>
		/// Shows a mesh set as preview.
		/// </summary>
		/// <param name="previewMesh"></param>
		public void ShowPreviewMesh (Mesh previewMesh, Material material = null) {
			if (previewMesh == null) return;
			if (material == null) {
				material = new Material(Shader.Find ("Standard"));
			}
			meshPreview.Clear ();
			meshPreview.CreateViewport ();
			previewMesh.RecalculateBounds ();
			currentPreviewMaterials = new Material[previewMesh.subMeshCount];
			for (int i = 0; i < previewMesh.subMeshCount; i++) {
				currentPreviewMaterials [i] = material;	
			}
			meshPreview.AddMesh (0, previewMesh, material, true);
		}
		/// <summary>
		/// Shows a mesh set as preview.
		/// </summary>
		/// <param name="previewMesh"></param>
		public void ShowPreviewMesh (Mesh previewMesh, Material[] materials) {
			if (previewMesh == null) return;
			meshPreview.Clear ();
			meshPreview.CreateViewport ();
			previewMesh.RecalculateBounds ();
			currentPreviewMaterials = materials;
			meshPreview.AddMesh (0, previewMesh, true);
		}
		/// <summary>
		/// Draw additional handles on the mesh preview area.
		/// </summary>
		/// <param name="r">Rect</param>
		/// <param name="camera">Camera</param>
		public void OnPreviewMeshDrawHandles (Rect r, Camera camera) {
			if (showLightControls) {
				Handles.color = Color.yellow;
				Handles.ArrowHandleCap (0,
					//Vector3.zero, 
					meshPreview.GetLightA ().transform.rotation * Vector3.back * 1.5f,
					meshPreview.GetLightA ().transform.rotation, 
					1f * MeshPreview.GetHandleSize (Vector3.zero, camera), 
					EventType.Repaint); 
			}
			if (currentStructureSettings.displayRadiusControl) {
				Handles.color = Color.yellow;
				Handles.DrawWireDisc (Vector3.zero, Vector3.up, 
					selectedBranchDescriptor.branchLevelDescriptors [0].radius);
			}
			if (debugEnabled && Event.current.type == EventType.Repaint) {
				List<PolygonArea> polygonAreas = 
					sproutSubfactory.sproutCompositeManager.GetPolygonAreas (selectedBranchDescriptor.id, selectedLODView - 1, true);
				// Draw debugging for each polygon area.
				PolygonArea _polygonArea;				
				for (int pI = 0; pI < polygonAreas.Count; pI++) {
					if (debugPolyIndex == 0 || pI == (debugPolyIndex - 1)) {
						_polygonArea = polygonAreas [pI];
						#if BROCCOLI_DEVEL
						if (debugShowTopoPoints && _polygonArea != null) {
							Handles.color = Color.white;
							float handleSize;
							float handleSizeScale = 0.045f;
							for (int i = 0; i < _polygonArea.topoPoints.Count; i++) {
								handleSize = HandleUtility.GetHandleSize (_polygonArea.topoPoints[i]) * handleSizeScale;
								Handles.DotHandleCap (-1, _polygonArea.topoPoints [i], 
									Quaternion.identity, handleSize, EventType.Repaint);
							}
						}
						#endif
						if (debugShowConvexHull && _polygonArea != null) {
							Handles.color = Color.yellow;
							int i = 0;
							for (i = 0; i < _polygonArea.lastConvexPointIndex; i++) {
								Handles.DrawLine (_polygonArea.points [i], _polygonArea.points [i + 1]);
							}
							Handles.DrawLine (_polygonArea.points [0], _polygonArea.points [_polygonArea.lastConvexPointIndex]);
						}
						if (debugShowConvexHullPoints && _polygonArea != null) {
							float handleSize;
							float handleSizeScale = 0.045f;
							Handles.color = Color.yellow;
							for (int i = 0; i <= _polygonArea.lastConvexPointIndex; i++) {
								if (debugShowConvexHullPointsOrder) {
									float step = (float)i / (_polygonArea.lastConvexPointIndex == 0?1:_polygonArea.lastConvexPointIndex + 1);
									handleSize = HandleUtility.GetHandleSize (_polygonArea.points [i]) * handleSizeScale * Mathf.Lerp (1f, 2f, step);
									Handles.color = Color.Lerp (Color.yellow, Color.red, step * 0.65f);
								} else {
									handleSize = HandleUtility.GetHandleSize (_polygonArea.points[i]) * handleSizeScale;
								}
								Handles.DotHandleCap (-1, _polygonArea.points [i], Quaternion.identity, handleSize, EventType.Repaint);
							}
							List<Vector3> anglesPos = GeometryAnalyzer.Current ().debugAnglePos;
							List<float> angles = GeometryAnalyzer.Current ().debugAngles;
							float scale = sproutSubfactory.factoryScale;
							Vector3 rPos = Vector3.zero;
							Matrix4x4 formerMatrix = Handles.matrix;
							Handles.matrix = Matrix4x4.identity;
							for (int i = 0; i < angles.Count; i++) {
								Handles.Label (anglesPos [i], i + ": " + angles [i]);
								Handles.DrawLine (rPos, anglesPos [i]);
								rPos = anglesPos [i];
							}
							Handles.matrix = formerMatrix;
						}
						if (debugShowAABB) {
							Handles.color = Color.white;
							Vector3 topLeft = _polygonArea.aabb.min;
							topLeft.y = _polygonArea.aabb.max.y;
							Handles.DrawLine (_polygonArea.aabb.min, topLeft);
							Handles.DrawLine (topLeft, _polygonArea.aabb.max);
							Vector3 bottomRight = _polygonArea.aabb.max;
							bottomRight.y = _polygonArea.aabb.min.y;
							Handles.DrawLine (_polygonArea.aabb.max, bottomRight);
							Handles.DrawLine (bottomRight, _polygonArea.aabb.min);
						}
						if (debugShowOBB) {
							Handles.color = Color.white;
							Vector3 topLeft = _polygonArea.obb.min;
							topLeft.y = _polygonArea.obb.max.y;
							Handles.DrawLine (_polygonArea.obb.min, topLeft);
							Handles.DrawLine (topLeft, _polygonArea.obb.max);
							Vector3 bottomRight = _polygonArea.obb.max;
							bottomRight.y = _polygonArea.obb.min.y;
							Handles.DrawLine (_polygonArea.obb.max, bottomRight);
							Handles.DrawLine (bottomRight, _polygonArea.obb.min);
						}
						if (debugShowTris && _polygonArea != null && _polygonArea.mesh != null) {
							int[] tris = _polygonArea.mesh.triangles;
							Vector3[] vert = _polygonArea.mesh.vertices;
							Handles.color = Color.white;
							for (int i = 0; i < tris.Length; i = i + 3) {
								Handles.DrawLine (vert [tris [i]], vert [tris [i + 1]]);
								Handles.DrawLine (vert [tris [i + 1]], vert [tris [i + 2]]);
								Handles.DrawLine (vert [tris [i + 2]], vert [tris [i]]);
							}
						}
						if (debugShowMeshNormals  && _polygonArea != null && _polygonArea.mesh != null) {
							Vector3[] _vertices = _polygonArea.mesh.vertices;
							Vector3[] _normals = _polygonArea.mesh.normals;
							Handles.color = Color.yellow;
							for (int i = 0; i < _vertices.Length; i++) {
								Handles.DrawLine (_vertices [i], _vertices [i] + _normals [i]);
							}
						}
						if (debugShowMeshTangents  && _polygonArea != null && _polygonArea.mesh != null) {
							Vector3[] _vertices = _polygonArea.mesh.vertices;
							Vector4[] _tangents = _polygonArea.mesh.tangents;
							Vector3 _tan;
							Handles.color = Color.Lerp (Color.red, Color.white, 0.6f);
							for (int i = 0; i < _vertices.Length; i++) {
								_tan = (Vector3)_tangents [i];
								Handles.DrawLine (_vertices [i], _vertices [i] + _tan);
							}
						}
						if (_polygonArea != null) {
							GeometryAnalyzer ga = GeometryAnalyzer.Current ();
							Handles.color = Color.white;
							float scale = sproutSubfactory.factoryScale;
							Handles.color = Color.white;
							for (int i = 0; i < ga.debugCombinedPoly.Count - 1; i ++) {
								Handles.DrawLine (ga.debugCombinedPoly [i] * scale, ga.debugCombinedPoly [i + 1] * scale);
							}
						}
					}
				}
			}
		}
		/// <summary>
		/// Draws GUI elements on the mesh preview area.
		/// </summary>
		/// <param name="r">Rect</param>
		/// <param name="camera">Camera</param>
		public void OnPreviewMeshDrawGUI (Rect r, Camera camera) {
			if (showLightControls) {
				DrawLightControls (r);
			}
			DrawLODViewControls (r);
			if (showProgressBar) {
				EditorGUI.ProgressBar(new Rect(0, 0, r.width, EditorGUIUtility.singleLineHeight), 
					progressBarProgress, progressBarTitle);
			}
		}
		/// <summary>
		/// Called when the mesh preview requires repaint.
		/// </summary>
		void OnMeshPreviewRequiresRepaint () {
			if (SproutFactoryEditorWindow.editorWindow != null)
				SproutFactoryEditorWindow.editorWindow.Repaint ();
		}
		/// <summary>
		/// Draws the control to rotate the lights.
		/// </summary>
		/// <param name="r"></param>
		public void DrawLightControls (Rect r) {
			r.x = 4;
			r.y = r.height - 4 - EditorGUIUtility.singleLineHeight;
			r.height = EditorGUIUtility.singleLineHeight;
			r.width = 100;
			if (GUI.Button (r, "Light: " + lightAngleDisplayStr)) {
				AddLightStep ();
			}
		}
		/// <summary>
		/// Draws the controls to switch between mesh LOD views.
		/// </summary>
		/// <param name="r"></param>
		public void DrawLODViewControls (Rect r) {
			if (!showLODOptions) return;
			r.x = (r.width / 2f) - (lodViewOptions.Length * 45f / 2f);
			r.y = r.height - 4 - EditorGUIUtility.singleLineHeight;
			r.height = EditorGUIUtility.singleLineHeight;
			r.width = lodViewOptions.Length * 45f;
			int _selectedLODView = GUI.SelectionGrid (r, selectedLODView, lodViewOptions, lodViewOptions.Length);
			if (_selectedLODView != selectedLODView) {
				selectedLODView = _selectedLODView;
				if (selectedLODView == 0) {
					ShowPreviewMesh ();
				} else {
					if (debugEnabled) {
						sproutSubfactory.simplifyHullEnabled =!debugSkipSimplifyHull;
					}
					sproutSubfactory.ProcessSnapshotPolygons (selectedBranchDescriptor);
					meshPreview.hasSecondPass = false;
					Mesh lodMesh = sproutSubfactory.sproutCompositeManager.GetMesh (selectedBranchDescriptor.id, selectedLODView - 1);
					Material[] mats = sproutSubfactory.sproutCompositeManager.GetMaterials (selectedBranchDescriptor.id, selectedLODView - 1);
					ShowPreviewMesh (lodMesh, mats);
				}
			}
		}
		public void AddLightStep () {
			if (lightAngleToAddTimeTmp <= 0) {
				SetEditorDeltaTime ();
				lightAngleToAddTimeTmp = lightAngleToAddTime;
				lightAngleEulerFrom = meshPreview.GetLightA ().transform.rotation.eulerAngles;
				lightAngleEulerTo = lightAngleEulerFrom;
				lightAngleEulerTo.y += lightAngleStepValue;
				lightAngleStep++;
				if (lightAngleStep >= 8) lightAngleStep = 0;
				switch (lightAngleStep) {
					case 0: lightAngleDisplayStr = "Front";
						break;
					case 1:  lightAngleDisplayStr = "Left 45";
						break;
					case 2:  lightAngleDisplayStr = "Left";
						break;
					case 3:  lightAngleDisplayStr = "Left -45";
						break;
					case 4:  lightAngleDisplayStr = "Back";
						break;
					case 5:  lightAngleDisplayStr = "Right -45";
						break;
					case 6:  lightAngleDisplayStr = "Right";
						break;
					case 7:  lightAngleDisplayStr = "Right 45";
						break;
				}
			}
		}
		void SetCanvasSettings (CanvasSettings canvasSettings) {
			if (currentCanvasSettings == null || currentCanvasSettings.id != canvasSettings.id) {
				// Set the current canvas settings.
				if (canvasSettings == null) currentCanvasSettings = defaultCanvasSettings;
				else currentCanvasSettings = canvasSettings;

				// Set mesh preview controls and helpers.
				meshPreview.freeViewEnabled = currentCanvasSettings.freeViewEnabled;
				if (currentCanvasSettings.showPlane) {
					meshPreview.planeColor = Color.Lerp (Color.black, Color.white, 0.7f);
					// Load plane texture.
					int textureIndex = GUITextureManager.LoadCustomTexture ("broccoli_plane.png");
					if (textureIndex >= 0) {
						meshPreview.planeTexture = GUITextureManager.GetCustomTexture (textureIndex);
					}
				}
				meshPreview.ShowPlaneMesh (currentCanvasSettings.showPlane, 
					currentCanvasSettings.planeSize, Vector3.zero);

				if (currentCanvasSettings.resetZoom) {
					SetEditorDeltaTime ();
					zoomTransitionEnabled = true;
					cameraTransitionZoom = currentCanvasSettings.defaultZoomFactor;
					cameraTransitionZoomTmp = meshPreview.GetZoom ();
				} else {
					zoomTransitionEnabled = false;
				}

				// Transition to the new view settings.
				if (currentCanvasSettings.resetView) {
					SetEditorDeltaTime ();
					viewTransitionEnabled = true;
					cameraTransitionDirection = currentCanvasSettings.viewDirection;
					cameraTransitionOffset = currentCanvasSettings.viewOffset;
					cameraTransitionTargetRotation = currentCanvasSettings.viewTargetRotation;
					cameraTransitionTimeTmp = cameraTransitionTime;
					cameraTransitionDirectionTmp = meshPreview.GetDirection ();
					cameraTransitionOffsetTmp = meshPreview.GetOffset ();
					cameraTransitionTargetRotationTmp = meshPreview.GetTargetRotation ();
				} else {
					viewTransitionEnabled = false;
				}
			}
		}
		void SetStructureSettings (StructureSettings structureSettings) {
			if (currentStructureSettings == null || currentStructureSettings.id != structureSettings.id) {
				// Set the current structure settings.
				if (structureSettings == null) currentStructureSettings = defaultStructureSettings;
				else currentStructureSettings = structureSettings;
				levelOptions = new string[] {"Main " + currentStructureSettings.branchEntityName, 
					"One Level", "Two Levels", "Three Levels"};
				structureViewOptions [0] = 
					new GUIContent (currentStructureSettings.branchEntitiesName, "Settings for branches.");
			}
		}
		#endregion

		#region Persistence
		/// <summary>
		/// Creates a new branch descriptor collection.
		/// </summary>
		private void OnCreateNewBranchDescriptorCollectionSO () {}
		/// <summary>
		/// Loads a BanchDescriptorCollection from a file.
		/// </summary>
		/// <param name="loadedBranchDescriptorCollection">Branch collection loaded.</param>
		/// <param name="pathToFile">Path to file.</param>
		private void OnLoadBranchDescriptorCollectionSO (BranchDescriptorCollectionSO loadedBranchDescriptorCollectionSO, string pathToFile) {
			onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
			// TEMP TODO: remove.
			loadedBranchDescriptorCollectionSO.branchDescriptorCollection.descriptorImplId = 0;

			LoadBranchDescriptorCollection (loadedBranchDescriptorCollectionSO.branchDescriptorCollection.Clone (), sproutSubfactory);
			onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
			/*
			rockFactory.localPipeline = loadedBranchDescriptorCollection;
			rockFactory.localPipelineFilepath = pathToFile;
			LoadFactory (rockFactory, true);
			RequestUpdateRockPreview ();
			_isDirty = true;
			*/
		}
		/// <summary>
		/// Gets the branch descriptor collection to save when the user requests it.
		/// </summary>
		/// <returns>Object to save.</returns>
		private BranchDescriptorCollectionSO OnGetBranchDescriptorCollectionSOToSave () {
			BranchDescriptorCollectionSO toSave = ScriptableObject.CreateInstance<BranchDescriptorCollectionSO> ();
			toSave.branchDescriptorCollection = branchDescriptorCollection;
			return toSave;
		}
		/// <summary>
		/// Gets the path to file when the user requests it.
		/// </summary>
		/// <returns>The path to file or empty string if not has been set.</returns>
		private string OnGetBranchDescriptorCollectionSOToSaveFilePath () {
			return "";
		}
		/// <summary>
		/// Receives the object just saved.
		/// </summary>
		/// <param name="branchDescriptorCollectionSO">Saved object.</param>
		/// <param name="pathToFile">Path to file.</param>
		private void OnSaveBranchDescriptorCollectionSO (BranchDescriptorCollectionSO branchDescriptorCollectionSO, string pathToFile) {
			//LoadBranchDescriptorCollection (branchDescriptorCollectionSO.branchDescriptorCollection, sproutSubfactory);
		}
		#endregion

		#region Sprout Map List
		/// <summary>
		/// Inits the sprout map list.
		/// </summary>
		private void InitSproutMapLists () {
			// Sprout A Map List.
			sproutAMapList = new ReorderableList (branchDescriptorCollection.sproutAMapAreas, 
					typeof (SproutMap.SproutMapArea), false, true, true, true);
			sproutAMapList.draggable = false;
			sproutAMapList.drawHeaderCallback += DrawSproutMapListHeader;
			sproutAMapList.drawElementCallback += DrawSproutAMapListItemElement;
			sproutAMapList.onAddCallback += AddSproutAMapListItem;
			sproutAMapList.onRemoveCallback += RemoveSproutAMapListItem;
			// Sprout B Map List.
			sproutBMapList = new ReorderableList (branchDescriptorCollection.sproutBMapAreas, 
					typeof (SproutMap.SproutMapArea), false, true, true, true);
			sproutBMapList.draggable = false;
			sproutBMapList.drawHeaderCallback += DrawSproutMapListHeader;
			sproutBMapList.drawElementCallback += DrawSproutBMapListItemElement;
			sproutBMapList.onAddCallback += AddSproutBMapListItem;
			sproutBMapList.onRemoveCallback += RemoveSproutBMapListItem;
		}
		/// <summary>
		/// Draws the sprout map list header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawSproutMapListHeader (Rect rect) {
			GUI.Label(rect, "Sprout Maps", BroccoEditorGUI.labelBoldCentered);
		}
		/// <summary>
		/// Draws each sprout map list item element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawSproutAMapListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			SproutMap.SproutMapArea sproutMapArea = branchDescriptorCollection.sproutAMapAreas [index];
			if (sproutMapArea != null) {
				GUI.Label (new Rect (rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight + 5), 
					"Textures for Leaf Type " + (index + 1));
				if (isActive) {
					if (selectedSproutMap != branchDescriptorCollection.sproutAMapAreas [index]) {
						selectedSproutMap = branchDescriptorCollection.sproutAMapAreas [index];
						selectedSproutMapGroup = 0;
						selectedSproutMapIndex = index;
						selectedSproutMapDescriptor = branchDescriptorCollection.sproutAMapDescriptors [index];
					}
					CopyToProxySproutMap ();
					EditorGUILayout.BeginVertical (GUILayout.Width (200));
					EditorGUI.BeginChangeCheck ();
					proxySproutMap.texture = (Texture2D) EditorGUILayout.ObjectField ("Main Texture", proxySproutMap.texture, typeof (Texture2D), false);
					proxySproutMap.normalMap = (Texture2D) EditorGUILayout.ObjectField ("Normal Texture", proxySproutMap.normalMap, typeof (Texture2D), false);
					//proxySproutMap.extraMap = (Texture2D) EditorGUILayout.ObjectField ("Extra Texture", proxySproutMap.extraMap, typeof (Texture2D), false);
					EditorGUILayout.EndVertical ();
					proxySproutMapDescriptor.alphaFactor = EditorGUILayout.Slider ("Alpha Factor", proxySproutMapDescriptor.alphaFactor, 0.7f, 1f);
					if (EditorGUI.EndChangeCheck ()) {
						WaitProcessTexture (sproutSubfactory.GetSproutTextureId (0, index), proxySproutMapDescriptor.alphaFactor);
						sproutMapChanged = true;
					}
				}
			}
		}
		/// <summary>
		/// Adds the sprout map list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void AddSproutAMapListItem (ReorderableList list) {
			branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
			onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
			if (branchDescriptorCollection.sproutAMapAreas.Count < 10) {
				SproutMap.SproutMapArea sproutMapArea= new SproutMap.SproutMapArea ();
				branchDescriptorCollection.sproutAMapAreas.Add (sproutMapArea);
				BranchDescriptorCollection.SproutMapDescriptor sproutMapDescriptor = new BranchDescriptorCollection.SproutMapDescriptor ();
				branchDescriptorCollection.sproutAMapDescriptors.Add (sproutMapDescriptor);
			}
			onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
		}
		/// <summary>
		/// Removes the sprout map list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void RemoveSproutAMapListItem (ReorderableList list) {
			SproutMap.SproutMapArea sproutMap = branchDescriptorCollection.sproutAMapAreas [list.index];
			if (sproutMap != null) {
				if (EditorUtility.DisplayDialog (MSG_DELETE_SPROUT_MAP_TITLE, 
					MSG_DELETE_SPROUT_MAP_MESSAGE, 
					MSG_DELETE_SPROUT_MAP_OK, 
					MSG_DELETE_SPROUT_MAP_CANCEL)) {
					branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
					onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
					branchDescriptorCollection.sproutAMapAreas.RemoveAt (list.index);
					selectedSproutMap = null;
					selectedSproutMapDescriptor = null;
					onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				}
			}
		}
		/// <summary>
		/// Draws each sprout map list item element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawSproutBMapListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			SproutMap.SproutMapArea sproutMapArea = branchDescriptorCollection.sproutBMapAreas [index];
			if (sproutMapArea != null) {
				GUI.Label (new Rect (rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight + 5), 
					"Textures for Leaf Type " + (index + 1));
				if (isActive) {
					if (selectedSproutMap != branchDescriptorCollection.sproutBMapAreas [index]) {
						selectedSproutMap = branchDescriptorCollection.sproutBMapAreas [index];
						selectedSproutMapGroup = 1;
						selectedSproutMapIndex = 0;
						selectedSproutMapDescriptor = branchDescriptorCollection.sproutBMapDescriptors [index];
					}
					CopyToProxySproutMap ();
					EditorGUILayout.BeginVertical (GUILayout.Width (200));
					EditorGUI.BeginChangeCheck ();
					proxySproutMap.texture = (Texture2D) EditorGUILayout.ObjectField ("Main Texture", proxySproutMap.texture, typeof (Texture2D), false);
					proxySproutMap.normalMap = (Texture2D) EditorGUILayout.ObjectField ("Normal Texture", proxySproutMap.normalMap, typeof (Texture2D), false);
					//proxySproutMap.extraMap = (Texture2D) EditorGUILayout.ObjectField ("Extra Texture", proxySproutMap.extraMap, typeof (Texture2D), false);
					EditorGUILayout.EndVertical ();
					proxySproutMapDescriptor.alphaFactor = EditorGUILayout.Slider ("Alpha Factor", proxySproutMapDescriptor.alphaFactor, 0f, 1f);
					if (EditorGUI.EndChangeCheck ()) {
						WaitProcessTexture (sproutSubfactory.GetSproutTextureId (1, index), proxySproutMapDescriptor.alphaFactor);
						sproutMapChanged = true;
					}
				}
			}
		}
		/// <summary>
		/// Adds the sprout map list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void AddSproutBMapListItem (ReorderableList list) {
			branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
			onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
			if (branchDescriptorCollection.sproutBMapAreas.Count < 10) {
				SproutMap.SproutMapArea sproutMapArea= new SproutMap.SproutMapArea ();
				branchDescriptorCollection.sproutBMapAreas.Add (sproutMapArea);
				BranchDescriptorCollection.SproutMapDescriptor sproutMapDescriptor = new BranchDescriptorCollection.SproutMapDescriptor ();
				branchDescriptorCollection.sproutBMapDescriptors.Add (sproutMapDescriptor);
			}
			onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
		}
		/// <summary>
		/// Removes the sprout map list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void RemoveSproutBMapListItem (ReorderableList list) {
			SproutMap.SproutMapArea sproutMap = branchDescriptorCollection.sproutBMapAreas [list.index];
			if (sproutMap != null) {
				if (EditorUtility.DisplayDialog (MSG_DELETE_SPROUT_MAP_TITLE, 
					MSG_DELETE_SPROUT_MAP_MESSAGE, 
					MSG_DELETE_SPROUT_MAP_OK, 
					MSG_DELETE_SPROUT_MAP_CANCEL)) {
					branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
					onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
					branchDescriptorCollection.sproutBMapAreas.RemoveAt (list.index);
					selectedSproutMap = null;
					onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				}
			}
		}
		#endregion
		
		#region Export Process
		/// <summary>
		/// Exports the collection to a BranchDescriptorSO.
		/// </summary>
		/// <param name="exportAtlas"></param>
		void ExportDescriptor (bool exportAtlas) {
			// Get file path to save to.
			string savePath = editorPersistence.GetSavePath ();

			if (!string.IsNullOrEmpty (savePath)) {
				// Exporting the branch descriptor.
				bool isValid = false;
				string basePath = "Assets" + branchDescriptorCollection.exportPath;
				string subfolder = branchDescriptorCollection.exportPrefix + FileUtils.GetFileTakeSuffix (branchDescriptorCollection.exportTake);
				string albedoPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Albedo, true);
				string normalPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Normals, true);
				string extrasPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Extras, true);
				string subsurfacePath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Subsurface, true);
				string compositePath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Composite, true);
				bool done = sproutSubfactory.GenerateAtlasTextureFromPolygons (
					branchDescriptorCollection,
					GetTextureSize (branchDescriptorCollection.exportTextureSize),
					GetTextureSize (branchDescriptorCollection.exportTextureSize),
					branchDescriptorCollection.exportAtlasPadding,
					albedoPath, normalPath, extrasPath, subsurfacePath, compositePath);
				if (done) {
					BranchDescriptorCollectionSO bdSO = ScriptableObject.CreateInstance<BranchDescriptorCollectionSO> ();
					bdSO.branchDescriptorCollection = branchDescriptorCollection;
					editorPersistence.SaveElementToFile (bdSO, savePath);
					onShowNotification?.Invoke ("Branch Descriptor Saved at: " + savePath);
					onShowNotification?.Invoke ("Atlas textures Saved at: " + basePath);
					AssetDatabase.LoadAssetAtPath<BranchDescriptorCollectionSO> (savePath);
				}
			}

			GUIUtility.ExitGUI ();
		}
		void ExportPrefab () {
			AssetManager assetManager = new AssetManager ();
			sproutSubfactory.ProcessSnapshotPolygons (selectedBranchDescriptor);
			Mesh lodMesh = sproutSubfactory.sproutCompositeManager.GetMesh (selectedBranchDescriptor.id, 0);
			Material[] mats = sproutSubfactory.sproutCompositeManager.GetMaterials (selectedBranchDescriptor.id, 0);
		}
		void ExportTextures () {
			// Generate Snapshot Texture
			if (branchDescriptorCollection.exportMode == BranchDescriptorCollection.ExportMode.SelectedSnapshot) {
				ExportTexturesSingleSnapshot ();
			} else {
				// Generate atlas texture.
				ExportTexturesAtlas ();
			}
		}
		void ExportTexturesSingleSnapshot () {
			int index = branchDescriptorCollection.branchDescriptorIndex;
			string basePath = "Assets" + branchDescriptorCollection.exportPath;
			bool isValid = false;
			string subfolder = branchDescriptorCollection.exportPrefix + FileUtils.GetFileTakeSuffix (branchDescriptorCollection.exportTake);
			bool subfolderCreated = FileUtils.CreateSubfolder ("Assets" + branchDescriptorCollection.exportPath, subfolder);
			if (!subfolderCreated) return;
			string albedoPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Albedo, false);
			string normalPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Normals, false);
			string extrasPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Extras, false);
			string subsurfacePath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Subsurface, false);
			string compositePath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Composite, false);
			bool done = sproutSubfactory.GenerateSnapshopTextures (
				branchDescriptorCollection.branchDescriptorIndex,
				branchDescriptorCollection,
				GetTextureSize (branchDescriptorCollection.exportTextureSize),
				GetTextureSize (branchDescriptorCollection.exportTextureSize),
				albedoPath, normalPath, extrasPath, subsurfacePath, compositePath);
			if (done) {
				onShowNotification?.Invoke ("Textures for Snapshot S" + index + " saved at: \n" + basePath);
			}
		}
		void ExportTexturesAtlas () {
			bool isValid = false;
			string basePath = "Assets" + branchDescriptorCollection.exportPath;
			
			string subfolder = branchDescriptorCollection.exportPrefix + FileUtils.GetFileTakeSuffix (branchDescriptorCollection.exportTake);
			bool subfolderCreated = FileUtils.CreateSubfolder ("Assets" + branchDescriptorCollection.exportPath, subfolder);
			if (!subfolderCreated) return;
			string albedoPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Albedo, true);
			string normalPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Normals, true);
			string extrasPath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Extras, true);
			string subsurfacePath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Subsurface, true);
			string compositePath = GetTextureFileName (subfolder, branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Composite, true);
			bool done = sproutSubfactory.GenerateAtlasTexture (
				branchDescriptorCollection,
				GetTextureSize (branchDescriptorCollection.exportTextureSize),
				GetTextureSize (branchDescriptorCollection.exportTextureSize),
				branchDescriptorCollection.exportAtlasPadding,
				albedoPath, normalPath, extrasPath, subsurfacePath, compositePath);
			if (done) {
				onShowNotification?.Invoke ("Atlas textures saved at: " + basePath);
			}
		}
		int GetTextureSize (BranchDescriptorCollection.TextureSize textureSize) {
			if (textureSize == BranchDescriptorCollection.TextureSize._2048px) {
				return 2048;
			} else if (textureSize == BranchDescriptorCollection.TextureSize._1024px) {
				return 1024;
			} else if (textureSize == BranchDescriptorCollection.TextureSize._512px) {
				return 512;
			} else if (textureSize == BranchDescriptorCollection.TextureSize._256px) {
				return 256;
			} else {
				return 128;
			}
		}
		string GetTextureFileName (string subfolder, int take, out bool isValid, SproutSubfactory.MaterialMode materialMode, bool isAtlas) {
			isValid = true;
			string path = "";
			string takeString = FileUtils.GetFileTakeSuffix (take);
			string modeString;
			if (materialMode == SproutSubfactory.MaterialMode.Albedo) {
				modeString = "Albedo";
			} else if (materialMode == SproutSubfactory.MaterialMode.Normals) {
				modeString = "Normals";
			} else if (materialMode == SproutSubfactory.MaterialMode.Extras) {
				modeString = "Extras";
			} else if (materialMode == SproutSubfactory.MaterialMode.Subsurface) {
				modeString = "Subsurface";
			} else if (materialMode == SproutSubfactory.MaterialMode.Mask) {
				modeString = "Mask";
			} else if (materialMode == SproutSubfactory.MaterialMode.Thickness) {
				modeString = "Thickness";
			} else {
				modeString = "Composite";
			}
			path = "Assets" + branchDescriptorCollection.exportPath + "/" + subfolder + "/" + 
				branchDescriptorCollection.exportPrefix + takeString +
				(isAtlas?"_Atlas":"_Snapshot") + "_" + modeString + ".png";
			return path;
		}
		void OnReportProgress (string title, float progress) {
			if (!showProgressBar) {
				showProgressBar = true;
			}
			progressBarProgress = progress;
			progressBarTitle = title;
			//UnityEditor.EditorUtility.DisplayProgressBar (sproutSubfactory.progressTitle, title, progress);
			//UnityEditor.EditorUtility.DisplayCancelableProgressBar (sproutSubfactory.progressTitle, title, progress);
			EditorGUI.ProgressBar(new Rect (0, 0, meshPreviewRect.width, 
				EditorGUIUtility.singleLineHeight), progressBarProgress, progressBarTitle);
			meshPreview.RenderViewport (meshPreviewRect, GUIStyle.none, currentPreviewMaterials);
			EditorWindow view = EditorWindow.GetWindow<SproutFactoryEditorWindow>();
			view.Repaint ();
			InternalEditorUtility.RepaintAllViews ();
		}
		void OnFinishProgress () {
			showProgressBar = false;
			//UnityEditor.EditorUtility.ClearProgressBar ();
			//GUIUtility.ExitGUI();
		}
		#endregion

		#region Editor Updates
		double editorDeltaTime = 0f;
		double lastTimeSinceStartup = 0f;
		double secondsToUpdateTexture = 0f;
		string _textureId = "";
		float _alpha = 1.0f;
		/// <summary>
		/// Raises the editor update event.
		/// </summary>
		void OnEditorUpdate () {
			if (secondsToUpdateTexture > 0) {
				SetEditorDeltaTime();
				secondsToUpdateTexture -= (float) editorDeltaTime;
				if (secondsToUpdateTexture < 0) {
					sproutSubfactory.ProcessTexture (selectedSproutMapGroup, selectedSproutMapIndex, _alpha);
					secondsToUpdateTexture = 0;
					EditorWindow view = EditorWindow.GetWindow<SproutFactoryEditorWindow>();
					view.Repaint();
				}
			}
			if (lightAngleToAddTimeTmp >= 0f) {
				SetEditorDeltaTime();
				lightAngleToAddTimeTmp -= (float)editorDeltaTime;
				UpdateLightAngle ();
				EditorWindow view = EditorWindow.GetWindow<SproutFactoryEditorWindow>();
				view.Repaint();
			}
			if (cameraTransitionTimeTmp >= 0f) {
				SetEditorDeltaTime();
				cameraTransitionTimeTmp -= (float)editorDeltaTime;
				if (viewTransitionEnabled) {
					meshPreview.SetDirection (Vector2.Lerp (cameraTransitionDirection, cameraTransitionDirectionTmp, cameraTransitionTimeTmp/cameraTransitionTime));
					meshPreview.SetOffset (Vector3.Lerp (cameraTransitionOffset, cameraTransitionOffsetTmp, cameraTransitionTimeTmp/cameraTransitionTime));
					meshPreview.SetTargetRotation (Quaternion.Lerp (cameraTransitionTargetRotation, cameraTransitionTargetRotationTmp, cameraTransitionTimeTmp/cameraTransitionTime));
				}
				if (zoomTransitionEnabled) {
					meshPreview.SetZoom (Mathf.Lerp (cameraTransitionZoom, cameraTransitionZoomTmp, cameraTransitionTimeTmp/cameraTransitionTime));
				}
				EditorWindow view = EditorWindow.GetWindow<SproutFactoryEditorWindow>();
				view.Repaint();
			}
		}
		void SetEditorDeltaTime ()
		{
			#if UNITY_EDITOR
			if (lastTimeSinceStartup == 0f)
			{
				lastTimeSinceStartup = EditorApplication.timeSinceStartup;
			}
			editorDeltaTime = EditorApplication.timeSinceStartup - lastTimeSinceStartup;
			lastTimeSinceStartup = EditorApplication.timeSinceStartup;
			#endif
		}
		void WaitProcessTexture (string textureId, float alpha) {
			secondsToUpdateTexture = 0.5f;
			_textureId = textureId;
			_alpha = alpha;
			SetEditorDeltaTime ();
		}
		void UpdateLightAngle () {
			Vector3 angle = Vector3.Lerp (lightAngleEulerFrom, lightAngleEulerTo, Mathf.InverseLerp (lightAngleToAddTime, 0, lightAngleToAddTimeTmp));
			meshPreview.GetLightA ().transform.rotation = Quaternion.Euler (angle);
		}
		#endregion
	}
}