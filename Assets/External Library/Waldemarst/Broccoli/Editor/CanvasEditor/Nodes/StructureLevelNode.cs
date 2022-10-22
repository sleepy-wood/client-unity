using System;

using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;

using Broccoli.Pipe;
using Broccoli.Generator;
using Broccoli.Utils;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Structure level node.
	/// Used for branch and sprout level structure canvas.
	/// </summary>
	[Node (false, "Structure Level", 0, new System.Type[] {typeof(StructureCanvas)})]
	public class StructureLevelNode : Broccoli.NodeEditorFramework.Node 
	{
		#region Vars
		/// <summary>
		/// The width of the node.
		/// </summary>
		public static float nodeWidth = 40f;
		/// <summary>
		/// The height of the node.
		/// </summary>
		public static float nodeHeight = 40f;
		/// <summary>
		/// The node is root of the structure.
		/// </summary>
		public bool isRoot = false;
		/// <summary>
		/// The size of the rect when drawing this node.
		/// </summary>
		protected Vector2 rectSize = new Vector2 (nodeWidth, nodeHeight);
		/// <summary>
		/// The structure level.
		/// </summary>
		public StructureGenerator.StructureLevel structureLevel;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public virtual string nodeName {
			get { return "Structure Level"; }	
		}
		/// <summary>
		/// Gets the get Id of the node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (StructureLevelNode).ToString(); } 
		}
		/// <summary>
		/// The color of the range bar.
		/// </summary>
		private static Color rangeBarColor = new Color (0.3f, 0.9f, 0.3f, 0.5f);
		/// <summary>
		/// The color of the shared probability bar.
		/// </summary>
		private static Color sharedProbBarColor = new Color (1f, 0.2f, 0.3f, 0.9f);
		/// <summary>
		/// The node tint.
		/// </summary>
		private static Color nodeTint = new Color (0.7f, 0.7f, 0.7f, 0.9f);
		/// <summary>
		/// The visted node tint.
		/// </summary>
		private static Color visitedNodeTint = new Color (1f, 1f, 1f, 1f);
		/// <summary>
		/// The selected node tint.
		/// </summary>
		private static Color selectedNodeTint = new Color (0.3f, 0.3f, 0.8f, 0.35f);
		#endregion

		#region Node Creation
		/// <summary>
		/// Create an instance of this Node at the given position
		/// </summary>
		/// <param name="pos">Position.</param>
		public override Broccoli.NodeEditorFramework.Node Create (Vector2 pos) 
		{
			StructureLevelNode node = ScriptableObject.CreateInstance<StructureLevelNode>();
			InitNode (node, pos);
			return node;
		}
		/// <summary>
		/// Create the a Node of the type specified by the nodeID at position
		/// </summary>
		public static Broccoli.NodeEditorFramework.Node Create (string nodeID, 
			Vector2 position, 
			StructureGenerator.StructureLevel structureLevel = null) 
		{
			StructureLevelNode node = NodeTypes.getDefaultNode (nodeID) as StructureLevelNode; // TODO: this create node is called two times.
			if (node == null)
				throw new UnityException ("Cannot create Node as ID '" + nodeID + "' is not registered!!!");

			node = node.Create (position) as StructureLevelNode;

			if (node == null)
				return null;

			if (structureLevel == null) {
				node.isRoot = true;
			}
			node.structureLevel = structureLevel;

			StructureCanvas.GetInstance ().nodes.Add (node);
			node.canvas = StructureCanvas.GetInstance ();
			//node.InitBase ();

			//NodeEditorCallbacks.IssueOnAddNode (node);
			//NodeEditor.curNodeCanvas.Validate ();
			return node;
		}
		/// <summary>
		/// Inits the node.
		/// </summary>
		/// <param name="node">Node.</param>
		/// <param name="pos">Position.</param>
		private void InitNode (StructureLevelNode node, Vector2 pos) {
			node.rect = new Rect (pos.x, pos.y, rectSize.x, rectSize.y);
			node.name = GetID;
		}
		#endregion

		#region Draw
		/// <summary>
		/// Draws the node frame and calls NodeGUI. Can be overridden to customize drawing.
		/// </summary>
		protected override void DrawNode () 
		{
			AssureNodeBGStyle ();

			if (structureLevel != null && !structureLevel.isDrawVisible) {
				GUI.color = new Color (1f, 1f, 1f, 0.5f);
			}

			// Create a rect that is adjusted to the editor zoom and pixel perfect
			Rect nodeRect = rect;
			Vector2 pos = NodeEditor.curEditorState.zoomPanAdjust + NodeEditor.curEditorState.panOffset;
			nodeRect.position = new Vector2((int)(nodeRect.x+pos.x), (int)(nodeRect.y+pos.y));

			// Begin the body frame around the NodeGUI
			Rect bodyRect = new Rect (nodeRect.x, nodeRect.y + contentOffset.y, nodeRect.width, nodeRect.height - contentOffset.y);
			GUI.BeginGroup (bodyRect, nodeBGStyle);
			bodyRect.position = Vector2.zero;

			GUILayout.BeginArea (bodyRect);
			AssureIsSelected ();
			// Call NodeGUI
			GUI.changed = false;
			NodeGUI ();
			// End NodeGUI frame
			GUILayout.EndArea ();
			GUI.EndGroup ();

			if (structureLevel != null && !structureLevel.isDrawVisible) {
				GUI.color = Color.white;
			}
		}
		/// <summary>
		/// Assures the node background style.
		/// </summary>
		private void AssureNodeBGStyle ()
		{
			if (nodeBGStyle == null) {
				nodeBGStyle = new GUIStyle (GUI.skin.box);
			}
			if (structureLevel == null || structureLevel.isVisitedAndActive) {
				nodeBGStyle.normal.background = ResourceManager.GetTintedTexture ("Broccoli/GUI/NE_Box_Selected.png", visitedNodeTint);
			} else {
				nodeBGStyle.normal.background = ResourceManager.GetTintedTexture ("Broccoli/GUI/NE_Box.png", nodeTint);
			}
		}
		/// <summary>
		/// Assures the node is selected.
		/// </summary>
		private void AssureIsSelected () {
			if (StructureCanvas.GetInstance ().structureGeneratorElement.selectedLevel == structureLevel) {
				EditorGUI.DrawRect (new Rect (-1, -1, rectSize.x, rectSize.y), selectedNodeTint);
			}
		}
		/// <summary>
		/// Draws the nodeKnobs.
		/// </summary>
		protected override void DrawKnobs () 
		{
			CheckNodeKnobMigration ();
			for (int knobCnt = 0; knobCnt < nodeKnobs.Count; knobCnt++) {
				//nodeKnobs [knobCnt].DrawKnob ();
				if (structureLevel != null &&
					structureLevel.IsSharedNotMain () &&
				    nodeKnobs [knobCnt].side == NodeSide.Bottom) {
					// Don't draw bottom knob, node is 
				} else {
					nodeKnobs [knobCnt].DrawKnob ();
				}
			}
		}
		/// <summary>
		/// Draws the node curves
		/// </summary>
		protected override void DrawConnections () 
		{
			CheckNodeKnobMigration ();
			if (Event.current.type != EventType.Repaint)
				return;
			for (int outCnt = 0; outCnt < Outputs.Count; outCnt++) 
			{
				NodeOutput output = Outputs [outCnt];
				Vector2 startPos = output.GetGUIKnob ().center;
				Vector2 startDir = output.GetDirection ();

				for (int conCnt = 0; conCnt < output.connections.Count; conCnt++) 
				{
					NodeInput input = output.connections [conCnt];
					StructureLevelNode structureLevelNode = (StructureLevelNode)input.body;
					// Not drawing connections to non group representant shared nodes.
					if (!structureLevelNode.structureLevel.IsSharedNotMain ()) {
						Vector2 endPos = input.GetGUIKnob ().center;
						Vector2 endDir = input.GetDirection ();
						NodeEditorGUI.OptimiseBezierDirections (startPos, ref startDir, endPos, ref endDir);
						if (structureLevelNode.structureLevel.isVisited) {
							NodeEditorGUI.DrawConnection (startPos, startDir, endPos, endDir, Color.white, output.typeData.LineWidth);
						} else {
							NodeEditorGUI.DrawConnection (startPos, startDir, endPos, endDir, output.typeData.Color, output.typeData.LineWidth);
						}
					}
				}
			}
		}
		/// <summary>
		/// Draw the Node immediately.
		/// </summary>
		protected override void NodeGUI () {
			if (structureLevel != null) {
				// Level visibility icon.
				if (structureLevel.enabled) {
					GUI.DrawTexture (new Rect (rectSize.x - 14, 0, 14, 14), 
						GUITextureManager.visibilityOnTexture, ScaleMode.ScaleToFit);
				} else {
					GUI.DrawTexture (new Rect (rectSize.x - 14, 0, 14, 14), 
						GUITextureManager.visibilityOffTexture, ScaleMode.ScaleToFit);
				}

				// Draw sprout/branch icon.
				if (structureLevel.isSprout) {
					GUI.DrawTexture (new Rect (12, rectSize.y - 28, 24, 24), GUITextureManager.GetNodeBgSprout (), ScaleMode.ScaleToFit);
				} else if (structureLevel.isRoot) {
					GUI.DrawTexture (new Rect (12, rectSize.y - 28, 24, 24), GUITextureManager.GetNodeBgRoot (), ScaleMode.ScaleToFit);
				} else {
					GUI.DrawTexture (new Rect (12, rectSize.y - 28, 24, 24), GUITextureManager.GetNodeBgBranch (), ScaleMode.ScaleToFit);
				}

				// Draw action range.
				DrawActionRange (structureLevel.minRange<structureLevel.minMaskRange?structureLevel.minRange:structureLevel.minMaskRange, 
					structureLevel.maxRange>structureLevel.maxMaskRange?structureLevel.maxRange:structureLevel.maxMaskRange);

				// Draw shared probability.
				if (structureLevel.IsShared ()) {
					DrawSharedProbability (structureLevel.sharedProbability);
				}

				// Draw probability.
				DrawProbability (structureLevel.probability);

				// Draw assigned sprout group.
				if (structureLevel.isSprout) {
					DrawSproutGroup ();
				}
			} else {
				GUI.DrawTexture (new Rect (4, rectSize.y - 36, 32, 32), GUITextureManager.GetNodeBgTrunk (), ScaleMode.ScaleToFit);
			}
		}
		/// <summary>
		/// Draws the action range bar.
		/// </summary>
		/// <param name="minRangeLimit">Minimum range limit.</param>
		/// <param name="maxRangeLimit">Max range limit.</param>
		protected void DrawActionRange (float minRangeLimit, float maxRangeLimit) {
			float minRangePos = (rectSize.y - 2) * (1f - minRangeLimit);
			float maxRangePos = (rectSize.y - 2) * (1f - maxRangeLimit);
			EditorGUI.DrawRect (new Rect (1, maxRangePos + 1, 4, minRangePos - maxRangePos), rangeBarColor);
		}
		/// <summary>
		/// Draws the shared probability of occurence for this node.
		/// </summary>
		/// <param name="sharedProbability">Shared probability.</param>
		protected void DrawSharedProbability (float sharedProbability) {
			float maxRangePos = (rectSize.y - 2) * sharedProbability;
			EditorGUI.DrawRect (new Rect (5, 1, 4, maxRangePos), sharedProbBarColor);
		}
		/// <summary>
		/// Draws the probability of ocurrence.
		/// </summary>
		/// <param name="probability">Probability.</param>
		protected void DrawProbability (float probability) {
			EditorGUI.LabelField (new Rect (10, 1, 20, 20), 
				Math.Round((double)probability, 2).ToString(), TreeCanvasGUI.smallNodeLabel);
		}
		/// <summary>
		/// Draws the sprout group.
		/// </summary>
		protected void DrawSproutGroup () {
			//SproutGroups.SproutGroup sproutGroup = 
			EditorGUI.DrawRect (new Rect (rectSize.x - 8, rectSize.y - 8, 7, 7), 
				structureLevel.sproutGroupColor);
		}
		#endregion
	}

	/// <summary>
	/// Connection Type only for visual purposes.
	/// </summary>
	public class StructureLevelType : IConnectionTypeDeclaration 
	{
		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string Identifier { get { return "StructureLevel"; } }
		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		public System.Type Type { get { return typeof(void); } }
		/// <summary>
		/// Gets the color of the connection.
		/// </summary>
		/// <value>The color.</value>
		public Color Color { get { return Color.gray; } }
		/// <summary>
		/// Gets the in knob texture.
		/// </summary>
		/// <value>The in knob texture.</value>
		public string InKnobTex { get { return "Broccoli/GUI/In_Knob_Structure.png"; } }
		/// <summary>
		/// Gets the out knob texture.
		/// </summary>
		/// <value>The out knob texture.</value>
		public string OutKnobTex { get { return "Broccoli/GUI/Out_Knob_Structure.png"; } }
		/// <summary>
		/// Gets the width of the line.
		/// </summary>
		/// <value>The width of the line.</value>
		public int LineWidth { get { return 2; } }
	}
}