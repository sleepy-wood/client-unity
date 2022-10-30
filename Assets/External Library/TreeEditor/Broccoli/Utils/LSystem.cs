using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace Broccoli.Utils
{
	/// <summary>
	/// Lindenmayer system to generate fractal structures.
	/// </summary>
	public class LSystem
	{
		/// <summary>
		/// Upward pointing vector.
		/// </summary>
		public static Vector3 upward = Vector3.up;
		/// <summary>
		/// Forward pointing vector.
		/// </summary>
		public static Vector3 forward = Vector3.forward;
		/// <summary>
		/// The right pointing vector.
		/// </summary>
		public static Vector3 right = Vector3.right;
		#region Classes
		/// <summary>
		/// Representation of a rule on a set of rules to
		/// build the tree structure.
		/// </summary>
		[Serializable]
		public class Rule {
			/// <summary>
			/// Probability type.
			/// </summary>
			public enum ProbabilityType {
				Fill,
				Fixed
			};
			/// <summary>
			/// The symbol of the rule.
			/// </summary>
			public string symbol = "";
			/// <summary>
			/// The rule to replace the symbol.
			/// </summary>
			public string rule = "";
			/// <summary>
			/// If the rule is enabled.
			/// </summary>
			public bool enabled = true;
			/// <summary>
			/// The probability of occurrence.
			/// </summary>
			[Range(0,1)]
			public float probability = 1f;
			/// <summary>
			/// The type of the probability.
			/// </summary>
			public ProbabilityType probabilityType = ProbabilityType.Fill;
			/// <summary>
			/// Range of action for the rule (from).
			/// </summary>
			public int fromIteration = 0;
			/// <summary>
			/// Range of action for the rule (to), if -1 there is no limiting iteration.
			/// </summary>
			public int toIteration   = -1;
			/// <summary>
			/// Initializes a new instance of the <see cref="Broccoli.Utils.LSystem+Rule"/> class.
			/// </summary>
			public Rule () {}
			/// <summary>
			/// Initializes a new instance of the <see cref="Broccoli.Utils.LSystem+Rule"/> class.
			/// </summary>
			/// <param name="symbol">Symbol.</param>
			/// <param name="rule">Rule.</param>
			public Rule (string symbol, string rule) {
				this.symbol = symbol;
				this.rule = rule;
			}
			/// <summary>
			/// Clone this instance.
			/// </summary>
			public Rule Clone () {
				Rule clone = new Rule (symbol, rule);
				clone.enabled = enabled;
				clone.probability = probability;
				clone.probabilityType = probabilityType;
				clone.fromIteration = fromIteration;
				clone.toIteration = toIteration;
				return clone;
			}
		}
		/// <summary>
		/// State while traversing the system.
		/// </summary>
		public class State
		{
			/// <summary>
			/// Length.
			/// </summary>
			public float length;
			/// <summary>
			/// Turn angle.
			/// </summary>
			public float turnAngle;
			/// <summary>
			/// Pitch angle.
			/// </summary>
			public float pitchAngle;
			/// <summary>
			/// Roll angle.
			/// </summary>
			public float rollAngle;
			/// <summary>
			/// Direction.
			/// </summary>
			public Vector3 direction;
			/// <summary>
			/// Clone this instance.
			/// </summary>
			public State Clone () {
				State clone = new State ();
				clone.length = length;
				clone.turnAngle = turnAngle;
				clone.pitchAngle = pitchAngle;
				clone.rollAngle = rollAngle;
				clone.direction = new Vector3 (direction.x, direction.y, direction.z);
				return clone;
			}
			/// <summary>
			/// Resets the angles.
			/// </summary>
			public void ResetAngles () {
				turnAngle = 0f;
				rollAngle = 0f;
				pitchAngle = 0f;
			}
		}
		/// <summary>
		/// Ray created from the system.
		/// </summary>
		public class Ray
		{
			/// <summary>
			/// Direction.
			/// </summary>
			public Vector3 direction;
			/// <summary>
			/// Length.
			/// </summary>
			public float length;
			/// <summary>
			/// Children rays.
			/// </summary>
			public List<Ray> rays = new List<Ray> ();
			/// <summary>
			/// Initializes a new instance of the <see cref="Broccoli.Utils.LSystem+Ray"/> class.
			/// </summary>
			/// <param name="direction">Direction.</param>
			/// <param name="length">Length.</param>
			/// <param name="level">Level.</param>
			public Ray (Vector3 direction, float length, int level = 0) {
				this.direction = direction;
				this.length = length;
			}
			/// <summary>
			/// Clone this instance.
			/// </summary>
			public Ray Clone () {
				Ray clone = new Ray (this.direction, this.length);
				for (int i = 0; i < rays.Count; i++) {
					clone.rays.Add (rays[i].Clone ());
				}
				return clone;
			}
		}
		/// <summary>
		/// Container class to simplify axioms.
		/// </summary>
		public class SimpleAxiom
		{
			/// <summary>
			/// The axiom.
			/// </summary>
			public string parentAxiom = "";
			/// <summary>
			/// Children axioms.
			/// </summary>
			public Dictionary<string, SimpleAxiom> axioms = new Dictionary<string, SimpleAxiom> ();
		}
		#endregion

		#region Vars
		/// <summary>
		/// The axiom.
		/// </summary>
		public string axiom = "F";
		/// <summary>
		/// The input.
		/// </summary>
		public string input = "";
		/// <summary>
		/// The iterations.
		/// </summary>
		public int iterations = 3;
		/// <summary>
		/// Flag for accumulative mode.
		/// </summary>
		public bool accumulativeModeEnabled = false;
		/// <summary>
		/// Flag for remove overlaps.
		/// </summary>
		public bool removeOverlapsEnabled = true;
		/// <summary>
		/// Base length-
		/// </summary>
		public float length = 1f;
		/// <summary>
		/// The length growth.
		/// </summary>
		public float lengthGrowth = -1.5f;
		/// <summary>
		/// The turn angle.
		/// </summary>
		public float turnAngle = 30f;
		/// <summary>
		/// The turn angle growth.
		/// </summary>
		public float turnAngleGrowth = 0f;
		/// <summary>
		/// The pitch angle.
		/// </summary>
		public float pitchAngle = 30f;
		/// <summary>
		/// The pitch angle growth.
		/// </summary>
		public float pitchAngleGrowth = 0f;
		/// <summary>
		/// The roll angle.
		/// </summary>
		public float rollAngle = 30f;
		/// <summary>
		/// The roll angle growth.
		/// </summary>
		public float rollAngleGrowth = 0f;
		/// <summary>
		/// The global scale.
		/// </summary>
		public float globalScale = 1f;
		/// <summary>
		/// Set of rules to build a tree.
		/// </summary>
		public Dictionary<char, List<Rule>> rules = new Dictionary<char, List<Rule>> ();
		/// <summary>
		/// Current state of the system.
		/// </summary>
		private State state;
		/// <summary>
		/// Stack of states.
		/// </summary>
		private Stack<State> states = new Stack<State> ();
		/// <summary>
		/// Current ray.
		/// </summary>
		private Ray ray;
		/// <summary>
		/// The ray stack.
		/// </summary>
		private Stack<Ray> rayStack = new Stack<Ray> ();
		/// <summary>
		/// The ray branching.
		/// </summary>
		private List<Ray> rayBranching;
		/// <summary>
		/// Use the last generated input if not empty.
		/// </summary>
		public bool useLastGeneratedInput = false;
		/// <summary>
		/// The last generated input.
		/// </summary>
		private string lastGeneratedInput = "";
		/// <summary>
		/// The candidate rules when selecting the rule to apply.
		/// </summary>
		List<LSystem.Rule> candidateRules = new List<LSystem.Rule> ();
		#endregion

		#region Contructor
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Utils.LSystem"/> class.
		/// </summary>
		/// <param name="axiom">Axiom.</param>
		public LSystem (string axiom = "F") {
			this.axiom = axiom;
		}
		#endregion
		/// <summary>
		/// Iterates n times throught the root axiom replacing symbols with rules.
		/// </summary>
		public void Iterate () {
			if (!useLastGeneratedInput || string.IsNullOrEmpty (lastGeneratedInput)) {
				input = axiom;
				for (int i = 0; i < iterations; i++) {
					input = Replace (input, i);
				}
				if (removeOverlapsEnabled) {
					input = Simplify (input);
				}
				lastGeneratedInput = input;
			}
		}
		/// <summary>
		/// Generate the rays based on the iteration input.
		/// </summary>
		public List<Ray> Generate() {
			Iterate ();
			Ray parentRay = null;
			rayBranching = new List<Ray> ();
			state = new State() {
				direction = upward,
				length = length,
				turnAngle  = 0f,
				pitchAngle = 0f,
				rollAngle  = 0f
			};
			if (useLastGeneratedInput) {
				input = lastGeneratedInput;
			}
			for (int i = 0; i < input.Length; i++) {
				char c = input [i];
				switch (c) {
				case 'F':
					Vector3 newDirection = 
						Quaternion.Euler (state.pitchAngle, state.turnAngle, state.rollAngle) * state.direction;
					Ray newRay = new Ray (newDirection, state.length);
					if (parentRay == null) {
						rayBranching.Add (newRay);
					} else {
						parentRay.rays.Add (newRay);
					}
					parentRay = newRay;
					if (!accumulativeModeEnabled)
						state.ResetAngles ();
					break;
				case '+':
					state.turnAngle += turnAngle;
					break;
				case '-':
					state.turnAngle -= turnAngle;
					break;
				case '&':
					state.pitchAngle += pitchAngle;
					break;
				case '%':
					state.pitchAngle -= pitchAngle;
					break;
				case '\\':
					state.rollAngle += rollAngle;
					break;
				case '/':
					state.rollAngle -= rollAngle;
					break;
				case '>':
					state.length *= (1 - lengthGrowth);
					break;
				case '<':
					state.length *= (1 + lengthGrowth);
					break;
				case ')':
					state.turnAngle *= (1 + turnAngleGrowth);
					state.pitchAngle *= (1 + pitchAngleGrowth);
					state.rollAngle *= (1 + rollAngleGrowth);
					break;
				case '(':
					state.turnAngle *= (1 - turnAngleGrowth);
					state.pitchAngle *= (1 - pitchAngleGrowth);
					state.rollAngle *= (1 - rollAngleGrowth);
					break;
				case '[':
					states.Push (state.Clone ());
					/*
					state.rollAngle = 0;
					state.turnAngle = 0;
					state.pitchAngle = 0;
					*/
					rayStack.Push (parentRay);
					break;
				case ']':
					state = states.Pop ();
					parentRay = rayStack.Pop ();
					break;
				case '!':
					state.turnAngle *= -1;
					break;
				case '|':
					state.direction = (Quaternion.AngleAxis(180, upward)) * state.direction;
					break;
				}
			}
			return rayBranching;
		}
		/// <summary>
		/// Simplify the specified input to avoid repeated rays.
		/// </summary>
		/// <param name="input">Input.</param>
		public string Simplify (string input) {
			string accumAxiom = "";
			//input = "F[/F][/F[/F]][/F[/F][/F]][/FF]";
			//input = "F[/FF][/F]";
			//input = "[FF][\\FF][/FF]";
			Stack <Dictionary <string, SimpleAxiom>> stackAxioms = new Stack <Dictionary <string, SimpleAxiom>> ();
			Dictionary <string, SimpleAxiom> axiomsRoot = new Dictionary <string, SimpleAxiom> ();
			Dictionary <string, SimpleAxiom> parentAxioms;
			Dictionary <string, SimpleAxiom> currAxioms;
			bool parentSet = false;
			stackAxioms.Push (axiomsRoot);
			parentAxioms = axiomsRoot;
			currAxioms = axiomsRoot;
			for (int i = 0; i < input.Length; i++) {
				char c = input [i];
				switch (c) {
				case 'F':
					accumAxiom += c;
					if (!currAxioms.ContainsKey (accumAxiom)) {
						currAxioms.Add (accumAxiom, new SimpleAxiom{ parentAxiom = accumAxiom });
					}
					currAxioms = currAxioms [accumAxiom].axioms;
					if (!parentSet) {
						parentAxioms = currAxioms;
						parentSet = true;
					}
					accumAxiom = "";
					break;
				case '[':
					stackAxioms.Push (parentAxioms);
					accumAxiom = "";
					parentSet = false;
					break;
				case ']':
					currAxioms = stackAxioms.Pop ();
					parentAxioms = currAxioms;
					accumAxiom = "";
					break;
				default:
					accumAxiom += c;
					break;
				}
			}
			input = AxiomsToString (axiomsRoot);
			return input;
		}
		/// <summary>
		/// Axioms to string.
		/// </summary>
		/// <returns>The to string.</returns>
		/// <param name="axiomsRoot">Axioms root.</param>
		public string AxiomsToString (Dictionary <string, SimpleAxiom> axiomsRoot) {
			string result = "";
			var axiomsRootEnumerator = axiomsRoot.GetEnumerator ();
			while (axiomsRootEnumerator.MoveNext ()) {
				var axiomsRootPair = axiomsRootEnumerator.Current;
				result += "[" + axiomsRootPair.Value.parentAxiom;
				if (axiomsRootPair.Value.axioms.Count > 0) {
					result += AxiomsToString (axiomsRootPair.Value.axioms);
				}
				result += "]";
			}
			return result;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			axiom = "";
			rules.Clear ();
			rayStack.Clear ();
			rayBranching.Clear ();
		}
		/// <summary>
		/// Gets the rule.
		/// </summary>
		/// <returns>The rule.</returns>
		/// <param name="symbol">Symbol.</param>
		/// <param name="iteration">Iteration.</param>
		public Rule GetRule (char symbol, int iteration) {
			Rule ruleT = null;
			if (rules.ContainsKey (symbol)) {
				candidateRules.Clear ();
				int sharedPropCount = 0;
				float fixedProb = 0f;
				for (int i = 0; i < rules[symbol].Count; i++) {
					if (rules[symbol][i].enabled && iteration >= rules[symbol][i].fromIteration
					    && (rules[symbol][i].toIteration == -1 || iteration <= rules[symbol][i].toIteration)) {
						candidateRules.Add (rules[symbol][i]);
						if (rules[symbol][i].probabilityType == Rule.ProbabilityType.Fill) {
							sharedPropCount++;
						} else {
							fixedProb += rules[symbol][i].probability;
						}
					}
				}
				candidateRules.Sort (delegate (LSystem.Rule ruleA, LSystem.Rule ruleB) {
					if (ruleA.probabilityType == Rule.ProbabilityType.Fill) {
						return 1;
					} else {
						if (ruleA.probability < ruleB.probability)
							return 1;
						else
							return -1;
					}
				});
				float pAccum = 0;
				float p = UnityEngine.Random.Range (0f, 1f);
				for (int i = 0; i < candidateRules.Count; i++) {
					if (candidateRules[i].probabilityType == Rule.ProbabilityType.Fill)
						candidateRules[i].probability = (1 - fixedProb) / (float)sharedPropCount;
					if (p >= pAccum && p <= candidateRules[i].probability + pAccum) {
						ruleT = candidateRules[i];
						break;
					}
					pAccum += candidateRules[i].probability;
				}
				candidateRules.Clear ();
			}
			return ruleT;
		}
		/// <summary>
		/// Replace a input according to the rules that apply to the iteration.
		/// </summary>
		/// <param name="s">Input string</param>
		/// <param name="iteration">Iteration.</param>
		public string Replace (string s, int iteration)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < s.Length; i++) {
				Rule rule = GetRule (s[i], iteration);
				if (rule != null)
				{
					sb.Append(rule.rule);
				}
				else
				{
					sb.Append(s[i]);
				}
			}
			return sb.ToString();
		}
	}
}