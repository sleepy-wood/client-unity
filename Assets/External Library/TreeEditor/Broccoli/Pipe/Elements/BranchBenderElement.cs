using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

using Broccoli.Model;

namespace Broccoli.Pipe {
	/// <summary>
	/// Branch bender element.
	/// </summary>
	[System.Serializable]
	public class BranchBenderElement : PipelineElement {
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
			get { return PipelineElement.ClassType.BranchBender; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.structureTransformWeight + 40; }
		}
		/// <summary>
		/// Gets a value indicating whether this <see cref="Broccoli.Pipe.BranchBenderElement"/> uses randomization.
		/// </summary>
		/// <value><c>true</c> if uses randomization; otherwise, <c>false</c>.</value>
		public override bool usesRandomization {
			get { return true; }
		}

		public bool applyJointSmoothing = true;
		public bool applyDirectionalBending = true;
		[FormerlySerializedAs("applyBranchNoise")]
		public bool applyNoise = false;

		public float smoothJointStrength = 0.75f;

		public float forceAtTips = 0.5f;
		public float forceAtTrunk = 0f;
		public Vector3 direction = Base.GlobalSettings.gravityDirection;
		public AnimationCurve hierarchyDistributionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
		[Range(-1f,1f)]
		public float horizontalAlignAtBase = 0.5f;
		[Range(0f,1f)]
		public float horizontalAlignStrength = 0.5f;
		public AnimationCurve horizontalAlignHierarchyDistributionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		[Range(-1f,1f)]
		public float verticalAlignAtTop = 0.5f;
		[Range(0f,1f)]
		public float verticalAlignStrength = 0.5f;
		public AnimationCurve verticalAlignHierarchyDistributionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public float noiseAtTop = 0.5f;
		public float noiseAtBase = 0.2f;
		public float noiseScaleAtTop = 0.5f;
		public float noiseScaleAtBase = 0.2f;

		public float smoothRootJointStrength = 0.75f;
		public float forceAtRootTips = 0.5f;
		public float forceAtRootTrunk = 0f;
		public Vector3 rootDirection = Base.GlobalSettings.gravityDirection;
		public AnimationCurve rootHierarchyDistributionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
		public float noiseAtRootBase = 0.2f;
		public float noiseAtRootBottom = 0.5f;
		public float noiseScaleAtRootBase = 0.2f;
		public float noiseScaleAtRootBottom = 0.5f;
		#endregion

		#region Delegates
		public delegate void OnBranchBending (BroccoTree tree, BranchBenderElement branchBenderElement);
		public OnBranchBending onDirectionalBending;
		public OnBranchBending onFollowUpSmoothing;
		public OnBranchBending onBranchNoise;		
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.BranchBenderElement"/> class.
		/// </summary>
		public BranchBenderElement () {}
		#endregion

		#region Clone
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			BranchBenderElement clone = ScriptableObject.CreateInstance<BranchBenderElement> ();
			SetCloneProperties (clone);
			clone.applyJointSmoothing = applyJointSmoothing;
			clone.applyDirectionalBending = applyDirectionalBending;
			clone.applyNoise = applyNoise;

			clone.smoothJointStrength = smoothJointStrength;
			clone.forceAtTips = forceAtTips;
			clone.forceAtTrunk = forceAtTrunk;
			clone.direction = direction;
			clone.hierarchyDistributionCurve = new AnimationCurve (hierarchyDistributionCurve.keys);
			clone.horizontalAlignAtBase = horizontalAlignAtBase;
			clone.horizontalAlignStrength = horizontalAlignStrength;
			clone.horizontalAlignHierarchyDistributionCurve = new AnimationCurve (horizontalAlignHierarchyDistributionCurve.keys);
			clone.verticalAlignAtTop = verticalAlignAtTop;
			clone.verticalAlignStrength = verticalAlignStrength;
			clone.verticalAlignHierarchyDistributionCurve = new AnimationCurve (verticalAlignHierarchyDistributionCurve.keys);

			clone.noiseAtBase = noiseAtBase;
			clone.noiseAtTop = noiseAtTop;
			clone.noiseScaleAtBase = noiseScaleAtBase;
			clone.noiseScaleAtTop = noiseScaleAtTop;
			
			clone.smoothRootJointStrength = smoothRootJointStrength;
			clone.forceAtRootTips = forceAtRootTips;
			clone.forceAtRootTrunk = forceAtRootTrunk;
			clone.rootDirection = rootDirection;
			clone.rootHierarchyDistributionCurve = rootHierarchyDistributionCurve;
			clone.noiseAtRootBottom = noiseAtRootBottom;
			clone.noiseAtRootBase = noiseAtRootBase;
			clone.noiseScaleAtRootBottom = noiseScaleAtRootBottom;
			clone.noiseScaleAtRootBase = noiseScaleAtRootBase;
			return clone;
		}
		#endregion
	}
}