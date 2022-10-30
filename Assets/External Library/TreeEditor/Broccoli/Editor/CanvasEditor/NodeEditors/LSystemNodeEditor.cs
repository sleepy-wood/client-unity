using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Utils;
using Broccoli.Base;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// LSystem node editor.
	/// </summary>
	[CustomEditor(typeof(LSystemGraphNode))]
	public class LSystemNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The LSystem node.
		/// </summary>
		public LSystemGraphNode lSystemNode;
		/// <summary>
		/// The rules list.
		/// </summary>
		ReorderableList rulesList;
		/// <summary>
		/// The catalog list.
		/// </summary>
		ReorderableList catalogList;

		SerializedProperty propIterations;
		SerializedProperty propAxiom;
		SerializedProperty propRules;
		SerializedProperty propAccumulativeMode;
		SerializedProperty propLength;
		SerializedProperty propLengthGrowth;
		SerializedProperty propTurnAngle;
		SerializedProperty propTurnAngleGrowth;
		SerializedProperty propPitchAngle;
		SerializedProperty propPitchAngleGrowth;
		SerializedProperty propRollAngle;
		SerializedProperty propRollAngleGrowth;

		/// <summary>
		/// Enables the advanced rules editor (experimental and dangerous).
		/// </summary>
		static bool advancedRulesEditor = false;
		#endregion

		#region LSystem Catalog Item Class
		//TODO: Comment
		public class LSystemCatalogItem 
		{
			public string name = "";
			public int id = -1;
			public int iterations = 2;
			public int minIterations = 0;
			public int maxIterations = 4;
			public bool accumulativeMode = false;
			public string axiom = "F";
			public List<LSystem.Rule> rules = new List<LSystem.Rule> ();
			public float lengthGrowth     = -1.5f;
			public float turnAngle        = 30f;
			public float turnAngleGrowth  = 0f;
			public float pitchAngle       = 30f;
			public float pitchAngleGrowth = 0f;
			public float rollAngle        = 30f;
			public float rollAngleGrowth  = 0f;

		}
		#endregion

		#region LSystem Catalog
		/// <summary>
		/// The Liendenmayer catalog.
		/// </summary>
		public List<LSystemCatalogItem> lSystemCatalog = new List<LSystemCatalogItem> {
			new LSystemCatalogItem {name = "Hierarchy A", id = 0, iterations = 3, turnAngle = 90f, rollAngle = 40f,
				rules = new List<LSystem.Rule> {
					new LSystem.Rule {symbol = "F", rule = "F[/F][\\F]", probability = 0.25f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F][+\\F]", probability = 0.5f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+\\F]", probability = 0.125f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F]", probability = 0.125f, probabilityType = LSystem.Rule.ProbabilityType.Fixed}
				}},
			new LSystemCatalogItem {name = "Hierarchy B", id = 1, axiom = "FF", iterations = 3, turnAngle = 70f, rollAngle = 30f,
				rules = new List<LSystem.Rule> {
					new LSystem.Rule {symbol = "F", rule = "F[/F][\\F][\\\\F]", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F][+\\F][++\\F]", probability = 0.5f, fromIteration = 2, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+\\F]", probability = 0.125f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F]", probability = 0.125f, probabilityType = LSystem.Rule.ProbabilityType.Fixed}
				}},
			new LSystemCatalogItem {name = "Hierarchy C",id = 2, axiom = "FGG", iterations = 3, turnAngle = 85f, rollAngle = 30f, accumulativeMode = true,
				rules = new List<LSystem.Rule> {
					new LSystem.Rule {symbol = "F", rule = "F[/F][\\F]", probability = 0.5f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F][+\\F]", probability = 0.5f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+\\F]", probability = 0.5f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F]", probability = 0.5f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed}
				}},
			new LSystemCatalogItem {name = "Hierarchy D", id = 3, iterations = 3, turnAngle = 90f, rollAngle = 30f,
				rules = new List<LSystem.Rule> {
					new LSystem.Rule {symbol = "F", rule = "F[/F][\\F]F", probability = 0.25f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F][+\\F]F", probability = 0.5f, toIteration = 0, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+\\F]F", probability = 0.125f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F]F", probability = 0.125f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[//F][\\\\F]F", probability = 0.75f, toIteration = 0, probabilityType = LSystem.Rule.ProbabilityType.Fixed}
				}},
			new LSystemCatalogItem {name = "Hierarchy E", id = 4, iterations = 3, turnAngle = 90f, rollAngle = 30f,
				rules = new List<LSystem.Rule> {
					new LSystem.Rule {symbol = "F", rule = "+F[\\F]F", probability = 0.2f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "+F[+\\F]F", probability = 0.1f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+\\F]F", probability = 0.1f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[\\F]F", probability = 0.1f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "+F[/F]F", probability = 0.1f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "+F[+/F]F", probability = 0.1f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F]F", probability = 0.1f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "+\\FF", probability = 0.1f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "+/FF", probability = 0.1f, probabilityType = LSystem.Rule.ProbabilityType.Fixed}
				}},
			new LSystemCatalogItem {name = "Hierarchy F", id = 5, axiom = "G", iterations = 3, turnAngle = 70f, rollAngle = 25f, accumulativeMode = true,
				rules = new List<LSystem.Rule> {
					new LSystem.Rule {symbol = "G", rule = "/F\\F", probability = 0.5f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "G", rule = "+/F+\\F", probability = 0.5f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "\\F+\\F", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F+/F", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F\\F", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[\\F]", probability = 0.25f, fromIteration = 2, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[/F]", probability = 0.25f, fromIteration = 2, probabilityType = LSystem.Rule.ProbabilityType.Fixed}
				}},
			new LSystemCatalogItem {name = "Hierarchy G", id = 6, axiom = "F", iterations = 3, turnAngle = 30f, rollAngle = 35f, accumulativeMode = true,
				rules = new List<LSystem.Rule> {
					new LSystem.Rule {symbol = "F", rule = "F[/F]A[\\F]", probability = 0.334f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[%F]A[&F]", probability = 0.333f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[/FF]A[\\FF]", probability = 0.333f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "A", rule = "FD", probability = 1f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "D", rule = "F", probability = 1f, probabilityType = LSystem.Rule.ProbabilityType.Fixed}
				}},
			new LSystemCatalogItem {name = "Hierarchy H", id = 7, axiom = "FA", iterations = 3, turnAngle = 30f, rollAngle = 35f, accumulativeMode = true,
				rules = new List<LSystem.Rule> {
					new LSystem.Rule {symbol = "F", rule = "F[/F]A[\\F]", probability = 0.3334f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[%F]A[&F]", probability = 0.333f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F/FFA\\FF", probability = 0.333f, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "A", rule = "FD", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "D", rule = "F", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "[/F][\\F]F", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "[%F][&F]F", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed}
				}},
			new LSystemCatalogItem {name = "Hierarchy I", id = 8, axiom = "G", iterations = 4, turnAngle = 70f, rollAngle = 25f, accumulativeMode = true,
				rules = new List<LSystem.Rule> {
					new LSystem.Rule {symbol = "G", rule = "[/F][\\F][\\\\F]", probability = 0.25f, toIteration = 0, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "G", rule = "[+/F][+\\F][++\\F]", probability = 0.25f, toIteration = 0, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "G", rule = "[/F][\\F][\\\\F]", probability = 0.25f, toIteration = 0, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "G", rule = "[/F][\\F][+F][+\\\\F]", probability = 0.25f, toIteration = 0, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+\\F]", probability = 0.5f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F]", probability = 0.5f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed}
				}},
			new LSystemCatalogItem {name = "Hierarchy J", id = 9, axiom = "G", iterations = 3, turnAngle = 90f, rollAngle = 20f, accumulativeMode = false,
				rules = new List<LSystem.Rule> {
					new LSystem.Rule {symbol = "G", rule = "[/F][\\F][\\\\F]", probability = 0.25f, toIteration = 0, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "G", rule = "[+/F][+\\F][++\\F]", probability = 0.25f, toIteration = 0, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "G", rule = "[/F][\\F][\\\\F]", probability = 0.25f, toIteration = 0, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "G", rule = "[/F][\\F][+F][+\\\\F]", probability = 0.25f, toIteration = 0, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+\\F]", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F]", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[/F][+/F]", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed},
					new LSystem.Rule {symbol = "F", rule = "F[+/F][+/F]", probability = 0.25f, fromIteration = 1, probabilityType = LSystem.Rule.ProbabilityType.Fixed}
				}}
		};
		/// <summary>
		/// Callback to a selected catalog item.
		/// </summary>
		/// <param name="index">Index.</param>
		void CatalogItemSelected (object index) {
			int selectedIndex = (int)index;
			LSystemCatalogItem item = lSystemCatalog [selectedIndex];

			lSystemNode.lSystemElement.name = item.name;
			lSystemNode.lSystemElement.accumulativeMode = item.accumulativeMode;
			lSystemNode.lSystemElement.iterations = item.iterations;
			lSystemNode.lSystemElement.minIterations = item.minIterations;
			lSystemNode.lSystemElement.maxIterations = item.maxIterations;
			lSystemNode.lSystemElement.axiom = item.axiom;
			lSystemNode.lSystemElement.rules.Clear ();
			for (int i = 0; i < item.rules.Count; i++) {
				lSystemNode.lSystemElement.rules.Add (item.rules[i].Clone ());
			}
			lSystemNode.lSystemElement.lengthGrowth     = item.lengthGrowth;
			lSystemNode.lSystemElement.turnAngle        = item.turnAngle;
			lSystemNode.lSystemElement.turnAngleGrowth  = item.turnAngleGrowth;
			lSystemNode.lSystemElement.pitchAngle       = item.pitchAngle;
			lSystemNode.lSystemElement.pitchAngleGrowth = item.pitchAngleGrowth;
			lSystemNode.lSystemElement.rollAngle        = item.rollAngle;
			lSystemNode.lSystemElement.rollAngleGrowth  = item.rollAngleGrowth;

			lSystemNode.lSystemElement.requiresNewStructure = true;

			UpdatePipeline (GlobalSettings.processingDelayHigh);
		}
		#endregion

		#region Messages
		private static string MSG_ITERATIONS = "Iterations determine the number of " +
			"branch levels the tree structure will reach. For each iteration replacement " +
			"rules are applied on the string that will generate the branches.";
		/*
		private static string MSG_AXIOM = "String to start the iterations on. It is possible " +
			"to start with a complex string already representing a tree structure.";
		private static string MSG_RULES = "Rules to be applied on the axiom and resulting " +
			"strings every iteration. Symbols are replaced with the rules equivalence.";
		*/
		private static string MSG_ACCUM_MODE = "This mode adds the parent branch angle to its children.";
		private static string MSG_LENGTH = "Length of the branches generated by the L-System.";
		private static string MSG_LENGTH_GROWTH = "Branch length increases or decreases with each " +
			"iteration level. Rule symbols are '>' to decrease and '<' to increase.";
		private static string MSG_TURN_ANGLE = "Turn angle of the branches taking the 'up' axis as " +
			"pivot, the '+' symbol adds to the angle and the '-' symbol substracts to it.";
		private static string MSG_TURN_ANGLE_GROWTH = "Turn angle increases or decreases with each " +
			"iteration level. Rule symbols are ')' to increase angle and '(' to decrease.";
		private static string MSG_PITCH_ANGLE = "Pitch angle of the branches taking the 'right' axis as " +
			"pivot, the '&' symbol adds to the angle and the '^' symbol substracts to it.";
		private static string MSG_PITCH_ANGLE_GROWTH = "Pitch angle increases or decreases with each " +
			"iteration level. Rule symbols are ')' to increase angle and '(' to decrease.";
		private static string MSG_ROLL_ANGLE = "Turn angle of the branches taking the 'forward' axis as " +
			"pivot, the '\\' symbol adds to the angle and the '/' symbol substracts to it.";
		private static string MSG_ROLL_ANGLE_GROWTH = "Roll angle increases or decreases with each " +
			"iteration level. Rule symbols are ')' to increase angle and '(' to decrease.";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			lSystemNode = target as LSystemGraphNode;

			SetPipelineElementProperty ("lSystemElement");
			propIterations = GetSerializedProperty ("iterations");
			if (advancedRulesEditor) {
				propAxiom = GetSerializedProperty ("axiom");
				propRules = GetSerializedProperty ("rules");
				rulesList = new ReorderableList (serializedObject, propRules, true, true, true, true);
				rulesList.drawHeaderCallback += DrawListItemHeader;
				rulesList.drawElementCallback += DrawListItemElement;
				rulesList.onAddCallback += AddListItem;
				rulesList.onRemoveCallback += RemoveListItem;
			}
			propAccumulativeMode = GetSerializedProperty ("accumulativeMode");
			propLength = GetSerializedProperty ("length");
			propLengthGrowth = GetSerializedProperty ("lengthGrowth");
			propTurnAngle = GetSerializedProperty ("turnAngle");
			propTurnAngleGrowth = GetSerializedProperty ("turnAngleGrowth");
			propPitchAngle = GetSerializedProperty ("pitchAngle");
			propPitchAngleGrowth = GetSerializedProperty ("pitchAngleGrowth");
			propRollAngle = GetSerializedProperty ("rollAngle");
			propRollAngleGrowth = GetSerializedProperty ("rollAngleGrowth");
		}
		/// <summary>
		/// Raises the disable event.
		/// </summary>
		void OnDisable () {
			if (advancedRulesEditor) {
				rulesList.drawHeaderCallback -= DrawListItemHeader;
				rulesList.drawElementCallback -= DrawListItemElement;
				rulesList.onAddCallback -= AddListItem;
				rulesList.onRemoveCallback -= RemoveListItem;
			}
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {

			CheckUndoRequest ();

			UpdateSerialized ();

			// LOG
			DrawLogBox();

			EditorGUILayout.Space ();

			// CATALOG
			if (GUILayout.Button ("Select Pattern")) {
				GenericMenu menu = new GenericMenu();
				for (int i = 0; i < lSystemCatalog.Count; i++) {
					menu.AddItem (new GUIContent (lSystemCatalog[i].name), true, CatalogItemSelected, i);
				}
				menu.ShowAsContext();
			}
			EditorGUILayout.Space ();

			EditorGUILayout.LabelField ("Base Pattern", lSystemNode.lSystemElement.name);
			EditorGUILayout.Space ();

			EditorGUI.BeginChangeCheck ();

			// ITERATIONS
			int iterations = propIterations.intValue;
			EditorGUILayout.IntSlider (propIterations, 
				lSystemNode.lSystemElement.minIterations, 
				lSystemNode.lSystemElement.maxIterations, "Iterations");
			ShowHelpBox (MSG_ITERATIONS);
			EditorGUILayout.Space ();

			if (advancedRulesEditor) {
				// AXIOM
				EditorGUILayout.DelayedTextField (propAxiom);
				EditorGUILayout.Space ();

				// RULES
				rulesList.DoLayoutList ();
			}
		
			propAccumulativeMode.boolValue = 
				EditorGUILayout.Toggle ("Acumulative Mode", propAccumulativeMode.boolValue);
			ShowHelpBox (MSG_ACCUM_MODE);
			EditorGUILayout.Space ();

			EditorGUILayout.Slider (propLength, 0.1f, 50f, "Length");
			ShowHelpBox (MSG_LENGTH);
			EditorGUILayout.Space ();

			EditorGUILayout.Slider (propLengthGrowth, -1f, 1f, "Length Growth");
			ShowHelpBox (MSG_LENGTH_GROWTH);
			EditorGUILayout.Space ();

			EditorGUILayout.Slider (propTurnAngle, -180f, 180f, "Turn Angle");
			ShowHelpBox (MSG_TURN_ANGLE);
			EditorGUILayout.Space ();

			EditorGUILayout.Slider (propTurnAngleGrowth, -1f, 1f, "Turn Angle Growth");
			ShowHelpBox (MSG_TURN_ANGLE_GROWTH);
			EditorGUILayout.Space ();

			EditorGUILayout.Slider (propPitchAngle, -180f, 180f, "Pitch Angle");
			ShowHelpBox (MSG_PITCH_ANGLE);
			EditorGUILayout.Space ();

			EditorGUILayout.Slider (propPitchAngleGrowth, -1f, 1f, "Pitch Angle Growth");
			ShowHelpBox (MSG_PITCH_ANGLE_GROWTH);
			EditorGUILayout.Space ();

			EditorGUILayout.Slider (propRollAngle, -180f, 180f, "Roll Angle");
			ShowHelpBox (MSG_ROLL_ANGLE);
			EditorGUILayout.Space ();

			EditorGUILayout.Slider (propRollAngleGrowth, -1f, 1f, "Roll Angle Growth");
			ShowHelpBox (MSG_ROLL_ANGLE_GROWTH);
			EditorGUILayout.Space ();

			// Seed options.
			DrawSeedOptions ();

			if (EditorGUI.EndChangeCheck ()) {
				ApplySerialized ();
				if (iterations != propIterations.intValue) {
					lSystemNode.lSystemElement.requiresNewStructure = true;
				}
				UpdatePipeline (GlobalSettings.processingDelayHigh);
				SetUndoControlCounter ();
			}

			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		#endregion

		#region Rules Ordereable List
		/// <summary>
		/// Draws the list item header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawListItemHeader(Rect rect)
		{
			GUI.Label(rect, "Rules");
		}
		/// <summary>
		/// Draws the list item element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			var rule = rulesList.serializedProperty.GetArrayElementAtIndex (index);
			rect.y += 2;

			// Enabled
			EditorGUI.PropertyField (
				new Rect (rect.x, rect.y, 18, EditorGUIUtility.singleLineHeight),
				rule.FindPropertyRelative ("enabled"), GUIContent.none);

			// Symbol
			EditorGUI.BeginChangeCheck ();
			EditorGUI.DelayedTextField(
				new Rect (rect.x + 18, rect.y, 18, EditorGUIUtility.singleLineHeight),
				rule.FindPropertyRelative ("symbol"), GUIContent.none);
			if (EditorGUI.EndChangeCheck ()) {
				if (rule.FindPropertyRelative ("symbol").stringValue.Length > 1) {
					rule.FindPropertyRelative ("symbol").stringValue = 
						rule.FindPropertyRelative ("symbol").stringValue.Substring (0, 1);
				}
			}

			// Rule
			EditorGUI.DelayedTextField (
				new Rect (rect.x + 40, rect.y, 150, EditorGUIUtility.singleLineHeight),
				rule.FindPropertyRelative ("rule"), GUIContent.none);

			// Probability type
			float rightSpace = 92f;
			EditorGUI.PropertyField(
				new Rect (rect.width - rightSpace, rect.y, 45, EditorGUIUtility.singleLineHeight),
				rule.FindPropertyRelative ("probabilityType"), GUIContent.none);

			// Probability
			rightSpace -= 40f;
			EditorGUI.BeginChangeCheck ();
			EditorGUI.DelayedFloatField (
				new Rect (rect.width - rightSpace, rect.y, 35, EditorGUIUtility.singleLineHeight),
				rule.FindPropertyRelative ("probability"), GUIContent.none);
			if (EditorGUI.EndChangeCheck ()) {
				float stochasticValue = rule.FindPropertyRelative ("probability").floatValue;
				if (stochasticValue < 0f) {
					rule.FindPropertyRelative ("probability").floatValue = 0f;
				} else if (stochasticValue > 1f) {
					rule.FindPropertyRelative ("probability").floatValue = 1f;
				}
			}

			// From Iteration
			rightSpace -= 39f;
			EditorGUI.BeginChangeCheck ();
			EditorGUI.DelayedIntField (
				new Rect (rect.width - rightSpace, rect.y, 18, EditorGUIUtility.singleLineHeight),
				rule.FindPropertyRelative ("fromIteration"), GUIContent.none);
			if (EditorGUI.EndChangeCheck ()) {
				float fromIterationValue = rule.FindPropertyRelative ("fromIteration").intValue;
				if (fromIterationValue < 0) {
					rule.FindPropertyRelative ("fromIteration").intValue = 0;
				}
			}

			// To Iteration
			rightSpace -= 22f;
			EditorGUI.BeginChangeCheck ();
			EditorGUI.DelayedIntField (
				new Rect (rect.width - rightSpace, rect.y, 18, EditorGUIUtility.singleLineHeight),
				rule.FindPropertyRelative ("toIteration"), GUIContent.none);
			if (EditorGUI.EndChangeCheck ()) {
				float toIterationValue = rule.FindPropertyRelative ("toIteration").intValue;
				if (toIterationValue < -1) {
					rule.FindPropertyRelative ("toIteration").intValue = -1;
				}
			}
		}
		/// <summary>
		/// Adds the list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void AddListItem(ReorderableList list)
		{
			lSystemNode.lSystemElement.rules.Add(new LSystem.Rule());
			EditorUtility.SetDirty(target);
		}
		/// <summary>
		/// Removes the list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void RemoveListItem(ReorderableList list)
		{
			lSystemNode.lSystemElement.rules.RemoveAt(list.index);
			EditorUtility.SetDirty(target);
		}
		#endregion
	}
}
