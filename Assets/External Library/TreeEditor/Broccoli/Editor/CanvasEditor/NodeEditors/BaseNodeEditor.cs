using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Factory;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Base class for node editors.
	/// </summary>
	public abstract class BaseNodeEditor : Editor {
		#region Events
		/// <summary>
		/// The base node of this editor.
		/// </summary>
		protected BaseNode baseNode = null;
		/// <summary>
		/// True if the node editor has been initialized.
		/// </summary>
		protected bool isInit = false;
		/// <summary>
		/// The serialized property.
		/// </summary>
		protected SerializedProperty serializedProperty;
		/// <summary>
		/// The serialized pipeline element.
		/// </summary>
		protected SerializedObject serializedPipelineElement;
		/// <summary>
		/// The seconds to wait to update the pipeline.
		/// </summary>
		private float secondsToUpdatePipeline = 0;
		/// <summary>
		/// The editor delta time.
		/// </summary>
		double editorDeltaTime = 0f;
		/// <summary>
		/// The last time since startup.
		/// </summary>
		double lastTimeSinceStartup = 0f;
		/// <summary>
		/// True if the waiting is for the UpdatePipeline method.
		/// </summary>
		bool waitingToUpdatePipeline = false;
		/// <summary>
		/// True if the waiting is for the UpdatePipelineUpstream method.
		/// </summary>
		bool waitingToUpdatePipelineFromUpstream = false;
		/// <summary>
		/// True if the waiting is for the UpdateComponent method.
		/// </summary>
		bool waitingToUpdateComponent = false;
		/// <summary>
		/// The waiting command passed to the update pipeline method.
		/// </summary>
		int waitingCmd = 0;
		/// <summary>
		/// The waiting class type passed to the update pipeline method.
		/// </summary>
		PipelineElement.ClassType waitingClassType;
		/// <summary>
		/// Flag to rebuild the preview tree from anew.
		/// </summary>
		public bool rebuildTreePreview = false;
		/// <summary>
		/// The show field help flag.
		/// </summary>
		public bool showFieldHelp = false;
		/// <summary>
		/// The current undo group to check for undoable actions.
		/// </summary>
		public int currentUndoGroup = 0;
		#endregion

		#region Events
		/// <summary>
		/// Init the specified baseNode.
		/// </summary>
		/// <param name="baseNode">Base node.</param>
		void Init(BaseNode baseNode) {
			this.baseNode = baseNode;
			isInit = true;
		}
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		void OnEnable () {
			Init (target as BaseNode);
			OnEnableSpecific ();
			EditorApplication.update += OnEditorUpdate;
			#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += OnSceneGUI;
			#else
			SceneView.onSceneGUIDelegate += OnSceneGUI;
			#endif
		}
		/// <summary>
		/// Raises the disable event.
		/// </summary>
		void OnDisable () {
			EditorApplication.update -= OnEditorUpdate;
			#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= OnSceneGUI;
			#else
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			#endif
			OnDisableSpecific ();
		}
		/// <summary>
		/// Raises the scene GUI event.
		/// </summary>
		/// <param name="sceneView">Scene view.</param>
		protected virtual void OnSceneGUI (SceneView sceneView)
		{
			// your OnSceneGUI stuffs here
		}
		/// <summary>
		/// Raises the editor update event.
		/// </summary>
		void OnEditorUpdate () {
			if (secondsToUpdatePipeline > 0) {
				SetEditorDeltaTime ();
				secondsToUpdatePipeline -= (float) editorDeltaTime;
				if (secondsToUpdatePipeline < 0) {
					if (waitingToUpdatePipeline) {
						UpdatePipeline ();
					} else if (waitingToUpdatePipelineFromUpstream) {
						UpdatePipelineUpstream (waitingClassType);
					} else if (waitingToUpdateComponent) {
						UpdateComponent (waitingCmd);
					}
					secondsToUpdatePipeline = 0;
					waitingToUpdatePipeline = false;
					waitingToUpdatePipelineFromUpstream = false;
					waitingToUpdateComponent = false;
				}
			}
		}
		/// <summary>
		/// Sets the editor delta time.
		/// </summary>
		private void SetEditorDeltaTime ()
		{
			if (lastTimeSinceStartup == 0f)
			{
				lastTimeSinceStartup = EditorApplication.timeSinceStartup;
			}
			editorDeltaTime = EditorApplication.timeSinceStartup - lastTimeSinceStartup;
			lastTimeSinceStartup = EditorApplication.timeSinceStartup;
		}
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected abstract void OnEnableSpecific ();
		/// <summary>
		/// Raises the disable specific event.
		/// </summary>
		protected virtual void OnDisableSpecific () {}
		#endregion

		#region Pipeline Update
		/// <summary>
		/// Updates the pipeline.
		/// </summary>
		/// <param name="secondsToWait">Seconds to wait before calling the pipeline to update.</param>
		/// <param name="rebuild">If set to <c>true</c> the whole preview tree gets rebuild from anew.</param>
		protected void UpdatePipeline (float secondsToWait = 0, bool rebuild = false) {
			if (rebuild) {
				this.rebuildTreePreview = true;
			}
			if (secondsToWait > 0) {
				this.secondsToUpdatePipeline = secondsToWait;
				waitingToUpdatePipeline = true;
				SetEditorDeltaTime ();
			} else {
				if (baseNode.pipelineElement != null && baseNode.pipelineElement.isOnValidPipeline) {
					if (rebuildTreePreview) {
						TreeFactory.GetActiveInstance ().ProcessPipelinePreview (null, true, true);
					} else {
						TreeFactory.GetActiveInstance ().ProcessPipelinePreviewDownstream (baseNode.pipelineElement, true);
					}
				}
				this.secondsToUpdatePipeline = 0;
				this.rebuildTreePreview = false;
			}
		}
		/// <summary>
		/// Updates the pipeline from an upstream element if it is found.
		/// </summary>
		/// <param name="classType">Class type for the upstream pipeline element.</param>
		/// <param name="secondsToWait">Seconds to wait.</param>
		protected void UpdatePipelineUpstream (PipelineElement.ClassType classType, float secondsToWait = 0) {
			if (secondsToWait > 0) {
				this.secondsToUpdatePipeline = secondsToWait;
				waitingToUpdatePipelineFromUpstream = true;
				waitingClassType = classType;
				SetEditorDeltaTime ();
			} else {
				if (baseNode.pipelineElement != null && baseNode.pipelineElement.isOnValidPipeline) {
					TreeFactory.GetActiveInstance ().ProcessPipelinePreviewFromUpstream (baseNode.pipelineElement, classType, true);
				}
				this.secondsToUpdatePipeline = 0;
			}
		}
		/// <summary>
		/// Updates the pipeline with only one component.
		/// </summary>
		/// <param name="cmd">Command passed to the component.</param>
		/// <param name="secondsToWait">Seconds to wait.</param>
		protected void UpdateComponent (int cmd, float secondsToWait = 0) {
			if (secondsToWait > 0) {
				this.secondsToUpdatePipeline = secondsToWait;
				waitingToUpdateComponent = true;
				waitingCmd = cmd;
				SetEditorDeltaTime ();
			} else {
				if (baseNode.pipelineElement != null && baseNode.pipelineElement.isOnValidPipeline) {
					TreeFactory.GetActiveInstance ().ProcessPipelineComponent (baseNode.pipelineElement, cmd);
				}
				this.secondsToUpdatePipeline = 0;
			}
		}
		#endregion

		#region GUI and Serialization
		/// <summary>
		/// Sets the pipeline element to edit on the node.
		/// </summary>
		/// <param name="propertyName">Property name for the pipeline element on the node.</param>
		protected void SetPipelineElementProperty (string propertyName) {
			serializedProperty = serializedObject.FindProperty (propertyName);
			if (serializedProperty != null) {
				serializedPipelineElement = new SerializedObject (serializedProperty.objectReferenceValue);
			}
		}
		/// <summary>
		/// Gets the serialized property for an editable var on the pipeline element.
		/// </summary>
		/// <returns>The serialized property.</returns>
		/// <param name="propertyName">Property name on the pipeline.</param>
		protected SerializedProperty GetSerializedProperty (string propertyName) {
			return serializedPipelineElement.FindProperty (propertyName);
		}
		/// <summary>
		/// Updates the serialized object.
		/// </summary>
		protected void UpdateSerialized() {
			if (serializedObject != null) {
				serializedObject.Update ();
			}
			if (serializedPipelineElement != null) {
				serializedPipelineElement.Update ();
			}
		}
		/// <summary>
		/// Applies any pending changes to the serialized object.
		/// </summary>
		protected void ApplySerialized () {
			serializedObject.ApplyModifiedProperties ();
			serializedPipelineElement.ApplyModifiedProperties ();
		}
		/// <summary>
		/// Checks if the pipeline has changed due to an undo action.
		/// </summary>
		protected void CheckUndoRequest () {
			if (EventType.ValidateCommand == Event.current.type &&
				"UndoRedoPerformed" == Event.current.commandName) {
				if (TreeFactory.GetActiveInstance ().lastUndoProcessed !=
					baseNode.pipelineElement.pipeline.undoControl.undoCount) {
					OnUndo ();
					TreeFactory.GetActiveInstance ().RequestPipelineUpdate ();
				}
			}
			currentUndoGroup = Undo.GetCurrentGroup ();
		}
		protected virtual void OnUndo () {}
		/// <summary>
		/// Sets the undo control counter.
		/// This counter is collapsed with the latest change to the undo stack,
		/// then when an undo event is called the factory could check for this
		/// counter value to update the tree.
		/// </summary>
		protected void SetUndoControlCounter (bool collapseUndos = true) {
			if (baseNode != null && baseNode.pipelineElement != null && 
				baseNode.pipelineElement.pipeline != null && baseNode.pipelineElement.isOnValidPipeline) {
				Undo.RecordObject (baseNode.pipelineElement.pipeline, "undoControl");
				baseNode.pipelineElement.pipeline.undoControl.undoCount++;
				if (collapseUndos) {
					Undo.CollapseUndoOperations (currentUndoGroup);
				}
				baseNode.pipelineElement.Validate ();
			}
		}
		#endregion

		#region Draw
		/// <summary>
		/// Draws the log box.
		/// </summary>
		protected virtual void DrawLogBox () {
			if (baseNode.pipelineElement.log.Count > 0) {
				var enumerator = baseNode.pipelineElement.log.GetEnumerator ();
				while (enumerator.MoveNext ()) {
					MessageType messageType = UnityEditor.MessageType.Info;
					switch (enumerator.Current.messageType) {
					case LogItem.MessageType.Error:
						messageType = UnityEditor.MessageType.Error;
						break;
					case LogItem.MessageType.Warning:
						messageType = UnityEditor.MessageType.Warning;
						break;
					}
					EditorGUILayout.HelpBox (enumerator.Current.message, messageType);
				}
			}
		}
		/// <summary>
		/// Draws the seed options.
		/// </summary>
		protected virtual void DrawSeedOptions () {
			bool isSeedFixed = EditorGUILayout.Toggle ("Use Fixed Seed", baseNode.pipelineElement.isSeedFixed);
			if (isSeedFixed != baseNode.pipelineElement.isSeedFixed) {
				Undo.RecordObject (baseNode.pipelineElement, "Using fixed seed on " + baseNode.pipelineElement.name + " element changed.");
				baseNode.pipelineElement.isSeedFixed = isSeedFixed;
				NodeEditorFramework.NodeEditor.RepaintClients ();
				SetUndoControlCounter ();
			}
			if (baseNode.pipelineElement.isSeedFixed) {
				int newFixedSeed = EditorGUILayout.IntSlider ("Seed", baseNode.pipelineElement.fixedSeed, 0, 10000);
				if (newFixedSeed != baseNode.pipelineElement.fixedSeed) {
					Undo.RecordObject (baseNode.pipelineElement, "Fixed seed on " + baseNode.pipelineElement.name + " element changed.");
					baseNode.pipelineElement.fixedSeed = newFixedSeed;
					SetUndoControlCounter ();
				}
			}
		}
		/// <summary>
		/// Draws the key name options.
		/// </summary>
		protected virtual void DrawKeyNameOptions () {
			bool useKeyName = EditorGUILayout.Toggle ("Use Key Name.", baseNode.pipelineElement.useKeyName);
			if (useKeyName != baseNode.pipelineElement.useKeyName) {
				Undo.RecordObject (baseNode.pipelineElement, "Using key name.");
				baseNode.pipelineElement.useKeyName = useKeyName;
				NodeEditorFramework.NodeEditor.RepaintClients ();
				SetUndoControlCounter ();
			}
			if (baseNode.pipelineElement.useKeyName) {
				string newKeyName = baseNode.pipelineElement.keyName;
				newKeyName = EditorGUILayout.TextField ("Key Name", newKeyName);
				if (string.Compare (newKeyName, baseNode.pipelineElement.keyName) != 0) {
					Undo.RecordObject (baseNode.pipelineElement, "New pipeline element key name: " + baseNode.pipelineElement.keyName + ".");
					baseNode.pipelineElement.keyName = newKeyName;
					SetUndoControlCounter ();
				}
			}
		}
		/// <summary>
		/// Draws the field help options.
		/// </summary>
		protected virtual void DrawFieldHelpOptions () {
			bool _showFieldHelp = EditorGUILayout.Toggle ("Show Fields Description", showFieldHelp);
			if (_showFieldHelp != showFieldHelp) {
				showFieldHelp = _showFieldHelp;
				OnShowFieldHelpChanged ();
			}
		}
		/// <summary>
		/// Shows the help box.
		/// </summary>
		/// <param name="msg">Message.</param>
		protected void ShowHelpBox (string msg) {
			if (showFieldHelp)
				EditorGUILayout.HelpBox (msg, MessageType.None);
		}
		/// <summary>
		/// Called when the ShowFieldHelp flag changed.
		/// </summary>
		protected virtual void OnShowFieldHelpChanged () {}
		#endregion

		#region Property Fields
		/// <summary>
		/// Range slider for float min and max value properties.
		/// </summary>
		/// <param name="propMinValue">Property with the minumum value.</param>
		/// <param name="propMaxValue">Property with the maximum value.</param>
		/// <param name="minRangeValue">Minimum possible value in the range.</param>
		/// <param name="maxRangeValue">Maximum possible value in the range.</param>
		/// <param name="label">Label to display on the field.</param>
		/// <returns>True if the range was changed.</returns>
		protected bool FloatRangePropertyField (SerializedProperty propMinValue, SerializedProperty propMaxValue, float minRangeValue, float maxRangeValue, string label) {
			float minValue = propMinValue.floatValue;
			float maxValue = propMaxValue.floatValue;
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.MinMaxSlider (label, ref minValue, ref maxValue, minRangeValue, maxRangeValue);
			EditorGUILayout.LabelField (minValue.ToString("F2") + "/" + maxValue.ToString("F2"), GUILayout.Width (72));
			EditorGUILayout.EndHorizontal ();
			if (minValue != propMinValue.floatValue || maxValue != propMaxValue.floatValue) {
				propMinValue.floatValue = minValue;
				propMaxValue.floatValue = maxValue;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Range slider for float min and max value properties.
		/// </summary>
		/// <param name="propMinValue">Property with the minumum value.</param>
		/// <param name="propMaxValue">Property with the maximum value.</param>
		/// <param name="minRangeValue">Minimum possible value in the range.</param>
		/// <param name="maxRangeValue">Maximum possible value in the range.</param>
		/// <param name="label">Label to display on the field.</param>
		/// <returns>True if the range was changed.</returns>
		protected bool IntRangePropertyField (SerializedProperty propMinValue, SerializedProperty propMaxValue, int minRangeValue, int maxRangeValue, string label) {
			float minValue = propMinValue.intValue;
			float maxValue = propMaxValue.intValue;
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.MinMaxSlider (label, ref minValue, ref maxValue, minRangeValue, maxRangeValue);
			EditorGUILayout.LabelField (minValue.ToString("F0") + "-" + maxValue.ToString("F0"), GUILayout.Width (60));
			EditorGUILayout.EndHorizontal ();
			if (Mathf.RoundToInt (minValue) != propMinValue.intValue || Mathf.RoundToInt (maxValue) != propMaxValue.intValue) {
				propMinValue.intValue = Mathf.RoundToInt (minValue);
				propMaxValue.intValue = Mathf.RoundToInt (maxValue);
				return true;
			}
			return false;
		}
		#endregion
	}
}
