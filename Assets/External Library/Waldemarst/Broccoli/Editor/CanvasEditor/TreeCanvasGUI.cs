using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Class for nodes GUI style properties.
	/// </summary>
	public static class TreeCanvasGUI {
		/// <summary>
		/// Defaut color of the text.
		/// </summa-ry>
		public static Color textColor = new Color (0.6f, 0.6f, 0.6f);
		/// <summary>
		/// The color of the text light.
		/// </summary>
		public static Color textLightColor = new Color (0.8f, 0.8f, 0.8f);
		public static Color neutralColor = new Color (0.2f, 0.2f, 0.2f);

		public static Color structureGeneratorNodeColor = new Color (0f, 0.32f, 0.44f); // Blue.
		public static Color structureTransformerNodeColor = new Color (0.2f, 0.50f, 0.50f); // Light blue.
		public static Color meshGeneratorNodeColor = new Color (0.53f, 0.03f, 0.19f); // Pomegranade.
		public static Color meshTransformerNodeColor = new Color (0.63f, 0.18f, 0.29f); // Light pomegranade.
		public static Color mapperNodeColor = new Color (0.8f, 0.51f, 0.05f); // Mustard.
		public static Color functionNodeColor = new Color (0.27f, 0.28f, 0.5f); // Purple.

		public static Color inactiveNodeOutputColor = Color.gray;
		public static Color activeNodeOutputColor = new Color(0.77f, 0.77f, 0.77f);
		public static Color connectDotColor = new Color(1f, 0.4f, 0.4f);
		/// <summary>
		/// Default label style.
		/// </summary>
		public static GUIStyle nodeLabel;
		/// <summary>
		/// Small node label style.
		/// </summary>
		public static GUIStyle smallNodeLabel;
		/// <summary>
		/// Header label style.
		/// </summary>
		public static GUIStyle nodeHeaderLabel;
		/// <summary>
		/// Selected header label style.
		/// </summary>
		public static GUIStyle nodeHeaderSelectedLabel;
		private static GUIStyle _verticalScrollStyle = null;
		public static GUIStyle verticalScrollStyle {
			get {
				if (_verticalScrollStyle == null) {
					_verticalScrollStyle = new GUIStyle (GUI.skin.verticalScrollbar);
				}
				return _verticalScrollStyle;
			}
		}
		private static GUIStyle _catalogItemStyle = null;
		public static GUIStyle catalogItemStyle {
			get {
				if (_catalogItemStyle == null) {
					_catalogItemStyle = new GUIStyle (GUI.skin.button);
					_catalogItemStyle.imagePosition = ImagePosition.ImageAbove;
					_catalogItemStyle.fixedHeight = 100;
				}
				return _catalogItemStyle;
			}
		}
		private static GUIStyle _catalogCategoryStyle = null;
		public static GUIStyle catalogCategoryStyle {
			get {
				if (_catalogCategoryStyle == null) {
					_catalogCategoryStyle = new GUIStyle (EditorStyles.boldLabel);
					_catalogCategoryStyle.normal.textColor = Color.white;
				}
				return _catalogCategoryStyle;
			}
		}
		private static GUIStyle _nodeEditorButtonStyle = null;
		public static GUIStyle nodeEditorButtonStyle {
			get {
				if (_nodeEditorButtonStyle == null) {
					_nodeEditorButtonStyle = new GUIStyle (GUI.skin.button);
					_nodeEditorButtonStyle.alignment = TextAnchor.MiddleLeft;
				}
				return _nodeEditorButtonStyle;
			}
		}
		static TreeCanvasGUI() {
			nodeLabel = new GUIStyle ();
			nodeLabel.fontSize = 10;
			nodeLabel.normal.textColor = textColor;
			smallNodeLabel = new GUIStyle (nodeLabel);
			smallNodeLabel.fontSize = 9;
			smallNodeLabel.normal.textColor = textLightColor;
			nodeHeaderLabel = new GUIStyle (nodeLabel);
			nodeHeaderLabel.fontSize = 11;
			nodeHeaderLabel.normal.textColor = textLightColor;
			nodeHeaderLabel.alignment = TextAnchor.MiddleLeft;
			nodeHeaderSelectedLabel = new GUIStyle (nodeHeaderLabel);
			nodeHeaderSelectedLabel.fontStyle = FontStyle.Bold;
		}
	}
}