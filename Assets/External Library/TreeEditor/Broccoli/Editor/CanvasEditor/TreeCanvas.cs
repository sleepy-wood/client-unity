using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.Pipe;
using Broccoli.Factory;
using Broccoli.Utils;

namespace Broccoli.TreeNodeEditor
{
	using Pipeline = Broccoli.Pipe.Pipeline;
	using NodeCanvas = Broccoli.NodeEditorFramework.NodeCanvas;
	/// <summary>
	/// Tree canvas.
	/// </summary>
	[NodeCanvasType("Broccoli Canvas")]
	public class TreeCanvas : NodeCanvas
	{
		#region Vars
		/// <summary>
		/// Building pipeline.
		/// </summary>
		public Broccoli.Pipe.Pipeline pipeline = null;
		/// <summary>
		/// Factory used to build the pipeline.
		/// </summary>
		public TreeFactory treeFactory = null;
		/// <summary>
		/// Flag to mark the canvas as dirty.
		/// </summary>
		public bool isDirty = false;
		/// <summary>
		/// Flag to set status of the canvas as loading a pipeline.
		/// </summary>
		public bool isLoadingPipeline = false;
		/// <summary>
		/// Gets the name of the canvas.
		/// </summary>
		/// <value>The name of the canvas.</value>
		public override string canvasName { get { return "Tree Graph"; } }
		/// <summary>
		/// Relationship between pipeline element id and canvas nodes
		/// </summary>
		Dictionary<int, BaseNode> idToNode = new Dictionary<int, BaseNode> ();
		/// <summary>
		/// Last value of undo count registered from a processed pipeline.
		/// </summary>
		public int lastUndoProcessed = 0;
		/// <summary>
		/// The current undo group.
		/// </summary>
		int currentUndoGroup = -1;
		#endregion

		#region Events
		/// <summary>
		/// Raises the create event.
		/// </summary>
		protected override void OnCreate () 
		{
			Traversal = new TreeCanvasTraversal (this);
		}
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		private void OnEnable () 
		{
			// Register to other callbacks
			//NodeEditorCallbacks.OnEditorStartUp = null;
			//NodeEditorCallbacks.OnEditorStartUp += LoadPipeline;
			NodeEditorCallbacks.OnAddNode = null;
			NodeEditorCallbacks.OnAddNode += OnAddNode;
			NodeEditorCallbacks.OnDeleteNode = null;
			NodeEditorCallbacks.OnDeleteNode += OnDeleteNode;
			NodeEditorCallbacks.OnMoveNode [this.GetType ()] = null;
			NodeEditorCallbacks.OnMoveNode [this.GetType ()] += OnMoveNode;
			NodeEditorCallbacks.OnSelectNode [this.GetType ()] = null;
			NodeEditorCallbacks.OnSelectNode [this.GetType ()] += OnSelectNode;
			NodeEditorCallbacks.OnDeselectNode [this.GetType ()] = null;
			NodeEditorCallbacks.OnDeselectNode [this.GetType ()] += OnDeselectNode;
			NodeEditorCallbacks.OnAddConnection [this.GetType ()] = null;
			NodeEditorCallbacks.OnAddConnection [this.GetType ()] += OnAddConnection;
			NodeEditorCallbacks.OnRemoveConnection [this.GetType ()] = null;
			NodeEditorCallbacks.OnRemoveConnection [this.GetType ()] += OnRemoveConnection;
			NodeEditorCallbacks.OnPanCanvas [this.GetType ()] = null;
			NodeEditorCallbacks.OnPanCanvas [this.GetType ()] += OnPanCanvas;
			bool undoRedoPerformedExists = false;
			System.Delegate[] invocations = Undo.undoRedoPerformed.GetInvocationList ();
			for (int i = 0; i < invocations.Length; i++) {
				if (invocations[i].Method.Name == "OnBroccoliUndoRedoPerformed") {
					undoRedoPerformedExists = true;
				}
			}
			if (!undoRedoPerformedExists) {
				Undo.undoRedoPerformed -= OnBroccoliUndoRedoPerformed;
				Undo.undoRedoPerformed += OnBroccoliUndoRedoPerformed;
			}
		}
		/// <summary>
		/// Raises the disable event.
		/// </summary>
		private void OnDisable () {
			//Undo.undoRedoPerformed -= OnUndoRedoPerformed;
		}
		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		private void OnDestroy () {
			NodeEditorCallbacks.OnAddNode -= OnAddNode;
			NodeEditorCallbacks.OnDeleteNode -= OnDeleteNode;
			NodeEditorCallbacks.OnMoveNode [this.GetType ()] -= OnMoveNode;
			NodeEditorCallbacks.OnSelectNode [this.GetType ()] -= OnSelectNode;
			NodeEditorCallbacks.OnDeselectNode [this.GetType ()] -= OnDeselectNode;
			NodeEditorCallbacks.OnAddConnection [this.GetType ()] -= OnAddConnection;
			NodeEditorCallbacks.OnRemoveConnection [this.GetType ()] -= OnRemoveConnection;
			NodeEditorCallbacks.OnPanCanvas [this.GetType ()] -= OnPanCanvas;
			Undo.undoRedoPerformed -= OnBroccoliUndoRedoPerformed;
			idToNode.Clear ();
			TreeCanvasConnect.GetInstance ().Clear ();
		}
		/// <summary>
		/// Raises the validate event.
		/// </summary>
		protected override void OnValidate ()
		{
			if (Traversal == null)
				Traversal = new TreeCanvasTraversal (this);
		}
		#endregion

		#region Canvas Population
		/// <summary>
		/// Loads the set pipeline as nodes on the canvas.
		/// </summary>
		public void LoadPipeline () {
			isLoadingPipeline = true;
			idToNode.Clear ();
			if (pipeline != null) {
				List<PipelineElement> pipelineElements = pipeline.GetElements ();
				for (int i = 0; i < pipelineElements.Count; i++) {
					if (!pipelineElements[i].isMarkedForDeletion && !idToNode.ContainsKey (pipelineElements[i].id)) {
						CreateAndConnectNode (pipelineElements[i]);
					}
				}
				lastUndoProcessed = pipeline.undoControl.canvasUndoCount;
			}
			isLoadingPipeline = false;
			// Set pan offset.
			editorStates[0].panOffset = treeFactory.treeFactoryPreferences.canvasOffset;
		}
		/// <summary>
		/// Clears the canvas.
		/// </summary>
		public void ClearCanvas () {
			for (int i = 0; i < nodes.Count; i++) {
				DestroyImmediate (nodes[i]);
			}
			nodes.Clear ();
			groups.Clear ();
			idToNode.Clear ();
		}
		/// <summary>
		/// Updates the pipeline nodes on the canvas.
		/// </summary>
		public void UpdatePipeline () {
			// If nodes have been added or deleted then load the pipeline again.
			bool reloadPipeline = false;
			if (nodes.Count != pipeline.GetElementsCount ()) {
				reloadPipeline = true;
			} else {
				List<PipelineElement> pipelineElements = pipeline.GetElements ();
				for (int i = 0; i < pipelineElements.Count; i++) {
					if (!idToNode.ContainsKey (pipelineElements[i].id)) {
						reloadPipeline = true;
					}
				}
			}
			if (reloadPipeline) {
				this.nodes.Clear ();
				this.groups.Clear ();
				LoadPipeline ();
				return;
			}
			
			lastUndoProcessed = pipeline.undoControl.canvasUndoCount;

			// Reposition
			var enumerator = idToNode.GetEnumerator();
			while (enumerator.MoveNext ()) {
				var idToNodeValue = enumerator.Current.Value;
				if (idToNodeValue.pipelineElement != null) {
					idToNodeValue.rect.position = idToNodeValue.pipelineElement.nodePosition;
				}
			}

			// Disconnect
			enumerator = idToNode.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				var idToNodeValue = enumerator.Current.Value;
				for (int i = 0; i < idToNodeValue.Inputs.Count; i++) {
					idToNodeValue.Inputs[i].RemoveConnection (false);
				}
			}

			// Reconnect
			enumerator = idToNode.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				var idToNodeValue = enumerator.Current.Value;
				if (idToNodeValue.pipelineElement.sinkElementId >= 0) {
					ConnectSrcToSinkNode (idToNodeValue, idToNode [idToNodeValue.pipelineElement.sinkElementId]);
				}
			}

			pipeline.Validate ();
		}
		/// <summary>
		/// Creates and connects nodes.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		/// <param name="parentNode">Parent node.</param>
		private void CreateAndConnectNode (PipelineElement pipelineElement, BaseNode parentNode = null) {
			if (!idToNode.ContainsKey (pipelineElement.id)) {
				BaseNode node = CreateNode (pipelineElement);
				idToNode.Add (pipelineElement.id, node);
				if (parentNode != null) {
					ConnectSrcToSinkNode (parentNode, node);
				}
				if (pipelineElement.sinkElement != null) {
					CreateAndConnectNode (pipelineElement.sinkElement, node);
				}
			}
		}
		/// <summary>
		/// Determines whether this instance can add node the specified nodeID.
		/// </summary>
		/// <returns><c>true</c> if this instance can add node the specified nodeID; otherwise, <c>false</c>.</returns>
		/// <param name="nodeID">Node identifier.</param>
		public override bool CanAddNode (string nodeID)
		{
			return true;
		}
		/// <summary>
		/// Creates the node from a pipeline.
		/// </summary>
		/// <returns>The node.</returns>
		/// <param name="pipelineElement">Pipeline element.</param>
		BaseNode CreateNode (PipelineElement pipelineElement) {
			BaseNode baseNode = null;
			switch (pipelineElement.classType) {
			case PipelineElement.ClassType.StructureGenerator:
				baseNode = BaseNode.Create (typeof(StructureGeneratorNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.LSystem:
				baseNode = BaseNode.Create (typeof(LSystemGraphNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.BranchMeshGenerator:
				baseNode = BaseNode.Create (typeof(BranchMeshGeneratorNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.TrunkMeshGenerator:
				baseNode = BaseNode.Create (typeof(TrunkMeshGeneratorNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.GirthTransform:
				baseNode = BaseNode.Create (typeof(GirthTransformNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.BranchBender:
				baseNode = BaseNode.Create (typeof(BranchBenderNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.LengthTransform:
				baseNode = BaseNode.Create (typeof(LengthTransformNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.SparsingTransform:
				baseNode = BaseNode.Create (typeof(SparsingTransformNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.SproutGenerator:
				baseNode = BaseNode.Create (typeof(SproutGeneratorNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.SproutMeshGenerator:
				baseNode = BaseNode.Create (typeof(SproutMeshGeneratorNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.BranchMapper:
				baseNode = BaseNode.Create (typeof(BranchMapperNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.ProceduralBranchMapper:
				baseNode = BaseNode.Create (typeof(ProceduralBranchMapperNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.SproutMapper:
				baseNode = BaseNode.Create (typeof(SproutMapperNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.WindEffect:
				baseNode = BaseNode.Create (typeof(WindEffectNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.Positioner:
				baseNode = BaseNode.Create (typeof(PositionerNode).ToString (), pipelineElement) as BaseNode;
				break;
			case PipelineElement.ClassType.Baker:
				baseNode = BaseNode.Create (typeof(BakerNode).ToString (), pipelineElement) as BaseNode;
				break;
			}
			return baseNode;
		}
		/// <summary>
		/// Connects the src node to the sink node.
		/// </summary>
		/// <param name="srcNode">Source node.</param>
		/// <param name="sinkNode">Sink node.</param>
		void ConnectSrcToSinkNode (BaseNode srcNode, BaseNode sinkNode) {
			if (srcNode != null && sinkNode != null && srcNode.Outputs.Count > 0) {
				sinkNode.Inputs [0].TryApplyConnection (srcNode.Outputs [0], false);
			}
		}
		/// <summary>
		/// Validates to the pipeline.
		/// </summary>
		void ValidatePipeline () {
			treeFactory.ValidatePipeline ();
			for (int i = 0; i < nodes.Count; i++) {
				BaseNode baseNode = (BaseNode)nodes[i]; // TODO: try catch
				if (baseNode.pipelineElement != null && baseNode.pipelineElement.isOnValidPipeline) {
					baseNode.SetInValidPipeline (true);
				} else {
					baseNode.SetInValidPipeline (false);
				}
			}
		}
		/// <summary>
		/// Begins a collapsable undo with the canvas undo count.
		/// </summary>
		void BeginUndoCollapse () {
			currentUndoGroup = Undo.GetCurrentGroup ();
		}
		/// <summary>
		/// collapses an undo with the canvas undo count.
		/// </summary>
		void EndUndoCollapse (Pipeline pipeline) {
			Undo.RecordObject (pipeline, "canvasUndoControl");
			pipeline.undoControl.canvasUndoCount++;
			Undo.CollapseUndoOperations (currentUndoGroup);
			lastUndoProcessed++;
		}
		#endregion

		#region Node Events
		/// <summary>
		/// Raises the add node event.
		/// </summary>
		/// <param name="node">Node.</param>
		public void OnAddNode (Broccoli.NodeEditorFramework.Node node) {
			if (pipeline != null) {
				BaseNode baseNode = node as BaseNode;
				if ( !isLoadingPipeline ) {
					baseNode.SetPipelineElement ();
					baseNode.SetConnectors ();
					baseNode.name = baseNode.nodeName;
				}
				PipelineElement pipelineElement = ((BaseNode)node).pipelineElement;
				pipelineElement.nodePosition = node.rect.position;
				if ( !isLoadingPipeline) {
					BeginUndoCollapse ();
					Undo.RecordObject (pipeline, "Adding " + pipelineElement.name + " node.");
					pipeline.AddElement (pipelineElement);
					idToNode.Add (pipelineElement.id, baseNode);
					EndUndoCollapse (pipelineElement.pipeline);
				}
			}
		}
		/// <summary>
		/// Raises the move node event.
		/// </summary>
		/// <param name="node">Node.</param>
		public void OnMoveNode (Broccoli.NodeEditorFramework.Node node) {
			if (pipeline != null) {
				PipelineElement pipelineElement = ((BaseNode)node).pipelineElement;
				if (pipelineElement.nodePosition != node.rect.position) {
					Undo.IncrementCurrentGroup ();
					BeginUndoCollapse ();
					Undo.RecordObject (pipelineElement, "Moved node " + pipelineElement.name);
					pipelineElement.nodePosition = node.rect.position;
					EndUndoCollapse (pipelineElement.pipeline);
					EditorUtility.SetDirty (pipelineElement);
				}
			}
		}
		/// <summary>
		/// Raises the select node event.
		/// </summary>
		/// <param name="node">Node.</param>
		public void OnSelectNode (Broccoli.NodeEditorFramework.Node node) {
			if (pipeline != null) {
				PipelineElement pipelineElement = ((BaseNode)node).pipelineElement;
				TreeCanvasConnect.GetInstance ().LookForCandidates (pipelineElement, pipelineElement.pipeline);
				UnityEditor.Selection.activeObject = node;
			}
		}
		/// <summary>
		/// Raises the deselect node event.
		/// </summary>
		public void OnDeselectNode () {
			if (pipeline != null) {
				TreeCanvasConnect.GetInstance ().Clear ();
			}
		}
		/// <summary>
		/// Raises the delete node event.
		/// </summary>
		/// <param name="node">Node.</param>
		public void OnDeleteNode (Broccoli.NodeEditorFramework.Node node) {
			if (pipeline != null) {
				PipelineElement pipelineElement = ((BaseNode)node).pipelineElement;
				/*
				 * Couldn't get undo to work for this operation without compromising the architecture too much.
				 */
				pipelineElement.isMarkedForDeletion = true;
				if (idToNode.ContainsKey (pipelineElement.id)) {
					idToNode.Remove (pipelineElement.id);
				}

				TreeCanvasConnect.GetInstance ().LookForCandidates ();
				ValidatePipeline ();
			}
		}
		/// <summary>
		/// Raises the add connection event.
		/// </summary>
		/// <param name="input">Input.</param>
		public void OnAddConnection (NodeInput input) {
			if (input.connection.connections.Count > 0) {
				BaseNode srcNode = input.connection.body as BaseNode;
				BaseNode sinkNode = input.connection.connections [0].body as BaseNode;
				if (srcNode.pipelineElement.positionWeight < sinkNode.pipelineElement.positionWeight || 
					(!srcNode.pipelineElement.uniqueOnPipeline && 
						srcNode.pipelineElement.positionWeight == sinkNode.pipelineElement.positionWeight)) 
				{
					Undo.IncrementCurrentGroup ();
					BeginUndoCollapse ();

					srcNode.pipelineElement.sinkElementId = -1;
					sinkNode.pipelineElement.srcElementId = -1;
					Undo.RecordObject (srcNode.pipelineElement, "Adding node connection");
					srcNode.pipelineElement.sinkElementId = sinkNode.pipelineElement.id;
					sinkNode.pipelineElement.srcElementId = srcNode.pipelineElement.id;
					srcNode.pipelineElement.sinkElement = sinkNode.pipelineElement;
					sinkNode.pipelineElement.srcElement = srcNode.pipelineElement;

					EndUndoCollapse (srcNode.pipelineElement.pipeline);
					TreeCanvasConnect.GetInstance ().LookForCandidates ();
					ValidatePipeline ();
					if (treeFactory.localPipeline.IsValid ()) {
						treeFactory.ProcessPipelinePreview (treeFactory.localPipeline.root, true, true);
					}
				} else {
					input.RemoveConnection ();
					treeFactory.AddLogWarn ("Invalid node connection.");
				}
			}
		}
		/// <summary>
		/// Raises the remove connection event.
		/// </summary>
		/// <param name="input">Input.</param>
		public void OnRemoveConnection (NodeInput input) {
			if (input.connection.connections.Count > 0) {
				BaseNode srcNode = input.connection.body as BaseNode;
				BaseNode sinkNode = input.connection.connections [0].body as BaseNode;

				Undo.IncrementCurrentGroup ();
				BeginUndoCollapse ();

				Undo.RecordObject (srcNode.pipelineElement, "Removing node connection");
				srcNode.pipelineElement.sinkElement = null;
				srcNode.pipelineElement.sinkElementId = -1;
				sinkNode.pipelineElement.srcElement = null;
				sinkNode.pipelineElement.srcElementId = -1;
				EndUndoCollapse (srcNode.pipelineElement.pipeline);

				TreeCanvasConnect.GetInstance ().LookForCandidates ();
				ValidatePipeline ();
			}
		}
		/// <summary>
		/// Raises the pan canvas event.
		/// </summary>
		/// <param name="nodeCanvas">Node canvas.</param>
		public void OnPanCanvas (NodeCanvas nodeCanvas) {
			if (treeFactory != null) {
				treeFactory.treeFactoryPreferences.canvasOffset = nodeCanvas.editorStates [0].panOffset;
				isDirty = true;
			}
		}
		/// <summary>
		/// Raises the undo redo performed event.
		/// </summary>
		void OnBroccoliUndoRedoPerformed () {
			if (pipeline != null && pipeline.undoControl.canvasUndoCount != lastUndoProcessed) {
				UpdatePipeline ();
			}
		}
		#endregion
	}
}