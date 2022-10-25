using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

using Broccoli.Model;
using Broccoli.Utils;
using Broccoli.Pipe;

namespace Broccoli.Generator
{
	/// <summary>
	/// Tree structure generator. Populates the tree with a hierarchy of branches and sprouts based on 
	/// a structure level specification.
	/// </summary>
	public class StructureGenerator {
		#region StructureLevel Class
		/// <summary>
		/// Class containing the specifications for branches or sprouts generation.
		/// </summary>
		[System.Serializable]
		public class StructureLevel {
			#region Vars
			/// <summary>
			/// Identifier.
			/// </summary>
			public int id = 0;
			/// <summary>
			/// The parent identifier.
			/// </summary>
			public int parentId = 0;
			/// <summary>
			/// Id of the sharing group if the structure share its occurrence with other nodes.
			/// </summary>
			public int sharingGroupId = 0;
			/// <summary>
			/// The id of the next structure level in the sharing group, 0 means theres no sharing next.
			/// </summary>
			public int sharingNextId = 0;
			/// <summary>
			/// Enabled status for the level.
			/// </summary>
			public bool enabled = true;
			/// <summary>
			/// This structure level models sprouts.
			/// </summary>
			public bool isSprout = false;
			/// <summary>
			/// This structure level models tree roots.
			/// </summary>
			public bool isRoot = false;
			/// <summary>
			/// True is the level is locked, thus not editable.
			/// The level does not generate new sprouts, but takes them from a cache.
			/// </summary>
			public bool isLocked = false;
			/// <summary>
			/// The sprout group identifier.
			/// </summary>
			public int sproutGroupId = 0;
			/// <summary>
			/// The color of the sprout group.
			/// </summary>
			[System.NonSerialized]
			public Color sproutGroupColor = Color.clear;
			/// <summary>
			/// The probability of ocurrance for this level.
			/// </summary>
			[Range(0f, 1f)]
			public float probability = 1f;
			/// <summary>
			/// The probability to be picked up from a shared group of levels.
			/// </summary>
			[Range(0f, 1f)]
			public float sharedProbability = 0.5f;
			/// <summary>
			/// Distribution origin modes.
			/// </summary>
			public enum DistributionOrigin
			{
				FromTip,
				FromBase
			}
			/// <summary>
			/// The distribution origin mode.
			/// </summary>
			public DistributionOrigin distributionOrigin = DistributionOrigin.FromTip;
			/// <summary>
			/// Distribution mode.
			/// </summary>
			public enum Distribution
			{
				Alternative,
				Opposite,
				Whorled
			}
			/// <summary>
			/// The distribution mode used for this level.
			/// </summary>
			public Distribution distribution = Distribution.Alternative;
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
			/// The distribution curve from base to top of a branch.
			/// </summary>
			public AnimationCurve distributionCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
			/// <summary>
			/// The children (sprouts of branches) per node.
			/// </summary>
			public int childrenPerNode = 1;
			/// <summary>
			/// The minimum frequency of elements.
			/// </summary>
			public int minFrequency = 1;
			/// <summary>
			/// The maximum frequency of elements.
			/// </summary>
			public int maxFrequency = 1;
			/// <summary>
			/// Use randomized twirl offset if enabled.
			/// </summary>
			public bool randomTwirlOffsetEnabled = true;
			/// <summary>
			/// The global twirl offset.
			/// </summary>
			[Range(-1f, 1f)]
			public float twirlOffset = 0;
			/// <summary>
			/// The maximum twirl value around the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("twirl")]
			public float maxTwirl = 0;
			/// <summary>
			/// The minimum twirl value around the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("twirl")]
			public float minTwirl = 0;
			/// <summary>
			/// The twirl value around the parent branch.
			/// </summary>
			public float twirl {
				get { return maxTwirl; }
				set { minTwirl = value; maxTwirl = value; }
			}
			/// <summary>
			/// Maximum grade of alignment wit the parent branch at top of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("parallelAlignAtTop")]
			public float maxParallelAlignAtTop = 0.7f;
			/// <summary>
			/// Minimum grade of alignment wit the parent branch at top of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("parallelAlignAtTop")]
			public float minParallelAlignAtTop = 0.7f;
			/// <summary>
			/// Grade of alignment wit the parent branch at top of the parent branch.
			/// </summary>
			public float parallelAlignAtTop {
				get { return maxParallelAlignAtTop; }
				set { minParallelAlignAtTop = value; maxParallelAlignAtTop = value; }
			}
			/// <summary>
			/// Maximum grade of alignment wit the parent branch at base of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("parallelAlignAtBase")]
			public float maxParallelAlignAtBase = 0.3f;
			/// <summary>
			/// Minimum grade of alignment wit the parent branch at base of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("parallelAlignAtBase")]
			public float minParallelAlignAtBase = 0.3f;
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
			public AnimationCurve parallelAlignCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
			/// <summary>
			/// Maximum grade of alignment against the gravity at the top of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("gravityAlignAtTop")]
			public float maxGravityAlignAtTop = 0.4f;
			/// <summary>
			/// Minimum grade of alignment against the gravity at the top of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("gravityAlignAtTop")]
			public float minGravityAlignAtTop = 0.4f;
			/// <summary>
			/// Grade of alignment against the gravity at the top of the parent branch.
			/// </summary>
			public float gravityAlignAtTop {
				get { return maxGravityAlignAtTop; }
				set { minGravityAlignAtTop = value; maxGravityAlignAtTop = value; }
			}
			/// <summary>
			/// Maximum grade of alignment against the gravity at the base of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("gravityAlignAtBase")]
			public float maxGravityAlignAtBase = 0.4f;
			/// <summary>
			/// Minimum grade of alignment against the gravity at the base of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("gravityAlignAtBase")]
			public float minGravityAlignAtBase = 0.4f;
			/// <summary>
			/// Grade of alignment against the gravity at the base of the parent branch.
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
			/// Maximum grade of alignment to the horizontal plane at the top of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("horizontalAlignAtTop")]
			public float maxHorizontalAlignAtTop = 0.4f;
			/// <summary>
			/// Minimum grade of alignment to the horizontal plane at the top of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("horizontalAlignAtTop")]
			public float minHorizontalAlignAtTop = 0.4f;
			public float horizontalAlignAtTop {
				get { return maxHorizontalAlignAtTop; }
				set { minHorizontalAlignAtTop = value; maxHorizontalAlignAtTop = value; }
			}
			/// <summary>
			/// Maximum grade of alignment to the horizontal plane at the base of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("horizontalAlignAtBase")]
			public float maxHorizontalAlignAtBase = 0.4f;
			/// <summary>
			/// Minimum grade of alignment to the horizontal plane at the base of the parent branch.
			/// </summary>
			[Range(-1f, 1f)]
			[FormerlySerializedAs("horizontalAlignAtBase")]
			public float minHorizontalAlignAtBase = 0.4f;
			/// <summary>
			/// Grade of alignment to the horizontal plane at the base of the parent branch.
			/// </summary>
			public float horizontalAlignAtBase {
				get { return maxHorizontalAlignAtBase; }
				set { minHorizontalAlignAtBase = value; maxHorizontalAlignAtBase = value; }
			}
			/// <summary>
			/// The horizontal align curve.
			/// </summary>
			public AnimationCurve horizontalAlignCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
			/// <summary>
			/// Directional flip alignment value.
			/// </summary>
			[Range (0f,1f)]
			public float flipSproutAlign = 0f;
			/// <summary>
			/// Directional flip direction.
			/// </summary>
			public Vector3 flipSproutDirection = Vector3.right;
			/// <summary>
			/// The maximum length at top of the parent branch.
			/// </summary>
			[FormerlySerializedAs("lengthAtTop")]
			public float maxLengthAtTop = 4f;
			/// <summary>
			/// The minimum length at top of the parent branch.
			/// </summary>
			[FormerlySerializedAs("lengthAtTop")]
			public float minLengthAtTop = 4f;
			/// <summary>
			/// The length at top of the parent branch.
			/// </summary>
			public float lengthAtTop {
				get { return maxLengthAtTop; }
				set { minLengthAtTop = value; maxLengthAtTop = value; }
			}
			/// <summary>
			/// The maximum length at base of the parent branch.
			/// </summary>
			[FormerlySerializedAs("lengthAtBase")]
			public float maxLengthAtBase = 4f;
			/// <summary>
			/// The minimum length at base of the parent branch.
			/// </summary>
			[FormerlySerializedAs("lengthAtBase")]
			public float minLengthAtBase = 4f;
			/// <summary>
			/// The length at base of the parent branch.
			/// </summary>
			public float lengthAtBase {
				get { return maxLengthAtBase; }
				set { minLengthAtBase = value; maxLengthAtBase = value;}
			}
			/// <summary>
			/// The maximum length curve from base to top of the parent branch.
			/// </summary>
			public AnimationCurve lengthCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
			/// <summary>
			/// The minimum girth scale to apply on the branches generated by this level.
			/// </summary>
			public float minGirthScale = 1f;
			/// <summary>
			/// The maximum girth scale to apply on the branches generated by this level.
			/// </summary>
			public float maxGirthScale = 1f;
			/// <summary>
			/// Radius to spawn sprouts.
			/// </summary>
			public float radius = 0f;
			/// <summary>
			/// If true then sprouts generate from the center of the branch and not at its surface.
			/// </summary>
			public bool fromBranchCenter = false;
			/// <summary>
			/// Keeps the structure level node position on the canvas. UI helper property,
			/// </summary>
			public Vector2 nodePosition = Vector2.zero;
			/// <summary>
			/// Number of children structure levels.
			/// </summary>
			[System.NonSerialized]
			public List<StructureLevel> structureLevels = new List<StructureLevel> ();
			/// <summary>
			/// The parent structure level.
			/// </summary>
			[System.NonSerialized]
			public StructureLevel parentStructureLevel = null;
			/// <summary>
			/// The level on the hierarchy.
			/// </summary>
			[System.NonSerialized]
			public int level = 1;
			/// <summary>
			/// True if the structure level is marked for deletion.
			/// </summary>
			[System.NonSerialized]
			public bool isMarkedForDeletion = false;
			/// <summary>
			/// True if the structure level node is visible.
			/// </summary>
			[System.NonSerialized]
			public bool isDrawVisible = true;
			/// <summary>
			/// The level was visited when traversing the level structure.
			/// </summary>
			[System.NonSerialized]
			public bool isVisited = false;
			/// <summary>
			/// The level is visited and is producing structure 
			/// (based on the probability properties).
			/// </summary>
			[System.NonSerialized]
			public bool isVisitedAndActive = false;
			/// <summary>
			/// Marks a structure level as evaluated for a specific branch, 
			/// auxiliary temp var containing the id of the branch.
			/// </summary>
			[System.NonSerialized]
			public int evaluatedBranchId = -1;
			/// <summary>
			/// List of previously generated tree branches.
			/// </summary>
			/// <typeparam name="BroccoTree.Branch"></typeparam>
			/// <returns>Previously generated branches by this structure level.</returns>
			[System.NonSerialized]
			public List<BroccoTree.Branch> generatedBranches = new List<BroccoTree.Branch> ();
			#endregion

			#region Range Vars
			/// <summary>
			/// If true the level generates branches or sprouts on a range of the parent branch length.
			/// </summary>
			public bool actionRangeEnabled = false;
			/// <summary>
			/// Minimum range value to spawn the number of structures.
			/// </summary>
			[Range(0f,1f)]
			public float minRange = 0f;
			/// <summary>
			/// Maximum range value to spawn the number of structures.
			/// </summary>
			[Range(0f,1f)]
			public float maxRange = 1f;
			/// <summary>
			/// Structures falling out of this range does not get added to the parent structure.
			/// </summary>
			[Range(0f, 1f)]
			[FormerlySerializedAs("minActionRange")]
			public float minMaskRange = 0f;
			/// <summary>
			/// Structures falling out of this range does not get added to the parent structure.
			/// </summary>
			[Range(0f, 1f)]
			[FormerlySerializedAs("maxActionRange")]
			public float maxMaskRange = 1f;
			/// <summary>
			/// If true then generated branches have a change to break based on the break branch probability curve.
			/// </summary>
			public bool applyBranchBreak = false;
			/// <summary>
			/// Probability for branches generated by this structure to break. The x axis is the position of the branch at its
			/// parent branch (0 at base, 1 at top.). The y axis is the probability to break (0 to 1).
			/// </summary>
			public AnimationCurve breakBranchProbability = AnimationCurve.Linear (0f, 0f, 1f, 0f);
			/// <summary>
			/// When a generated branch breaks the minimum range to choose the breaking position.
			/// </summary>
			[Range(0f, 1f)]
			public float minBreakRange = 0.5f;
			/// <summary>
			/// When a generated branch breaks the minimum range to choose the breaking position.
			/// </summary>
			[Range(0f, 1f)]
			public float maxBreakRange = 1f;
			#endregion

			#region Noise Vars
			/// <summary>
			/// Flag to mark structures from this generator to have their own noise parameters.
			/// </summary>
			public bool overrideNoise = false;
			/// <summary>
			/// Noise value.
			/// </summary>
			public float noise = 0.5f;
			/// <summary>
			/// Noise scale.
			/// </summary>
			public float noiseScale = 0.5f;
			#endregion

			#region Ops
			/// <summary>
			/// Validate this instance.
			/// </summary>
			public void Validate () {
				if (minFrequency < 0)
					minFrequency = 0;
				if (maxFrequency < minFrequency)
					maxFrequency = minFrequency;
				if (minRange < 0f)
					minRange = 0f;
				if (maxRange > 1f)
					maxRange = 1f;
				if (maxRange < minRange)
					maxRange = minRange;
				if (minMaskRange < 0f)
					minMaskRange = 0f;
				if (maxMaskRange > 1f)
					maxMaskRange = 1f;
				if (maxMaskRange < minMaskRange)
					maxMaskRange = minMaskRange;
			}
			/// <summary>
			/// Ins the range.
			/// </summary>
			/// <returns><c>true</c>, if range was ined, <c>false</c> otherwise.</returns>
			/// <param name="position">Position.</param>
			public bool InRange (float position) {
				if (position <= maxMaskRange) {
					if (minMaskRange == 0 && position >= minMaskRange) {
						return true;
					} else if (position > minMaskRange) {
						return true;
					}
				}
				return false;
			}
			/// <summary>
			/// Determines whether this node shares its occurrence probability.
			/// </summary>
			/// <returns><c>true</c> if this instance is shared; otherwise, <c>false</c>.</returns>
			public bool IsShared () {
				return (this.sharingGroupId != 0 || this.sharingNextId != 0);
			}
			/// <summary>
			/// Determines whether this node shares its occurrence probability and
			/// is not the group representant.
			/// </summary>
			/// <returns><c>true</c> if this instance is shared not main; otherwise, <c>false</c>.</returns>
			public bool IsSharedNotMain () {
				return this.sharingGroupId != 0;
			}
			/// <summary>
			/// Returns the id of the representant of the share group if the node belongs to one.
			/// </summary>
			/// <returns>Id of the representant of the share group.</returns>
			public int GetSharedGroupIdOrMainId () {
				if (sharingGroupId != 0)
					return sharingGroupId;
				else if (sharingNextId != 0)
					return id;
				else
					return 0;
			}
			public int GetMainId () {
				if (sharingGroupId != 0)
					return sharingGroupId;
				else
					return id;
			}
			// TODO: remove
			public bool CommitBranchCurve (int branchId, BezierCurve bezierCurve) {
				foreach (BroccoTree.Branch branch in generatedBranches) {
					if (branch.id == branchId) {
						branch.curve = bezierCurve.Clone ();
						return true;
					}
				}
				return false;
			}
			#endregion

			#region Traversing
			/// <summary>
			/// Gets the first branch structure level in the children nodes.
			/// </summary>
			/// <returns>First branch structure level or null if none is found.</returns>
			public StructureLevel GetFirstBranchStructureLevel () {
				for (int i = 0; i < structureLevels.Count; i++) {
					if (!structureLevels[i].isSprout)
						return structureLevels[i];
				}
				return null;
			}
			/// <summary>
			/// Gets a sprout structure at the given index order in the children nodes.
			/// </summary>
			/// <param name="index">Index order of the sprout to look for.</param>
			/// <returns>Sprout structure levelat the index requested.</returns>
			public StructureLevel GetSproutStructureLevel (int index) {
				int sproutIndex = 0;
				for (int i = 0; i < structureLevels.Count; i++) {
					if (structureLevels[i].isSprout)
						if (sproutIndex == index) {
							return structureLevels[i];
						} else {
							sproutIndex++;
						}
				}
				return null;
			}
			/// <summary>
			/// Gets the first sprout structure level in the children nodes.
			/// </summary>
			/// <returns>First sprout structure level or null if none is found.</returns>
			public StructureLevel GetFirstSproutStructureLevel () {
				return GetSproutStructureLevel (0);
			}
			#endregion

			#region Clone
			/// <summary>
			/// Clone this instance.
			/// </summary>
			public StructureLevel Clone() {
				StructureLevel clone = new StructureLevel ();
				clone.id = id;
				clone.parentId = parentId;
				clone.sharingGroupId = sharingGroupId;
				clone.sharingNextId = sharingNextId;
				clone.enabled = enabled;
				clone.isSprout = isSprout;
				clone.isRoot = isRoot;
				clone.radius = radius;
				clone.fromBranchCenter = fromBranchCenter;
				clone.sproutGroupId = sproutGroupId;
				clone.probability = probability;
				clone.sharedProbability = sharedProbability;
				clone.distributionOrigin = distributionOrigin;
				clone.distribution = distribution;
				clone.distributionSpacingVariance = distributionSpacingVariance;
				clone.distributionAngleVariance = distributionAngleVariance;
				clone.distributionCurve = new AnimationCurve (distributionCurve.keys);
				clone.childrenPerNode = childrenPerNode;
				clone.minFrequency = minFrequency;
				clone.maxFrequency = maxFrequency;
				clone.randomTwirlOffsetEnabled = randomTwirlOffsetEnabled;
				clone.twirlOffset = twirlOffset;
				clone.maxTwirl = maxTwirl;
				clone.minTwirl = minTwirl;
				clone.maxParallelAlignAtTop = maxParallelAlignAtTop;
				clone.minParallelAlignAtTop = minParallelAlignAtTop;
				clone.maxParallelAlignAtBase = maxParallelAlignAtBase;
				clone.minParallelAlignAtBase = minParallelAlignAtBase;
				clone.parallelAlignCurve = new AnimationCurve (parallelAlignCurve.keys);
				clone.maxGravityAlignAtTop = maxGravityAlignAtTop;
				clone.minGravityAlignAtTop = minGravityAlignAtTop;
				clone.maxGravityAlignAtBase = maxGravityAlignAtBase;
				clone.minGravityAlignAtBase = minGravityAlignAtBase;
				clone.gravityAlignCurve = new AnimationCurve (gravityAlignCurve.keys);
				clone.maxHorizontalAlignAtTop = maxHorizontalAlignAtTop;
				clone.minHorizontalAlignAtTop = minHorizontalAlignAtTop;
				clone.maxHorizontalAlignAtBase = maxHorizontalAlignAtBase;
				clone.minHorizontalAlignAtBase = minHorizontalAlignAtBase;
				clone.horizontalAlignCurve = new AnimationCurve (horizontalAlignCurve.keys);
				clone.flipSproutAlign = flipSproutAlign;
				clone.flipSproutDirection = flipSproutDirection;
				clone.maxLengthAtTop = maxLengthAtTop;
				clone.minLengthAtTop = minLengthAtTop;
				clone.maxLengthAtBase = maxLengthAtBase;
				clone.minLengthAtBase = minLengthAtBase;
				clone.lengthCurve = new AnimationCurve (lengthCurve.keys);
				clone.minGirthScale = minGirthScale;
				clone.maxGirthScale = maxGirthScale;
				clone.actionRangeEnabled = actionRangeEnabled;
				clone.minRange = minRange;
				clone.maxRange = maxRange;
				clone.minMaskRange = minMaskRange;
				clone.maxMaskRange = maxMaskRange;
				clone.applyBranchBreak = applyBranchBreak;
				clone.breakBranchProbability = new AnimationCurve (breakBranchProbability.keys);
				clone.minBreakRange = minBreakRange;
				clone.maxBreakRange = maxBreakRange;
				clone.nodePosition = nodePosition;
				clone.isLocked = isLocked;
				clone.overrideNoise = overrideNoise;
				clone.noise = noise;
				clone.noiseScale = noiseScale;
				return clone;
			}
			#endregion
		}
		/// <summary>
		/// Class containing a structure unit generated from a StructureLevel specification.
		/// </summary>
		[System.Serializable]
		public class Structure {
			#region Vars
			/// <summary>
			/// Id of this structure.
			/// </summary>
			public int id = 0;
			/// <summary>
			/// Guid of this structure if represents a branch.
			/// </summary>
			public System.Guid guid {
				get {
					if (branch != null) return branch.guid;
					return System.Guid.Empty;
				}
			}
			/// <summary>
			/// Id of the StructureLevel generating this structure.
			/// </summary>
			public int generatorId = 0;
			/// <summary>
			/// Id of the main StructureLevel generating this structure when in a shared group.
			/// </summary>
			public int mainGeneratorId = 0;
			/// <summary>
			/// Id of the parent structure.
			/// </summary>
			public int parentStructureId = 0;
			/// <summary>
			/// True if the structure has been generated from a shared group of StructureLevel entities.
			/// </summary>
			public bool isSharedGenerator = false;
			/// <summary>
			/// Index for this structure on the generated structure list.
			/// </summary>
			public int sproutChildIndex = -1;
			/// <summary>
			/// True if the structure has been edited.
			/// </summary>
			public bool isTuned = false;
			/// <summary>
			/// Random state to use if the structure is tuned.
			/// </summary>
			public Random.State randomState;
			/// <summary>
			/// Position offset from is parent structure.
			/// </summary>
			public Vector3 positionOffset = Vector3.zero;
			/// <summary>
			/// When the structure is a branch, this property holds a clone from the generated branch.
			/// </summary>
			public BroccoTree.Branch branch = null;
			/// <summary>
			/// Parent structure.
			/// </summary>
			[System.NonSerialized]
			public Structure parentStructure = null;
			/// <summary>
			/// Id to the parent structure, if set it is used to flat serialization.
			/// </summary>
			public int structureId = -1;
                        /// <summary>
                        /// Children structures.
                        /// </summary>
                        /// <typeparam name="Structure"></typeparam>
                        /// <returns></returns>
                        [System.NonSerialized]
                        public List<Structure> childrenStructures = new List<Structure> ();
                        /// <summary>
                        /// Children terminal structures.
                        /// </summary>
                        /// <typeparam name="TerminalStructure"></typeparam>
                        /// <returns></returns>
                        [System.NonSerialized]
                        public List<TerminalStructure> childrenTerminalStructures = new List<TerminalStructure> ();
			#endregion
			#region Clone
			public Structure Clone () {
				Structure clone = new Structure ();
				clone.id = id;
				clone.generatorId = generatorId;
				clone.mainGeneratorId = mainGeneratorId;
				clone.parentStructureId = parentStructureId;
				clone.isSharedGenerator = isSharedGenerator;
				clone.sproutChildIndex = sproutChildIndex;
				clone.isTuned = isTuned;
				clone.randomState = randomState;
				clone.positionOffset = positionOffset;
				clone.branch = branch.PlainClone ();
				return clone;
			}
			#endregion
		}
		/// <summary>
		/// Class containing a terminal structure unit generated from a StructureLevel specification.
		/// </summary>
		[System.Serializable]
		public class TerminalStructure {
			#region Vars
			/// <summary>
			/// Id of this structure.
			/// </summary>
			public int id = 0;
			/// <summary>
			/// Id of the StructureLevel generating this structure.
			/// </summary>
			public int generatorId = 0;
			/// <summary>
			/// Main id of the StructureLevel generating this structure when in a shared group.
			/// </summary>
			public int mainGeneratorId = 0;
			/// <summary>
			/// Id of the parent structure.
			/// </summary>
			public int parentStructureId = 0;
			/// <summary>
			/// True if the structure has been generated from a shared group of StructureLevel entities.
			/// </summary>
			public bool isSharedGenerator = false;
			/// <summary>
			/// Random state to generate the terminal structure objects.
			/// </summary>
			public Random.State randomState;
			/// <summary>
			/// Parent structure.
			/// </summary>
			[System.NonSerialized]
			public Structure parentStructure = null;
			#endregion
			#region Clone
			public Structure Clone () {
				Structure clone = new Structure ();
				clone.id = id;
				clone.generatorId = generatorId;
				clone.mainGeneratorId = mainGeneratorId;
				clone.parentStructureId = parentStructureId;
				clone.isSharedGenerator = isSharedGenerator;
				clone.randomState = randomState;
				return clone;
			}
			#endregion
		}
		#endregion

		#region Vars
		/// <summary>
		/// The reference position.
		/// </summary>
		public Vector3 referencePosition = Vector3.zero;
		/// <summary>
		/// If true the random state is loaded from the tuned parent structure before generating
		/// new children structures.
		/// </summary>
		public bool useParentStructureRandomState = false;
		/// <summary>
		/// The identifier to level relationship.
		/// </summary>
		public Dictionary<int, StructureLevel> idToStructureLevel = 
			new Dictionary<int, StructureLevel> ();
		/// <summary>
		/// The positions.
		/// </summary>
		public List<Position> positions = new List<Position> ();
		/// <summary>
		/// The global scale.
		/// </summary>
		public float globalScale = 1f;
		/// <summary>
		/// The branch identifier count.
		/// </summary>
		int branchIdCount = 0;
		// Id to control the counting of structures.
		int structureIdCount = 0;
		/// <summary>
		/// List of already assigned ids used on tuned structures.
		/// </summary>
		/// <typeparam name="int">Id of the tuned structure.</typeparam>
		/// <returns>List of ids assigned to tuned structures.</returns>
		List<int> existingStructureId = new List<int> ();
		#endregion

		#region Singleton
		/// <summary>
		/// The structure generator singleton.
		/// </summary>
		static StructureGenerator _structureGenerator = null;
		/// <summary>
		/// Gets the singleton instance.
		/// </summary>
		/// <returns>The singleton instance.</returns>
		public static StructureGenerator GetInstance() {
			if (_structureGenerator == null) {
				_structureGenerator = new StructureGenerator ();
			}
			return _structureGenerator;
		}
		#endregion

		#region Preparing methods
		/// <summary>
		/// Clears this instance.
		/// </summary>
		public void Clear () {
			idToStructureLevel.Clear ();
			existingStructureId.Clear ();
			//positions.Clear ();
			branchIdCount = 0;
		}
		/// <summary>
		/// Prepares a StructureLevel and its structureLevels recursively to be processed.
		/// </summary>
		/// <param name="structureLevel">StructureLevel to prepare.</param>
		/// <param name="level">Hierarchy level for the StructureLevel.</param>
		protected void PrepareStructureLevelsRecursive (StructureLevel structureLevel, int level) {
			structureLevel.level = level;
			// Initializes each structure level setting isVisited, isVisitedAndActive
			for (int i = 0; i < structureLevel.structureLevels.Count; i++) {
				idToStructureLevel.Add (structureLevel.structureLevels[i].id, structureLevel.structureLevels[i]);
				structureLevel.structureLevels[i].evaluatedBranchId = -1;
				structureLevel.structureLevels[i].isVisited = false;
				structureLevel.structureLevels[i].isVisitedAndActive = false;
				if (!structureLevel.structureLevels[i].isSprout && structureLevel.structureLevels[i].structureLevels.Count > 0) {
					PrepareStructureLevelsRecursive (structureLevel.structureLevels[i], level + 1);
				}
			}
		}
		/// <summary>
		/// Prepares structures and their children structures recursively to be processed.
		/// </summary>
		/// <param name="structures">Structure to prepare.</param>
		private void PrepareStructuresRecursive (List<Structure> structures) {
			// Gets the last id used by the given structures.
			for (int i = 0; i < structures.Count; i++) {
				if (structures[i].id >= structureIdCount) {
					structureIdCount = structures[i].id;
				}
				// Adds the id for the structure to the already assigned ids.
				if (structures [i].isTuned) {
					existingStructureId.Add (structures [i].id);
				}
				structures [i].childrenTerminalStructures.Clear ();
				PrepareStructuresRecursive (structures[i].childrenStructures);
			}
		}
		#endregion

		#region Structure methods
		/// <summary>
		/// Get the next structure id.
		/// </summary>
		/// <returns>StructureId</returns>
		private int GetNextStructureId () {
			structureIdCount++;
			while (existingStructureId.Contains (structureIdCount)) {
				structureIdCount++;
			}
			return structureIdCount;
		}
		/// <summary>
		/// Generates the branch and sprout structures to be used to build a the tree.
		/// </summary>
		/// <param name="structures">List of root structures.</param>
		/// <param name="structureLevel">Structure level containing the root rules to build branches.</param>
		/// <returns>List of structures to build a tree.</returns>
		public List<Structure> GenerateStructures (List<Structure> structures, StructureLevel structureLevel) {
			Clear ();
			PrepareStructureLevelsRecursive (structureLevel, 0);
			PrepareStructuresRecursive (structures);
			structures = GenerateStructuresRecursive (structures, structureLevel, null);
			return structures;
		}
		/// <summary>
		/// Generate the children structures for a parent structure using a StructureLevel as rules.
		/// It recursively calls this function on the created children structures.
		/// </summary>
		/// <param name="structures">Structures to generate.</param>
		/// <param name="structureLevel">Rules to generate the structures.</param>
		/// <param name="parentStructure">Parent structure.</param>
		/// <returns>List of generated structures.</returns>
		public List<Structure> GenerateStructuresRecursive (List<Structure> structures, 
			StructureLevel structureLevel, 
			Structure parentStructure) 
		{
			// Validation
			if (structures == null) return structures;
			if (parentStructure != null && parentStructure.isTuned && structures.Count == 0) return structures;

			// If parent structure is tuned then set random init state.
			if (useParentStructureRandomState && parentStructure != null && parentStructure.isTuned) {
				Random.state = parentStructure.randomState;
			}

			// StructureLevel that generated the structures (could change when using shared StructureLevel)
			StructureLevel originStructureLevel = null;

			// Candidate branches.
			List<BroccoTree.Branch> candidateBranches = new List<BroccoTree.Branch> ();

			// Frequency or number of candidates to generate.
			int frequency = 0;

			// Count how many tuned structures there are and saves the index 
			// of the StructureLevel that generated the structures (if it is a shared StructureLevel)
			int tunedStructures = 0;
			int originStructureLevelIndex = -1;
			foreach (Structure structure in structures) {
				if (structure.isTuned && BelongsToStructureLevel (structure.generatorId, structureLevel)) {
					tunedStructures++;
					originStructureLevelIndex = structure.generatorId;
				}
			}

			// If there are no tuned structures then we create them from the candidates.
			if (tunedStructures == 0) { // No tuned structures found, meaning we will create anew
				// Get the main structure level, if generated from a shared generator.
				originStructureLevel = GetStructureLevel (structureLevel);

				// If the structure level is a sprout, just return the sprout structures.
				if (originStructureLevel.isSprout) {
					TerminalStructure sproutStructure = new TerminalStructure ();
					sproutStructure.randomState = Random.state;
					sproutStructure.id = GetNextStructureId ();
					sproutStructure.generatorId = structureLevel.id;
					// If the structure comes from a shared structure, assign its main generator id.
					if (originStructureLevel.id != structureLevel.id) {
						sproutStructure.mainGeneratorId = originStructureLevel.id;
						sproutStructure.isSharedGenerator = true;
					}
					parentStructure.childrenTerminalStructures.Add (sproutStructure);
					return structures;
				}
				// If the structure level is a branch generator, create the candidate branches. 
				else {
					bool occurred = false;
					candidateBranches = GenerateBranchCandidates (originStructureLevel, (parentStructure!=null?parentStructure.branch:null), out occurred);
					frequency = candidateBranches.Count;
				}
			}
			// If there are tuned structures, branch merging with existing structures is required.
			else {
				// Get the main structure level, if generated from a shared generator.
				originStructureLevel = GetStructureLevel (structureLevel, originStructureLevelIndex);
				// Get the number of candidates to generate.
				frequency = Random.Range (originStructureLevel.minFrequency, originStructureLevel.maxFrequency + 1);
				// Generate the candidates if the frequency is not already covered by the tuned structures.
				if (frequency > tunedStructures) {
					bool occurred = false;
					candidateBranches = GenerateBranchCandidates (originStructureLevel, (parentStructure!=null?parentStructure.branch:null), out occurred, frequency);
				}
			}

			// For branch structures, merge existing structures with the candidates.
			List<Structure> mergedStructures = new List<Structure> ();
			int tunedCount = 0;
			// For each existing structure, add tuned to the merge.
			for (int _i = 0; _i < structures.Count; _i++) {
				// If the structure belongs to this structure level generator and is tuned, add to the merge.
				if (BelongsToStructureLevel (structures[_i].generatorId, structureLevel) && structures[_i].isTuned) {
					tunedCount++;
					// Add the tuned structure.
					mergedStructures.Add (structures[_i]);
					if (parentStructure != null) {
						structures[_i].parentStructureId = parentStructure.id;
						structures[_i].parentStructure = parentStructure;
					}
					if (!useParentStructureRandomState) {
						structures[_i].randomState = Random.state;
					}
				}
			}
			// There are candidate branches to add to the tuned branches.
			if (mergedStructures.Count < candidateBranches.Count) {
				int candI = 0; // Candidate index.
				int tunI = 0; // Tuned index.
				// While there are candidates to complete.
				while (candI < candidateBranches.Count && mergedStructures.Count < candidateBranches.Count) {
					// Add candidates above tuned branches.
					if (tunedCount > 0 && tunI < mergedStructures.Count && candidateBranches [candI].position > mergedStructures [tunI].branch.position) {
						// Create the merged structure.
						Structure mergedStructure = new Structure ();
						mergedStructure.id = GetNextStructureId ();
						mergedStructure.branch = candidateBranches [candI];
						mergedStructure.branch.id = mergedStructure.id;
						mergedStructure.branch.isRoot = originStructureLevel.isRoot;
						mergedStructure.generatorId = originStructureLevel.id;
						mergedStructure.mainGeneratorId = originStructureLevel.GetMainId ();
						if (mergedStructure.generatorId == 0) {
							SetRootPositionAndDirection (mergedStructure, structureLevel);
						}
						if (parentStructure == null) {
							mergedStructure.parentStructureId = -1;
							mergedStructure.parentStructure = null;
						} else {
							mergedStructure.parentStructureId = parentStructure.id;
							mergedStructure.parentStructure = parentStructure;
						}
						mergedStructure.isSharedGenerator = originStructureLevel.IsShared();
						mergedStructure.randomState = Random.state;
						// Add the structure to the merges.
						mergedStructures.Add (mergedStructure);
					}
					// Shuffling between existing tuned branch position.
					else {
						if (tunI < tunedCount) tunI++;
						else {
							// Create the merged structure.
							Structure mergedStructure = new Structure ();
							mergedStructure.id = GetNextStructureId ();
							mergedStructure.branch = candidateBranches [candI];
							mergedStructure.branch.id = mergedStructure.id;
							mergedStructure.branch.isRoot = originStructureLevel.isRoot;
							mergedStructure.generatorId = originStructureLevel.id;
							mergedStructure.mainGeneratorId = originStructureLevel.GetMainId ();
							if (mergedStructure.generatorId == 0) {
								SetRootPositionAndDirection (mergedStructure, structureLevel);
							}
							if (parentStructure == null) {
								mergedStructure.parentStructureId = -1;
								mergedStructure.parentStructure = null;
							} else {
								mergedStructure.parentStructureId = parentStructure.id;
								mergedStructure.parentStructure = parentStructure;
							}
							mergedStructure.isSharedGenerator = originStructureLevel.IsShared();
							mergedStructure.randomState = Random.state;
							// Add the structure to the merges.
							mergedStructures.Add (mergedStructure);
						}
					}
					candI++;
				}
			}
			// Order merged structures by branch position.
			mergedStructures.Sort ((s1,s2) => s2.branch.position.CompareTo(s1.branch.position));
			/*
			int i;
			for (i = 0;i < candidateBranches.Count; i++) {
				if (i < structures.Count && structures[i] != null && structures[i].isTuned) {
					if (parentStructure != null) {
						structures[i].parentStructureId = parentStructure.id;
						structures[i].parentStructure = parentStructure;
					}
					if (!useParentStructureRandomState) {
						structures[i].randomState = Random.state;
					}
					mergedStructures.Add (structures[i]);
				} else {
					Structure mergedStructure = new Structure ();
					mergedStructure.id = GetNextStructureId ();
					mergedStructure.branch = candidateBranches [i];
					mergedStructure.branch.id = mergedStructure.id;
					mergedStructure.branch.isRoot = originStructureLevel.isRoot;
					mergedStructure.generatorId = originStructureLevel.id;
					mergedStructure.mainGeneratorId = originStructureLevel.GetMainId ();
					if (mergedStructure.generatorId == 0) {
						SetRootPositionAndDirection (mergedStructure, structureLevel);
					}
					if (parentStructure == null) {
						mergedStructure.parentStructureId = -1;
						mergedStructure.parentStructure = null;
					} else {
						mergedStructure.parentStructureId = parentStructure.id;
						mergedStructure.parentStructure = parentStructure;
					}
					mergedStructure.isSharedGenerator = originStructureLevel.IsShared();
					mergedStructure.randomState = Random.state;
					mergedStructures.Add (mergedStructure);
				}
			}

			// TODO: add remaining structures
			for (;i < structures.Count; i++) {
				if (structures[i].isTuned && BelongsToStructureLevel (structures[i].generatorId, structureLevel)) {
					if (!useParentStructureRandomState) {
						structures[i].randomState = Random.state;
					}
					mergedStructures.Add (structures[i]);
				}
			}
			*/
			/*
			List<Structure> mergedStructures = new List<Structure> ();
			int i;
			for (i = 0;i < candidateBranches.Count; i++) {
				if (i < structures.Count && structures[i] != null && structures[i].isTuned) {
					if (parentStructure != null) {
						structures[i].parentStructureId = parentStructure.id;
						structures[i].parentStructure = parentStructure;
					}
					if (!useParentStructureRandomState) {
						structures[i].randomState = Random.state;
					}
					mergedStructures.Add (structures[i]);
				} else {
					Structure mergedStructure = new Structure ();
					mergedStructure.id = GetNextStructureId ();
					mergedStructure.branch = candidateBranches [i];
					mergedStructure.branch.id = mergedStructure.id;
					mergedStructure.branch.isRoot = originStructureLevel.isRoot;
					mergedStructure.generatorId = originStructureLevel.id;
					mergedStructure.mainGeneratorId = originStructureLevel.GetMainId ();
					if (mergedStructure.generatorId == 0) {
						SetRootPositionAndDirection (mergedStructure, structureLevel);
					}
					if (parentStructure == null) {
						mergedStructure.parentStructureId = -1;
						mergedStructure.parentStructure = null;
					} else {
						mergedStructure.parentStructureId = parentStructure.id;
						mergedStructure.parentStructure = parentStructure;
					}
					mergedStructure.isSharedGenerator = originStructureLevel.IsShared();
					mergedStructure.randomState = Random.state;
					mergedStructures.Add (mergedStructure);
				}
			}

			// TODO: add remaining structures
			for (;i < structures.Count; i++) {
				if (structures[i].isTuned && BelongsToStructureLevel (structures[i].generatorId, structureLevel)) {
					if (!useParentStructureRandomState) {
						structures[i].randomState = Random.state;
					}
					mergedStructures.Add (structures[i]);
				}
			}
			*/
			
			// Call recursively
			for (int i = 0; i < mergedStructures.Count; i++) {
				if (structureLevel.structureLevels != null) {
					for (int j = 0; j < structureLevel.structureLevels.Count; j++) {
						GenerateStructuresRecursive (mergedStructures[i].childrenStructures, structureLevel.structureLevels[j], mergedStructures[i]);
					}
				}
			}

			if (parentStructure != null) {
				// Filter existing structures belonging to this StructureLevel from the parent structure
				List<StructureGenerator.Structure> filteredOutStructures = new List<Structure> ();
				int mainStructureLevelId = originStructureLevel.GetMainId ();
				for (int j = 0; j < parentStructure.childrenStructures.Count; j++) {
					if (parentStructure.childrenStructures[j].mainGeneratorId != mainStructureLevelId) {
						filteredOutStructures.Add (parentStructure.childrenStructures[j]);
					}
				}
				// Add the newly generated structures.
				for (int j = 0; j < mergedStructures.Count; j++) {
					if (!filteredOutStructures.Contains (mergedStructures[j])) {
						filteredOutStructures.Add (mergedStructures[j]);
					}
				}
				parentStructure.childrenStructures = filteredOutStructures;
			}
		
			return mergedStructures;
		}
		/// <summary>
		/// Generate the branches and sprouts based on a given structure.
		/// </summary>
		/// <param name="tree">Tree class.</param>
		/// <param name="structures">Structures collection.</param>
		public void BuildTree (BroccoTree tree, List<Structure> structures) {
			// VALIDATION
			if (tree == null) return;
			tree.Clear ();
			for (int i = 0; i < structures.Count; i++) {
				tree.AddBranch (structures[i].branch, structures[i].branch.positionFromRoot);
				GenerateBranchesAndSproutsRecursive (structures[i]);
			}
		}
		/// <summary>
		/// Generate the branches and sprouts recursively based on a given structure.
		/// </summary>
		/// <param name="parentStructure">Structure used to build branches and sprouts.</param>
		private void GenerateBranchesAndSproutsRecursive (Structure parentStructure) {
			parentStructure.branch.ClearSprouts ();
			parentStructure.branch.branches.Clear ();
			// Generate Branches
			for (int i = 0; i < parentStructure.childrenStructures.Count; i++) {
				// Normalize girth scale.
				if (parentStructure.childrenStructures[i].branch.girthScale > parentStructure.branch.girthScale) {
					parentStructure.childrenStructures[i].branch.girthScale = parentStructure.branch.girthScale;
				}
				parentStructure.branch.AddBranch (parentStructure.childrenStructures[i].branch);
				GenerateBranchesAndSproutsRecursive (parentStructure.childrenStructures[i]);
			}
			// Generate Sprouts
			for (int i = 0; i < parentStructure.childrenTerminalStructures.Count; i++) {
				if (idToStructureLevel.ContainsKey (parentStructure.childrenTerminalStructures[i].generatorId)) {
					StructureLevel level = idToStructureLevel[parentStructure.childrenTerminalStructures[i].generatorId];
					List<BroccoTree.Sprout> levelSprouts;
					bool occurred = false;
					//Random.state = parentStructure.childrenTerminalStructures[i].randomState;
					levelSprouts = GenerateSproutLevel (parentStructure.branch, level, out occurred);
					level.isVisited = occurred;
					// Add sprouts to the tree.
					for (int j = 0; j < levelSprouts.Count; j++) {
						if (level.sproutGroupId > 0) {
							levelSprouts [j].groupId = level.sproutGroupId;
						}
						parentStructure.branch.AddSprout (levelSprouts [j]);
					}
					levelSprouts.Clear ();
				}
			}
		}
		/// <summary>
		/// Get the structure level that generate a given structure using a preferred index in a shared structure level context.
		/// </summary>
		/// <param name="structureLevel">Structure level.</param>
		/// <param name="preferredIndex">Preferred index.</param>
		/// <returns>StructureLevel selected.</returns>
		private StructureLevel GetStructureLevel (StructureLevel structureLevel, int preferredIndex = -1) {
			// If it is not a shared level, then return it.
			if (!structureLevel.IsShared()) {
				return structureLevel;
			} else { // If it is shared and the preferred index = 1, then return it.
				if (preferredIndex == 0) {
					return structureLevel;
				}
			}

			// If theres a preferred index, iterate throug the structure levels.
			int maxLoop = 40;
			StructureLevel mainStructureLevel = idToStructureLevel[structureLevel.GetSharedGroupIdOrMainId()];
			if (preferredIndex > -1) {
				int index = 0;
				structureLevel = mainStructureLevel;
				while (structureLevel.sharingNextId != 0 && maxLoop > 0) {
					if (index == preferredIndex) {
						return structureLevel;
					}
					structureLevel = idToStructureLevel [structureLevel.sharingNextId];
					index++;
					maxLoop--;
				}
			}
			// Select a structure level based on the probability
			float probability = Random.Range (0f, 1f);
			// If the probability falls withing the main structure level, return main structure level.
			if (probability <= mainStructureLevel.sharedProbability) {
				return mainStructureLevel;
			}
			// Continue with accumulated probability.
			float accumProb = mainStructureLevel.sharedProbability;
			// Loop though until a structure level falling within the probability is found.
			structureLevel = mainStructureLevel;
			maxLoop = 40;
			do {
				structureLevel = idToStructureLevel [structureLevel.sharingNextId];
				accumProb += structureLevel.sharedProbability;
				if (probability <= accumProb) {
					return structureLevel;
				}
				maxLoop--;
			} while (structureLevel.sharingNextId != 0 && maxLoop > 0);
			if (maxLoop <= 0) {
				Debug.LogWarning ("Probable endless loop found on shared structure levels.");
			}
			return structureLevel;
		}
		/// <summary>
		/// Checks if a given structure generator id matches that of a single structure level or its peers when in a shared group.
		/// </summary>
		/// <param name="generatorId">Id of the structure level.</param>
		/// <param name="structureLevel">Structure level.</param>
		/// <returns>True if the id matches the structure level id or one of its peer whan in a shared group.</returns>
		private bool BelongsToStructureLevel (int generatorId, StructureLevel structureLevel) {
			return structureLevel.GetMainId () == generatorId;
		}
		/// <summary>
		/// Generate branch candidates to merge with the structures.
		/// </summary>
		/// <param name="structureLevel">StructureLevel containing the rules to generate the branches.</param>
		/// <param name="parentBranch">Parent branch.</param>
		/// <param name="occurred"></param>
		/// <param name="preferredFrequency"></param>
		/// <returns></returns>
		private List<BroccoTree.Branch> GenerateBranchCandidates (StructureLevel structureLevel, BroccoTree.Branch parentBranch, out bool occurred, int preferredFrequency = -1) {
			List<BroccoTree.Branch> branches = new List<BroccoTree.Branch> ();
			occurred = false;
			if (!structureLevel.isSprout) {
				float probability = Random.Range (0f, 1f);
				if (structureLevel.enabled && structureLevel.probability >= probability) {
					structureLevel.isVisitedAndActive = true;
					occurred = true;
					if (parentBranch != null) {
						parentBranch.Update ();
						parentBranch.RecalculateNormals ();
					}
					List<BroccoTree.Sprout> spawnedSprouts = new List<BroccoTree.Sprout> ();
					if (preferredFrequency < 0) {
						preferredFrequency = Random.Range (structureLevel.minFrequency, structureLevel.maxFrequency + 1);
					}
					if (preferredFrequency > 0) {
						int childrenPerNode = GetChildrenPerNode (structureLevel);
						float twirlOffset;
						if (structureLevel.randomTwirlOffsetEnabled) {
							twirlOffset = Random.Range (-1f, 1f);
						} else {
							twirlOffset = structureLevel.twirlOffset;
						}

						// Mask range is override by branch break position.
						float minMaskRange = (structureLevel.actionRangeEnabled ? structureLevel.minMaskRange : 0f);
						float maxMaskRange = (structureLevel.actionRangeEnabled ? structureLevel.maxMaskRange : 1f);
						if (parentBranch != null && parentBranch.isBroken) {
							if (maxMaskRange > parentBranch.breakPosition) maxMaskRange = parentBranch.breakPosition;
							if (minMaskRange > maxMaskRange) minMaskRange = maxMaskRange;
						}

						// Get sprouts, branches will be created out of these.
						spawnedSprouts = SproutGenerator.GetSprouts (preferredFrequency, childrenPerNode,
							structureLevel.minTwirl, structureLevel.maxTwirl, twirlOffset, 
							structureLevel.distributionSpacingVariance, structureLevel.distributionAngleVariance, structureLevel.distributionCurve,
							structureLevel.minParallelAlignAtBase, structureLevel.maxParallelAlignAtBase, structureLevel.minParallelAlignAtTop, structureLevel.maxParallelAlignAtTop, structureLevel.parallelAlignCurve,
							structureLevel.minGravityAlignAtBase, structureLevel.maxGravityAlignAtBase, structureLevel.minGravityAlignAtTop, structureLevel.maxGravityAlignAtTop, structureLevel.gravityAlignCurve,
							structureLevel.minHorizontalAlignAtBase, structureLevel.maxHorizontalAlignAtBase, structureLevel.minHorizontalAlignAtTop, structureLevel.maxHorizontalAlignAtTop, structureLevel.horizontalAlignCurve,
							structureLevel.flipSproutAlign, structureLevel.flipSproutDirection,
							(structureLevel.actionRangeEnabled ? structureLevel.minRange : 0f), 
							(structureLevel.actionRangeEnabled ? structureLevel.maxRange : 1f),
							minMaskRange, 
							maxMaskRange,
							structureLevel.fromBranchCenter, (structureLevel.distributionOrigin == StructureLevel.DistributionOrigin.FromTip), structureLevel.id);
						
						for (int i = 0; i < spawnedSprouts.Count; i++) {
							spawnedSprouts [i].CalculateVectors (parentBranch, true);
							BroccoTree.Branch childBranch = new BroccoTree.Branch ();
							childBranch.id = branchIdCount;
							branchIdCount++;
							childBranch.helperStructureLevelId = structureLevel.id;
							// Branch brakes or not.
							if (structureLevel.applyBranchBreak) {
								float breakBranchProbability = structureLevel.breakBranchProbability.Evaluate (spawnedSprouts[i].position);
								if (Random.Range (0f, 1f) <= breakBranchProbability) {
									childBranch.isBroken = true;
									childBranch.breakPosition = Random.Range (structureLevel.minBreakRange, structureLevel.maxBreakRange);
								}
							}
							childBranch.maxLength = GetLength (spawnedSprouts [i].position, structureLevel);
							childBranch.girthScale = Random.Range (structureLevel.minGirthScale, structureLevel.maxGirthScale);
							if (structureLevel.id == 0) {
								childBranch.direction = Base.GlobalSettings.againstGravityDirection;
							} else {
								childBranch.direction = spawnedSprouts [i].sproutDirection;
								//TODO RE: check for direction, normal and forward vectors.
								childBranch.rollAngle = spawnedSprouts [i].rollAngle;
								childBranch.forward = spawnedSprouts [i].forward;
							}
							childBranch.position = spawnedSprouts [i].position;
							childBranch.Update ();
							childBranch.UpdatePosition (parentBranch);
							branches.Add (childBranch);
						}
					}
					spawnedSprouts.Clear ();
				} else {
					structureLevel.isVisitedAndActive = false;
					occurred = false;
				}
			}
			return branches;
		}
		/// <summary>
		/// Gets the length assigned to a branch or sprout using the level structure specification.
		/// </summary>
		/// <returns>The length.</returns>
		/// <param name="positionAtBranch">Position at parent branch.</param>
		/// <param name="level">Structure level.</param>
		protected float GetLength (float positionAtBranch, StructureLevel level) {
			/*
			if (level.id == 0) {
				return Random.Range (level.lengthAtBase, level.lengthAtTop);
			} else {
				return Mathf.Lerp (level.lengthAtBase, level.lengthAtTop, 
					Mathf.Clamp(level.lengthCurve.Evaluate(positionAtBranch), 0f, 1f));
			}
			*/
			if (level.id == 0) {
				return Random.Range (level.minLengthAtBase, level.maxLengthAtBase);
			} else {
				return Mathf.Lerp (Random.Range (level.minLengthAtBase, level.maxLengthAtBase), Random.Range (level.minLengthAtTop, level.maxLengthAtTop), 
					Mathf.Clamp(level.lengthCurve.Evaluate(positionAtBranch), 0f, 1f));
			}
		}
		/// <summary>
		/// Get the number of children elements per node.
		/// </summary>
		/// <returns>Number of children per node.</returns>
		/// <param name="level">Structure level.</param>
		protected int GetChildrenPerNode (StructureLevel level) {
			int childrenPerNode = 1;
			if (level.distribution == StructureLevel.Distribution.Opposite) {
				childrenPerNode = 2;
			} else if (level.distribution == StructureLevel.Distribution.Whorled) {
				childrenPerNode = level.childrenPerNode;
			}
			return childrenPerNode;
		}
		/// <summary>
		/// Get a random position inside of a circle area.
		/// </summary>
		/// <returns>The random position.</returns>
		/// <param name="radius">Radius of the circle.</param>
		protected Vector3 GetRandCircle (float radius) {
			float t = 2 * Mathf.PI * Random.Range (0f, 1f);
			float u = Random.Range (0f, 1f) + Random.Range (0f, 1f);
			float r = 0f;
			if (u > 1)
				r = 2f - u;
			else
				r = u;
			Vector3 point = new Vector3 (radius * r * Mathf.Cos (t), 0f, radius * r * Mathf.Sin (t));
			return point;
		}
		#endregion

		#region Branch methods
		/// <summary>
		/// Position and direction for root structures.
		/// </summary>
		/// <param name="structure">Structures to set position and direction into.</param>
		/// <param name="rootStructureLevel">StructureLevel for instructions.</param>
		protected void SetRootPositionAndDirection (Structure structure, StructureLevel rootStructureLevel) {
			if (positions.Count > 0) {
				int index = Random.Range (0, positions.Count);
					structure.positionOffset = positions [index].rootPosition / globalScale;
					structure.branch.positionFromRoot = structure.positionOffset;
				if (positions [index].overrideRootDirection) {
					structure.branch.ResetDirection (positions [index].rootDirection);
				}
			} else {
				structure.positionOffset = GetRandCircle (rootStructureLevel.radius) / globalScale;
				structure.branch.positionFromRoot = structure.positionOffset;
			}
		}
		#endregion

		#region Sprout methods
		/// <summary>
		/// Generate sprouts for a branch.
		/// </summary>
		/// <returns>Sprouts.</returns>
		/// <param name="branch">Parent branch.</param>
		/// <param name="level">Structure level.</param>
		/// <param name="occurred">Occurred.</param>
		protected List<BroccoTree.Sprout> GenerateSproutLevel (BroccoTree.Branch branch, 
			StructureLevel level, 
			out bool occurred) 
		{
			List<BroccoTree.Sprout> spawnedSprouts = null;
			float probability = Random.Range (0f, 1f);
			if (level.enabled && level.probability >= probability) {
				level.isVisitedAndActive = true;
				occurred = true;
				branch.Update ();
				branch.RecalculateNormals ();
				int frequency = Random.Range (level.minFrequency, level.maxFrequency + 1);
				if (frequency > 0) {
					int childrenPerNode = GetChildrenPerNode (level);

					float twirlOffset;
					if (level.randomTwirlOffsetEnabled) {
						twirlOffset = Random.Range (-1f, 1f);
					} else {
						twirlOffset = level.twirlOffset;
					}

					spawnedSprouts = SproutGenerator.GetSprouts (frequency, childrenPerNode,
						level.minTwirl, level.maxTwirl, twirlOffset, 
						level.distributionSpacingVariance, level.distributionAngleVariance, level.distributionCurve,
						level.minParallelAlignAtBase, level.maxParallelAlignAtBase, level.minParallelAlignAtTop, level.maxParallelAlignAtTop, level.parallelAlignCurve,
						level.minGravityAlignAtBase, level.maxGravityAlignAtBase, level.minGravityAlignAtTop, level.maxGravityAlignAtTop, level.gravityAlignCurve,
						level.minHorizontalAlignAtBase, level.maxHorizontalAlignAtBase, level.minHorizontalAlignAtTop, level.maxHorizontalAlignAtTop, level.horizontalAlignCurve,
						level.flipSproutAlign, level.flipSproutDirection,
						(level.actionRangeEnabled ? level.minRange : 0f), 
						(level.actionRangeEnabled ? level.maxRange : 1f), 
						(level.actionRangeEnabled ? level.minMaskRange : 0f), 
						(level.actionRangeEnabled ? level.maxMaskRange : 1f), 
						level.fromBranchCenter, true, level.id);
				}
			} else {
				level.isVisitedAndActive = false;
				occurred = false;
			}
			if (spawnedSprouts == null)
				spawnedSprouts = new List<BroccoTree.Sprout> ();
			
			return spawnedSprouts;
		}
		#endregion
	}
}