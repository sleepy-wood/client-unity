using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Branch mesh generator node.
	/// </summary>
	[Node (false, "Mesh Generator/Branches Mesh Generator", 100)]
	public class BranchMeshGeneratorNode : BaseNode 
	{
		#region Vars
		/// <summary>
		/// Gets the get Id of the node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (BranchMeshGeneratorNode).ToString(); } 
		}
		/// <summary>
		/// Gets the category of the node.
		/// </summary>
		/// <value>Category of the node.</value>
		public override Category category { get { return Category.MeshGenerator; } }
		/// <summary>
		/// The branch mesh generator element.
		/// </summary>
		public BranchMeshGeneratorElement branchMeshGeneratorElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Branch Mesh Generator"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			BranchMeshGeneratorNode node = CreateInstance<BranchMeshGeneratorNode> ();
			node.rectSize = new Vector2 (162, 72);
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				branchMeshGeneratorElement = ScriptableObject.CreateInstance<BranchMeshGeneratorElement> ();
			} else {
				branchMeshGeneratorElement = (BranchMeshGeneratorElement)pipelineElement;
			}
			this.pipelineElement = branchMeshGeneratorElement;
		}
		/// <summary>
		/// Explicit drawing method for this node.
		/// </summary>
		protected override void NodeGUIExplicit () {
			/*
			if (branchMeshGeneratorElement != null) {
				if (branchMeshGeneratorElement.showLODInfoLevel == 1) {
					DrawLabel (branchMeshGeneratorElement.verticesCountFirstPass + " vertices");
					DrawLabel (branchMeshGeneratorElement.trianglesCountFirstPass + " triangles");
				} else if (branchMeshGeneratorElement.showLODInfoLevel == 2) {
					DrawLabel (branchMeshGeneratorElement.verticesCountSecondPass + " vertices");
					DrawLabel (branchMeshGeneratorElement.trianglesCountSecondPass + " triangles");
				} else {
					DrawLabel ("LOD0: " + branchMeshGeneratorElement.verticesCountSecondPass + " v, " + 
						branchMeshGeneratorElement.trianglesCountSecondPass + " t");
					DrawLabel ("LOD1: " + branchMeshGeneratorElement.verticesCountFirstPass + " v, " + 
						branchMeshGeneratorElement.trianglesCountFirstPass + " t");
				}
			}
			*/
		}
		#endregion
	}
}