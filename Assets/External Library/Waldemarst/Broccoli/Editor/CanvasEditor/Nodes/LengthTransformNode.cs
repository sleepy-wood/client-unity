using System.Collections;

using UnityEditor;
using UnityEngine;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Length transform node.
	/// </summary>
	[Node (false, "Structure Transformer/Length Transform", 20)]
	public class LengthTransformNode : BaseNode
	{
		#region Vars
		/// <summary>
		/// Gets the get Id of the node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (LengthTransformNode).ToString(); } 
		}
		/// <summary>
		/// Gets the category of the node.
		/// </summary>
		/// <value>Category of the node.</value>
		public override Category category { get { return Category.StructureTransformer; } }
		/// <summary>
		/// The length transform element.
		/// </summary>
		public LengthTransformElement lengthTransformElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Length Transform"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			LengthTransformNode node = CreateInstance<LengthTransformNode> ();
			node.rectSize.x = 150;
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				lengthTransformElement = ScriptableObject.CreateInstance<LengthTransformElement> ();
			} else {
				lengthTransformElement = (LengthTransformElement)pipelineElement;
			}
			this.pipelineElement = lengthTransformElement;
		}
		#endregion
	}
}