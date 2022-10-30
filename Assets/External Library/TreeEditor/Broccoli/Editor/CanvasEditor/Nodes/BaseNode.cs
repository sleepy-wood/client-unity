using UnityEngine;
using UnityEditor;

using Broccoli.Base;
using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;
using Broccoli.Factory;
using Broccoli.Utils;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Base node.
	/// </summary>
	public abstract class BaseNode : Broccoli.NodeEditorFramework.Node 
	{
		#region Vars
		/// <summary>
		/// Category of the node.
		/// </summary>
		public enum Category
		{
			None,
			StructureGenerator,
			StructureTransformer,
			MeshGenerator,
			MeshTransformer,
			Mapper,
			Function
		}
		/// <summary>
		/// Gets the category of the node.
		/// </summary>
		/// <value>Category of the node.</value>
		public virtual Category category { get { return Category.None; } }
		/// <summary>
		/// Gets the color of the node based on its category.
		/// </summary>
		/// <value>The color.</value>
		public Color color {
			get {
				switch (category) {
				case Category.StructureGenerator:
					return TreeCanvasGUI.structureGeneratorNodeColor;
				case Category.StructureTransformer:
					return TreeCanvasGUI.structureTransformerNodeColor;
				case Category.MeshGenerator:
					return TreeCanvasGUI.meshGeneratorNodeColor;
				case Category.MeshTransformer:
					return TreeCanvasGUI.meshTransformerNodeColor;
				case Category.Mapper:
					return TreeCanvasGUI.mapperNodeColor;
				case Category.Function:
					return TreeCanvasGUI.functionNodeColor;
				default:
					return TreeCanvasGUI.neutralColor;
				}
			} 
		}
		/// <summary>
		/// The pipeline element.
		/// </summary>
		public PipelineElement pipelineElement = null;
		/// <summary>
		/// The size of the rect to draw this node.
		/// </summary>
		protected Vector2 rectSize = new Vector2(124, 62);
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public virtual string nodeName {
			get { return ""; }	
		}
		/// <summary>
		/// The node header background style.
		/// </summary>
		[System.NonSerialized]
		private GUIStyle nodeHeaderBGStyle;
		/// <summary>
		/// The node output.
		/// </summary>
		[System.NonSerialized]
		private NodeOutput nodeOutput = null;
		/// <summary>
		/// The node input.
		/// </summary>
		[System.NonSerialized]
		private NodeInput nodeInput = null;
		/// <summary>
		/// The in knob active texture.
		/// </summary>
		private static Texture2D InKnobActiveTex;
		/// <summary>
		/// The out knob active texture.
		/// </summary>
		private static Texture2D OutKnobActiveTex;
		/// <summary>
		/// The in knob suggest texture.
		/// </summary>
		private static Texture2D InKnobSuggestTex;
		/// <summary>
		/// The out knob suggest texture.
		/// </summary>
		private static Texture2D OutKnobSuggestTex;
		/// <summary>
		/// The suggest connect dot texture.
		/// </summary>
		private static Texture2D SuggestConnectDotTex;
		/// <summary>
		/// True if the node is selected.
		/// </summary>
		bool isSelected = false;
		#endregion

		#region Node Creation
		/// <summary>
		/// Create an instance of this Node at the given position
		/// </summary>
		/// <param name="pos">Position.</param>
		public override Broccoli.NodeEditorFramework.Node Create (Vector2 pos) 
		{
			BaseNode node = CreateExplicit ();
			InitNode (node, pos);
			return node;
		}
		/// <summary>
		/// Sets the name of the node.
		/// </summary>
		public void SetName() {
			name = nodeName;
		}
		/// <summary>
		/// Create the a Node of the type specified by the nodeID at position
		/// </summary>
		public static Broccoli.NodeEditorFramework.Node Create (string nodeID, PipelineElement plElement) 
		{
			if (!NodeCanvasManager.CheckCanvasCompability (nodeID, NodeEditor.curNodeCanvas))
				throw new UnityException ("Cannot create Node with ID '" + nodeID + 
					"' as it is not compatible with the current canavs type (" + NodeEditor.curNodeCanvas.GetType ().ToString () + ")!");
			if (!NodeEditor.curNodeCanvas.CanAddNode (nodeID))
				throw new UnityException ("Cannot create another Node with ID '" + nodeID + 
					"' on the current canvas of type (" + NodeEditor.curNodeCanvas.GetType ().ToString () + ")!");
			BaseNode node = NodeTypes.getDefaultNode (nodeID) as BaseNode; // TODO: this create node is called two times.
			if (node == null)
				throw new UnityException ("Cannot create Node as ID '" + nodeID + "' is not registered!");

			node = node.Create (plElement.nodePosition) as BaseNode;

			if(node == null)
				return null;

			node.SetName ();
			node.InitBase ();
			node.InitTextures ();
			node.SetPipelineElement (plElement);
			node.SetConnectors ();

			NodeEditorCallbacks.IssueOnAddNode (node);
			NodeEditor.curNodeCanvas.Validate ();

			return node;
		}
		/// <summary>
		/// Inits the node.
		/// </summary>
		/// <param name="node">Node.</param>
		/// <param name="pos">Position.</param>
		private void InitNode (BaseNode node, Vector2 pos) {
			node.rect = new Rect (pos.x, pos.y, rectSize.x, rectSize.y);
			node.name = GetID;
		}
		/// <summary>
		/// Inits the textures.
		/// </summary>
		public void InitTextures () {
			if (OutKnobActiveTex == null)
				OutKnobActiveTex = 
				RTEditorGUI.RotateTextureCCW (ResourceManager.GetTintedTexture (
					"Broccoli/GUI/Out_Knob.png", TreeCanvasGUI.activeNodeOutputColor), 3);
			if (InKnobActiveTex == null)
				InKnobActiveTex = 
				RTEditorGUI.RotateTextureCCW (ResourceManager.GetTintedTexture (
					"Broccoli/GUI/In_Knob.png", TreeCanvasGUI.activeNodeOutputColor), 3);
			if (OutKnobSuggestTex == null)
				OutKnobSuggestTex = 
				RTEditorGUI.RotateTextureCCW (ResourceManager.GetTintedTexture (
					"Broccoli/GUI/Out_Knob.png", TreeCanvasGUI.connectDotColor), 3);
			if (InKnobSuggestTex == null)
				InKnobSuggestTex = 
				RTEditorGUI.RotateTextureCCW (ResourceManager.GetTintedTexture (
					"Broccoli/GUI/In_Knob.png", TreeCanvasGUI.connectDotColor), 3);
			if (SuggestConnectDotTex == null)
				SuggestConnectDotTex = 
					ResourceManager.GetTintedTexture ("Broccoli/GUI/Connect_Dot.png", TreeCanvasGUI.connectDotColor);
			
		}
		/// <summary>
		/// Sets the connectors.
		/// </summary>
		public void SetConnectors () {
			if (pipelineElement != null) {
				if (pipelineElement.connectionType ==
					PipelineElement.ConnectionType.Source ||
					pipelineElement.connectionType ==
					PipelineElement.ConnectionType.Transform) {
					nodeOutput = CreateOutput ("Src", "Base", NodeSide.Bottom, rectSize.x/2f);
				}
				if (pipelineElement.connectionType ==
					PipelineElement.ConnectionType.Sink ||
					pipelineElement.connectionType ==
					PipelineElement.ConnectionType.Transform) {
					nodeInput = CreateInput ("Sink", "Base", NodeSide.Top, rectSize.x/2f);
				}
			}
		}
		/// <summary>
		/// Sets the in valid pipeline.
		/// </summary>
		/// <param name="isInValidPipeline">If set to <c>true</c> is in valid pipeline.</param>
		public void SetInValidPipeline (bool isInValidPipeline) {	
			if (nodeOutput != null) {
				if (isInValidPipeline) {
					nodeOutput.typeData.Color = TreeCanvasGUI.activeNodeOutputColor;
					for (int i = 0; i < Outputs.Count; i++) {
						Outputs[i].typeData.Color = TreeCanvasGUI.activeNodeOutputColor;
					}
				} else {
					nodeOutput.typeData.Color = TreeCanvasGUI.inactiveNodeOutputColor;
					for (int i = 0; i < Outputs.Count; i++) {
						Outputs[i].typeData.Color = TreeCanvasGUI.inactiveNodeOutputColor;
					}
				}
			}
			if (nodeInput != null) {
				if (isInValidPipeline) {
					nodeInput.typeData.Color = TreeCanvasGUI.activeNodeOutputColor;
					for (int i = 0; i < Inputs.Count; i++) {
						Inputs[i].typeData.Color = TreeCanvasGUI.activeNodeOutputColor;
					}
				} else {
					nodeInput.typeData.Color = TreeCanvasGUI.inactiveNodeOutputColor;
					for (int i = 0; i < Inputs.Count; i++) {
						Inputs[i].typeData.Color = TreeCanvasGUI.inactiveNodeOutputColor;
					}
				}
			}
		}
		#endregion

		#region Abstract Methods
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected abstract BaseNode CreateExplicit ();
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public abstract void SetPipelineElement (PipelineElement pipelineElement = null);
		#endregion

		#region Draw
		/// <summary>
		/// Draw the Node immediately
		/// </summary>
		protected override void NodeGUI () {
			if (pipelineElement.log.Count > 0) {
				LogItem logItem = pipelineElement.log.Peek ();
				switch (logItem.messageType) {
				case LogItem.MessageType.Info:
					GUI.DrawTexture (new Rect (rectSize.x - 34, rectSize.y - 34 - contentOffset.y, 32, 32), GUITextureManager.infoTexture, ScaleMode.ScaleToFit);
					break;
				case LogItem.MessageType.Warning:
					GUI.DrawTexture (new Rect (rectSize.x - 34, rectSize.y - 34 - contentOffset.y, 32, 32), GUITextureManager.warnTexture, ScaleMode.ScaleToFit);
					break;
				case LogItem.MessageType.Error:
					GUI.DrawTexture (new Rect (rectSize.x - 34, rectSize.y - 34 - contentOffset.y, 32, 32), GUITextureManager.errorTexture, ScaleMode.ScaleToFit);
					break;
				}
			}

			NodeGUIExplicit ();
		}
		/// <summary>
		/// Explicit drawing method for this node.
		/// </summary>
		protected virtual void NodeGUIExplicit () {}
		/// <summary>
		/// Draws the node frame and calls NodeGUI. Can be overridden to customize drawing.
		/// </summary>
		protected override void DrawNode () 
		{
			AssureIsSelected ();
			AssureNodeBGStyle ();
			// TODO: Node Editor Feature: Custom Windowing System
			// Create a rect that is adjusted to the editor zoom and pixel perfect
			Rect nodeRect = rect;
			Vector2 pos = NodeEditor.curEditorState.zoomPanAdjust + NodeEditor.curEditorState.panOffset;
			nodeRect.position = new Vector2((int)(nodeRect.x+pos.x), (int)(nodeRect.y+pos.y));
			if (pipelineElement.hasKeyName) {
				contentOffset = new Vector2 (0, 27);
			} else {
				contentOffset = new Vector2 (0, 20);
			}

			// Create a headerRect out of the previous rect and draw it, marking the selected node as such by making the header bold
			Rect headerRect = new Rect (nodeRect.x, nodeRect.y, nodeRect.width, contentOffset.y);
			GUI.BeginGroup (headerRect, nodeHeaderBGStyle);
			headerRect.position = Vector2.zero;
			GUILayout.BeginArea (headerRect, TreeCanvasGUI.nodeHeaderLabel);
			DrawNodeHeader (headerRect);
			GUILayout.EndArea ();
			GUI.EndGroup ();

			// Begin the body frame around the NodeGUI
			Rect bodyRect = new Rect (nodeRect.x, nodeRect.y + contentOffset.y, nodeRect.width, nodeRect.height - contentOffset.y);
			GUI.BeginGroup (bodyRect, nodeBGStyle);
			bodyRect.position = Vector2.zero;
			GUILayout.BeginArea (bodyRect, TreeCanvasGUI.nodeHeaderLabel);

			//GUI.DrawTexture (new Rect (rectSize.x - 16 - contentOffset.x, 0, 16, 16), GUITextureManager.GetIconShuffleOn (), ScaleMode.StretchToFill);
			//GUILayout.BeginArea (bodyRect, GUIStyle.none);
			// Call NodeGUI
			GUI.changed = false;
			NodeGUI ();

			// End NodeGUI frame
			GUILayout.EndArea ();
			GUI.EndGroup ();
		}
		/// <summary>
		/// Draws the node header.
		/// </summary>
		/// <param name="headerRect">Header rect.</param>
		protected internal virtual void DrawNodeHeader (Rect headerRect) {
			GUI.backgroundColor = color;
			GUI.Box (headerRect, GUIContent.none, nodeHeaderBGStyle);
			GUI.backgroundColor = Color.white;
			if (pipelineElement.connectionType == PipelineElement.ConnectionType.Transform) {
				Rect toggleRect = new Rect(headerRect);
				toggleRect.position += new Vector2 (2, 0);
				toggleRect.size = new Vector2 (18, headerRect.height);
				bool isActive = GUI.Toggle (toggleRect, pipelineElement.isActive, "");
				if (isActive != pipelineElement.isActive) {
					pipelineElement.isActive = isActive;
					TreeFactory.GetActiveInstance ().ProcessPipelinePreviewDownstream (pipelineElement, true);
				}
				if (pipelineElement.hasKeyName) {
					headerRect.position += new Vector2 (20, -5);
				} else {
					headerRect.position += new Vector2 (20, -1);
				}
				GUI.Label (headerRect, name, NodeEditor.curEditorState.selectedNode == this ? 
					TreeCanvasGUI.nodeHeaderSelectedLabel : TreeCanvasGUI.nodeHeaderLabel);
				if (pipelineElement.hasKeyName) {
					headerRect.position += new Vector2 (0, 19);
					GUI.Label (headerRect, pipelineElement.keyName, TreeCanvasGUI.smallNodeLabel);
				}
				if (NodeEditor.curEditorState.zoom == 1)
					EditorGUIUtility.AddCursorRect (headerRect, MouseCursor.MoveArrow); //TODO: Does this add up?
			} else {
				//headerRect.position += new Vector2 (7, 0);
				if (pipelineElement.hasKeyName) {
					headerRect.position += new Vector2 (10, -5);
				} else {
					headerRect.position += new Vector2 (10, -1);
				}
				GUI.Label (headerRect, name, NodeEditor.curEditorState.selectedNode == this ? 
					TreeCanvasGUI.nodeHeaderSelectedLabel : TreeCanvasGUI.nodeHeaderLabel);
				if (pipelineElement.hasKeyName) {
					headerRect.position += new Vector2 (0, 19);
					GUI.Label (headerRect, pipelineElement.keyName, TreeCanvasGUI.smallNodeLabel);
				}
				if (NodeEditor.curEditorState.zoom == 1)
					EditorGUIUtility.AddCursorRect (headerRect, MouseCursor.MoveArrow);
			}
			if (pipelineElement.usesRandomization) {
				if (pipelineElement.isSeedFixed)
					Graphics.DrawTexture (new Rect (rectSize.x - 22 - contentOffset.x, 2, 16, 16), GUITextureManager.GetIconShuffleOff ());
				else
					Graphics.DrawTexture (new Rect (rectSize.x - 22 - contentOffset.x, 2, 16, 16), GUITextureManager.GetIconShuffleOn ());
			}
		}
		/// <summary>
		/// Assures the node background style.
		/// </summary>
		private void AssureNodeBGStyle ()
		{
			if (nodeBGStyle == null || nodeBGStyle.normal.background == null || lastBGColor != backgroundColor)
			{
				lastBGColor = backgroundColor;
				nodeBGStyle = new GUIStyle (GUI.skin.box);
				nodeBGStyle.normal.background = ResourceManager.GetTintedTexture ("Broccoli/GUI/NE_Box_B.png", backgroundColor);
				nodeHeaderBGStyle = new GUIStyle (GUI.skin.box);
				nodeHeaderBGStyle.normal.background = 
					ResourceManager.GetTintedTexture ("Broccoli/GUI/NE_Header_Box_B.png", backgroundColor);
			}
		}
		/// <summary>
		/// Assures the node is selected.
		/// </summary>
		private void AssureIsSelected () {
			//isSelected = UnityEditor.Selection.activeObject == this;
			//if (NodeEditor.curEditorState.selectedNode == this && UnityEditor.Selection.activeObject == this) {
			if (UnityEditor.Selection.activeObject == this) {
				if (! isSelected) {
					nodeBGStyle.normal.background = 
						ResourceManager.GetTintedTexture ("Broccoli/GUI/NE_Box_B_Selected.png", backgroundColor);
					nodeHeaderBGStyle.normal.background = 
						ResourceManager.GetTintedTexture ("Broccoli/GUI/NE_Header_Box_B_Selected.png", backgroundColor);
					isSelected = true;
				}
			} else {
				if (isSelected) {
					nodeBGStyle.normal.background =
						ResourceManager.GetTintedTexture ("Broccoli/GUI/NE_Box_B.png", backgroundColor);
					nodeHeaderBGStyle.normal.background = 
						ResourceManager.GetTintedTexture ("Broccoli/GUI/NE_Header_Box_B.png", backgroundColor);
					isSelected = false;
				}
			}
		}
		/// <summary>
		/// Draws the nodeKnobs.
		/// </summary>
		protected override void DrawKnobs () 
		{
			CheckNodeKnobMigration ();

			for (int knobCnt = 0; knobCnt < nodeKnobs.Count; knobCnt++) {
				if (pipelineElement.isOnValidPipeline) {
					GUI.DrawTexture (nodeKnobs [knobCnt].GetGUIKnob (), GetKnobTexture (nodeKnobs [knobCnt], true));
					if (NodeEditor.curEditorState.zoom == 1)
						EditorGUIUtility.AddCursorRect (nodeKnobs[knobCnt].GetGUIKnob(), MouseCursor.ArrowMinus);
				} else {
					nodeKnobs [knobCnt].DrawKnob ();
					if (NodeEditor.curEditorState.zoom == 1)
						EditorGUIUtility.AddCursorRect (nodeKnobs[knobCnt].GetGUIKnob(), MouseCursor.ArrowPlus);
				}
				if (TreeCanvasConnect.GetInstance ().IsCandidate (pipelineElement) &&
					!TreeCanvasConnect.GetInstance().IsCandidateDroppable (pipelineElement)) {
					if (nodeKnobs [knobCnt] is NodeInput && TreeCanvasConnect.GetInstance ().IsCandidateSrc (pipelineElement)) {
						Graphics.DrawTexture (nodeKnobs [knobCnt].GetGUIKnob (), GetKnobTexture(nodeKnobs [knobCnt], false, true));
					} else if (nodeKnobs [knobCnt] is NodeOutput && TreeCanvasConnect.GetInstance ().IsCandidateSink (pipelineElement)) {
						Graphics.DrawTexture (nodeKnobs [knobCnt].GetGUIKnob (), GetKnobTexture(nodeKnobs [knobCnt], false, true));
					}
				}
			}
		}
		/// <summary>
		/// Gets the knob texture.
		/// </summary>
		/// <returns>The knob texture.</returns>
		/// <param name="knob">Knob.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isSuggestion">If set to <c>true</c> is suggestion.</param>
		private Texture2D GetKnobTexture (NodeKnob knob, bool isActive = false, bool isSuggestion = false) {
			if (isSuggestion) {
				return (knob is NodeInput?InKnobSuggestTex:OutKnobSuggestTex);
			} else if (isActive) {
				return (knob is NodeInput?InKnobActiveTex:OutKnobActiveTex);
			}
			return knob.knobTexture;
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
					Vector2 endPos = input.GetGUIKnob ().center;
					Vector2 endDir = input.GetDirection();
					NodeEditorGUI.OptimiseBezierDirections (startPos, ref startDir, endPos, ref endDir);
					if (pipelineElement.isOnValidPipeline) {
						NodeEditorGUI.DrawConnection (startPos, startDir, endPos, endDir, TreeCanvasGUI.activeNodeOutputColor, 2);
					} else { 
						NodeEditorGUI.DrawConnection (startPos, startDir, endPos, endDir, TreeCanvasGUI.inactiveNodeOutputColor, 2);
					}
					if (TreeCanvasConnect.GetInstance ().IsCandidateSink (pipelineElement)
						|| TreeCanvasConnect.GetInstance ().IsCandidateDroppable (pipelineElement)) {
						Vector2 middlePos = (startPos + endPos) / 2f;
						Graphics.DrawTexture (new Rect(middlePos.x - 8, middlePos.y - 8, 16, 16), SuggestConnectDotTex);
					}
				}
			}
		}
		/// <summary>
		/// Draws the label.
		/// </summary>
		/// <param name="content">Content.</param>
		public void DrawLabel (string content) {
			EditorGUILayout.LabelField (" " + content, TreeCanvasGUI.nodeLabel);
		}
		#endregion
	}

	/// <summary>
	/// Connection Type only for visual purposes.
	/// </summary>
	public class BaseType : IConnectionTypeDeclaration 
	{
		/// <summary>
		/// Gets the identifier of the connection.
		/// </summary>
		/// <value>The identifier.</value>
		public string Identifier { get { return "Base"; } }
		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		public System.Type Type { get { return typeof(void); } }
		/// <summary>
		/// Gets the color of the connection.
		/// </summary>
		/// <value>The color.</value>
		public Color Color { get { return Color.white; } }
		/// <summary>
		/// Gets the in knob texture.
		/// </summary>
		/// <value>The in knob texture.</value>
		public string InKnobTex { get { return "Broccoli/GUI/In_Knob.png"; } }
		/// <summary>
		/// Gets the out knob texture.
		/// </summary>
		/// <value>The out knob texture.</value>
		public string OutKnobTex { get { return "Broccoli/GUI/Out_Knob.png"; } }
		/// <summary>
		/// Gets the width of the line.
		/// </summary>
		/// <value>The width of the line.</value>
		public int LineWidth { get { return 3; } }
	}
}