using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Girth transform node.
	/// </summary>
	[Node (false, "Structure Transformer/Girth Transform", 30)]
	public class GirthTransformNode : BaseNode
	{
		#region Vars
		/// <summary>
		/// Gets the get Id of the node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (GirthTransformNode).ToString(); } 
		}
		/// <summary>
		/// Gets the category of the node.
		/// </summary>
		/// <value>Category of the node.</value>
		public override Category category { get { return Category.StructureTransformer; } }
		/// <summary>
		/// The girth transform element.
		/// </summary>
		public GirthTransformElement GirthTransformElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Girth Transform"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			GirthTransformNode node = CreateInstance<GirthTransformNode> ();
			node.rectSize = new Vector2 (140, 72);
			node.name = "Girth Transform";
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				GirthTransformElement = ScriptableObject.CreateInstance<GirthTransformElement> ();
			} else {
				GirthTransformElement = (GirthTransformElement)pipelineElement;
			}
			this.pipelineElement = GirthTransformElement;
		}
		/// <summary>
		/// Explicit drawing method for this node.
		/// </summary>
		protected override void NodeGUIExplicit () {
			if (GirthTransformElement != null) {
				DrawLabel ("At Base :" + GirthTransformElement.minGirthAtBase + "-" + GirthTransformElement.maxGirthAtBase);
				DrawLabel ("At Top :" + GirthTransformElement.minGirthAtTop + "-" + GirthTransformElement.maxGirthAtTop);
			}
		}
		#endregion
	}
}