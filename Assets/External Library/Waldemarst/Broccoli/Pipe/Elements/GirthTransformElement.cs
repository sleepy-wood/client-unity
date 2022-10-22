using UnityEngine;
using UnityEngine.Serialization;

namespace Broccoli.Pipe {
	/// <summary>
	/// Girth transform element.
	/// </summary>
	[System.Serializable]
	public class GirthTransformElement : PipelineElement {
		#region Vars
		/// <summary>
		/// Gets the type of the connection.
		/// </summary>
		/// <value>The type of the connection.</value>
		public override ConnectionType connectionType {
			get { return PipelineElement.ConnectionType.Transform; }
		}
		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		/// <value>The type of the element.</value>
		public override ElementType elementType {
			get { return PipelineElement.ElementType.StructureTransform; }
		}
		/// <summary>
		/// Gets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public override ClassType classType {
			get { return PipelineElement.ClassType.GirthTransform; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.structureTransformWeight + 20; }
		}
		/// <summary>
		/// The minimum girth at top of the tree.
		/// </summary>
		[FormerlySerializedAs("girthAtTop")]
		public float minGirthAtTop = 0.05f;
		/// <summary>
		/// The maximum girth at top of the tree.
		/// </summary>
		[FormerlySerializedAs("girthAtTop")]
		public float maxGirthAtTop = 0.05f;
		/// <summary>
		/// The minimum girth at base of the tree.
		/// </summary>
		[FormerlySerializedAs("girthAtBase")]
		public float minGirthAtBase = 0.5f;
		/// <summary>
		/// The maximum girth at base of the tree.
		/// </summary>
		[FormerlySerializedAs("girthAtBase")]
		public float maxGirthAtBase = 0.5f;
		/// <summary>
		/// The transition curve.
		/// </summary>
		public AnimationCurve curve = 
			AnimationCurve.Linear(0f, 0f, 1f, 1f);
		/// <summary>
		/// Enables hierarchy scaling for branches, depending on their position and length of the tree.
		/// </summary>
		public bool hierarchyScalingEnabled = false;
		/// <summary>
		/// The minimum hierarchy scaling.
		/// </summary>
		public float minHierarchyScaling = 1f;
		/// <summary>
		/// The maximum hierarchy scaling.
		/// </summary>
		public float maxHierarchyScaling = 1f;
		/// <summary>
		/// The girth at tip of terminal roots.
		/// </summary>
		public float girthAtRootBottom = 0.05f;
		/// <summary>
		/// The girth at roots at the base of the tree.
		/// </summary>
		public float girthAtRootBase = 0.5f;
		/// <summary>
		/// The transition curve for curves.
		/// </summary>
		public AnimationCurve rootCurve = 
			AnimationCurve.Linear(0f, 0f, 1f, 1f);
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.GirthTransformElement"/> class.
		/// </summary>
		public GirthTransformElement () {}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			GirthTransformElement clone = ScriptableObject.CreateInstance<GirthTransformElement> ();
			SetCloneProperties (clone);
			clone.minGirthAtTop = minGirthAtTop;
			clone.maxGirthAtTop = maxGirthAtTop;
			clone.minGirthAtBase = minGirthAtBase;
			clone.maxGirthAtBase = maxGirthAtBase;
			clone.curve = new AnimationCurve(curve.keys);
			clone.hierarchyScalingEnabled = hierarchyScalingEnabled;
			clone.maxHierarchyScaling = maxHierarchyScaling;
			clone.minHierarchyScaling = minHierarchyScaling;
			clone.girthAtRootBase = girthAtRootBase;
			clone.girthAtRootBottom = girthAtRootBottom;
			clone.rootCurve = new AnimationCurve(rootCurve.keys);
			return clone;
		}
		#endregion
	}
}