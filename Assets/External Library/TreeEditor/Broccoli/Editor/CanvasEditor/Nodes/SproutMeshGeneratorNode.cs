using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Sprout mesh generator node.
	/// </summary>
	[Node (false, "Mesh Generator/Sprouts Mesh Generator", 110)]
	public class SproutMeshGeneratorNode : BaseNode
	{
		#region Vars
		/// <summary>
		/// Get the Id of the Node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (SproutMeshGeneratorNode).ToString(); } 
		}
		public override Category category { get { return Category.MeshGenerator; } }
		/// <summary>
		/// The girth transform element.
		/// </summary>
		public SproutMeshGeneratorElement sproutMeshGeneratorElement;
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public override string nodeName {
			get { return "Sprout Mesh Generator"; }
		}
		/// <summary>
		/// Saves the selected option on the node editor.
		/// </summary>
		public int selectedToolbar = 0;
		public bool showSectionSize = true;
		public bool showSectionScale = true;
		public bool showSectionHorizontalAlign = true;
		public bool showSectionResolution = true;
		public bool showSectionGravityBending = true;
		public bool showSectionMesh = true;
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			SproutMeshGeneratorNode node = CreateInstance<SproutMeshGeneratorNode> ();
			node.rectSize = new Vector2 (160, 72);
			node.name = "Sprout Mesh Generator";
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				sproutMeshGeneratorElement = ScriptableObject.CreateInstance<SproutMeshGeneratorElement> ();
			} else {
				sproutMeshGeneratorElement = (SproutMeshGeneratorElement)pipelineElement;
			}
			this.pipelineElement = sproutMeshGeneratorElement;
		}
		/// <summary>
		/// Explicit drawing method for this node.
		/// </summary>
		protected override void NodeGUIExplicit () {
			if (sproutMeshGeneratorElement != null) {
				int j = 0;
				Rect sproutGroupsRect = new Rect (7, 3, 8, 8);
				for (int i = 0; i < sproutMeshGeneratorElement.sproutMeshes.Count; i++) {
					EditorGUI.DrawRect (sproutGroupsRect, 
						sproutMeshGeneratorElement.pipeline.sproutGroups.GetSproutGroupColor (
							sproutMeshGeneratorElement.sproutMeshes [i].groupId));
					j++;
					if (j >= 4) {
						sproutGroupsRect.x += 11;
						sproutGroupsRect.y = 3;
						j = 0;
					} else {
						sproutGroupsRect.y += 11;
					}
				}
				if (sproutMeshGeneratorElement != null) {
					if (sproutMeshGeneratorElement.showLODInfoLevel == 1) {
						DrawLabel ("\t" + sproutMeshGeneratorElement.verticesCountFirstPass + " verts");
						DrawLabel ("\t" + sproutMeshGeneratorElement.trianglesCountFirstPass + " tris");
					} else if (sproutMeshGeneratorElement.showLODInfoLevel == 2) {
						DrawLabel ("\t" + sproutMeshGeneratorElement.verticesCountSecondPass + " verts");
						DrawLabel ("\t" + sproutMeshGeneratorElement.trianglesCountSecondPass + " tris");
					} else {
						DrawLabel ("            LOD0: " + sproutMeshGeneratorElement.verticesCountSecondPass + " v, " + 
							sproutMeshGeneratorElement.trianglesCountSecondPass + " t");
						DrawLabel ("            LOD1: " + sproutMeshGeneratorElement.verticesCountFirstPass + " v, " + 
							sproutMeshGeneratorElement.trianglesCountFirstPass + " t");
					}
				}
			}
		}
		#endregion
	}
}