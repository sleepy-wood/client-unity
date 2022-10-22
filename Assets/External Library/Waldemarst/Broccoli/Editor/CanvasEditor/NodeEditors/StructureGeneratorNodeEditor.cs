using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Base;
using Broccoli.Utils;
using Broccoli.Pipe;
using Broccoli.Model;
using Broccoli.NodeEditorFramework;
using Broccoli.Generator;
using Broccoli.Component;
using Broccoli.Factory;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Structure generator node editor.
	/// </summary>
	[CustomEditor(typeof(StructureGeneratorNode))]
	public class StructureGeneratorNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The structure generator node.
		/// </summary>
		public StructureGeneratorNode structureGeneratorNode;
		/// <summary>
		/// Component used to commit structure changes.
		/// </summary>
		public StructureGeneratorComponent structureGeneratorComponent;
		/// <summary>
		/// Bezier curve editor to edit branch curves.
		/// </summary>
		/// <returns>Bezier curve editor.</returns>
		public BezierCurveEditor curveEditor;
		/// <summary>
		/// Id of a single selected curve.
		/// </summary>
		private System.Guid _singleSelectedCurveId = System.Guid.Empty;
		/// <summary>
		/// The selected curve required processing.
		/// </summary>
		private bool _singleSelectedCurveProcessRequired = false;
		/// <summary>
		/// The canvas used for the level nodes.
		/// </summary>
		StructureCanvas structureCanvas;
		/// <summary>
		/// The structure canvas cache.
		/// </summary>
		NodeEditorUserCache structureCanvasCache;
		SerializedProperty propRootStructureLevel;
		/// <summary>
		/// The property minimum frequency.
		/// </summary>
		SerializedProperty propMinFrequency;
		/// <summary>
		/// The property max frequency.
		/// </summary>
		SerializedProperty propMaxFrequency;
		/// <summary>
		/// The length of the property minimum.
		/// </summary>
		SerializedProperty propMinLength;
		/// <summary>
		/// The length of the property max.
		/// </summary>
		SerializedProperty propMaxLength;
		/// <summary>
		/// The property radius.
		/// </summary>
		SerializedProperty propRadius;
		/// <summary>
		/// The property to override noise.
		/// </summary>
		SerializedProperty propOverrideNoise;
		/// <summary>
		/// The property noise.
		/// </summary>
		SerializedProperty propNoise;
		/// <summary>
		/// The property noise scale.
		/// </summary>
		SerializedProperty propNoiseScale;
		/// <summary>
		/// The property structure levels.
		/// </summary>
		SerializedProperty propStructureLevels;
		/// <summary>
		/// True when the canvas needs reinitialization.
		/// </summary>
		bool reinitCanvas = false;
		/// <summary>
		/// Saves the id of the structure currently being drawn.
		/// </summary>
		private System.Guid _editStructureId = System.Guid.Empty;
		/// <summary>
		/// Structure instance being drawn.
		/// </summary>
		private StructureGenerator.Structure _editStructure = null;
		/// <summary>
		/// Saves the id of the structure level selected.
		/// </summary>
		private int _editStructureLevelId = -1;
		public int selectedStructureLevelId {
			get { return _editStructureLevelId; }
		}
		/// <summary>
		/// Dictionary to save the id of a node and the curve (branch) it belongs to.
		/// </summary>
		/// <typeparam name="System.Guid">Curve unique id.</typeparam>
		/// <typeparam name="int">Id of the curve.</typeparam>
		/// <returns></returns>
		private Dictionary<System.Guid, System.Guid> _nodeToCurve = new Dictionary<System.Guid, System.Guid> ();
		/// <summary>
		/// List of selected nodes.
		/// </summary>
		/// <typeparam name="BezierNode">Node of bezier curves.</typeparam>
		/// <returns></returns>
		private List<BezierNode> _selectedNodes = new List<BezierNode> ();
		private List<int> _selectedIndexes = new List<int> ();
		/// <summary>
		/// Id of selected curves.
		/// </summary>
		/// <typeparam name="System.Guid">Id of curve.</typeparam>
		/// <returns></returns>
		private List<System.Guid> _selectedCurveIds = new List<System.Guid> ();
		private List<System.Guid> _tunedBranchIds = new List<System.Guid> ();
		/// <summary>
		/// Color of bezier curves.
		/// </summary>
		/// <returns>Color.</returns>
		Color curveColor = new Color (1, 0.372f, 0.058f);
		/// <summary>
		/// Color of selected bezier curves.
		/// </summary>
		/// <returns>Color.</returns>
		Color selectedCurveColor = new Color (1, 0.239f, 0.058f);
		/// <summary>
		/// Selected toolbar to edit structure level properties.
		/// </summary>
		int selectedPanel = 0;
		/// <summary>
		/// Range mask to apply curves from 0,0 to 1,1 values.
		/// </summary>
		private static Rect scaleCurveRange = new Rect (0f, 0f, 1f, 1f);
		#endregion

		#region GUI Contents and Labels
		private static GUIContent[] aspectsTrunkPanelOptions = new GUIContent[] {
			new GUIContent ("Structure", "Options to control the frequency, position, twirl, length and girth of structures."), 
			new GUIContent ("Advanced", "Other options to control per structure generator, like overriding noise.")};
		private static GUIContent[] aspectsSproutPanelOptions = new GUIContent[] {
			new GUIContent ("Structure", "Options to control the frequency, position, twirl, length and girth of structures."), 
			new GUIContent ("Alignment", "Options to control the direction and orientation of structures."), 
			new GUIContent ("Range", "Options to control the spawning range of the structures.")};
		private static GUIContent[] aspectsFullPanelOptions = new GUIContent[] {
			new GUIContent ("Structure", "Options to control the frequency, position, twirl, length and girth of structures."), 
			new GUIContent ("Alignment", "Options to control the direction and orientation of structures."), 
			new GUIContent ("Range", "Options to control the spawning range of the structures."),
			new GUIContent ("Advanced", "Other options to control per structure generator, like overriding noise.")};
		#endregion

		#region Constants
		public const int PANEL_STRUCTURE = 0;
		public const int PANEL_ALIGNMENT = 1;
		public const int PANEL_RANGE = 2;
		public const int PANEL_ADVANCED = 3;
		#endregion

		#region Messages
		private static string MSG_ENABLED = "Enables/disabled this structure level element generation. " +
			"If disabled then all the downstream levels are disabled as well.";
		private static string MSG_MAIN_MIN_MAX_FREQUENCY = "Number of possible branches to produce.";
		private static string MSG_MAIN_MIN_MAX_LENGTH = "Length range for each produced branch.";
		private static string MSG_MAIN_RADIUS = "Radius for the circular area where the branches will spawn.";
		private static string MSG_OVERRIDE_NOISE = "Overrides global noise parameters for branch structures generated by this node.";
		private static string MSG_NOISE = "Override noise value for this structure generator.";
		private static string MSG_NOISE_SCALE = "Override noise scale value for this structure generator.";
		private static string MSG_SPROUT_GROUP = "Selects the sprout group applied to the generated sprouts. " +
			"Sprouts must belong to a sprout group in order to be meshed.";
		private static string MSG_MIN_FREQUENCY = "Minimum number of possible elements to produce.";
		private static string MSG_MAX_FREQUENCY = "Maximum number of possible elements to produce.";
		private static string MSG_PROBABILITY = "Probability of occurrence for this level.";
		private static string MSG_SHARED_PROBABILITY = "Probability to be chosen from a group of shared levels.";
		private static string MSG_DISTRIBUTION_MODE = "Distribution mode to place te elements along the parent branch.";
		private static string MSG_DISTRIBUTION_WHORLED = "Number of elements per node on the parent branch.";
		private static string MSG_DISTRIBUTION_SPACING_VARIANCE = "Adds spacing variance between branches along the parent branch.";
		private static string MSG_DISTRIBUTION_ANGLE_VARIANCE = "Add angle variance between branches along the parent branch.";
		private static string MSG_DISTRIBUTION_CURVE = "Curve of distribution for the nodes of elements along the parent branch. " +
			"From the base of the branch to the tip.";
		private static string MSG_RANDOM_TWIRL_OFFSET_ENABLED = "";
		private static string MSG_TWIRL_OFFSET = "";
		private static string MSG_TWIRL = "Rotation angle on the spawned elements taking the parent branch direction as axis.";
		private static string MSG_PARALLEL_ALIGN_AT_TOP = "Value of direction alignment for spawned element following " +
			"their parent branch direction at the top end of it.";
		private static string MSG_PARALLEL_ALIGN_AT_BASE = "Value of direction alignment for spawned element following " +
			"their parent branch direction at the base end of it.";
		private static string MSG_PARALLEL_ALIGN_CURVE = "Parallel alignment distribution curve from base to top of the parent branch.";
		private static string MSG_GRAVITY_ALIGN_AT_TOP = "Value of direction alignment for spawned element against " +
			"gravity at the top end of the parent branch.";
		private static string MSG_GRAVITY_ALIGN_AT_BASE = "Value of direction alignment for spawned element against " +
			"gravity at the base end of the parent branch.";
		private static string MSG_GRAVITY_ALIGN_CURVE = "Gravity alignment distribution curve from base to top of the parent branch.";
		private static string MSG_HORIZONTAL_ALIGN_AT_TOP = "Value of direction alignment for spawned elements " + 
			"to the horizontal plane at the top end of the parent branch.";
		private static string MSG_HORIZONTAL_ALIGN_AT_BASE = "Value of direction alignment for spawned elements " + 
			"to the horizontal plane at the base end of the parent branch.";
		private static string MSG_HORIZONTAL_ALIGN_CURVE = "Horizontal alignment distribution curve from base to top of the parent branch.";
		private static string MSG_LENGTH_AT_TOP = "Length value for spawned branches at the top end of the parent branch.";
		private static string MSG_LENGTH_AT_BASE = "Length value for spawned branches at the base end of the parent branch.";
		private static string MSG_LENGTH_CURVE = "Length distribution curve, from base to the top of the parent branch.";
		private static string MSG_GIRTH_SCALE = "Girth scale to apply to generated branches.";
		private static string MSG_RANGE_ENABLED = "If enabled spawned elements will only appear along the specified " +
			"range of their parent branch length.";
		private static string MSG_RANGE = "The total number of structures to generate by this level is distributed along this range (0 = base of the structure, 1 = top of the structure).";
		private static string MSG_MASK_RANGE = "From the total number of structures generated, only those falling within this range are added to their parent structure (0 = base of the structure, 1 = top of the structure).";
		private static string MSG_FROM_BRANCH_CENTER = "If set sprouts origin is at the center of the branch and not its surface.";
		private static string MSG_APPLY_BREAK = "Branches generated have a chance to break at some point, thus not getting meshed after the break point.";
		private static string MSG_BREAK_PROBABILITY = "Probability for branches generated by this structure to break. " +
			"The x axis is the position of the branch at its parent branch (0 at base, 1 at top.). The y axis is the probability to break (0 to 1).";
		private static string MSG_BREAK_RANGE = "Length range for the break point to appear.";
		#endregion

		#region Events
		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		void OnDestroy () {
			DestroyImmediate (structureCanvas);
		}
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			structureGeneratorNode = target as StructureGeneratorNode;

			structureGeneratorNode.structureGeneratorElement.BuildStructureLevelTree ();

			// SETUP CANVAS
			ReinitCanvas ();
			structureCanvas.onSelectNode -= OnSelectStructureLevel;
			structureCanvas.onSelectNode += OnSelectStructureLevel;
			structureCanvas.onDeselectNode -= OnDeselectStructureLevel;
			structureCanvas.onDeselectNode += OnDeselectStructureLevel;

			// SETUP BEZIER CURVE EDITOR
			curveEditor = new BezierCurveEditor ();
			curveEditor.OnEnable ();
			curveEditor.showTools = true;
			/*
			curveEditor.debugEnabled = true;
			curveEditor.debugShowCurvePoints = true;
			curveEditor.debugShowPointForward = true;
			curveEditor.debugShowPointNormal = true;
			curveEditor.debugShowPointUp = true;
			curveEditor.debugShowPointTangent = true;
			*/
			curveEditor.onEditModeChanged += OnEditModeChanged;
			curveEditor.onSelectionChanged += OnNodeSelectionChanged;
			curveEditor.onCheckMoveNodes += OnCheckMoveNodes;

			// Move nodes.
			curveEditor.onBeginMoveNodes += OnBeginMoveNodes;
			curveEditor.onMoveNodes += OnMoveNodes;
			curveEditor.onEndMoveNodes += OnEndMoveNodes;
			curveEditor.onBeforeEditNode += OnBeforeEditNode;
			curveEditor.onEditNode += OnEditNode;

			// Add nodes.
			curveEditor.onBeforeAddNode += OnBeforeAddNode;
			curveEditor.onAddNode += OnAddNode;

			// Remove nodes.
			curveEditor.onBeforeRemoveNodes += OnBeforeRemoveNodes;
			curveEditor.onRemoveNodes += OnRemoveNodes;

			// Move handles.
			curveEditor.onBeginMoveHandle += OnBeginMoveHandle;
			curveEditor.onMoveHandle += OnMoveHandle;
			curveEditor.onEndMoveHandle += OnEndMoveHandle;

			curveEditor.onCheckNodeControls += OnCheckNodeMoveControls;
			curveEditor.nodeSize = 0.065f;
			curveEditor.curveWidth = 3f;
			curveEditor.selectedCurveWidth = 4f;
			curveEditor.curveColor = curveColor;
			curveEditor.selectedCurveColor = selectedCurveColor;
			curveEditor.nodeColor = curveColor;
			curveEditor.selectedNodeColor = selectedCurveColor;
			curveEditor.nodeHandleColor = curveColor;
			curveEditor.selectedNodeHandleColor = selectedCurveColor;
			curveEditor.preselectedNodeColor = Color.red;

			SetPipelineElementProperty ("structureGeneratorElement");
			propRootStructureLevel = GetSerializedProperty ("rootStructureLevel");
			propMinFrequency = propRootStructureLevel.FindPropertyRelative ("minFrequency");
			propMaxFrequency = propRootStructureLevel.FindPropertyRelative ("maxFrequency");
			propMinLength = propRootStructureLevel.FindPropertyRelative ("minLengthAtBase");
			propMaxLength = propRootStructureLevel.FindPropertyRelative ("maxLengthAtBase");
			propRadius = propRootStructureLevel.FindPropertyRelative ("radius");
			propOverrideNoise = propRootStructureLevel.FindPropertyRelative ("overrideNoise");
			propNoise = propRootStructureLevel.FindPropertyRelative ("noise");
			propNoiseScale = propRootStructureLevel.FindPropertyRelative ("noiseScale");
			propStructureLevels = GetSerializedProperty ("flatStructureLevels");

			structureGeneratorComponent = (StructureGeneratorComponent)TreeFactory.GetActiveInstance ().componentManager.GetFactoryComponent (structureGeneratorNode.structureGeneratorElement);

			TreeFactory.GetActiveInstance ().onBeforeProcessPipelinePreview += onBeforeProcessPipelinePreview;
			TreeFactory.GetActiveInstance ().onProcessPipeline += onProcessPipeline;

			SetStructureInspectorEnabled (structureGeneratorNode.inspectStructureEnabled);

			if (structureGeneratorNode.structureGeneratorElement.selectedLevel != null) {
				SetSelectedStructureLevel (structureGeneratorNode.structureGeneratorElement.selectedLevel.id);
			} else {
				SetSelectedStructureLevel (structureGeneratorNode.structureGeneratorElement.rootStructureLevel.id);
			}
			GetTunedBranches ();
			UpdateVertexToSelection (curveEditor.selectedCurveIds);
		}
		private void SetStructureInspectorEnabled (bool enabled) {
			TreeFactory.GetActiveInstance().forcePreviewModeColored = enabled;
			TreeFactory.GetActiveInstance ().ProcessMaterials (TreeFactory.GetActiveInstance ().previewTree);
		}
		/// <summary>
		/// Event called after this editor lose focus.
		/// </summary>
		override protected void OnDisableSpecific () {
			if (curveEditor != null)
				curveEditor.ClearSelection ();
			else
				return;
			curveEditor.OnDisable ();
			_selectedCurveIds.Clear ();
			_selectedIndexes.Clear ();
			_selectedNodes.Clear ();
			_tunedBranchIds.Clear ();
			if (TreeFactory.GetActiveInstance() != null) {
				TreeFactory.GetActiveInstance().forcePreviewModeColored = false;
				TreeFactory.GetActiveInstance ().ProcessMaterials (TreeFactory.GetActiveInstance ().previewTree);
				//TreeFactory.GetActiveInstance().ProcessPipelinePreview (null, true);

				TreeFactory.GetActiveInstance ().onBeforeProcessPipelinePreview -= onBeforeProcessPipelinePreview;
				TreeFactory.GetActiveInstance ().onProcessPipeline -= onProcessPipeline;

				// Clear structure level branches
				SetSelectedStructureLevel (-1);
			}
		}
		void GetTunedBranches () {
			_tunedBranchIds.Clear ();
			GetTunedBranchesRecursive (TreeFactory.GetActiveInstance ().previewTree.branches);
		}
		void GetTunedBranchesRecursive (List<BroccoTree.Branch> branches) {
			for (int i = 0; i < branches.Count; i++) {
				if (branches[i].isTuned) _tunedBranchIds.Add (branches[i].guid);
				GetTunedBranchesRecursive (branches[i].branches);
			}
		}
		bool onBeforeProcessPipelinePreview (Broccoli.Pipe.Pipeline pipeline, 
			BroccoTree tree, 
			int lodIndex,
			PipelineElement referenceElement = null, 
			bool useCache = false, 
			bool forceNewTree = false)
		{
			if (useCache) { // Save the selected nodes and their curves.
				_nodeToCurve.Clear ();
				foreach (System.Guid guid in curveEditor.nodeToCurve.Keys) {
					_nodeToCurve.Add (guid, curveEditor.nodeToCurve[guid]);	
				}
			} else {
				_nodeToCurve.Clear ();
			}
			return true;
		}
		
		bool onProcessPipeline (Broccoli.Pipe.Pipeline pipeline, 
			BroccoTree tree, 
			int lodIndex,
			PipelineElement referenceElement = null, 
			bool useCache = false, 
			bool forceNewTree = false)
		{
			if (useCache) {
				_selectedNodes.Clear ();
				_selectedIndexes.Clear ();
				_selectedCurveIds.Clear ();
				//MatchSelectedBranches (tree.branches);
				curveEditor.AddNodesToSelection (_selectedNodes, _selectedIndexes, _selectedCurveIds);
				//OnNodeSelectionChanged (_selectedNodes, _selectedIndexes, _selectedCurveIds);
			}
			GetTunedBranches ();
			return true;
		}
		/// <summary>
		/// Persists selection of nodes and curves while processing the pipeline.
		/// </summary>
		/// <param name="branches">Branches to inspect for selected nodes and curves.</param>
		void MatchSelectedBranches (List<BroccoTree.Branch> branches) {
			for (int i = 0; i < branches.Count; i++) {
				for (int j = 0; j < branches[i].curve.nodes.Count; j++) {
					if (_nodeToCurve.ContainsKey(branches[i].curve.nodes[j].guid)) {
						_selectedNodes.Add (branches[i].curve.nodes[j]);
						_selectedIndexes.Add (j);
						_selectedCurveIds.Add (branches[i].guid);
					}
				}
				MatchSelectedBranches (branches[i].branches);
			}
		}
		/// <summary>
		/// Reinits the canvas.
		/// </summary>
		private void ReinitCanvas () {
			structureCanvas = StructureCanvas.GetInstance ();
			structureCanvas.Clear ();
			structureCanvasCache = new NodeEditorUserCache (structureCanvas);
			structureCanvasCache.SetupCacheEvents();
			structureCanvas.LoadStructureGenerator (structureGeneratorNode.structureGeneratorElement);
			reinitCanvas = false;
		}
		/// <summary>
		/// Raises the scene GUI event.
		/// </summary>
		/// <param name="sceneView">Scene view.</param>
		protected override void OnSceneGUI (SceneView sceneView) {
			Handles.color = Color.yellow;
			if (structureGeneratorNode != null && 
				structureGeneratorNode.structureGeneratorElement.selectedLevel == null && 
				structureGeneratorNode.structureGeneratorElement.rootStructureLevel.radius > 0) {
				Handles.DrawWireArc (structureGeneratorNode.pipelineElement.pipeline.origin,
					GlobalSettings.againstGravityDirection,
					Vector3.right,
					360,
					structureGeneratorNode.structureGeneratorElement.rootStructureLevel.radius);
			} if (structureGeneratorNode.structureGeneratorElement.selectedLevel == null) {
				DrawStructures (structureGeneratorNode.structureGeneratorElement.flatStructures, 0,
					TreeFactoryEditorWindow.editorWindow.treeFactory.GetPreviewTreeWorldOffset (),
					TreeFactoryEditorWindow.editorWindow.treeFactory.treeFactoryPreferences.factoryScale);
			} else {
				if (structureGeneratorNode.structureGeneratorElement.selectedLevel.isSprout) {
					TreeEditorUtils.DrawTreeSproutsForStructureLevel (
						structureGeneratorNode.structureGeneratorElement.selectedLevel.id, 
						TreeFactoryEditorWindow.editorWindow.treeFactory.previewTree,
						TreeFactoryEditorWindow.editorWindow.treeFactory.GetPreviewTreeWorldOffset (),
						TreeFactoryEditorWindow.editorWindow.treeFactory.treeFactoryPreferences.factoryScale);
				} else {
					DrawStructures (structureGeneratorNode.structureGeneratorElement.flatStructures,
						structureGeneratorNode.structureGeneratorElement.selectedLevel.id,
						TreeFactoryEditorWindow.editorWindow.treeFactory.GetPreviewTreeWorldOffset (),
						TreeFactoryEditorWindow.editorWindow.treeFactory.treeFactoryPreferences.factoryScale);
				}
			}
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI () {
			CheckUndoRequest ();

			UpdateSerialized ();

			NodeEditorGUI.BeginUsingDefaultSkin ();
	
			// Log box.
			DrawLogBox ();

			//NodeEditorGUI.BeginUsingDefaultSkin ();
			if (reinitCanvas) {
				ReinitCanvas ();
			}
			NodeEditorGUI.EndUsingSkin ();

			if (curveEditor == null) return;

			bool selectedStructureChanged = false;
			System.Guid selectedStructureId = curveEditor.selectedCurveId;

			StructureGenerator.StructureLevel selectedLevel = 
				structureGeneratorNode.structureGeneratorElement.selectedLevel;

			// Node canvas.
			DrawNodeCanvas ();
			selectedStructureChanged = DrawNodeCanvasControls ();
			EditorGUILayout.Space ();

			// When a single branch is selected show its information and options to tune it.
			if (selectedStructureId != System.Guid.Empty && curveEditor.hasSingleSelection && 
				structureGeneratorNode.structureGeneratorElement.guidToStructure.ContainsKey (selectedStructureId)) {
				// ROOT LEVEL OPTIONS.
				EditorGUILayout.LabelField ("Selected Branch", EditorStyles.boldLabel);

				// Get the selected structure.
				StructureGenerator.Structure selectedStructure = structureGeneratorNode.structureGeneratorElement.guidToStructure[selectedStructureId];

				// Display selected structure information.
				EditorGUILayout.HelpBox ("Branch Id: " + selectedStructure.id + (selectedStructure.isTuned?" (Tuned)":"") +
					"\nLength: " + selectedStructure.branch.length, MessageType.None);

				// Tune branch girth scale.
				float girthScale = selectedStructure.branch.girthScale;
				girthScale = EditorGUILayout.Slider ("Girth Scale", girthScale, 0f, 1f);
				if (girthScale != selectedStructure.branch.girthScale) {
					Undo.RecordObject (structureGeneratorNode.structureGeneratorElement, "Girth Scale");
					selectedStructure.branch.girthScale = girthScale;
					structureGeneratorComponent.CommitStructure (selectedStructure);
					selectedStructureChanged = true;
					ApplySerialized ();
				}

				// Branch is tuned, show option to unlock it.
				if (selectedStructure.isTuned) {
					EditorGUILayout.LabelField ("You have selected a tuned branch. Unlock it to remove changes.");
					if (GUILayout.Button ("Unlock Branch")) {
						if (EditorUtility.DisplayDialog ("Unlock branch",
						"Unlocking this branch will unlock all of its ascending branches in the hierarchy. The next time the pipeline updates it will lose the changes made to this branch too. Do you want to continue?", "Yes", "No")) {
							Undo.RecordObject (structureGeneratorNode.structureGeneratorElement, "Move Nodes");
							structureGeneratorComponent.UnlockStructure (selectedStructureId);
							ApplySerialized ();
							curveEditor.ClearSelection ();
							GetTunedBranches ();
							UpdateVertexToSelection (curveEditor.selectedCurveIds);
							SceneView.RepaintAll ();
						}
						GUIUtility.ExitGUI();
					}
					EditorGUILayout.Space ();
				}
			}

			// Structure level edition.
			bool rootElementChanged = false;
			bool selectedElementChanged = false;


			if (selectedLevel == null) {
				// MAIN LEVEL OPTIONS.
				EditorGUILayout.LabelField ("Main Level Node", EditorStyles.boldLabel);
				EditorGUILayout.Space ();

				bool rootChanged = false;

				if (selectedPanel > 1) selectedPanel = PANEL_STRUCTURE;
				selectedPanel = GUILayout.Toolbar (selectedPanel, aspectsTrunkPanelOptions);

				// STRUCTURE PANEL.
				if (selectedPanel == PANEL_STRUCTURE) {
					// FREQUENCY
					EditorGUI.BeginChangeCheck ();
					IntRangePropertyField (propMinFrequency, propMaxFrequency, 0, 30, "Frequency");
					ShowHelpBox (MSG_MAIN_MIN_MAX_FREQUENCY);

					// LENGTH
					FloatRangePropertyField (propMinLength, propMaxLength, 0.1f, 40f, "Length");
					ShowHelpBox (MSG_MAIN_MIN_MAX_LENGTH);
					if (EditorGUI.EndChangeCheck ()) {
						rootChanged = true;
					}
						
					// RADIUS
					float radius = propRadius.floatValue;
					EditorGUILayout.Slider (propRadius, 0f, 20f, "Radius");
					ShowHelpBox (MSG_MAIN_RADIUS);
					if (radius != propRadius.floatValue) {
						rootChanged = true;
					}
				}
				// ADVANCED PANEL.
				else {
					// OVERRIDE NOISE.
					bool overrideNoise = propOverrideNoise.boolValue;
					EditorGUILayout.PropertyField (propOverrideNoise);
					ShowHelpBox (MSG_OVERRIDE_NOISE);
					if (overrideNoise != propOverrideNoise.boolValue) {
						rootChanged = true;
					}
					if (overrideNoise) {
						// NOISE.
						EditorGUI.BeginChangeCheck ();
						EditorGUILayout.Slider (propNoise, 0f, 1f, "Noise");
						ShowHelpBox (MSG_NOISE);

						// NOISE SCALE.
						EditorGUILayout.Slider (propNoiseScale, 0f, 1f, "Noise Scale");
						ShowHelpBox (MSG_NOISE_SCALE);
						if (EditorGUI.EndChangeCheck ()) {
							rootChanged = true;
						}
					}
				}

				if (rootChanged &&
					propMinFrequency.intValue <= propMaxFrequency.intValue &&
					propMinLength.floatValue <= propMaxLength.floatValue) {
					rootElementChanged = true;
				} else if (propMinLength.floatValue > propMaxLength.floatValue) { // FIX
					propMinLength.floatValue = propMaxLength.floatValue;
					rootElementChanged = true;
				}
			} else {
				// NON ROOT LEVEL OPTIONS.
				int index = structureGeneratorNode.structureGeneratorElement.GetStructureLevelIndex (selectedLevel);
				if (index >= 0) {
					SerializedProperty propStructureLevel = propStructureLevels.GetArrayElementAtIndex (index);

					bool levelChanged = false;

					if (selectedLevel.isSprout) {
						EditorGUILayout.LabelField ("Sprout Level " + selectedLevel.level + " Node", EditorStyles.boldLabel);
					} else if (selectedLevel.isRoot) {
						EditorGUILayout.LabelField ("Root Level " + selectedLevel.level + " Node", EditorStyles.boldLabel);
					} else {
						EditorGUILayout.LabelField ("Branch Level " + selectedLevel.level + " Node", EditorStyles.boldLabel);
					}

					// ENABLED
					EditorGUI.BeginChangeCheck ();
					// Enabled.
					SerializedProperty propIsEnabled = propStructureLevel.FindPropertyRelative ("enabled");
					bool isEnabled = propIsEnabled.boolValue;
					EditorGUILayout.PropertyField (propIsEnabled);
					ShowHelpBox (MSG_ENABLED);
					if (isEnabled != propIsEnabled.boolValue) {
						bool isDrawVisible = propIsEnabled.boolValue;
						selectedLevel.enabled = propIsEnabled.boolValue;
						structureGeneratorNode.structureGeneratorElement.UpdateDrawVisible();
					}
					if (EditorGUI.EndChangeCheck ()) {
						levelChanged = true;
					}
					EditorGUILayout.Space ();

					if (selectedLevel.isSprout) {
						if (selectedPanel >= PANEL_ADVANCED) selectedPanel = PANEL_STRUCTURE;
						selectedPanel = GUILayout.Toolbar (selectedPanel, aspectsSproutPanelOptions);
					} else {
						selectedPanel = GUILayout.Toolbar (selectedPanel, aspectsFullPanelOptions);
					}
					
					EditorGUILayout.Space ();
					EditorGUI.BeginDisabledGroup (!isEnabled);
					switch (selectedPanel) {
						case PANEL_STRUCTURE: // Structure
							levelChanged = DrawStructurePanel (selectedLevel, propStructureLevel, levelChanged);
							break;
						case PANEL_ALIGNMENT: // Alignment
							levelChanged = DrawAlignmentPanel (selectedLevel, propStructureLevel, levelChanged);
							break;
						case PANEL_RANGE: // Range
							levelChanged = DrawRangePanel (selectedLevel, propStructureLevel, levelChanged);
							break;
						case PANEL_ADVANCED: // Advanced
							levelChanged = DrawAdvancedPanel (selectedLevel, propStructureLevel, levelChanged);
							break;
					}
					EditorGUI.EndDisabledGroup ();

					if (levelChanged) {
						selectedElementChanged = true;
					}
				}
			}
			EditorGUILayout.Space ();

			// Seed options.
			DrawSeedOptions ();

			if (rootElementChanged || selectedElementChanged || selectedStructureChanged) {
				ApplySerialized ();
				UpdatePipeline (GlobalSettings.processingDelayMedium, true);
				SetUndoControlCounter ();
			}

			//NodeEditorGUI.EndUsingSkin ();

			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		bool DrawStructurePanel (StructureGenerator.StructureLevel selectedLevel, SerializedProperty propStructureLevel, bool levelChanged) {
			// SPROUT GROUP
			if (selectedLevel.isSprout) {
				EditorGUI.BeginChangeCheck ();
				if (structureGeneratorNode.pipelineElement.pipeline.sproutGroups.Count () > 0) {
					int sproutGroupIndex = EditorGUILayout.Popup ("Sprout Group",
												structureGeneratorNode.pipelineElement.pipeline.sproutGroups.GetSproutGroupIndex (selectedLevel.sproutGroupId, true),
												structureGeneratorNode.pipelineElement.pipeline.sproutGroups.GetPopupOptions (true));
					ShowHelpBox (MSG_SPROUT_GROUP);
					int selectedSproutGroupId = structureGeneratorNode.pipelineElement.pipeline.sproutGroups.GetSproutGroupId (sproutGroupIndex);
					if (selectedLevel.sproutGroupId != selectedSproutGroupId) {
						SproutGroups.SproutGroup sproutGroup = 
							structureGeneratorNode.pipelineElement.pipeline.sproutGroups.GetSproutGroup (selectedSproutGroupId);
						if (sproutGroup != null) {
							selectedLevel.sproutGroupId = sproutGroup.id;
							selectedLevel.sproutGroupColor = sproutGroup.GetColor ();
						} else {
							selectedLevel.sproutGroupId = -1;
							selectedLevel.sproutGroupColor = Color.clear;
						}
					}
				} else {
					EditorGUILayout.HelpBox ("Add at least one Sprout Group to the pipeline to assign it to this sprout node.", MessageType.Warning);
				}
				if (EditorGUI.EndChangeCheck ()) {
					levelChanged = true;
				}
				EditorGUILayout.Space ();
			}

			// FREQUENCY & PROBABILITY
			EditorGUI.BeginChangeCheck ();
			// Max frequency.
			EditorGUILayout.IntSlider (propStructureLevel.FindPropertyRelative ("maxFrequency"), 0, 30);
			int maxFrequency = propStructureLevel.FindPropertyRelative ("maxFrequency").intValue;
			ShowHelpBox (MSG_MAX_FREQUENCY);
			// Min frequency.
			EditorGUILayout.IntSlider (propStructureLevel.FindPropertyRelative ("minFrequency"), 0, 30);
			int minFrequency = propStructureLevel.FindPropertyRelative ("minFrequency").intValue;
			ShowHelpBox (MSG_MIN_FREQUENCY);
			// Probability.
			EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("probability"), 0f, 1f);
			ShowHelpBox (MSG_PROBABILITY);
			// Shared Probability.
			if (selectedLevel.IsShared ()) {
				// Shared probability.
				EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("sharedProbability"), 0f, 1f);
				ShowHelpBox (MSG_SHARED_PROBABILITY);
			}
			EditorGUILayout.Space ();
			if (EditorGUI.EndChangeCheck ()) {
				levelChanged = true;
			}

			// DISTRIBUTION
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (propStructureLevel.FindPropertyRelative ("distribution"));
			ShowHelpBox (MSG_DISTRIBUTION_MODE);
			if (selectedLevel.distribution == StructureGenerator.StructureLevel.Distribution.Whorled) {
				EditorGUILayout.IntSlider (propStructureLevel.FindPropertyRelative ("childrenPerNode"), 1, 10, "Whorled Step");
				ShowHelpBox (MSG_DISTRIBUTION_WHORLED);
			}
			EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("distributionSpacingVariance"), 0f, 1f);
			ShowHelpBox (MSG_DISTRIBUTION_SPACING_VARIANCE);
			EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("distributionAngleVariance"), 0f, 1f);
			ShowHelpBox (MSG_DISTRIBUTION_ANGLE_VARIANCE);
			EditorGUILayout.PropertyField (propStructureLevel.FindPropertyRelative ("distributionCurve"));
			ShowHelpBox (MSG_DISTRIBUTION_CURVE);
			EditorGUILayout.Space ();
			if (EditorGUI.EndChangeCheck ()) {
				levelChanged = true;
			}

			// TWIRL OFFSET
			EditorGUI.BeginChangeCheck ();
			if (!propStructureLevel.FindPropertyRelative ("randomTwirlOffsetEnabled").boolValue) {
				EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("twirlOffset"), -1f, 1f);
				ShowHelpBox (MSG_TWIRL_OFFSET);
			}
			EditorGUILayout.PropertyField (propStructureLevel.FindPropertyRelative ("randomTwirlOffsetEnabled"), 
				new GUIContent ("Randomize Twirl Offset"));
			ShowHelpBox (MSG_RANDOM_TWIRL_OFFSET_ENABLED);
			if (EditorGUI.EndChangeCheck ()) {
				levelChanged = true;
			}

			// TWIRL
			EditorGUI.BeginChangeCheck ();
			//EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("twirl"), -1f, 1f);
			FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minTwirl"), 
				propStructureLevel.FindPropertyRelative ("maxTwirl"), -1f, 1f, "Twirl");
			ShowHelpBox (MSG_TWIRL);
			EditorGUILayout.Space ();
			if (EditorGUI.EndChangeCheck () &&
				minFrequency <= maxFrequency) {
				levelChanged = true;
			}

			if (!selectedLevel.isSprout) {
				EditorGUILayout.Space ();
				// LENGTH
				EditorGUI.BeginChangeCheck ();
				//EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("lengthAtTop"), 0.1f, 20f);
				FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minLengthAtTop"), 
					propStructureLevel.FindPropertyRelative ("maxLengthAtTop"), 0.1f, 20f, "Length at Top");
				ShowHelpBox (MSG_LENGTH_AT_TOP);
				//EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("lengthAtBase"), 0.1f, 20f);
				FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minLengthAtBase"), 
				propStructureLevel.FindPropertyRelative ("maxLengthAtBase"), 0.1f, 20f, "Length at Base");
				ShowHelpBox (MSG_LENGTH_AT_BASE);
				EditorGUILayout.PropertyField (propStructureLevel.FindPropertyRelative ("lengthCurve"));
				ShowHelpBox (MSG_LENGTH_CURVE);
				EditorGUILayout.Space ();

				// GIRTH SCALE
				SerializedProperty propMinGirthScale = propStructureLevel.FindPropertyRelative ("minGirthScale");
				SerializedProperty propMaxGirthScale = propStructureLevel.FindPropertyRelative ("maxGirthScale");
				float minGirthScale = propMinGirthScale.floatValue;
				float maxGirthScale = propMaxGirthScale.floatValue;
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.MinMaxSlider ("Girth Scale", ref minGirthScale, ref maxGirthScale, 0.01f, 1f);
				EditorGUILayout.LabelField (minGirthScale.ToString("F2") + "-" + maxGirthScale.ToString("F2"), GUILayout.Width (60));
				EditorGUILayout.EndHorizontal ();
				if (minGirthScale != propMinGirthScale.floatValue || maxGirthScale != propMaxGirthScale.floatValue) {
					propMinGirthScale.floatValue = minGirthScale;
					propMaxGirthScale.floatValue = maxGirthScale;
				}
				ShowHelpBox (MSG_GIRTH_SCALE);
				EditorGUILayout.Space ();

				if (EditorGUI.EndChangeCheck ()) {
					levelChanged = true;
				}
			}

			// FROM BRANCH CENTER
			if (selectedLevel.isSprout) {
				EditorGUILayout.Space ();
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (propStructureLevel.FindPropertyRelative ("fromBranchCenter"));
				ShowHelpBox (MSG_FROM_BRANCH_CENTER);
				EditorGUILayout.Space ();
				if (EditorGUI.EndChangeCheck ()) {
					levelChanged = true;
				}
			}

			return levelChanged;
		}
		bool DrawAlignmentPanel (StructureGenerator.StructureLevel selectedLevel, SerializedProperty propStructureLevel, bool levelChanged) {
			EditorGUI.BeginChangeCheck ();
			// Parallel align at top and at base.
			//EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("parallelAlignAtTop"), -1f, 1f);
			FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minParallelAlignAtTop"), 
				propStructureLevel.FindPropertyRelative ("maxParallelAlignAtTop"), -1f, 1f, "Parallel Align at Top");
			ShowHelpBox (MSG_PARALLEL_ALIGN_AT_TOP);
			//EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("parallelAlignAtBase"), -1f, 1f);
			FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minParallelAlignAtBase"), 
				propStructureLevel.FindPropertyRelative ("maxParallelAlignAtBase"), -1f, 1f, "Parallel Align at Base");
			ShowHelpBox (MSG_PARALLEL_ALIGN_AT_BASE);
			EditorGUILayout.PropertyField (propStructureLevel.FindPropertyRelative ("parallelAlignCurve"));
			ShowHelpBox (MSG_PARALLEL_ALIGN_CURVE);
			EditorGUILayout.Space ();
			// Gravity align at top and at base.
			//EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("gravityAlignAtTop"), -1f, 1f);
			FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minGravityAlignAtTop"), 
				propStructureLevel.FindPropertyRelative ("maxGravityAlignAtTop"), -1f, 1f, "Gravity Align at Top");
			ShowHelpBox (MSG_GRAVITY_ALIGN_AT_TOP);
			//EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("gravityAlignAtBase"), -1f, 1f);
			FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minGravityAlignAtBase"), 
				propStructureLevel.FindPropertyRelative ("maxGravityAlignAtBase"), -1f, 1f, "Gravity Align at Base");
			ShowHelpBox (MSG_GRAVITY_ALIGN_AT_BASE);
			EditorGUILayout.PropertyField (propStructureLevel.FindPropertyRelative ("gravityAlignCurve"));
			ShowHelpBox (MSG_GRAVITY_ALIGN_CURVE);
			EditorGUILayout.Space ();
			if (!selectedLevel.isSprout) {
				// Horizontal align at top and at base.
				//EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("horizontalAlignAtTop"), -1f, 1f);
				FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minHorizontalAlignAtTop"), 
					propStructureLevel.FindPropertyRelative ("maxHorizontalAlignAtTop"), -1f, 1f, "Horizontal Align at Top");
				ShowHelpBox (MSG_HORIZONTAL_ALIGN_AT_TOP);
				//EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("horizontalAlignAtBase"), -1f, 1f);
				FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minHorizontalAlignAtBase"), 
					propStructureLevel.FindPropertyRelative ("maxHorizontalAlignAtBase"), -1f, 1f, "Horizontal Align at Base");
				ShowHelpBox (MSG_HORIZONTAL_ALIGN_AT_BASE);
				EditorGUILayout.PropertyField (propStructureLevel.FindPropertyRelative ("horizontalAlignCurve"));
				ShowHelpBox (MSG_HORIZONTAL_ALIGN_CURVE);
				EditorGUILayout.Space ();
			} else {
				SerializedProperty propFlipAlign = propStructureLevel.FindPropertyRelative ("flipSproutAlign");
				float flipAlign = propFlipAlign.floatValue;
				flipAlign = EditorGUILayout.Slider ("Flip Align", flipAlign, 0f, 1f);
				if (flipAlign != propFlipAlign.floatValue) {
					propFlipAlign.floatValue = flipAlign;
				}
				SerializedProperty propFlipDirection = propStructureLevel.FindPropertyRelative ("flipSproutDirection");
				EditorGUILayout.PropertyField (propFlipDirection);
				EditorGUILayout.Space ();
			}
			if (EditorGUI.EndChangeCheck ()) {
				levelChanged = true;
			}
			return levelChanged;
		}
		bool DrawRangePanel (StructureGenerator.StructureLevel selectedLevel, SerializedProperty propStructureLevel, bool levelChanged) {
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (propStructureLevel.FindPropertyRelative ("actionRangeEnabled"));
			ShowHelpBox (MSG_RANGE_ENABLED);
			if (selectedLevel.actionRangeEnabled) {
				FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minRange"), 
					propStructureLevel.FindPropertyRelative ("maxRange"), 0f, 1f, "Range");
				ShowHelpBox (MSG_RANGE);
				FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minMaskRange"), 
					propStructureLevel.FindPropertyRelative ("maxMaskRange"), 0f, 1f, "Mask");
				ShowHelpBox (MSG_MASK_RANGE);
			}
			EditorGUILayout.Space ();
			if (!selectedLevel.isSprout && GlobalSettings.experimentalBranchBreak) {
				EditorGUILayout.PropertyField (propStructureLevel.FindPropertyRelative ("applyBranchBreak"));
				ShowHelpBox (MSG_APPLY_BREAK);
				if (selectedLevel.applyBranchBreak) {
					EditorGUILayout.CurveField (propStructureLevel.FindPropertyRelative ("breakBranchProbability"), Color.green, scaleCurveRange);
					ShowHelpBox (MSG_BREAK_PROBABILITY);
					FloatRangePropertyField (propStructureLevel.FindPropertyRelative ("minBreakRange"), 
						propStructureLevel.FindPropertyRelative ("maxBreakRange"), 0f, 1f, "BreakRange");
					ShowHelpBox (MSG_BREAK_RANGE);
				}
				EditorGUILayout.Space ();
			}
			if (EditorGUI.EndChangeCheck ()) {
				levelChanged = true;
			}
			return levelChanged;
		}
		bool DrawAdvancedPanel (StructureGenerator.StructureLevel selectedLevel, SerializedProperty propStructureLevel, bool levelChanged) {
			EditorGUI.BeginChangeCheck ();
			// OVERRIDE NOISE.
			SerializedProperty _propOverrideNoise = propStructureLevel.FindPropertyRelative ("overrideNoise");
			bool overrideNoise = EditorGUILayout.Toggle ("Override Noise", _propOverrideNoise.boolValue);
			ShowHelpBox (MSG_OVERRIDE_NOISE);
			if (overrideNoise != _propOverrideNoise.boolValue) {
				_propOverrideNoise.boolValue = overrideNoise;
				levelChanged = true;
			}
			if (overrideNoise) {
				// NOISE.
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("noise"), 0f, 1f, "Noise");
				ShowHelpBox (MSG_NOISE);

				// NOISE SCALE.
				EditorGUILayout.Slider (propStructureLevel.FindPropertyRelative ("noiseScale"), 0f, 1f, "Noise Scale");
				ShowHelpBox (MSG_NOISE_SCALE);
				if (EditorGUI.EndChangeCheck ()) {
					levelChanged = true;
				}
			}
			return levelChanged;
		}
		protected override void OnUndo() {
			structureGeneratorNode.structureGeneratorElement.DeserializeStructures ();
		}
		#endregion

		#region Draw Functions
		/// <summary>
		/// Draws the node canvas.
		/// </summary>
		private void DrawNodeCanvas () {
			GUILayout.Box ("", GUIStyle.none, 
				GUILayout.Width (Screen.width * 0.9f), 
				GUILayout.Height (Screen.width * 0.9f));
			Rect canvasRect = GUILayoutUtility.GetLastRect ();
			if (structureCanvasCache.editorState != null) {
				structureCanvasCache.editorState.canvasRect = canvasRect;
			}

			NodeEditor.checkInit (true);
			NodeEditorGUI.StartNodeGUI ("StructureGenerator", false);
			NodeEditor.DrawCanvas (structureCanvasCache.nodeCanvas, structureCanvasCache.editorState);

			if (structureGeneratorNode.inspectStructureEnabled) {
				if (GUI.Button (new Rect (canvasRect.x, canvasRect.y, 32, 32),
					new GUIContent("", GUITextureManager.inspectMeshOnTexture,
						"Structure Inspection View is On. Click to turn it off."))) {
							structureGeneratorNode.inspectStructureEnabled = false;
							SetStructureInspectorEnabled (false);
						}
			} else {
				if (GUI.Button (new Rect (canvasRect.x, canvasRect.y, 32, 32),
					new GUIContent("", GUITextureManager.inspectMeshOffTexture,
						"Structure Inspection View is Off. Click to turn it on."))) {
							structureGeneratorNode.inspectStructureEnabled = true;
							SetStructureInspectorEnabled (true);
						}
			}

			NodeEditorGUI.EndNodeGUI();
			
			if (structureCanvas.isDirty) {
				structureCanvas.isDirty = false;
				EditorUtility.SetDirty (structureGeneratorNode);
			}
		}
		/// <summary>
		/// Draws the node canvas controls.
		/// </summary>
		/// <returns>True if a change has been made to the structure.</returns>
		private bool DrawNodeCanvasControls () {
			bool changed = false;

			// ADD STRUCTURE GENERATORS.
			// Structure canvas edit options.
			GUILayout.BeginHorizontal ();
			bool mainStructureSelected = structureGeneratorNode.structureGeneratorElement.selectedLevel == null;
			bool sproutStructureSelected = structureGeneratorNode.structureGeneratorElement.selectedLevel != null && 
				structureGeneratorNode.structureGeneratorElement.selectedLevel.isSprout == true;
			bool rootStructureSelected = structureGeneratorNode.structureGeneratorElement.selectedLevel != null && 
				structureGeneratorNode.structureGeneratorElement.selectedLevel.isRoot == true;

			// Branch Add Button.
			EditorGUI.BeginDisabledGroup (sproutStructureSelected || rootStructureSelected);
			if (GUILayout.Button (new GUIContent ("+ Branch Level", "Adds a child branch level to the selected structure level."))) {
				StructureGenerator.StructureLevel newLevel = 
					structureGeneratorNode.structureGeneratorElement.AddStructureLevel (
						structureGeneratorNode.structureGeneratorElement.selectedLevel);
				structureGeneratorNode.structureGeneratorElement.selectedLevel = newLevel;
				curveEditor.ClearSelection ();
				reinitCanvas = true;
				changed = true;
			}
			EditorGUI.EndDisabledGroup ();

			// Sprout Add Button
			EditorGUI.BeginDisabledGroup (sproutStructureSelected || rootStructureSelected);
			if (GUILayout.Button (new GUIContent ("+ Sprout Level", "Adds a child sprout level to the selected structure level."))) {
				StructureGenerator.StructureLevel newLevel = 
					structureGeneratorNode.structureGeneratorElement.AddStructureLevel (
						structureGeneratorNode.structureGeneratorElement.selectedLevel, true);
				structureGeneratorNode.structureGeneratorElement.selectedLevel = newLevel;
				curveEditor.ClearSelection ();
				reinitCanvas = true;
				changed = true;
			}
			EditorGUI.EndDisabledGroup ();

			// Root Add Button
			EditorGUI.BeginDisabledGroup (!rootStructureSelected && !mainStructureSelected);
			if (GUILayout.Button (new GUIContent ("+ Root Level", "Add a child root level to the selected structure level."))) {
				StructureGenerator.StructureLevel newLevel = 
					structureGeneratorNode.structureGeneratorElement.AddStructureLevel (
						structureGeneratorNode.structureGeneratorElement.selectedLevel, false, true);
				structureGeneratorNode.structureGeneratorElement.selectedLevel = newLevel;
				curveEditor.ClearSelection ();
				reinitCanvas = true;
				changed = true;
			}
			EditorGUI.EndDisabledGroup ();
			GUILayout.EndHorizontal ();

			// ADD STRUCTURE GENERATORS.
			GUILayout.BeginHorizontal ();
			// Delete level.
			EditorGUI.BeginDisabledGroup (mainStructureSelected);
			if (GUILayout.Button (new GUIContent ("- Remove Level", "Removes the selected structure level."))) {
				StructureGenerator.StructureLevel selectedLevel =
					structureGeneratorNode.structureGeneratorElement.selectedLevel;
				curveEditor.ClearSelection ();
				if (selectedLevel != null) {
					if (EditorUtility.DisplayDialog ("Delete Structure Level",
						"Delete this level and its children?", "Yes", "No")) {
						structureGeneratorNode.structureGeneratorElement.RemoveStructureLevel (selectedLevel);
						structureGeneratorNode.structureGeneratorElement.selectedLevel = null;
						curveEditor.ClearSelection ();
						reinitCanvas = true;
						changed = true;
					}
					GUIUtility.ExitGUI();
				}
			}
			EditorGUI.EndDisabledGroup ();
			GUILayout.EndHorizontal ();

			// SHARED STRUCTURES OPTIONS.
			// Structure canvas shared nodes options.
			GUILayout.BeginHorizontal ();
			// Add new shared branch level.
			bool addSharedBranchLevelDisabled = 
				structureGeneratorNode.structureGeneratorElement.selectedLevel != null &&
				structureGeneratorNode.structureGeneratorElement.selectedLevel.isSprout == true;
			EditorGUI.BeginDisabledGroup (addSharedBranchLevelDisabled || rootStructureSelected);
			if (GUILayout.Button (new GUIContent ("+ Shared Branch Level", "Add shared branch level."))) {
				StructureGenerator.StructureLevel newLevel = 
					structureGeneratorNode.structureGeneratorElement.AddSharedStructureLevel (
						structureGeneratorNode.structureGeneratorElement.selectedLevel);
				structureGeneratorNode.structureGeneratorElement.selectedLevel = newLevel;
				curveEditor.ClearSelection ();
				reinitCanvas = true;
				changed = true;
			}
			EditorGUI.EndDisabledGroup ();

			// Add new shared sprout level.
			bool addSharedSproutLevelDisabled = false;
			EditorGUI.BeginDisabledGroup (addSharedSproutLevelDisabled || rootStructureSelected);
			if (GUILayout.Button (new GUIContent ("+ Shared Sprout Level", "Add shared sprout level."))) {
				StructureGenerator.StructureLevel newLevel = 
					structureGeneratorNode.structureGeneratorElement.AddSharedStructureLevel (
						structureGeneratorNode.structureGeneratorElement.selectedLevel, true);
				structureGeneratorNode.structureGeneratorElement.selectedLevel = newLevel;
				curveEditor.ClearSelection ();
				reinitCanvas = true;
				changed = true;
			}
			EditorGUI.EndDisabledGroup ();
			GUILayout.EndHorizontal ();

			return changed;
		}
		#endregion
		
		#region Drawing
		void DrawStructures (List<StructureGenerator.Structure> structures, int structureLevelId, Vector3 offset, float scale = 1) {
			curveEditor.scale = scale;
			for (int i = 0; i < structures.Count; i++) {
				if (structures[i].generatorId == structureLevelId) {
					_editStructure = structures[i];
					_editStructureId = _editStructure.branch.guid;
					curveEditor.curveId = _editStructureId;
					_editStructureLevelId = structureLevelId;
					curveEditor.scale = scale;
					bool isSelected = _selectedCurveIds.Contains (_editStructureId);
					curveEditor.showFirstHandleAlways = false;
					curveEditor.showSecondHandleAlways = false;
					if (structures[i].branch.IsFollowUp()) {
						curveEditor.showFirstHandleAlways = true;
					}
					if (structures[i].branch.followUp != null) {
						curveEditor.showSecondHandleAlways = true;
					}
					// Draw a unique curve if edit mode is add.
					if (curveEditor.editMode == BezierCurveEditor.EditMode.Add) {
						if (_singleSelectedCurveId == curveEditor.curveId) {
							curveEditor.OnSceneGUI (structures[i].branch.curve, offset + (structures[i].branch.positionFromRoot * scale), isSelected);
							if (_singleSelectedCurveProcessRequired) {
								//curveEditor.SetAddNodeCandidates ();
								_singleSelectedCurveProcessRequired = false;
							}
						}
					} else {
						curveEditor.OnSceneGUI (structures[i].branch.curve, offset + (structures[i].branch.positionFromRoot * scale), isSelected);
					}
					if (structures[i].branch.IsFollowUp ()) {
						_editStructure = structures[i].parentStructure;
						_editStructureId = _editStructure.branch.guid;
						curveEditor.curveId = _editStructureId;
						curveEditor.OnSceneGUIDrawSingleNode (
							structures[i].branch.parent.curve,
							structures[i].branch.parent.curve.nodes.Count - 1, 
							offset + (structures[i].branch.parent.positionFromRoot * scale), 
							isSelected);
					}
				}
			}
			_editStructureId = System.Guid.Empty;
			_editStructure = null;
		}
		void DrawStructureLevel (StructureGenerator.StructureLevel level, Vector3 offset, float scale = 1) {
			curveEditor.scale = scale;
			for (int i = 0; i < level.generatedBranches.Count; i++) {
				curveEditor.OnSceneGUI (level.generatedBranches[i].curve, offset + (level.generatedBranches[i].positionFromRoot * scale));
			}
		}
		void UpdateVertexToSelection (List<System.Guid> selectedBranchIds) {
			/*
			 * x: branch id.
			 * y: generator id.
			 * z: 0, 1: tuned, 2: selected, 3: selected + tuned
			 * w:
			 */
			/// UV6 information of the mesh.
			/// x: id of the branch.
			/// y: if of the branch skin.
			/// z: id of the struct.
			/// w: tuned.
			/*
			#if UNITY_2018_2_OR_NEWER
			List<Vector4> uv6s = new List<Vector4> ();
			MeshFilter meshFilter = TreeFactory.GetActiveInstance().previewTree.obj.GetComponent<MeshFilter>();
			if (meshFilter != null) {
				meshFilter.sharedMesh.GetUVs (5, uv6s);
				for (int i = 0; i < uv6s.Count; i++) {
					int selectionTunedValue = 0;
					if (_tunedBranchIds.Contains ((int)uv6s[i].x)) {
						selectionTunedValue += 1;
					}
					if (selectedBranchIds.Contains ((int)uv6s[i].x)) {
						selectionTunedValue += 2;
					}
					uv6s[i] = new Vector4 (uv6s[i].x, uv6s[i].y, uv6s[i].z, selectionTunedValue);
				}
				meshFilter.sharedMesh.SetUVs (5, uv6s);
			}
			Repaint ();
			#endif
			*/
		}
		#endregion

		#region Canvas Editor
		void OnSelectStructureLevel (Broccoli.NodeEditorFramework.Node node) {
			bool shouldExitGUI = curveEditor.hasSingleSelection && curveEditor.showTools;
			curveEditor.ClearSelection ();
			if (shouldExitGUI) GUIUtility.ExitGUI ();
			if (structureGeneratorNode.structureGeneratorElement.selectedLevel == null) {
				SetSelectedStructureLevel (structureGeneratorNode.structureGeneratorElement.rootStructureLevel.id);
			} else {
				SetSelectedStructureLevel (structureGeneratorNode.structureGeneratorElement.selectedLevel.id);
			}
		}
		void OnDeselectStructureLevel () {
			curveEditor.ClearSelection ();
		}
		void SetSelectedStructureLevel (int selectedLevelId) {
			MeshRenderer meshRenderer = TreeFactory.GetActiveInstance ().previewTree.obj.GetComponent<MeshRenderer> ();
			MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock ();
			if (meshRenderer != null) {
				meshRenderer.GetPropertyBlock (propertyBlock);
				propertyBlock.SetFloat ("_SelectedLevel", selectedLevelId);
				meshRenderer.SetPropertyBlock (propertyBlock);
			}
		}
		#endregion

		#region Curve Editor
		void DrawBranchForStructureLevel (int structureLevelId, BroccoTree.Branch branch, 
			Vector3 origin, Vector3 offset, float scale = 1f) 
		{
			#if UNITY_EDITOR
			if (branch.helperStructureLevelId == structureLevelId) {
				_editStructureId = branch.guid;
				_editStructureLevelId = structureLevelId;
				curveEditor.scale = scale;
				curveEditor.OnSceneGUI (branch.curve, offset + (branch.positionFromRoot * scale));
			}
			for (int i = 0; i < branch.branches.Count; i++) {
				Vector3 childBranchOrigin = branch.branches[i].origin;
				DrawBranchForStructureLevel (structureLevelId, branch.branches[i], childBranchOrigin, offset, scale);
			}
			#endif
		}
		/// <summary>
		/// Called when the editor changed mode.
		/// </summary>
		/// <param name="editMode">New edit mode.</param>
		void OnEditModeChanged (BezierCurveEditor.EditMode editMode) {
			if (editMode == BezierCurveEditor.EditMode.Add) {
				_singleSelectedCurveId = curveEditor.selectedCurveId;
				_singleSelectedCurveProcessRequired = true;
			} else {
				_singleSelectedCurveId = System.Guid.Empty;
			}
		}
		/// <summary>
		/// Called when the node selection changes.
		/// </summary>
		/// <param name="nodes">Nodes in the selection.</param>
		/// <param name="indexes">Indexes of the nodes in the selection.</param>
		void OnNodeSelectionChanged (List<BezierNode> nodes, List<int> indexes, List<System.Guid> curveIds) {
			UpdateVertexToSelection (curveIds);
			// If more than one node is selected disable bezier tools.
			if (nodes.Count == 1) {
				if (curveIds.Count > 0) curveEditor.focusedCurveId = curveIds [0];
				curveEditor.showTools = true;
			} else {
				curveEditor.focusedCurveId = System.Guid.Empty;
				curveEditor.showTools = false;
				curveEditor.editMode = BezierCurveEditor.EditMode.Selection;
			}
		}
		/// <summary>
		/// Checks the offset used to move the selected nodes.
		/// </summary>
		/// <param name="offset">Offset value.</param>
		/// <returns>The offset to use to move the selected nodes.</returns>
		Vector3 OnCheckMoveNodes (Vector3 offset) {
			return offset;
		}
		/// <summary>
		/// Called right before a list of nodes get moved.
		/// </summary>
		/// <param name="nodes">Nodes to be moved.</param>
		/// <param name="indexes">Index of nodes to be moved.</param>
		void OnBeginMoveNodes (List<BezierNode> nodes, List<int> indexes, List<System.Guid> curveIds) {
			Undo.RecordObject (structureGeneratorNode.structureGeneratorElement, "Move Nodes");
		}
		/// <summary>
		/// Called after a list of nodes have been moved.
		/// </summary>
		/// <param name="nodes">Nodes moved.</param>
		/// <param name="indexes">Index of nodes moved.</param>
		void OnMoveNodes (List<BezierNode> nodes, List<int> indexes, List<System.Guid> curveIds) {
			for (int i = 0; i < curveIds.Count; i++) {
				structureGeneratorComponent.CommitBranchCurve (curveIds[i], indexes[i], curveEditor.offsetStep);
			}
			GetTunedBranches ();
			ApplySerialized ();
			UpdatePipeline (GlobalSettings.processingDelayMedium);
		}
		/// <summary>
		/// Called at the end of moving nodes.
		/// </summary>
		/// <param name="nodes">Nodes to be moved.</param>
		/// <param name="indexes">Index of nodes to be moved.</param>
		void OnEndMoveNodes (List<BezierNode> nodes, List<int> indexes, List<System.Guid> curveIds) {
			SetUndoControlCounter (false);
		}
		/// <summary>
		/// Called right before a node is edited (ex. handle style changed).
		/// </summary>
		/// <param name="node">Node to edit.</param>
		/// <param name="index">Index of edited node.</param>
		void OnBeforeEditNode (BezierNode node, int index) {
			Undo.RecordObject (structureGeneratorNode.structureGeneratorElement, "Change Node Mode");
		}
		/// <summary>
		/// Called after a node is edited (ex. handle style changed).
		/// </summary>
		/// <param name="node">Node to edit.</param>
		/// <param name="index">Index of edited node.</param>
		void OnEditNode (BezierNode node, int index) {
			structureGeneratorComponent.CommitBranchCurve (curveEditor.selectedCurveId, index, curveEditor.offsetStep, true);
			ApplySerialized ();
			UpdatePipeline (GlobalSettings.processingDelayMedium);
		}
		/// <summary>
		/// Called before a new node gets added.
		/// </summary>
		/// <param name="node">Node to add.</param>
		void OnBeforeAddNode (BezierNode node) {
			Undo.RecordObject (structureGeneratorNode.structureGeneratorElement, "Add Node");
		}
		/// <summary>
		/// Called after a new node gets added.
		/// </summary>
		/// <param name="node">Node to add.</param>
		/// <param name="index">Index of the node added.</param>
		/// <param name="relativePosition">Relative position of the new node.</param>
		void OnAddNode (BezierNode node, int index, float relativePosition) {
			node.handleStyle = BezierNode.HandleStyle.Auto;
			SetUndoControlCounter (false);
			GetTunedBranches ();
			ApplySerialized ();
			UpdatePipeline (GlobalSettings.processingDelayMedium);
			curveEditor.editMode = BezierCurveEditor.EditMode.Selection;
			_singleSelectedCurveId = node.curve.guid;
			curveEditor.ClearSelection (true, false);
			curveEditor.AddNodeToSelection (node, index, node.curve.guid);
			GUIUtility.hotControl = 0;
		}
		/// <summary>
		/// Called before removing nodes fron the curve.
		/// </summary>
		/// <param name="nodes">Nodes to remove.</param>
		/// <param name="index">Index of the nodes in the curve.</param>
		/// <param name="curveIds">Ids of the curves to remove nodes from.</param>
		void OnBeforeRemoveNodes (List<BezierNode> nodes, List<int> index, List<System.Guid> curveIds) {
			Undo.RecordObject (structureGeneratorNode.structureGeneratorElement, "Remove Nodes");
		}
		/// <summary>
		/// Called before removing nodes fron the curve.
		/// </summary>
		/// <param name="nodes">Nodes to remove.</param>
		/// <param name="index">Index of the nodes in the curve.</param>
		/// <param name="curveIds">Ids of the curves to remove nodes from.</param>
		void OnRemoveNodes (List<BezierNode> nodes, List<int> index, List<System.Guid> curveIds) {
			SetUndoControlCounter (false);
			GetTunedBranches ();
			ApplySerialized ();
			UpdatePipeline (GlobalSettings.processingDelayMedium);
		}
		/// <summary>
		/// Called when a handle begins to move.
		/// </summary>
		/// <param name="node">Node owner of the handle.</param>
		/// <param name="index">Index of the node.</param>
		/// <param name="curveId">Id of the curve the node.</param>
		/// <param name="handle">Number of the handle.</param>
		bool OnBeginMoveHandle (BezierNode node, int index, System.Guid curveId, int handle) {
			Undo.RecordObject (structureGeneratorNode.structureGeneratorElement, "Move Handle");
			return true;
		}
		/// <summary>
		/// Called when a handle of a node is moved.
		/// </summary>
		/// <param name="node">Node owner of the handle.</param>
		/// <param name="index">Index of the node.</param>
		/// <param name="curveId">Id of the curve the node.</param>
		/// <param name="handle">Number of the handle.</param>
		bool OnMoveHandle (BezierNode node, int index, System.Guid curveId, int handle) {
			structureGeneratorComponent.CommitBranchCurve (_editStructureId, index, Vector3.zero, true);
			GetTunedBranches ();
			ApplySerialized ();
			UpdatePipeline (GlobalSettings.processingDelayLow);
			return true;
		}
		/// <summary>
		/// Called when a handle of a node is moved.
		/// </summary>
		/// <param name="node">Node owner of the handle.</param>
		/// <param name="index">Index of the node.</param>
		/// <param name="curveId">Id of the curve the node.</param>
		/// <param name="handle">Number of the handle.</param>
		bool OnEndMoveHandle (BezierNode node, int index, System.Guid curveId, int handle) {
			SetUndoControlCounter (false);
			return true;
		}
		/// <summary>
		/// Called to check if a node should draw move controls.
		/// </summary>
		/// <param name="node">Node to check.</param>
		/// <param name="index">Index of the node.</param>
		/// <param name="curveId">Id of the curve the node belongs to.</param>
		/// <returns>True if the node should be drawn.</returns>
		BezierCurveEditor.ControlType OnCheckNodeMoveControls (BezierNode node, int index, System.Guid curveId) {
			if (index == 0 && _editStructure != null) {
				if (!_editStructure.branch.IsFollowUp () || _editStructure.branch.parent == null || curveEditor.hasMultipleSelection) {
					return BezierCurveEditor.ControlType.DrawOnly;
				} else if (_editStructure.branch.IsFollowUp ()) {
					return BezierCurveEditor.ControlType.None;
				}
			}
			return BezierCurveEditor.ControlType.FreeMove;
		}
		#endregion
	}
}
