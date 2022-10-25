using System.Collections.Generic;

using UnityEngine;

using Broccoli.Pipe;
using Broccoli.Model;
using Broccoli.Factory;
using Broccoli.Generator;

namespace Broccoli.Component
{
	/// <summary>
	/// Branch bender component.
	/// </summary>
	public class BranchBenderComponent : TreeFactoryComponent {
		#region Vars
		/// <summary>
		/// The branch bender element.
		/// </summary>
		BranchBenderElement branchBenderElement = null;
		/// <summary>
		/// Dictionary to save structure levels overriding global noise.
		/// </summary>
		private Dictionary<int, StructureGenerator.StructureLevel> overrideNoiseLevels = new Dictionary<int, StructureGenerator.StructureLevel> ();
		/// <summary>
		/// Number of offspring levels on the tree.
		/// </summary>
		private float treeMaxHierarchy;
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
			branchBenderElement.PrepareSeed ();

			// Get structure levels overriding noise.
			overrideNoiseLevels.Clear ();
			StructureGeneratorElement structureGeneratorElement = 
				(StructureGeneratorElement)pipelineElement.GetUpstreamElement (PipelineElement.ClassType.StructureGenerator);
			if (structureGeneratorElement != null) {
				// Add root generator?
				if (structureGeneratorElement.rootStructureLevel.overrideNoise) {
					overrideNoiseLevels.Add (structureGeneratorElement.rootStructureLevel.id, structureGeneratorElement.rootStructureLevel);
				}
				// Add children generators?
				for (int i = 0; i < structureGeneratorElement.flatStructureLevels.Count; i++) {
					if (structureGeneratorElement.flatStructureLevels [i].overrideNoise) {
						overrideNoiseLevels.Add (structureGeneratorElement.flatStructureLevels [i].id, structureGeneratorElement.flatStructureLevels [i]);
					}
				}

			}

			treeMaxHierarchy = tree.GetOffspringLevel ();
		}
		/// <summary>
		/// Gets the changed aspects.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.StructureBendPoints;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
			overrideNoiseLevels.Clear ();
			branchBenderElement = null;
		}
		#endregion

		#region Processing
		/// <summary>
		/// Process the tree branches for bending.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="ProcessControl">Process control.</param>
		public override bool Process (TreeFactory treeFactory, 
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null) 
		{
			branchBenderElement = pipelineElement as BranchBenderElement;
			if (processControl != null) {
				/*
				if (pipelineElement != null && tree != null && processControl.lodIndex == 1 && 
					(processControl.lockedAspects & (int)TreeFactoryProcessControl.ChangedAspect.Structure) == 0
				) {
					*/
				if (pipelineElement != null && tree != null && 
					(processControl.lockedAspects & (int)TreeFactoryProcessControl.ChangedAspect.Structure) == 0
				) {
					PrepareParams (treeFactory, useCache, useLocalCache, processControl);
					if (branchBenderElement.applyDirectionalBending) {
						ApplyDirectionalBending ();
						branchBenderElement.onDirectionalBending?.Invoke (tree, branchBenderElement);
					}
					if (branchBenderElement.applyJointSmoothing) {
						ApplyFollowUpSmoothing ();
						branchBenderElement.onFollowUpSmoothing?.Invoke (tree, branchBenderElement);
					}
					if (branchBenderElement.applyNoise) {
						ApplyBranchNoise ();
						branchBenderElement.onBranchNoise?.Invoke (tree, branchBenderElement);
					}
				}
				return true;
			}
			return false;
		}
		private void SetAutoProcess (bool autoProcessEnable) {
			for (int i = 0; i < tree.branches.Count; i++) {
				SetAutoProcessRecursive (tree.branches [i], autoProcessEnable);
			}
		}
		private void SetAutoProcessRecursive (BroccoTree.Branch branch, bool autoProcessEnable) {
			branch.curve.autoProcess = autoProcessEnable;
			for (int i = 0; i < branch.branches.Count; i++) {
				SetAutoProcessRecursive (branch.branches [i], autoProcessEnable);
			}
		}
		#endregion

		#region Directional Bending
		private void ApplyDirectionalBending () {
			/*
			 * Action per branch (starting at the parent).
			 * 1. Get length of branch + followups.
			 * 2. Get first branch first node girth.
			 * 3. Get last branch last node girth.
			 * 4. Get first branch Hierarchy Level Factor (0-1, use curve). HLF
			 * 5. Get first branch first node step (-1, 1) * HLF.
			 * 6. Get last branch last node step (-1, 1) * HLF.
			 * 7. Apply new direction to branch ND = LERP (node.direction, up, (absPosA + absPosB /2))
			 * 8. Apply handlers.
			 */
			List<BroccoTree.Branch> branches = tree.branches;
			for (int i = 0; i < branches.Count; i++) {
				ApplyDirectionalBendingRecursive (branches[i]);
			}
		}
		private void ApplyDirectionalBendingRecursive (BroccoTree.Branch branch) {
			float totalLength = 0f;
			float girthAtBase = 1f;
			float girthAtTop = 1f;
			Vector3 directionAtBase = Vector3.up;
			Vector3 directionAtTop = Vector3.up;
			float hierarchyLevelFactor = 1f;
			float girthFactorAtBase = 0f;
			float girthFactorAtTop = 0f;

			BroccoTree.Branch baseBranch = branch;

			// 1. Traverse parent to child branch to add up the total length.
			// 2,3. Get girth at base and at top.
			girthAtBase = branch.GetGirthAtPosition (0f);
			directionAtBase = branch.GetDirectionAtPosition (0f);
			do {
				totalLength += branch.length;
				if (branch.followUp == null) {
					girthAtTop = branch.GetGirthAtPosition (1f);
					directionAtTop = branch.GetDirectionAtPosition (1f);
				}
				branch = branch.followUp;
			} while (branch != null);

			// 4. Get hierarchy level factor.
			branch = baseBranch;
			

			// 5, 6 Get girth factor at base and at top.
			if (branch.isRoot) {
				girthFactorAtBase = Mathf.InverseLerp (tree.maxGirth, tree.minGirth, girthAtBase);
				girthFactorAtBase = Mathf.Lerp (branchBenderElement.forceAtRootTrunk, branchBenderElement.forceAtRootTips, girthFactorAtBase);
				girthFactorAtTop = Mathf.InverseLerp (tree.maxGirth, tree.minGirth, girthAtTop);
				girthFactorAtTop = Mathf.Lerp (branchBenderElement.forceAtRootTrunk, branchBenderElement.forceAtRootTips, girthFactorAtTop);
			} else {
				girthFactorAtBase = Mathf.InverseLerp (tree.maxGirth, tree.minGirth, girthAtBase);
				girthFactorAtBase = Mathf.Lerp (branchBenderElement.forceAtTrunk, branchBenderElement.forceAtTips, girthFactorAtBase);
				girthFactorAtTop = Mathf.InverseLerp (tree.maxGirth, tree.minGirth, girthAtTop);
				girthFactorAtTop = Mathf.Lerp (branchBenderElement.forceAtTrunk, branchBenderElement.forceAtTips, girthFactorAtTop);
			}

			// 7. Set position to the last node of each branch.
			float accumLength = 0f;
			float positionStep = 1f;
			float positionStepAtBase = 0f;
			float positionStepAtTop = 0f;
			bool baseChanged = false;
			bool topChanged = false;
			float baseBranchLength = 0f;
			float topBranchLength = 0f;
			Vector3 newDirection;
			
			do {
				// 9. Apply bending to branch.
				if (!branch.isTuned) {
					// Get hierarchy factor
					hierarchyLevelFactor = Mathf.InverseLerp (0f, treeMaxHierarchy - 1, branch.GetHierarchyLevel ());
					if (branch.isRoot) {
						hierarchyLevelFactor = branchBenderElement.rootHierarchyDistributionCurve.Evaluate (hierarchyLevelFactor);
					} else {
						hierarchyLevelFactor = branchBenderElement.hierarchyDistributionCurve.Evaluate (hierarchyLevelFactor);
					}

					positionStep = Mathf.Lerp (girthFactorAtBase, girthFactorAtTop,
						(accumLength + (branch.length / 2f)) / totalLength
					);
					if (positionStep > 0) {
						newDirection = Vector3.Lerp (branch.GetDirectionAtPosition (0f), 
							(branch.isRoot?Base.GlobalSettings.gravityDirection:Base.GlobalSettings.againstGravityDirection), 
							positionStep * hierarchyLevelFactor);
					} else {
						positionStep *= -1;
						newDirection = Vector3.Lerp (branch.GetDirectionAtPosition (0f), 
							(branch.isRoot?Base.GlobalSettings.againstGravityDirection:Base.GlobalSettings.gravityDirection), 
							positionStep * hierarchyLevelFactor);
					}
					branch.ResetDirection (newDirection, true);
					if (!branch.IsFollowUp ()) {
						baseChanged = true;
						baseBranchLength = branch.length;
						positionStepAtBase = positionStep;
					}
					if (branch.followUp == null) {
						topChanged = true;
						topBranchLength = branch.length;
						positionStepAtTop = girthFactorAtTop;
						if (positionStepAtTop > 0) {
							directionAtTop = Vector3.Lerp (branch.GetDirectionAtPosition (1f), 
								(branch.isRoot?Base.GlobalSettings.gravityDirection:Base.GlobalSettings.againstGravityDirection), 
								positionStepAtTop * hierarchyLevelFactor);
						} else {
							directionAtTop = Vector3.Lerp (branch.GetDirectionAtPosition (1f), 
								(branch.isRoot?Base.GlobalSettings.againstGravityDirection:Base.GlobalSettings.gravityDirection), 
								-positionStepAtTop * hierarchyLevelFactor);
						}
					}
				}
				accumLength += branch.length;
				// Apply handles
				if (baseChanged || topChanged) {
					// APPLY TO BRANCHES
					if (!branch.isRoot) {
						Vector3 hVector = Vector3.ProjectOnPlane (baseBranch.direction * baseBranch.length, Base.GlobalSettings.againstGravityDirection);
						Vector3 vVector = Vector3.ProjectOnPlane (baseBranch.direction * baseBranch.length, hVector);
						Vector3 directionVector = baseBranch.direction * baseBranch.length * 0.5f;
						float branchHierarchyFactor = Mathf.InverseLerp (0f, treeMaxHierarchy - 1, branch.GetHierarchyLevel ()); 
						float baseHierarchyFactor = branchBenderElement.horizontalAlignHierarchyDistributionCurve.Evaluate (branchHierarchyFactor);
						float topHierarchyFactor = branchBenderElement.verticalAlignHierarchyDistributionCurve.Evaluate (branchHierarchyFactor);

						/*
						Vector3 treeOrigin = TreeFactory.GetActiveInstance ().GetPreviewTreeWorldOffset ();
						float treeScale = TreeFactory.GetActiveInstance ().treeFactoryPreferences.factoryScale;
						*/

						// Apply bending to branch base.
						if (baseChanged) {
							baseBranch.curve.nodes[0].handleStyle = BezierNode.HandleStyle.Free;
							if (branchBenderElement.horizontalAlignAtBase > 0) {
								baseBranch.curve.nodes[0].handle2 = Vector3.Lerp (
									directionVector * branchBenderElement.horizontalAlignStrength * baseHierarchyFactor, 
									hVector * branchBenderElement.horizontalAlignStrength * baseHierarchyFactor, 
									branchBenderElement.horizontalAlignAtBase);
							} else {
								baseBranch.curve.nodes[0].handle2 = Vector3.Lerp (
									directionVector * branchBenderElement.horizontalAlignStrength * baseHierarchyFactor, 
									vVector * branchBenderElement.horizontalAlignStrength * baseHierarchyFactor, 
									-branchBenderElement.horizontalAlignAtBase);
							}
							/*
							Debug.DrawLine (
								Broccoli.Utils.TreeEditorUtils.ToTreeSpace (baseBranch.origin + baseBranch.curve.nodes[0].handle2, treeOrigin, treeScale),
								Broccoli.Utils.TreeEditorUtils.ToTreeSpace (baseBranch.origin, treeOrigin, treeScale),
								Color.red, 2f); // Draw H Vector
							*/
							baseBranch.curve.ProcessAfterCurveChanged ();
						}
						// Apply bending to branch top.
						if (topChanged) {
							branch.curve.Last ().handleStyle = BezierNode.HandleStyle.Free;
							if (branchBenderElement.verticalAlignAtTop > 0) {
								branch.curve.Last ().handle1 = Vector3.Lerp (
									-directionVector * branchBenderElement.verticalAlignStrength * topHierarchyFactor, 
									-vVector * branchBenderElement.verticalAlignStrength * topHierarchyFactor, 
									branchBenderElement.verticalAlignAtTop);
							} else {
								branch.curve.Last ().handle1 = Vector3.Lerp (
									-directionVector * branchBenderElement.verticalAlignStrength * topHierarchyFactor, 
									-hVector * branchBenderElement.verticalAlignStrength * topHierarchyFactor, 
									-branchBenderElement.verticalAlignAtTop);
							}
							/*
							Debug.DrawLine (
								Broccoli.Utils.TreeEditorUtils.ToTreeSpace (baseBranch.origin, treeOrigin, treeScale),
								Broccoli.Utils.TreeEditorUtils.ToTreeSpace (baseBranch.origin + branch.curve.Last ().handle1, treeOrigin, treeScale),
								Color.cyan, 2f); // Draw V Vector
							*/
							branch.curve.ProcessAfterCurveChanged ();
						}
					} else {
						// APPLY TO ROOTS
						if (baseChanged) {
							baseBranch.curve.nodes[0].handleStyle = BezierNode.HandleStyle.Free;
							baseBranch.curve.nodes[0].handle2 = directionAtBase.normalized * (baseBranchLength * (1f - Mathf.Abs(positionStepAtBase * hierarchyLevelFactor)) * 0.5f);
							baseBranch.curve.ProcessAfterCurveChanged ();
						}
						if (topChanged) {
							branch.curve.Last ().handleStyle = BezierNode.HandleStyle.Free;
							branch.curve.Last ().handle1 = -directionAtTop * (topBranchLength * Mathf.Abs (positionStepAtTop * hierarchyLevelFactor) * 0.5f);
							branch.curve.ProcessAfterCurveChanged ();
						}
					}
				}

				//Apply changes to children branches not being a followup.
				for (int i = 0; i < branch.branches.Count; i++) {
					if (!branch.branches[i].IsFollowUp ()){
						ApplyDirectionalBendingRecursive (branch.branches[i]);
					}
				}

				branch = branch.followUp;
			} while (branch != null);
		}
		#endregion

		#region FollowUp Smoothing
		/// <summary>
		/// Smooths angle between a branch and its followup branch.
		/// </summary>
		private void ApplyFollowUpSmoothing () {
			List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
			// Apply bending to joints between branches and roots.
			for (int i = 0; i < branches.Count; i++) {
				if (branches[i].followUp != null) {
					Vector3 followUpDirection = branches[i].followUp.GetPointAtPosition (1f) - branches[i].GetPointAtPosition (0f).normalized;
					Vector3 handle1 = -followUpDirection.normalized * 
							branches[i].curve.length * 0.5f * (branches[i].isRoot?branchBenderElement.smoothRootJointStrength:branchBenderElement.smoothJointStrength);
					Vector3 handle2 = followUpDirection.normalized * 
							branches[i].followUp.curve.length * 0.5f * (branches[i].isRoot?branchBenderElement.smoothRootJointStrength:branchBenderElement.smoothJointStrength);
					if (!branches[i].isTuned) {
						branches[i].curve.Last().handleStyle = BezierNode.HandleStyle.Aligned;
						branches[i].followUp.curve.First().handleStyle = BezierNode.HandleStyle.Aligned;
						branches[i].curve.Last().handle1 = handle1;
						branches[i].curve.Last().handle2 = handle2;
					}
					if (!branches[i].followUp.isTuned) {
						branches[i].curve.Last().handleStyle = BezierNode.HandleStyle.Aligned;
						branches[i].followUp.curve.First().handleStyle = BezierNode.HandleStyle.Aligned;
						branches[i].followUp.curve.First().handle2 = handle2;
						branches[i].followUp.curve.First().handle1 = handle1;
					}
				}
			}
		}
		#endregion

		#region Branch Noise
		private void ApplyBranchNoise () {
			for (int i = 0; i < tree.branches.Count; i++) {
				ApplyBranchNoiseRecursive (tree.branches [i], Random.Range (0f, 3f));
			}
		}
		private void ApplyBranchNoiseRecursive (BroccoTree.Branch branch, float lengthOffset) {
			float factorAtBase = Mathf.InverseLerp (0f, treeMaxHierarchy, branch.GetHierarchyLevel ());
			float factorAtTop = Mathf.InverseLerp (0f, treeMaxHierarchy, branch.GetHierarchyLevel () + 1);

			if (overrideNoiseLevels.Count > 0 && overrideNoiseLevels.ContainsKey (branch.helperStructureLevelId)) {
				StructureGenerator.StructureLevel structureLevel = overrideNoiseLevels [branch.helperStructureLevelId];
				branch.curve.SetNoise (structureLevel.noise, structureLevel.noise,
					structureLevel.noiseScale, structureLevel.noiseScale, false, lengthOffset);
			} else {
				if (branch.isRoot) {
					branch.curve.SetNoise (
						Mathf.Lerp (branchBenderElement.noiseAtRootBase, branchBenderElement.noiseAtRootBottom, factorAtBase),
						Mathf.Lerp (branchBenderElement.noiseAtRootBase, branchBenderElement.noiseAtRootBottom, factorAtTop),
						Mathf.Lerp (branchBenderElement.noiseScaleAtBase, branchBenderElement.noiseScaleAtRootBottom, factorAtBase),
						Mathf.Lerp (branchBenderElement.noiseScaleAtBase, branchBenderElement.noiseScaleAtRootBottom, factorAtTop),
						false,
						lengthOffset
					);
				} else {
					branch.curve.SetNoise (
						Mathf.Lerp (branchBenderElement.noiseAtBase, branchBenderElement.noiseAtTop, factorAtBase),
						Mathf.Lerp (branchBenderElement.noiseAtBase, branchBenderElement.noiseAtTop, factorAtTop),
						Mathf.Lerp (branchBenderElement.noiseScaleAtBase, branchBenderElement.noiseScaleAtTop, factorAtBase),
						Mathf.Lerp (branchBenderElement.noiseScaleAtBase, branchBenderElement.noiseScaleAtTop, factorAtTop),
						false,
						lengthOffset
					);
				}
			}
			branch.curve.ComputeSamples ();
			branch.curve.ProcessAfterCurveChanged ();
			for (int i = 0; i < branch.branches.Count; i++) {
				ApplyBranchNoiseRecursive (branch.branches[i], lengthOffset + (branch.curve.length * branch.branches[i].position));
			}
		}
		#endregion

		#region Others
		/// <summary>
		/// Sets the curving on the branches.
		/// </summary>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="ProcessControl">Process control.</param>
		private void SetCurve (bool useCache, TreeFactoryProcessControl ProcessControl = null) {
			/*
			List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
			if (!useCache || (ProcessControl != null && 
				ProcessControl.HasChangedAspect(TreeFactoryProcessControl.ChangedAspect.Structure))) {

				// Spacing bend points.
				int totalBendPoints;
				//float bendAngleUp, bendAngleRight, bendAngleForward, bendSegmentPos;
				float bendSegmentPos, bendPosition, gravityAngleFactor, angleToGravity;
				Vector3 branchOriginalDirection, bendPointDirection;

				// Get min and max branch level on the tree.
				float branchesMinLength;
				float branchesMaxLength;
				tree.GetMinMaxLength (out branchesMinLength, out branchesMaxLength);

				for (int i = 0; i < branches.Count; i++) {
					// Clear previos bend points.
					branches[i].ClearBendPoints ();

					// Get total number of bend points for this branch.
					totalBendPoints = GetTotalBendPoints (branches[i], branchesMinLength, branchesMaxLength, treeLevelCount);
					bendSegmentPos = 1 / (float)(totalBendPoints + 1);

					// Get angle to gravity and normalize it (0,1).
					if (branchBenderElement.gravityAngleFactor > 0) {
						angleToGravity = Vector3.Angle (branches[i].direction, GlobalSettings.againstGravityDirection);
					} else {
						angleToGravity = Vector3.Angle (branches[i].direction, GlobalSettings.gravityDirection);
					}
					if (angleToGravity > 90) {
						angleToGravity = 180 - angleToGravity;
					} else {
						angleToGravity = 90;
					}
					angleToGravity = gravityNormalCurve.Evaluate (angleToGravity);

					// Set gravity angle factor and normalize it (0,1).
					gravityAngleFactor = branchBenderElement.gravityAngleFactor / (float)(totalBendPoints + 1) * 180f;
					if (gravityAngleFactor < 0) {
						gravityAngleFactor = -Mathf.InverseLerp (0f, -180f, gravityAngleFactor);
					} else {
						gravityAngleFactor = Mathf.InverseLerp (0f, 180f, gravityAngleFactor);
					}

					//Apply gravity factor to branch steam.
					branchOriginalDirection = branches[i].direction;
					if (gravityAngleFactor > 0) {
						branches[i].direction = Vector3.Lerp (branches[i].direction, GlobalSettings.againstGravityDirection,
							angleToGravity * gravityAngleFactor);
					} else if (gravityAngleFactor < 0) {
						branches[i].direction = Vector3.Lerp (branches[i].direction, GlobalSettings.gravityDirection,
							-angleToGravity * gravityAngleFactor);
					}

					// Apply direction to bendpoints and add to branch.
					for (int j = 1; j <= totalBendPoints; j++) {
						bendPosition = bendSegmentPos * j;
						bendPosition += GetBendPointRandomSpacing (branches[i], bendSegmentPos);
						bendPosition = Mathf.Clamp01 (branchBenderElement.spacingCurve.Evaluate (bendPosition));
						float gravityAngleFactorCurve = 1f;
						if (bendPosition < 1 && bendPosition > 0) {
							gravityAngleFactorCurve = 
								Mathf.Clamp01 (branchBenderElement.gravityAngleCurve.Evaluate (
									Mathf.InverseLerp (0, treeLevelCount, branches[i].GetHierarchyLevel () + bendPosition)));
							if (gravityAngleFactor > 0) {
								bendPointDirection = 
									Vector3.Lerp (branchOriginalDirection, GlobalSettings.againstGravityDirection,
										angleToGravity * gravityAngleFactor * gravityAngleFactorCurve * (j + 1));
							} else {
								bendPointDirection = Vector3.Lerp (branchOriginalDirection, GlobalSettings.gravityDirection,
									-angleToGravity * gravityAngleFactor * gravityAngleFactorCurve * (j + 1));
							}
							branches[i].AddBendPoint (new BroccoTree.Branch.BendPoint(bendPosition, bendPointDirection));
						}
					}
					branches[i].RecalculateNormals (0f);
				}
			} else {
				// TODO: use cache.
			}
			*/
		}
		/// <summary>
		/// Randomizes bending points on the tree.
		/// </summary>
		private void RandomizeBending () {
			for (int i = 0; i < tree.branches.Count; i++) {
				RandomizeBendingBranch (tree.branches[i]);
			}
		}
		/// <summary>
		/// Apply randomized angle bending to all the beding points on a branch..
		/// </summary>
		/// <param name="branch">Branch.</param>
		private void RandomizeBendingBranch (BroccoTree.Branch branch) {
			/*
			if (branch.GetBendPoints ().Count > 0) {
				Vector3 tempRotateAround, branchDirection, lastBranchPosition;
				float angle = 0f;
				bool isAlt = false;
				float levelCurvePosition = branchBenderElement.randomAngleCurve.Evaluate (
					                          Mathf.InverseLerp (0, treeLevelCount, branch.GetHierarchyLevel ()));
				// Set new bendpoint positions.
				List<BroccoTree.Branch.BendPoint> bendPoints = branch.GetBendPoints ();
				for (int i = 0; i < bendPoints.Count; i++) {
					tempRotateAround = bendPoints[i].normal * medianBranchGirth * levelCurvePosition;
					if (isAlt) {
						angle += 180;
					} else {
						angle -= 180;
					}
					angle += Random.Range (-branchBenderElement.randomAngleRange, branchBenderElement.randomAngleRange);
					branchDirection = bendPoints[i].direction;
					tempRotateAround = Quaternion.AngleAxis (angle, branchDirection) * tempRotateAround;
					bendPoints[i].tempRotateAround = branch.GetPointAtPosition (bendPoints[i].position) + tempRotateAround;
					isAlt = !isAlt;
				}
				lastBranchPosition = branch.GetPointAtPosition (1f);
				// Change direction of branch towards 1st bendpoint new position.
				branch.direction = (bendPoints[0].tempRotateAround - branch.GetPointAtPosition (0f)).normalized;
				// Change direction for the rest of the bendpoints.
				for (int i = 0; i < bendPoints.Count - 1; i++) {
					bendPoints[i].direction = 
						(bendPoints[i + 1].tempRotateAround - 
							bendPoints[i].tempRotateAround).normalized;
				}
				// Change direction for the last bendpoint;
				bendPoints[bendPoints.Count - 1].direction = 
					(lastBranchPosition - bendPoints[bendPoints.Count - 1].tempRotateAround).normalized;

				List<BroccoTree.Branch> childrenBranches = branch.branches;
				for (int i = 0; i < childrenBranches.Count; i++) {
					RandomizeBendingBranch (childrenBranches[i]);
				}
			}
			*/
		}
		/// <summary>
		/// Gets the total number of bend points.
		/// </summary>
		/// <returns>The total number bend points.</returns>
		/// <param name="branch">Branch to receive the bend points.</param>
		/// <param name="minBranchLength">Minimum branch length.</param>
		/// <param name="maxBranchLength">Max branch length.</param>
		/// <param name="treeLevelCount">Tree level count.</param>
		private int GetTotalBendPoints (BroccoTree.Branch branch, float minBranchLength, float maxBranchLength, int treeLevelCount) {
			/*
			float lengthCurvePosition = branchBenderElement.lengthDistributionCurve.Evaluate (
				Mathf.InverseLerp (minBranchLength, maxBranchLength, branch.length));
			float levelCurvePosition = branchBenderElement.levelDistributionCurve.Evaluate (
				Mathf.InverseLerp (0, treeLevelCount, branch.GetHierarchyLevel()));
			return Mathf.Clamp (
				(int)Mathf.Lerp ((float)branchBenderElement.minBendPoints, 
					(float)branchBenderElement.maxBendPoints, 
					lengthCurvePosition * levelCurvePosition),
				branchBenderElement.minBendPoints, branchBenderElement.maxBendPoints);
				*/
			return 0;
		}
		/// <summary>
		/// Gets the bend point random spacing.
		/// </summary>
		/// <returns>The bend point random spacing.</returns>
		/// <param name="branch">Branch.</param>
		/// <param name="segmentSpacing">Segment spacing.</param>
		private float GetBendPointRandomSpacing (BroccoTree.Branch branch, float segmentSpacing) {
			/*
			return Random.Range(-segmentSpacing / 2f * branchBenderElement.randomSpacing * 0.9f,
				segmentSpacing / 2f * branchBenderElement.randomSpacing * 0.9f);
				*/
				return 0f;
		}
		/// <summary>
		/// Gets the random direction.
		/// </summary>
		/// <param name="directionalAngle">Directional angle.</param>
		/// <param name="angleUp">Angle up.</param>
		/// <param name="angleRight">Angle right.</param>
		/// <param name="angleForward">Angle forward.</param>
		/// <param name="stayUp">Stay up.</param>
		/// <param name="stayRight">Stay right.</param>
		/// <param name="stayForward">Stay forward.</param>
		private void GetRandomDirection(float directionalAngle, 
			out float angleUp, out float angleRight, out float angleForward,
			out bool stayUp, out bool stayRight, out bool stayForward) {
			angleUp = Random.Range (-directionalAngle * 0.33f, directionalAngle * 0.33f);
			angleRight = Random.Range (-directionalAngle * 0.33f, directionalAngle * 0.33f);
			angleForward = Random.Range (-directionalAngle * 0.33f, directionalAngle * 0.33f);
			stayUp = true;
			stayRight = true;
			stayForward = true;
		}
		#endregion
	}
}