using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Length transform element.
	/// </summary>
	[System.Serializable]
	public class LengthTransformElement : PipelineElement {
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
			get { return PipelineElement.ClassType.LengthTransform; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.structureTransformWeight + 10; }
		}
		/// <summary>
		/// Gets a value indicating whether this <see cref="Broccoli.Pipe.LengthTransformElement"/> uses randomization.
		/// </summary>
		/// <value><c>true</c> if uses randomization; otherwise, <c>false</c>.</value>
		public override bool usesRandomization {
			get { return true; }
		}
		/// <summary>
		/// The level curve.
		/// </summary>
		public AnimationCurve levelCurve = AnimationCurve.Linear (0f, 1f, 1f, 1f);
		/// <summary>
		/// The position curve.
		/// </summary>
		public AnimationCurve positionCurve = AnimationCurve.Linear (0f, 1f, 1f, 1f);
		/// <summary>
		/// The minimum factor.
		/// </summary>
		public float minFactor = 0.2f;
		/// <summary>
		/// The max factor.
		/// </summary>
		public float maxFactor = 5.0f;
		#endregion

		#region Constructors/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.LengthTransformElement"/> class.
		/// </summary>
		public LengthTransformElement () {}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			LengthTransformElement clone = ScriptableObject.CreateInstance<LengthTransformElement> ();
			SetCloneProperties (clone);
			clone.levelCurve = new AnimationCurve (levelCurve.keys);
			clone.positionCurve = new AnimationCurve (positionCurve.keys);
			clone.minFactor = minFactor;
			clone.maxFactor = maxFactor;
			return clone;
		}
		#endregion
	}
}