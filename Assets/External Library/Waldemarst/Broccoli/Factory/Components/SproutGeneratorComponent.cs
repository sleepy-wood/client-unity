using Broccoli.Pipe;
using Broccoli.Generator;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Sprout generator component.
	/// </summary>
	public class SproutGeneratorComponent : TreeFactoryComponent {
		#region Vars
		/// <summary>
		/// The sprout generator.
		/// </summary>
		SproutGenerator sproutGenerator = null;
		/// <summary>
		/// The sprout generator element.
		/// </summary>
		SproutGeneratorElement sproutGeneratorElement = null;
		#endregion

		#region Configuration
		/// <summary>
		/// Prepares the parameters to process with this component.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="processControl">Process control.</param>
		protected override void PrepareParams (TreeFactory treeFactory,
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null) 
		{
			if (sproutGenerator == null) {
				sproutGenerator = new SproutGenerator ();
			}
			base.PrepareParams (treeFactory, useCache, useLocalCache, processControl);
			sproutGenerator.Clear ();
			sproutGenerator.minFrequency = sproutGeneratorElement.minFrequency;
			sproutGenerator.maxFrequency = sproutGeneratorElement.maxFrequency;
			sproutGenerator.distribution = sproutGeneratorElement.distribution;
			sproutGenerator.distributionSpacingVariance = sproutGeneratorElement.distributionSpacingVariance;
			sproutGenerator.distributionAngleVariance = sproutGeneratorElement.distributionAngleVariance;
			sproutGenerator.distributionCurve = sproutGeneratorElement.distributionCurve;
			sproutGenerator.whorledStep = sproutGeneratorElement.whorledStep;
			sproutGenerator.minTwirl = sproutGeneratorElement.minTwirl;
			sproutGenerator.maxTwirl = sproutGeneratorElement.maxTwirl;
			sproutGenerator.minParallelAlignAtTop = sproutGeneratorElement.minParallelAlignAtTop;
			sproutGenerator.maxParallelAlignAtTop = sproutGeneratorElement.maxParallelAlignAtTop;
			sproutGenerator.minParallelAlignAtBase = sproutGeneratorElement.minParallelAlignAtBase;
			sproutGenerator.maxParallelAlignAtBase = sproutGeneratorElement.maxParallelAlignAtBase;
			sproutGenerator.parallelAlignCurve = sproutGeneratorElement.parallelAlignCurve;
			sproutGenerator.minGravityAlignAtTop = sproutGeneratorElement.minGravityAlignAtTop;
			sproutGenerator.maxGravityAlignAtTop = sproutGeneratorElement.maxGravityAlignAtTop;
			sproutGenerator.minGravityAlignAtBase = sproutGeneratorElement.minGravityAlignAtBase;
			sproutGenerator.maxGravityAlignAtBase = sproutGeneratorElement.maxGravityAlignAtBase;
			sproutGenerator.gravityAlignCurve = sproutGeneratorElement.gravityAlignCurve;
			/* TODO.
			sproutGenerator.horizontalAlignAtTop = sproutGeneratorElement.horizontalAlignAtTop;
			sproutGenerator.horizontalAlignAtBase = sproutGeneratorElement.horizontalAlignAtBase;
			sproutGenerator.horizontalAlignCurve = sproutGeneratorElement.horizontalAlignCurve;
			*/
			sproutGenerator.fromBranchCenter = sproutGeneratorElement.fromBranchCenter;
			sproutGenerator.distributionOrigin = sproutGeneratorElement.distributionOrigin;
			sproutGenerator.spreadEnabled = sproutGeneratorElement.spreadEnabled;
			sproutGenerator.spreadRange = sproutGeneratorElement.spreadRange;
			sproutGenerator.sproutSeeds = sproutGeneratorElement.sproutSeeds;
			sproutGenerator.helperId = pipelineElement.GetInstanceID ();
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.StructureGirth; // TODO.
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear () {
			base.Clear ();
			sproutGeneratorElement = null;
			/*
			if (sproutGenerator != null) {
				sproutGenerator.Clear ();
			}
			*/
			sproutGenerator = null;
		}
		#endregion

		#region Processing
		/// <summary>
		/// Process the tree according to the pipeline element.
		/// </summary>
		/// <param name="treeFactory">Parent tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="processControl">Process control.</param>
		public override bool Process (TreeFactory treeFactory, 
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null) 
		{
			sproutGeneratorElement = pipelineElement as SproutGeneratorElement;
			if (processControl != null) {
				PrepareParams (treeFactory, useCache, useLocalCache, processControl);
				//if (processControl.lodIndex == 1) {
					SpawnSprouts (useCache);
					/*
				} else {
					sproutGenerator.AddCachedSprouts (tree);
					sproutGenerator.Clear ();
				}
				*/
				return true;
			}
			return false;
		}
		/// <summary>
		/// Spawns the sprouts.
		/// </summary>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		private void SpawnSprouts (bool useCache) {
			if (sproutGeneratorElement.isSeedFixed) {
				sproutGenerator.randomSeed = sproutGeneratorElement.seed;
				sproutGenerator.enableRandom = false;
			} else {
				sproutGenerator.enableRandom = !useCache;
			}
			if (sproutGeneratorElement.sproutSeeds.Count > 0) {
				sproutGenerator.SpawnSprouts (tree);
			}
		}
		#endregion
	}
}