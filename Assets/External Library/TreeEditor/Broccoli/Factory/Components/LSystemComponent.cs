using System.Collections.Generic;

using UnityEngine;

using Broccoli.Base;
using Broccoli.Model;
using Broccoli.Pipe;
using Broccoli.Factory;
using Broccoli.Utils;

namespace Broccoli.Component
{
	/// <summary>
	/// LSystem component.
	/// </summary>
	public class LSystemComponent : TreeFactoryComponent, IStructureGeneratorComponent {
		#region Vars
		/// <summary>
		/// The Lindermayer system generator.
		/// </summary>
		LSystem lSystem = null;
		/// <summary>
		/// The LSystem element.
		/// </summary>
		LSystemElement lSystemElement;
		/// <summary>
		/// The branch identifier counter.
		/// </summary>
		int idCount = 0;
		/// <summary>
		/// The cached rays.
		/// </summary>
		List<LSystem.Ray> cachedRays = new List<LSystem.Ray> ();
		/// <summary>
		/// The position for the generated branches.
		/// </summary>
		Position position = new Position ();
		/// <summary>
		/// Global scale used by the factory.
		/// </summary>
		float globalScale = 1f;
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
			base.PrepareParams (treeFactory, useCache, useLocalCache, processControl);
			lSystemElement.PrepareSeed ();

			// Prepare LSystem.
			if (lSystem == null) {
				LSystem.forward = Vector3.forward;
				LSystem.right = Vector3.right;
				lSystem = new LSystem ();
			} else {
				lSystem.Clear ();
			}

			// Set upward direction
			Vector3 upwardDirection;
			PositionerElement positionerElement = 
				(PositionerElement) lSystemElement.GetDownstreamElement (PipelineElement.ClassType.Positioner);
			if (positionerElement != null && positionerElement.useCustomPositions) {
				position = positionerElement.GetPosition ();
				if (position.overrideRootDirection)
					upwardDirection = position.rootDirection;
				else
					upwardDirection = GlobalSettings.againstGravityDirection;
			} else {
				position.rootPosition = Vector3.zero;
				upwardDirection = GlobalSettings.againstGravityDirection;
			}
			// Set direction for the branches.
			LSystem.upward = upwardDirection;

			lSystem.useLastGeneratedInput = useLocalCache && !lSystemElement.requiresNewStructure;
			lSystem.iterations = lSystemElement.iterations;
			lSystem.axiom = lSystemElement.axiom;
			lSystem.accumulativeModeEnabled = lSystemElement.accumulativeMode;
			lSystem.length = lSystemElement.length;
			lSystem.lengthGrowth = lSystemElement.lengthGrowth;
			lSystem.turnAngle = lSystemElement.turnAngle;
			lSystem.turnAngleGrowth = lSystemElement.turnAngleGrowth;
			lSystem.pitchAngle = lSystemElement.pitchAngle;
			lSystem.pitchAngleGrowth = lSystemElement.pitchAngleGrowth;
			lSystem.rollAngle = lSystemElement.rollAngle;
			lSystem.rollAngleGrowth = lSystemElement.rollAngleGrowth;
			for (int i = 0; i < lSystemElement.rules.Count; i++) {
				if (!lSystem.rules.ContainsKey (lSystemElement.rules[i].symbol [0]))
					lSystem.rules.Add (lSystemElement.rules[i].symbol [0], new List<LSystem.Rule> ());
				lSystem.rules [lSystemElement.rules[i].symbol [0]].Add (lSystemElement.rules[i]);
			}
			lSystemElement.requiresNewStructure = false;

			globalScale = treeFactory.treeFactoryPreferences.factoryScale;
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.Structure;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
			lSystemElement = null;
			if (lSystem != null) {
				lSystem.Clear ();
			}
			idCount = 0;
			cachedRays.Clear ();
		}
		#endregion

		#region Processing
		/// <summary>
		/// Process the tree according to the pipeline element.
		/// </summary>
		/// <param name="treeFactory">Parent tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="ProcessControl">Process control.</param>
		public override bool Process (TreeFactory treeFactory, 
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl ProcessControl = null) {
			if (pipelineElement != null && tree != null) {
				lSystemElement = pipelineElement as LSystemElement;
				PrepareParams (treeFactory, useCache, useLocalCache || useCache);
				StructureTree (false);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Structures the tree.
		/// </summary>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		private void StructureTree (bool useCache = false) {
			List<LSystem.Ray> rays;
			if (useCache && cachedRays.Count > 0) {
				rays = cachedRays;
			} else {
				rays = lSystem.Generate ();
				cachedRays.Clear();
				for (int i = 0; i < rays.Count; i++) {
					cachedRays.Add (rays[i].Clone ());
				}
			}
			idCount = 0;
			for (int i = 0; i < rays.Count; i++) {
				BroccoTree.Branch branch = new BroccoTree.Branch ();
				branch.id = idCount;
				idCount++;
				branch.direction = rays[i].direction;
				tree.AddBranch (branch, position.rootPosition / globalScale);
				PopulateBranch (branch, rays[i].rays);
			}
			//tree.position = position.rootPosition;
		}
		/// <summary>
		/// Populates the branch.
		/// </summary>
		/// <param name="branch">Branch.</param>
		/// <param name="rays">Rays.</param>
		public void PopulateBranch (BroccoTree.Branch branch, List<LSystem.Ray> rays) {
			for (int i = 0; i < rays.Count; i++) {
				BroccoTree.Branch childBranch = new BroccoTree.Branch();
				childBranch.id = idCount;
				idCount++;
				childBranch.direction = rays[i].direction;
				childBranch.position = 1.0f;
				branch.AddBranch (childBranch);
				PopulateBranch (childBranch, rays[i].rays);
			}
		}
		#endregion

		#region IStructureGeneratorComponent
		/// <summary>
		/// The number of available root positions.
		/// </summary>
		public int GetAvailableRootPositions () {
			return 1;
		}
		/// <summary>
		/// The unique root position offset when all the root branches have this origin.
		/// </summary>
		public Vector3 GetUniqueRootPosition () {
			return position.rootPosition;
		}
		#endregion
	}
}