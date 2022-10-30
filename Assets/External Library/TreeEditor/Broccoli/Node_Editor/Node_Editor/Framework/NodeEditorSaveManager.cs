﻿using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;

namespace Broccoli.NodeEditorFramework 
{
	/// <summary>
	/// Manager handling all save and load operations on NodeCanvases and NodeEditorStates of the Node Editor, both as assets and in the scene
	/// </summary>
	public static partial class NodeEditorSaveManager 
	{
		static List<ScriptableObject> allSOs = new List<ScriptableObject> ();
		static List<ScriptableObject> clonedSOs = new List<ScriptableObject> ();

		#region Scene Saving

		private static GameObject sceneSaveHolder;

		/// <summary>
		/// Fetches the saveHolder of the current scene if not already found or creates it and stores it into sceneSaveHolder
		/// </summary>
		private static void FetchSceneSaveHolder () 
		{
			if (sceneSaveHolder == null)
			{
				// TODO: Might need to check here if the object is in the active scene / the system works with multiple scenes
				//#if UNITY_5_3 | UNITY_5_3_OR_NEWER
				//if (UnityEngine.SceneManagement.SceneManager.GetActiveScene ())
				//#endif
				sceneSaveHolder = GameObject.Find ("NodeEditor_SceneSaveHolder");
				if (sceneSaveHolder == null)
					sceneSaveHolder = new GameObject ("NodeEditor_SceneSaveHolder");
				sceneSaveHolder.hideFlags = HideFlags.None;//HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}
		}

		/// <summary>
		/// Gets all existing stored saves in the current scene and returns their names
		/// </summary>
		public static string[] GetSceneSaves ()
		{ // Get the saveHolder, find the existing stored saves and return their names
			FetchSceneSaveHolder ();
			return sceneSaveHolder.GetComponents<NodeCanvasSceneSave> ().Select (((NodeCanvasSceneSave save) => save.saveName)).ToArray ();
		}

		/// <summary>
		/// Finds a scene save in the current scene with specified name or null if it does not exist
		/// </summary>
		internal static NodeCanvasSceneSave FindSceneSave (string saveName)
		{
			NodeCanvasSceneSave sceneSave = null;
			if (sceneSaveHolder != null)
			{
				sceneSave = sceneSaveHolder.GetComponents<NodeCanvasSceneSave> ().ToList ().Find ((NodeCanvasSceneSave save) => save.saveName == saveName || (save.savedNodeCanvas != null && save.savedNodeCanvas.name == saveName));
				if (sceneSave != null)
					sceneSave.saveName = saveName;
			}
			return sceneSave;
		}

		/// <summary>
		/// Finds a scene save in the current scene with specified name or null if it does not exist
		/// </summary>
		internal static NodeCanvasSceneSave FindOrCreateSceneSave (string saveName)
		{
			FetchSceneSaveHolder ();
			NodeCanvasSceneSave sceneSave = sceneSaveHolder.GetComponents<NodeCanvasSceneSave> ().ToList ().Find ((NodeCanvasSceneSave save) => save.saveName == saveName || save.savedNodeCanvas.name == saveName);
			if (sceneSave == null)
				sceneSave = sceneSaveHolder.AddComponent<NodeCanvasSceneSave> ();
			sceneSave.saveName = saveName;
			return sceneSave;
		}

		/// <summary>
		/// Finds a scene save in the current scene with specified name or null if it does not exist
		/// </summary>
		internal static NodeCanvasSceneSave CreateSceneSave (string saveName)
		{
			FetchSceneSaveHolder ();
			NodeCanvasSceneSave sceneSave = sceneSaveHolder.AddComponent<NodeCanvasSceneSave> ();
			sceneSave.saveName = saveName;
			return sceneSave;
		}

		/// <summary>
		/// Saves the nodeCanvas in the current scene under the specified name along with the specified editorStates or, if specified, their working copies
		/// If also stored as an asset, it will loose the reference to the asset first
		/// </summary>
		public static void SaveSceneNodeCanvas (string saveName, ref NodeCanvas nodeCanvas, bool createWorkingCopy, bool safeOverwrite = true) 
		{
			if (string.IsNullOrEmpty (saveName))
			{
				Debug.LogError ("Cannot save Canvas to scene: No save name specified!");
				return;
			}

			if (nodeCanvas.GetType () == typeof(NodeCanvas)) throw new UnityException ("Cannot save NodeCanvas: The NodeCanvas has no explicit type: '" + nodeCanvas.GetType ().ToString () + "'. Please convert it to a valid type!");

			if (!nodeCanvas.livesInScene
		#if UNITY_EDITOR // Make sure the canvas has no reference to an asset
			|| UnityEditor.AssetDatabase.Contains (nodeCanvas)
		#endif
			) {
				//Debug.LogWarning ("Forced to create working copy of '" + saveName + "' when saving to scene because it already exists as an asset!");
				nodeCanvas = CreateWorkingCopy (nodeCanvas, true);
			}
			else
				nodeCanvas.Validate (true);

			nodeCanvas.livesInScene = true;
			nodeCanvas.name = saveName;

			nodeCanvas.OnBeforeSavingCanvas();

			nodeCanvas.UpdateSource ("SCENE/" + saveName);

			NodeCanvas savedCanvas = nodeCanvas;
			// Preprocess canvas
			ProcessCanvas (ref savedCanvas, createWorkingCopy);

			// Get the saveHolder and store the canvas
			NodeCanvasSceneSave sceneSave;
		#if UNITY_EDITOR
			if ((sceneSave = FindSceneSave (saveName)) != null && safeOverwrite) // OVERWRITE
				OverwriteCanvas (ref sceneSave.savedNodeCanvas, savedCanvas);
			else
			{
				if (sceneSave == null) 
					sceneSave = CreateSceneSave (saveName);
				sceneSave.savedNodeCanvas = savedCanvas;
			}
			if (!Application.isPlaying)
			{
			#if UNITY_5_3_OR_NEWER
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty (UnityEngine.SceneManagement.SceneManager.GetActiveScene ());
			#else
				UnityEditor.EditorApplication.MarkSceneDirty ();
			#endif
			}
		#else
			sceneSave = FindOrCreateSceneSave (saveName);
			sceneSave.savedNodeCanvas = savedCanvas;
		#endif

		#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty (sceneSaveHolder);
		#endif
		}

		/// <summary>
		/// Loads the nodeCanvas and it's editorState stored in the current scene under the specified name and, if specified, creates working copies before returning
		/// </summary>
		public static NodeCanvas LoadSceneNodeCanvas (string saveName, bool createWorkingCopy)
		{
			if (string.IsNullOrEmpty (saveName))
			{
				Debug.LogError ("Cannot load Canvas from scene: No save name specified!");
				return null;
			}

			NodeCanvasSceneSave sceneSave = FindSceneSave (saveName);
			if (sceneSave == null || sceneSave.savedNodeCanvas == null) // No such save file
				return null;

			// Extract the saved canvas and editorStates
			NodeCanvas savedCanvas = sceneSave.savedNodeCanvas;
			savedCanvas.livesInScene = true;

			savedCanvas.UpdateSource ("SCENE/" + saveName);

			// Postprocess the loaded canvas
			ProcessCanvas (ref savedCanvas, createWorkingCopy);

			#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty (sceneSaveHolder);
			#endif

			return savedCanvas;
		}

		/// <summary>
		/// Deletes the nodeCanvas and it's editorState stored in the current scene under the specified name
		/// </summary>
		public static void DeleteSceneNodeCanvas (string saveName)
		{
			if (string.IsNullOrEmpty (saveName))
			{
				Debug.LogError ("Cannot delete Canvas from scene: No save name specified!");
				return;
			}

			NodeCanvasSceneSave sceneSave = FindSceneSave (saveName);
			if (sceneSave != null)
			{
		#if UNITY_EDITOR
				Object.DestroyImmediate (sceneSave);
		#else
				Object.Destroy (sceneSave);
		#endif
			}
		}

		#endregion

		#region Asset Saving

		/// <summary>
		/// Saves the the given NodeCanvas along with the given NodeEditorStates if specified as a new asset, optionally as working copies
		/// </summary>
		public static void SaveNodeCanvas (string path, NodeCanvas nodeCanvas, bool createWorkingCopy, bool safeOverwrite = true) 
		{
		#if !UNITY_EDITOR
			throw new System.NotImplementedException ();
		#else
			if (string.IsNullOrEmpty (path) || !path.StartsWith ("Assets")) {
				throw new UnityException ("Cannot save NodeCanvas: Invalid path specified: '" + path + "'!");
			}
			if (nodeCanvas == null) throw new UnityException ("Cannot save NodeCanvas: The specified NodeCanvas that should be saved to path " + path + " is null!");
			if (nodeCanvas.GetType () == typeof(NodeCanvas)) throw new UnityException ("Cannot save NodeCanvas: The NodeCanvas has no explicit type: '" + nodeCanvas.GetType ().ToString () + "'. Please convert it to a valid type!");
			if (nodeCanvas.livesInScene)
				Debug.LogWarning ("Attempting to save scene canvas " + nodeCanvas.name + " to an asset, scene object references may be broken!" + (!createWorkingCopy? " No workingCopy is going to be created, so your scene save is broken, too!" : ""));
			if (!createWorkingCopy && UnityEditor.AssetDatabase.Contains (nodeCanvas) && UnityEditor.AssetDatabase.GetAssetPath (nodeCanvas) != path) { Debug.LogError ("Trying to create a duplicate save file for '" + nodeCanvas.name + "'! Forcing to create a working copy!"); createWorkingCopy = true; }

			path = ResourceManager.PreparePath (path);

			nodeCanvas.OnBeforeSavingCanvas ();

			NodeCanvas canvasSave = nodeCanvas;

			nodeCanvas.UpdateSource (path);

			// Preprocess the canvas
			ProcessCanvas (ref nodeCanvas, createWorkingCopy);
			nodeCanvas.livesInScene = false;

			canvasSave = nodeCanvas;
			NodeCanvas prevSave;
			if (safeOverwrite && (prevSave = ResourceManager.LoadResource<NodeCanvas> (path)) != null && prevSave.GetType () == canvasSave.GetType ()) // OVERWRITE
			{ // Delete contents of old save
				for (int nodeCnt = 0; nodeCnt < prevSave.nodes.Count; nodeCnt++) 
				{
					Node node = prevSave.nodes[nodeCnt];
					for (int knobCnt = 0; knobCnt < node.nodeKnobs.Count; knobCnt++)
					{
						if (node.nodeKnobs[knobCnt] != null)
							Object.DestroyImmediate (node.nodeKnobs[knobCnt], true);
					}
					Object.DestroyImmediate (node, true);
				}
				for (int i = 0; i < prevSave.editorStates.Length; i++)
				{
					if (prevSave.editorStates[i] != null)
						Object.DestroyImmediate (prevSave.editorStates[i], true);
				}
				// Overwrite main canvas
				OverwriteCanvas (ref prevSave, nodeCanvas);
				canvasSave = prevSave;
			}
			else
			{ // Write main canvas
				UnityEditor.AssetDatabase.CreateAsset (nodeCanvas, path);
			}

			// Write editorStates
			AddSubAssets (nodeCanvas.editorStates, canvasSave);

			// Write nodes + contents
			for (int i = 0; i < nodeCanvas.nodes.Count; i++)
			{ // Write node and additional scriptable objects
				AddSubAsset (nodeCanvas.nodes[i], canvasSave);
				AddSubAssets (nodeCanvas.nodes[i].GetScriptableObjects (), nodeCanvas.nodes[i]);
				for (int j = 0; j < nodeCanvas.nodes[i].nodeKnobs.Count; j++)
				{ // Write knobs and their additional scriptable objects
					AddSubAsset (nodeCanvas.nodes[i].nodeKnobs[j], nodeCanvas.nodes[i]);
					AddSubAssets (nodeCanvas.nodes[i].nodeKnobs[j].GetScriptableObjects (), nodeCanvas.nodes[i].nodeKnobs[j]);
				}
			}

			//UnityEditor.AssetDatabase.SaveAssets ();
			//UnityEditor.AssetDatabase.Refresh ();

			NodeEditorCallbacks.IssueOnSaveCanvas (canvasSave);
		#endif
		}

		/// <summary>
		/// Loads the NodeCanvas from the asset file at path and optionally creates a working copy of it before returning
		/// </summary>
		public static NodeCanvas LoadNodeCanvas (string path, bool createWorkingCopy) 
		{
			if (!File.Exists (path)) throw new UnityException ("Cannot Load NodeCanvas: File '" + path + "' deos not exist!");

			// Load only the NodeCanvas from the save file
			NodeCanvas nodeCanvas = ResourceManager.LoadResource<NodeCanvas> (path);
			if (nodeCanvas == null) throw new UnityException ("Cannot Load NodeCanvas: The file at the specified path '" + path + "' is no valid save file as it does not contain a NodeCanvas!");

			path = ResourceManager.PreparePath (path);

			nodeCanvas.UpdateSource (path);

		#if UNITY_EDITOR
			if (!Application.isPlaying && (nodeCanvas.editorStates == null || nodeCanvas.editorStates.Length == 0))
			{ // Try to load any contained editorStates, possibly old format that did not references the states in the canvas
				nodeCanvas.editorStates = ResourceManager.LoadResources<NodeEditorState> (path);
			}
		#endif

			// Postprocess the loaded canvas
			ProcessCanvas (ref nodeCanvas, createWorkingCopy);

		#if UNITY_EDITOR
			UnityEditor.AssetDatabase.Refresh ();
		#endif
			NodeEditorCallbacks.IssueOnLoadCanvas (nodeCanvas);
			return nodeCanvas;
		}

		#region Utility

		#if UNITY_EDITOR

		/// <summary>
		/// Adds the specified hidden subAssets to the mainAsset
		/// </summary>
		public static void AddSubAssets (ScriptableObject[] subAssets, ScriptableObject mainAsset) 
		{
			for (int i = 0; i < subAssets.Length; i++) {
				AddSubAsset (subAssets [i], mainAsset); 
			}
		}

		/// <summary>
		/// Adds the specified hidden subAsset to the mainAsset
		/// </summary>
		public static void AddSubAsset (ScriptableObject subAsset, ScriptableObject mainAsset) 
		{
			if (subAsset != null && mainAsset != null)
			{
				UnityEditor.AssetDatabase.AddObjectToAsset (subAsset, mainAsset);
				subAsset.hideFlags = HideFlags.HideInHierarchy;
			}
		}

		/// <summary>
		/// Adds the specified hidden subAsset to the mainAsset at path
		/// </summary>
		public static void AddSubAsset (ScriptableObject subAsset, string path) 
		{
			if (subAsset != null && !string.IsNullOrEmpty (path))
			{
				UnityEditor.AssetDatabase.AddObjectToAsset (subAsset, path);
				subAsset.hideFlags = HideFlags.HideInHierarchy;
			}
		}

		#endif

		/// <summary>
		/// Applies a general process on the canvas for loading/saving operations
		/// </summary>
		private static void ProcessCanvas (ref NodeCanvas canvas, bool workingCopy) 
		{
			//Uncompress (ref canvas);
			if (workingCopy)
				canvas = CreateWorkingCopy (canvas, true);
			else
				canvas.Validate (true);
		}

		#endregion

		#endregion

		#region Working Copy

		/// <summary>
		/// Creates a working copy of the specified nodeCanvas, and optionally also of it's associated editorStates.
		/// This breaks the link of this object to any stored assets and references. That means, that all changes to this object will have to be explicitly saved.
		/// </summary>
		public static NodeCanvas CreateWorkingCopy (NodeCanvas nodeCanvas, bool editorStates) 
		{
			nodeCanvas.Validate (true);
			nodeCanvas = Clone (nodeCanvas);

			// Take each SO, make a clone of it and store both versions in the respective list
			// This will only iterate over the 'source instances'
			allSOs.Clear ();
			clonedSOs.Clear ();
			for (int nodeCnt = 0; nodeCnt < nodeCanvas.nodes.Count; nodeCnt++) 
			{
				Node node = nodeCanvas.nodes[nodeCnt];
				node.CheckNodeKnobMigration ();

				// Clone Node and additional scriptableObjects
				Node clonedNode = AddClonedSO (allSOs, clonedSOs, node);
				AddClonedSOs (allSOs, clonedSOs, clonedNode.GetScriptableObjects ());

				for (int i = 0; i < clonedNode.nodeKnobs.Count; i++)
				{ // Clone NodeKnobs and additional scriptableObjects
					AddClonedSO (allSOs, clonedSOs, clonedNode.nodeKnobs[i]);
					AddClonedSOs (allSOs, clonedSOs, clonedNode.nodeKnobs[i].GetScriptableObjects ());
				}
			}

			// Replace every reference to any of the initial SOs of the first list with the respective clones of the second list
			for (int nodeCnt = 0; nodeCnt < nodeCanvas.nodes.Count; nodeCnt++) 
			{ // Clone Nodes, structural content and additional scriptableObjects
				Node node = nodeCanvas.nodes[nodeCnt];
				// Replace node and additional ScriptableObjects
				Node clonedNode = nodeCanvas.nodes[nodeCnt] = ReplaceSO (allSOs, clonedSOs, node);
				clonedNode.CopyScriptableObjects ((ScriptableObject so) => ReplaceSO (allSOs, clonedSOs, so));

				// We're going to restore these from NodeKnobs, no need to Replace muliple times
				clonedNode.Inputs = new List<NodeInput> ();
				clonedNode.Outputs = new List<NodeOutput> ();
				for (int knobCnt = 0; knobCnt < clonedNode.nodeKnobs.Count; knobCnt++) 
				{ // Clone generic NodeKnobs
					NodeKnob knob = clonedNode.nodeKnobs[knobCnt] = ReplaceSO (allSOs, clonedSOs, clonedNode.nodeKnobs[knobCnt]);
					knob.body = clonedNode;
					// Replace additional scriptableObjects in the NodeKnob
					knob.CopyScriptableObjects ((ScriptableObject so) => ReplaceSO (allSOs, clonedSOs, so));
					// Add it into Inputs/Outputs again
					if (knob is NodeInput)
						clonedNode.Inputs.Add (knob as NodeInput);
					else if (knob is NodeOutput) 
						clonedNode.Outputs.Add (knob as NodeOutput);
				}
			}

			if (editorStates)
			{
				nodeCanvas.editorStates = CreateWorkingCopy (nodeCanvas.editorStates, nodeCanvas);
				for (int i = 0; i < nodeCanvas.editorStates.Length; i++) {
					nodeCanvas.editorStates[i].selectedNode = ReplaceSO (allSOs, clonedSOs, nodeCanvas.editorStates[i].selectedNode);
				}
			}
			else
			{
				for (int i = 0; i < nodeCanvas.editorStates.Length; i++) {
					nodeCanvas.editorStates[i].selectedNode = null;
				}
			}

			return nodeCanvas;
		}

		/// <summary>
		/// Creates a working copy of the specified editorStates. Also remains the link of the canvas to these associated editorStates.
		/// This breaks the link of this object to any stored assets and references. That means, that all changes to this object will have to be explicitly saved.
		/// </summary>
		private static NodeEditorState[] CreateWorkingCopy (NodeEditorState[] editorStates, NodeCanvas associatedNodeCanvas) 
		{
			if (editorStates == null)
				return new NodeEditorState[0];
			editorStates = (NodeEditorState[])editorStates.Clone ();
			for (int stateCnt = 0; stateCnt < editorStates.Length; stateCnt++) 
			{
				if (editorStates[stateCnt] == null)
					continue;
				NodeEditorState state = editorStates[stateCnt] = Clone (editorStates[stateCnt]);
				if (state == null) 
				{
					Debug.LogError ("Failed to create a working copy for an NodeEditorState during the loading process of " + associatedNodeCanvas.name + "!");
					continue;
				}
				state.canvas = associatedNodeCanvas;
			}
			associatedNodeCanvas.editorStates = editorStates;
			return editorStates;
		}

		#region Utility

		/// <summary>
		/// Clones the specified SO, preserving its name
		/// </summary>
		private static T Clone<T> (T SO) where T : ScriptableObject 
		{
			string soName = SO.name;
			SO = Object.Instantiate<T> (SO);
			SO.name = soName;
			return SO;
		}

		/// <summary>
		/// Clones SO and writes both the initial and cloned versions into the respective list
		/// </summary>
		private static void AddClonedSOs (List<ScriptableObject> scriptableObjects, List<ScriptableObject> clonedScriptableObjects, ScriptableObject[] initialSOs)
		{
			scriptableObjects.AddRange (initialSOs);
			clonedScriptableObjects.AddRange (initialSOs.Select ((ScriptableObject so) => Clone (so)));
		}

		/// <summary>
		/// Clones SO and writes both the initial and cloned versions into the respective list
		/// </summary>
		private static T AddClonedSO<T> (List<ScriptableObject> scriptableObjects, List<ScriptableObject> clonedScriptableObjects, T initialSO) where T : ScriptableObject 
		{
			if (initialSO == null)
				return null;
			scriptableObjects.Add (initialSO);
			T clonedSO = Clone (initialSO);
			clonedScriptableObjects.Add (clonedSO);
			return clonedSO;
		}

		/// <summary>
		/// First two parameters contains SOs and their respective clones. 
		/// Returns the clone of initialSO found in the cloned list at the respective position of initialSO in the initial list
		/// </summary>
		private static T ReplaceSO<T> (List<ScriptableObject> scriptableObjects, List<ScriptableObject> clonedScriptableObjects, T initialSO) where T : ScriptableObject 
		{
			if (initialSO == null)
				return null;
			int soInd = scriptableObjects.IndexOf (initialSO);
			if (soInd == -1)
				Debug.LogError ("GetWorkingCopy: ScriptableObject " + initialSO.name + " was not copied before! It will be null!");
			return soInd == -1? null : (T)clonedScriptableObjects[soInd];
		}

		#endregion

		#endregion

		#region Utility

		/// <summary>
		/// Extracts the state with the specified name out of the canvas, takes a random different one and renames it or creates a new one with that name if not found
		/// </summary>
		public static NodeEditorState ExtractEditorState (NodeCanvas canvas, string stateName) 
		{
			NodeEditorState state = null;
			if (canvas.editorStates.Length > 0)
			{
				state = canvas.editorStates.First ((NodeEditorState s) => s.name == stateName);
				if (state == null)
					state = canvas.editorStates[0];
			}
			if (state == null)
			{
				state = ScriptableObject.CreateInstance<NodeEditorState> ();
				state.canvas = canvas;
				canvas.editorStates = new NodeEditorState[] { state };
			#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty (canvas);
			#endif
			}
			state.name = stateName;
			return state;
		}

		/// <summary>
		/// Overwrites canvas with canvasData, so that all references to canvas will be remained, but both canvases are still seperate.
		/// Only works in the editor!
		/// </summary>
		public static void OverwriteCanvas (ref NodeCanvas canvas, NodeCanvas canvasData)
		{
		#if UNITY_EDITOR
			if (canvasData == null)
				throw new System.ArgumentNullException ("Cannot overwrite canvas as data is null!");
			if (canvas == null)
				canvas = ScriptableObject.CreateInstance(canvasData.GetType ()) as NodeCanvas;
			UnityEditor.EditorUtility.CopySerialized (canvasData, canvas);
			canvas.name = canvasData.name;
		#else
			throw new System.NotSupportedException ("Cannot overwrite canvas in player!");
		#endif
		}

		#endregion
	}
}