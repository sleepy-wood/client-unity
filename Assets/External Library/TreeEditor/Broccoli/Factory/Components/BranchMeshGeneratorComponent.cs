using System.Collections.Generic;

using UnityEngine;

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.Model;
using Broccoli.Builder;
using Broccoli.Manager;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Branch mesh generator component.
	/// </summary>
	public class BranchMeshGeneratorComponent : TreeFactoryComponent {
		#region vars
		/// <summary>
		/// The mesh builder.
		/// </summary>
		public BranchMeshBuilder meshBuilder = null;
		/// <summary>
		/// The branch mesh generator element.
		/// </summary>
		BranchMeshGeneratorElement branchMeshGeneratorElement = null;
		/// <summary>
		/// BranchSkin instances representing this tree. There instances hold
		/// information about the structure and meshing for the tree.
		/// </summary>
		public List <BranchMeshBuilder.BranchSkin> branchSkins {
			get {
				if (meshBuilder != null) {
					return meshBuilder.branchSkins;
				} else {
					return new List<BranchMeshBuilder.BranchSkin> ();
				}
			}
		}
		/// <summary>
		/// A mesh generated per each branch skin.
		/// </summary>
		/// <typeparam name="int">Branch skin ind.</typeparam>
		/// <typeparam name="Mesh">Mesh instante.</typeparam>
		public Dictionary<int, Mesh> branchMeshes = new Dictionary<int, Mesh> ();
		/// <summary>
		/// Girth at hierarchy base of the tree.
		/// </summary>
		float girthAtHierarchyBase = 0f;
		/// <summary>
		/// Maximum length on the processed.
		/// </summary>
		float treeMaxLength = 0f;
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
			meshBuilder = BranchMeshBuilder.GetInstance ();
			LODDef lod = treeFactory.treeFactoryPreferences.GetLOD (processControl != null? processControl.lodIndex:-1);
			meshBuilder.minPolygonSides = lod.minPolygonSides;
			meshBuilder.maxPolygonSides = lod.maxPolygonSides;
			meshBuilder.useMeshCapAtBase = lod.useMeshCapAtBase;
			meshBuilder.averageNormalsLevelLimit = (branchMeshGeneratorElement.useAverageNormals?2:0);
			meshBuilder.globalScale = treeFactory.treeFactoryPreferences.factoryScale;
			/*
			meshBuilder.minPolygonSides = branchMeshGeneratorElement.minPolygonSides;
			meshBuilder.maxPolygonSides = branchMeshGeneratorElement.maxPolygonSides;
			meshBuilder.segmentAngle = branchMeshGeneratorElement.segmentAngle;
			meshBuilder.useHardNormals = branchMeshGeneratorElement.useHardNormals;
			meshBuilder.useMeshCapAtBase = branchMeshGeneratorElement.useMeshCapAtBase;
			meshBuilder.averageNormalsLevelLimit = (branchMeshGeneratorElement.useAverageNormals?2:0);
			meshBuilder.globalScale = treeFactory.treeFactoryPreferences.factoryScale;
			*/

			/*
			if (processControl.pass == 1 && !treeFactory.treeFactoryPreferences.prefabStrictLowPoly) {
				if (branchMeshGeneratorElement.maxPolygonSides > 6) {
					meshBuilder.maxPolygonSides = 6;
				} else {
					meshBuilder.maxPolygonSides = branchMeshGeneratorElement.maxPolygonSides;
				}
				if (branchMeshGeneratorElement.minPolygonSides > 3) {
					meshBuilder.minPolygonSides = 3;
				} else {
					meshBuilder.minPolygonSides = branchMeshGeneratorElement.minPolygonSides;
				}
			}
			*/
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.Mesh;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
			branchMeshGeneratorElement = null;
			meshBuilder = null;
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
			TreeFactoryProcessControl processControl = null) {
			if (pipelineElement != null && tree != null) {
				branchMeshGeneratorElement = pipelineElement as BranchMeshGeneratorElement;
				PrepareParams (treeFactory, useCache, useLocalCache, processControl);
				tree.RecalculateNormals ();
				BuildMesh (treeFactory, processControl.lodIndex);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Removes the product of this component on the factory processing.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		public override void Unprocess (TreeFactory treeFactory) {
			treeFactory.meshManager.DeregisterMesh (MeshManager.MeshData.Type.Branch);
		}
		/// <summary>
		/// Builds the mesh.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="lodIndex">LOD definition index.</param>
		private void BuildMesh (TreeFactory treeFactory, int lodIndex) {
			Mesh branchMesh = null;
			/*
			if (lodIndex == 1) {
				meshBuilder.branchAngleTolerance = Mathf.Lerp (33f, 5f, branchMeshGeneratorElement.minBranchCurveResolution);
			} else {
				meshBuilder.branchAngleTolerance = Mathf.Lerp (33f, 5f, branchMeshGeneratorElement.maxBranchCurveResolution);
			}
			*/
			LODDef lod = treeFactory.treeFactoryPreferences.GetLOD (lodIndex);
			meshBuilder.branchAngleTolerance = lod.branchAngleTolerance;

			meshBuilder.ClearReferenceBranchSkins ();
			meshBuilder.ClearMeshBuilders ();

			// TRUNK MESH GENERATOR.
			TrunkMeshGeneratorElement trunkMeshGeneratorElement =
				(TrunkMeshGeneratorElement) branchMeshGeneratorElement.GetDownstreamElement (PipelineElement.ClassType.TrunkMeshGenerator);
			if (trunkMeshGeneratorElement != null && trunkMeshGeneratorElement.isActive) {
				CreateTrunkMeshBuilder (trunkMeshGeneratorElement, treeFactory, lodIndex);
			}
			// SHAPE MESH GENERATOR.
			if (branchMeshGeneratorElement.meshMode == BranchMeshGeneratorElement.MeshMode.Shape && 
				branchMeshGeneratorElement.shapeCollection != null && 
				branchMeshGeneratorElement.shapeCollection.shapes.Count > 0)
			{
				CreateShapeMeshBuilder (treeFactory, lodIndex);
			}
			// WELDING BRANCH GENERATOR.
			if (branchMeshGeneratorElement.meshMode == BranchMeshGeneratorElement.MeshMode.Default &&
				(branchMeshGeneratorElement.useBranchWelding || branchMeshGeneratorElement.useRootWelding)) {
				CreateWeldingMeshBuilder (treeFactory, lodIndex);
			}
			
			branchMesh = meshBuilder.MeshTree (tree); // TODO: CUT

			treeFactory.meshManager.DeregisterMeshByType (MeshManager.MeshData.Type.Branch);

			treeFactory.meshManager.RegisterBranchMesh (branchMesh);
			treeFactory.meshManager.AddMeshPart (0, // TODO: see if we can remove this.
				branchMesh.vertices.Length, 
				tree.obj.transform.position, 
				MeshManager.MeshData.Type.Branch);
			
			/*
			branchMeshGeneratorElement.maxGirth = meshBuilder.treeMaxGirth;
			branchMeshGeneratorElement.minGirth = meshBuilder.treeMinGirth;
			if (pass == 1) {
				branchMeshGeneratorElement.verticesCountFirstPass = meshBuilder.verticesGenerated;
				branchMeshGeneratorElement.trianglesCountFirstPass = meshBuilder.trianglesGenerated;
			} else {
				branchMeshGeneratorElement.verticesCountSecondPass = meshBuilder.verticesGenerated;
				branchMeshGeneratorElement.trianglesCountSecondPass = meshBuilder.trianglesGenerated;
			}
			*/
			/*
			if (treeFactory.treeFactoryPreferences.prefabStrictLowPoly) {
				branchMeshGeneratorElement.showLODInfoLevel = 1;
			} else if (!treeFactory.treeFactoryPreferences.prefabUseLODGroups) {
				branchMeshGeneratorElement.showLODInfoLevel = 2;
			} else {
				branchMeshGeneratorElement.showLODInfoLevel = -1;
			}
			*/
		}
		/// <summary>
		/// Creates the BranchSkin instances for root branches assigning a trunk range to be meshed.
		/// </summary>
		/// <param name="trunkMeshGeneratorElement">Trunk mesh generator element containing the information to generate ranges.</param>
		/// <param name="treeFactory">Tree factory instance.</param>
		/// <param name="lodIndex">Processing pipeline LOD definition.</param>
		private void CreateTrunkMeshBuilder (TrunkMeshGeneratorElement trunkMeshGeneratorElement, TreeFactory treeFactory, int lodIndex) {
			// Register the trunk mesh builder.
			TrunkMeshBuilder trunkMeshBuilder = new TrunkMeshBuilder ();
			/*
			int passDiv = 1;
			if (lodIndex == 1) { passDiv = 2; }
			trunkMeshBuilder.minPolygonSides = trunkMeshGeneratorElement.minPolygonSides / passDiv;
			trunkMeshBuilder.maxPolygonSides = trunkMeshGeneratorElement.maxPolygonSides / passDiv;
			trunkMeshBuilder.lengthPosResolution = trunkMeshGeneratorElement.lengthPosResolution / lodIndex;
			*/
			LODDef lod = treeFactory.treeFactoryPreferences.GetLOD (lodIndex);
			trunkMeshBuilder.minPolygonSides = lod.minPolygonSides * 2;
			trunkMeshBuilder.maxPolygonSides = lod.maxPolygonSides * 2;
			//trunkMeshBuilder.lengthPosResolution = trunkMeshGeneratorElement.lengthPosResolution / lodIndex;
			meshBuilder.AddMeshBuilder (trunkMeshBuilder);

			// Get BranchSkin instance per root branch, set range and register its values on the TrunkMeshBuilder instance.
			for (int i = 0; i < tree.branches.Count; i++) {
				// Register BranchSkin instance.
				Model.BroccoTree.Branch currentBranch = tree.branches [i];
				BranchMeshBuilder.BranchSkin branchSkin = meshBuilder.GetOrCreateBranchSkin (currentBranch, 0, 0f, 0f);

				bool trunkHasRoots = true;
				if (trunkMeshGeneratorElement.rootMode == TrunkMeshGeneratorElement.RootMode.IntegrationOrPseudo) {
					trunkHasRoots = false;
					for (int j = 0; j < tree.branches[i].branches.Count; j++) {
						if (tree.branches[i].branches[j].isRoot) {
							trunkHasRoots = true;
							break;
						}
					}
				}

				// Set range.
				if (trunkMeshGeneratorElement.rootMode == TrunkMeshGeneratorElement.RootMode.Pseudo ||
					(!trunkHasRoots && trunkMeshGeneratorElement.rootMode == TrunkMeshGeneratorElement.RootMode.IntegrationOrPseudo)) {
					float range = Mathf.Lerp (
						trunkMeshGeneratorElement.minSpread, 
						trunkMeshGeneratorElement.maxSpread, 
						Random.Range (0f, 1f));
					BranchMeshBuilder.BranchSkinRange rangeAtBranchSkin = new BranchMeshBuilder.BranchSkinRange ();
					rangeAtBranchSkin.from = 0f;
					rangeAtBranchSkin.to = range;
					
					// Define length subdivisions.
					rangeAtBranchSkin.subdivisions = 
						(int)Mathf.Lerp (24f * trunkMeshGeneratorElement.lengthResolutionFactor * range, 
							12f * trunkMeshGeneratorElement.lengthResolutionFactor * range, Mathf.InverseLerp (5, 45, lod.branchAngleTolerance));
					rangeAtBranchSkin.radialResolutionFactor = trunkMeshGeneratorElement.radialResolutionFactor;
					rangeAtBranchSkin.builderType = BranchMeshBuilder.BuilderType.Trunk;

					// Define radial segments.
					float radialStep = 1f / (float)lod.maxPolygonSides * 1.2f / trunkMeshGeneratorElement.radialResolutionFactor;

					branchSkin.AddBuilderRange (rangeAtBranchSkin);

					// Register branch values on the trunk mesh builder.
					trunkMeshBuilder.RegisterPseudoTrunk (
						tree.branches [i],
						branchSkin,
						Random.Range (trunkMeshGeneratorElement.minDisplacementPoints, trunkMeshGeneratorElement.maxDisplacementPoints), 
						range, 
						trunkMeshGeneratorElement.minDisplacementScaleAtBase,
						trunkMeshGeneratorElement.maxDisplacementScaleAtBase,
						trunkMeshGeneratorElement.minDisplacementAngleVariance,
						trunkMeshGeneratorElement.maxDisplacementAngleVariance,
						Random.Range (trunkMeshGeneratorElement.minDisplacementTwirl, trunkMeshGeneratorElement.maxDisplacementTwirl),
						trunkMeshGeneratorElement.strength, 
						trunkMeshGeneratorElement.scaleCurve,
						radialStep);
				}
			}
		}
		/// <summary>
		/// Creates the BranchSkin instances for root branches assigning a shape range to be meshed.
		/// </summary>
		/// <param name="treeFactory">Tree factory instance.</param>
		/// <param name="pass">Processing pipeline pass.</param>
		private void CreateShapeMeshBuilder (TreeFactory treeFactory, int pass) {
			// Register the shape mesh builder.
			ShapeMeshBuilder shapeMeshBuilder = new ShapeMeshBuilder ();
			shapeMeshBuilder.shapeCollection = branchMeshGeneratorElement.shapeCollection;
			shapeMeshBuilder.shapeCollection.Process ();
			for (int i = 0; i < shapeMeshBuilder.shapeCollection.shapes.Count; i++) {
				shapeMeshBuilder.shapeCollection.shapes [i].Process ();
			}
			shapeMeshBuilder.shapeScaleMultiplier = branchMeshGeneratorElement.shapeScale;
			shapeMeshBuilder.adherenceToHierarchyScale = branchMeshGeneratorElement.branchHierarchyScaleAdherence;
			shapeMeshBuilder.girthAtHierarchyBase = tree.maxGirth;
			girthAtHierarchyBase = tree.maxGirth;
			treeMaxLength = tree.GetMaxLength ();
			meshBuilder.AddMeshBuilder (shapeMeshBuilder);

			for (int i = 0; i < tree.branches.Count; i++) {
				ProcesShapeContext (tree.branches [i], shapeMeshBuilder.shapeCollection);
			}
		}
		/// <summary>
		/// Creates a branch builder for welding branches and roots.
		/// </summary>
		/// <param name="treeFactory">Tree factory instance.</param>
		/// <param name="lodIndex">LOD definition index.</param>
		private void CreateWeldingMeshBuilder (TreeFactory treeFactory, int lodIndex) {
			// Register the welding mesh builder.
			WeldingMeshBuilder weldingMeshBuilder = new WeldingMeshBuilder ();

			LODDef lod = treeFactory.treeFactoryPreferences.GetLOD (lodIndex);

			// Properties for branches.
			weldingMeshBuilder.useBranchWelding = branchMeshGeneratorElement.useBranchWelding && lod.allowBranchWelding;
			weldingMeshBuilder.useBranchWeldingMeshCap = lod.useMeshCapAtBase && branchMeshGeneratorElement.useBranchWeldingMeshCap;
			weldingMeshBuilder.minBranchWeldingHierarchyRange = branchMeshGeneratorElement.minBranchWeldingHierarchyRange;
			weldingMeshBuilder.maxBranchWeldingHierarchyRange = branchMeshGeneratorElement.maxBranchWeldingHierarchyRange;
			weldingMeshBuilder.branchWeldingHierarchyRange = Random.Range (branchMeshGeneratorElement.minBranchWeldingHierarchyRange, 
				branchMeshGeneratorElement.minBranchWeldingHierarchyRange);
			weldingMeshBuilder.branchWeldingHierarchyRangeCurve = branchMeshGeneratorElement.branchWeldingHierarchyRangeCurve;
			weldingMeshBuilder.branchWeldingCurve = branchMeshGeneratorElement.branchWeldingCurve;
			weldingMeshBuilder.minBranchWeldingDistance = branchMeshGeneratorElement.minBranchWeldingDistance;
			weldingMeshBuilder.maxBranchWeldingDistance = branchMeshGeneratorElement.maxBranchWeldingDistance;
			weldingMeshBuilder.minAdditionalBranchWeldingSegments = branchMeshGeneratorElement.minAdditionalBranchWeldingSegments;
			weldingMeshBuilder.maxAdditionalBranchWeldingSegments = branchMeshGeneratorElement.maxAdditionalBranchWeldingSegments;
			weldingMeshBuilder.minBranchWeldingUpperSpread = branchMeshGeneratorElement.minBranchWeldingUpperSpread;
			weldingMeshBuilder.maxBranchWeldingUpperSpread = branchMeshGeneratorElement.maxBranchWeldingUpperSpread;
			weldingMeshBuilder.minBranchWeldingLowerSpread = branchMeshGeneratorElement.minBranchWeldingLowerSpread;
			weldingMeshBuilder.maxBranchWeldingLowerSpread = branchMeshGeneratorElement.maxBranchWeldingLowerSpread;

			// Properties for roots.
			weldingMeshBuilder.useRootWelding = branchMeshGeneratorElement.useRootWelding && lod.allowRootWelding;
			weldingMeshBuilder.useRootWeldingMeshCap = lod.useMeshCapAtBase && branchMeshGeneratorElement.useRootWeldingMeshCap;
			weldingMeshBuilder.minRootWeldingHierarchyRange = branchMeshGeneratorElement.minRootWeldingHierarchyRange;
			weldingMeshBuilder.maxRootWeldingHierarchyRange = branchMeshGeneratorElement.maxRootWeldingHierarchyRange;
			weldingMeshBuilder.rootWeldingHierarchyRange = Random.Range (branchMeshGeneratorElement.minRootWeldingHierarchyRange, 
				branchMeshGeneratorElement.minRootWeldingHierarchyRange);
			weldingMeshBuilder.rootWeldingHierarchyRangeCurve = branchMeshGeneratorElement.rootWeldingHierarchyRangeCurve;
			weldingMeshBuilder.rootWeldingCurve = branchMeshGeneratorElement.rootWeldingCurve;
			weldingMeshBuilder.minRootWeldingDistance = branchMeshGeneratorElement.minRootWeldingDistance;
			weldingMeshBuilder.maxRootWeldingDistance = branchMeshGeneratorElement.maxRootWeldingDistance;
			weldingMeshBuilder.minAdditionalRootWeldingSegments = branchMeshGeneratorElement.minAdditionalRootWeldingSegments;
			weldingMeshBuilder.maxAdditionalRootWeldingSegments = branchMeshGeneratorElement.maxAdditionalRootWeldingSegments;
			weldingMeshBuilder.minRootWeldingUpperSpread = branchMeshGeneratorElement.minRootWeldingUpperSpread;
			weldingMeshBuilder.maxRootWeldingUpperSpread = branchMeshGeneratorElement.maxRootWeldingUpperSpread;
			weldingMeshBuilder.minRootWeldingLowerSpread = branchMeshGeneratorElement.minRootWeldingLowerSpread;
			weldingMeshBuilder.maxRootWeldingLowerSpread = branchMeshGeneratorElement.maxRootWeldingLowerSpread;

			// Add the builder to the builders.
			meshBuilder.AddMeshBuilder (weldingMeshBuilder);
		}
		private void ProcesShapeContext (Broccoli.Model.BroccoTree.Branch branch, ShapeDescriptorCollection shapeCollection) {
			BranchMeshBuilder.BranchSkin branchSkin = meshBuilder.GetOrCreateBranchSkin (branch, 0, 0f, 0f);

			// Create subcontexts
			List<float> subcontexts = new List<float> ();
			if (branchMeshGeneratorElement.meshContext == BranchMeshGeneratorElement.MeshContext.BranchSequence) {
				// Create a whole range.
				subcontexts.Add (0f);
				subcontexts.Add (1f);
			} else {
				// Create ranges per branch.
				Broccoli.Model.BroccoTree.Branch refBranch = branch;
				float accumLength = 0f;
				do {
					subcontexts.Add (accumLength / branchSkin.length);
					accumLength += refBranch.length;
					subcontexts.Add (accumLength / branchSkin.length);
					refBranch = refBranch.followUp;
				} while (refBranch != null);
			}

			// If context is whole, add subcontexts as ranges, if not then create nodes ranges.
			if (branchMeshGeneratorElement.meshRange == BranchMeshGeneratorElement.MeshRange.Whole) {
				// Add subcontext as ranges for WHOLE range mode.
				for (int i = 0; i < subcontexts.Count; i = i + 2) {
					BranchMeshBuilder.BranchSkinRange rangeAtBranchSkin = new BranchMeshBuilder.BranchSkinRange ();
					rangeAtBranchSkin.from = subcontexts [i];
					rangeAtBranchSkin.to = subcontexts [i + 1];
					SetShapeRange (rangeAtBranchSkin, shapeCollection);
					SetShapeRangeCaps (branchSkin, branch, rangeAtBranchSkin, shapeCollection);
					branchSkin.AddBuilderRange (rangeAtBranchSkin);
				}
			} else {
				// Create nodes from subcontext to add them as ranges for NODE range mode.
				List<float> subcontextNodes = new List<float> ();
				if (branchMeshGeneratorElement.nodesMode == BranchMeshGeneratorElement.NodesMode.Number) {
					// Number of nodes are calculated from a range.
					for (int i = 0; i < subcontexts.Count; i = i + 2) {
						int numberOfNodes = Random.Range (branchMeshGeneratorElement.minNodes, branchMeshGeneratorElement.maxNodes + 1);
						float nodeLength = (subcontexts[i + 1] - subcontexts[i]) / numberOfNodes;
						// Get nodes sizes.
						List<float> nodeSizePos = new List<float> ();
						float accumNodeSize = 0f;
						for (int j = 1; j < numberOfNodes; j++) {
							float nodeLengthVariance = Random.Range (nodeLength * -0.6f, nodeLength * 0.6f) * branchMeshGeneratorElement.nodeLengthVariance;
							nodeSizePos.Add (nodeLength + nodeLengthVariance);
							accumNodeSize += nodeLength + nodeLengthVariance;
						}
						nodeSizePos.Add (nodeLength * numberOfNodes - accumNodeSize);
						// Reorder if needed.
						if (branchMeshGeneratorElement.nodesDistribution != BranchMeshGeneratorElement.NodesDistribution.Random) {
							nodeSizePos.Sort ();
							if (branchMeshGeneratorElement.nodesDistribution == BranchMeshGeneratorElement.NodesDistribution.biggerAtBottom) {
								nodeSizePos.Reverse ();
							}
						}
						// Add subcontexts
						subcontextNodes.Add (subcontexts[i]);
						accumNodeSize = 0f;
						for (int j = 0; j < nodeSizePos.Count; j++) {
							accumNodeSize += nodeSizePos [j];
							subcontextNodes.Add (subcontexts[i] + accumNodeSize);
							subcontextNodes.Add (subcontexts[i] + accumNodeSize);
						}
						subcontextNodes.Add (subcontexts[i] + (nodeLength * numberOfNodes));
						/*
						for (int j = 0; j < numberOfNodes; j++) {
							subcontextNodes.Add (subcontexts[i] + nodeLength * j);
							subcontextNodes.Add (subcontexts[i] + nodeLength * (j + 1));
						}
						*/
					}
				} else {
					// Number of nodes are calculated from relative length of the context.
				}
				// Add subcontext from nodes to the branch skin.
				for (int i = 0; i < subcontextNodes.Count; i = i + 2) {
					BranchMeshBuilder.BranchSkinRange rangeAtBranchSkin = new BranchMeshBuilder.BranchSkinRange ();
					rangeAtBranchSkin.from = subcontextNodes [i];
					rangeAtBranchSkin.to = subcontextNodes [i + 1];
					SetShapeRange (rangeAtBranchSkin, shapeCollection);
					SetShapeRangeCaps (branchSkin, branch, rangeAtBranchSkin, shapeCollection);
					branchSkin.AddBuilderRange (rangeAtBranchSkin);
				}
			}
			
			// Process context for non follow up children branches.
			do {
				for (int i = 0; i < branch.branches.Count; i++) {
					if (!branch.branches[i].IsFollowUp ()) {
						ProcesShapeContext (branch.branches [i], shapeCollection);
					}
				}
				branch = branch.followUp;
			} while (branch != null);
		}
		void SetShapeRange (BranchMeshBuilder.BranchSkinRange range, ShapeDescriptorCollection shapeCollection) {
			int shapeId = -1;
			// Unique
			if (range.from == 0 && range.to == 1f) {
				shapeId = shapeCollection.GetUniqueShapeId ();
			}
			if (range.to == 1 && shapeId == -1) {
				shapeId = shapeCollection.GetTerminalShapeId ();
			}
			if (range.from == 0 && shapeId == -1) {
				shapeId = shapeCollection.GetInitialShapeId ();
			}
			if (shapeId == -1) {
				shapeId = shapeCollection.GetMiddleShapeId ();
			}
			range.builderType = BranchMeshBuilder.BuilderType.Shape;
			range.shapeId = shapeId;
		}
		void SetShapeRangeCaps (BranchMeshBuilder.BranchSkin branchSkin, BroccoTree.Branch firstBranch, BranchMeshBuilder.BranchSkinRange range, ShapeDescriptorCollection shapeCollection) {
			ShapeDescriptor shape = shapeCollection.GetShape (range.shapeId);
			if (shape != null) {
				if (shape.hasTopCap) {
					float atTopCap = branchSkin.GetGirthAtPosition (range.to, firstBranch);
					atTopCap = Mathf.Lerp (girthAtHierarchyBase, atTopCap, branchMeshGeneratorElement.branchHierarchyScaleAdherence) * branchMeshGeneratorElement.shapeScale;
					atTopCap = 1f - (atTopCap / branchSkin.length * (range.to - range.from)) * (shape.maxTopCapPos - 1f);
					if (atTopCap < 0.5f) atTopCap = 0.5f;
					range.topCap = atTopCap;
				} else {
					range.topCap = 1f;
				}

				if (shape.hasBottomCap) {
					float atBottomCap = branchSkin.GetGirthAtPosition (range.from, firstBranch);
					atBottomCap = Mathf.Lerp (girthAtHierarchyBase, atBottomCap, branchMeshGeneratorElement.branchHierarchyScaleAdherence) * branchMeshGeneratorElement.shapeScale;
					atBottomCap = atBottomCap / branchSkin.length * (range.to - range.from) * -shape.minBottomCapPos;
					if (atBottomCap > 0.5f) atBottomCap = 0.5f;
					range.bottomCap = atBottomCap;
				} else {
					range.bottomCap = 0f;
				}
			}
		}
		void AddShapeRange (Broccoli.Model.BroccoTree.Branch branch) {
			BranchMeshBuilder.BranchSkin branchSkin = meshBuilder.GetOrCreateBranchSkin (branch, 0, 0f, 0f);
			BranchMeshBuilder.BranchSkinRange rangeAtBranchSkin = new BranchMeshBuilder.BranchSkinRange ();
			rangeAtBranchSkin.from = 0f;
			rangeAtBranchSkin.to = 0.5f;
			rangeAtBranchSkin.builderType = BranchMeshBuilder.BuilderType.Shape;
			branchSkin.AddBuilderRange (rangeAtBranchSkin);
			for (int i = 0; i < branch.branches.Count; i++) {
				if (!branch.branches[i].IsFollowUp ()) {
					AddShapeRange (branch.branches [i]);
				}
			}
		}

		#endregion

	}
}