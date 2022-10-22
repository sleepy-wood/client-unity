using UnityEngine;
using System;
using System.Collections.Generic;

using Broccoli.NodeEditorFramework;

namespace Broccoli.NodeEditorFramework 
{
	public abstract partial class NodeEditorCallbackReceiver : MonoBehaviour
	{
		// Editor
		public virtual void OnEditorStartUp () {}
		// Canvas: Save and Load
		public virtual void OnLoadCanvas (NodeCanvas canvas) {}
		public virtual void OnLoadEditorState (NodeEditorState editorState) {}
		public virtual void OnSaveCanvas (NodeCanvas canvas) {}
		public virtual void OnSaveEditorState (NodeEditorState editorState) {}
		// Canvas: navigation
		public virtual void OnPanCanvas (NodeCanvas canvas) {}
		// Node
		public virtual void OnAddNode (Node node) {}
		public virtual void OnDeleteNode (Node node) {}
		public virtual void OnMoveNode (Node node) {}
		public virtual void OnDraggingNode (Node node, Vector2 offset) {}
		public virtual void OnSelectNode (Node node) {}
		public virtual void OnDeselectNode () {}
		public virtual void OnAddNodeKnob (NodeKnob knob) {}
		// Connection
		public virtual void OnAddConnection (NodeInput input) {}
		public virtual void OnRemoveConnection (NodeInput input) {}
	}

	public static partial class NodeEditorCallbacks
	{
		private static int receiverCount;
		private static List<NodeEditorCallbackReceiver> callbackReceiver;

		public static void SetupReceivers () 
		{
			callbackReceiver = new List<NodeEditorCallbackReceiver> (MonoBehaviour.FindObjectsOfType<NodeEditorCallbackReceiver> ());
			receiverCount = callbackReceiver.Count;
		}

		#region Editor (1)
		public static Action OnEditorStartUp = null;
		public static void IssueOnEditorStartUp () 
		{
			if (OnEditorStartUp != null)
				OnEditorStartUp.Invoke ();
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnEditorStartUp ();
			}
		}
		#endregion

		#region Canvas: Save and Load (4)
		public static Action<NodeCanvas> OnLoadCanvas;
		public static void IssueOnLoadCanvas (NodeCanvas canvas) 
		{
			if (OnLoadCanvas != null)
				OnLoadCanvas.Invoke (canvas);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnLoadCanvas (canvas) ;
			}
		}

		public static Action<NodeEditorState> OnLoadEditorState;
		public static void IssueOnLoadEditorState (NodeEditorState editorState) 
		{
			if (OnLoadEditorState != null)
				OnLoadEditorState.Invoke (editorState);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnLoadEditorState (editorState) ;
			}
		}

		public static Action<NodeCanvas> OnSaveCanvas;
		public static void IssueOnSaveCanvas (NodeCanvas canvas) 
		{
			if (OnSaveCanvas != null)
				OnSaveCanvas.Invoke (canvas);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnSaveCanvas (canvas) ;
			}
		}

		public static Action<NodeEditorState> OnSaveEditorState;
		public static void IssueOnSaveEditorState (NodeEditorState editorState) 
		{
			if (OnSaveEditorState != null)
				OnSaveEditorState.Invoke (editorState);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnSaveEditorState (editorState) ;
			}
		}
		#endregion

		#region Canvas: navigation
		public static Dictionary<System.Type, Action<NodeCanvas>> OnPanCanvas = 
			new Dictionary<System.Type, Action<NodeCanvas>> ();
		public static void IssueOnPanCanvas (NodeCanvas canvas, System.Type canvasType) 
		{
			if (OnPanCanvas.ContainsKey (canvasType) && OnPanCanvas[canvasType] != null)
				OnPanCanvas[canvasType].Invoke (canvas);
		}
		#endregion

		#region Node (4)
		public static Action<Node> OnAddNode;
		public static void IssueOnAddNode (Node node) 
		{
			if (OnAddNode != null)
				OnAddNode.Invoke (node);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnAddNode (node);
			}
		}

		public static Action<Node> OnDeleteNode;
		public static void IssueOnDeleteNode (Node node) 
		{
			if (OnDeleteNode != null)
				OnDeleteNode.Invoke (node);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnDeleteNode (node);
			}
			node.OnDelete ();
		}

		public static Dictionary<System.Type, Action<Node>> OnMoveNode = 
			new Dictionary<System.Type, Action<Node>> ();
		public static void IssueOnMoveNode (Node node, System.Type canvasType) 
		{
			if (OnMoveNode.ContainsKey (canvasType) && OnMoveNode[canvasType] != null)
				OnMoveNode[canvasType].Invoke (node);
		}

		public static Dictionary<System.Type, Action<Node, Vector2>> OnDraggingNode = 
			new Dictionary<System.Type, Action<Node, Vector2>> ();
		public static void IssueOnDraggingNode (Node node, Vector2 offset, System.Type canvasType) 
		{
			if (OnDraggingNode.ContainsKey (canvasType) && OnDraggingNode[canvasType] != null)
				OnDraggingNode[canvasType].Invoke (node, offset);
		}

		public static Dictionary<System.Type, Action<Node>> OnSelectNode = 
			new Dictionary<System.Type, Action<Node>> ();
		public static void IssueOnSelectNode (Node node, System.Type canvasType) 
		{
			if (OnSelectNode.ContainsKey (canvasType) && OnSelectNode[canvasType] != null)
				OnSelectNode[canvasType].Invoke (node);
		}

		public static Dictionary<System.Type, Action> OnDeselectNode = 
			new Dictionary<System.Type, Action> ();
		public static void IssueOnDeselectNode (System.Type canvasType) 
		{
			if (OnDeselectNode.ContainsKey (canvasType) && OnDeselectNode[canvasType] != null)
				OnDeselectNode[canvasType].Invoke ();
		}

		public static Action<NodeKnob> OnAddNodeKnob;
		public static void IssueOnAddNodeKnob (NodeKnob nodeKnob) 
		{
			if (OnAddNodeKnob != null)
				OnAddNodeKnob.Invoke (nodeKnob);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnAddNodeKnob (nodeKnob);
			}
		}
		#endregion

		#region Connection (2)
		public static Dictionary<System.Type, Action<NodeInput>> OnAddConnection = 
			new Dictionary<System.Type, Action<NodeInput>> ();
		public static void IssueOnAddConnection (NodeInput input, System.Type canvasType) 
		{
			if (OnAddConnection.ContainsKey (canvasType) && OnAddConnection [canvasType] != null)
				OnAddConnection [canvasType].Invoke (input);
		}
		/*
		public static Action<NodeInput> OnAddConnection;
		public static void IssueOnAddConnection (NodeInput input) 
		{
			if (OnAddConnection != null)
				OnAddConnection.Invoke (input);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnAddConnection (input);
			}
		}
		*/

		public static Dictionary<System.Type, Action<NodeInput>> OnRemoveConnection = 
			new Dictionary<System.Type, Action<NodeInput>> ();
		public static void IssueOnRemoveConnection (NodeInput input, System.Type canvasType) 
		{
			if (OnRemoveConnection.ContainsKey (canvasType) && OnRemoveConnection [canvasType] != null)
				OnRemoveConnection [canvasType].Invoke (input);
		}
		/*
		public static Action<NodeInput> OnRemoveConnection;
		public static void IssueOnRemoveConnection (NodeInput input) 
		{
			if (OnRemoveConnection != null)
				OnRemoveConnection.Invoke (input);
			for (int cnt = 0; cnt < receiverCount; cnt++) 
			{
				if (callbackReceiver [cnt] == null)
					callbackReceiver.RemoveAt (cnt--);
				else
					callbackReceiver [cnt].OnRemoveConnection (input);
			}
		}
		*/
		#endregion

	}
}