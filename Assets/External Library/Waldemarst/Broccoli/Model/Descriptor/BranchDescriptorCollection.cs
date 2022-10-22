using System;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Container and manager for branch descriptors.
	/// </summary>
	[System.Serializable]
	public class BranchDescriptorCollection : ISerializationCallbackReceiver {
		#region Subclasses
		[System.Serializable]
		public class SproutMapDescriptor {
			public float alphaFactor = 1.0f;
			public SproutMapDescriptor Clone () {
				SproutMapDescriptor clone = new SproutMapDescriptor ();
				clone.alphaFactor = alphaFactor;
				return clone;
			}
		}
		[System.Serializable]
		public class SproutStyle {
			public float minColorShade = 0.75f;
			public float maxColorShade = 1f;
			public float minColorTint = 0f;
			public float maxColorTint = 0.2f;
			public Color colorTint = Color.white;
			public float colorSaturation = 1f;
			public float metallic = 0f;
			public float glossiness = 0f;
			public float subsurfaceMul = 1f;
			public SproutStyle Clone () {
				SproutStyle clone = new SproutStyle ();
				clone.minColorShade = minColorShade;
				clone.maxColorShade = maxColorShade;
				clone.minColorTint = minColorTint;
				clone.maxColorTint = maxColorTint;
				clone.colorTint = colorTint;
				clone.colorSaturation = colorSaturation;
				clone.metallic = metallic;
				clone.glossiness = glossiness;
				clone.subsurfaceMul = subsurfaceMul;
				return clone;
			}
		}
		#endregion
		
		#region Vars
		/// <summary>
		/// Type of descriptor for this collection.
		/// </summary>
		[SerializeField]
		public int descriptorImplId = -1;
		/// <summary>
		/// The branch descriptors.
		/// </summary>
		[SerializeField]
		public List<BranchDescriptor> branchDescriptors = new List<BranchDescriptor> ();
		/// <summary>
		/// The variation descriptors.
		/// </summary>
		[SerializeField]
		public List<VariationDescriptor> variationDescriptors = new List<VariationDescriptor> ();
		public int branchDescriptorIndex = -1;
		public int lastBranchDescriptorIndex = -1;
		public int variationDescriptorIndex = -1;
		public int lastVariationDescriptorIndex = -1;
		#endregion

		#region Constructor
		public BranchDescriptorCollection () {}
		#endregion

		#region Map Vars
        /// <summary>
        /// Main texture for branches.
        /// </summary>
        public Texture2D branchAlbedoTexture = null;
        /// <summary>
        /// Normal map texture for branches.
        /// </summary>
        public Texture2D branchNormalTexture = null;
        public float branchTextureYDisplacement = 0f;
        public List<SproutMap.SproutMapArea> sproutAMapAreas = new List<SproutMap.SproutMapArea> ();
        public List<SproutMap.SproutMapArea> sproutBMapAreas = new List<SproutMap.SproutMapArea> ();
		public List<SproutMapDescriptor> sproutAMapDescriptors = new List<SproutMapDescriptor> ();
		public List<SproutMapDescriptor> sproutBMapDescriptors = new List<SproutMapDescriptor> ();
		public SproutStyle sproutStyleA = new SproutStyle ();
		public SproutStyle sproutStyleB = new SproutStyle ();
		public float branchColorShade = 1f;
		public float branchColorSaturation = 1f;
        #endregion

		#region Export Settings Vars
		/// <summary>
		/// Available texture export modes.
		/// </summary>
		public enum ExportMode {
			SelectedSnapshot,
			Atlas
		}
		/// <summary>
		/// Export mode selected.
		/// </summary>
		public ExportMode exportMode = ExportMode.Atlas;
		/// <summary>
		/// Path to save the textures relative to the data application path.
		/// </summary>
		public string exportPath = "";
		public string exportPrefix = "branch";
		public int exportTake = 0;
		/// <summary>
		/// Texture size.
		/// </summary>
		public enum TextureSize
		{
			_128px,
			_256px,
			_512px,
			_1024px,
			_2048px
		}
		public TextureSize exportTextureSize = TextureSize._1024px;
		public int exportAtlasPadding = 5;
		public bool exportAlbedoEnabled = true;
		public int exportTexturesFlags = 15;
		public Texture2D atlasAlbedoTexture = null;
		public Texture2D atlasNormalsTexture = null;
		public Texture2D atlasExtrasTexture = null;
		public Texture2D atlasSubsurfaceTexture = null;
		#endregion

		#region Management
		/// <summary>
		/// Adds a branch descriptor instance to this collection, assigning it and id.
		/// </summary>
		/// <param name="branchDescriptor"></param>
		public void AddBranchDescriptor (BranchDescriptor branchDescriptor) {
			branchDescriptor.id = GetNextBranchDescriptorId ();
			branchDescriptors.Add (branchDescriptor);
		}
		public void AddVariationDescriptor (VariationDescriptor variationDescriptor) {
			variationDescriptor.id = GetNextBranchDescriptorId ();
			variationDescriptors.Add (variationDescriptor);
		}
		/// <summary>
		/// Gets the next id for the branch descriptors in this collection.
		/// </summary>
		/// <returns>Next id for branch descriptors.</returns>
		private int GetNextBranchDescriptorId () {
			int _lastBranchDescriptorId = 0;
			for (int i = 0; i < branchDescriptors.Count; i++) {
				if (branchDescriptors [i].id > _lastBranchDescriptorId) {
					_lastBranchDescriptorId = branchDescriptors [i].id;
				}
			}
			return _lastBranchDescriptorId++;
		}
		/// <summary>
		/// Gets the next id for the variation descriptors in this collection.
		/// </summary>
		/// <returns>Next id for variation descriptors.</returns>
		private int GetNextVariationDescriptorId () {
			int _lastVariationDescriptorId = 0;
			for (int i = 0; i < variationDescriptors.Count; i++) {
				if (variationDescriptors [i].id > _lastVariationDescriptorId) {
					_lastVariationDescriptorId = variationDescriptors [i].id;
				}
			}
			return _lastVariationDescriptorId++;
		}
		#endregion

		#region Serializable
        /// <summary>
        /// Before serialization method.
        /// </summary>
		public void OnBeforeSerialize() {}
        /// <summary>
        /// After serialization method.
        /// </summary>
		public void OnAfterDeserialize() {
			// Get the last id and set the id on branch descriptors with id = 0.
			bool hasUnassigned = false;
			int _lastId = 0;
			for (int i = 0; i < branchDescriptors.Count; i++) {
				if (branchDescriptors [i].id <= 0) hasUnassigned = true;
				else if (branchDescriptors [i].id > _lastId) {
					_lastId = branchDescriptors [i].id;
				}
			}
			// Assign missing ids on branch descriptors.
			if (hasUnassigned) {
				for (int i = 0; i < branchDescriptors.Count; i++) {
					_lastId++;
					if (branchDescriptors [i].id <= 0) branchDescriptors [i].id = _lastId;
				}
			}
		}
		#endregion

		#region Clone
		public BranchDescriptorCollection Clone () {
			BranchDescriptorCollection clone = new BranchDescriptorCollection ();
			clone.descriptorImplId = descriptorImplId;
			
			for (int i = 0; i < branchDescriptors.Count; i++) {
				clone.branchDescriptors.Add (branchDescriptors [i].Clone ());
			}
			clone.branchDescriptorIndex = branchDescriptorIndex;
			clone.lastBranchDescriptorIndex = lastBranchDescriptorIndex;

			for (int i = 0; i < variationDescriptors.Count; i++) {
				clone.variationDescriptors.Add (variationDescriptors [i].Clone ());
			}
			clone.variationDescriptorIndex = variationDescriptorIndex;
			clone.lastVariationDescriptorIndex = lastVariationDescriptorIndex;

			clone.branchTextureYDisplacement = branchTextureYDisplacement;
            clone.branchAlbedoTexture = branchAlbedoTexture;
            clone.branchNormalTexture = branchNormalTexture;
			for (int i = 0; i < sproutAMapAreas.Count; i++) {
                clone.sproutAMapAreas.Add (sproutAMapAreas [i].Clone ());
            }
			for (int i = 0; i < sproutBMapAreas.Count; i++) {
                clone.sproutBMapAreas.Add (sproutBMapAreas [i].Clone ());
            }
			for (int i = 0; i < sproutAMapDescriptors.Count; i++) {
                clone.sproutAMapDescriptors.Add (sproutAMapDescriptors [i].Clone ());
            }
			for (int i = 0; i < sproutBMapDescriptors.Count; i++) {
                clone.sproutBMapDescriptors.Add (sproutBMapDescriptors [i].Clone ());
            }
			clone.branchColorShade = branchColorShade;
			clone.branchColorSaturation = branchColorSaturation;

			clone.sproutStyleA = sproutStyleA.Clone ();
			clone.sproutStyleB = sproutStyleB.Clone ();

			clone.exportMode = exportMode;
			clone.exportPath = exportPath;
			clone.exportPrefix = exportPrefix;
			clone.exportTake = exportTake;
			clone.exportTextureSize = exportTextureSize;
			clone.exportAtlasPadding = exportAtlasPadding;
			clone.exportTexturesFlags = exportTexturesFlags;
			clone.atlasAlbedoTexture = atlasAlbedoTexture;
			clone.atlasNormalsTexture = atlasNormalsTexture;
			clone.atlasExtrasTexture = atlasExtrasTexture;
			clone.atlasSubsurfaceTexture = atlasSubsurfaceTexture;
			return clone;
		}
		#endregion
	}
}