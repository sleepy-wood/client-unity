using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;
using Broccoli.Utils;
using Broccoli.Factory;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Sprout mapper node.
	/// </summary>
	[Node (false, "Mapper/Sprout Texture Mapper", 210)]
	public class SproutMapperNode : BaseNode
	{
		#region Vars
		/// <summary>
		/// Get the Id of the Node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (SproutMapperNode).ToString(); } 
		}
		public override Category category { get { return Category.Mapper; } }
		/// <summary>
		/// The girth transform element.
		/// </summary>
		public SproutMapperElement sproutMapperElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Sprout Mapper"; }
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			SproutMapperNode node = CreateInstance<SproutMapperNode> ();
			node.name = "Sprout Mapper";
			node.rectSize = new Vector2 (132, 72);
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				sproutMapperElement = ScriptableObject.CreateInstance<SproutMapperElement> ();
			} else {
				sproutMapperElement = (SproutMapperElement)pipelineElement;
			}
			this.pipelineElement = sproutMapperElement;
		}
		/// <summary>
		/// Explicit drawing method for this node.
		/// </summary>
		protected override void NodeGUIExplicit () {
			if (sproutMapperElement != null) {
				int j = 0;
				Rect sproutGroupsRect = new Rect (7, 3, 8, 8);
				for (int i = 0; i < sproutMapperElement.sproutMaps.Count; i++) {
					EditorGUI.DrawRect (sproutGroupsRect, 
						sproutMapperElement.pipeline.sproutGroups.GetSproutGroupColor (
							sproutMapperElement.sproutMaps [i].groupId));
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