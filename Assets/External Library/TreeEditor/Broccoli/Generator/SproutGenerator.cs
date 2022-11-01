using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Model;
using Broccoli.Utils;
using Broccoli.Pipe;

namespace Broccoli.Generator
{
	/// <summary>
	/// Sprout generator.
	/// </summary>
	public class SproutGenerator {
		#region SeedOrigin class
		/// <summary>
		/// Information about the origin of a seed.
		/// </summary>
		class SeedOrigin {
			/// <summary>
			/// First branch for this seed.
			/// </summary>
			public BroccoTree.Branch firstBranch;
			/// <summary>
			/// Length of the seed.
			/// </summary>
			public float length = 0f;
			/// <summary>
			/// True if the seed spawns from the origin of trunk of the tree.
			/// </summary>
			public bool isTrunk = false;
		}
		#endregion

		#region Vars
		/// <summary>
		/// The random seed.
		/// </summary>
		public int randomSeed = 0;
		/// <summary>
		/// True if randomizing values.
		/// </summary>
		public bool enableRandom = true;
		/// <summary>
		/// Minimum number of sprouts to generate.
		/// </summary>
		int _minFrequency = 1;
		/// <summary>
		/// Gets or sets the minimum number of sprouts to generate.
		/// </summary>
		/// <value>The minimum frequency.</value>
		public int minFrequency {
			get { return _minFrequency; }
			set { if (value >= 0) {
					_minFrequency = value;
					if (_minFrequency > _maxFrequency) {
						_maxFrequency = _minFrequency;
					}
				}
			}
		}
		/// <summary>
		/// The maxium number of sprouts to generate.
		/// </summary>
		int _maxFrequency = 1;
		/// <summary>
		/// Gets or sets the maximum number of sprouts to generate.
		/// </summary>
		/// <value>The max frequency.</value>
		public int maxFrequency {
			get { return _maxFrequency; }
			set { if (value >= 0) {
					_maxFrequency = value;
					if (_minFrequency > _maxFrequency) {
						_maxFrequency = _minFrequency;
					}
				}
			}
		}
		/// <summary>
		/// Distriution modes for the sprouts along the branches.
		/// </summary>
		public enum Distribution
		{
			Alternative,
			Opposite,
			Whorled
		}
		/// <summary>
		/// The distribution mode.
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
		/// The distribution curve.
		/// </summary>
		public AnimationCurve distributionCurve = 
			AnimationCurve.Linear (0f, 0f, 1f, 1f);
		/// <summary>
		/// Distribution origin modes.
		/// </summary>
		public enum DistributionOrigin
		{
			FromTipBranches,
			FromTrunk
		}
		/// <summary>
		/// The whorled step.
		/// </summary>
		public int whorledStep = 3;
		/// <summary>
		/// The minimum twirl or angle of sprouts around the branch.
		/// </summary>
		public float minTwirl = 0f;
		/// <summary>
		/// The maximum twirl or angle of sprouts around the branch.
		/// </summary>
		public float maxTwirl = 0f;
		/// <summary>
		/// Minimum grade of alignment wit the parent branch at top of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float minParallelAlignAtTop = 0f;
		/// <summary>
		/// Maximum grade of alignment wit the parent branch at top of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float maxParallelAlignAtTop = 0f;
		/// <summary>
		/// Minimum grade of alignment wit the parent branch at base of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float minParallelAlignAtBase = 0f;
		/// <summary>
		/// Maximum grade of alignment wit the parent branch at base of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float maxParallelAlignAtBase = 0f;
		/// <summary>
		/// The parallel align curve.
		/// </summary>
		public AnimationCurve parallelAlignCurve = AnimationCurve.Linear(0f,0f,1f,1f);
		/// <summary>
		/// Minimum grade of alignment against the gravity at top of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float minGravityAlignAtTop = 0.25f;
		/// <summary>
		/// Maximum grade of alignment against the gravity at top of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float maxGravityAlignAtTop = 0.25f;
		/// <summary>
		/// Minimum grade of alignment against the gravity at base of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float minGravityAlignAtBase = 0.25f;
		/// <summary>
		/// Maximum grade of alignment against the gravity at base of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float maxGravityAlignAtBase = 0.25f;
		/// <summary>
		/// The gravity align curve.
		/// </summary>
		public AnimationCurve gravityAlignCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
		/// <summary>
		/// Minimum grade of alignment to the horizontal plane at top of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float minHorizontalAlignAtTop = 0f;
		/// <summary>
		/// Maximum grade of alignment to the horizontal plane at top of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float maxHorizontalAlignAtTop = 0f;
		/// <summary>
		/// Minimum grade of alignment against to the horizontal plane at base of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float minHorizontalAlignAtBase = 0f;
		/// <summary>
		/// Maximum grade of alignment against to the horizontal plane at base of the parent branch.
		/// </summary>
		[Range(-1f, 1f)]
		public float maxHorizontalAlignAtBase = 0f;
		/// <summary>
		/// The horizontal align curve.
		/// </summary>
		public AnimationCurve horizontalAlignCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
		/// <summary>
		/// True when the sprout has its origin at the center of the branch; when false the
		/// branch has its origin at the surface of it (girth dependant).
		/// </summary>
		public bool fromBranchCenter = false;
		/// <summary>
		/// The distribution origin mode
		/// </summary>
		public DistributionOrigin distributionOrigin = 
			DistributionOrigin.FromTipBranches;
		/// <summary>
		/// True if the spawned sprouts should go beyond their origin branch.
		/// </summary>
		public bool spreadEnabled = false;
		/// <summary>
		/// How much of the hierarchy the spawned sprouts should cover
		/// when spreadEnabled is true.
		/// </summary>
		[Range (0,1)]
		public float spreadRange = 0.2f;
		/// <summary>
		/// The sprout seeds used to spawn sprouts on the branches.
		/// </summary>
		public List<SproutSeed> sproutSeeds = new List<SproutSeed>();
		/// <summary>
		/// The helper identifier.
		/// </summary>
		public int helperId = -1;
		/// <summary>
		/// List of seed origins.
		/// </summary>
        [SerializeField]
		List<SeedOrigin> seedOrigins = new List<SeedOrigin> ();
		/// <summary>
		/// The length of the max seed origin.
		/// Length taken by the maximum lineage of parent and followup branches.
		/// </summary>
		float maxSeedOriginLength = 0f;
		/// <summary>
		/// Dictionary to save the relationshio between branches and sprouts created.
		/// </summary>
		/// <returns></returns>
		private Dictionary<int, List<BroccoTree.Sprout>> _branchToSprouts = 
			new Dictionary<int, List<BroccoTree.Sprout>> ();
		#endregion

		#region Singleton
		/// <summary>
		/// This class singleton.
		/// </summary>
		static SproutGenerator _treeSproutGenerator = null;
		/// <summary>
		/// Gets the instance singleton.
		/// </summary>
		/// <returns>The instance.</returns>
		public static SproutGenerator GetInstance() {
			if (_treeSproutGenerator == null) {
				_treeSproutGenerator = new SproutGenerator ();
			}
			return _treeSproutGenerator;
		}
		#endregion

		#region Seed Origins
		/// <summary>
		/// Sets the seed origins for all the points where the sprouts would be
		/// generated from.
		/// </summary>
		/// <param name="tree">Tree.</param>
		private void SetSeedOrigins (BroccoTree tree) {
			seedOrigins.Clear ();
			maxSeedOriginLength = 0f;
			if (distributionOrigin == DistributionOrigin.FromTipBranches) {
				List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
				for (int i = 0; i < branches.Count; i++) {
					if (branches[i].followUp == null && !branches[i].isRoot) {
						SeedOrigin seedOrigin = new SeedOrigin ();
						seedOrigin.firstBranch = branches[i];
						seedOrigin.length = GetLengthFromSeedOrigin (branches[i]);
						seedOrigins.Add (seedOrigin);
						if (seedOrigin.length > maxSeedOriginLength) {
							maxSeedOriginLength = seedOrigin.length;
						}
					}
				}
			} else {
				for (int i = 0; i < tree.branches.Count; i++) {
					SeedOrigin seedOrigin = new SeedOrigin ();
					seedOrigin.firstBranch = tree.branches[i];
					seedOrigin.length = GetLengthFromSeedOrigin (tree.branches[i], true);
					seedOrigin.isTrunk = true;
					seedOrigins.Add (seedOrigin);
					if (seedOrigin.length > maxSeedOriginLength) {
						maxSeedOriginLength = seedOrigin.length;
					}
				}
			}
		}
		/// <summary>
		/// Gets the length from a seed origin branch to its base branch.
		/// </summary>
		/// <returns>The length from seed origin.</returns>
		/// <param name="branch">Branch.</param>
		/// <param name="fromTrunk">True if the origin branch is trunk.</param>
		private float GetLengthFromSeedOrigin (BroccoTree.Branch branch, bool fromTrunk = false) {
			float length = branch.length;
			int maxLevels = 50;
			if (fromTrunk) {
				while (branch.followUp != null && maxLevels > 0) {
					branch = branch.followUp;
					length += branch.length;
					maxLevels--;
				}
			} else {
				while (branch.parent != null && branch.parent.followUp == branch && maxLevels > 0) {
					branch = branch.parent;
					length += branch.length;
					maxLevels--;
				}
			}
			return length;
		}
		#endregion

		#region Spawn Sprouts
		/// <summary>
		/// Spawns the sprouts.
		/// </summary>
		/// <param name="tree">Tree.</param>
		public void SpawnSprouts (BroccoTree tree) {
			SetSeedOrigins (tree);
			if (distributionOrigin == DistributionOrigin.FromTipBranches) {
				SpawnFromTipBranches (tree);
			} else {
				SpawnFromTrunk (tree);
			}
		}
		/// <summary>
		/// Spawns sprouts from tip branches.
		/// </summary>
		/// <param name="tree">Tree.</param>
		private void SpawnFromTipBranches (BroccoTree tree) {
			SeedOrigin seedOrigin;
			for (int i = 0; i < seedOrigins.Count; i++) {
				seedOrigin = seedOrigins [i];
				int frequency = Random.Range (minFrequency, maxFrequency + 1);
				int childrenPerNode = GetChildrenPerNode (distribution, whorledStep);
				List<BroccoTree.Sprout> sprouts = GetSprouts (frequency, childrenPerNode, minTwirl, maxTwirl, Random.Range (0f, 1f), 
					distributionSpacingVariance, distributionAngleVariance, distributionCurve,
					minParallelAlignAtBase, maxParallelAlignAtBase, minParallelAlignAtTop, maxParallelAlignAtTop, parallelAlignCurve,
					minGravityAlignAtBase, maxGravityAlignAtBase, minGravityAlignAtTop, maxGravityAlignAtTop, gravityAlignCurve,
					minHorizontalAlignAtBase, maxHorizontalAlignAtBase, minHorizontalAlignAtTop, maxHorizontalAlignAtTop, horizontalAlignCurve,
					0, Vector3.up,
					0f, 1f, 0f, 1f, fromBranchCenter, true, helperId);
				if (spreadEnabled) {
					BroccoTree.Branch currentBranch = null;
					float currentBranchConsumedLength = 0f;
					float newPosition = 1;
					float sproutLengthOffset = 0f;
					float maxLengthConsumed = maxSeedOriginLength * spreadRange;
					for (int j = 0; j < sprouts.Count; j++) {
						// Assign a random sprout group among the available.
						sprouts[j].groupId = sproutSeeds [Random.Range (0, sproutSeeds.Count)].groupId;

						if (currentBranch == null) {
							currentBranch = seedOrigin.firstBranch;
							currentBranchConsumedLength = currentBranch.length;
						} else {
							sproutLengthOffset = maxLengthConsumed - sprouts[j].position * maxSeedOriginLength * spreadRange;
							maxLengthConsumed -= sproutLengthOffset;
							currentBranchConsumedLength -= sproutLengthOffset;
							if (currentBranchConsumedLength < 0) {
								while (currentBranch.parent != null && currentBranchConsumedLength < 0) {
									currentBranch = currentBranch.parent;
									currentBranchConsumedLength = currentBranch.length + currentBranchConsumedLength;
								}
							}
							newPosition = currentBranchConsumedLength / currentBranch.length;
						}

						// The seedOrigin length has been consumed or no more parent
						if (maxSeedOriginLength * spreadRange - maxLengthConsumed > seedOrigin.length || 
							currentBranchConsumedLength < 0) {
							break;
						}

						sprouts[j].position = newPosition;
						sprouts[j].hierarchyPosition = maxLengthConsumed / (maxSeedOriginLength * spreadRange);
						sprouts[j].preferHierarchyPosition = true;
						currentBranch.AddSprout (sprouts[j]);
						RegisterSprout (currentBranch, sprouts[j]);
					}
				} else {
					for (int j = 0; j < sprouts.Count; j++) {
						sprouts[j].groupId = sproutSeeds [Random.Range (0, sproutSeeds.Count)].groupId;
						seedOrigin.firstBranch.AddSprout (sprouts[j]);
						RegisterSprout (seedOrigin.firstBranch, sprouts[j]);
					}
				}
				sprouts.Clear ();
			}
		}
		/// <summary>
		/// Spawns sprouts from trunk.
		/// </summary>
		/// <param name="tree">Tree.</param>
		private void SpawnFromTrunk (BroccoTree tree) {
			for (int i = 0; i < seedOrigins.Count; i++) {
				int frequency = Random.Range (minFrequency, maxFrequency + 1);
				int childrenPerNode = GetChildrenPerNode (distribution, whorledStep);
				List<BroccoTree.Sprout> sprouts = GetSprouts (frequency, childrenPerNode, minTwirl, maxTwirl, 0,
					distributionSpacingVariance, distributionAngleVariance, distributionCurve,
					minParallelAlignAtBase, maxParallelAlignAtBase, minParallelAlignAtTop, maxParallelAlignAtTop, parallelAlignCurve,
					minGravityAlignAtBase, maxGravityAlignAtBase, minGravityAlignAtTop, maxGravityAlignAtTop, gravityAlignCurve,
					minHorizontalAlignAtBase, maxHorizontalAlignAtBase, minHorizontalAlignAtTop, maxHorizontalAlignAtTop, horizontalAlignCurve,
					0f, Vector3.up,
					0f, 1f, 0f, 1f, fromBranchCenter, false, helperId);
				if (spreadEnabled) {

					BroccoTree.Branch currentBranch = null;
					float currentBranchConsumedLength = 0f;
					float newPosition = 0;
					float sproutLengthOffset = 0f;
					float maxLengthConsumed = maxSeedOriginLength * spreadRange;
					for (int j = 0; j < sprouts.Count; j++) {
						sprouts[j].groupId = sproutSeeds [Random.Range (0, sproutSeeds.Count)].groupId;
						if (currentBranch == null) {
							currentBranch = seedOrigins[i].firstBranch;
							currentBranchConsumedLength = currentBranch.length;
						} else {
							sproutLengthOffset = maxLengthConsumed - sprouts[j].position * maxSeedOriginLength * spreadRange;
							maxLengthConsumed -= sproutLengthOffset;
							currentBranchConsumedLength -= sproutLengthOffset;
							if (currentBranchConsumedLength < 0) {
								while (currentBranch.followUp != null && currentBranchConsumedLength < 0) {
									currentBranch = currentBranch.followUp;
									currentBranchConsumedLength = currentBranch.length + currentBranchConsumedLength;
								}
							}
							newPosition = 1 - (currentBranchConsumedLength / currentBranch.length);
						}

						// The seedOrigin length has been consumed or no more parent
						if (maxSeedOriginLength * spreadRange - maxLengthConsumed > seedOrigins[i].length || 
							currentBranchConsumedLength < 0) {
							break;
						}

						sprouts[j].position = newPosition;
						sprouts[j].hierarchyPosition = 1f - (maxLengthConsumed / (maxSeedOriginLength * spreadRange));
						sprouts[j].preferHierarchyPosition = true;
						currentBranch.AddSprout (sprouts[j]);
						RegisterSprout (currentBranch, sprouts[j]);
					}
				} else {
					for (int j = 0; j < sprouts.Count; j++) {
						sprouts[j].groupId = sproutSeeds [Random.Range(0, sproutSeeds.Count)].groupId;
						seedOrigins[i].firstBranch.AddSprout (sprouts[j]);
						RegisterSprout (seedOrigins[i].firstBranch, sprouts[j]);
					}
				}
			}
		}
		/// <summary>
		/// Spawns the sprouts randomly.
		/// </summary>
		/// <returns>The sprouts random.</returns>
		/// <param name="tree">Tree.</param>
		List<BroccoTree.Sprout> SpawnSproutsRandom (BroccoTree tree) {
			List<BroccoTree.Sprout> spawnedSprouts = new List<BroccoTree.Sprout> ();
			if (_maxFrequency > 0) {
				List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
				for (int i = 0; i < branches.Count; i++) {
					if (branches[i].followUp == null) {
						for (int j = 0; j < _maxFrequency; j++) {
							BroccoTree.Sprout sprout = new BroccoTree.Sprout ();
							sprout.position = Random.Range (0f, 1f);
							sprout.rollAngle = Random.Range (0f, Mathf.PI * 2);
							branches[i].AddSprout (sprout);
							RegisterSprout (branches[i], sprout);
							spawnedSprouts.Add (sprout);
						}
					}
				}
			}
			return spawnedSprouts;
		}
		public void AddCachedSprouts (BroccoTree tree) {
			for (int i = 0; i < tree.branches.Count; i++) {
				AddCachedSproutsRecursive (tree.branches [i]);		
			}
		}
		private void AddCachedSproutsRecursive (BroccoTree.Branch branch) {
			if (_branchToSprouts.ContainsKey (branch.id)) {
				List<BroccoTree.Sprout> sprouts = _branchToSprouts [branch.id];
				for (int i = 0; i < sprouts.Count; i++) {
					branch.AddSprout (sprouts [i]);
				}
			}
			for (int i = 0; i < branch.branches.Count; i++) {
				AddCachedSproutsRecursive (branch.branches [i]);
			}
		}
		void RegisterSprout (BroccoTree.Branch branch, BroccoTree.Sprout sprout) {
			if (!_branchToSprouts.ContainsKey (branch.id)) {
				_branchToSprouts.Add (branch.id, new List<BroccoTree.Sprout> ());
			}
			_branchToSprouts [ branch.id].Add (sprout);
		}
		/// <summary>
		/// Clears this instance temporary variables.
		/// </summary>
		public void Clear () {
			_branchToSprouts.Clear ();
		}
		#endregion

		#region Sprout Generation
		/// <summary>
		/// Gets the sprouts along a branch.
		/// </summary>
		/// <returns>The sprouts.</returns>
		/// <param name="frequency">Frequency.</param>
		/// <param name="childrenPerNode">Children per node.</param>
		/// <param name="minTwirl">Minimum twirl value.</param>
		/// <param name="maxTwirl">Maximum twirl value.</param>
		/// <param name="twirlOffset">Twirl offset value.</param>
		/// <param name="distributionSpacingVariance">Variance applied to spacing variation to branches belonging to a distribuition group.</param>
		/// <param name="distributionAngleVariance">Variance applied to angle variation to branches belonging to a distribuition group.</param>
		/// <param name="distributionCurve">Distribution curve.</param>
		/// <param name="minParallelAlignAtBase">Parallel align at base.</param>
		/// <param name="maxParallelAlignAtBase">Parallel align at base.</param>
		/// <param name="minParallelAlignAtTop">Parallel align at top.</param>
		/// <param name="maxParallelAlignAtTop">Parallel align at top.</param>
		/// <param name="parallelAlignCurve">Parallel align curve.</param>
		/// <param name="minGravityAlignAtBase">Minimum gravity align at base.</param>
		/// <param name="maxGravityAlignAtBase">Maximum gravity align at base.</param>
		/// <param name="minGravityAlignAtTop">Minimum gravity align at top.</param>
		/// <param name="maxGravityAlignAtTop">Maximum gravity align at top.</param>
		/// <param name="gravityAlignCurve">Gravity align curve.</param>
		/// <param name="minHorizontalAlignAtBase">Minimum horizontal align at base.</param>
		/// <param name="maxHorizontalAlignAtBase">Maximum horizontal align at base.</param>
		/// <param name="minHorizontalAlignAtTop">Minimum horizontal align at top.</param>
		/// <param name="maxHorizontalAlignAtTop">Maximum horizontal align at top.</param>
		/// <param name="horizontalAlignCurve">Horizontal align curve.</param>
		/// <param name="flipAlign">Flip align value.</param>
		/// <param name="flipAlignDirection">Flip align direction.</param>
		/// <param name="minRange">Minimum action range.</param>
		/// <param name="maxRange">Max action range.</param>
		/// <param name="minMaskRange">Minimum action range.</param>
		/// <param name="maxMaskRange">Max action range.</param>
		/// <param name="fromBranchCenter">If true the sprout origin comes from the center of the branch.</param>
		/// <param name="beginAtTop ">The spawned sprouts begin from the tip of the branch.</param>
		/// <param name="helperId">Helper identifier.</param>
		public static List<BroccoTree.Sprout> GetSprouts (int frequency, 
			int childrenPerNode,
			float minTwirl,
			float maxTwirl,
			float twirlOffset,
			float distributionSpacingVariance,
			float distributionAngleVariance,
			AnimationCurve distributionCurve,
			float minParallelAlignAtBase,
			float maxParallelAlignAtBase,
			float minParallelAlignAtTop,
			float maxParallelAlignAtTop,
			AnimationCurve parallelAlignCurve,
			float minGravityAlignAtBase,
			float maxGravityAlignAtBase,
			float minGravityAlignAtTop,
			float maxGravityAlignAtTop,
			AnimationCurve gravityAlignCurve,
			float minHorizontalAlignAtBase,
			float maxHorizontalAlignAtBase,
			float minHorizontalAlignAtTop,
			float maxHorizontalAlignAtTop,
			AnimationCurve horizontalAlignCurve,
			float flipAlign,
			Vector3 flipAlignDirection,
			float minRange,
			float maxRange,
			float minMaskRange,
			float maxMaskRange,
			bool fromBranchCenter,
			bool beginAtTop,
			int helperId) 
		{
			List<BroccoTree.Sprout> spawnedSprouts = new List<BroccoTree.Sprout> ();
			if (frequency > 0) {
				// Intra node information.
				float intraNodesAngle = Mathf.PI * 2f / (float)childrenPerNode;
				int sproutNodes = Mathf.CeilToInt (frequency / (float)childrenPerNode);
				float positionPerNode = 1f / (float)sproutNodes;

				// Angular difference between neighbour nodes.
				float angleBetweenNodes = Mathf.PI / (float)childrenPerNode;
				float accumAngleBetweenNodes = 0f;

				// Positioning.
				float sproutPosition;
				float halfSproutPosition = positionPerNode / 2f;

				// Twirl.
				float twirlToAdd = Mathf.PI * Random.Range (minTwirl, maxTwirl);
				twirlOffset *= Mathf.PI;
				float halfTwirlStep = intraNodesAngle / 2f;

				// Sprouts counters.
				int addedSprouts = 0;

				// For every sprout on the node.
				bool firstDistributionSpacingVariance = true;

				// For every node.
				for (int i = 1; i <= sproutNodes; i++) {
					sproutPosition = i * positionPerNode;
					sproutPosition = Mathf.Clamp(distributionCurve.Evaluate(sproutPosition), 0f, 1f);
					if (!beginAtTop) {
						sproutPosition = 1 - sproutPosition;
					}
					sproutPosition = Mathf.Lerp (minRange, maxRange, sproutPosition);

					twirlToAdd = Mathf.PI * Random.Range (minTwirl, maxTwirl);

					for (int j = 0; j < childrenPerNode; j++) {
						if (IsSproutInRange (sproutPosition, minMaskRange, maxMaskRange)) {
							BroccoTree.Sprout spawnedSprout = new BroccoTree.Sprout ();
							spawnedSprout.fromBranchCenter = fromBranchCenter;
							spawnedSprout.helperStructureLevelId = helperId;
							// Position.
							if (!firstDistributionSpacingVariance || sproutPosition + 0.00001f < 1f) {
							spawnedSprout.position = Mathf.Clamp (sproutPosition + 
								(Random.Range (-halfSproutPosition, halfSproutPosition) * distributionSpacingVariance), 0f, 1f);
							} else {
								firstDistributionSpacingVariance = false;
							}
							// Twirl.
							spawnedSprout.rollAngle = accumAngleBetweenNodes + (intraNodesAngle * j) + (twirlToAdd * i) + 
								(Random.Range (-halfTwirlStep, halfTwirlStep) * distributionAngleVariance) + twirlOffset;
							
							SetSproutRelativeAngle (spawnedSprout, 
								minParallelAlignAtBase, maxParallelAlignAtBase, minParallelAlignAtTop, maxParallelAlignAtTop, parallelAlignCurve,
								minGravityAlignAtBase, maxGravityAlignAtBase, minGravityAlignAtTop, maxGravityAlignAtTop, gravityAlignCurve,
								minHorizontalAlignAtBase, maxHorizontalAlignAtBase, minHorizontalAlignAtTop, maxHorizontalAlignAtTop, horizontalAlignCurve,
								flipAlign, flipAlignDirection);
							spawnedSprouts.Add (spawnedSprout);
						}
						addedSprouts++;
						if (addedSprouts >= frequency) {
							break;
						}
					}
					accumAngleBetweenNodes += angleBetweenNodes;
				}
			}
			spawnedSprouts.Sort (delegate (BroccoTree.Sprout a, BroccoTree.Sprout b) {
				return b.position.CompareTo (a.position);
			});
			return spawnedSprouts;
		}
		/*
		/// <summary>
		/// Gets the sprouts along a branch.
		/// </summary>
		/// <returns>The sprouts.</returns>
		/// <param name="frequency">Frequency.</param>
		/// <param name="childrenPerNode">Children per node.</param>
		/// <param name="parallelAlignAtBase">Parallel align at base.</param>
		/// <param name="parallelAlignAtTop">Parallel align at top.</param>
		/// <param name="parallelAlignCurve">Parallel align curve.</param>
		/// <param name="gravityAlignAtBase">Gravity align at base.</param>
		/// <param name="gravityAlignAtTop">Gravity align at top.</param>
		/// <param name="gravityAlignCurve">Gravity align curve.</param>
		/// <param name="horizontalAlignAtBase">Horizontal align at base.</param>
		/// <param name="horizontalAlignAtTop">Horizontal align at top.</param>
		/// <param name="horizontalAlignCurve">Horizontal align curve.</param>
		/// <param name="minActionRange">Minimum action range.</param>
		/// <param name="maxActionRange">Max action range.</param>
		/// <param name="fromBranchCenter">If true the sprout origin comes from the center of the branch.</param>
		/// <param name="beginAtTop ">The spawned sprouts begin from the tip of the branch.</param>
		/// <param name="helperId">Helper identifier.</param>
		public static List<BroccoTree.Sprout> GetRandomSprouts (int frequency, 
			int childrenPerNode,
			float parallelAlignAtBase,
			float parallelAlignAtTop,
			AnimationCurve parallelAlignCurve,
			float gravityAlignAtBase,
			float gravityAlignAtTop,
			AnimationCurve gravityAlignCurve,
			float horizontalAlignAtBase,
			float horizontalAlignAtTop,
			AnimationCurve horizontalAlignCurve,
			float minActionRange,
			float maxActionRange,
			bool fromBranchCenter,
			bool beginAtTop,
			int helperId) 
		{
			List<BroccoTree.Sprout> spawnedSprouts = new List<BroccoTree.Sprout> ();
			if (frequency > 0) {
				float angleIntraNodes = Mathf.PI * 2f / (float)childrenPerNode;
				int sproutNodes = Mathf.CeilToInt (frequency / (float)childrenPerNode);
				int addedSprouts = 0;
				float accumAngleBetweenNodes = 0f;
				for (int i = 1; i <= sproutNodes; i++) {
					float sproutPosition = Random.Range (minActionRange, maxActionRange);
					float accumAngleIntraNodes = 0f;
					for (int j = 1; j <= childrenPerNode; j++) {
						if (!beginAtTop)
							sproutPosition = 1 - sproutPosition;
						if (IsSproutInRange (sproutPosition, minActionRange, maxActionRange)) {
							BroccoTree.Sprout spawnedSprout = new BroccoTree.Sprout ();
							spawnedSprout.fromBranchCenter = fromBranchCenter;
							spawnedSprout.helperStructureLevelId = helperId;
							spawnedSprout.position = sproutPosition;
							spawnedSprout.aroundBranchAngle = accumAngleBetweenNodes + accumAngleIntraNodes;
							SetSproutRelativeAngle (spawnedSprout, 
								parallelAlignAtBase, parallelAlignAtTop, parallelAlignCurve,
								gravityAlignAtBase, gravityAlignAtTop, gravityAlignCurve,
								horizontalAlignAtBase, horizontalAlignAtTop, horizontalAlignCurve);
							spawnedSprouts.Add (spawnedSprout);
						}
						addedSprouts++;
						accumAngleIntraNodes += angleIntraNodes;
						if (addedSprouts >= frequency) {
							break;
						}
					}
					accumAngleBetweenNodes += Random.Range (0, Mathf.PI * 2); //angleBetweenNodes + twirlToAdd;
				}
			}
			spawnedSprouts.Sort (delegate (BroccoTree.Sprout a, BroccoTree.Sprout b) {
				return b.position.CompareTo (a.position);
			});
			return spawnedSprouts;
		}
		*/
		/// <summary>
		/// Determines if the sprout position is withing range.
		/// </summary>
		/// <returns><c>true</c> if the sprout is within range; otherwise, <c>false</c>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="minActionRange">Minimum action range position.</param>
		/// <param name="maxActionRange">Max action range position.</param>
		public static bool IsSproutInRange (float position, float minActionRange, float maxActionRange) {
			if (position <= maxActionRange) {
				if (minActionRange == 0 && position >= minActionRange) {
					return true;
				} else if (position > minActionRange) {
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Sets the sprout relative angle parameters.
		/// </summary>
		/// <param name="sprout">Sprout.</param>
		/// <param name="minParallelAlignAtBase">Minimum parallel align at base.</param>
		/// <param name="maxParallelAlignAtBase">Maximum parallel align at base.</param>
		/// <param name="minParallelAlignAtTop">Minimum parallel align at top.</param>
		/// <param name="maxParallelAlignAtTop">Maximum parallel align at top.</param>
		/// <param name="parallelAlignCurve">Parallel align curve.</param>
		/// <param name="minGravityAlignAtBase">Minimum gravity align at base.</param>
		/// <param name="maxGravityAlignAtBase">Maximum gravity align at base.</param>
		/// <param name="minGravityAlignAtTop">Minimum gravity align at top.</param>
		/// <param name="maxGravityAlignAtTop">Maximum gravity align at top.</param>
		/// <param name="gravityAlignCurve">Gravity align curve.</param>
		/// <param name="minHorizontalAlignAtBase">Minimum horizontal align at base.</param>
		/// <param name="maxHorizontalAlignAtBase">Maximum horizontal align at base.</param>
		/// <param name="minHorizontalAlignAtTop">Minimum horizontal align at top.</param>
		/// <param name="maxHorizontalAlignAtTop">Maximum horizontal align at top.</param>
		/// <param name="horizontalAlignCurve">Horizontal align curve.</param>
		/// <param name="flipAlign">Flip align value.</param>
		/// <param name="flipAlignDirection">Flip align direction.</param>
		public static void SetSproutRelativeAngle (BroccoTree.Sprout sprout, 
			float minParallelAlignAtBase, float maxParallelAlignAtBase, float minParallelAlignAtTop, float maxParallelAlignAtTop, AnimationCurve parallelAlignCurve,
			float minGravityAlignAtBase, float maxGravityAlignAtBase, float minGravityAlignAtTop, float maxGravityAlignAtTop, AnimationCurve gravityAlignCurve,
			float minHorizontalAlignAtBase, float maxHorizontalAlignAtBase, float minHorizontalAlignAtTop, float maxHorizontalAlignAtTop, AnimationCurve horizontalAlignCurve,
			float flipAlign, Vector3 flipAlignDirection) {
			sprout.branchAlignAngle = 
				Mathf.PI / 2f * 
				Mathf.Lerp (Random.Range (minParallelAlignAtBase, maxParallelAlignAtBase),
					Random.Range (minParallelAlignAtTop, maxParallelAlignAtTop), 
					Mathf.Clamp(parallelAlignCurve.Evaluate(sprout.position), 0f, 1f));
			sprout.gravityAlign = 
				Mathf.Lerp (Random.Range (minGravityAlignAtBase, maxGravityAlignAtBase), 
					Random.Range (minGravityAlignAtTop, maxGravityAlignAtTop), 
					Mathf.Clamp(gravityAlignCurve.Evaluate(sprout.position), 0f, 1f));
			sprout.horizontalAlign = 
				Mathf.Lerp (Random.Range (minHorizontalAlignAtBase, maxHorizontalAlignAtBase), 
					Random.Range (minHorizontalAlignAtTop, maxHorizontalAlignAtTop), 
					Mathf.Clamp(horizontalAlignCurve.Evaluate(sprout.position), 0f, 1f));
			sprout.flipAlign = flipAlign;
			sprout.flipDirection = flipAlignDirection;
		}
		/// <summary>
		/// Gets the children per node.
		/// </summary>
		/// <returns>The children per node.</returns>
		/// <param name="distributionMode">Distribution mode.</param>
		/// <param name="defaultChildrenPerNode">Default children per node.</param>
		protected int GetChildrenPerNode (Distribution distributionMode, int defaultChildrenPerNode = 2) {
			if (defaultChildrenPerNode <= 0)
				defaultChildrenPerNode = 2;
			int childrenPerNode = 1;
			if (distribution == Distribution.Opposite) {
				childrenPerNode = 2;
			} else if (distribution == Distribution.Whorled) {
				childrenPerNode = defaultChildrenPerNode;
			}
			return childrenPerNode;
		}
		#endregion
	}
}