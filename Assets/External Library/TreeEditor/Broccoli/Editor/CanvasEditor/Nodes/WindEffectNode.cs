using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Wind effect node.
	/// </summary>
	[Node (false, "Function/Wind Effect Node", 300)]
	public class WindEffectNode : BaseNode
	{
		#region Vars
		/// <summary>
		/// Get the Id of the Node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (WindEffectNode).ToString(); } 
		}
		public override Category category { get { return Category.Function; } }
		/// <summary>
		/// The wind effect element.
		/// </summary>
		public WindEffectElement windEffectElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Wind Effect"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			WindEffectNode node = CreateInstance<WindEffectNode> ();
			node.rectSize.x = 100;
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				windEffectElement = ScriptableObject.CreateInstance<WindEffectElement> ();
			} else {
				windEffectElement = (WindEffectElement)pipelineElement;
			}
			this.pipelineElement = windEffectElement;
		}
		#endregion
	}
}