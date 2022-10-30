using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

using Broccoli.Model;

namespace Broccoli.Utils
{
	/// <summary>
	/// Class to save and load ScriptableObjects to files.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EditorPersistence<T> where T: ScriptableObject, IClonable<T> {
		#region Vars
		/// <summary>
		/// Element name to fill all the GUI elements.
		/// </summary>
		public string elementName = "Element";
		/// <summary>
		/// Path to save folder.
		/// </summary>
		public string savePath = "";
		/// <summary>
		/// Default name for the save file.
		/// </summary>
		public string saveFileDefaultName = "element";
		/// <summary>
		/// Save file extension.
		/// </summary>
		public string saveFileExtension = "asset";
		/// <summary>
		/// Enables saving a current file.
		/// </summary>
		public bool saveCurrentFileEnabled = true;
		/// <summray>
		/// OnCreateNew delegate definition.
		/// </summary>
		public delegate void OnCreateNew ();
		/// <summray>
		/// OnCreateNew multidelegate.
		/// </summary>
		public OnCreateNew onCreateNew;
		/// <summary>
		/// OnLoad delegate definition.
		/// </summary>
		/// <param name="element">Scriptable object loaded.</param>
		public delegate void OnLoad (T element, string pathToFile);
		/// <summary>
		/// OnLoad multidelegate.
		/// </summary>
		public OnLoad onLoad;
		/// <summray>
		/// OnGetElementToSave delegate definition.
		/// </summary>
		public delegate T OnGetElementToSave ();
		/// <summary>
		/// OnGetElementToSave multidelegate.
		/// </summary>
		public OnGetElementToSave onGetElementToSave;
		/// <summary>
		/// OnGetElementToSaveFilePath delegate definition.
		/// </summary>
		/// <returns>File to path.</returns>
		public delegate string OnGetElementToSaveFilePath ();
		/// <summary>
		/// OnGetElementToSaveFilePath delegate definition.
		/// </summary>
		public OnGetElementToSaveFilePath onGetElementToSaveFilePath;
		/// <summary>
		/// OnSaveElement delegate definition.
		/// </summary>
		/// <param name="element">Element saved.</param>
		/// <param name="pathToFile">Path to file.</param>
		public delegate void OnSaveElement (T element, string pathToFile);
		/// <summary>
		/// OnSaveElement multidelegate.
		/// </summary>
		public OnSaveElement onSaveElement;
		public bool showCreateNewEnabled = true;
		public bool showSaveCurrentEnabled = true;
		public bool showLoadEnabled = true;
		public bool showSaveEnabled = true;
		#endregion

		#region GUI Vars
		/// <summary>
		/// Button to create a new object to persist.
		/// </summary>
		Button createNewButton;
		/// <summary>
		/// Button to load an object from a ScriptableObject file.
		/// </summary>
		Button loadButton;
		/// <summary>
		/// Button to save an object as a new ScriptableObject file.
		/// </summary>
		Button saveAsButton;
		/// <summary>
		/// Button to save an object to its assigned ScriptableObject file.
		/// </summary>
		Button saveButton;
		#endregion

		#region Messages
		public string btnNewElement = "Create New {0}";
		public string btnNewElementHelp = "Creates a new {0} to work with.";
		public string dialogNewElementTitle = "New {0}";
		public string dialogNewElementMsg = "By creating a new {0} you will lose any changes " +
			"not saved on the current one. Do you want to continue creating a new {0}?";
		public string dialogNewElementBtnOk = "Yes, create a new {0}";
		public string dialogElementBtnCancel = "No";
		public string btnLoadElement = "Load {0} from File";
		public string btnLoadElementHelp = "Loads a {0} from a file.";
		public string dialogLoadElementTitle = "Load {0}";
		public string dialogLoadElementError = "Failed to load {0}: The file at the specified path " +
			"is not a valid save file or does not contain {0} data.";
		public string btnSaveAsNewElement = "Save as New {0}";
		public string btnSaveAsNewElementHelp = "Saves the current {0} to a new file.";
		public string dialogSaveAsNewElementTitle = "Save as New {0}";
		public string btnSaveCurrentElement = "Save {0}";
		#endregion

		#region Events
		/// <summary>
		/// Initializes all the message string using the set element name.
		/// </summary>
		public void InitMessages () {
			// Create new element.
			btnNewElement = string.Format (btnNewElement, elementName);
			btnNewElementHelp = string.Format (btnNewElementHelp, elementName);
			dialogNewElementTitle = string.Format (dialogNewElementTitle, elementName);
			dialogNewElementMsg = string.Format (dialogNewElementMsg, elementName);
			dialogNewElementBtnOk = string.Format (dialogNewElementBtnOk, elementName);
			// Load from file.
			btnLoadElement = string.Format (btnLoadElement, elementName);
			btnLoadElementHelp = string.Format (btnLoadElementHelp, elementName);
			dialogLoadElementTitle = string.Format (dialogLoadElementTitle, elementName);
			dialogLoadElementError = string.Format (dialogLoadElementError, elementName);
			// Save as new element.
			btnSaveAsNewElement = string.Format (btnSaveAsNewElement, elementName);
			btnSaveAsNewElementHelp = string.Format (btnSaveAsNewElementHelp, elementName);
			dialogSaveAsNewElementTitle = string.Format (dialogSaveAsNewElementTitle, elementName);
			// Save current element.
			btnSaveCurrentElement = string.Format (btnSaveCurrentElement, elementName);
		}
		#endregion

		#region Draw
		public void SetupOptions (
			Button createNewButton,
			Button loadFromButton,
			Button saveAsNewButton,
			Button saveButton)
		{
			this.createNewButton = createNewButton;
			this.createNewButton.onClick.AddListener (CreateNewButtonClicked);
			this.loadButton = loadFromButton;
			this.loadButton.onClick.AddListener (LoadButtonClicked);
			this.saveAsButton = saveAsNewButton;
			this.saveAsButton.onClick.AddListener (SaveAsButtonClicked);
			this.saveButton = saveButton;
			this.saveButton.onClick.AddListener (SaveButtonClicked);
		}
		/// <summary>
		/// Draw the persistence options to the GUI.
		/// </summary>
		public void DrawOptions () {
			// Create new element.
			if (showCreateNewEnabled) {
				if (GUILayout.Button (new GUIContent (btnNewElement, btnNewElementHelp))) {
					CreateNewButtonClicked ();
				}
			}
			// Load from file.
			if (showLoadEnabled) {
				if (GUILayout.Button (new GUIContent (btnLoadElement, btnLoadElementHelp))) {
					LoadButtonClicked ();
				}
			}
			// Save as new to file.
			if (showSaveEnabled) {
				if (GUILayout.Button (new GUIContent (btnSaveAsNewElement, btnSaveAsNewElementHelp))) {
					SaveAsButtonClicked ();
				}
			}
			if (showSaveCurrentEnabled) {
				// Save current to file.
				EditorGUI.BeginDisabledGroup (!saveCurrentFileEnabled);
				if (GUILayout.Button (new GUIContent (btnSaveCurrentElement))) {
					SaveButtonClicked ();
				}
				EditorGUI.EndDisabledGroup ();
			}
		}
		/// <summary>
		/// Create new element button click event.
		/// </summary>
		void CreateNewButtonClicked () {
			if (EditorUtility.DisplayDialog (dialogNewElementTitle, 
					dialogNewElementMsg, dialogNewElementBtnOk, dialogElementBtnCancel)) {
				if (onCreateNew != null)
					onCreateNew ();
			}
			Event.current.Use ();
			GUIUtility.ExitGUI();
			return;
		}
		/// <summary>
		/// Load element button click event.
		/// </summary>
		void LoadButtonClicked () {
			string pathToFile = EditorUtility.OpenFilePanel (dialogLoadElementTitle, savePath, saveFileExtension);
			if (!string.IsNullOrEmpty (pathToFile)) {
				T element = LoadElementFromFile (pathToFile);
				if (element != null && onLoad != null)
					onLoad (element, pathToFile);
			}
			Event.current.Use ();
			GUIUtility.ExitGUI();
			return;
		}
		/// <summary>
		/// Save as new element button click event.
		/// </summary>
		void SaveAsButtonClicked () {
			if (onGetElementToSave != null) {
				string pathToFile = EditorUtility.SaveFilePanelInProject (dialogSaveAsNewElementTitle, 
					saveFileDefaultName, saveFileExtension, "", savePath);
				T elementToSave  = onGetElementToSave ();
				if (elementToSave != null) {
					if (SaveElementToFile (elementToSave, pathToFile)) {
						if (onSaveElement != null) {
							onSaveElement (elementToSave, pathToFile);
						}
					}
				} else {
					Debug.LogWarning ("Element to save cannot be null.");	
				}
			} else {
				Debug.LogWarning ("No OnGetElementToSave set. Could not get the object to save to a file.");
			}
			Event.current.Use ();
			GUIUtility.ExitGUI();
		}
		/// <summary>
		/// Save as element button click event.
		/// </summary>
		void SaveButtonClicked () {
			if (onGetElementToSave != null && onGetElementToSaveFilePath != null) {
				string pathToFile = onGetElementToSaveFilePath ();
				T elementToSave  = onGetElementToSave ();
				if (elementToSave != null && !string.IsNullOrEmpty (pathToFile)) {
					if (SaveElementToFile (elementToSave, pathToFile)) {
						if (onSaveElement != null) {
							onSaveElement (elementToSave, pathToFile);
						}
					}
				} else {
					Debug.LogWarning ("Element to save or path cannot be null.");	
				}
			} else {
				Debug.LogWarning ("No OnGetElementToSave or OnGetElementToSaveFilePath set. " +
					"Could not get the object or path to save to a file.");
			}
		}
		#endregion

		#region Persistence Ops
		/// <summary>
		/// Gets a save path from the file dialog. Should call GUIUtility.ExitGUI on the calling method.
		/// </summary>
		/// <returns>File path.</returns>
		public string GetSavePath () {
			string pathToFile = EditorUtility.SaveFilePanelInProject (dialogSaveAsNewElementTitle, 
				saveFileDefaultName, saveFileExtension, "", savePath);
			Event.current.Use ();
			return pathToFile;
		}
		/// <summary>
		/// Attemps to load an element from a file.
		/// </summary>
		/// <param name="pathToFile">Path to file.</param>
		/// <returns>Loaded element.If loading fails an exception is raised.</returns>
		public T LoadElementFromFile (string pathToFile) {
			pathToFile = pathToFile.Replace(Application.dataPath, "Assets");
			AssetDatabase.Refresh ();
			T loadedElement = AssetDatabase.LoadAssetAtPath<T> (pathToFile);
			if (loadedElement == null) {
				throw new UnityException (dialogLoadElementError + " Path: " + pathToFile);
			}
			T clonedElement = loadedElement.Clone ();
			Resources.UnloadAsset (loadedElement);
			return clonedElement;
		}
		/// <summary>
		/// Save element to file.
		/// </summary>
		/// <param name="elementToSave">Element to save.</param>
		/// <param name="pathToFile">Path to file.</param>
		/// <returns><c>True</c> if saving is successful.</returns>
		public bool SaveElementToFile (T elementToSave, string pathToFile) {
			if (!string.IsNullOrEmpty (pathToFile)) {
				try {
					pathToFile = pathToFile.Replace(Application.dataPath, "Assets");
					T loadedElement = AssetDatabase.LoadAssetAtPath<T> (pathToFile);
					if (loadedElement == null) {
						loadedElement = elementToSave.Clone ();
						AssetDatabase.CreateAsset (loadedElement, pathToFile);
					} else {
						EditorUtility.CopySerialized (elementToSave.Clone (), loadedElement);
					}
					AssetDatabase.SaveAssets ();
					//Resources.UnloadAsset (loadedElement);
					return true;
				} catch (UnityException e) {
					Debug.LogException (e);
				}
			}
			return false;
		}
		#endregion
	}
}
