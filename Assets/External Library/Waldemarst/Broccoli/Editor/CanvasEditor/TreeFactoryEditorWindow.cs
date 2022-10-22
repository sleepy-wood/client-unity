using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;

using Broccoli.Base;
using Broccoli.Factory;
using Broccoli.Utils;
using Broccoli.Catalog;
using Broccoli.Pipe;
using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Serialization;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Tree factory editor window.
	/// Contains the canvas to edit pipeline element nodes and
	/// all the available tree factory commands.
	/// </summary>
	public class TreeFactoryEditorWindow : EditorWindow 
	{
		#region Vars
		/// <summary>
		/// The tree factory game object.
		/// </summary>
		public GameObject treeFactoryGameObject;
		/// <summary>
		/// The tree factory behind the the canvas.
		/// </summary>
		public TreeFactory treeFactory;
		/// <summary>
		/// The tree factory serialized object behind the the canvas.
		/// </summary>
		public SerializedObject serializedTreeFactory;
		/// <summary>
		/// The property appendable components.
		/// </summary>
		public SerializedProperty propAppendableComponents;
		/// <summary>
		/// Tree canvas instance.
		/// </summary>
		public TreeCanvas treeCanvas;
		/// <summary>
		/// The canvas cache.
		/// </summary>
		public static NodeEditorUserCache canvasCache = null;
		/// <summary>
		/// The width of the side window.
		/// </summary>
		private int sidePanelWidth = 400;
		/// <summary>
		/// Gets the window rect.
		/// </summary>
		/// <value>The total window rect.</value>
		public Rect windowRect { 
			get { 
				return new Rect (0, 0, position.width, position.height); 
			} 
		}
		/// <summary>
		/// Gets the side window rect.
		/// </summary>
		/// <value>The side window rect.</value>
		public Rect sidePanelRect { 
			get { 
				return new Rect (position.width - sidePanelWidth, 0, 
					sidePanelWidth, position.height); 
			} 
		}
		/// <summary>
		/// Gets the canvas window rect.
		/// </summary>
		/// <value>The canvas window rect.</value>
		public Rect canvasWindowRect { 
			get { 
				return new Rect (0, 0, position.width - sidePanelWidth, 
					position.height); 
			} 
		}
		/// <summary>
		/// The catalog of tree pipelines.
		/// </summary>
		public BroccoliCatalog catalog;
		public enum EditorView {
			MainOptions,
			FactoryOptions,
			Catalog
		}
		EditorView editorView = EditorView.MainOptions;
		/*
		/// <summary>
		/// True when the extension window should display the catalog.
		/// </summary>
		private bool isCatalogView = false;
		/// <summary>
		/// True when the extension window should display the options related to the factory.
		/// </summary>
		private bool isFactoryOptionsView = false;
		*/
		/// <summary>
		/// True when the editor is on play mode.
		/// </summary>
		private bool isPlayModeView = false;
		/// <summary>
		/// The size of the catalog items.
		/// </summary>
		public static int catalogItemSize = 100;
		/// <summary>
		/// The sprout groups reorderable list.
		/// </summary>
		ReorderableList sproutGroupList;
		/// <summary>
		/// The branch descriptor reorderable list.
		/// </summary>
		ReorderableList branchDescriptorList;
		/// <summary>
		/// The appendable scripts reorderable list.
		/// </summary>
		BReorderableList appendableComponentList;
		/// <summary>
		/// Preview options GUIContent array.
		/// </summary>
		private static GUIContent[] previewOptions = new GUIContent[2];
		/// <summary>
		/// The normalized zoom.
		/// </summary>
		private float normalizedZoom = 0f;
		/// <summary>
		/// The prefab texture options GUIContent array.
		/// </summary>
		private static GUIContent[] prefabTextureOptions = new GUIContent[2];
		/// <summary>
		/// Saves the vertical scroll position for the side panel.
		/// </summary>
		private Vector2 sidePanelScroll;
		/// <summary>
		/// Saves the vertical scroll position for the catalog.
		/// </summary>
		private Vector2 catalogScroll;
		/// <summary>
		/// Displays the state of the loaded pipeline.
		/// </summary>
		private string pipelineLegend;
		#endregion

		#region Messages
		static string MSG_NOT_LOADED = "To edit a pipeline: please select a Broccoli Tree " +
			"Factory GameObject then press 'Open Broccoli Tree Editor' on the script inspector.";
		static string MSG_PLAY_MODE = "Tree factory node editor is not available on play mode.";
		static string MSG_LOAD_CATALOG_ITEM_TITLE = "Load catalog item";
		static string MSG_LOAD_CATALOG_ITEM_MESSAGE = "Do you really want to load this item? " +
			"You will lose any change not saved in the current pipeline.";
		static string MSG_LOAD_CATALOG_ITEM_OK = "Load Pipeline";
		static string MSG_LOAD_CATALOG_ITEM_CANCEL = "Cancel";
		static string MSG_DELETE_SPROUT_GROUP_TITLE = "Remove Sprout Group";
		static string MSG_DELETE_SPROUT_GROUP_MESSAGE = "Do you really want to remove this sprout group? " +
			"All meshes and maps assigned to it will be left unassigned.";
		static string MSG_DELETE_SPROUT_GROUP_OK = "Remove Sprout Group";
		static string MSG_DELETE_SPROUT_GROUP_CANCEL = "Cancel";
		static string MSG_NEW_PIPELINE_TITLE = "New Pipeline";
		static string MSG_NEW_PIPELINE_MESSAGE = "By creating a new pipeline you will lose any changes " +
			"not saved on the current one. Do you want to continue creating a new pipeline?";
		static string MSG_NEW_PIPELINE_OK = "Yes, create new pipeline";
		static string MSG_NEW_PIPELINE_CANCEL = "No";
		#endregion

		#region Singleton
		/// <summary>
		/// The editor window singleton.
		/// </summary>
		private static TreeFactoryEditorWindow _factoryWindow;
		/// <summary>
		/// Gets the editor window.
		/// </summary>
		/// <value>The editor.</value>
		public static TreeFactoryEditorWindow editorWindow { get { return _factoryWindow; } }
		/// <summary>
		/// Gets the tree editor window.
		/// </summary>
		static void GetWindow () {
			_factoryWindow = GetWindow<TreeFactoryEditorWindow> ();
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Static initializer for the <see cref="Broccoli.TreeNodeEditor.TreeFactoryEditorWindow"/> class.
		/// </summary>
		static TreeFactoryEditorWindow () {
			previewOptions [0] = 
				new GUIContent ("Textured", "Display textured meshes.");
			previewOptions [1] = 
				new GUIContent ("Colored", "Display colored meshes without textures.");
			prefabTextureOptions [0] = 
				new GUIContent ("Original", "Keep the provided texture or atlases on the materials.");
			prefabTextureOptions [1] = 
				new GUIContent ("New Atlas", "Create separated atlases for sprouts and branches if necessary.");
		}
		#endregion

		#region Open / Close
		/// <summary>
		/// Opens the tree factory window.
		/// </summary>
		[MenuItem("Window/Broccoli/Tree Factory Editor")]
		static void OpenTreeFactoryWindow ()
		{
			TreeFactory treeFactory = null;
			if (Selection.activeGameObject != null) {
				treeFactory = Selection.activeGameObject.GetComponent<TreeFactory> ();
			}
			OpenTreeFactoryWindow (treeFactory);
		}
		/// <summary>
		/// Checks if the menu item for opening the Tree Editor should be enabled.
		/// </summary>
		/// <returns><c>true</c>, if tree factory window is closed, <c>false</c> otherwise.</returns>
		[MenuItem("Tools/Broccoli Tree Editor", true)]
		static bool ValidateOpenTreeFactoryWindow ()
		{
			return !IsOpen ();
		}
		/// <summary>
		/// Opens the Node Editor window loading the pipeline contained on the TreeFactory object.
		/// </summary>
		/// <returns>The node editor.</returns>
		/// <param name="treeFactory">Tree factory.</param>
		public static TreeFactoryEditorWindow OpenTreeFactoryWindow (TreeFactory treeFactory = null) 
		{
			GUITextureManager.Init ();
			GetWindow ();

			if (EditorApplication.isPlayingOrWillChangePlaymode && !GlobalSettings.useTreeEditorOnPlayMode) {
				_factoryWindow.isPlayModeView = true;
			} else {
				_factoryWindow.isPlayModeView = false;
			}

			//if (treeFactory != null && treeFactory != _factoryWindow.treeFactory) {
			if (treeFactory != null) {
				SetupCanvas (treeFactory, _factoryWindow);
			}

			Texture iconTexture = 
				ResourceManager.LoadTexture (EditorGUIUtility.isProSkin? "Textures/Icon_Dark.png" : "Textures/Icon_Light.png");
			_factoryWindow.titleContent = new GUIContent ("Tree Editor", iconTexture);

			return _factoryWindow;
		}
		/// <summary>
		/// Setups the canvas.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="factoryWindow">Factory window.</param>
		public static void SetupCanvas (TreeFactory treeFactory, TreeFactoryEditorWindow factoryWindow) {
			// Setup Canvas
			if (factoryWindow.treeCanvas == null) {
				factoryWindow.treeCanvas = TreeCanvas.CreateInstance<TreeCanvas> ();
			} else {
				factoryWindow.treeCanvas.nodes.Clear ();
				factoryWindow.treeCanvas.groups.Clear ();
			}
			factoryWindow.treeCanvas.treeFactory = treeFactory;

			// Setup Cache
			canvasCache = new NodeEditorUserCache (factoryWindow.treeCanvas);
			canvasCache.SetupCacheEvents ();

			factoryWindow.treeFactory = treeFactory;
			factoryWindow.treeFactoryGameObject = treeFactory.gameObject;
			treeFactory.SetInstanceAsActive ();
			factoryWindow.serializedTreeFactory = new SerializedObject (treeFactory);
			factoryWindow.propAppendableComponents = 
				factoryWindow.serializedTreeFactory.FindProperty ("treeFactoryPreferences.appendableComponents");
			factoryWindow.treeCanvas.pipeline = factoryWindow.treeFactory.localPipeline;
			factoryWindow.treeCanvas.pipeline.Validate ();

			factoryWindow.minSize = new Vector2 (400, 200);
			NodeEditor.ReInit (false);

			NodeEditor.BeginEditingCanvas (factoryWindow.treeCanvas);
			factoryWindow.treeCanvas.LoadPipeline ();
			NodeEditor.EndEditingCanvas ();

			if (factoryWindow.treeCanvas.pipeline != null) {
				factoryWindow.InitSproutGroupList ();
				factoryWindow.InitAppendableComponentList ();
			}

			treeFactory.materialManager.SetBranchShader (treeFactory.treeFactoryPreferences.preferredShader, treeFactory.treeFactoryPreferences.customBranchShader);
			treeFactory.materialManager.SetLeavesShader (treeFactory.treeFactoryPreferences.preferredShader, treeFactory.treeFactoryPreferences.customSproutShader);
			treeFactory.materialManager.SetBillboardShader (treeFactory.treeFactoryPreferences.preferredShader);

			factoryWindow.SetEditorView ((TreeFactoryEditorWindow.EditorView)treeFactory.treeFactoryPreferences.editorView);
		}
		/// <summary>
		/// Reinitializes the editor canvas.
		/// </summary>
		private void NormalReInitCanvas()
		{
			NodeEditor.ReInit(false);
		}
		/// <summary>
		/// Unloads the tree factory instance associated to this window.
		/// </summary>
		void UnloadFactory () {
			treeFactory = null;
			// Clear Cache
			if (canvasCache != null) {
				canvasCache.ClearCacheEvents ();
			}
			canvasCache = null;
		}
		/// <summary>
		/// Determines if the editor is open.
		/// </summary>
		/// <returns><c>true</c> if is open; otherwise, <c>false</c>.</returns>
		public static bool IsOpen () {
			if (editorWindow == null)
				return false;
			return true;
		}
		/// <summary>
		/// Sets the view to the editor and persists it to the factory preferences.
		/// </summary>
		/// <param name="editorViewToSet">Editor view mode.</param>
		public void SetEditorView (EditorView editorViewToSet) {
			editorView = editorViewToSet;
			treeFactory.treeFactoryPreferences.editorView = (int)editorView;
		}
		#endregion

		#region Events
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		void OnEnable()
		{
			_factoryWindow = this;
			NodeEditor.checkInit(false);

			NodeEditor.ClientRepaints -= Repaint;
			NodeEditor.ClientRepaints += Repaint;

			EditorLoadingControl.beforeEnteringPlayMode -= OnBeforeEnteringPlayMode;
			EditorLoadingControl.beforeEnteringPlayMode += OnBeforeEnteringPlayMode;

			EditorLoadingControl.justLeftPlayMode -= OnJustLeftPlayMode;
			EditorLoadingControl.justLeftPlayMode += OnJustLeftPlayMode;
			// Here, both justLeftPlayMode and justOpenedNewScene have to act because of timing
			EditorLoadingControl.justOpenedNewScene -= OnJustOpenedNewScene;
			EditorLoadingControl.justOpenedNewScene += OnJustOpenedNewScene;
		}
		/// <summary>
		/// Raises the before entering play mode event.
		/// </summary>
		void OnBeforeEnteringPlayMode () {
			if (!GlobalSettings.useTreeEditorOnPlayMode) {
				isPlayModeView = true;
				//GUITextureManager.Clear ();
			}
		}
		/// <summary>
		/// Raises the just left play mode event.
		/// </summary>
		void OnJustLeftPlayMode () {
			isPlayModeView = false;
			if (treeFactory == null && treeFactoryGameObject != null) {
				treeFactory = treeFactoryGameObject.GetComponent<TreeFactory> ();
				SetupCanvas (treeFactory, this);
			}
			NodeEditor.ReInit(false);
			GUITextureManager.Init (true);
		}
		/// <summary>
		/// Raises the just opened new scene event.
		/// </summary>
		void OnJustOpenedNewScene () {}
		/// <summary>
		/// Raises the hierarchy change event.
		/// </summary>
		void OnHierarchyChange () {}
		/// <summary>
		/// Callback for the load scene canvas event.
		/// </summary>
		/// <param name="canvas">Canvas.</param>
		public void LoadSceneCanvasCallback (object canvas) 
		{
			canvasCache.LoadSceneNodeCanvas ((string)canvas);
		}
		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		private void OnDestroy () {
			NodeEditor.ClientRepaints -= Repaint;

			EditorLoadingControl.beforeEnteringPlayMode -= OnBeforeEnteringPlayMode;
			EditorLoadingControl.justLeftPlayMode -= OnJustLeftPlayMode;
			EditorLoadingControl.justOpenedNewScene -= OnJustOpenedNewScene;

			GUITextureManager.Clear ();

			UnloadFactory ();
		}
		#endregion

		#region GUI Events and Editor Methods
		/// <summary>
		/// Raises the GUI main draw event.
		/// </summary>
		private void OnGUI () {
			if (EventType.ValidateCommand == Event.current.type &&
				"UndoRedoPerformed" == Event.current.commandName) {
				if (treeFactory.lastUndoProcessed !=
				    treeFactory.localPipeline.undoControl.undoCount) {
					treeFactory.RequestPipelineUpdate ();
				}
			}

			// Editor view on canvas mode.
			if (editorView == EditorView.Catalog) {
				GUILayout.BeginArea (windowRect);
				DrawCatalogPanel ();
				GUILayout.EndArea ();
				return;
			}

			// Initiation
			NodeEditor.checkInit(true);

			if (isPlayModeView) {
				DrawPlayModeView ();
				return;
			}

			// Canvas is not initialized.
			if (NodeEditor.InitiationError || canvasCache == null)
			{
				DrawNotLoadedView ();
				if (Selection.activeGameObject != null &&
				    Selection.activeGameObject.GetComponent<TreeFactory> () != null) {
					OpenTreeFactoryWindow (Selection.activeGameObject.GetComponent<TreeFactory> ());
				} else {
					return;
				}
			}

			canvasCache.AssureCanvas ();
			// Specify the Canvas rect in the EditorState
			canvasCache.editorState.canvasRect = canvasWindowRect;
			// If you want to use GetRect:
			//			Rect canvasRect = GUILayoutUtility.GetRect (600, 600);
			//			if (Event.current.type != EventType.Layout)
			//				mainEditorState.canvasRect = canvasRect;

			
			NodeEditorGUI.StartNodeGUI ("TreeNodeEditorWindow", true);
			// Perform drawing with error-handling
			try
			{
				NodeEditor.DrawCanvas (canvasCache.nodeCanvas, canvasCache.editorState);
			}
			catch (UnityException e)
			{ // on exceptions in drawing flush the canvas to avoid locking the ui.
				canvasCache.NewNodeCanvas ();
				NodeEditor.ReInit (true);
				Debug.LogError ("Unloaded Canvas due to an exception during the drawing phase!");
				Debug.LogException (e);
			}
			NodeEditorGUI.EndNodeGUI();

			// Draw Side Window
			sidePanelWidth = Math.Min (600, Math.Max(200, (int)(position.width / 5)));
			//NodeEditorGUI.BeginUsingSidePanelSkin ();

			
			GUILayout.BeginArea (sidePanelRect);
			EditorGUILayout.BeginHorizontal ();
			sidePanelScroll = EditorGUILayout.BeginScrollView (sidePanelScroll, GUIStyle.none, TreeCanvasGUI.verticalScrollStyle);
			if (editorView == EditorView.FactoryOptions) {
				DrawFactoryOptionsPanel ();
			} else {
				DrawSidePanel ();
			}
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();
			GUILayout.EndArea ();



			//NodeEditorGUI.EndUsingSkin ();

			//NodeEditorGUI.EndNodeGUI();

			if (treeCanvas.isDirty) {
				treeCanvas.isDirty = false; // some set dirty, some undo
				EditorUtility.SetDirty (treeFactory);
				if (!Application.isPlaying) {
					EditorSceneManager.MarkAllScenesDirty ();
				}
				AssetDatabase.SaveAssets ();
				AssetDatabase.Refresh ();
			}
		}
		/// <summary>
		/// Clears a loaded pipeline.
		/// </summary>
		void ClearPipeline () {
			if (treeFactory.localPipeline != null) {
				List<PipelineElement> pipelineElements = treeFactory.localPipeline.GetElements ();
				for (int i = 0; i < pipelineElements.Count; i++) {
					Undo.ClearUndo (pipelineElements[i]);
				}
				Undo.ClearUndo (treeFactory.localPipeline);
			}
			for (int i = 0; i < treeCanvas.nodes.Count; i++) {
				Undo.ClearUndo (treeCanvas.nodes[i]);
			}
			treeCanvas.ClearCanvas ();
			treeFactory.UnloadAndClearPipeline ();
		}
		#endregion

		#region Sprout Group List
		/// <summary>
		/// Inits the sprout group list.
		/// </summary>
		private void InitSproutGroupList () {
			sproutGroupList = 
				new ReorderableList (_factoryWindow.treeCanvas.pipeline.sproutGroups.GetSproutGroups (), 
					typeof (SproutGroups.SproutGroup), false, true, true, true);
			sproutGroupList.draggable = false;
			sproutGroupList.drawHeaderCallback += DrawSproutGroupListHeader;
			sproutGroupList.drawElementCallback += DrawSproutGroupListItemElement;
			sproutGroupList.onAddCallback += AddSproutGroupListItem;
			sproutGroupList.onRemoveCallback += RemoveSproutGroupListItem;
		}
		/// <summary>
		/// Draws the sprout group list header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawSproutGroupListHeader (Rect rect) {
			GUI.Label(rect, "Sprout Groups", BroccoEditorGUI.labelBoldCentered);
		}
		/// <summary>
		/// Draws each sprout group list item element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawSproutGroupListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			SproutGroups.SproutGroup sproutGroup = 
				_factoryWindow.treeCanvas.pipeline.sproutGroups.GetSproutGroupAtIndex (index);
			if (sproutGroup != null) {
				rect.y += 2;
				EditorGUI.DrawRect (new Rect (rect.x, rect.y, EditorGUIUtility.singleLineHeight, 
					EditorGUIUtility.singleLineHeight), sproutGroup.GetColor ());
				rect.x += 22;
				rect.y -= 2;
				GUI.Label (new Rect (rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight + 5), 
					"Sprout Group " + sproutGroup.id);
			}
		}
		/// <summary>
		/// Adds the sprout group list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void AddSproutGroupListItem (ReorderableList list) {
			if (_factoryWindow.treeCanvas.pipeline.sproutGroups.CanCreateSproutGroup ()) {
				Undo.RecordObject (treeFactory.localPipeline, "Sprout Group Added");
				_factoryWindow.treeCanvas.pipeline.sproutGroups.CreateSproutGroup ();
			}
		}
		/// <summary>
		/// Removes the sprout group list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void RemoveSproutGroupListItem (ReorderableList list) {
			SproutGroups.SproutGroup sproutGroup = _factoryWindow.treeCanvas.pipeline.sproutGroups.GetSproutGroupAtIndex (list.index);
			if (sproutGroup != null) {
				bool hasUsage = _factoryWindow.treeCanvas.pipeline.HasSproutGroupUsage (sproutGroup.id);
				if ((hasUsage && EditorUtility.DisplayDialog (MSG_DELETE_SPROUT_GROUP_TITLE, 
					MSG_DELETE_SPROUT_GROUP_MESSAGE, 
					MSG_DELETE_SPROUT_GROUP_OK, 
					MSG_DELETE_SPROUT_GROUP_CANCEL)) || !hasUsage) {
					Undo.SetCurrentGroupName( "Zero out selected gameObjects" );
					int group = Undo.GetCurrentGroup();
					Undo.RecordObject (treeFactory.localPipeline, "Sprout Group Removed");
					_factoryWindow.treeCanvas.pipeline.DeleteSproutGroup (sproutGroup.id);
					Undo.CollapseUndoOperations( group );
				}
			}
		}
		#endregion

		#region Appendable Scripts
		/// <summary>
		/// Inits the appendable component list.
		/// </summary>
		private void InitAppendableComponentList () {
			appendableComponentList = 
				new BReorderableList (serializedTreeFactory, propAppendableComponents, false, true, true, true);
			appendableComponentList.drawHeaderCallback += DrawAppendableComponentListHeader;
			appendableComponentList.drawElementCallback += DrawAppendableComponentListItemElement;
			appendableComponentList.onAddCallback += AddAppendableComponentListItem;
			appendableComponentList.onRemoveCallback += RemoveAppendableComponentListItem;
		}
		/// <summary>
		/// Draws the appendable component list header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawAppendableComponentListHeader (Rect rect) {
			GUI.Label(rect, "Appendable Scripts", BroccoEditorGUI.labelBoldCentered);
		}
		/// <summary>
		/// Draws the appendable component list item element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawAppendableComponentListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			NodeEditorGUI.BeginUsingDefaultSkin ();
			rect.x += 10;
			rect.y += 3;
			rect.width -= 10;
			rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField (rect, propAppendableComponents.GetArrayElementAtIndex (index), GUIContent.none);
			NodeEditorGUI.EndUsingSkin ();
		}
		/// <summary>
		/// Adds an appendable component list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void AddAppendableComponentListItem (BReorderableList list) {
			propAppendableComponents.InsertArrayElementAtIndex (list.count);
		}
		/// <summary>
		/// Removes an appendable component list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void RemoveAppendableComponentListItem (BReorderableList list) {
			propAppendableComponents.DeleteArrayElementAtIndex (list.index);
		}
		#endregion

		#region Pipeline Operations
		/// <summary>
		/// Loads a new pipeline.
		/// </summary>
		private void LoadNewPipeline () {
			ClearPipeline ();
			if (!GlobalSettings.useTemplateOnCreateNewPipeline) {
				treeFactory.LoadPipeline (ScriptableObject.CreateInstance<Broccoli.Pipe.Pipeline> (), true);
			} else {
				LoadPipelineAsset (ExtensionManager.fullExtensionPath + GlobalSettings.templateOnCreateNewPipelinePath);
				treeFactory.localPipelineFilepath = "";
			}
			if (GlobalSettings.moveCameraToPipeline) {
				SceneView.lastActiveSceneView.LookAt (treeFactory.transform.position);
			}
			OpenTreeFactoryWindow (treeFactory);
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty (
				UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene ());
		}
		/// <summary>
		/// Loads a pipeline from an asset file.
		/// </summary>
		/// <param name="pathToAsset">Path to asset.</param>
		private void LoadPipelineAsset (string pathToAsset) {
			if (!pathToAsset.Contains (Application.dataPath)) {
				if (!string.IsNullOrEmpty (pathToAsset))
					ShowNotification (
						new GUIContent ("You should select an asset inside your project folder!"));
			} else {
				pathToAsset = pathToAsset.Replace(Application.dataPath, "Assets");
				AssetDatabase.Refresh ();

				Broccoli.Pipe.Pipeline loadedPipeline =
					AssetDatabase.LoadAssetAtPath<Broccoli.Pipe.Pipeline> (pathToAsset);

				if (loadedPipeline == null) {
					throw new UnityException ("Cannot Load Pipeline: The file at the specified path '" + 
						pathToAsset + "' is no valid save file as it does not contain a Pipeline.");
				} else {
					ClearPipeline ();
					treeFactory.UnloadAndClearPipeline ();
					treeFactory.LoadPipeline (loadedPipeline.Clone (), pathToAsset, true , true);
					if (treeFactory.previewTree != null && treeFactory.previewTree.obj != null) {
						Selection.activeGameObject = treeFactory.gameObject;
						if (GlobalSettings.moveCameraToPipeline) {
							SceneView.FrameLastActiveSceneView ();
						}
					} else if (GlobalSettings.moveCameraToPipeline) {
						SceneView.lastActiveSceneView.LookAt (treeFactory.transform.position);
					}
					Resources.UnloadAsset (loadedPipeline);
					OpenTreeFactoryWindow (treeFactory);
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty (
						UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene ());
				}
			}
		}
		/// <summary>
		/// Saves the pipeline asset.
		/// </summary>
		/// <returns><c>true</c>, if pipeline asset was saved, <c>false</c> otherwise.</returns>
		/// <param name="pathToAsset">Path to asset.</param>
		/// <param name="asNewAsset">If set to <c>true</c> as new asset.</param>
		private bool SavePipelineAsset (string pathToAsset, bool asNewAsset = false) {
			if (!string.IsNullOrEmpty (pathToAsset)) {
				try {
					Broccoli.Pipe.Pipeline pipelineToAsset = 
						AssetDatabase.LoadAssetAtPath<Broccoli.Pipe.Pipeline> (pathToAsset);
					if (pipelineToAsset != null && asNewAsset) {
						AssetDatabase.DeleteAsset (pathToAsset);
						DestroyImmediate (pipelineToAsset, true);
					}
					pipelineToAsset = treeFactory.localPipeline.Clone (pipelineToAsset);
					if (pipelineToAsset.isCatalogItem && 
						GlobalSettings.editCatalogEnabled == false && 
						asNewAsset) 
					{
						pipelineToAsset.isCatalogItem = false;
					}
					pipelineToAsset.treeFactoryPreferences = treeFactory.treeFactoryPreferences.Clone ();
					if (asNewAsset) {
						AssetDatabase.CreateAsset (pipelineToAsset, pathToAsset);
					} else {
						EditorUtility.SetDirty (pipelineToAsset);
					}
					
					List<PipelineElement> pipelineElements = pipelineToAsset.GetElements ();
					for (int i = 0; i < pipelineElements.Count; i++) {
						AssetDatabase.AddObjectToAsset (pipelineElements[i], pathToAsset);
					}

					AssetDatabase.SaveAssets ();
					Resources.UnloadAsset (pipelineToAsset);
					//DestroyImmediate (pipelineToAsset, true);
				} catch (UnityException e) {
					Debug.LogException (e);
					return false;
				}

				EditorUtility.FocusProjectWindow ();
				return true;
			}
			return false;
		}
		#endregion

		#region Draw Functions
		/// <summary>
		/// Draws the side window.
		/// </summary>
		private void DrawSidePanel ()
		{
			DrawLogo ();
			DrawLogBox ();
			EditorGUILayout.Space ();
			EditorGUI.BeginDisabledGroup (!treeFactory.HasValidPipeline ());
			DrawProcessingOptions ();
			EditorGUI.EndDisabledGroup ();
			EditorGUILayout.Space ();
			DrawPersistenceOptions ();
			EditorGUILayout.Space ();
			DrawShowCatalogOptions ();
			EditorGUILayout.Space ();
			DrawSproutGroupList ();
			EditorGUILayout.Space ();
			DrawDebugOptions ();
			EditorGUILayout.Space ();
			DrawShowPrefabOptions ();
			EditorGUILayout.Space ();
			DrawZoomSlider ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			DrawRateMe ();
		}
		/// <summary>
		/// Draws the catalog.
		/// </summary>
		private void DrawCatalogPanel () {
			//GUI.DrawTextureWithTexCoords (windowRect, NodeEditorGUI.Background, Rect.zero);
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Box ("", GUIStyle.none, 
				GUILayout.Width (150), 
				GUILayout.Height (60));
			GUI.DrawTexture (new Rect (5, 8, 140, 48), GUITextureManager.GetLogo (), ScaleMode.ScaleToFit);
			string catalogMsg = "Broccoli Tree Creator Catalog.\n v 1.0\n\nShowing " + catalog.totalItems + 
				" elements in " + catalog.totalCategories + " categories.";
			EditorGUILayout.HelpBox (catalogMsg, MessageType.None);
			EditorGUILayout.EndHorizontal ();
			if (GUILayout.Button (new GUIContent ("Close Catalog"))) {
				SetEditorView (EditorView.MainOptions);
			}
			catalogScroll = EditorGUILayout.BeginScrollView (catalogScroll, GUIStyle.none, TreeCanvasGUI.verticalScrollStyle);
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
							columns, TreeCanvasGUI.catalogItemStyle, GUILayout.Height (height), GUILayout.Width (windowRect.width - 8));
					if (selectedIndex >= 0 &&
					   EditorUtility.DisplayDialog (MSG_LOAD_CATALOG_ITEM_TITLE, 
						   MSG_LOAD_CATALOG_ITEM_MESSAGE, 
						   MSG_LOAD_CATALOG_ITEM_OK, 
						   MSG_LOAD_CATALOG_ITEM_CANCEL)) {
						SetEditorView (EditorView.MainOptions);
						LoadPipelineAsset (ExtensionManager.fullExtensionPath + 
							catalog.GetItemAtIndex (categoryKey, selectedIndex).path);
					}
				}
			}
			EditorGUILayout.EndScrollView ();
		}

		/// <summary>
		/// Draws the factory options panel.
		/// </summary>
		private void DrawFactoryOptionsPanel () {
			DrawLogo ();
			DrawLogBox ();
			EditorGUILayout.Space ();
			if (GUILayout.Button (new GUIContent ("Close Advanced Options"))) {
				SetEditorView (EditorView.MainOptions);
			}
			EditorGUILayout.Space ();
			DrawFactoryOptions ();
			EditorGUILayout.Space ();
			DrawMaterialOptions ();
			EditorGUILayout.Space ();
			DrawPrefabOptions ();
			if (GlobalSettings.editCatalogEnabled) { 
				// Must check against false, otherwise it raises a warning for unreachable code (const var).
				EditorGUILayout.Space ();
				treeFactory.localPipeline.isCatalogItem = 
					EditorGUILayout.Toggle ("Pipeline is a catalog item", treeFactory.localPipeline.isCatalogItem);
			}
			#if BROCCOLI_DEVEL
			//DrawBroccoliDevel ();
			#endif
		}
		/// <summary>
		/// Default view when no valid treeFactory is assigned to the window.
		/// </summary>
		private void DrawNotLoadedView () {
			EditorGUILayout.HelpBox(MSG_NOT_LOADED, MessageType.Warning);
		}
		/// <summary>
		/// Default view when in play mode.
		/// </summary>
		private void DrawPlayModeView () {
			EditorGUILayout.HelpBox(MSG_PLAY_MODE, MessageType.Warning);
		}
		/// <summary>
		/// Draws the logo.
		/// </summary>
		private void DrawLogo () {
			GUILayout.Space (58);
			if (GUITextureManager.GetLogo () != null) {
				GUI.DrawTexture (new Rect (5, 8, 180, 48), GUITextureManager.GetLogo (), ScaleMode.ScaleToFit);
			}
		}
		/// <summary>
		/// Draws the log box.
		/// </summary>
		private void DrawLogBox () {
			if (serializedTreeFactory != null && treeFactory != null) {
				if (treeFactory.log.Count > 0) {
					var enumerator = treeFactory.log.GetEnumerator ();
					while (enumerator.MoveNext ()) {
						var logItem = enumerator.Current;
						MessageType messageType = UnityEditor.MessageType.Info;
						switch (logItem.messageType) {
						case LogItem.MessageType.Error:
							messageType = UnityEditor.MessageType.Error;
							break;
						case LogItem.MessageType.Warning:
							messageType = UnityEditor.MessageType.Warning;
							break;
						}
						EditorGUILayout.HelpBox (logItem.message, messageType);
					}
				} else {
					pipelineLegend = "Valid Pipeline";
					if (Broccoli.Manager.MaterialManager.renderPipelineType == Manager.MaterialManager.RenderPipelineType.URP) {
						pipelineLegend += " (URP).";
					} else if (Broccoli.Manager.MaterialManager.renderPipelineType == Manager.MaterialManager.RenderPipelineType.HDRP) {
						pipelineLegend += " (HDRP).";
					} else {
						pipelineLegend += " (Std RP).";
					}
					EditorGUILayout.HelpBox (pipelineLegend, UnityEditor.MessageType.Info);
				}
			}
		}
		/// <summary>
		/// Draws the persistence options.
		/// </summary>
		private void DrawPersistenceOptions () {
			if (GUILayout.Button (new GUIContent ("Create New Pipeline", "Creates a new pipeline to work with."))) {
				if (treeFactory.localPipeline.GetElementsCount () == 0 ||
					EditorUtility.DisplayDialog (MSG_NEW_PIPELINE_TITLE, 
						MSG_NEW_PIPELINE_MESSAGE, MSG_NEW_PIPELINE_OK, MSG_NEW_PIPELINE_CANCEL)) {
					LoadNewPipeline ();
				}
			}
			if (GUILayout.Button (new GUIContent ("Load From Asset", "Load the Pipeline from an asset"))) {
				string panelPath = ExtensionManager.fullExtensionPath + GlobalSettings.pipelineSavePath;
				string path = EditorUtility.OpenFilePanel("Load Pipeline", panelPath, "asset");
				LoadPipelineAsset (path);
			}
			if (GUILayout.Button (new GUIContent ("Save As New Asset", "Save the Pipeline as an asset"))) {
				string panelPath = ExtensionManager.fullExtensionPath + GlobalSettings.pipelineSavePath;
				string path = EditorUtility.SaveFilePanelInProject ("Save Pipeline", "TreePipeline", "asset", "", panelPath);
				if (SavePipelineAsset (path, true)) {
					treeFactory.localPipelineFilepath = path;
					if (treeFactory.localPipeline.isCatalogItem && !GlobalSettings.editCatalogEnabled) {
						treeFactory.localPipeline.isCatalogItem = false;
					}
					ShowNotification (new GUIContent ("Asset saved at " + path));
					GUIUtility.ExitGUI();
				}
			}
			bool filePathEmpty = string.IsNullOrEmpty (treeFactory.localPipelineFilepath);
			EditorGUI.BeginDisabledGroup (filePathEmpty || 
				(!GlobalSettings.editCatalogEnabled && treeFactory.localPipeline.isCatalogItem));
			if (GUILayout.Button (new GUIContent ("Save"))) {
				if (SavePipelineAsset (treeFactory.localPipelineFilepath)) {
					ShowNotification (new GUIContent ("Asset saved at " + treeFactory.localPipelineFilepath));
					GUIUtility.ExitGUI();
				}
			}
			if (!filePathEmpty) {
				if (treeFactory.localPipeline != null && 
					treeFactory.localPipeline.isCatalogItem && 
					!GlobalSettings.editCatalogEnabled) {
					EditorGUILayout.HelpBox ("To persist changes save this pipeline as a new asset.", MessageType.Info);
				} else {
					GUILayout.Label (new GUIContent ("at: " + treeFactory.localPipelineFilepath, 
						treeFactory.localPipelineFilepath), BroccoEditorGUI.label);
				}
			}
			EditorGUI.EndDisabledGroup ();
		}
		/// <summary>
		/// Draws the processing options.
		/// </summary>
		private void DrawProcessingOptions () {
			if (GUILayout.Button(new GUIContent("Generate New Preview", GUITextureManager.newPreviewBtnTexture,
				"Process the pipeline to generate a new preview tree."))) {
				treeFactory.ProcessPipelinePreview ();
			}
			EditorGUILayout.Space ();
			if (GUILayout.Button(new GUIContent("Create Prefab", GUITextureManager.createPrefabBtnTexture,
				"Creates a prefab out of the preview tree processed by the pipeline."))) {
				if (treeFactory.CreatePrefab ()) {
					ShowNotification (
						new GUIContent ("Prefab created at " + treeFactory.lastPrefabPath));
					GUIUtility.ExitGUI();
				}
			}
		}
		/// <summary>
		/// Draws the catalog options.
		/// </summary>
		private void DrawShowCatalogOptions () {
			if (GUILayout.Button (new GUIContent ("From Catalog", 
				EditorGUIUtility.FindTexture ("FolderFavorite Icon"), 
				"Opens the catalog to select a predefined pipeline to work with."), GUILayout.Height(25), GUILayout.ExpandWidth(true))) {
				SetEditorView (EditorView.Catalog);
				catalog = BroccoliCatalog.GetInstance ();
			}
		}
		/// <summary>
		/// Draws the prefab creation options.
		/// </summary>
		private void DrawShowPrefabOptions () {
			if (GUILayout.Button(new GUIContent("Advanced Options",
				"Show the options available on the prefab creation process."), GUILayout.Height(25))) {
				SetEditorView (EditorView.FactoryOptions);
			}
		}
		/// <summary>
		/// Draws the sprout groups list.
		/// </summary>
		private void DrawSproutGroupList () {
			//if (NodeEditorGUI.IsUsingSidePanelSkin ())
				sproutGroupList.DoLayoutList  ();
		}
		/// <summary>
		/// Draws the debug options.
		/// </summary>
		private void DrawDebugOptions () {
			if (treeFactory == null)
				return;
			EditorGUILayout.LabelField ("Preview Mode", BroccoEditorGUI.labelBoldCentered);
			int currentPreviewMode = (int)treeFactory.treeFactoryPreferences.previewMode;

			currentPreviewMode = GUILayout.Toolbar (currentPreviewMode, previewOptions, GUI.skin.button);

			EditorGUILayout.Space ();
			/*
			EditorGUILayout.LabelField ("Gizmo Options", BroccoEditorGUI.labelBoldCentered);
			bool debugDrawBranches = GUILayout.Toggle (treeFactory.treeFactoryPreferences.debugShowDrawBranches, 
				" Branches");
			bool debugDrawSprouts = GUILayout.Toggle (treeFactory.treeFactoryPreferences.debugShowDrawSprouts, 
				" Sprouts");
			if (debugDrawBranches != treeFactory.treeFactoryPreferences.debugShowDrawBranches ||
				debugDrawSprouts != treeFactory.treeFactoryPreferences.debugShowDrawSprouts) {
				treeFactory.treeFactoryPreferences.debugShowDrawBranches = debugDrawBranches;
				treeFactory.treeFactoryPreferences.debugShowDrawSprouts = debugDrawSprouts;
				SceneView.RepaintAll ();
			}
			*/

			if (currentPreviewMode != (int)treeFactory.treeFactoryPreferences.previewMode) {
				treeFactory.treeFactoryPreferences.previewMode = (TreeFactory.PreviewMode)currentPreviewMode;
				if (treeFactory.localPipeline.state == Broccoli.Pipe.Pipeline.State.Valid) {
					treeFactory.ProcessPipelinePreview (null, true);
				}
			}

			if (GlobalSettings.showPipelineDebugOption) {
				if (GUILayout.Button (new GUIContent ("Print Pipeline Info", 
					"Prints pipeline debugging information to the console."))) {
					if (treeFactory.localPipeline != null) {
						Debug.Log ("Pipeline has: " + treeFactory.localPipeline.GetElementsCount());
						List<PipelineElement> pipelineElements = treeFactory.localPipeline.GetElements ();
						for (int i = 0; i < pipelineElements.Count; i++) {
							Debug.Log ("ID: " + pipelineElements[i].id + ", " + pipelineElements[i].ToString ());
						}
					}
				}
			}
		}
		/// <summary>
		/// Draws the factory options.
		/// </summary>
		private void DrawFactoryOptions () {
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Factory Scale", BroccoEditorGUI.label);
			float factoryScale = treeFactory.treeFactoryPreferences.factoryScale;
			factoryScale = EditorGUILayout.FloatField (factoryScale, GUILayout.Width (100));
			EditorGUILayout.EndHorizontal ();
			if (factoryScale > 0 && factoryScale != treeFactory.treeFactoryPreferences.factoryScale) {
				factoryScale = (Mathf.RoundToInt (factoryScale * 1000)) / 1000f;
				treeFactory.treeFactoryPreferences.factoryScale = factoryScale;
				TreeFactory.GetActiveInstance ().ProcessPipelinePreview (null, true);
			}
			if (treeFactory.localPipeline.IsValid ()) {
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Processing Options", BroccoEditorGUI.labelBoldCentered);
				DrawLastSeed ();
				if (GUILayout.Button (new GUIContent ("Regenerate Preview"))) {
					treeFactory.ProcessPipelinePreview (null, true, true);
				}
				if (GUILayout.Button (new GUIContent ("Generate with Custom Seed"))) {
					treeFactory.localPipeline.seed = treeFactory.treeFactoryPreferences.customSeed;
					treeFactory.ProcessPipelinePreview (null, true, true);
				}
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Custom Seed", BroccoEditorGUI.label);
				int customSeed = treeFactory.treeFactoryPreferences.customSeed;
				customSeed = EditorGUILayout.IntField (customSeed, GUILayout.Width (100));
				EditorGUILayout.EndHorizontal ();
				if (customSeed != treeFactory.treeFactoryPreferences.customSeed) {
					treeFactory.treeFactoryPreferences.customSeed = customSeed;
				}
			}
		}
		/// <summary>
		/// Draws the options on materials.
		/// </summary>
		private void DrawMaterialOptions () {
			EditorGUILayout.LabelField ("Material Options", BroccoEditorGUI.labelBoldCentered);

			// Preferred tree shader
			EditorGUILayout.LabelField ("Shader", BroccoEditorGUI.label);
			TreeFactoryPreferences.PreferredShader preferredShader = 
				(TreeFactoryPreferences.PreferredShader)EditorGUILayout.EnumPopup (treeFactory.treeFactoryPreferences.preferredShader);
			if (preferredShader != treeFactory.treeFactoryPreferences.preferredShader) {
				treeFactory.treeFactoryPreferences.preferredShader = preferredShader;
				treeFactory.ProcessPipelinePreview (null, true, true);
				Broccoli.Controller.BroccoTreeController controller = 
					treeFactory.previewTree.obj.GetComponent<Broccoli.Controller.BroccoTreeController> ();
				if (controller != null) {
					controller.EnableEditorWind (controller.editorWindEnabled);
				}
			}
			if (preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree7Compatible || 
				preferredShader == TreeFactoryPreferences.PreferredShader.SpeedTree8Compatible) {
				EditorGUILayout.Space ();
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Shader File", BroccoEditorGUI.label);
				Shader customShader = (Shader)EditorGUILayout.ObjectField (treeFactory.treeFactoryPreferences.customBranchShader, typeof(Shader), true, GUILayout.Width (100));
				EditorGUILayout.EndHorizontal ();
				if (customShader != treeFactory.treeFactoryPreferences.customBranchShader) {
					treeFactory.treeFactoryPreferences.customBranchShader = customShader;
					treeFactory.treeFactoryPreferences.customSproutShader = customShader;
					TreeFactory.GetActiveInstance ().ProcessPipelinePreview (null, true, true);
				}
			}
			/* TODO: remove Tree Creator.
			if (preferredShader == TreeFactoryPreferences.PreferredShader.TreeCreatorCompatible) {
				EditorGUILayout.Space ();
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Branch Shader", BroccoEditorGUI.label);
				Shader customBranchShader = (Shader)EditorGUILayout.ObjectField (treeFactory.treeFactoryPreferences.customBranchShader, typeof(Shader), true, GUILayout.Width (100));
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Sprout Shader", BroccoEditorGUI.label);
				Shader customSproutShader = (Shader)EditorGUILayout.ObjectField (treeFactory.treeFactoryPreferences.customSproutShader, typeof(Shader), true, GUILayout.Width (100));
				EditorGUILayout.EndHorizontal ();
				if (customBranchShader != treeFactory.treeFactoryPreferences.customBranchShader || 
					customSproutShader != treeFactory.treeFactoryPreferences.customSproutShader) {
					treeFactory.treeFactoryPreferences.customBranchShader = customBranchShader;
					treeFactory.treeFactoryPreferences.customSproutShader = customSproutShader;
					TreeFactory.GetActiveInstance ().ProcessPipelinePreview (null, true, true);
				}
			}
			*/
			EditorGUILayout.LabelField ("*SRP support relies on the shader availability for the render pipeline.", BroccoEditorGUI.label);

			// Override shader.
			/* Deprecated
			EditorGUILayout.BeginHorizontal ();
			bool overrideShader = GUILayout.Toggle (treeFactory.treeFactoryPreferences.overrideMaterialShaderEnabled, "");
			EditorGUILayout.LabelField ("Override shader on custom materials (for WindZone).");
			EditorGUILayout.EndHorizontal ();
			if (overrideShader != treeFactory.treeFactoryPreferences.overrideMaterialShaderEnabled) {
				treeFactory.treeFactoryPreferences.overrideMaterialShaderEnabled = overrideShader;
				TreeFactory.GetActiveInstance ().ProcessPipelinePreview (null, true, true);
			}
			*/
		}
		/// <summary>
		/// Draws options on prefab creation.
		/// </summary>
		private void DrawPrefabOptions () {
			EditorGUILayout.LabelField ("Prefab Options", BroccoEditorGUI.labelBoldCentered);

			// Clone custom materials.
			EditorGUILayout.BeginHorizontal ();
			bool cloneCustomMaterials = GUILayout.Toggle (treeFactory.treeFactoryPreferences.prefabCloneCustomMaterialEnabled, "");
			EditorGUILayout.LabelField ("Clone assigned custom materials.", BroccoEditorGUI.label);
			EditorGUILayout.EndHorizontal ();
			if (cloneCustomMaterials != treeFactory.treeFactoryPreferences.prefabCloneCustomMaterialEnabled) {
				treeFactory.treeFactoryPreferences.prefabCloneCustomMaterialEnabled = cloneCustomMaterials;
			}

			// Materials and mesh to folder enabled.
			EditorGUILayout.BeginHorizontal ();
			bool includeAssetsInsidePrefab = GUILayout.Toggle (treeFactory.treeFactoryPreferences.prefabIncludeAssetsInsidePrefab, "");
			EditorGUILayout.LabelField ("Include materials and meshes inside the prefab.", BroccoEditorGUI.label);
			EditorGUILayout.EndHorizontal ();
			if (includeAssetsInsidePrefab != treeFactory.treeFactoryPreferences.prefabIncludeAssetsInsidePrefab) {
				treeFactory.treeFactoryPreferences.prefabIncludeAssetsInsidePrefab = includeAssetsInsidePrefab;
			}

			// Copy textures from a bark custom material with shader override to the prefab folder.
			EditorGUILayout.BeginHorizontal ();
			bool copyCustomMaterialTextures = GUILayout.Toggle (treeFactory.treeFactoryPreferences.prefabCopyCustomMaterialBarkTexturesEnabled, "");
			EditorGUILayout.LabelField ("Copy bark textures to prefab folder.", BroccoEditorGUI.label);
			EditorGUILayout.EndHorizontal ();
			if (copyCustomMaterialTextures != treeFactory.treeFactoryPreferences.prefabCopyCustomMaterialBarkTexturesEnabled) {
				treeFactory.treeFactoryPreferences.prefabCopyCustomMaterialBarkTexturesEnabled = copyCustomMaterialTextures;
			}

			// Create atlas.
			EditorGUILayout.BeginHorizontal ();
			bool createAtlas = GUILayout.Toggle (treeFactory.treeFactoryPreferences.prefabCreateAtlas, "");
			EditorGUILayout.LabelField ("Create texture atlas for sprouts.", BroccoEditorGUI.label);
			EditorGUILayout.EndHorizontal ();
			if (createAtlas != treeFactory.treeFactoryPreferences.prefabCreateAtlas) {
				treeFactory.treeFactoryPreferences.prefabCreateAtlas = createAtlas;
			}
			if (treeFactory.treeFactoryPreferences.prefabCreateAtlas) {
				// Atlas size.
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Atlas Size", BroccoEditorGUI.label);
				TreeFactory.TextureSize atlasTextureSize = 
					(TreeFactory.TextureSize)EditorGUILayout.EnumPopup (treeFactory.treeFactoryPreferences.atlasTextureSize, GUILayout.Width (120));
				EditorGUILayout.EndHorizontal ();
				if (atlasTextureSize != treeFactory.treeFactoryPreferences.atlasTextureSize) {
					treeFactory.treeFactoryPreferences.atlasTextureSize = atlasTextureSize;
				}
			}
			EditorGUILayout.Space ();

			// Appendable components.
			/* Deprecated
			if (NodeEditorGUI.IsUsingSidePanelSkin ()) {
				appendableComponentList.DoLayoutList ();
			}
			EditorGUILayout.LabelField ("*Appendable scripts are added to final prefab as components.");
				
			EditorGUILayout.Space ();
			*/

			EditorGUILayout.LabelField ("Prefab Meshes", BroccoEditorGUI.labelBoldCentered);
			/*
			// Strict low poly.
			EditorGUILayout.BeginHorizontal ();
			bool strictLowPolyEnabled = GUILayout.Toggle (treeFactory.treeFactoryPreferences.prefabStrictLowPoly, "");
			EditorGUILayout.LabelField ("Strict low-poly prefab mesh generation (no LODs).", BroccoEditorGUI.label);
			EditorGUILayout.EndHorizontal ();
			if (strictLowPolyEnabled != treeFactory.treeFactoryPreferences.prefabStrictLowPoly) {
				treeFactory.treeFactoryPreferences.prefabStrictLowPoly = strictLowPolyEnabled;
				TreeFactory.GetActiveInstance ().ProcessPipelinePreview (null, true);
			}
			// LOD groups.
			if (!treeFactory.treeFactoryPreferences.prefabStrictLowPoly) {
				EditorGUILayout.BeginHorizontal ();
				bool LODGroupsEnabled = GUILayout.Toggle (treeFactory.treeFactoryPreferences.prefabUseLODGroups, "");
				EditorGUILayout.LabelField ("Use LOD groups on final prefab.", BroccoEditorGUI.label);
				EditorGUILayout.EndHorizontal ();
				if (LODGroupsEnabled != treeFactory.treeFactoryPreferences.prefabUseLODGroups) {
					treeFactory.treeFactoryPreferences.prefabUseLODGroups = LODGroupsEnabled;
				}
			}
			// Include billboard or not.
			EditorGUILayout.BeginHorizontal ();
			bool includeBillboard = GUILayout.Toggle (treeFactory.treeFactoryPreferences.prefabIncludeBillboard, "");
			EditorGUILayout.LabelField ("Include Billboard Asset.", BroccoEditorGUI.label);
			EditorGUILayout.EndHorizontal ();
			if (includeBillboard != treeFactory.treeFactoryPreferences.prefabIncludeBillboard) {
				treeFactory.treeFactoryPreferences.prefabIncludeBillboard = includeBillboard;
			}
			*/
			// Billboard size.
			if (treeFactory.treeFactoryPreferences.prefabIncludeBillboard) {
				// Billboard texture size.
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Billboard Texture", BroccoEditorGUI.label);
				TreeFactory.TextureSize billboardTextureSize = 
					(TreeFactory.TextureSize)EditorGUILayout.EnumPopup (treeFactory.treeFactoryPreferences.billboardTextureSize, 
						GUILayout.Width (105));
				EditorGUILayout.EndHorizontal ();
				if (billboardTextureSize != treeFactory.treeFactoryPreferences.billboardTextureSize) {
					treeFactory.treeFactoryPreferences.billboardTextureSize = billboardTextureSize;
				}
			}
			// Reposition root.
			EditorGUILayout.BeginHorizontal ();
			bool repositionEnabled = GUILayout.Toggle (treeFactory.treeFactoryPreferences.prefabRepositionEnabled, "");
			EditorGUILayout.LabelField ("Re-position prefab mesh to zero if the tree has a single root.", BroccoEditorGUI.label);
			EditorGUILayout.EndHorizontal ();
			if (repositionEnabled != treeFactory.treeFactoryPreferences.prefabRepositionEnabled) {
				treeFactory.treeFactoryPreferences.prefabRepositionEnabled = repositionEnabled;
			}
			EditorGUILayout.Space ();

			// Display Broccoli Version.
			EditorGUILayout.HelpBox ("Broccoli Tree Creator v" + BroccoliExtensionInfo.GetVersion (), MessageType.None);
		}
		/// <summary>
		/// Draws the zoom slider.
		/// </summary>
		private void DrawZoomSlider () {
			EditorGUILayout.Space ();
			GUILayout.Label("Zoom: " + Mathf.RoundToInt((1 / canvasCache.editorState.zoom * 100)) + "%", BroccoEditorGUI.label);
			//canvasCache.editorState.zoom = GUILayout.HorizontalSlider (canvasCache.editorState.zoom, 0.6f, 4f);
			normalizedZoom = GUILayout.HorizontalSlider (normalizedZoom, -5f, 3f);
			normalizedZoom = Mathf.RoundToInt (normalizedZoom);
			canvasCache.editorState.ApplyStepZoom ((int) normalizedZoom);
		}
		/// <summary>
		/// Draws the last seed.
		/// </summary>
		private void DrawLastSeed () {
			if (treeFactory != null && treeFactory.localPipeline != null) {
				EditorGUILayout.LabelField ("Last Seed: " + treeFactory.localPipeline.seed, BroccoEditorGUI.label);
			}
		}
		private void DrawRateMe () {
			EditorGUILayout.HelpBox ("Your honest review and rating will help us greatly to continue improving this asset or you.", MessageType.None);
			if (GUILayout.Button ("Rate Us!")) {
				Application.OpenURL("http://u3d.as/1emq");
			}
		}
		private void DrawBroccoliDevel () {
			bool useAutoCalculateTangents = GlobalSettings.useAutoCalculateTangents;
			useAutoCalculateTangents = EditorGUILayout.Toggle ("Auto Tangents", useAutoCalculateTangents);
			if (useAutoCalculateTangents != GlobalSettings.useAutoCalculateTangents) {
				GlobalSettings.useAutoCalculateTangents = useAutoCalculateTangents;
			}
			
			bool useCrossMeshPerpendicularNormasl = GlobalSettings.useCrossMeshPerpendicularNormasl;
			useCrossMeshPerpendicularNormasl = EditorGUILayout.Toggle ("Perpendicular Normals", useCrossMeshPerpendicularNormasl);
			if (useCrossMeshPerpendicularNormasl != GlobalSettings.useCrossMeshPerpendicularNormasl) {
				GlobalSettings.useCrossMeshPerpendicularNormasl = useCrossMeshPerpendicularNormasl;
			}
			Vector3 crossAPoint1 = GlobalSettings.crossAPoint1;
			crossAPoint1 = EditorGUILayout.Vector3Field ("crossAPoint1", crossAPoint1);
			if (crossAPoint1 != GlobalSettings.crossAPoint1) {
				GlobalSettings.crossAPoint1 = crossAPoint1;
			}
			Vector3 crossAPoint2 = GlobalSettings.crossAPoint2;
			crossAPoint2 = EditorGUILayout.Vector3Field ("crossAPoint2", crossAPoint2);
			if (crossAPoint2 != GlobalSettings.crossAPoint2) {
				GlobalSettings.crossAPoint2 = crossAPoint2;
			}
			Vector3 crossAPoint3 = GlobalSettings.crossAPoint3;
			crossAPoint3 = EditorGUILayout.Vector3Field ("crossAPoint3", crossAPoint3);
			if (crossAPoint3 != GlobalSettings.crossAPoint3) {
				GlobalSettings.crossAPoint3 = crossAPoint3;
			}
			Vector3 crossAPoint4 = GlobalSettings.crossAPoint4;
			crossAPoint4 = EditorGUILayout.Vector3Field ("crossAPoint4", crossAPoint4);
			if (crossAPoint4 != GlobalSettings.crossAPoint4) {
				GlobalSettings.crossAPoint4 = crossAPoint4;
			}
			Vector3 tangAPoint1 = GlobalSettings.tangAPoint1;
			tangAPoint1 = EditorGUILayout.Vector3Field ("tangAPoint1", tangAPoint1);
			if (tangAPoint1 != GlobalSettings.tangAPoint1) {
				GlobalSettings.tangAPoint1 = tangAPoint1;
			}
			Vector3 tangAPoint2 = GlobalSettings.tangAPoint2;
			tangAPoint2 = EditorGUILayout.Vector3Field ("tangAPoint2", tangAPoint2);
			if (tangAPoint2 != GlobalSettings.tangAPoint2) {
				GlobalSettings.tangAPoint2 = tangAPoint2;
			}
			Vector3 tangAPoint3 = GlobalSettings.tangAPoint3;
			tangAPoint3 = EditorGUILayout.Vector3Field ("tangAPoint3", tangAPoint3);
			if (tangAPoint3 != GlobalSettings.tangAPoint3) {
				GlobalSettings.tangAPoint3 = tangAPoint3;
			}
			Vector3 tangAPoint4 = GlobalSettings.tangAPoint4;
			tangAPoint4 = EditorGUILayout.Vector3Field ("tangAPoint4", tangAPoint4);
			if (tangAPoint4 != GlobalSettings.tangAPoint4) {
				GlobalSettings.tangAPoint4 = tangAPoint4;
			}
			Vector3 crossBPoint1 = GlobalSettings.crossBPoint1;
			crossBPoint1 = EditorGUILayout.Vector3Field ("crossBPoint1", crossBPoint1);
			if (crossBPoint1 != GlobalSettings.crossBPoint1) {
				GlobalSettings.crossBPoint1 = crossBPoint1;
			}
			Vector3 crossBPoint2 = GlobalSettings.crossBPoint2;
			crossBPoint2 = EditorGUILayout.Vector3Field ("crossBPoint2", crossBPoint2);
			if (crossBPoint2 != GlobalSettings.crossBPoint2) {
				GlobalSettings.crossBPoint2 = crossBPoint2;
			}
			Vector3 crossBPoint3 = GlobalSettings.crossBPoint3;
			crossBPoint3 = EditorGUILayout.Vector3Field ("crossBPoint3", crossBPoint3);
			if (crossBPoint3 != GlobalSettings.crossBPoint3) {
				GlobalSettings.crossBPoint3 = crossBPoint3;
			}
			Vector3 crossBPoint4 = GlobalSettings.crossBPoint4;
			crossBPoint4 = EditorGUILayout.Vector3Field ("crossBPoint4", crossBPoint4);
			if (crossBPoint4 != GlobalSettings.crossBPoint4) {
				GlobalSettings.crossBPoint4 = crossBPoint4;
			}
		}
		#endregion
	}
}