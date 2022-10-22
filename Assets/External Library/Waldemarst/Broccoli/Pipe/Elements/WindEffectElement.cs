using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Wind effect element.
	/// </summary>
	[System.Serializable]
	public class WindEffectElement : PipelineElement {
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
			get { return PipelineElement.ElementType.MeshTransform; }
		}
		/// <summary>
		/// Gets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public override ClassType classType {
			get { return PipelineElement.ClassType.WindEffect; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.effectWeight; }
		}
		/// <summary>
		/// The wind spread.
		/// </summary>
		[Range (0f, 2f)]
		public float windSpread = 1f;
		/// <summary>
		/// The wind amplitude.
		/// </summary>
		[Range (0f, 3f)]
		public float windAmplitude = 1f;
		/// <summary>
		/// The sprout turbulence.
		/// </summary>
		[Range (0f, 2f)]
		public float sproutTurbulence = 1f;
		/// <summary>
		/// The sprout sway from side to side.
		/// </summary>
		[Range (0f, 2f)]
		public float sproutSway = 1f;
		/// <summary>
		/// The branch sway from side to side.
		/// </summary>
		[Range (0f, 4f)]
		public float branchSway = 1f;
		public enum WindQuality {
			None,
			Fastest,
			Fast,
			Better,
			Best,
			Palm
		}
		public WindQuality windQuality = WindQuality.Better;
		/// <summary>
		/// Creates groups for swaying branches right from the trunk.
		/// </summary>
		public bool useMultiPhaseOnTrunk = false;
		/// <summary>
		/// For previewing wind on the preview tree all the time (if wind zones are available).
		/// </summary>
		public bool previewWindAlways = false;
		/// <summary>
		/// The animation curve.
		/// </summary>
		public AnimationCurve windFactorCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
		/// <summary>
		/// Flag to apply wind mapping to roots.
		/// </summary>
		public bool applyToRoots = false;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.WindEffectElement"/> class.
		/// </summary>
		public WindEffectElement () {}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			WindEffectElement clone = ScriptableObject.CreateInstance<WindEffectElement> ();
			SetCloneProperties (clone);
			clone.windSpread = windSpread;
			clone.windAmplitude = windAmplitude;
			clone.sproutTurbulence = sproutTurbulence;
			clone.sproutSway = sproutSway;
			clone.branchSway = branchSway;
			clone.useMultiPhaseOnTrunk = useMultiPhaseOnTrunk;
			clone.previewWindAlways = previewWindAlways;
			clone.windFactorCurve = new AnimationCurve (windFactorCurve.keys);
			clone.windQuality = windQuality;
			clone.applyToRoots = applyToRoots;
			return clone;
		}
		#endregion
	}
}