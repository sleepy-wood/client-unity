using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Branch mapper element.
	/// </summary>
	[System.Serializable]
	public class BranchMapperElement : PipelineElement {
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
			get { return PipelineElement.ElementType.MeshTransform; }
		}
		/// <summary>
		/// Gets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public override ClassType classType {
			get { return PipelineElement.ClassType.BranchMapper; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get {
				return PipelineElement.mapperWeight;
			}
		}
		/// <summary>
		/// Material modes.
		/// </summary>
		public enum MaterialMode {
			Texture = 0,
			Custom = 1
		}
		/// <summary>
		/// Current material mode.
		/// </summary>
		public MaterialMode materialMode = MaterialMode.Texture;
		/// <summary>
		/// The custom material.
		/// </summary>
		public Material customMaterial;
		/// <summary>
		/// The main texture.
		/// </summary>
		public Texture2D mainTexture;
		/// <summary>
		/// The normal map texture.
		/// </summary>
		public Texture2D normalTexture;
		/// <summary>
		/// The mapping X displacement.
		/// </summary>
		public float mappingXDisplacement = 0f;
		/// <summary>
		/// The mapping Y displacement.
		/// </summary>
		public float mappingYDisplacement = 0f;
		/// <summary>
		/// Number of tiles for mapping in the x axis.
		/// </summary>
		public int mappingXTiles = 1;
		/// <summary>
		/// Number of tiles for mapping in the y axis.
		/// </summary>
		public int mappingYTiles = 1;
		/// <summary>
		/// The UV mapping is sensitive to branch girth changes.
		/// </summary>
		public bool isGirthSensitive = false;
		/// <summary>
		/// Color to apply to shader.
		/// </summary>
		public Color color = Color.white;
		/// <summary>
		/// Glossiness for the shader.
		/// </summary>
		public float glossiness = 0f;
		/// <summary>
		/// Metallic value for the shader.
		/// </summary>
		public float metallic = 0f;
		/// <summary>
		/// Diffusion profile to set to branch materials only when using HDRP.
		/// </summary>
		public ScriptableObject diffusionProfileSettings = null;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.BranchMapperElement"/> class.
		/// </summary>
		public BranchMapperElement () {}
		#endregion

		#region Validation
		/// <summary>
		/// Validate this instance.
		/// </summary>
		public override bool Validate () {
			log.Clear ();
			if (materialMode == MaterialMode.Custom) {
				if (customMaterial == null) {
					log.Enqueue (LogItem.GetWarnItem ("No custom material has been assigned to this mapper."));
				}
			} else {
				if (mainTexture == null) {
					log.Enqueue (LogItem.GetWarnItem ("No main texture has been assigned to this mapper."));
				}
			}
			return true;
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			BranchMapperElement clone = ScriptableObject.CreateInstance<BranchMapperElement> ();
			SetCloneProperties (clone);
			clone.materialMode = materialMode;
			clone.customMaterial = customMaterial;
			clone.mainTexture = mainTexture;
			clone.normalTexture = normalTexture;
			clone.mappingXDisplacement = mappingXDisplacement;
			clone.mappingYDisplacement = mappingYDisplacement;
			clone.mappingXTiles = mappingXTiles;
			clone.mappingYTiles = mappingYTiles;
			clone.isGirthSensitive = isGirthSensitive;
			clone.color = color;
			clone.glossiness = glossiness;
			clone.metallic = metallic;
			clone.diffusionProfileSettings = diffusionProfileSettings;
			return clone;
		}
		#endregion
	}
}