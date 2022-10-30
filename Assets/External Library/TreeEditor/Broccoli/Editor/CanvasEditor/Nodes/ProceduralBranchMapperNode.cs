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
	[Node (true, "Mapper/Procedural Branch Mapper", 200)]
	public class ProceduralBranchMapperNode : BaseNode
	{
		#region Vars
		/// <summary>
		/// Get the Id of the Node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (ProceduralBranchMapperNode).ToString(); } 
		}
		/// <summary>
		/// Gets the category of the node.
		/// </summary>
		/// <value>Category of the node.</value>
		public override Category category { get { return Category.Mapper; } }
		/// <summary>
		/// The girth transform element.
		/// </summary>
		public ProceduralBranchMapperElement proceduralBranchMapperElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Procedural Branch Mapper"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			ProceduralBranchMapperNode node = CreateInstance<ProceduralBranchMapperNode> ();
			node.name = "Procedural Branch Mapper";
			node.rectSize.x = 182;
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				proceduralBranchMapperElement = ScriptableObject.CreateInstance<ProceduralBranchMapperElement> ();
			} else {
				proceduralBranchMapperElement = (ProceduralBranchMapperElement)pipelineElement;
			}
			this.pipelineElement = proceduralBranchMapperElement;
		}
		/// <summary>
		/// Explicit drawing method for this node.
		/// </summary>
		protected override void NodeGUIExplicit () {
			if (proceduralBranchMapperElement != null) {
			}
		}
		#endregion
	}
}