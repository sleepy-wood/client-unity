using UnityEngine;
using UnityEditor;

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// LSystem graph node.
	/// </summary>
	[Node (true, "Structure Generator/L-System (Deprecated)", 2)]
	public class LSystemGraphNode : BaseNode 
	{
		#region Vars
		/// Gets the get Id of the node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (LSystemGraphNode).ToString(); } 
		}
		/// <summary>
		/// Gets the category of the node.
		/// </summary>
		/// <value>Category of the node.</value>
		public override Category category { get { return Category.StructureGenerator; } }
		/// <summary>
		/// The LSystem element.
		/// </summary>
		public LSystemElement lSystemElement = null;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "LSystem"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			LSystemGraphNode node = CreateInstance<LSystemGraphNode> ();
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				//lSystemElement = new LSystemElement ();
				lSystemElement = ScriptableObject.CreateInstance<LSystemElement> ();
			} else {
				lSystemElement = (LSystemElement)pipelineElement;
			}
			this.pipelineElement = lSystemElement;
		}
		/// <summary>
		/// Explicit drawing method for this node.
		/// </summary>
		protected override void NodeGUIExplicit () {
			if (lSystemElement != null) {
				DrawLabel ("Iterations :" + lSystemElement.iterations);
			}
		}
		/// <summary>
		/// Used to display a custom node property editor in the side window of the NodeEditorWindow
		/// Optionally override this to implement
		/// </summary>
		public override void DrawNodePropertyEditor () {
			if (pipelineElement != null) {
				EditorGUI.BeginChangeCheck ();
				lSystemElement.axiom = 
					EditorGUILayout.DelayedTextField ("Axiom", lSystemElement.axiom);
			}
		}
		#endregion
	}
}