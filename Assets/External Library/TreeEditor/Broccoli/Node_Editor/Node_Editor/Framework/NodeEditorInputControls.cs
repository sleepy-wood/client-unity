using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;

namespace Broccoli.NodeEditorFramework 
{
	/// <summary>
	/// Collection of default Node Editor controls for the NodeEditorInputSystem
	/// </summary>
	public static class NodeEditorInputControls
	{
		#region Canvas Context Entries
		[ContextFillerAttribute (ContextType.Canvas)]
		private static void FillAddNodes (NodeEditorInputInfo inputInfo, GenericMenu canvasContextMenu) 
		{ // Show all nodes, and if a connection is drawn, only compatible nodes to auto-connect
			NodeEditorState state = inputInfo.editorState;
			List<Node> displayedNodes = state.connectOutput != null? NodeTypes.getCompatibleNodes (state.connectOutput) : NodeTypes.nodes.Keys.ToList ();
			//List<Node> displayedNodes = state.connectOutput != null? NodeTypes.getCompatibleNodes (state.connectOutput) : NodeTypes.nodes.OrderBy(x => x.Value.order).ToDictionary(x => x.Key, x => x.Value).Keys.ToList ();
			for (int i = 0; i < displayedNodes.Count; i++)
			{
				if (NodeCanvasManager.CheckCanvasCompability (displayedNodes[i].GetID, inputInfo.editorState.canvas) && inputInfo.editorState.canvas.CanAddNode (displayedNodes[i].GetID))
					canvasContextMenu.AddItem (new GUIContent ("Add " + NodeTypes.nodes[displayedNodes[i]].adress), false, CreateNodeCallback, new NodeEditorInputInfo (displayedNodes[i].GetID, state));
			}
		}

		private static void CreateNodeCallback (object infoObj)
		{
			NodeEditorInputInfo callback = infoObj as NodeEditorInputInfo;
			if (callback == null)
				throw new UnityException ("Callback Object passed by context is not of type NodeEditorInputInfo!");

			callback.SetAsCurrentEnvironment ();
			Node.Create (callback.message, NodeEditor.ScreenToCanvasSpace (callback.inputPos), callback.editorState.connectOutput);
			callback.editorState.connectOutput = null;
			NodeEditor.RepaintClients ();
		}
		#endregion

		#region Node Context Entries
		[ContextEntryAttribute (ContextType.Node, "Delete Node")]
		private static void DeleteNode (NodeEditorInputInfo inputInfo) 
		{
			inputInfo.SetAsCurrentEnvironment ();
			if (inputInfo.editorState.focusedNode != null) 
			{
				inputInfo.editorState.focusedNode.Delete ();
				inputInfo.inputEvent.Use ();
			}
		}
		/*
		[ContextEntryAttribute (ContextType.Node, "Duplicate Node")]
		private static void DuplicateNode (NodeEditorInputInfo inputInfo) 
		{
			inputInfo.SetAsCurrentEnvironment ();
			NodeEditorState state = inputInfo.editorState;
			if (state.focusedNode != null && NodeEditor.curNodeCanvas.CanAddNode (state.focusedNode.GetID)) 
			{ // Create new node of same type
				Node duplicatedNode = Node.Create (state.focusedNode.GetID, NodeEditor.ScreenToCanvasSpace (inputInfo.inputPos), state.connectOutput);
				state.selectedNode = state.focusedNode = duplicatedNode;
				state.connectOutput = null;
				state.connectInput = null;
				inputInfo.inputEvent.Use ();
			}
		}
		*/
		#endregion

		#region Node Keyboard Control
		// Main Keyboard_Move method
		[HotkeyAttribute(KeyCode.UpArrow, EventType.KeyDown)]
		[HotkeyAttribute(KeyCode.LeftArrow, EventType.KeyDown)]
		[HotkeyAttribute(KeyCode.RightArrow, EventType.KeyDown)]
		[HotkeyAttribute(KeyCode.DownArrow, EventType.KeyDown)]
		private static void KB_MoveNode(NodeEditorInputInfo inputInfo)
		{
			if (GUIUtility.keyboardControl > 0)
				return;
			NodeEditorState state = inputInfo.editorState;
			if (state.selectedNode != null && state.canvas.moveNodeEnabled)
			{ 
				Vector2 pos = state.selectedNode.rect.position;
				int shiftAmount = inputInfo.inputEvent.shift? 50 : 10;

				if (inputInfo.inputEvent.keyCode == KeyCode.RightArrow)
					pos = new Vector2(pos.x + shiftAmount, pos.y);
				else if (inputInfo.inputEvent.keyCode == KeyCode.LeftArrow)
					pos = new Vector2(pos.x - shiftAmount, pos.y);
				else if (inputInfo.inputEvent.keyCode == KeyCode.DownArrow)
					pos = new Vector2(pos.x, pos.y + shiftAmount);
				else if (inputInfo.inputEvent.keyCode == KeyCode.UpArrow)
					pos = new Vector2(pos.x, pos.y - shiftAmount);

				state.selectedNode.rect.position = pos;
				inputInfo.inputEvent.Use();
			}
			NodeEditor.RepaintClients();

		}
		#endregion

		#region Node Dragging
		[EventHandlerAttribute (EventType.MouseDown, 110)] // Priority over hundred to make it call after the GUI
		private static void HandleNodeDraggingStart (NodeEditorInputInfo inputInfo) 
		{
			if (GUIUtility.hotControl > 0)
				return; // GUI has control

			NodeEditorState state = inputInfo.editorState;
			if (inputInfo.inputEvent.button == 0 && state.focusedNode != null && 
				state.focusedNode == state.selectedNode && state.focusedNodeKnob == null && 
				state.canvas.moveNodeEnabled) 
			{ // Clicked inside the selected Node, so start dragging it
				state.dragNode = true;
				state.StartDrag ("node", inputInfo.inputPos, state.focusedNode.rect.position);
			}
		}

		[EventHandlerAttribute (EventType.MouseDrag)]
		private static void HandleNodeDragging (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (state.dragNode) 
			{ // If conditions apply, drag the selected node, else disable dragging
				if (state.selectedNode != null && GUIUtility.hotControl == 0 && inputInfo.editorState.dragUserID == "node")
				{ // Apply new position for the dragged node
					state.UpdateDrag ("node", inputInfo.inputPos);
					Vector2 offset = state.dragObjectPos - state.selectedNode.rect.position;
					state.selectedNode.rect.position = state.dragObjectPos;
					NodeEditorCallbacks.IssueOnDraggingNode (inputInfo.editorState.selectedNode, 
						offset, inputInfo.editorState.canvas.GetType());
					NodeEditor.RepaintClients ();
				} 
				else
					state.dragNode = false;
			}
		}

		[EventHandlerAttribute (EventType.MouseDown)]
		[EventHandlerAttribute (EventType.MouseUp)]
		private static void HandleNodeDraggingEnd (NodeEditorInputInfo inputInfo) 
		{
			if (inputInfo.editorState.dragUserID == "node") 
			{
				Vector2 totalDrag = inputInfo.editorState.EndDrag ("node");
				if (inputInfo.editorState.dragNode && inputInfo.editorState.selectedNode != null)
				{
					inputInfo.editorState.selectedNode.rect.position = totalDrag;
					NodeEditorCallbacks.IssueOnMoveNode (inputInfo.editorState.selectedNode, inputInfo.editorState.canvas.GetType());
				}
			}
			inputInfo.editorState.dragNode = false;
		}
		#endregion

		#region Window Panning
		[EventHandlerAttribute (EventType.MouseDown, 105)] // Priority over hundred to make it call after the GUI
		private static void HandleWindowPanningStart (NodeEditorInputInfo inputInfo) 
		{
			if (GUIUtility.hotControl > 0)
				return; // GUI has control

			NodeEditorState state = inputInfo.editorState;
			if ((inputInfo.inputEvent.button == 0 || inputInfo.inputEvent.button == 2) && state.focusedNode == null && state.canvas.panWindowEnabled) 
			{ // Left- or Middle clicked on the empty canvas -> Start panning
				state.panWindow = true;
				state.StartDrag ("window", inputInfo.inputPos, state.panOffset);
			}
		}

		[EventHandlerAttribute (EventType.MouseDrag)]
		private static void HandleWindowPanning (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (state.panWindow) 
			{ // Calculate change in panOffset
				if (inputInfo.editorState.dragUserID == "window")
					state.panOffset += state.UpdateDrag ("window", inputInfo.inputPos);
				else
					state.panWindow = false;
				NodeEditor.RepaintClients ();
			}
		}

		[EventHandlerAttribute (EventType.MouseDown)]
		[EventHandlerAttribute (EventType.MouseUp)]
		private static void HandleWindowPanningEnd (NodeEditorInputInfo inputInfo) 
		{
			if (inputInfo.editorState.dragUserID == "window") {
				inputInfo.editorState.panOffset = inputInfo.editorState.EndDrag ("window");
				NodeEditorCallbacks.IssueOnPanCanvas (inputInfo.editorState.canvas, inputInfo.editorState.canvas.GetType());
			}
			inputInfo.editorState.panWindow = false;
		}
		#endregion

		#region Connection
		[EventHandlerAttribute (EventType.MouseDown)]
		private static void HandleConnectionDrawing (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (inputInfo.inputEvent.button == 0 && state.focusedNodeKnob != null && state.canvas.editConnectionEnabled)
			{ // Left-Clicked on a NodeKnob, so check if any of them is a nodeInput or -Output
				if (state.focusedNodeKnob is NodeOutput)
				{ // Output clicked -> Draw new connection from it
					NodeOutput nodeOutput = state.focusedNodeKnob as NodeOutput;
					if (nodeOutput.connections.Count > 0) {
						NodeInput nodeInput = nodeOutput.connections [0];
						nodeInput.RemoveConnection ();
						/*
						for each (NodeInput node in nodeOutput.connections) {
							node.RemoveConnection ();
						}
						*/
						state.connectInput = nodeInput;
						state.connectOutput = null;
					} else {
						state.connectOutput = nodeOutput;
						state.connectInput = null;
					}
					inputInfo.inputEvent.Use ();
					/*
					state.connectOutput = (NodeOutput)state.focusedNodeKnob;
					NodeOutput nodeoutput = state.connectOutput;
					inputInfo.inputEvent.Use ();
					*/
				}
				else if (state.focusedNodeKnob is NodeInput)
				{ // Input clicked -> Loose and edit connection from it
					NodeInput nodeInput = state.focusedNodeKnob as NodeInput;
					if (nodeInput.connection != null) {
						NodeOutput nodeOutput = nodeInput.connection;
						nodeInput.RemoveConnection ();
						state.connectOutput = nodeOutput;
						state.connectInput = null;
					} else {
						state.connectInput = nodeInput;
						state.connectOutput = null;
					}
					inputInfo.inputEvent.Use ();
					/*
					NodeInput clickedInput = (NodeInput)state.focusedNodeKnob;
					state.connectOutput = clickedInput.connection;
					*/
					/*
					NodeInput clickedInput = (NodeInput)state.focusedNodeKnob;
					if (clickedInput.connection != null)
					{
						state.connectOutput = clickedInput.connection;
						clickedInput.RemoveConnection ();
						inputInfo.inputEvent.Use ();
					}
					*/
				}
			}
		}

		[EventHandlerAttribute (EventType.MouseUp)]
		private static void HandleApplyConnection (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (inputInfo.inputEvent.button == 0 && 
				state.connectOutput != null && 
				state.focusedNode != null && 
				state.focusedNodeKnob != null && 
				state.focusedNodeKnob is NodeInput &&
				state.canvas.editConnectionEnabled) 
			{ // An input was clicked, it'll will now be connected.
				NodeInput clickedInput = state.focusedNodeKnob as NodeInput;
				clickedInput.TryApplyConnection (state.connectOutput);
				inputInfo.inputEvent.Use ();
			} else if (inputInfo.inputEvent.button == 0 && 
				state.connectInput != null && 
				state.focusedNode != null && 
				state.focusedNodeKnob != null && 
				state.focusedNodeKnob is NodeOutput &&
				state.canvas.editConnectionEnabled) 
			{ // An output was clicked, it'll will now be connected.
				NodeOutput clickedOutput = state.focusedNodeKnob as NodeOutput;
				state.connectInput.TryApplyConnection (clickedOutput);
				inputInfo.inputEvent.Use ();
			}
			state.connectOutput = null;
			state.connectInput = null;
		}
		#endregion

		#region Zoom
		[EventHandlerAttribute (EventType.ScrollWheel)]
		private static void HandleZooming (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (state.canvas.zoomEnabled) {
				inputInfo.editorState.zoom = (float)Math.Round (Math.Min (4.0, Math.Max (0.6, inputInfo.editorState.zoom + inputInfo.inputEvent.delta.y / 15)), 2);
				NodeEditor.RepaintClients ();
				inputInfo.inputEvent.Use ();
			}
		}
		#endregion

		#region Navigation
		[HotkeyAttribute (KeyCode.N, EventType.KeyDown)]
		private static void HandleStartNavigating (NodeEditorInputInfo inputInfo) 
		{
			if (GUIUtility.keyboardControl > 0)
				return;
			inputInfo.editorState.navigate = true;
			inputInfo.inputEvent.Use ();
		}

		[HotkeyAttribute (KeyCode.N, EventType.KeyUp)]
		private static void HandleEndNavigating (NodeEditorInputInfo inputInfo) 
		{
			if (GUIUtility.keyboardControl > 0)
				return;
			inputInfo.editorState.navigate = false;
			inputInfo.inputEvent.Use ();
		}
		#endregion

		#region Node Snap
		[HotkeyAttribute (KeyCode.LeftControl, EventType.KeyDown, 60)] // 60 ensures it is checked after the dragging was performed before
		[HotkeyAttribute (KeyCode.LeftControl, EventType.KeyUp, 60)]
		private static void HandleNodeSnap (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (state.selectedNode != null && state.canvas.nodeSnapEnabled)
			{ // Snap selected Node's position and the drag to multiples of 10
				state.selectedNode.rect.x = Mathf.Round (state.selectedNode.rect.x/10) * 10;
				state.selectedNode.rect.y = Mathf.Round (state.selectedNode.rect.y/10) * 10;
				inputInfo.inputEvent.Use ();
			}
			if (state.activeGroup != null && state.canvas.nodeSnapEnabled)
			{
				state.activeGroup.rect.x = Mathf.Round (state.activeGroup.rect.x/10) * 10;
				state.activeGroup.rect.y = Mathf.Round (state.activeGroup.rect.y/10) * 10;
				inputInfo.inputEvent.Use ();
			}
			NodeEditor.RepaintClients ();
		}
		#endregion

	}

}

