using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Sprout map.
	/// Used to apply texture and materials to sprouts.
	/// </summary>
	[System.Serializable]
	public class SproutMap {
		#region SproutMapArea class
		/// <summary>
		/// Sprout area.
		/// Crop and offset for a texture to assign maps on sprouts.
		/// </summary>
		[System.Serializable]
		public class SproutMapArea
		{
			/// <summary>
			/// True if this map area is enabled.
			/// </summary>
			public bool enabled = true;
			/// <summary>
			/// The x offset (0 to 1).
			/// </summary>
			public float x = 0f;
			/// <summary>
			/// The y offset (0 to 1).
			/// </summary>
			public float y = 0f;
			/// <summary>
			/// The width of the area.
			/// </summary>
			public float width = 1f;
			/// <summary>
			/// The height of the area.
			/// </summary>
			public float height = 1f;
			/// <summary>
			/// Diagonal length.
			/// </summary>
			public float diagonal {
				get { return Mathf.Sqrt (height * height + width * width); }
			}
			/// <summary>
			/// The pivot x coordinate of the map.
			/// </summary>
			public float pivotX = 0.5f;
			/// <summary>
			/// The pivot y coordinate of the map.
			/// </summary>
			public float pivotY = 0f;
			/// <summary>
			/// The normalized step.
			/// Number of rotations (steps) to normalize the
			/// area to face the sprout origin.
			/// </summary>
			[System.NonSerialized]
			public int normalizedStep = 0;
			/// <summary>
			/// The normalized x offset after the rotation
			/// applied to the area to face the sprout origin.
			/// </summary>
			[System.NonSerialized]
			public float normalizedX = 0f;
			/// <summary>
			/// The normalized y offset after the rotation
			/// applied to the area to face the sprout origin.
			/// </summary>
			[System.NonSerialized]
			public float normalizedY = 0f;
			/// <summary>
			/// The width of the area after the rotation
			/// applied to it to face the sprout origin.
			/// </summary>
			[System.NonSerialized]
			public float normalizedWidth = 1f;
			/// <summary>
			/// The height of the area after the rotation
			/// applied to it to face the sprout origin.
			/// </summary>
			[System.NonSerialized]
			public float normalizedHeight = 1f;
			/// <summary>
			/// The normalized pivot x coordinate after the rotation
			/// applied to the area to face the sprout origin.
			/// </summary>
			[System.NonSerialized]
			public float normalizedPivotX = 0.5f;
			/// <summary>
			/// The normalized pivot y coordinate after the rotation
			/// applied to the area to face the sprout origin..
			/// </summary>
			[System.NonSerialized]
			public float normalizedPivotY = 0f;
			/// <summary>
			/// The normalized width in pixels.
			/// </summary>
			[System.NonSerialized]
			public int normalizedWidthPx = 1;
			/// <summary>
			/// The normalized height in pixels.
			/// </summary>
			[System.NonSerialized]
			public int normalizedHeightPx = 1;
			/// <summary>
			/// The texture for this area.
			/// </summary>
			public Texture2D texture;
			/// <summary>
			/// The normal map.
			/// </summary>
			public Texture2D normalMap;
			/// <summary>
			/// The extra map.
			/// </summary>
			public Texture2D extraMap;
			/// <summary>
			/// The subsurface map.
			/// </summary>
			public Texture2D subsurfaceMap;
			/// <summary>
			/// Validate this area.
			/// </summary>
			public void Validate () {
				if (x > 1) {
					x = 1;
				} else if (x < 0) {
					x = 0;
				}
				if (y > 1) {
					y = 1;
				} else if (y < 0) {
					y = 0;
				}
				if (x + width > 1) {
					width = 1f - x;
				} else if (x + width < 0) {
					x = 0;
					width = 1f;
				}
				if (y + height > 1) {
					height = 1f - y;
				} else if (y + height < 0) {
					y = 0;
					height = 1f;
				}
				if (pivotX > 1) {
					pivotX = 1;
				} else if (pivotX < 0) {
					pivotX = 0;
				}
				if (pivotY > 1) {
					pivotY = 1;
				} else if (pivotY < 0) {
					pivotY = 0;
				}
			}
			/// <summary>
			/// Normalizes this instance values.
			/// </summary>
			public void Normalize () {
				float sin = Mathf.Sin (-Mathf.PI / 4f);
				float cos = Mathf.Cos (-Mathf.PI / 4f);
				float xPos = (pivotX - 0.5f) * cos - (pivotY - 0.5f) * sin;
				float yPos = (pivotX - 0.5f) * sin + (pivotY - 0.5f) * cos;
				if (xPos > 0) {
					if (yPos > 0) {
						normalizedStep = 2;
						normalizedX = 1f - x;
						normalizedY = 1f - y;
						normalizedWidth = width;
						normalizedHeight = height;
						normalizedPivotX = 1f - pivotX;
						normalizedPivotY = 1f - pivotY;
					} else {
						normalizedStep = 1;
						normalizedX = y;
						normalizedY = 1f - (x + width);
						normalizedWidth = height;
						normalizedHeight = width;
						normalizedPivotX = pivotY;
						normalizedPivotY = 1f - pivotX;
					}
				} else {
					if (yPos > 0) {
						normalizedStep = 3;
						normalizedX = 1f - (y + height);
						normalizedY = x;
						normalizedWidth = height;
						normalizedHeight = width;
						normalizedPivotX = 1f - pivotY;
						normalizedPivotY = pivotX;
					} else {
						normalizedStep = 0;
						normalizedX = x;
						normalizedY = y;
						normalizedWidth = width;
						normalizedHeight = height;
						normalizedPivotX = pivotX;
						normalizedPivotY = pivotY;
					}
				}
				if (texture != null) {
					if (normalizedStep == 0 || normalizedStep == 2) {
						normalizedWidthPx = Mathf.RoundToInt (texture.width * width);
						normalizedHeightPx = Mathf.RoundToInt (texture.height * height);
					} else {
						normalizedWidthPx = Mathf.RoundToInt (texture.height * height);
						normalizedHeightPx = Mathf.RoundToInt (texture.width * width);
					}
				} else {
					normalizedWidthPx = 1;
					normalizedHeightPx = 1;
				}
			}
			/// <summary>
			/// Clone this instance.
			/// </summary>
			public SproutMapArea Clone () {
				SproutMapArea clone = new SproutMapArea ();
				clone.enabled = enabled;
				clone.x = x;
				clone.y = y;
				clone.width = width;
				clone.height = height;
				clone.pivotX = pivotX;
				clone.pivotY = pivotY;
				clone.texture = texture;
				clone.normalMap = normalMap;
				clone.extraMap = extraMap;
				clone.subsurfaceMap = subsurfaceMap;
				return clone;
			}
		}
		#endregion

		#region Vars
		/// <summary>
		/// The sprout group id this map apply to.
		/// </summary>
		public int groupId = 0;
		/// <summary>
		/// Mapping modes.
		/// </summary>
		public enum Mode
		{
			Texture,
			Material,
			MaterialOverride
		}
		/// <summary>
		/// The mapping mode.
		/// </summary>
		public Mode mode = Mode.Texture;
		/// <summary>
		/// The custom material to use on this mapper.
		/// </summary>
		public Material customMaterial;
		/// <summary>
		/// The color value for the material.
		/// </summary>
		public Color color = Color.white;
		/// <summary>
		/// The color value for the material subsurface.
		/// </summary>
		public Color subsurfaceColor = new Color (0.75f, 0.75f, 0.75f, 1f);
		/// <summary>
		/// Subsurface value from 0 to 1.
		/// </summary>
		public float subsurfaceValue = 0.75f;
		/// <summary>
		/// Glossiness value.
		/// </summary>
		public float glossiness = 0f;
		/// <summary>
		/// Metallic value.
		/// </summary>
		public float metallic = 0.75f;
		/// <summary>
		/// Diffusion profile to set to branch materials only when using HDRP.
		/// </summary>
		public ScriptableObject diffusionProfileSettings = null;
		/// <summary>
		/// The transparency value for the material.
		/// </summary>
		public Color transparency = Color.Lerp (Color.green, Color.black, 0.6f);
		/// <summary>
		/// The alpha cutoff value for the material.
		/// </summary>
		[Range (0f, 1f)]
		public float alphaCutoff = 0.3f;
		/// <summary>
		/// The translucency view dependency value for the material.
		/// </summary>
		[Range (0f, 1f)]
		public float translucencyViewDependency = 0.7f;
		/// <summary>
		/// The shadow strength value for the material.
		/// </summary>
		[Range (0f, 1f)]
		public float shadowStrength = 0.8f;
		/// <summary>
		/// The shadow offset scale value for the material.
		/// </summary>
		public float shadowOffsetScale = 1f;
		/// <summary>
		/// Modes available to apply color variance to sprout meshes.
		/// </summary>
		public enum ColorVarianceMode {
			None,
			Shades
		}
		/// <summary>
		/// Current variance mode to apply to sprout meshes.
		/// </summary>
		public ColorVarianceMode colorVarianceMode = ColorVarianceMode.None;
		/// <summary>
		/// Minimum value for shading. The lower the darker.
		/// </summary>
		public float minColorShade = 0.78f;
		/// <summary>
		/// Max value for shading. The lower the darker.
		/// </summary>
		public float maxColorShade = 0.95f;
		/// <summary>
		/// Enable color tint.
		/// </summary>
		public bool colorTintEnabled = false;
		/// <summary>
		/// Tint to apply.
		/// </summary>
		public Color colorTint = Color.white;
		/// <summary>
		/// Minimum value for tint. The lower the most loyal to the original color.
		/// </summary>
		public float minColorTint = 0f;
		/// <summary>
		/// Max value for tint. The lower the most loyal to the original color.
		/// </summary>
		public float maxColorTint = 0.5f;
		/// <summary>
		/// Mode to select if the textures will come from each area or from the whole group.
		/// </summary>
		public enum TextureMode {
			PerArea,
			PerGroup
		}
		/// <summary>
		/// Texture mode to switch taken textures from each area of from the whole group.
		/// </summary>
		public TextureMode textureMode = TextureMode.PerArea;
		/// <summary>
		/// The texture for this area.
		/// </summary>
		public Texture2D texture;
		/// <summary>
		/// The normal map.
		/// </summary>
		public Texture2D normalMap;
		/// <summary>
		/// The extra map.
		/// </summary>
		public Texture2D extraMap;
		/// <summary>
		/// The subsurface map.
		/// </summary>
		public Texture2D subsurfaceMap;
		/// <summary>
		/// Areas used on this map.
		/// </summary>
		public List<SproutMapArea> sproutAreas = new List<SproutMapArea> ();
		/// <summary>
		/// If true UV0 is set to use Vector4 values, with atlas coordinates on xy and area coordinates on zw.
		/// </summary>
		public bool vector4UV0Enabled = false;
		/// <summary>
		/// The index of the selected map area.
		/// </summary>
		public int selectedAreaIndex = -1;
		#endregion

		#region Normalize
		/// <summary>
		/// Normalizes the areas.
		/// </summary>
		public void NormalizeAreas () {
			for (int i = 0; i < sproutAreas.Count; i++) {
				sproutAreas[i].Normalize ();
			}
		}
		#endregion

		#region Utils
		/// <summary>
		/// Determines whether this mapper uses a base material.
		/// </summary>
		/// <returns><c>true</c> if this instance is material mode; otherwise, <c>false</c>.</returns>
		public bool IsMaterialMode () {
			return (this.mode == Mode.Material || this.mode == Mode.MaterialOverride);
		}
		/// <summary>
		/// Determines whether this instance uses a texture array.
		/// </summary>
		/// <returns><c>true</c> if this instance is textured; otherwise, <c>false</c>.</returns>
		public bool IsTextured () {
			return (this.mode == Mode.Texture || this.mode == Mode.MaterialOverride);
		}
		/// <summary>
		/// Gets the first valid sprout map area available.
		/// </summary>
		/// <returns>Sprout map area available or null.</returns>
		public SproutMap.SproutMapArea GetMapArea () {
			for (int i = 0; i < sproutAreas.Count; i++) {
				if (sproutAreas[i].enabled) {
					return sproutAreas [i];
				}
			}
			return null;
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		public SproutMap Clone () {
			SproutMap clone = new SproutMap();
			clone.groupId = groupId;
			clone.mode = mode;
			clone.customMaterial = customMaterial;
			clone.color = color;
			clone.transparency = transparency;
			clone.alphaCutoff = alphaCutoff;
			clone.translucencyViewDependency = translucencyViewDependency;
			clone.shadowStrength = shadowStrength;
			clone.shadowOffsetScale = shadowOffsetScale;
			clone.colorVarianceMode = colorVarianceMode;
			clone.minColorShade = minColorShade;
			clone.maxColorShade = maxColorShade;
			clone.colorTintEnabled = colorTintEnabled;
			clone.colorTint = colorTint;
			clone.minColorTint = minColorTint;
			clone.maxColorTint = maxColorTint;
			clone.textureMode = textureMode;
			clone.texture = texture;
			clone.normalMap = normalMap;
			clone.extraMap = extraMap;
			clone.subsurfaceMap = subsurfaceMap;
			clone.sproutAreas.Clear ();
			for (int i = 0; i < sproutAreas.Count; i++) {
				clone.sproutAreas.Add (sproutAreas[i].Clone ());
			}
			clone.vector4UV0Enabled = vector4UV0Enabled;
			clone.selectedAreaIndex = selectedAreaIndex;
			clone.subsurfaceColor = subsurfaceColor;
			clone.subsurfaceValue = subsurfaceValue;
			clone.glossiness = glossiness;
			clone.metallic = metallic;
			clone.diffusionProfileSettings = diffusionProfileSettings;
			return clone;
		}
		#endregion
	}
}