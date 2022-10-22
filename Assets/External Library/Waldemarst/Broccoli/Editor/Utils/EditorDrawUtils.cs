using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Broccoli.Utils
{
	public class EditorDrawUtils {
		#region Vars
		private Dictionary<Color, Texture2D> _colorToTexture = new Dictionary<Color, Texture2D> ();
		#endregion

		#region Singleton
		/// <summary>
		/// Singleton to this instance.
		/// </summary>
		private static EditorDrawUtils _editorDrawUtils;
		/// <summary>
		/// Gets this singleton.
		/// </summary>
		/// <returns>Singleton to this instance.</returns>
		public static EditorDrawUtils GetInstance () {
			if (_editorDrawUtils == null) {
				_editorDrawUtils = new EditorDrawUtils ();
			}
			return _editorDrawUtils;
		}
		#endregion

		#region Line Drawing
		#endregion

		#region Textures
		public Texture2D GetColoredTexture (Color color) {
			if (!_colorToTexture.ContainsKey (color)) {
				_colorToTexture.Add (color, TextureUtils.ColorToTex (1, color));
			}
			return _colorToTexture [color];
		}
		#endregion
	}
}