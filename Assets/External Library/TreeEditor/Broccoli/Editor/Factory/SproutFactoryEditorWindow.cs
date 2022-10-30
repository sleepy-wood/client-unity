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

namespace Broccoli.BroccoEditor
{
	/// <summary>
	/// Tree factory editor window.
	/// Contains the canvas to edit pipeline element nodes and
	/// all the available tree factory commands.
	/// </summary>
	public class SproutFactoryEditorWindow : EditorWindow 
	{
		#region Vars
		/// <summary>
		/// The tree factory game object.
		/// </summary>
		public GameObject sproutFactoryGameObject;
		/// <summary>
		/// The tree factory behind the the canvas.
		/// </summary>
		public SproutFactory sproutFactory;
		/// <summary>
		/// The tree factory serialized object behind the the canvas.
		/// </summary>
		public SerializedObject serializedSproutFactory;
        /// <summary>
		/// True when the editor is on play mode.
		/// </summary>
		private bool isPlayModeView = false;
        /// <summary>
		/// Gets the window rect.
		/// </summary>
		/// <value>The total window rect.</value>
		public Rect windowRect { 
			get { 
				return new Rect (0, 0, position.width, position.height); 
			} 
		}
		#endregion

		#region Subeditors
		private SproutLabEditor sproutLabEditor = null;
		#endregion

		#region Messages
		static string MSG_NOT_LOADED = "To create sprout textures please select a Sprout Lab " +
			"GameObject then press 'Open Sprout Lab Editor' on the script inspector.";
		static string MSG_PLAY_MODE = "Sprout Lab editor is not available on play mode.";
		#endregion

		#region Singleton
		/// <summary>
		/// The editor window singleton.
		/// </summary>
		private static SproutFactoryEditorWindow _sproutFactoryWindow;
		/// <summary>
		/// Gets the editor window.
		/// </summary>
		/// <value>The editor.</value>
		public static SproutFactoryEditorWindow editorWindow { get { return _sproutFactoryWindow; } }
		/// <summary>
		/// Gets the tree editor window.
		/// </summary>
		static void GetWindow () {
			_sproutFactoryWindow = GetWindow<SproutFactoryEditorWindow> ();
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Static initializer for the <see cref="Broccoli.TreeNodeEditor.SproutFactoryEditorWindow"/> class.
		/// </summary>
		static SproutFactoryEditorWindow () {}
		#endregion

		#region Open / Close
		/// <summary>
		/// Opens the tree factory window.
		/// </summary>
		[MenuItem("Window/Broccoli/Sprout Lab Editor")]
		static void OpenSproutFactoryWindow ()
		{
			SproutFactory sproutFactory = null;
			if (Selection.activeGameObject != null) {
				sproutFactory = Selection.activeGameObject.GetComponent<SproutFactory> ();
			}
			OpenSproutFactoryWindow (sproutFactory);
		}
		/// <summary>
		/// Checks if the menu item for opening the Tree Editor should be enabled.
		/// </summary>
		/// <returns><c>true</c>, if tree factory window is closed, <c>false</c> otherwise.</returns>
		[MenuItem("Window/Broccoli/Sprout Lab Editor", true)]
		static bool ValidateOpenSproutFactoryWindow ()
		{
			return !IsOpen ();
		}
		/// <summary>
		/// Opens the Node Editor window loading the pipeline contained on the SproutFactory object.
		/// </summary>
		/// <returns>The node editor.</returns>
		/// <param name="sproutFactory">Tree factory.</param>
		public static SproutFactoryEditorWindow OpenSproutFactoryWindow (SproutFactory sproutFactory = null) 
		{
			GUITextureManager.Init (true);
			GetWindow ();

			if (EditorApplication.isPlayingOrWillChangePlaymode && !GlobalSettings.useTreeEditorOnPlayMode) {
				_sproutFactoryWindow.isPlayModeView = true;
			} else {
				_sproutFactoryWindow.isPlayModeView = false;
			}

			//if (treeFactory != null && treeFactory != _factoryWindow.treeFactory) {
			if (sproutFactory != null) {
				SetupSproutFactory (sproutFactory, _sproutFactoryWindow);
			}

			Texture iconTexture = 
				ResourceManager.LoadTexture (EditorGUIUtility.isProSkin? "Textures/Icon_Dark.png" : "Textures/Icon_Light.png");
			_sproutFactoryWindow.titleContent = new GUIContent ("Sprout Lab Editor", iconTexture);

			return _sproutFactoryWindow;
		}
		/// <summary>
		/// Setups the canvas.
		/// </summary>
		/// <param name="sproutFactory">Tree factory.</param>
		/// <param name="factoryWindow">Factory window.</param>
		public static void SetupSproutFactory (SproutFactory sproutFactory, SproutFactoryEditorWindow factoryWindow) {
			factoryWindow.sproutFactory = sproutFactory;
			factoryWindow.sproutFactoryGameObject = sproutFactory.gameObject;
			sproutFactory.SetInstanceAsActive ();
			factoryWindow.serializedSproutFactory = new SerializedObject (sproutFactory);
			factoryWindow.minSize = new Vector2 (400, 200);
			factoryWindow.sproutLabEditor = null;

			GUIScaleUtility.CheckInit ();
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
			sproutFactory = null;
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
		#endregion

		#region Events
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		void OnEnable()
		{
			_sproutFactoryWindow = this;
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
			if (sproutFactory == null && sproutFactoryGameObject != null) {
				sproutFactory = sproutFactoryGameObject.GetComponent<SproutFactory> ();
				//SetupCanvas (sproutFactory, this);
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
		/// Raises the destroy event.
		/// </summary>
		private void OnDestroy () {
			NodeEditor.ClientRepaints -= Repaint;

			EditorLoadingControl.beforeEnteringPlayMode -= OnBeforeEnteringPlayMode;
			EditorLoadingControl.justLeftPlayMode -= OnJustLeftPlayMode;
			EditorLoadingControl.justOpenedNewScene -= OnJustOpenedNewScene;

			sproutLabEditor.OnDisable ();
			sproutLabEditor = null;

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
				OnSproutLabUndo ();
			}
			if (sproutFactory == null) {
				DrawNotLoadedView ();
				return;
			}
			if (isPlayModeView) {
				DrawPlayModeView ();
				return;
			}

            GUILayout.BeginArea (windowRect);
            DrawSproutLabPanel (windowRect);
            GUILayout.EndArea ();
		}
		#endregion

		#region Sprout Lab
		void InitSproutLab () {
			sproutLabEditor = new SproutLabEditor ();
			sproutLabEditor.onBeforeBranchDescriptorChange -= OnSproutLabBeforeChange;
			sproutLabEditor.onBeforeBranchDescriptorChange += OnSproutLabBeforeChange;
			sproutLabEditor.onBranchDescriptorChange -= OnSproutLabChange;
			sproutLabEditor.onBranchDescriptorChange += OnSproutLabChange;
			sproutLabEditor.onBeforeVariationDescriptorChange -= OnSproutLabBeforeChange;
			sproutLabEditor.onBeforeVariationDescriptorChange += OnSproutLabBeforeChange;
			sproutLabEditor.onVariationDescriptorChange -= OnSproutLabChange;
			sproutLabEditor.onVariationDescriptorChange += OnSproutLabChange;
			sproutLabEditor.onShowNotification -= OnShowNotification;
			sproutLabEditor.onShowNotification += OnShowNotification;
		}
		void OnSproutLabBeforeChange (BranchDescriptorCollection branchDescriptorCollection) {
			Undo.SetCurrentGroupName( "Branch Descriptor Change" );
			Undo.RecordObject (sproutFactory.localPipeline, "Branch Descriptor Changed");
			//Undo.RegisterCompleteObjectUndo (treeFactory.localPipeline, "Branch Descriptor Changed");
		}
		void OnSproutLabChange (BranchDescriptorCollection branchDescriptorCollection) {
			int group = Undo.GetCurrentGroup();
			Undo.CollapseUndoOperations( group );
			sproutFactory.branchDescriptorCollection = branchDescriptorCollection;
			sproutFactory.localPipeline.undoControl.undoCount++;
			EditorUtility.SetDirty (sproutFactory);
		}
		void OnSproutLabUndo () {
			if (sproutLabEditor != null) {
				sproutFactory.branchDescriptorCollection.branchDescriptorIndex = sproutFactory.branchDescriptorCollection.lastBranchDescriptorIndex;
				sproutLabEditor.SelectSnapshot (sproutFactory.branchDescriptorCollection.branchDescriptorIndex);
				sproutLabEditor.ReflectChangesToPipeline ();
				sproutLabEditor.RegeneratePreview ();
			}
		}
		void OnShowNotification (string notification) {
			ShowNotification (new GUIContent (notification));
		}
		#endregion

		#region Draw Functions
		/// <summary>
		/// Draws the SproutLab.
		/// </summary>
		private void DrawSproutLabPanel (Rect windowRect) {
			if (sproutLabEditor == null) {
				InitSproutLab ();
				if (sproutLabEditor.branchDescriptorCollection != sproutFactory.branchDescriptorCollection) {
					sproutLabEditor.LoadBranchDescriptorCollection (sproutFactory.branchDescriptorCollection, sproutFactory.GetSproutSubFactory ());
				}
			}
			//verticalSplitView.BeginSplitView ();
			//verticalSplitView.EndSplitView ();
			sproutLabEditor.Draw (windowRect);
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
		#endregion
	}
}