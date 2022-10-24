using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.Rendering;

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.Model;
using Broccoli.Builder;
using Broccoli.Generator;
using Broccoli.Manager;

namespace Broccoli.Factory
{
    using Pipeline = Broccoli.Pipe.Pipeline;
    public class SproutSubfactory {
        #region Vars
        /// <summary>
        /// Internal TreeFactory instance to create branches. 
        /// It must be provided from a parent TreeFactory when initializing this subfactory.
        /// </summary>
        public TreeFactory treeFactory = null;
        /// <summary>
        /// Factory scale to override every pipeline loaded.
        /// All the exposed factory values will be multiplied by this scale and displayed in meters.
        /// The generated mesh will have scaled vertex positions.
        /// </summary>
        public float factoryScale = 0.1f;
        /// <summary>
        /// Polygon area builder.
        /// </summary>
        public PolygonAreaBuilder polygonBuilder = new PolygonAreaBuilder ();
        /// <summary>
        /// Sprout composite manager.
        /// </summary>
        public SproutCompositeManager sproutCompositeManager = new SproutCompositeManager ();
        /// <summary>
        /// Simplyfies the convex hull on the branch segments.
        /// </summary>
        public bool simplifyHullEnabled = true;
        /// <summary>
        /// Branch descriptor collection to handle values.
        /// </summary>
        BranchDescriptorCollection branchDescriptorCollection = null;
        /// <summary>
        /// Selected branch descriptor index.
        /// </summary>
        public int branchDescriptorIndex = 0;
        /// <summary>
        /// Selected variation descriptor index.
        /// </summary>
        public int variationDescriptorIndex = 0;
        /// <summary>
        /// Saves the branch structure levels on the loaded pipeline.
        /// </summary>
        List<StructureGenerator.StructureLevel> branchLevels = new List<StructureGenerator.StructureLevel> ();
        /// <summary>
        /// Saves the sprout A structure levels on the loaded pipeline.
        /// </summary>
        List<StructureGenerator.StructureLevel> sproutALevels = new List<StructureGenerator.StructureLevel> ();
        /// <summary>
        /// Saves the sprout B structure levels on the loaded pipeline.
        /// </summary>
        List<StructureGenerator.StructureLevel> sproutBLevels = new List<StructureGenerator.StructureLevel> ();
        /// <summary>
        /// Saves the sprout mesh instances representing sprout groups.
        /// </summary>
        List<SproutMesh> sproutMeshes = new List<SproutMesh> ();
        /// <summary>
        /// Branch mapper element to set branch textures.
        /// </summary>
        BranchMapperElement branchMapperElement = null;
        /// <summary>
        /// Branch girth element to set branch girth.
        /// </summary>
        GirthTransformElement girthTransformElement = null;
        /// <summary>
        /// Sprout mapper element to set sprout textures.
        /// </summary>
        SproutMapperElement sproutMapperElement = null;
        /// <summary>
        /// Branch bender element to set branch noise.
        /// </summary>
        BranchBenderElement branchBenderElement = null;
        /// <summary>
        /// Number of branch levels available on the pipeline.
        /// </summary>
        /// <value>Count of branch levels.</value>
        public int branchLevelCount { get; private set; }
        /// <summary>
        /// Number of sprout levels available on the pipeline.
        /// </summary>
        /// <value>Count of sprout levels.</value>
        public int sproutLevelCount { get; private set; }
        /// <summary>
        /// Enum describing the possible materials to apply to a preview.
        /// </summary>
        public enum MaterialMode {
            Composite,
            Albedo,
            Normals,
            Extras,
            Subsurface,
            Mask,
            Thickness
        }
        public Broccoli.Model.BroccoTree snapshotTree = null;
        public Mesh snapshotTreeMesh = null;
        public static Dictionary<int, ISproutProcessor> _sproutProcessors = 
            new Dictionary<int, ISproutProcessor> ();
        #endregion

        #region Texture Vars
        TextureManager textureManager;
        #endregion

        #region Constructors
        /// <summary>
		/// Static constructor. Registers processors for this factory.
		/// </summary>
		static SproutSubfactory () {
			_sproutProcessors.Clear ();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (Type type in assembly.GetTypes()) {
                    SproutProcessorAttribute processorAttribute = type.GetCustomAttribute<SproutProcessorAttribute> ();
					if (processorAttribute != null) {
						ISproutProcessor instance = (ISproutProcessor)Activator.CreateInstance (type);
                        if (!_sproutProcessors.ContainsKey (processorAttribute.id)) {
						    _sproutProcessors.Add (processorAttribute.id, instance);
                        }
					}
				}
			}
		}
        #endregion

        #region Factory Initialization and Termination
        /// <summary>
        /// Initializes the subfactory instance.
        /// </summary>
        /// <param name="treeFactory">TreeFactory instance to use to produce branches.</param>
        public void Init (TreeFactory treeFactory) {
            this.treeFactory = treeFactory;
            if (textureManager != null) {
                textureManager.Clear ();
            }
            textureManager = new TextureManager ();
        }
        /// <summary>
        /// Check if there is a valid tree factory assigned to this sprout factory.
        /// </summary>
        /// <returns>True is there is a valid TreeFactory instance.</returns>
        public bool HasValidTreeFactory () {
            return treeFactory != null;
        }
        /// <summary>
        /// Clears data from this instance.
        /// </summary>
        public void Clear () {
            treeFactory = null;
            branchLevels.Clear ();
            sproutALevels.Clear ();
            sproutBLevels.Clear ();
            sproutMeshes.Clear ();
            branchMapperElement = null;
            girthTransformElement = null;
            sproutMapperElement = null;
            branchBenderElement = null;
            textureManager.Clear ();
            snapshotTree= null;
            snapshotTreeMesh = null;
        }
        #endregion

        #region Pipeline Load and Analysis
        /// <summary>
        /// Loads a Broccoli pipeline to process branches.
        /// The branch is required to have from 1 to 3 hierarchy levels of branch nodes.
        /// </summary>
        /// <param name="pipeline">Pipeline to load on this subfactory.</param>
        /// <param name="pathToAsset">Path to the asset.</param>
        public void LoadPipeline (Pipeline pipeline, BranchDescriptorCollection branchDescriptorCollection, string pathToAsset) {
            if (treeFactory != null) {
                treeFactory.UnloadAndClearPipeline ();
                treeFactory.LoadPipeline (pipeline.Clone (), pathToAsset, true , true);
                AnalyzePipeline ();
                this.branchDescriptorCollection = branchDescriptorCollection;
                ProcessTextures ();
            }
        }
        /// <summary>
        /// Analyzes the loaded pipeline to index the branch and sprout levels to modify using the
        /// BranchDescriptor instance values.
        /// </summary>
        void AnalyzePipeline () {
            branchLevelCount = 0;
            sproutLevelCount = 0;
            branchLevels.Clear ();
            sproutALevels.Clear ();
            sproutBLevels.Clear ();
            sproutMeshes.Clear ();
            // t structures for branches and sprouts.
            StructureGeneratorElement structureGeneratorElement = 
                (StructureGeneratorElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.StructureGenerator);
            AnalyzePipelineStructure (structureGeneratorElement.rootStructureLevel);
            // Get sprout meshes.
            SproutMeshGeneratorElement sproutMeshGeneratorElement = 
                (SproutMeshGeneratorElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.SproutMeshGenerator);
            if (sproutMeshGeneratorElement != null) {
                for (int i = 0; i < sproutMeshGeneratorElement.sproutMeshes.Count; i++) {
                    sproutMeshes.Add (sproutMeshGeneratorElement.sproutMeshes [i]);
                }
            }
            // Get the branch mapper to set textures for branches.
            branchMapperElement = 
                (BranchMapperElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.BranchMapper);
            girthTransformElement = 
                (GirthTransformElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.GirthTransform);
            sproutMapperElement = 
                (SproutMapperElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.SproutMapper);
            branchBenderElement = 
                (BranchBenderElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.BranchBender);
            branchBenderElement.onDirectionalBending -= OnDirectionalBending;
            branchBenderElement.onDirectionalBending += OnDirectionalBending;
        }
        void OnDirectionalBending (BroccoTree tree, BranchBenderElement branchBenderElement) {
            BranchDescriptor branchDescriptor = branchDescriptorCollection.branchDescriptors [branchDescriptorIndex];
            BranchDescriptor.BranchLevelDescriptor branchLevelDesc;
            int branchLevel;
            List<BroccoTree.Branch> allBranches = tree.GetDescendantBranches ();
            for (int i = 0; i < allBranches.Count; i++) {
                branchLevel = allBranches [i].GetLevel();
                if (branchLevel >= 1) {
                    branchLevelDesc = branchDescriptor.branchLevelDescriptors [branchLevel];
                    Vector3 dir = allBranches [i].GetDirectionAtPosition (0f);
                    dir.x = UnityEngine.Random.Range (branchLevelDesc.minPlaneAlignAtBase, branchLevelDesc.maxPlaneAlignAtBase);
					allBranches [i].ResetDirection (dir, true);
                }
            }
        }
        void AnalyzePipelineStructure (StructureGenerator.StructureLevel structureLevel) {
            if (!structureLevel.isSprout) {
                // Add branch structure level.
                branchLevels.Add (structureLevel);
                branchLevelCount++;
                // Add sprout A structure level.
                StructureGenerator.StructureLevel sproutStructureLevel = 
                    structureLevel.GetFirstSproutStructureLevel ();
                if (sproutStructureLevel != null) {
                    sproutALevels.Add (sproutStructureLevel);
                    sproutLevelCount++;
                }
                // Add sprout B structure level.
                sproutStructureLevel = structureLevel.GetSproutStructureLevel (1);
                if (sproutStructureLevel != null) {
                    sproutBLevels.Add (sproutStructureLevel);
                }
                // Send the next banch structure level to analysis if found.
                StructureGenerator.StructureLevel branchStructureLevel = 
                    structureLevel.GetFirstBranchStructureLevel ();
                if (branchStructureLevel != null) {
                    AnalyzePipelineStructure (branchStructureLevel);                    
                }
            }
        }
        public void UnloadPipeline () {
            if (treeFactory != null) {
                treeFactory.UnloadAndClearPipeline ();
            }
        }
        #endregion

        #region Pipeline Reflection
        public void BranchDescriptorCollectionToPipeline () {
            if (branchDescriptorIndex < 0) return;

            BranchDescriptor.BranchLevelDescriptor branchLD;
            StructureGenerator.StructureLevel branchSL;
            BranchDescriptor.SproutLevelDescriptor sproutALD;
            StructureGenerator.StructureLevel sproutASL;
            BranchDescriptor.SproutLevelDescriptor sproutBLD;
            StructureGenerator.StructureLevel sproutBSL;

            BranchDescriptor branchDescriptor = branchDescriptorCollection.branchDescriptors [branchDescriptorIndex];

            // Set seed.
            treeFactory.localPipeline.seed = branchDescriptor.seed;

            // Set Factory Scale to 1/10.
            treeFactory.treeFactoryPreferences.factoryScale = factoryScale;

            // Update branch girth.
            if (girthTransformElement != null) {
                girthTransformElement.minGirthAtBase = branchDescriptor.girthAtBase;
                girthTransformElement.maxGirthAtBase = branchDescriptor.girthAtBase;
                girthTransformElement.minGirthAtTop = branchDescriptor.girthAtTop;
                girthTransformElement.maxGirthAtTop = branchDescriptor.girthAtTop;
            }
            // Update branch noise.
            if (branchBenderElement) {
                branchBenderElement.noiseAtBase = branchDescriptor.noiseAtBase;
                branchBenderElement.noiseAtTop = branchDescriptor.noiseAtTop;
                branchBenderElement.noiseScaleAtBase = branchDescriptor.noiseScaleAtBase;
                branchBenderElement.noiseScaleAtTop = branchDescriptor.noiseScaleAtTop;
            }
            // Update branch descriptor active levels.
            for (int i = 0; i < branchLevels.Count; i++) {
                if (i <= branchDescriptor.activeLevels) {
                    branchLevels [i].enabled = true;
                } else {
                    branchLevels [i].enabled = false;
                }
            }
            // Update branch level descriptors.
            for (int i = 0; i < branchDescriptor.branchLevelDescriptors.Count; i++) {
                if (i < branchLevelCount) {
                    branchLD = branchDescriptor.branchLevelDescriptors [i];
                    branchSL = branchLevels [i];
                    // Pass Values.
                    branchSL.minFrequency = branchLD.minFrequency;
                    branchSL.maxFrequency = branchLD.maxFrequency;
                    branchSL.radius = branchLD.radius;
                    branchSL.minLengthAtBase = branchLD.minLengthAtBase;
                    branchSL.maxLengthAtBase = branchLD.maxLengthAtBase;
                    branchSL.minLengthAtTop = branchLD.minLengthAtTop;
                    branchSL.maxLengthAtTop = branchLD.maxLengthAtTop;
                    branchSL.minParallelAlignAtBase = branchLD.minParallelAlignAtBase;
                    branchSL.maxParallelAlignAtBase = branchLD.maxParallelAlignAtBase;
                    branchSL.minParallelAlignAtTop = branchLD.minParallelAlignAtTop;
                    branchSL.maxParallelAlignAtTop = branchLD.maxParallelAlignAtTop;
                    branchSL.minGravityAlignAtBase = branchLD.minGravityAlignAtBase;
                    branchSL.maxGravityAlignAtBase = branchLD.maxGravityAlignAtBase;
                    branchSL.minGravityAlignAtTop = branchLD.minGravityAlignAtTop;
                    branchSL.maxGravityAlignAtTop = branchLD.maxGravityAlignAtTop;
                }
            }
            // Update branch mapping textures.
            if (branchMapperElement != null) {
                branchMapperElement.mainTexture = branchDescriptorCollection.branchAlbedoTexture;
                branchMapperElement.normalTexture = branchDescriptorCollection.branchNormalTexture;
                branchMapperElement.mappingYDisplacement = branchDescriptorCollection.branchTextureYDisplacement;
            }
            // Update sprout A level descriptors.
            for (int i = 0; i < branchDescriptor.sproutALevelDescriptors.Count; i++) {
                if (i < branchLevelCount) {
                    sproutALD = branchDescriptor.sproutALevelDescriptors [i];
                    sproutASL = sproutALevels [i];
                    // Pass Values.
                    sproutASL.enabled = sproutALD.isEnabled;
                    sproutASL.minFrequency = sproutALD.minFrequency;
                    sproutASL.maxFrequency = sproutALD.maxFrequency;
                    sproutASL.minParallelAlignAtBase = sproutALD.minParallelAlignAtBase;
                    sproutASL.maxParallelAlignAtBase = sproutALD.maxParallelAlignAtBase;
                    sproutASL.minParallelAlignAtTop = sproutALD.minParallelAlignAtTop;
                    sproutASL.maxParallelAlignAtTop = sproutALD.maxParallelAlignAtTop;
                    sproutASL.minGravityAlignAtBase = sproutALD.minGravityAlignAtBase;
                    sproutASL.maxGravityAlignAtBase = sproutALD.maxGravityAlignAtBase;
                    sproutASL.minGravityAlignAtTop = sproutALD.minGravityAlignAtTop;
                    sproutASL.maxGravityAlignAtTop = sproutALD.maxGravityAlignAtTop;
                    sproutASL.flipSproutAlign = branchDescriptor.sproutAFlipAlign;
                    sproutASL.actionRangeEnabled = true;
                    sproutASL.minRange = sproutALD.minRange;
                    sproutASL.maxRange = sproutALD.maxRange;
                }
            }
            // Update sprout A properties.
            if (sproutMeshes.Count > 0) {
                sproutMeshes [0].width = branchDescriptor.sproutASize;
                sproutMeshes [0].scaleAtBase = branchDescriptor.sproutAScaleAtBase;
                sproutMeshes [0].scaleAtTop = branchDescriptor.sproutAScaleAtTop;
            }
            // Update sprout mapping textures.
            if (sproutMapperElement != null) {
                sproutMapperElement.sproutMaps [0].colorVarianceMode = SproutMap.ColorVarianceMode.Shades;
                sproutMapperElement.sproutMaps [0].minColorShade = branchDescriptorCollection.sproutStyleA.minColorShade;
                sproutMapperElement.sproutMaps [0].maxColorShade = branchDescriptorCollection.sproutStyleA.maxColorShade;
                sproutMapperElement.sproutMaps [0].colorTintEnabled = true;
                sproutMapperElement.sproutMaps [0].colorTint = branchDescriptorCollection.sproutStyleA.colorTint;
                sproutMapperElement.sproutMaps [0].minColorTint = branchDescriptorCollection.sproutStyleA.minColorTint;
                sproutMapperElement.sproutMaps [0].maxColorTint = branchDescriptorCollection.sproutStyleA.maxColorTint;
                sproutMapperElement.sproutMaps [0].metallic = branchDescriptorCollection.sproutStyleA.metallic;
                sproutMapperElement.sproutMaps [0].glossiness = branchDescriptorCollection.sproutStyleA.glossiness;
                sproutMapperElement.sproutMaps [0].subsurfaceValue = 0.5f + Mathf.Lerp (-0.4f, 0.4f, branchDescriptorCollection.sproutStyleA.subsurfaceMul - 0.5f);
                sproutMapperElement.sproutMaps [0].sproutAreas.Clear ();
                for (int i = 0; i < branchDescriptorCollection.sproutAMapAreas.Count; i++) {
                    SproutMap.SproutMapArea sma = branchDescriptorCollection.sproutAMapAreas [i].Clone ();
                    sma.texture = GetSproutTexture (0, i);
                    sproutMapperElement.sproutMaps [0].sproutAreas.Add (sma);
                }
            }
            // Update sprout B level descriptors.
            for (int i = 0; i < branchDescriptor.sproutBLevelDescriptors.Count; i++) {
                if (i < branchLevelCount) {
                    sproutBLD = branchDescriptor.sproutBLevelDescriptors [i];
                    sproutBSL = sproutBLevels [i];
                    // Pass Values.
                    sproutBSL.enabled = sproutBLD.isEnabled;
                    sproutBSL.minFrequency = sproutBLD.minFrequency;
                    sproutBSL.maxFrequency = sproutBLD.maxFrequency;
                    sproutBSL.minParallelAlignAtBase = sproutBLD.minParallelAlignAtBase;
                    sproutBSL.maxParallelAlignAtBase = sproutBLD.maxParallelAlignAtBase;
                    sproutBSL.minParallelAlignAtTop = sproutBLD.minParallelAlignAtTop;
                    sproutBSL.maxParallelAlignAtTop = sproutBLD.maxParallelAlignAtTop;
                    sproutBSL.minGravityAlignAtBase = sproutBLD.minGravityAlignAtBase;
                    sproutBSL.maxGravityAlignAtBase = sproutBLD.maxGravityAlignAtBase;
                    sproutBSL.minGravityAlignAtTop = sproutBLD.minGravityAlignAtTop;
                    sproutBSL.maxGravityAlignAtTop = sproutBLD.maxGravityAlignAtTop;
                    sproutBSL.flipSproutAlign = branchDescriptor.sproutBFlipAlign;
                    sproutBSL.actionRangeEnabled = true;
                    sproutBSL.minRange = sproutBLD.minRange;
                    sproutBSL.maxRange = sproutBLD.maxRange;
                }
            }
            // Update sprout A properties.
            if (sproutMeshes.Count > 1) {
                sproutMeshes [1].width = branchDescriptor.sproutBSize;
                sproutMeshes [1].scaleAtBase = branchDescriptor.sproutBScaleAtBase;
                sproutMeshes [1].scaleAtTop = branchDescriptor.sproutBScaleAtTop;
            }
            // Update sprout mapping textures.
            if (sproutMapperElement != null && sproutMapperElement.sproutMaps.Count > 1) {
                sproutMapperElement.sproutMaps [1].colorVarianceMode =  SproutMap.ColorVarianceMode.Shades;
                sproutMapperElement.sproutMaps [1].minColorShade = branchDescriptorCollection.sproutStyleB.minColorShade;
                sproutMapperElement.sproutMaps [1].maxColorShade = branchDescriptorCollection.sproutStyleB.maxColorShade;
                sproutMapperElement.sproutMaps [1].colorTintEnabled = true;
                sproutMapperElement.sproutMaps [1].colorTint = branchDescriptorCollection.sproutStyleB.colorTint;
                sproutMapperElement.sproutMaps [1].minColorTint = branchDescriptorCollection.sproutStyleB.minColorTint;
                sproutMapperElement.sproutMaps [1].maxColorTint = branchDescriptorCollection.sproutStyleB.maxColorTint;
                sproutMapperElement.sproutMaps [1].metallic = branchDescriptorCollection.sproutStyleB.metallic;
                sproutMapperElement.sproutMaps [1].glossiness = branchDescriptorCollection.sproutStyleB.glossiness;
                sproutMapperElement.sproutMaps [1].subsurfaceValue = 0.5f + Mathf.Lerp (-0.4f, 0.4f, branchDescriptorCollection.sproutStyleB.subsurfaceMul - 0.5f);
                sproutMapperElement.sproutMaps [1].sproutAreas.Clear ();
                for (int i = 0; i < branchDescriptorCollection.sproutBMapAreas.Count; i++) {
                    SproutMap.SproutMapArea sma = branchDescriptorCollection.sproutBMapAreas [i].Clone ();
                    sma.texture = GetSproutTexture (1, i);
                    sproutMapperElement.sproutMaps [1].sproutAreas.Add (sma);
                }
            }
        }
        #endregion

        #region Snapshot Processing
        /// <summary>
        /// Gets a sprout processor given and id.
        /// </summary>
        /// <param name="processorId">Sprout processor id.</param>
        /// <returns>Processor id.</returns>
        public ISproutProcessor GetSproutProcessor (int processorId) {
            if (_sproutProcessors.ContainsKey (processorId)) {
                return _sproutProcessors [processorId];
            }
            return null;
        }
        /// <summary>
        /// Regenerates a preview for the loaded snapshot.
        /// </summary>
        /// <param name="materialMode">Materials mode to apply.</param>
        /// <param name="isNewSeed"><c>True</c> to create a new preview (new seed).</param>
        public void ProcessSnapshot (bool clearCompositeManager = true, MaterialMode materialMode = MaterialMode.Composite, bool isNewSeed = false) {
            if (branchDescriptorIndex < 0) return;

            if (branchDescriptorCollection == null) return;
            BranchDescriptor branchDescriptor = branchDescriptorCollection.branchDescriptors [branchDescriptorIndex];
            if (!isNewSeed) {
                treeFactory.localPipeline.seed = branchDescriptor.seed;
                treeFactory.ProcessPipelinePreview (null, true, true);
            } else {
                treeFactory.ProcessPipelinePreview ();
                branchDescriptor.seed = treeFactory.localPipeline.seed;
            }
            if (GlobalSettings.showSproutLabTreeFactoryInHierarchy) {
                treeFactory.previewTree.obj.SetActive (true);
            } else {
                treeFactory.previewTree.obj.SetActive (false);
            }

            // Clear polygon areas.
            // Clear existing polygons for the descriptor.
            branchDescriptor.polygonAreas.Clear ();
            if (clearCompositeManager)
                sproutCompositeManager.Clear ();

            // Get materials.
            MeshRenderer meshRenderer = treeFactory.previewTree.obj.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = treeFactory.previewTree.obj.GetComponent<MeshFilter> ();
            Material[] compositeMaterials = meshRenderer.sharedMaterials;
            if (materialMode == MaterialMode.Albedo) { // Albedo
                meshRenderer.sharedMaterials = GetAlbedoMaterials (compositeMaterials,
                    branchDescriptorCollection.sproutStyleA.colorTint,
                    branchDescriptorCollection.sproutStyleB.colorTint,
                    branchDescriptorCollection.sproutStyleA.colorSaturation,
                    branchDescriptorCollection.sproutStyleB.colorSaturation,
                    branchDescriptorCollection.branchColorShade,
                    branchDescriptorCollection.branchColorSaturation,
                    SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
                    SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
            } else if (materialMode == MaterialMode.Normals) { // Normals
                meshRenderer.sharedMaterials = GetNormalMaterials (compositeMaterials);
            } else if (materialMode == MaterialMode.Extras) { // Extras
                meshRenderer.sharedMaterials = GetExtraMaterials (compositeMaterials,
                    branchDescriptorCollection.sproutStyleA.metallic, 
                    branchDescriptorCollection.sproutStyleA.glossiness,
                    branchDescriptorCollection.sproutStyleB.metallic, 
                    branchDescriptorCollection.sproutStyleB.glossiness,
                    GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
            } else if (materialMode == MaterialMode.Subsurface) { // Subsurface
                meshRenderer.sharedMaterials = GetSubsurfaceMaterials (compositeMaterials,
                    branchDescriptorCollection.sproutStyleA.colorTint, 
                    branchDescriptorCollection.sproutStyleB.colorTint,
                    branchDescriptorCollection.branchColorSaturation,
                    branchDescriptorCollection.sproutStyleA.colorSaturation,
                    branchDescriptorCollection.sproutStyleB.colorSaturation,
                    branchDescriptorCollection.sproutStyleA.subsurfaceMul,
					branchDescriptorCollection.sproutStyleB.subsurfaceMul,
                    GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
            } else if (materialMode == MaterialMode.Composite) { // Composite
                meshRenderer.sharedMaterials = GetCompositeMaterials (compositeMaterials,
                    branchDescriptorCollection.sproutStyleA.colorTint, 
                    branchDescriptorCollection.sproutStyleB.colorTint,
                    GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
            }

            snapshotTree = treeFactory.previewTree;
            snapshotTreeMesh = meshFilter.sharedMesh;
        }
        /// <summary>
        /// Creates the polygons for a branch descriptor. It saves their textures to the
        /// snapshotTextures buffer.
        /// It should be called with after ProcessSnapshot to have the mesh, materials and tree
        /// corresponding to the last snapshot processed.
        /// </summary>
        /// <param name="branchDescriptor"></param>
        public void ProcessSnapshotPolygons (BranchDescriptor branchDescriptor) {
            if (sproutCompositeManager.HasSnapshot (branchDescriptor.id)) return;
            //Debug.Log ("Zz Process LOD polygons.");

            // Generate polygon areas for each LOD.
            for (int lodLevel = 0; lodLevel < branchDescriptor.lodCount; lodLevel++) {
                GenerateSnapshotPolygonsPerLOD (lodLevel, branchDescriptor);
            }

            /*
            // TEMP: get unique texture and materials.
            PolygonArea polygonArea = SproutCompositeManager.Current ().GetPolygonArea (branchDescriptor.id, 0, 0);
            
            // Generate texture for this single snapshot.
            // Clear existing albedo textures.
            for (int i = 0; i < snapshotAlbedoTextures.Count; i++) {
                Object.DestroyImmediate (snapshotAlbedoTextures [i]);
            }
            // Clear existing normals textures.
            for (int i = 0; i < snapshotNormalsTextures.Count; i++) {
                Object.DestroyImmediate (snapshotNormalsTextures [i]);
            }
            // Clear existing extras textures.
            for (int i = 0; i < snapshotExtrasTextures.Count; i++) {
                Object.DestroyImmediate (snapshotExtrasTextures [i]);
            }
            // Clear existing subsurface textures.
            for (int i = 0; i < snapshotSubsurfaceTextures.Count; i++) {
                Object.DestroyImmediate (snapshotSubsurfaceTextures [i]);
            }
            snapshotAlbedoTextures.Clear ();
            snapshotNormalsTextures.Clear ();
            snapshotExtrasTextures.Clear ();
            snapshotSubsurfaceTextures.Clear ();
            Texture2D albedoTex = null;
            Texture2D normalsTex = null;
            Texture2D extrasTex = null;
            Texture2D subsurfaceTex = null;
            GeneratePolygonTexture (snapshotMesh, polygonArea.aabb, snapshotMaterials, 
                MaterialMode.Albedo, 512, 512, out albedoTex);
            GeneratePolygonTexture (snapshotMesh, polygonArea.aabb, snapshotMaterials, 
                MaterialMode.Normals, 512, 512, out normalsTex);
            GeneratePolygonTexture (snapshotMesh, polygonArea.aabb, snapshotMaterials, 
                MaterialMode.Extras, 512, 512, out extrasTex);
            GeneratePolygonTexture (snapshotMesh, polygonArea.aabb, snapshotMaterials, 
                MaterialMode.Subsurface, 512, 512, out subsurfaceTex);
            snapshotAlbedoTextures.Add (albedoTex);
            snapshotNormalsTextures.Add (normalsTex);
            snapshotExtrasTextures.Add (extrasTex);
            snapshotSubsurfaceTextures.Add (subsurfaceTex);

            // Create materials.
            snapshotPolygonMaterials = new Material [1];
            Material m = MaterialManager.GetLeavesMaterial ();
            MaterialManager.SetLeavesMaterialProperties (
                m, Color.white, 0.6f, 0.1f, 0.1f, 0.5f, Color.white, 
                snapshotAlbedoTextures [0], snapshotNormalsTextures [0],
                snapshotExtrasTextures [0], snapshotSubsurfaceTextures [0], null);
            snapshotPolygonMaterials [0] = m;
            */
        }
        /// <summary>
        /// Generates and registers polygon areas for a snapshot at a specific LOD.
        /// </summary>
        /// <param name="lodLevel">Level of detail.</param>
        /// <param name="branchDescriptor">Branch descriptor of the snapshot.</param>
        public void GenerateSnapshotPolygonsPerLOD (int lodLevel, BranchDescriptor branchDescriptor) {
            // Get fragmentation setting for this LOD.
            int treeMaxLevel = snapshotTree.GetOffspringLevel ();
            ISproutProcessor processor = GetSproutProcessor (branchDescriptor.processorId);
            if (processor == null) {
                Debug.Log ("No Sprout Processor found with id " + branchDescriptor + ", skipping processing.");
            }
            int fragLevels, minFragLevel, maxFragLevel;
            SproutProcessor.FragmentationBias fragmentBias = processor.GetFragmentation (treeMaxLevel, lodLevel,
                out fragLevels, out minFragLevel, out maxFragLevel);

            // Begin building the snapshot polygons.
            polygonBuilder.BeginUsage (snapshotTree, snapshotTreeMesh, factoryScale);
            sproutCompositeManager.BeginUsage (snapshotTree, factoryScale);
            polygonBuilder.simplifyHullEnabled = simplifyHullEnabled;

            // Define the plane alignment.
            if (fragmentBias == SproutProcessor.FragmentationBias.PlaneAlignment) {
                BranchDescriptor.BranchLevelDescriptor branchLevelDesc;
                branchLevelDesc = branchDescriptor.branchLevelDescriptors [1];
                polygonBuilder.SetFragmentsDirectionalBias (
                    branchLevelDesc.minPlaneAlignAtBase, branchLevelDesc.maxPlaneAlignAtBase);
            } else {
                polygonBuilder.SetNoFragmentBias ();
            }

            // Define fragments.
            List<PolygonAreaBuilder.Fragment> fragments = 
                polygonBuilder.GenerateSnapshotFragments (lodLevel, branchDescriptor);
            
            // 
            PolygonAreaBuilder.Fragment fragment;
            Transform parentTransform = treeFactory.previewTree.obj.transform.parent;
            treeFactory.previewTree.obj.transform.parent = null;
            for (int fragIndex = 0; fragIndex < fragments.Count; fragIndex++) {
                // Create polygon per fragment.
                PolygonArea polygonArea = new PolygonArea (branchDescriptor.id, fragIndex, lodLevel);
                fragment = fragments [fragIndex];
                polygonArea.includes.AddRange (fragment.includes);
                polygonArea.excludes.AddRange (fragment.excludes);
                polygonArea.includedBranchIds.AddRange (fragment.includeIds);
                polygonArea.excludedBranchIds.AddRange (fragment.excludeIds);
                Hash128 _hash = Hash128.Compute (fragment.IncludesExcludesToString (branchDescriptor.id));
                polygonArea.hash = _hash;

                // Get fragment convex hull and bounds.
                polygonBuilder.ProcessPolygonAreaBounds (polygonArea, fragment);

                // Additional points for the fragment.
                //polygonBuilder.ProcessPolygonDetailPoints (polygonArea, fragment);

                // Set the triangles and build the mesh.
                polygonBuilder.ProcessPolygonAreaMesh (polygonArea);

                // Adds the unique polygon to the branch descriptor.
                branchDescriptor.polygonAreas.Add (polygonArea);

                // Add polygon area to the SproutCompositeManager.
                sproutCompositeManager.ManagePolygonArea (polygonArea, branchDescriptor);

                // Generate Textures and materials.

                sproutCompositeManager.GenerateTextures (polygonArea, branchDescriptor, this);
                sproutCompositeManager.GenerateMaterials (polygonArea, branchDescriptor);
            }
            treeFactory.previewTree.obj.transform.parent = parentTransform;
            sproutCompositeManager.ShowAllBranchesInMesh ();
            sproutCompositeManager.EndUsage ();
            polygonBuilder.EndUsage ();
        }        
        #endregion

        #region Texture Processing
        public bool GeneratePolygonTexture (
            Mesh mesh, 
            Bounds bounds,
            Material[] originalMaterials,
            MaterialMode materialMode,
            int width,
            int height,
            out Texture2D texture)
        {
            texture = null;

            // Apply material mode.
            GameObject previewTree = TreeFactory.GetActiveInstance ().previewTree.obj;
            MeshRenderer meshRenderer = previewTree.GetComponent<MeshRenderer> ();
            if (materialMode == MaterialMode.Albedo) { // Albedo
                meshRenderer.sharedMaterials = GetAlbedoMaterials (originalMaterials,
                    branchDescriptorCollection.sproutStyleA.colorTint,
                    branchDescriptorCollection.sproutStyleB.colorTint,
                    branchDescriptorCollection.sproutStyleA.colorSaturation,
                    branchDescriptorCollection.sproutStyleB.colorSaturation,
                    branchDescriptorCollection.branchColorShade,
                    branchDescriptorCollection.branchColorSaturation,
                    SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
                    SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
            } else if (materialMode == MaterialMode.Normals) { // Normals
                meshRenderer.sharedMaterials = GetNormalMaterials (originalMaterials);
            } else if (materialMode == MaterialMode.Extras) { // Extras
                meshRenderer.sharedMaterials = GetExtraMaterials (originalMaterials,
                    branchDescriptorCollection.sproutStyleA.metallic, 
                    branchDescriptorCollection.sproutStyleA.glossiness,
                    branchDescriptorCollection.sproutStyleB.metallic, 
                    branchDescriptorCollection.sproutStyleB.glossiness,
                    GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
            } else if (materialMode == MaterialMode.Subsurface) { // Subsurface
                meshRenderer.sharedMaterials = GetSubsurfaceMaterials (originalMaterials,
                    branchDescriptorCollection.sproutStyleA.colorTint, 
                    branchDescriptorCollection.sproutStyleB.colorTint,
                    branchDescriptorCollection.branchColorSaturation,
                    branchDescriptorCollection.sproutStyleA.colorSaturation,
                    branchDescriptorCollection.sproutStyleB.colorSaturation,
                    branchDescriptorCollection.sproutStyleA.subsurfaceMul,
					branchDescriptorCollection.sproutStyleB.subsurfaceMul,
                    GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
            } else if (materialMode == MaterialMode.Composite) { // Composite
                meshRenderer.sharedMaterials = GetCompositeMaterials (originalMaterials,
                    branchDescriptorCollection.sproutStyleA.colorTint, 
                    branchDescriptorCollection.sproutStyleB.colorTint,
                    GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
            }

            // Prepare texture builder according to the material mode.
            TextureBuilder tb = new TextureBuilder ();
            if (materialMode == MaterialMode.Normals) {
                tb.backgroundColor = new Color (0.5f, 0.5f, 1f, 1f);
                tb.textureFormat = TextureFormat.RGB24;
            } else if (materialMode == MaterialMode.Subsurface) {
                tb.backgroundColor = new Color (0f, 0f, 0f, 1f);
                tb.textureFormat = TextureFormat.RGB24;
            } else if (materialMode == MaterialMode.Extras) {
                tb.backgroundColor = new Color (0f, 0f, 1f, 1f);
                tb.textureFormat = TextureFormat.RGB24;
            }

            // Set the mesh..
            tb.useTextureSizeToTargetRatio = true;
            tb.BeginUsage (previewTree, mesh);
            tb.textureSize = new Vector2 (width, height);
            texture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up, bounds, null);
            tb.EndUsage ();

            return true;
        }
        public bool GenerateSnapshopTextures (int snapshotIndex, BranchDescriptorCollection branchDescriptorCollection,
            int width, int height, string albedoPath, string normalPath, string extrasPath, string subsurfacePath, string compositePath) {
            return GenerateSnapshopTextures (snapshotIndex, branchDescriptorCollection, width, height, GetPreviewTreeBounds (),
                albedoPath, normalPath, extrasPath, subsurfacePath, compositePath);
        }
        public bool GenerateSnapshopTextures (int snapshotIndex, BranchDescriptorCollection branchDescriptorCollection,
            int width, int height, Bounds bounds,
            string albedoPath, string normalPath, string extrasPath, string subsurfacePath, string compositePath) {
            BeginSnapshotProgress (branchDescriptorCollection);
            // ALBEDO
            if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) {
                ReportProgress ("Processing albedo texture.", 0f);
                GenerateSnapshopTexture (
                    branchDescriptorCollection.branchDescriptorIndex, 
                    branchDescriptorCollection,
                    SproutSubfactory.MaterialMode.Albedo, 
                    width,
                    height,
                    bounds,
                    albedoPath);
                ReportProgress ("Processing albedo texture.", 20f);
            }
            // NORMALS
            if ((branchDescriptorCollection.exportTexturesFlags & 2) == 2) {
                ReportProgress ("Processing normal texture.", 0f);
                GenerateSnapshopTexture (
                    branchDescriptorCollection.branchDescriptorIndex,
                    branchDescriptorCollection,
                    SproutSubfactory.MaterialMode.Normals, 
                    width,
                    height,
                    bounds,
                    normalPath);
                ReportProgress ("Processing normal texture.", 20f);
            }
            // EXTRAS
            if ((branchDescriptorCollection.exportTexturesFlags & 4) == 4) {
                ReportProgress ("Processing extras texture.", 0f);
                GenerateSnapshopTexture (
                    branchDescriptorCollection.branchDescriptorIndex, 
                    branchDescriptorCollection,
                    SproutSubfactory.MaterialMode.Extras, 
                    width,
                    height,
                    bounds,
                    extrasPath);
                ReportProgress ("Processing extras texture.", 20f);
            }
            // SUBSURFACE
            if ((branchDescriptorCollection.exportTexturesFlags & 8) == 8) {
                ReportProgress ("Processing subsurface texture.", 0f);
                GenerateSnapshopTexture (
                    branchDescriptorCollection.branchDescriptorIndex, 
                    branchDescriptorCollection,
                    SproutSubfactory.MaterialMode.Subsurface, 
                    width,
                    height,
                    bounds,
                    subsurfacePath);
                ReportProgress ("Processing subsurface texture.", 20f);
            }
            // COMPOSITE
            if ((branchDescriptorCollection.exportTexturesFlags & 16) == 16) {
                ReportProgress ("Processing composite texture.", 0f);
                GenerateSnapshopTexture (
                    branchDescriptorCollection.branchDescriptorIndex, 
                    branchDescriptorCollection,
                    SproutSubfactory.MaterialMode.Composite, 
                    width,
                    height,
                    bounds,
                    compositePath);
                ReportProgress ("Processing composite texture.", 20f);
            }
            FinishSnapshotProgress ();
            
            // Cleanup.
            MeshFilter meshFilter = treeFactory.previewTree.obj.GetComponent<MeshFilter>();
            UnityEngine.Object.DestroyImmediate (meshFilter.sharedMesh);

            return true;
        }
        /// <summary>
        /// Generates the texture for a giver snapshot.
        /// </summary>
        /// <param name="snapshotIndex">Index for the snapshot.</param>
        /// <param name="materialMode">Mode mode: composite, albedo, normals, extras or subsurface.</param>
        /// <param name="width">Maximum width for the texture.</param>
        /// <param name="height">Maximum height for the texture.</param>
        /// <param name="texturePath">Path to save the texture.</param>
        /// <returns>Texture generated.</returns>
        public Texture2D GenerateSnapshopTexture (
            int snapshotIndex, 
            BranchDescriptorCollection branchDescriptorCollection, 
            MaterialMode materialMode, 
            int width, 
            int height,
            Bounds bounds,
            string texturePath = "") 
        {
            if (snapshotIndex >= branchDescriptorCollection.branchDescriptors.Count) {
                Debug.LogWarning ("Could not generate branch snapshot texture. Index out of range.");
            } else {
                // Regenerate branch mesh and apply material mode.
                branchDescriptorIndex = snapshotIndex;
                ProcessSnapshot (true, materialMode);
                // Build and save texture.
                TextureBuilder tb = new TextureBuilder ();
                if (materialMode == MaterialMode.Normals) {
                    tb.backgroundColor = new Color (0.5f, 0.5f, 1f, 1f);
                    tb.textureFormat = TextureFormat.RGB24;
                } else if (materialMode == MaterialMode.Subsurface) {
                    tb.backgroundColor = new Color (0f, 0f, 0f, 1f);
                    tb.textureFormat = TextureFormat.RGB24;
                } else if (materialMode == MaterialMode.Extras) {
                    tb.backgroundColor = new Color (0f, 0f, 1f, 1f);
                    tb.textureFormat = TextureFormat.RGB24;
                }
                // Get tree mesh.
                GameObject previewTree = treeFactory.previewTree.obj;
                tb.useTextureSizeToTargetRatio = true;
                tb.BeginUsage (previewTree);
                tb.textureSize = new Vector2 (width, height);
                Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up, bounds, texturePath);
                tb.EndUsage ();
                return sproutTexture;
            }
            return null;
        }
        Bounds GetPreviewTreeBounds () {
            GameObject previewTree = treeFactory.previewTree.obj;
            MeshFilter meshFilder = previewTree.GetComponent<MeshFilter> ();
            if (meshFilder != null) {
                return meshFilder.sharedMesh.bounds;
            }
            return new Bounds ();
        }
        /// <summary>
        /// Generates an atlas texture from a snapshot at each branch descriptor in the collection.
        /// </summary>
        /// <param name="branchDescriptorCollection">Collection of branch descriptor.</param>
        /// <param name="width">Width in pixels for the atlas.</param>
        /// <param name="height">Height in pixels for the atlas.</param>
        /// <param name="padding">Padding in pixels between each atlas sprite.</param>
        /// <param name="albedoPath">Path to save the albedo texture.</param>
        /// <param name="normalsPath">Path to save the normals texture.</param>
        /// <param name="extrasPath">Path to save the extras texture.</param>
        /// <param name="subsurfacePath">Path to save the subsurface texture.</param>
        /// <param name="compositePath">Path to save the composite texture.</param>
        /// <returns><c>True</c> if the atlases were created.</returns>
        public bool GenerateAtlasTexture (
            BranchDescriptorCollection branchDescriptorCollection, 
            int width, 
            int height, 
            int padding,
            string albedoPath, 
            string normalPath, 
            string extrasPath, 
            string subsurfacePath, 
            string compositePath) 
        {
            #if UNITY_EDITOR
            if (branchDescriptorCollection.branchDescriptors.Count == 0) {
                Debug.LogWarning ("Could not generate atlas texture, no branch snapshots were found.");
            } else {
                // 1. Generate each snapshot mesh.
                float largestMeshSize = 0f; 
                List<Mesh> meshes = new List<Mesh> (); // Save the mesh for each snapshot.
                List<Material[]> materials = new List<Material[]> ();
                List<Texture2D> texturesForAtlas = new List<Texture2D> ();
                Material[] modeMaterials;
                TextureBuilder tb = new TextureBuilder ();
                Texture2D atlas;
                tb.useTextureSizeToTargetRatio = true;

                double editorTime = UnityEditor.EditorApplication.timeSinceStartup;

                BeginAtlasProgress (branchDescriptorCollection);

                MeshFilter meshFilter = treeFactory.previewTree.obj.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = treeFactory.previewTree.obj.GetComponent<MeshRenderer>();
                for (int i = 0; i < branchDescriptorCollection.branchDescriptors.Count; i++) {
                    ReportProgress ("Creating mesh for snapshot " + i + ".", 0f);
                    branchDescriptorIndex = i;
                    BranchDescriptorCollectionToPipeline ();
                    ProcessSnapshot ();
                    meshes.Add (UnityEngine.Object.Instantiate (meshFilter.sharedMesh));
                    materials.Add (meshRenderer.sharedMaterials);
                    ReportProgress ("Creating mesh for snapshot " + i + ".", 10f);
                }

                // 2. Get the larger snapshot.
                for (int i = 0; i < meshes.Count; i++) {
                    if (meshes [i].bounds.max.magnitude > largestMeshSize) {
                        largestMeshSize = meshes [i].bounds.max.magnitude;
                    }
                }

                // Generate each mode texture.
                GameObject previewTree = treeFactory.previewTree.obj;

                // ALBEDO
                if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) {
                    for (int i = 0; i < meshes.Count; i++) {
                        ReportProgress ("Creating albedo texture for snapshot " + i + ".", 0f);
                        // 3. Get the texture scale for the texture.
                        float meshScale = meshes [i].bounds.max.magnitude / largestMeshSize;
                        // 4.1 Albedo.
                        modeMaterials = GetAlbedoMaterials (materials [i],
                            branchDescriptorCollection.sproutStyleA.colorTint,
                            branchDescriptorCollection.sproutStyleB.colorTint,
                            branchDescriptorCollection.sproutStyleA.colorSaturation,
							branchDescriptorCollection.sproutStyleB.colorSaturation,
							branchDescriptorCollection.branchColorShade,
							branchDescriptorCollection.branchColorSaturation,
                            GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
                        meshFilter.sharedMesh = meshes [i];
                        meshRenderer.sharedMaterials = modeMaterials;
                        tb.backgroundColor = new Color (0.5f, 0.5f, 0.5f, 0f);
                        tb.textureFormat = TextureFormat.RGBA32;
                        tb.BeginUsage (previewTree);
                        tb.textureSize = new Vector2 (width * meshScale, height * meshScale);
                        Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up);
                        texturesForAtlas.Add (sproutTexture);
                        tb.EndUsage ();
                        DestroyMaterials (modeMaterials);
                        ReportProgress ("Creating albedo texture for snapshot " + i + ".", 20f);
                    }
                    ReportProgress ("Creating albedo atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    SaveTextureToFile (atlas, albedoPath);
                    CleanTextures (texturesForAtlas);
                    UnityEngine.Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating albedo atlas texture.", 10f);
                }

                // NORMALS
                if ((branchDescriptorCollection.exportTexturesFlags & 2) == 2) {
                    for (int i = 0; i < meshes.Count; i++) {
                        ReportProgress ("Creating normal texture for snapshot " + i + ".", 0f);
                        // 3. Get the texture scale for the texture.
                        float meshScale = meshes [i].bounds.max.magnitude / largestMeshSize;
                        // 4.2 Normals.
                        modeMaterials = GetNormalMaterials (materials [i]);
                        meshFilter.sharedMesh = meshes [i];
                        meshRenderer.sharedMaterials = modeMaterials;
                        tb.backgroundColor = new Color (0.5f, 0.5f, 1f, 1f);
                        tb.textureFormat = TextureFormat.RGB24;
                        tb.BeginUsage (previewTree);
                        tb.textureSize = new Vector2 (width * meshScale, height * meshScale);
                        Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up);
                        texturesForAtlas.Add (sproutTexture);
                        tb.EndUsage ();
                        DestroyMaterials (modeMaterials);
                        ReportProgress ("Creating extra texture for snapshot " + i + ".", 20f);
                    }
                    ReportProgress ("Creating normal atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    SaveTextureToFile (atlas, normalPath);
                    CleanTextures (texturesForAtlas);
                    UnityEngine.Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating normal atlas texture.", 10f);
                }

                // EXTRAS
                if ((branchDescriptorCollection.exportTexturesFlags & 4) == 4) {
                    for (int i = 0; i < meshes.Count; i++) {
                        ReportProgress ("Creating extras texture for snapshot " + i + ".", 0f);
                        // 3. Get the texture scale for the texture.
                        float meshScale = meshes [i].bounds.max.magnitude / largestMeshSize;
                        // 4.3 Extra.
                        modeMaterials = GetExtraMaterials (materials [i],
                            branchDescriptorCollection.sproutStyleA.metallic, 
                            branchDescriptorCollection.sproutStyleA.glossiness,
                            branchDescriptorCollection.sproutStyleB.metallic, 
                            branchDescriptorCollection.sproutStyleB.glossiness,
                            GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
                        meshFilter.sharedMesh = meshes [i];
                        meshRenderer.sharedMaterials = modeMaterials;
                        tb.backgroundColor = new Color (0f, 0f, 1f, 1f);
                        tb.textureFormat = TextureFormat.RGB24;
                        tb.BeginUsage (previewTree);
                        tb.textureSize = new Vector2 (width * meshScale, height * meshScale);
                        Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up);
                        texturesForAtlas.Add (sproutTexture);
                        tb.EndUsage ();
                        DestroyMaterials (modeMaterials);
                        ReportProgress ("Creating extras texture for snapshot " + i + ".", 20f);
                    }
                    ReportProgress ("Creating extras atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    SaveTextureToFile (atlas, extrasPath);
                    CleanTextures (texturesForAtlas);
                    UnityEngine.Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating extras atlas texture.", 10f);
                }

                // SUBSURFACE
                if ((branchDescriptorCollection.exportTexturesFlags & 8) == 8) {
                    for (int i = 0; i < meshes.Count; i++) {
                        ReportProgress ("Creating subsurface texture for snapshot " + i + ".", 0f);
                        // 3. Get the texture scale for the texture.
                        float meshScale = meshes [i].bounds.max.magnitude / largestMeshSize;
                        // 4.4 Subsurface.
                        modeMaterials = GetSubsurfaceMaterials (materials [i],
                            branchDescriptorCollection.sproutStyleA.colorTint, 
                            branchDescriptorCollection.sproutStyleB.colorTint,
                            branchDescriptorCollection.branchColorSaturation,
                            branchDescriptorCollection.sproutStyleA.colorSaturation,
                            branchDescriptorCollection.sproutStyleB.colorSaturation,
                            branchDescriptorCollection.sproutStyleA.subsurfaceMul,
							branchDescriptorCollection.sproutStyleB.subsurfaceMul,
                            GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
                        meshFilter.sharedMesh = meshes [i];
                        meshRenderer.sharedMaterials = modeMaterials;
                        tb.backgroundColor = new Color (0f, 0f, 0f, 1f);
                        tb.textureFormat = TextureFormat.RGB24;
                        tb.BeginUsage (previewTree);
                        tb.textureSize = new Vector2 (width * meshScale, height * meshScale);
                        Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up);
                        texturesForAtlas.Add (sproutTexture);
                        tb.EndUsage ();
                        DestroyMaterials (modeMaterials);
                        ReportProgress ("Creating subsurface texture for snapshot " + i + ".", 20f);
                    }
                    ReportProgress ("Creating subsurface atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    SaveTextureToFile (atlas, subsurfacePath);
                    CleanTextures (texturesForAtlas);
                    UnityEngine.Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating subsurface atlas texture.", 10f);
                }

                // COMPOSITE
                if ((branchDescriptorCollection.exportTexturesFlags & 16) == 16) {
                    for (int i = 0; i < meshes.Count; i++) {
                        ReportProgress ("Creating composite texture for snapshot " + i + ".", 0f);
                        // 3. Get the texture scale for the texture.
                        float meshScale = meshes [i].bounds.max.magnitude / largestMeshSize;
                        // 4.5 Composite.
                        modeMaterials = materials [i];
                        /*
                        GetCompositeMaterials (materials [i],
                            GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
                            */
                        meshFilter.sharedMesh = meshes [i];
                        meshRenderer.sharedMaterials = modeMaterials;
                        tb.backgroundColor = new Color (0.5f, 0.5f, 0.5f, 0f);
                        tb.textureFormat = TextureFormat.RGBA32;
                        tb.BeginUsage (previewTree);
                        tb.textureSize = new Vector2 (width * meshScale, height * meshScale);
                        Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up);
                        texturesForAtlas.Add (sproutTexture);
                        tb.EndUsage ();
                        DestroyMaterials (modeMaterials);
                        ReportProgress ("Creating composite texture for snapshot " + i + ".", 20f);
                    }
                    ReportProgress ("Creating composite atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    SaveTextureToFile (atlas, compositePath);
                    CleanTextures (texturesForAtlas);
                    UnityEngine.Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating composite atlas texture.", 10f);
                }

                // Cleanup, destroy meshes, materials and textures.
                for (int i = 0; i < meshes.Count; i++) {
                    UnityEngine.Object.DestroyImmediate (meshes [i]);
                }
                for (int i = 0; i < materials.Count; i++) {
                    for (int j = 0; j < materials [i].Length; j++) {
                        UnityEngine.Object.DestroyImmediate (materials [i][j]);   
                    }
                }
                FinishAtlasProgress ();
                return true;
            }
            #endif
            return false;
        }
        /// <summary>
        /// Generates an atlas texture from the textures registered at the SproutCompositeManager.
        /// </summary>
        /// <param name="branchDescriptorCollection">Collection of branch descriptor.</param>
        /// <param name="width">Width in pixels for the atlas.</param>
        /// <param name="height">Height in pixels for the atlas.</param>
        /// <param name="padding">Padding in pixels between each atlas sprite.</param>
        /// <param name="albedoPath">Path to save the albedo texture.</param>
        /// <param name="normalsPath">Path to save the normals texture.</param>
        /// <param name="extrasPath">Path to save the extras texture.</param>
        /// <param name="subsurfacePath">Path to save the subsurface texture.</param>
        /// <param name="compositePath">Path to save the composite texture.</param>
        /// <returns><c>True</c> if the atlases were created.</returns>
        public bool GenerateAtlasTextureFromPolygons (
            BranchDescriptorCollection branchDescriptorCollection, 
            int width, 
            int height, 
            int padding,
            string albedoPath, 
            string normalsPath, 
            string extrasPath, 
            string subsurfacePath, 
            string compositePath) 
        {
            #if UNITY_EDITOR
            if (branchDescriptorCollection.branchDescriptors.Count == 0) {
                Debug.LogWarning ("Could not generate atlas texture, no branch snapshots were found.");
            } else {
                // 1. Save the mesh and materials for each snapshot.
                List<Mesh> meshes = new List<Mesh> (); // Save the mesh for each snapshot.
                List<Material[]> materials = new List<Material[]> ();
                List<Texture2D> texturesForAtlas = new List<Texture2D> ();
                List<BroccoTree> trees = new List<BroccoTree> ();

                // 2. Create atlas texture and texture builder.
                Texture2D atlas;
                TextureBuilder tb = new TextureBuilder ();
                tb.useTextureSizeToTargetRatio = true;

                // 3. Init helper vars.
                float largestMeshSize = 0f;
                double editorTime = UnityEditor.EditorApplication.timeSinceStartup;
                Rect[] atlasRects = null;

                // 4. Begin atlas creation process.
                BeginAtlasProgress (branchDescriptorCollection);

                // 5. For each branch descriptor create its snapshot.
                MeshFilter meshFilter = treeFactory.previewTree.obj.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = treeFactory.previewTree.obj.GetComponent<MeshRenderer>();
                sproutCompositeManager.Clear ();
                for (int i = 0; i < branchDescriptorCollection.branchDescriptors.Count; i++) {
                    ReportProgress ("Creating mesh for snapshot " + i + ".", 0f);
                    branchDescriptorIndex = i;
                    BranchDescriptorCollectionToPipeline ();
                    ProcessSnapshot (false);

                    // 5.1 Save the snapshot tree, mesh and snapshot materials.
                    meshes.Add (UnityEngine.Object.Instantiate (meshFilter.sharedMesh));
                    materials.Add (meshRenderer.sharedMaterials);
                    trees.Add (treeFactory.previewTree);
                    treeFactory.previewTree.obj.transform.parent = null;
                    treeFactory.previewTree.obj.hideFlags = HideFlags.None;
                    treeFactory.previewTree = null;
                    ReportProgress ("Creating mesh for snapshot " + i + ".", 10f);

                }

                // 6. Get the snapshot with the largest area.
                for (int i = 0; i < meshes.Count; i++) {
                    if (meshes [i].bounds.max.magnitude > largestMeshSize) {
                        largestMeshSize = meshes [i].bounds.max.magnitude;
                    }
                }

                // 7. For each snapshot create its polygons.
                for (int i = 0; i < branchDescriptorCollection.branchDescriptors.Count; i++) {
                    treeFactory.previewTree = trees [i];
                    snapshotTree = treeFactory.previewTree;
                    snapshotTreeMesh = meshFilter.sharedMesh;
                    sproutCompositeManager.textureGlobalScale = snapshotTreeMesh.bounds.max.magnitude / largestMeshSize; 
                    ProcessSnapshotPolygons (branchDescriptorCollection.branchDescriptors [i]);
                }

                // 8.1 Generate the ALBEDO texture.
                branchDescriptorCollection.atlasAlbedoTexture = null;
                if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) {
                    List<Texture2D> albedoTextures = sproutCompositeManager.GetAlbedoTextures ();
                    for (int i = 0; i < albedoTextures.Count; i++) {
                        texturesForAtlas.Add (albedoTextures [i]);
                    }
                    ReportProgress ("Creating albedo atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlasRects = atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    branchDescriptorCollection.atlasAlbedoTexture = SaveTextureToFile (atlas, albedoPath, true);
                    CleanTextures (texturesForAtlas);
                    UnityEngine.Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating albedo atlas texture.", 10f);
                }

                // 8.2 Generate the NORMALS texture.
                branchDescriptorCollection.atlasNormalsTexture = null;
                if ((branchDescriptorCollection.exportTexturesFlags & 2) == 2) {
                    List<Texture2D> normalsTextures = sproutCompositeManager.GetNormalsTextures ();
                    for (int i = 0; i < normalsTextures.Count; i++) {
                        texturesForAtlas.Add (normalsTextures [i]);
                    }
                    ReportProgress ("Creating normals atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlasRects = atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    branchDescriptorCollection.atlasNormalsTexture = SaveTextureToFile (atlas, normalsPath, true);
                    CleanTextures (texturesForAtlas);
                    UnityEngine.Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating normals atlas texture.", 10f);
                }

                // 8.3 Generate the EXTRAS texture.
                branchDescriptorCollection.atlasExtrasTexture = null;
                if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) {
                    List<Texture2D> extrasTextures = sproutCompositeManager.GetExtrasTextures ();
                    for (int i = 0; i < extrasTextures.Count; i++) {
                        texturesForAtlas.Add (extrasTextures [i]);
                    }
                    ReportProgress ("Creating extras atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlasRects = atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    branchDescriptorCollection.atlasExtrasTexture = SaveTextureToFile (atlas, extrasPath, true);
                    CleanTextures (texturesForAtlas);
                    UnityEngine.Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating extras atlas texture.", 10f);
                }

                // 8.4 Generate the SUBSURFACE texture.
                branchDescriptorCollection.atlasSubsurfaceTexture = null;
                if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) {
                    List<Texture2D> subsurfaceTextures = sproutCompositeManager.GetSubsurfaceTextures ();
                    for (int i = 0; i < subsurfaceTextures.Count; i++) {
                        texturesForAtlas.Add (subsurfaceTextures [i]);
                    }
                    ReportProgress ("Creating subsurface atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlasRects = atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    branchDescriptorCollection.atlasSubsurfaceTexture = SaveTextureToFile (atlas, subsurfacePath, true);
                    CleanTextures (texturesForAtlas);
                    UnityEngine.Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating subsurface atlas texture.", 10f);
                }

                // 8.5 Finish atlases creation.
                FinishAtlasProgress ();

                // 9. Set atlas rects to meshes.
                if (atlasRects != null) {
                    sproutCompositeManager.SetAtlasRects (atlasRects);
                    sproutCompositeManager.ApplyAtlasUVs ();
                }

                // 11. Clean up atlas building.
                sproutCompositeManager.textureGlobalScale = 1f;
                treeFactory.previewTree = null;
                for (int i = 0; i < trees.Count; i++) {
                    UnityEngine.Object.DestroyImmediate (trees[i].obj);
                }
                UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate ();
                return true;
            }
            #endif
            return false;
        }
        public Texture2D GetSproutTexture (int group, int index) {
            string textureId = GetSproutTextureId (group, index);
            return textureManager.GetTexture (textureId);
        }
        Texture2D GetOriginalSproutTexture (int group, int index) {
            Texture2D texture = null;
            List<SproutMap.SproutMapArea> sproutMapAreas = null;
            if (group == 0) {
                sproutMapAreas = branchDescriptorCollection.sproutAMapAreas;
            } else if (group == 1) {
                sproutMapAreas = branchDescriptorCollection.sproutBMapAreas;
            }
            if (sproutMapAreas != null && sproutMapAreas.Count >= index) {
                texture = sproutMapAreas[index].texture;
            }
            return texture;
        }
        public void ProcessTextures () {
            textureManager.Clear ();
            string textureId;
            // Process Sprout A albedo textures.
            for (int i = 0; i < branchDescriptorCollection.sproutAMapAreas.Count; i++) {    
                Texture2D texture = ApplyTextureTransformations (
                    branchDescriptorCollection.sproutAMapAreas [i].texture, 
                    branchDescriptorCollection.sproutAMapDescriptors [i].alphaFactor);
                if (texture != null) {
                    textureId = GetSproutTextureId (0, i);
                    textureManager.AddOrReplaceTexture (textureId, texture);
                }
            }
            // Process Sprout B albedo textures.    
            for (int i = 0; i < branchDescriptorCollection.sproutBMapAreas.Count; i++) {
                Texture2D texture = ApplyTextureTransformations (
                    branchDescriptorCollection.sproutBMapAreas [i].texture, 
                    branchDescriptorCollection.sproutBMapDescriptors [i].alphaFactor);
                if (texture != null) {
                    textureId = GetSproutTextureId (1, i);
                    textureManager.AddOrReplaceTexture (textureId, texture);
                }
            }
        }
        public void ProcessTexture (int group, int index, float alpha) {
            #if UNITY_EDITOR
            string textureId = GetSproutTextureId (group, index);
            //if (textureManager.HasTexture (textureId)) {
                Texture2D originalTexture = GetOriginalSproutTexture (group, index);
                Texture2D newTexture = ApplyTextureTransformations (originalTexture, alpha);
                newTexture.alphaIsTransparency = true;
                textureManager.AddOrReplaceTexture (textureId, newTexture, true);
                BranchDescriptorCollectionToPipeline ();
            //}
            #endif
        }
        Texture2D ApplyTextureTransformations (Texture2D originTexture, float alpha) {
            if (originTexture != null) {
                Texture2D tex = textureManager.GetCopy (originTexture, alpha);
                return tex;
            }
            return null;
        }
        public string GetSproutTextureId (int group, int index) {
            return  "sprout_" + group + "_" + index;
        }
        /// <summary>
		/// Saves a texture to a file.
		/// </summary>
		/// <param name="texture">Texture.</param>
		/// <param name="filename">Filename.</param>
		public Texture2D SaveTextureToFile (Texture2D texture, string filename, bool importAsset = true) {
			#if UNITY_EDITOR
			System.IO.File.WriteAllBytes (filename, texture.EncodeToPNG());
            if (importAsset) {
                texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D> (filename);
            }
            return texture;
#endif
            return null;
		}
        void CleanTextures (List<Texture2D> texturesToClean) {
            for (int i = 0; i < texturesToClean.Count; i++) {
                UnityEngine.Object.DestroyImmediate (texturesToClean [i]);
            }
            texturesToClean.Clear ();
        }
        #endregion

        #region Material Processing
        public Material[] GetCompositeMaterials (Material[] originalMaterials,
            Color tintColorA,
            Color tintColorB,
            int materialAStartIndex = -1,
            int materialBStartIndex = -1) 
        {
            Material[] mats = new Material[originalMaterials.Length];
            if (materialAStartIndex == -1) materialAStartIndex = 0;
            for (int i = 0; i < originalMaterials.Length; i++) {
                if (originalMaterials [i] != null) {
                    if (i == 0) {
                        mats[0] = originalMaterials [0];
                        mats[0].shader = GetSpeedTree8Shader ();
                    } else {
                        Material m = new Material (originalMaterials[i]);
                        //m.shader = Shader.Find ("Hidden/Broccoli/SproutLabComposite");
                        //m.shader = Shader.Find ("Broccoli/SproutLabComposite");
                        m.shader = GetSpeedTree8Shader ();
                        /*
                        m.EnableKeyword ("EFFECT_BUMP");
                        m.EnableKeyword ("EFFECT_SUBSURFACE");
                        m.EnableKeyword ("EFFECT_EXTRA_TEX");
                        */
                        m.EnableKeyword ("GEOM_TYPE_LEAF");
                        mats [i] = m;
                        if (i >= materialAStartIndex) {
                            if (i >= materialBStartIndex) {
                                m.SetColor ("_TintColor", tintColorB);
                            } else {
                                m.SetColor ("_TintColor", tintColorA);
                            }
                        }
                    }
                }
            }
            return mats;
        }
        public Material[] GetAlbedoMaterials (Material[] originalMaterials,
            Color tintColorA,
            Color tintColorB,
            float materialASaturation = 1f,
            float materialBSaturation = 1f,
            float branchMaterialShade = 1f,
            float branchMaterialSaturation = 1f,
            int materialAStartIndex = -1,
            int materialBStartIndex = -1,
            bool applyExtraSaturation = true)
        {
            Material[] mats = new Material[originalMaterials.Length];
            if (materialAStartIndex == -1) materialAStartIndex = 0;
            for (int i = 0; i < originalMaterials.Length; i++) {
                Material m;
                if (originalMaterials [i] == null) {
                    m = originalMaterials [i];
                } else {
                    m = new Material (originalMaterials[i]);
                    m.shader = Shader.Find ("Hidden/Broccoli/SproutLabAlbedo");
                    m.SetFloat ("_BranchShade", branchMaterialShade);
                    m.SetFloat ("_BranchSat", branchMaterialSaturation);
                    m.SetFloat ("_ApplyExtraSat", applyExtraSaturation?1f:0f);
                    #if UNITY_EDITOR
                    m.SetFloat ("_IsLinearColorSpace", UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear?1f:0f);
                    #endif
                    mats [i] = m;
                    if (i >= materialAStartIndex) {
                        if (i >= materialBStartIndex) {
                            m.SetColor ("_TintColor", tintColorB);
                            m.SetFloat ("_SproutSat", materialBSaturation);
                        } else {
                            m.SetColor ("_TintColor", tintColorA);
                            m.SetFloat ("_SproutSat", materialASaturation);
                        }
                    }
                }
            }
            return mats;
        }
        public Material[] GetNormalMaterials (Material[] originalMaterials) {
            Material[] mats = new Material[originalMaterials.Length];
            for (int i = 0; i < originalMaterials.Length; i++) {
                Material m = new Material (originalMaterials[i]);
                m.shader = Shader.Find ("Hidden/Broccoli/SproutLabNormals");
                #if UNITY_EDITOR
                m.SetFloat ("_IsLinearColorSpace", UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear?1f:0f);
                #endif
                mats [i] = m;
            }
            return mats;
        }
        public Material[] GetExtraMaterials (Material[] originalMaterials,
            float metallicA,
            float glossinessA,
            float metallicB,
            float glossinessB,
            int materialAStartIndex = -1,
            int materialBStartIndex = -1)
        {
            Material[] mats = new Material[originalMaterials.Length];
            if (materialAStartIndex == -1) materialAStartIndex = 0;
            for (int i = 0; i < originalMaterials.Length; i++) {
                Material m = new Material (originalMaterials[i]);
                m.shader = Shader.Find ("Hidden/Broccoli/SproutLabExtra");
                #if UNITY_EDITOR
                m.SetFloat ("_IsLinearColorSpace", UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear?1f:0f);
                #endif
                mats [i] = m;
                if (i >= materialAStartIndex) {
                    if (i >= materialBStartIndex) {
                        m.SetFloat ("_Metallic", metallicB);
                        m.SetFloat ("_Glossiness", glossinessB);
                    } else {
                        m.SetFloat ("_Metallic", metallicA);
                        m.SetFloat ("_Glossiness", glossinessA);
                    }
                }
            }
            return mats;
        }
        public Material[] GetSubsurfaceMaterials (Material[] originalMaterials,
            Color tintColorA,
            Color tintColorB,
            float branchSaturation = 1f,
            float sproutASaturation = 1f,
            float sproutBSaturation = 1f,
            float sproutASubsurfaceMul = 1f,
            float sproutBSubsurfaceMul = 1f,
            int materialAStartIndex = -1,
            int materialBStartIndex = -1) 
        {
            Material[] mats = new Material[originalMaterials.Length];
            if (materialAStartIndex == -1) materialAStartIndex = 0;
            for (int i = 0; i < originalMaterials.Length; i++) {
                Material m = new Material (originalMaterials[i]);
                m.shader = Shader.Find ("Hidden/Broccoli/SproutLabSubsurface");
                m.SetFloat ("_BranchSat", branchSaturation);
                #if UNITY_EDITOR
                m.SetFloat ("_IsLinearColorSpace", UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear?1f:0f);
                #endif
                mats [i] = m;
                if (i >= materialAStartIndex) {
                    if (i >= materialBStartIndex) {
                        m.SetColor ("_TintColor", tintColorB);
                        m.SetFloat ("_SproutSat", sproutBSaturation);
                        m.SetFloat ("_SproutBr", sproutBSubsurfaceMul);
                    } else {
                        m.SetColor ("_TintColor", tintColorA);
                        m.SetFloat ("_SproutSat", sproutASaturation);
                        m.SetFloat ("_SproutBr", sproutASubsurfaceMul);
                    }
                }
            }
            return mats;
        }
        public Material[] GetCompositeMaterials (Material[] originalMaterials,
            int materialAStartIndex = -1,
            int materialBStartIndex = -1) 
        {
            Material[] mats = new Material[originalMaterials.Length];
            if (materialAStartIndex == -1) materialAStartIndex = 0;
            for (int i = 0; i < originalMaterials.Length; i++) {
                Material m = new Material (originalMaterials[i]);
            }
            return mats;
        }
        public static int GetMaterialAStartIndex (BranchDescriptorCollection branchDescriptorCollection) {
            return 1;
        }
        public static int GetMaterialBStartIndex (BranchDescriptorCollection branchDescriptorCollection) {
            int materialIndex = branchDescriptorCollection.sproutAMapAreas.Count + 1;
            return materialIndex;
        }
        public void DestroyMaterials (Material[] materials) {
            for (int i = 0; i < materials.Length; i++) {
                UnityEngine.Object.DestroyImmediate (materials [i]);
            }
        }
        private Shader GetSpeedTree8Shader () {
            Shader st8Shader = null;
            /*
            var currentRenderPipeline = GraphicsSettings.defaultRenderPipeline;
            if (currentRenderPipeline != null) {
                st8Shader = currentRenderPipeline.defaultSpeedTree8Shader;
            } else {
                */
                st8Shader = Shader.Find ("Nature/SpeedTree8");
            //}
            return st8Shader;
        }
        #endregion

        #region Processing Progress
        public delegate void OnReportProgress (string msg, float progress);
        public delegate void OnFinishProgress ();
        public OnReportProgress onReportProgress;
        public OnFinishProgress onFinishProgress;
        float progressGone = 0f;
        float progressToGo = 0f;
        public string progressTitle = "";
        public void BeginSnapshotProgress (BranchDescriptorCollection branchDescriptorCollection) {
            progressGone = 0f;
            progressToGo = 0f;
            if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) progressToGo += 20; // Albedo
            if ((branchDescriptorCollection.exportTexturesFlags & 2) == 2) progressToGo += 20; // Normals
            if ((branchDescriptorCollection.exportTexturesFlags & 4) == 4) progressToGo += 20; // Extras
            if ((branchDescriptorCollection.exportTexturesFlags & 8) == 8) progressToGo += 20; // Subsurface
            if ((branchDescriptorCollection.exportTexturesFlags & 16) == 16) progressToGo += 20; // Composite
            progressTitle = "Creating Snapshot Textures";
        }
        public void FinishSnapshotProgress () {
            progressGone = progressToGo;
            ReportProgress ("Finish " + progressTitle, 0f);
            onFinishProgress?.Invoke ();
        }
        public void BeginAtlasProgress (BranchDescriptorCollection branchDescriptorCollection) {
            progressGone = 0f;
            progressToGo = branchDescriptorCollection.branchDescriptors.Count * 10f;
            if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) progressToGo += 30; // Albedo
            if ((branchDescriptorCollection.exportTexturesFlags & 2) == 2) progressToGo += 30; // Normals
            if ((branchDescriptorCollection.exportTexturesFlags & 4) == 4) progressToGo += 30; // Extras
            if ((branchDescriptorCollection.exportTexturesFlags & 8) == 8) progressToGo += 30; // Subsurface
            if ((branchDescriptorCollection.exportTexturesFlags & 16) == 16) progressToGo += 30; // Composite
            progressTitle = "Creating Atlas Textures";
        }
        public void FinishAtlasProgress () {
            progressGone = progressToGo;
            ReportProgress ("Finish " + progressTitle, 0f);
            onFinishProgress?.Invoke ();
        }
        void ReportProgress (string title, float progressToAdd) {
            progressGone += progressToAdd;
            onReportProgress?.Invoke (title, progressGone/progressToGo);
        }
        #endregion
    }
}