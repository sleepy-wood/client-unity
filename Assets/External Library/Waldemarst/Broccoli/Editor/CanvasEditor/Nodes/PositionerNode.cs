using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Positioner node.
	/// </summary>
	[Node (false, "Function/Positioner", 310)]
	public class PositionerNode : BaseNode 
	{
		#region Vars
		/// <summary>
		/// Gets the get Id of the node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (PositionerNode).ToString(); } 
		}
		/// <summary>
		/// Gets the category.
		/// </summary>
		/// <value>The category.</value>
		public override Category category { get { return Category.Function; } }
		/// <summary>
		/// The positioner element.
		/// </summary>
		public PositionerElement positionerElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Positioner"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			PositionerNode node = CreateInstance<PositionerNode> ();
			node.rectSize.x = 100;
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				positionerElement = ScriptableObject.CreateInstance<PositionerElement> ();
			} else {
				positionerElement = (PositionerElement)pipelineElement;
			}
			this.pipelineElement = positionerElement;
		}
		#endregion
	}
}