using System;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.NodeEditorFramework;

namespace Broccoli.TreeNodeEditor
{
	public class TreeCanvasTraversal : NodeCanvasTraversal
	{
		//TreeCanvas Canvas;

		public TreeCanvasTraversal (TreeCanvas canvas) : base(canvas)
		{
			//Canvas = canvas;
		}

		/// <summary>
		/// Traverse the canvas and evaluate it
		/// </summary>
		public override void TraverseAll () 
		{
			/*
			RootGraphNode rootNode = Canvas.rootNode;
			rootNode.Calculate ();
			*/
		}
	}
}