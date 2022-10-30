using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Branch mapper node.
	/// </summary>
	[Node (false, "Mapper/Branch Mapper", 200)]
	public class BranchMapperNode : BaseNode
	{
		#region Vars
		/// <summary>
		/// Get the Id of the Node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (BranchMapperNode).ToString(); } 
		}
		/// <summary>
		/// Gets the category of the node.
		/// </summary>
		/// <value>Category of the node.</value>
		public override Category category { get { return Category.Mapper; } }
		/// <summary>
		/// The girth transform element.
		/// </summary>
		public BranchMapperElement branchMapperElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Branch Mapper"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			BranchMapperNode node = CreateInstance<BranchMapperNode> ();
			node.name = "Branch Mapper";
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				branchMapperElement = ScriptableObject.CreateInstance<BranchMapperElement> ();
			} else {
				branchMapperElement = (BranchMapperElement)pipelineElement;
			}
			this.pipelineElement = branchMapperElement;
		}
		/// <summary>
		/// Explicit drawing method for this node.
		/// </summary>
		protected override void NodeGUIExplicit () {
			if (branchMapperElement != null) {
			}
		}
		#endregion
	}
}