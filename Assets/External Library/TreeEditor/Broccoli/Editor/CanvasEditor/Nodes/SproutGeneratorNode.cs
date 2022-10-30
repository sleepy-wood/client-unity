using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Sprout generator node.
	/// </summary>
	[Node (false, "Structure Generator/Sprouts Generator", 1)]
	public class SproutGeneratorNode : BaseNode
	{
		#region Vars
		/// <summary>
		/// Gets the get Id of the node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (SproutGeneratorNode).ToString(); } 
		}
		public override Category category { get { return Category.StructureGenerator; } }
		/// <summary>
		/// The girth transform element.
		/// </summary>
		public SproutGeneratorElement sproutGeneratorElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Sprout Generator"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			SproutGeneratorNode node = CreateInstance<SproutGeneratorNode> ();
			node.rectSize = new Vector2(144, 72);
			node.name = "Sprout Generator";
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				sproutGeneratorElement = ScriptableObject.CreateInstance<SproutGeneratorElement> ();
			} else {
				sproutGeneratorElement = (SproutGeneratorElement)pipelineElement;
			}
			this.pipelineElement = sproutGeneratorElement;
		}
		/// <summary>
		/// Explicit drawing method for this node.
		/// </summary>
		protected override void NodeGUIExplicit () {
			if (sproutGeneratorElement != null) {
				int j = 0;
				Rect sproutGroupsRect = new Rect (7, 3, 8, 8);
				for (int i = 0; i < sproutGeneratorElement.sproutSeeds.Count; i++) {
					EditorGUI.DrawRect (sproutGroupsRect, 
						sproutGeneratorElement.GetSproutGroupColor (sproutGeneratorElement.sproutSeeds [i].groupId));
					j++;
					if (j >= 4) {
						sproutGroupsRect.x += 11;
						sproutGroupsRect.y = 3;
						j = 0;
					} else {
						sproutGroupsRect.y += 11;
					}
				}
			}
		}
		#endregion
	}
}