using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.NodeEditorFramework;
using Broccoli.Pipe;
using Broccoli.Generator;

namespace Broccoli.TreeNodeEditor
{
	using NodeCanvas = Broccoli.NodeEditorFramework.NodeCanvas;
	/// <summary>
	/// Tree canvas.
	/// </summary>
	[NodeCanvasType("Structure Canvas")]
	public class StructureCanvas : NodeCanvas
	{
		#region Vars
		/// <summary>
		/// Gets the name of the canvas.
		/// </summary>
		/// <value>The name of the canvas.</value>
		public override string canvasName { get { return "Structure Graph"; } }
		/// <summary>
		/// The structure generator element to populate the canvas.
		/// </summary>
		public StructureGeneratorElement structureGeneratorElement = null;
		/// <summary>
		/// Main structure node.
		/// </summary>
		public StructureLevelNode rootNode = null;
		/// <summary>
		/// The width of the nodes.
		/// </summary>
		private static float nodeWidth = 40f;
		/// <summary>
		/// The height of the canvas rect.
		/// </summary>
		public float canvasRectHeight = 100f;
		/// <summary>
		/// Flag to mark the canvas as dirty.
		/// </summary>
		public bool isDirty = false;
		/// <summary>
		/// The nodes to connect when loading the structure element.
		/// </summary>
		List<StructureLevelNode> nodesToConnect = new List<StructureLevelNode> ();
		/// <summary>
		/// Id to nodes dictionary.
		/// </summary>
		Dictionary<int, StructureLevelNode> idToNode = new Dictionary<int, StructureLevelNode> ();
		public delegate void OnSelectNodeDelegate (Broccoli.NodeEditorFramework.Node node);
		public delegate void OnDeselectNodeDelegate ();
		public OnSelectNodeDelegate onSelectNode;
		public OnDeselectNodeDelegate onDeselectNode;
		#endregion

		#region Singleton
		/// <summary>
		/// The structure canvas singleton.
		/// </summary>
		private static StructureCanvas _structureCanvas;
		/// <summary>
		/// Gets the singleton instance.
		/// </summary>
		/// <returns>The instance.</returns>
		public static StructureCanvas GetInstance () {
			if (_structureCanvas == null) {
				_structureCanvas = ScriptableObject.CreateInstance<StructureCanvas> ();
				//_structureCanvas.moveNodeEnabled = false;
				_structureCanvas.editConnectionEnabled = false;
				_structureCanvas.zoomEnabled = false;
				_structureCanvas.showCanvasMenuEnabled = false;
				_structureCanvas.showNodeMenuEnabled = false;
			}
			return _structureCanvas;
		}
		#endregion

		#region Events
		/// <summary>
		/// Raises the create event on the canvas.
		/// </summary>
		protected override void OnCreate () { }
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		private void OnEnable () {
			NodeEditorCallbacks.OnSelectNode[this.GetType ()] = null;
			NodeEditorCallbacks.OnSelectNode[this.GetType ()] += OnSelectNode;
			NodeEditorCallbacks.OnDeselectNode[this.GetType ()] = null;
			NodeEditorCallbacks.OnDeselectNode[this.GetType ()] += OnDeselectNode;
			NodeEditorCallbacks.OnMoveNode[this.GetType ()] = null;
			NodeEditorCallbacks.OnMoveNode[this.GetType ()] += OnMoveNode;
			NodeEditorCallbacks.OnDraggingNode[this.GetType ()] = null;
			NodeEditorCallbacks.OnDraggingNode[this.GetType ()] += OnDraggingNode;
			NodeEditorCallbacks.OnPanCanvas[this.GetType ()] = null;
			NodeEditorCallbacks.OnPanCanvas[this.GetType ()] += OnPanCanvas;
		}
		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		private void OnDestroy () {
			structureGeneratorElement = null;
		}
		/// <summary>
		/// Raises the validate event.
		/// </summary>
		protected override void OnValidate () { }
		#endregion

		#region Ops
		/// <summary>
		/// Loads the structure generator.
		/// </summary>
		/// <param name="structureGeneratorElement">Structure generator element.</param>
		public void LoadStructureGenerator (StructureGeneratorElement structureGeneratorElement) {
			bool downOutputSet = false;
			Clear ();
			this.structureGeneratorElement = structureGeneratorElement;
			
			// Create root node.
			Vector2 rootNodePosition = structureGeneratorElement.rootStructureLevel.nodePosition;
			rootNode = StructureLevelNode.Create (typeof(StructureLevelNode).ToString (), 
				rootNodePosition) as StructureLevelNode;
			rootNode.CreateOutput ("Output", "StructureLevel", NodeSide.Top, nodeWidth / 2f);

			// Add top levels.
			nodesToConnect.Clear ();
			for (int i = 0; i < structureGeneratorElement.structureLevels.Count; i++) {
				if (!structureGeneratorElement.structureLevels [i].isRoot) {
					nodesToConnect.Add (CreateNode (structureGeneratorElement.structureLevels[i], 1));
				}
			}
			ConnectNodes (rootNode, nodesToConnect);

			// Add bottom levels.
			nodesToConnect.Clear ();
			for (int i = 0; i < structureGeneratorElement.structureLevels.Count; i++) {
				if (structureGeneratorElement.structureLevels [i].isRoot) {
					if (!downOutputSet) {
						rootNode.CreateOutput ("DownOutput", "StructureLevel", NodeSide.Bottom, nodeWidth / 2f);
						downOutputSet = true;
					}
					nodesToConnect.Add (CreateNode (structureGeneratorElement.structureLevels[i], 1, true));
				}
			}
			ConnectNodes (rootNode, nodesToConnect, 1);
			nodesToConnect.Clear ();

			// Set pan offset.
			editorStates[0].panOffset = structureGeneratorElement.canvasOffset;
		}
		/// <summary>
		/// Creates a node for a structure level.
		/// </summary>
		/// <returns>The node.</returns>
		/// <param name="structureLevel">Structure level.</param>
		/// <param name="level">Level.</param>
		private StructureLevelNode CreateNode (StructureGenerator.StructureLevel structureLevel, int level = 0, bool isRoot = false) {
			List<StructureLevelNode> nodes = new List<StructureLevelNode> ();
			for (int i = 0; i < structureLevel.structureLevels.Count; i++) {
				nodes.Add (CreateNode (structureLevel.structureLevels[i], level + 1, isRoot));
			}
			Vector2 posNode = structureLevel.nodePosition;
			StructureLevelNode node = 
				StructureLevelNode.Create (typeof(StructureLevelNode).ToString (), posNode) as StructureLevelNode;
			idToNode.Add (structureLevel.id, node);
			if (structureLevel.structureLevels.Count > 0) {
				if (isRoot) {
					node.CreateOutput ("Output", "StructureLevel", NodeSide.Bottom, nodeWidth / 2f);
				} else {
					node.CreateOutput ("Output", "StructureLevel", NodeSide.Top, nodeWidth / 2f);
				}
			}
			if (isRoot) {
				node.CreateInput ("Input", "StructureLevel", NodeSide.Top, nodeWidth / 2f);
			} else {
				node.CreateInput ("Input", "StructureLevel", NodeSide.Bottom, nodeWidth / 2f);
			}
			node.structureLevel = structureLevel;
			ConnectNodes (node, nodes);
			return node;
		}
		/// <summary>
		/// Connects the nodes.
		/// </summary>
		/// <param name="parentNode">Parent node.</param>
		/// <param name="childrenNodes">Children nodes.</param>
		private void ConnectNodes (StructureLevelNode parentNode, List<StructureLevelNode> childrenNodes, int outputIndex = 0) {
			if (parentNode != null && childrenNodes != null) {
				for (int i = 0; i < childrenNodes.Count; i++) {
					childrenNodes[i].Inputs [0].TryApplyConnection (parentNode.Outputs [outputIndex]);
				}
			}
		}
		/// <summary>
		/// Determines whether this instance can add node the specified nodeID.
		/// </summary>
		/// <returns><c>true</c> if this instance can add node the specified nodeID; otherwise, <c>false</c>.</returns>
		/// <param name="nodeID">Node identifier.</param>
		public override bool CanAddNode (string nodeID) {
			return true;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			nodes.Clear ();
			groups.Clear ();
			nodesToConnect.Clear ();
			idToNode.Clear ();
		}
		#endregion

		#region Node Events
		/// <summary>
		/// Raises the select node event.
		/// </summary>
		/// <param name="node">Node.</param>
		void OnSelectNode (Broccoli.NodeEditorFramework.Node node) {
			if (structureGeneratorElement != null) {
				StructureLevelNode levelNode = node as StructureLevelNode;
				structureGeneratorElement.selectedLevel = levelNode.structureLevel;
				if (onSelectNode != null)
                    onSelectNode (node);
			}
		}
		/// <summary>
		/// Raises the deselect node event.
		/// </summary>
		void OnDeselectNode () {
			if (structureGeneratorElement != null) {
				if (onDeselectNode != null) {
					onDeselectNode ();
				}
				structureGeneratorElement.selectedLevel = null;
			}
		}
		/// <summary>
		/// Raises the move node event.
		/// </summary>
		/// <param name="node">Node.</param>
		void OnMoveNode (Broccoli.NodeEditorFramework.Node node) {
			if (structureGeneratorElement != null) {
				StructureGenerator.StructureLevel structureLevel = ( (StructureLevelNode)node).structureLevel;
				if (structureLevel == null) {
					structureGeneratorElement.rootStructureLevel.nodePosition = node.rect.position;
				} else {
					structureLevel.nodePosition = node.rect.position;
				}
				isDirty = true;
			}
		}
		/// <summary>
		/// Raises the dragging node event.
		/// </summary>
		/// <param name="node">Node.</param>
		/// <param name="offset">Offset.</param>
		void OnDraggingNode (Broccoli.NodeEditorFramework.Node node, Vector2 offset) {
			if (structureGeneratorElement != null) {
				StructureLevelNode levelNode = node as StructureLevelNode;
				if (levelNode.structureLevel != null && levelNode.structureLevel.IsShared ()) {
					PropagateSharedNodeDragging (levelNode.structureLevel.GetSharedGroupIdOrMainId (),
						levelNode.structureLevel.id, offset);
				}
			}
		}
		/// <summary>
		/// Raises the pan canvas event.
		/// </summary>
		/// <param name="nodeCanvas">Node canvas.</param>
		void OnPanCanvas (NodeCanvas nodeCanvas) {
			if (structureGeneratorElement != null) {
				structureGeneratorElement.canvasOffset = nodeCanvas.editorStates [0].panOffset;
				isDirty = true;
			}
		}
		#endregion

		#region Shared nodes
		/// <summary>
		/// Propagates the shared node dragging.
		/// </summary>
		/// <param name="mainId">Main node of the shared group identifier.</param>
		/// <param name="elicitingId">Eliciting node identifier.</param>
		/// <param name="offset">Offset.</param>
		void PropagateSharedNodeDragging (int mainId, int elicitingId, Vector2 offset) {
			if (idToNode.ContainsKey (mainId)) {
				StructureLevelNode node = idToNode [mainId];
				do {
					if (node.structureLevel.id != elicitingId) {
						node.rect.position += offset;
						node.structureLevel.nodePosition = node.rect.position;
					}
					if (idToNode.ContainsKey (node.structureLevel.sharingNextId)) {
						node = idToNode [node.structureLevel.sharingNextId];
					} else {
						node = null;
					}
				} while (node != null);
			}
		}
		#endregion
	}
}