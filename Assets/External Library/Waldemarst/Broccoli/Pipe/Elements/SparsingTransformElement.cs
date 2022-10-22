using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Sparsing transform element.
	/// </summary>
	[System.Serializable]
	public class SparsingTransformElement : PipelineElement {
		#region SparseLevel class
		/// <summary>
		/// Sparse level class.
		/// Sparsing values applied to a hierarchy level.
		/// </summary>
		[System.Serializable]
		public class SparseLevel {
			/// <summary>
			/// The reorder mode.
			/// </summary>
			public SparsingTransformElement.ReorderMode reorderMode = 
				SparsingTransformElement.ReorderMode.None;
			/// <summary>
			/// The length sparsing mode.
			/// </summary>
			public SparsingTransformElement.LengthSparsingMode lengthSparsingMode = 
				SparsingTransformElement.LengthSparsingMode.None;
			/// <summary>
			/// The twirl sparsing mode.
			/// </summary>
			public SparsingTransformElement.TwirlSparsingMode twirlSparsingMode = 
				SparsingTransformElement.TwirlSparsingMode.None;
			/// <summary>
			/// The length sparsing value.
			/// </summary>
			public float lengthSparsingValue = 0.5f;
			/// <summary>
			/// The twirl sparsing value.
			/// </summary>
			public float twirlSparsingValue = 0f;
			/// <summary>
			/// The level.
			/// </summary>
			public int level = 0;
			/// <summary>
			/// Clone this instance.
			/// </summary>
			public SparseLevel Clone () {
				SparseLevel clone = new SparseLevel ();
				clone.reorderMode = reorderMode;
				clone.lengthSparsingMode = lengthSparsingMode;
				clone.twirlSparsingMode = twirlSparsingMode;
				clone.lengthSparsingValue = lengthSparsingValue;
				clone.twirlSparsingValue = twirlSparsingValue;
				clone.level = level;
				return clone;
			}
		}
		#endregion

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
			get { return PipelineElement.ClassType.SparsingTransform; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.structureTransformWeight + 30; }
		}
		/// <summary>
		/// Gets a value indicating whether this <see cref="Broccoli.Pipe.SparsingTransformElement"/> uses randomization.
		/// </summary>
		/// <value><c>true</c> if uses randomization; otherwise, <c>false</c>.</value>
		public override bool usesRandomization {
			get { return true; }
		}
		/// <summary>
		/// Reorder modes.
		/// </summary>
		public enum ReorderMode
		{
			None,
			Reverse,
			Random,
			HeavierOnTop,
			HeavierAtBottom
		}
		/// <summary>
		/// Length sparsing modes.
		/// </summary>
		public enum LengthSparsingMode
		{
			None,
			Absolute
		}
		/// <summary>
		/// Twirl sparsing modes.
		/// </summary>
		public enum TwirlSparsingMode
		{
			None,
			Additive
		}
		/// <summary>
		/// The sparse levels.
		/// </summary>
		public List<SparseLevel> sparseLevels = new List<SparseLevel> ();
		/// <summary>
		/// The index of the selected sparse level.
		/// </summary>
		public int selectedSparseLevelIndex = -1;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.SparsingTransformElement"/> class.
		/// </summary>
		public SparsingTransformElement () {}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			SparsingTransformElement clone = ScriptableObject.CreateInstance<SparsingTransformElement> ();
			SetCloneProperties (clone);
			for (int i = 0; i < sparseLevels.Count; i++) {
				clone.sparseLevels.Add (sparseLevels[i].Clone ());
			}
			clone.selectedSparseLevelIndex = selectedSparseLevelIndex;
			return clone;
		}
		#endregion
	}
}