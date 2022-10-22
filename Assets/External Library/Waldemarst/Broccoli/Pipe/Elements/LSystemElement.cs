using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Utils;

namespace Broccoli.Pipe {
	/// <summary>
	/// L system element.
	/// </summary>
	[System.Serializable]
	public class LSystemElement : PipelineElement, IStructureGenerator {
		#region Vars
		/// <summary>
		/// Gets the type of the connection.
		/// </summary>
		/// <value>The type of the connection.</value>
		public override ConnectionType connectionType {
			get { return PipelineElement.ConnectionType.Source; }
		}
		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		/// <value>The type of the element.</value>
		public override ElementType elementType {
			get { return PipelineElement.ElementType.StructureGenerator; }
		}
		/// <summary>
		/// Gets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public override ClassType classType {
			get { return PipelineElement.ClassType.LSystem; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.structureGeneratorWeight;	}
		}
		/// <summary>
		/// Gets a value indicating whether this <see cref="Broccoli.Pipe.LSystemElement"/> uses randomization.
		/// </summary>
		/// <value><c>true</c> if uses randomization; otherwise, <c>false</c>.</value>
		public override bool usesRandomization {
			get { return true; }
		}
		/// <summary>
		/// The iterations.
		/// </summary>
		public int iterations = 2;
		/// <summary>
		/// The minimum number of iterations.
		/// </summary>
		public int minIterations = 0;
		/// <summary>
		/// The maximum number of iterations.
		/// </summary>
		public int maxIterations = 4;
		/// <summary>
		/// The axiom.
		/// </summary>
		public string axiom = "F";
		/// <summary>
		/// The rules.
		/// </summary>
		public List<LSystem.Rule> rules = new List<LSystem.Rule> ();
		/// <summary>
		/// The accumulative mode.
		/// </summary>
		public bool accumulativeMode  = true;
		/// <summary>
		/// The length.
		/// </summary>
		public float length           = 10f;
		/// <summary>
		/// The length growth.
		/// </summary>
		public float lengthGrowth     = -1.5f;
		/// <summary>
		/// The turn angle.
		/// </summary>
		public float turnAngle        = 30f;
		/// <summary>
		/// The turn angle growth.
		/// </summary>
		public float turnAngleGrowth  = 0f;
		/// <summary>
		/// The pitch angle.
		/// </summary>
		public float pitchAngle       = 30f;
		/// <summary>
		/// The pitch angle growth.
		/// </summary>
		public float pitchAngleGrowth = 0f;
		/// <summary>
		/// The roll angle.
		/// </summary>
		public float rollAngle        = 30f;
		/// <summary>
		/// The roll angle growth.
		/// </summary>
		public float rollAngleGrowth  = 0f;
		/// <summary>
		/// The catalog item identifier.
		/// </summary>
		public int catalogId = -1;
		/// <summary>
		/// If true the LSystem should create a new structure without using cache.
		/// </summary>
		public bool requiresNewStructure = false;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.LSystemElement"/> class.
		/// </summary>
		public LSystemElement () {
			// Default init values.
			rules.Add(new LSystem.Rule("F", "F[/F][\\F]")); //TODO: pass to OnAddToPipeline.
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			LSystemElement clone = ScriptableObject.CreateInstance<LSystemElement> ();
			SetCloneProperties (clone);
			clone.catalogId = catalogId;
			clone.iterations = iterations;
			clone.axiom = axiom;
			clone.rules.Clear ();
			for (int i = 0; i < rules.Count; i++) {
				clone.rules.Add (rules[i].Clone ());
			}
			clone.accumulativeMode = accumulativeMode;
			clone.length = length;
			clone.lengthGrowth = lengthGrowth;
			clone.turnAngle = turnAngle;
			clone.turnAngleGrowth = turnAngleGrowth;
			clone.pitchAngle = pitchAngle;
			clone.pitchAngleGrowth = pitchAngleGrowth;
			clone.rollAngle = rollAngle;
			clone.rollAngleGrowth = rollAngleGrowth;
			return clone;
		}
		#endregion
	}
}
