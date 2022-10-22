using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Broccoli.Base;
using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;

namespace Broccoli.Utils
{
	public static partial class BroccoEditorGUI {
        #region Vars
		public static GUIStyle label;
		public static GUIStyle labelBold;
		public static GUIStyle labelCentered;
		public static GUIStyle labelBoldCentered;
		public static GUIStyle foldoutBold;
        #endregion

        #region Constructor
        static BroccoEditorGUI () {
            BroccoEditorGUI.Init ();
        }
        #endregion

        #region Methods
		public static bool Init () {
			RectOffset labelPadding = new RectOffset (0, 0, 3, 3);
			// Label
			label = new GUIStyle (EditorStyles.label);
			label.padding = labelPadding;
			label.wordWrap = true;
			// Label bold
			labelBold = new GUIStyle (EditorStyles.boldLabel);
			labelBold.padding = labelPadding;
			labelBold.wordWrap = true;
			// Label centered.
			labelCentered = new GUIStyle (EditorStyles.label);
			labelCentered.alignment = TextAnchor.MiddleCenter;
			labelCentered.padding = labelPadding;
			labelCentered.wordWrap = true;
			// Label bold and centered.
			labelBoldCentered = new GUIStyle (EditorStyles.boldLabel);
			labelBoldCentered.alignment = TextAnchor.MiddleCenter;
			labelBoldCentered.padding = labelPadding;
			labelBoldCentered.wordWrap = true;
			// Foldout bold.
			foldoutBold = new GUIStyle (EditorStyles.foldout);
			foldoutBold.font = labelBold.font;
			return true;
		}
		#endregion

		#region Property Fields
		/// <summary>
		/// Range slider for float min and max value properties.
		/// </summary>
		/// <param name="propMinValue">Property with the minumum value.</param>
		/// <param name="propMaxValue">Property with the maximum value.</param>
		/// <param name="minRangeValue">Minimum possible value in the range.</param>
		/// <param name="maxRangeValue">Maximum possible value in the range.</param>
		/// <param name="label">Label to display on the field.</param>
		/// <returns>True if the range was changed.</returns>
		public static bool FloatRangePropertyField (SerializedProperty propMinValue, SerializedProperty propMaxValue, float minRangeValue, float maxRangeValue, string label) {
			float minValue = propMinValue.floatValue;
			float maxValue = propMaxValue.floatValue;
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.MinMaxSlider (label, ref minValue, ref maxValue, minRangeValue, maxRangeValue);
			EditorGUILayout.LabelField (minValue.ToString("F2") + "/" + maxValue.ToString("F2"), GUILayout.Width (72));
			EditorGUILayout.EndHorizontal ();
			if (minValue != propMinValue.floatValue || maxValue != propMaxValue.floatValue) {
				propMinValue.floatValue = minValue;
				propMaxValue.floatValue = maxValue;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Range slider for float min and max value properties.
		/// </summary>
		/// <param name="minValue">Property with the minumum value.</param>
		/// <param name="maxValue">Property with the maximum value.</param>
		/// <param name="minRangeValue">Minimum possible value in the range.</param>
		/// <param name="maxRangeValue">Maximum possible value in the range.</param>
		/// <param name="label">Label to display on the field.</param>
		/// <returns>True if the range was changed.</returns>
		public static bool FloatRangePropertyField (ref float minValue, ref float maxValue, 
			float minRangeValue, float maxRangeValue, string label, int labelWidth = 72) {
			float _minValue = minValue;
			float _maxValue = maxValue;
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.MinMaxSlider (label, ref _minValue, ref _maxValue, minRangeValue, maxRangeValue);
			EditorGUILayout.LabelField (_minValue.ToString("F2") + "/" + _maxValue.ToString("F2"), GUILayout.Width (labelWidth));
			EditorGUILayout.EndHorizontal ();
			if (_minValue != minValue || _maxValue != maxValue) {
				minValue = _minValue;
				maxValue = _maxValue;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Range slider for float min and max value properties.
		/// </summary>
		/// <param name="propMinValue">Property with the minumum value.</param>
		/// <param name="propMaxValue">Property with the maximum value.</param>
		/// <param name="minRangeValue">Minimum possible value in the range.</param>
		/// <param name="maxRangeValue">Maximum possible value in the range.</param>
		/// <param name="label">Label to display on the field.</param>
		/// <returns>True if the range was changed.</returns>
		public static bool IntRangePropertyField (SerializedProperty propMinValue, SerializedProperty propMaxValue, int minRangeValue, int maxRangeValue, string label) {
			float minValue = propMinValue.intValue;
			float maxValue = propMaxValue.intValue;
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.MinMaxSlider (label, ref minValue, ref maxValue, minRangeValue, maxRangeValue);
			EditorGUILayout.LabelField (Mathf.RoundToInt (minValue) + "-" + Mathf.RoundToInt (maxValue), GUILayout.Width (60));
			EditorGUILayout.EndHorizontal ();
			if (Mathf.RoundToInt (minValue) != propMinValue.intValue || Mathf.RoundToInt (maxValue) != propMaxValue.intValue) {
				propMinValue.intValue = Mathf.RoundToInt (minValue);
				propMaxValue.intValue = Mathf.RoundToInt (maxValue);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Range slider for int min and max value properties.
		/// </summary>
		/// <param name="minValue">Property with the minumum value.</param>
		/// <param name="maxValue">Property with the maximum value.</param>
		/// <param name="minRangeValue">Minimum possible value in the range.</param>
		/// <param name="maxRangeValue">Maximum possible value in the range.</param>
		/// <param name="label">Label to display on the field.</param>
		/// <returns>True if the range was changed.</returns>
		public static bool IntRangePropertyField (ref int minValue, ref int maxValue, 
			int minRangeValue, int maxRangeValue, string label, int labelWidth = 72) {
			float _minValue = minValue;
			float _maxValue = maxValue;
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.MinMaxSlider (label, ref _minValue, ref _maxValue, minRangeValue, maxRangeValue);
			EditorGUILayout.LabelField (Mathf.RoundToInt (_minValue) + "/" + Mathf.RoundToInt (_maxValue) , GUILayout.Width (labelWidth));
			EditorGUILayout.EndHorizontal ();
			if (Mathf.RoundToInt (_minValue) != minValue || Mathf.RoundToInt (_maxValue) != maxValue) {
				minValue = Mathf.RoundToInt (_minValue);
				maxValue = Mathf.RoundToInt (_maxValue);
				return true;
			}
			return false;
		}
		#endregion
	}
}