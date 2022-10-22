using System.Collections.Generic;

using UnityEngine;

using Broccoli.Pipe;
using Broccoli.Model;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Sparsing transform component.
	/// </summary>
	public class SparsingTransformComponent : TreeFactoryComponent {
		#region Vars
		/// <summary>
		/// The sparsing transform element.
		/// </summary>
		SparsingTransformElement sparsingTransformElement = null;
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
			sparsingTransformElement.PrepareSeed ();
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.StructurePosition;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
			sparsingTransformElement = null;
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
				sparsingTransformElement = pipelineElement as SparsingTransformElement;
				PrepareParams (treeFactory, useCache, useLocalCache);
				ReorderBranches ();
				SparseLengthBranches ();
				SparseTwirlBranches ();
				return true;
			}
			return false;
		}
		#endregion

		#region Reordering
		/// <summary>
		/// Reorders the branches.
		/// </summary>
		void ReorderBranches () {
			List<BroccoTree.Branch> allBranches = tree.GetDescendantBranches ();
			for (int i = 0; i < sparsingTransformElement.sparseLevels.Count; i++) {
				for (int j = 0; j < allBranches.Count; j++) {
					if (!allBranches [j].isRoot) {
						if (sparsingTransformElement.sparseLevels[i].level == allBranches[j].GetLevel () && allBranches[j].branches.Count > 1) {
							switch (sparsingTransformElement.sparseLevels[i].reorderMode) {
							case SparsingTransformElement.ReorderMode.Reverse:
								ReorderReverse (allBranches[j].branches);
								break;
							case SparsingTransformElement.ReorderMode.Random:
								ReorderRandom (allBranches[j].branches);
								break;
							case SparsingTransformElement.ReorderMode.HeavierOnTop:
								ReorderHeavier (allBranches[j].branches);
								break;
							case SparsingTransformElement.ReorderMode.HeavierAtBottom:
								ReorderHeavier (allBranches[j].branches, false);
								break;
							}
						}
					}
				}
			}
		}
		/// <summary>
		/// Reverses the order for the branches.
		/// </summary>
		/// <param name="branches">Branches.</param>
		void ReorderReverse (List<BroccoTree.Branch> branches) {
			if (branches.Count > 1) {
				float[] positions = new float[branches.Count];
				for (int i = 0; i < branches.Count; i++) {
					if (!branches[i].isRoot) {
						positions [i] = branches [branches.Count - 1 - i].position;
					}
				}
				for (int i = 0; i < branches.Count; i++) {
					if (!branches[i].isRoot) {
						branches [i].position = positions [i];
					}
				}
				OrderBranchesByPosition (branches);
			}
		}
		/// <summary>
		/// Randomly reorders the branches.
		/// </summary>
		/// <param name="branches">Branches.</param>
		void ReorderRandom (List<BroccoTree.Branch> branches) {
			if (branches.Count > 0) {
				for (int i = 0; i < branches.Count; i++) {
					if (!branches[i].isRoot) {
						BroccoTree.Branch tempBranch = branches [i];
						float tempPosition = branches [i].position;
						int randomIndex = Random.Range (i, branches.Count);
						branches [i].position = branches [randomIndex].position;
						branches [randomIndex].position = tempPosition;
						branches [i] = branches [randomIndex];
						branches [randomIndex] = tempBranch;
					}
				}
				OrderBranchesByPosition (branches);
				branches [0].parent.UpdateFollowUps ();
			}
		}
		/// <summary>
		/// Reorders the branches according to their weight on top or bottom of the main trunk.
		/// </summary>
		/// <param name="branches">Branches.</param>
		/// <param name="heavierOnTop">If set to <c>true</c> heavier branches on top.</param>
		void ReorderHeavier (List<BroccoTree.Branch> branches, bool heavierOnTop = true) {
			float[] positions = new float[branches.Count];
			for (int i = 0; i < branches.Count; i++) {
				positions[i] = branches [i].position;
			}
			System.Array.Sort (positions);
			if (heavierOnTop) {
				branches.Sort (delegate (BroccoTree.Branch a, BroccoTree.Branch b) {
					return b.offspringLevels.CompareTo (a.offspringLevels);
				});
			} else {
				branches.Sort (delegate (BroccoTree.Branch a, BroccoTree.Branch b) {
					return a.offspringLevels.CompareTo (b.offspringLevels);
				});
			}
			for (int i = 0; i < branches.Count; i++) {
				if (!branches[i].isRoot) {
					branches [i].position = positions [branches.Count - 1 - i];
				}
			}
			OrderBranchesByPosition (branches);
		}
		/// <summary>
		/// Orders the branches by position.
		/// </summary>
		/// <param name="branches">Branches.</param>
		void OrderBranchesByPosition (List<BroccoTree.Branch> branches) {
			branches.Sort (delegate (BroccoTree.Branch x, BroccoTree.Branch y) {
				return y.position.CompareTo (x.position);
			});
		}
		#endregion

		#region Length Sparsing
		/// <summary>
		/// Sparses the length branches.
		/// </summary>
		void SparseLengthBranches () {
			for (int i = 0; i < sparsingTransformElement.sparseLevels.Count; i++) {
				if (sparsingTransformElement.sparseLevels[i].lengthSparsingMode == SparsingTransformElement.LengthSparsingMode.Absolute) {
					List<BroccoTree.Branch> allBranches = tree.GetDescendantBranches ();
					for (int j = 0; j < allBranches.Count; j++) {
						if (!allBranches [j].isRoot) {
							if (sparsingTransformElement.sparseLevels[i].level == allBranches[j].GetLevel () && allBranches[j].branches.Count > 1) {
								float sparceFraction = sparsingTransformElement.sparseLevels[i].lengthSparsingValue / (float)allBranches[j].branches.Count;
								int sparseStep = 0;
								float minAngle = -1f;
								//int minAngleBranchIndex = 0;
								for (int k = 0; k < allBranches[j].branches.Count; k++) {
									if (!allBranches[j].branches[k].isRoot && allBranches[j].branches [k].parent != null) {
										float angle = Vector3.Angle (allBranches[j].branches [k].direction, allBranches[j].branches [k].parent.direction);
										if (minAngle < 0f ||
										angle < minAngle) {
											minAngle = angle;
											//minAngleBranchIndex = k;
										}
									}
								}
								for (int k = 0; k < allBranches[j].branches.Count; k++) {
									if (!allBranches[j].branches[k].isRoot) {
										allBranches[j].branches [k].position = 1 - sparseStep * sparceFraction;
										allBranches[j].branches [k].UpdatePosition ();
										sparseStep++;
									}
								}
							}
						}
					}
				}
			}
		}
		#endregion

		#region Twirl Sparsing
		/// <summary>
		/// Sparses the twirl branches.
		/// </summary>
		void SparseTwirlBranches () {
			for (int i = 0; i < sparsingTransformElement.sparseLevels.Count; i++) {
				if (sparsingTransformElement.sparseLevels[i].twirlSparsingMode == SparsingTransformElement.TwirlSparsingMode.Additive) {
					List<BroccoTree.Branch> allBranches = tree.GetDescendantBranches ();
					for (int j = 0; j < allBranches.Count; j++) {
						if (!allBranches[j].isRoot) {
							if (sparsingTransformElement.sparseLevels[i].level == allBranches[j].GetLevel () && allBranches[j].branches.Count > 1) {
								SparseTwirlBranchesSegment (sparsingTransformElement.sparseLevels[i].twirlSparsingValue, allBranches[j].branches, allBranches[j]);
							}
						}
					}
				}
			}
		}
		/// <summary>
		/// Sparses the twirl branches segment.
		/// </summary>
		/// <param name="twirlSparsingValue">Twirl parsing value.</param>
		/// <param name="branches">Branches.</param>
		/// <param name="parentBranch">Parent branch.</param>
		/// <param name="referenceTwirl">Reference twirl.</param>
		void SparseTwirlBranchesSegment (float twirlSparsingValue, List<BroccoTree.Branch> branches, BroccoTree.Branch parentBranch, float referenceTwirl = 0) {
			int twirlStep = 1;
			for (int i = 0; i < branches.Count; i++) {
				if (!branches[i].isRoot) {
					float twirl = referenceTwirl;
					if (branches[i].parent == parentBranch) {
						twirl = twirlSparsingValue * twirlStep;
					}
					Quaternion rotation = Quaternion.AngleAxis (twirl * Mathf.Rad2Deg, parentBranch.direction);
					branches[i].direction = rotation * branches[i].direction;
					//branches[i].normal = rotation * branches[i].normal;
					SparseTwirlBranchesSegment (twirlSparsingValue, branches[i].branches, parentBranch, twirl);
					twirlStep++;
				}
			}
		}
		#endregion
	}
}