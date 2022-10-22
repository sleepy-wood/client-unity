using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

using Broccoli.Generator;

namespace Broccoli.Pipe {
	/// <summary>
	/// Sprout generator element.
	/// </summary>
	[System.Serializable]
	public class SproutGeneratorElement : PipelineElement, ISproutGroupConsumer {
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
			get { return PipelineElement.ElementType.SproutGenerator; }
		}
		/// <summary>
		/// Gets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public override ClassType classType {
			get { return PipelineElement.ClassType.SproutGenerator; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.structureGeneratorWeight + 10; }
		}
		/// <summary>
		/// Gets a value indicating whether this <see cref="Broccoli.Pipe.SproutGeneratorElement"/> uses randomization.
		/// </summary>
		/// <value><c>true</c> if uses randomization; otherwise, <c>false</c>.</value>
		public override bool usesRandomization {
			get { return true; }
		}
		/// <summary>
		/// Gets a value indicating whether this <see cref="Broccoli.Pipe.SproutGeneratorElement"/> unique on pipeline.
		/// </summary>
		/// <value><c>true</c> if unique on pipeline; otherwise, <c>false</c>.</value>
		public override bool uniqueOnPipeline { 
			get { return false; } 
		}
		/// <summary>
		/// The maximum number of sprouts to generate per branch unit.
		/// </summary>
		public int maxFrequency = 1;
		/// <summary>
		/// The minimum number of sprouts to generate per branch unit.
		/// </summary>
		public int minFrequency = 1;
		/// <summary>
		/// The sprout distribution mode.
		/// </summary>
		public SproutGenerator.Distribution distribution = 
			SproutGenerator.Distribution.Alternative;
		/// <summary>
		/// Variance applied to spacing variation to branches belonging to a distribuition group.
		/// </summary>
		[Range(0f,1f)]
		public float distributionSpacingVariance = 0f;
		/// <summary>
		/// Variance applied to angle variation to branches belonging to a distribuition group.
		/// </summary>
		[Range(0f,1f)]
		public float distributionAngleVariance = 0f;
		/// <summary>
		/// The whorled step.
		/// </summary>
		public int whorledStep = 3;
		/// <summary>
		/// The distribution curve.
		/// </summary>
		public AnimationCurve distributionCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
		/// <summary>
		/// The minimum twirl used for the sprouts.
		/// </summary>
		[Range(-1f,1f)]
		[FormerlySerializedAs("twirl")]
		public float minTwirl = 0f;
		/// <summary>
		/// The maximum twirl used for the sprouts.
		/// </summary>
		[Range(-1f,1f)]
		[FormerlySerializedAs("twirl")]
		public float maxTwirl = 0f;
		/// <summary>
		/// The twirl used for the sprouts.
		/// </summary>
		public float twirl {
			get { return maxTwirl; }
			set { minTwirl = value; maxTwirl = value; }
		}
		/// <summary>
		/// Minimum grade of alignment wit the parent branch at top of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		[FormerlySerializedAs("parallelAlignAtTop")]
		public float minParallelAlignAtTop = 0f;
		/// <summary>
		/// Maximum grade of alignment wit the parent branch at top of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		[FormerlySerializedAs("parallelAlignAtTop")]
		public float maxParallelAlignAtTop = 0f;
		/// <summary>
		/// Grade of alignment wit the parent branch at top of the parent branch.
		/// </summary>
		public float parallelAlignAtTop {
			get { return maxParallelAlignAtTop; }
			set { minParallelAlignAtTop = value; maxParallelAlignAtTop = value; }
		}
		/// <summary>
		/// Minimum grade of alignment wit the parent branch at base of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		[FormerlySerializedAs("parallelAlignAtBase")]
		public float minParallelAlignAtBase = 0f;
		/// <summary>
		/// Maximum grade of alignment wit the parent branch at base of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		[FormerlySerializedAs("parallelAlignAtBase")]
		public float maxParallelAlignAtBase = 0f;
		/// <summary>
		/// Grade of alignment wit the parent branch at base of the parent branch.
		/// </summary>
		public float parallelAlignAtBase {
			get { return maxParallelAlignAtBase; }
			set { minParallelAlignAtBase = value; maxParallelAlignAtBase = value; }
		}
		/// <summary>
		/// The parallel align curve.
		/// </summary>
		public AnimationCurve parallelAlignCurve = AnimationCurve.Linear(0f,0f,1f,1f);
		/// <summary>
		/// Minimum grade of alignment against the gravity at top of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		[FormerlySerializedAs("gravityAlignAtTop")]
		public float minGravityAlignAtTop = 0.25f;
		/// <summary>
		/// Maximum grade of alignment against the gravity at top of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		[FormerlySerializedAs("gravityAlignAtTop")]
		public float maxGravityAlignAtTop = 0.25f;
		/// <summary>
		/// Grade of alignment against the gravity at top of the parent branch.
		/// </summary>
		public float gravityAlignAtTop {
			get { return maxGravityAlignAtTop; }
			set { minGravityAlignAtTop = value; maxGravityAlignAtTop = value; }
		}
		/// <summary>
		/// Minimum grade of alignment against the gravity at base of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		[FormerlySerializedAs("gravityAlignAtBase")]
		public float minGravityAlignAtBase = 0.25f;
		/// <summary>
		/// Maximum grade of alignment against the gravity at base of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		[FormerlySerializedAs("gravityAlignAtBase")]
		public float maxGravityAlignAtBase = 0.25f;
		/// <summary>
		/// Grade of alignment against the gravity at base of the parent branch.
		/// </summary>
		public float gravityAlignAtBase {
			get { return maxGravityAlignAtBase; }
			set { minGravityAlignAtBase = value; maxGravityAlignAtBase = value; }
		}
		/// <summary>
		/// The gravity align curve.
		/// </summary>
		public AnimationCurve gravityAlignCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
		/// <summary>
		/// True when the sprout has its origin at the center of the branch; when false the
		/// branch has its origin at the surface of it (girth dependant).
		/// </summary>
		public bool fromBranchCenter = false;
		/// <summary>
		/// The distribution origin on the branch for the sprouts to generate.
		/// </summary>
		public SproutGenerator.DistributionOrigin distributionOrigin = 
			SproutGenerator.DistributionOrigin.FromTipBranches;
		/// <summary>
		/// If true then the sprouts go beyond their origin branch.
		/// </summary>
		public bool spreadEnabled = false;
		/// <summary>
		/// The spread range (1 is the whole hierarchy).
		/// </summary>
		[Range (0,1)]
		public float spreadRange = 0.2f;
		/// <summary>
		/// The sprout seeds.
		/// </summary>
		public List<SproutSeed> sproutSeeds = new List<SproutSeed> ();
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.SproutGeneratorElement"/> class.
		/// </summary>
		public SproutGeneratorElement () {}
		#endregion

		#region Validation
		/// <summary>
		/// Validate this instance.
		/// </summary>
		public override bool Validate () {
			log.Clear ();
			if (sproutSeeds.Count == 0) {
				log.Enqueue (LogItem.GetWarnItem ("There sprout seed are empty. " +
					"Add an entry and assign it to a sprout group to generate sprouts."));
			} else {
				bool allAssigned = true;
				for (int i = 0; i < sproutSeeds.Count; i++) {
					if (sproutSeeds[i].groupId <= 0) {
						allAssigned = false;
						break;
					}
				}
				if (!allAssigned) {
					log.Enqueue (LogItem.GetWarnItem ("Not all sprout seed entries are assigned to a sprout group."));
				}
			}
			return true;
		}
		#endregion

		#region Sprout Seeds
		/// <summary>
		/// Adds a sprout seed.
		/// </summary>
		/// <param name="sproutGroup">Sprout group.</param>
		public void AddSproutSeed (SproutSeed sproutGroup) {
			sproutSeeds.Add (sproutGroup);
		}
		/// <summary>
		/// Removes a sprout seed.
		/// </summary>
		/// <param name="listIndex">List index.</param>
		public void RemoveSproutSeed (int listIndex) {
			SproutSeed sproutGroup = sproutSeeds [listIndex];
			sproutSeeds.RemoveAt (listIndex);
		}
		/// <summary>
		/// Gets the color of the sprout group.
		/// </summary>
		/// <returns>The sprout group color.</returns>
		/// <param name="groupId">Group identifier.</param>
		public Color GetSproutGroupColor (int groupId) {
			if (pipeline != null) {
				return pipeline.sproutGroups.GetSproutGroupColor (groupId);
			}
			return Color.black;
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			SproutGeneratorElement clone = ScriptableObject.CreateInstance<SproutGeneratorElement> ();
			SetCloneProperties (clone);
			clone.maxFrequency = maxFrequency;
			clone.minFrequency = minFrequency;
			clone.distribution = distribution;
			clone.distributionSpacingVariance = distributionSpacingVariance;
			clone.distributionAngleVariance = distributionAngleVariance;
			clone.whorledStep = whorledStep;
			clone.distributionCurve = new AnimationCurve (distributionCurve.keys);
			clone.minTwirl = minTwirl;
			clone.maxTwirl = maxTwirl;
			clone.minParallelAlignAtTop = minParallelAlignAtTop;
			clone.maxParallelAlignAtTop = maxParallelAlignAtTop;
			clone.minParallelAlignAtBase = minParallelAlignAtBase;
			clone.maxParallelAlignAtBase = maxParallelAlignAtBase;
			clone.parallelAlignCurve = new AnimationCurve (parallelAlignCurve.keys);
			clone.minGravityAlignAtTop = minGravityAlignAtTop;
			clone.maxGravityAlignAtTop = maxGravityAlignAtTop;
			clone.minGravityAlignAtBase = minGravityAlignAtBase;
			clone.maxGravityAlignAtBase = maxGravityAlignAtBase;
			clone.gravityAlignCurve = new AnimationCurve (gravityAlignCurve.keys);
			clone.fromBranchCenter = fromBranchCenter;
			clone.distributionOrigin = distributionOrigin;
			clone.spreadEnabled = spreadEnabled;
			clone.spreadRange = spreadRange;
			clone.sproutSeeds.Clear ();
			for (int i = 0; i < sproutSeeds.Count; i++) {
				clone.sproutSeeds.Add (sproutSeeds[i].Clone ());
			}
			return clone;
		}
		#endregion

		#region Sprout Group Consumer
		/// <summary>
		/// Look if certain sprout group is being used in this element.
		/// </summary>
		/// <returns><c>true</c>, if sprout group is being used, <c>false</c> otherwise.</returns>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public bool HasSproutGroupUsage (int sproutGroupId) {
			for (int i = 0; i < sproutSeeds.Count; i++) {
				if (sproutSeeds[i].groupId == sproutGroupId)
					return true;
			}
			return false;
		}
		/// <summary>
		/// Commands the element to stop using certain sprout group.
		/// </summary>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public void StopSproutGroupUsage (int sproutGroupId) {
			for (int i = 0; i < sproutSeeds.Count; i++) {
				if (sproutSeeds[i].groupId == sproutGroupId) {
					#if UNITY_EDITOR
					UnityEditor.Undo.RecordObject (this, "Sprout Group Removed from Seed");
					#endif
					sproutSeeds[i].groupId = 0;
				}
			}
		}
		#endregion
	}
}